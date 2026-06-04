// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

// Run test methods in parallel within the assembly.
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
