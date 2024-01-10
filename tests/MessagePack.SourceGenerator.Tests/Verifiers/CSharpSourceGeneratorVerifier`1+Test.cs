﻿// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Uncomment the following line to write expected files to disk
////#define WRITE_EXPECTED

#if WRITE_EXPECTED
#warning WRITE_EXPECTED is fine for local builds, but should not be merged to the main branch.
#endif

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using MessagePack;
using MessagePack.Analyzers;
using MessagePack.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using AnalyzerOptions = MessagePack.Analyzers.CodeAnalysis.AnalyzerOptions;

public static partial class CSharpSourceGeneratorVerifier
{
    public class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, XUnitVerifier>
    {
        private readonly string? testFile;
        private readonly string testMethod;
        private AnalyzerOptions options = AnalyzerOptions.Default;

        public Test([CallerFilePath] string? testFile = null, [CallerMemberName] string testMethod = null!)
        {
            this.CompilerDiagnostics = CompilerDiagnostics.Warnings;

            this.ReferenceAssemblies = ReferenceAssemblies.Net.Net60;
            this.TestState.AdditionalReferences.Add(typeof(MessagePackObjectAttribute).Assembly);
            this.TestState.AdditionalReferences.Add(typeof(MessagePackSerializer).Assembly);

            this.testFile = testFile;
            this.testMethod = testMethod;

#if WRITE_EXPECTED
            TestBehaviors |= TestBehaviors.SkipGeneratedSourcesCheck;
#endif
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Latest;

        public AnalyzerOptions Options
        {
            get => this.options;
            set
            {
                this.options = value;
                const string filename = "/.globalconfig";
                if (this.TestState.AnalyzerConfigFiles.FirstOrDefault(t => t.filename == filename) is { } tuple)
                {
                    this.TestState.AnalyzerConfigFiles.Remove(tuple);
                }

                this.TestState.AnalyzerConfigFiles.Add((filename, ConstructGlobalConfigString(value)));
                this.TestState.AdditionalFiles.Add(($"./{AnalyzerOptions.JsonOptionsFileName}", ConstructConfigJsonString(value)));
            }
        }

        public static async Task RunDefaultAsync(string testSource, AnalyzerOptions? options = null, [CallerFilePath] string? testFile = null, [CallerMemberName] string testMethod = null!)
        {
            await new Test(testFile, testMethod)
            {
                TestState =
                {
                    Sources = { testSource },
                },
                Options = options ?? AnalyzerOptions.Default,
            }.RunAsync();
        }

        public Test AddGeneratedSources()
        {
            static void AddGeneratedSources(ProjectState project, string testMethod, bool withPrefix)
            {
                string prefix = withPrefix ? $"{project.Name}." : string.Empty;
                string expectedPrefix = $"{ThisAssembly.AssemblyName}.Resources.{testMethod}.{prefix}"
                    .Replace(' ', '_')
                    .Replace(',', '_')
                    .Replace('(', '_')
                    .Replace(')', '_');

                foreach (var resourceName in typeof(Test).Assembly.GetManifestResourceNames())
                {
                    if (!resourceName.StartsWith(expectedPrefix))
                    {
                        continue;
                    }

                    using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                    if (resourceStream is null)
                    {
                        throw new InvalidOperationException();
                    }

                    using var reader = new StreamReader(resourceStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
                    var name = resourceName.Substring(expectedPrefix.Length);
                    project.GeneratedSources.Add((typeof(MessagePackGenerator), name, reader.ReadToEnd()));
                }
            }

            AddGeneratedSources(this.TestState, this.testMethod, this.TestState.AdditionalProjects.Count > 0);
            foreach (ProjectState addlProject in this.TestState.AdditionalProjects.Values)
            {
                AddGeneratedSources(addlProject, this.testMethod, true);
            }

            return this;
        }

        protected override Task RunImplAsync(CancellationToken cancellationToken)
        {
            this.AddGeneratedSources();

            foreach (ProjectState addlProject in this.TestState.AdditionalProjects.Values)
            {
                addlProject.AdditionalReferences.AddRange(this.TestState.AdditionalReferences);
                addlProject.DocumentationMode = DocumentationMode.Parse;
            }

            return base.RunImplAsync(cancellationToken);
        }

        protected override IEnumerable<Type> GetSourceGenerators()
        {
            yield return typeof(MessagePackGenerator);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
        {
            foreach (Type analyzer in typeof(MsgPack001SpecifyOptionsAnalyzer).Assembly.GetTypes().Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t)))
            {
                yield return (DiagnosticAnalyzer)Activator.CreateInstance(analyzer)!;
            }
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = (CSharpCompilationOptions)base.CreateCompilationOptions();
            return compilationOptions
                .WithAllowUnsafe(false)
                .WithWarningLevel(99)
                .WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItem("CS1591", ReportDiagnostic.Suppress));
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(this.LanguageVersion);
        }

        protected override async Task<(Compilation, ImmutableArray<Diagnostic>)> GetProjectCompilationAsync(Project project, IVerifier verifier, CancellationToken cancellationToken)
        {
            string fileNamePrefix = this.TestState.AdditionalProjects.Count > 0 ? $"{project.Name}." : string.Empty;
            var resourceDirectory = Path.Combine(Path.GetDirectoryName(this.testFile)!, "Resources", this.testMethod);

            var (compilation, diagnostics) = await base.GetProjectCompilationAsync(project, verifier, cancellationToken);
            var expectedNames = new HashSet<string>();
            foreach (var tree in compilation.SyntaxTrees.Skip(project.DocumentIds.Count))
            {
                WriteTreeToDiskIfNecessary(tree, resourceDirectory, fileNamePrefix);
                expectedNames.Add(Path.GetFileName(tree.FilePath));
            }

            var currentTestPrefix = $"{ThisAssembly.AssemblyName}.Resources.{this.testMethod}.{fileNamePrefix}";
            foreach (var name in this.GetType().Assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith(currentTestPrefix))
                {
                    continue;
                }

                if (!expectedNames.Contains(name.Substring(currentTestPrefix.Length)))
                {
                    throw new InvalidOperationException($"Unexpected test resource: {name.Substring(currentTestPrefix.Length)}");
                }
            }

            return (compilation, diagnostics);
        }

        [Conditional("WRITE_EXPECTED")]
        private static void WriteTreeToDiskIfNecessary(SyntaxTree tree, string resourceDirectory, string fileNamePrefix)
        {
            if (tree.Encoding is null)
            {
                throw new ArgumentException("Syntax tree encoding was not specified");
            }

            string name = fileNamePrefix + Path.GetFileName(tree.FilePath);
            string filePath = Path.Combine(resourceDirectory, name);
            Directory.CreateDirectory(resourceDirectory);
            File.WriteAllText(filePath, tree.GetText().ToString(), tree.Encoding);
        }

        private static string ConstructConfigJsonString(AnalyzerOptions options)
        {
            string json = JsonSerializer.Serialize(
                options,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
            return json;
        }

        private static string ConstructGlobalConfigString(AnalyzerOptions options)
        {
            StringBuilder globalConfigBuilder = new();
            globalConfigBuilder.AppendLine("is_global = true");
            globalConfigBuilder.AppendLine();

            return globalConfigBuilder.ToString();
        }
    }
}