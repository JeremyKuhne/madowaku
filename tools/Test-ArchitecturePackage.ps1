# Copyright (c) 2025 Jeremy W Kuhne
# SPDX-License-Identifier: MIT
# See LICENSE file in the project root for full license information

#requires -Version 7.0

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackagePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$repoRoot = Split-Path $PSScriptRoot -Parent
$package = (Resolve-Path -LiteralPath $PackagePath).Path
$work = Join-Path ([IO.Path]::GetTempPath()) "madowaku-package-$([Guid]::NewGuid().ToString('N'))"

function Get-AssemblyMachine
{
    param([string] $Path)

    $stream = [IO.File]::OpenRead($Path)
    $reader = [Reflection.PortableExecutable.PEReader]::new($stream)
    try
    {
        return $reader.PEHeaders.CoffHeader.Machine.ToString()
    }
    finally
    {
        $reader.Dispose()
        $stream.Dispose()
    }
}

Push-Location $repoRoot
try
{
    New-Item -ItemType Directory -Path $work | Out-Null
    $feed = Join-Path $work 'feed'
    New-Item -ItemType Directory -Path $feed | Out-Null
    Copy-Item -LiteralPath $package -Destination $feed
    $expanded = Join-Path $work 'expanded'
    [IO.Compression.ZipFile]::ExtractToDirectory($package, $expanded)
    [xml] $manifest = Get-Content -LiteralPath (Join-Path $expanded 'KlutzyNinja.Madowaku.nuspec') -Raw
    $version = [string]$manifest.package.metadata.version
    [xml] $frameworksFile = Get-Content -LiteralPath (Join-Path $expanded 'buildTransitive' 'KlutzyNinja.Madowaku.props') -Raw
    $frameworks = @(
        [string]$frameworksFile.Project.PropertyGroup._MadowakuNetFramework,
        [string]$frameworksFile.Project.PropertyGroup._MadowakuWindowsFramework,
        [string]$frameworksFile.Project.PropertyGroup._MadowakuDesktopFramework
    )

    foreach ($architecture in @('x64', 'arm64'))
    {
        $expectedMachine = if ($architecture -eq 'x64') { 'Amd64' } else { 'Arm64' }
        foreach ($framework in $frameworks)
        {
            $assembly = Join-Path $expanded 'runtimes' "win-$architecture" 'lib' $framework 'madowaku.dll'
            if ((Get-AssemblyMachine $assembly) -ne $expectedMachine)
            {
                throw "Incorrect architecture for packaged assembly '$assembly'."
            }
        }
    }

    if (@(Get-ChildItem -LiteralPath (Join-Path $expanded 'lib') -Filter '*.dll' -Recurse).Count -ne 0)
    {
        throw 'The package must not expose a default compiler assembly from lib/.'
    }

    foreach ($platform in @('x64', 'ARM64'))
    {
        $check = & dotnet msbuild madowaku.slnx -nologo -t:ValidateMadowakuPlatform "-p:Platform=$platform" 2>&1
        if ($LASTEXITCODE -ne 0)
        {
            throw "Solution platform validation failed: $platform.`n$($check -join [Environment]::NewLine)"
        }
    }

    foreach ($platform in @('AnyCPU', 'x86'))
    {
        $check = & dotnet msbuild madowaku/madowaku.csproj -nologo -t:ValidateMadowakuPlatform "-p:Platform=$platform" 2>&1
        if ($LASTEXITCODE -eq 0 -or ($check -join [Environment]::NewLine) -notmatch 'Madowaku supports only x64 and ARM64')
        {
            throw "Unsupported repository platform was not rejected: $platform.`n$($check -join [Environment]::NewLine)"
        }
    }

    foreach ($architecture in @('x64', 'arm64'))
    {
        $expectedMachine = if ($architecture -eq 'x64') { 'Amd64' } else { 'Arm64' }
        foreach ($framework in $frameworks)
        {
            $consumer = Join-Path $work "$architecture-$framework"
            New-Item -ItemType Directory -Path $consumer | Out-Null
            Copy-Item -LiteralPath (Join-Path $repoRoot 'global.json') -Destination $consumer
            Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ArchitecturePackageConsumer' 'Consumer.csproj.template') `
                -Destination (Join-Path $consumer 'Consumer.csproj')
            Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ArchitecturePackageConsumer' 'Program.cs.template') `
                -Destination (Join-Path $consumer 'Program.cs')
            $project = Join-Path $consumer 'Consumer.csproj'
            $properties = @(
                "-p:ConsumerTargetFramework=$framework",
                "-p:MadowakuPackageVersion=$version",
                "-p:PlatformTarget=$architecture"
            )

            & dotnet restore $project "-p:RestoreAdditionalProjectSources=$feed" `
                --packages (Join-Path $work 'packages') @properties
            if ($LASTEXITCODE -ne 0) { throw "Consumer restore failed: $architecture / $framework." }
            & dotnet build $project -c Release --no-restore @properties
            if ($LASTEXITCODE -ne 0) { throw "Consumer build failed: $architecture / $framework." }

            $output = Join-Path $consumer 'bin' 'Release' $framework
            if ((Get-AssemblyMachine (Join-Path $output 'madowaku.dll')) -ne $expectedMachine)
            {
                throw "Consumer copied the wrong runtime assembly: $architecture / $framework."
            }

            $transitive = Join-Path $work "transitive-$architecture-$framework"
            New-Item -ItemType Directory -Path $transitive | Out-Null
            Copy-Item -LiteralPath (Join-Path $repoRoot 'global.json') -Destination $transitive
            Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ArchitecturePackageConsumer' 'TransitiveConsumer.csproj.template') `
                -Destination (Join-Path $transitive 'TransitiveConsumer.csproj')
            Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ArchitecturePackageConsumer' 'Program.cs.template') `
                -Destination (Join-Path $transitive 'Program.cs')
            $transitiveProject = Join-Path $transitive 'TransitiveConsumer.csproj'
            $transitiveProperties = $properties + "-p:MadowakuConsumerProject=$project"
            & dotnet restore $transitiveProject "-p:RestoreAdditionalProjectSources=$feed" `
                --packages (Join-Path $work 'packages') @transitiveProperties
            if ($LASTEXITCODE -ne 0) { throw "Transitive consumer restore failed: $architecture / $framework." }
            & dotnet build $transitiveProject -c Release --no-restore @transitiveProperties
            if ($LASTEXITCODE -ne 0) { throw "Transitive consumer build failed: $architecture / $framework." }
            $transitiveOutput = Join-Path $transitive 'bin' 'Release' $framework
            if ((Get-AssemblyMachine (Join-Path $transitiveOutput 'madowaku.dll')) -ne $expectedMachine)
            {
                throw "Transitive consumer copied the wrong runtime assembly: $architecture / $framework."
            }

            if ($architecture -eq [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant())
            {
                if ($framework -like 'net4*')
                {
                    $result = & (Join-Path $output 'Consumer.exe')
                }
                else
                {
                    $result = & dotnet (Join-Path $output 'Consumer.dll')
                }

                if ($LASTEXITCODE -ne 0 -or $result -ne 'madowaku')
                {
                    throw "Native consumer execution failed: $architecture / $framework."
                }

                if ($framework -like 'net4*')
                {
                    $result = & (Join-Path $transitiveOutput 'TransitiveConsumer.exe')
                }
                else
                {
                    $result = & dotnet (Join-Path $transitiveOutput 'TransitiveConsumer.dll')
                }

                if ($LASTEXITCODE -ne 0 -or $result -ne 'madowaku')
                {
                    throw "Native transitive consumer execution failed: $architecture / $framework."
                }
            }

            if ($framework -eq $frameworks[0])
            {
                $rid = "win-$architecture"
                & dotnet restore $project --runtime $rid "-p:RestoreAdditionalProjectSources=$feed" `
                    --packages (Join-Path $work 'packages') @properties
                if ($LASTEXITCODE -ne 0) { throw "RID-specific consumer restore failed: $rid." }
                $publish = Join-Path $consumer 'published'
                & dotnet publish $project -c Release --no-restore --runtime $rid `
                    --self-contained false --output $publish @properties
                if ($LASTEXITCODE -ne 0) { throw "RID-specific consumer publish failed: $rid." }
                if ((Get-AssemblyMachine (Join-Path $publish 'madowaku.dll')) -ne $expectedMachine)
                {
                    throw "Published consumer contains the wrong assembly: $rid."
                }

                if ($architecture -eq [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant())
                {
                    $result = & (Join-Path $publish 'Consumer.exe')
                    if ($LASTEXITCODE -ne 0 -or $result -ne 'madowaku')
                    {
                        throw "Published native consumer execution failed: $rid."
                    }
                }
            }
        }
    }

    foreach ($unsupported in @('AnyCPU', 'x86'))
    {
        & dotnet restore $project "-p:RestoreAdditionalProjectSources=$feed" `
            --packages (Join-Path $work 'packages') `
            "-p:ConsumerTargetFramework=$framework" "-p:MadowakuPackageVersion=$version" `
            "-p:PlatformTarget=$unsupported"
        if ($LASTEXITCODE -ne 0) { throw "Unsupported-platform consumer could not be restored for validation: $unsupported." }

        $rejected = & dotnet build $project -c Release --no-restore `
            "-p:ConsumerTargetFramework=$framework" "-p:MadowakuPackageVersion=$version" `
            "-p:PlatformTarget=$unsupported" 2>&1
        if ($LASTEXITCODE -eq 0 -or ($rejected -join [Environment]::NewLine) -notmatch 'AnyCPU and x86 are not supported')
        {
            throw "$unsupported consumer was not rejected correctly:`n$($rejected -join [Environment]::NewLine)"
        }
    }

    Write-Host 'Architecture package checks passed for all frameworks and both 64-bit platforms.'
}
finally
{
    Pop-Location
    if (Test-Path -LiteralPath $work)
    {
        Remove-Item -LiteralPath $work -Recurse -Force
    }
}

exit 0
