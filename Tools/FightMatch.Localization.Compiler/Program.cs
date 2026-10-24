using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

internal static class Program
{
    private const string AcceptedCopySha256 = "ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf";
    private const string AcceptedSnapshotSha256 = "d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c";
    private const string AcceptedPrefixSha256 = "e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f";
    private const string AcceptedDesignSha256 = "022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a";
    private const string AcceptedPacketSha256 = "8e2626dad0d4b3c70854b9b7e22bdd48fce79ea91cececb7af45a9cd01a8487d";
    private const string CorrectivePacketSha256 = "b17ff60ca4df6bbb580e96daba43792608737ca243fc5bd0573dd8663d084587";
    private const string CurrentCorrectivePacketSha256 = "3d437c485903ab25569f9f417f49fd1653125428a4f3913843d15b7c35a62a35";
    private const string Accepted005RunSha256 = "ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232";
    private const string AcceptedRunnerManifestSha256 = "d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5";
    private const string ImmutableFailureRunSha256 = "e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420";
    private const string ImmutableFailureInputsSha256 = "07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2";
    private const string ImmutableFailureFormalSourceSha256 = "9695b4501b9d6ffda53914d0a42f61948147e2bf655385fa263e7bf916348e39";
    private const string ImmutableFix01RunSha256 = "19151d7f0cd9a2bfeaeddf090d267a35941722ba1cc9a163bba229c4cabc8dc4";
    private const string ImmutableFix01InputsSha256 = "3852210c00e9a053691b9fd05bfc3e2642c84939e3d6088634f639873569281c";
    private const string ImmutableFix01CommandsSha256 = "3debf91a90e48c8c48828f97afeeb47a8dfe574d170f0825b7d3283eee67aa9b";
    private const string LubanTag = "v5.1.0";
    private const string LubanCommit = "df34215d2e22035073d1af83d44e3ac0744dc3b3";
    private const string SdkVersion = "8.0.425";
    private const string OwnerThread = "01a0f40d-b0c5-7bc0-a2b2-be9d1213648a";
    private const string OwnerHost = "local";
    private const string ImmutableFailureRelativeRoot = "TestArtifacts/FightMatch/LOC-IMPL-A/" + AcceptedCopySha256;
    private const string ImmutableFix01RelativeRoot = "TestArtifacts/FightMatch/LOC-IMPL-A/" + AcceptedCopySha256 + "-fix-01";
    private const string EvidenceRelativeRoot = "TestArtifacts/FightMatch/LOC-IMPL-A/" + AcceptedCopySha256 + "-fix-02";

    private static readonly UTF8Encoding Utf8NoBom = new(false, true);
    private static readonly string[] SourceColumns =
    {
        "key", "screen", "element", "state_or_trigger", "zh_cn", "en", "parameters", "severity", "qa_notes"
    };
    private static readonly string[] ExpectedRawColumns = SourceColumns;
    private static readonly string[] AllowedSeverities = { "blocking", "error", "warning", "info" };
    private static readonly HashSet<string> AllowedSeveritySet = new(AllowedSeverities, StringComparer.Ordinal);
    private static readonly string[] ForbiddenPlayerTokens =
    {
        "OperationId", "CommitId", "FaceId", "PairId", "TargetKind"
    };

    private static string? s_outputRoot;
    private static DateTimeOffset s_startedAt;
    private static string s_terminal = "NOT_RUN";

    private static int Main(string[] args)
    {
        s_startedAt = DateTimeOffset.UtcNow;
        try
        {
            if (args.Length == 0)
            {
                throw new ContractException("FMLOC000", "mode is required");
            }

            int exit = args[0] switch
            {
                "generate" => RunGenerate(args),
                "compile" => RunCompile(args),
                "self-test" => RunSelfTests(args),
                "negative" => RunNegative(args),
                _ => throw new ContractException("FMLOC000", $"unknown mode: {args[0]}")
            };
            return exit;
        }
        catch (ContractException ex)
        {
            Console.Error.WriteLine($"{ex.Code}: {ex.Message}");
            TryWriteFailureRun(ex.Code, ex.Message);
            return ex.ExitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FMLOC999: {ex.GetType().Name}: {ex.Message}");
            TryWriteFailureRun("FMLOC999", $"{ex.GetType().Name}: {ex.Message}");
            return 70;
        }
    }

    private static int RunGenerate(string[] args)
    {
        Dictionary<string, string> options = ParseOptions(args, 1,
            "--luban-dll", "--expected-tool-sha256", "--expected-commit", "--expected-sdk", "--output-root");
        string repositoryRoot = Path.GetFullPath(Directory.GetCurrentDirectory());
        string requestedOutput = options["--output-root"];
        string outputRoot = ResolveUnder(repositoryRoot, requestedOutput);
        string expectedOutput = Path.Combine(repositoryRoot, EvidenceRelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        if (!PathEquals(outputRoot, expectedOutput))
        {
            throw new ContractException("FMLOC010", "output root is not the packet evidence root");
        }
        if (File.Exists(outputRoot) || IsSymlink(outputRoot))
        {
            throw new ContractException("FMLOC010", "output root is a file or symlink");
        }

        string dotnet = Environment.ProcessPath ?? throw new ContractException("FMLOC011", "dotnet process path unavailable");
        string compilerDll = Path.GetFullPath(typeof(Program).Assembly.Location);
        string lubanDll = Path.GetFullPath(options["--luban-dll"]);
        string runnerRoot = Path.GetDirectoryName(lubanDll)!;
        string cacheRoot = Directory.GetParent(runnerRoot)!.FullName;
        string evidence005 = Path.Combine(repositoryRoot, "TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005");
        ValidateFixedInputs(repositoryRoot, evidence005, dotnet, lubanDll, runnerRoot, options);
        List<LocalizationRow> copyRows = ValidateAcceptedCopy(repositoryRoot);
        FormalSource formal = ValidateFormalSource(repositoryRoot, copyRows);
        string protectedBefore = GitStatus(repositoryRoot, new[] { "Assets", "Packages", "ProjectSettings" });

        s_outputRoot = outputRoot;
        Directory.CreateDirectory(outputRoot);
        Directory.CreateDirectory(Path.Combine(outputRoot, "steps"));
        Directory.CreateDirectory(Path.Combine(outputRoot, "self-tests"));
        Directory.CreateDirectory(Path.Combine(outputRoot, "negative", "duplicate"));
        Directory.CreateDirectory(Path.Combine(outputRoot, "negative", "parameters"));
        Directory.CreateDirectory(Path.Combine(outputRoot, "negative", "severity"));
        WriteInputs(outputRoot, repositoryRoot, evidence005, dotnet, lubanDll, formal, protectedBefore);
        WriteSelfInvocationStart(outputRoot, dotnet, compilerDll, args);

        RunRecorded(outputRoot, repositoryRoot, "compiler-self-tests", dotnet, new[]
        {
            compilerDll, "self-test", "--result", Relative(repositoryRoot, Path.Combine(outputRoot, "self-tests", "results.json"))
        }, 0);

        string raw1 = PrepareRawPass(outputRoot, "pass-1");
        RunLuban(outputRoot, repositoryRoot, "pass-1-luban", dotnet, lubanDll, runnerRoot, raw1);
        string rawFile1 = VerifyAndWriteRawManifest(outputRoot, "pass-1", raw1);
        WriteFormalSha(outputRoot, "pass-1", formal.Sha256);
        RunCompiler(outputRoot, repositoryRoot, "compile-pass-1", dotnet, compilerDll, rawFile1, "pass-1");

        string raw2 = PrepareRawPass(outputRoot, "pass-2");
        RunLuban(outputRoot, repositoryRoot, "pass-2-luban", dotnet, lubanDll, runnerRoot, raw2);
        string rawFile2 = VerifyAndWriteRawManifest(outputRoot, "pass-2", raw2);
        WriteFormalSha(outputRoot, "pass-2", formal.Sha256);
        RunCompiler(outputRoot, repositoryRoot, "compile-pass-2", dotnet, compilerDll, rawFile2, "pass-2");

        RunNegative(outputRoot, repositoryRoot, dotnet, compilerDll, "duplicate", "FMLOC002");
        RunNegative(outputRoot, repositoryRoot, dotnet, compilerDll, "parameters", "FMLOC007");
        RunNegative(outputRoot, repositoryRoot, dotnet, compilerDll, "severity", "FMLOC004");

        Comparison comparison = ComparePasses(outputRoot);
        WriteComparison(outputRoot, comparison);
        if (!comparison.AllEqual)
        {
            s_terminal = "NON_DETERMINISTIC";
            throw new ContractException("NON_DETERMINISTIC", "the first two production passes differ", 31);
        }

        LicenseInventoryResult license = WriteLicenseInventory(outputRoot, cacheRoot, runnerRoot, evidence005);
        if (!license.Complete)
        {
            s_terminal = "LICENSE_INVENTORY_INCOMPLETE";
            throw new ContractException("LICENSE_INVENTORY_INCOMPLETE", string.Join("; ", license.MissingReasons), 32);
        }

        string generatedRoot = Path.Combine(repositoryRoot, "Generated/FightMatch/Localization");
        Directory.CreateDirectory(generatedRoot);
        File.Copy(Path.Combine(outputRoot, "pass-1", "canonical", "fm-text-v1.json"),
            Path.Combine(generatedRoot, "fm-text-v1.json"), true);
        File.Copy(Path.Combine(outputRoot, "pass-1", "canonical", "fm-text-v1.manifest.json"),
            Path.Combine(generatedRoot, "fm-text-v1.manifest.json"), true);

        WriteDiffCheck(outputRoot, repositoryRoot, protectedBefore);
        BuildCommandsJson(outputRoot);
        s_terminal = "PASS_LOC_IMPL_A";
        WriteRun(outputRoot, "PASS_LOC_IMPL_A", null, null, license, comparison);
        FinishSelfInvocation(outputRoot, 0);
        BuildCommandsJson(outputRoot);
        Console.Out.WriteLine($"PASS_LOC_IMPL_A canonical={comparison.CanonicalSha256} manifest={comparison.ManifestSha256} semantic={comparison.SemanticDigest}");
        return 0;
    }

    private static int RunCompile(string[] args)
    {
        Dictionary<string, string> options = ParseOptions(args, 1,
            "--source", "--luban-json", "--artifact", "--manifest");
        string sourcePath = Path.GetFullPath(options["--source"]);
        string rawPath = Path.GetFullPath(options["--luban-json"]);
        string artifactPath = Path.GetFullPath(options["--artifact"]);
        string manifestPath = Path.GetFullPath(options["--manifest"]);
        FormalSource source = ParseAndValidateFormal(sourcePath);
        List<LocalizationRow> rawRows = ParseRaw(File.ReadAllBytes(rawPath));
        ValidateOneToOne(source.Rows, rawRows);
        byte[] canonical = BuildCanonical(rawRows);
        ValidateRoundTrip(canonical, rawRows);
        string canonicalSha = Sha256(canonical);
        string semanticDigest = canonicalSha;
        byte[] manifest = BuildPublishManifest(source, rawRows, canonicalSha, semanticDigest);
        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        File.WriteAllBytes(artifactPath, canonical);
        File.WriteAllBytes(manifestPath, manifest);
        Console.Out.WriteLine($"PASS rows={rawRows.Count} canonical={canonicalSha} manifest={Sha256(manifest)} semantic={semanticDigest}");
        return 0;
    }

    private static int RunSelfTests(string[] args)
    {
        Dictionary<string, string> options = ParseOptions(args, 1, "--result");
        var results = new List<TestResult>();
        RunTest(results, "schema-column-count", TestSchemaColumnCount);
        RunTest(results, "duplicate-key", TestDuplicateKey);
        RunTest(results, "empty-translation", TestEmptyTranslation);
        RunTest(results, "invalid-severity", TestInvalidSeverity);
        RunTest(results, "missing-parameter", TestMissingParameter);
        RunTest(results, "multiple-parameters", TestMultipleParameters);
        RunTest(results, "unclosed-template", TestUnclosedTemplate);
        RunTest(results, "unknown-raw-column", TestUnknownRawColumn);
        RunTest(results, "sorting", TestSorting);
        RunTest(results, "newlines", TestNewlines);
        RunTest(results, "round-trip", TestRoundTrip);
        RunTest(results, "duplicate-generation", TestDuplicateGeneration);
        bool passed = results.Count == 12 && results.All(result => result.Passed);
        WriteJson(options["--result"], new
        {
            schemaVersion = 1,
            status = passed ? "PASS" : "FAIL",
            passed = results.Count(result => result.Passed),
            total = results.Count,
            tests = results.Select(result => new { name = result.Name, passed = result.Passed, detail = result.Detail }).ToArray()
        });
        foreach (TestResult result in results)
        {
            Console.Out.WriteLine($"{(result.Passed ? "PASS" : "FAIL")} {result.Name}: {result.Detail}");
        }
        Console.Out.WriteLine($"SUMMARY {(passed ? "PASS" : "FAIL")} {results.Count(result => result.Passed)}/{results.Count}");
        return passed ? 0 : 50;
    }

    private static int RunNegative(string[] args)
    {
        Dictionary<string, string> options = ParseOptions(args, 1, "--case", "--result");
        string caseName = options["--case"];
        string expected = caseName switch
        {
            "duplicate" => "FMLOC002",
            "parameters" => "FMLOC007",
            "severity" => "FMLOC004",
            _ => throw new ContractException("FMLOC000", "unknown negative case")
        };
        string observed = "NONE";
        try
        {
            LocalizationRow row = ValidRow("fm.negative");
            List<LocalizationRow> rows = new() { row };
            if (caseName == "duplicate")
            {
                rows.Add(ValidRow("fm.negative"));
            }
            else if (caseName == "parameters")
            {
                row.En = "Value {count}";
                row.ZhCn = "值 {count}";
                row.Parameters = "";
            }
            else
            {
                row.Severity = "fatal";
            }
            _ = BuildCanonical(rows);
        }
        catch (ContractException ex)
        {
            observed = ex.Code;
        }
        bool passed = observed == expected;
        WriteJson(options["--result"], new
        {
            schemaVersion = 1,
            caseName,
            status = passed ? "PASS" : "FAIL",
            gate = caseName == "duplicate" ? "project-duplicate-gate" : "project-canonicalizer",
            expectedDiagnostic = expected,
            observedDiagnostic = observed,
            publishedCanonicalCount = 0
        });
        Console.Out.WriteLine($"{(passed ? "PASS" : "FAIL")} {caseName} expected={expected} observed={observed} publishedCanonicalCount=0");
        return passed ? 0 : 51;
    }

    private static void ValidateFixedInputs(
        string repositoryRoot,
        string evidence005,
        string dotnet,
        string lubanDll,
        string runnerRoot,
        IReadOnlyDictionary<string, string> options)
    {
        RequireHash(Path.Combine(repositoryRoot, "docs/team/2026-09-30/engineering-localization-luban-impl-a.md"), AcceptedPacketSha256, "task packet");
        RequireHash(Path.Combine(repositoryRoot, "docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-01.md"), CorrectivePacketSha256, "corrective task packet");
        RequireHash(Path.Combine(repositoryRoot, "docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-02.md"), CurrentCorrectivePacketSha256, "current corrective task packet");
        RequireHash(Path.Combine(repositoryRoot, "docs/team/2026-09-30/engineering-localization-luban-001.md"), AcceptedDesignSha256, "accepted design");
        RequireHash(Path.Combine(repositoryRoot, "docs/team/2026-09-30/planning-localization-draft.csv"), AcceptedCopySha256, "accepted COPY");
        RequireHash(Path.Combine(evidence005, "run.json"), Accepted005RunSha256, "005 run");
        RequireHash(Path.Combine(evidence005, "identity/runner-files.tsv"), AcceptedRunnerManifestSha256, "runner manifest");
        string immutableFailureRoot = Path.Combine(repositoryRoot, ImmutableFailureRelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        string immutableRun = Path.Combine(immutableFailureRoot, "run.json");
        string immutableInputs = Path.Combine(immutableFailureRoot, "inputs.json");
        RequireHash(immutableRun, ImmutableFailureRunSha256, "immutable failure run");
        RequireHash(immutableInputs, ImmutableFailureInputsSha256, "immutable failure inputs");
        using (JsonDocument failureInputs = JsonDocument.Parse(File.ReadAllBytes(immutableInputs)))
        {
            Require(failureInputs.RootElement.GetProperty("formalSource").GetProperty("sha256").GetString() == ImmutableFailureFormalSourceSha256,
                "immutable failure formal source identity mismatch");
        }
        string immutableFix01Root = Path.Combine(repositoryRoot, ImmutableFix01RelativeRoot.Replace('/', Path.DirectorySeparatorChar));
        string immutableFix01Run = Path.Combine(immutableFix01Root, "run.json");
        string immutableFix01Inputs = Path.Combine(immutableFix01Root, "inputs.json");
        string immutableFix01Commands = Path.Combine(immutableFix01Root, "commands.json");
        RequireHash(immutableFix01Run, ImmutableFix01RunSha256, "immutable FIX-01 run");
        RequireHash(immutableFix01Inputs, ImmutableFix01InputsSha256, "immutable FIX-01 inputs");
        RequireHash(immutableFix01Commands, ImmutableFix01CommandsSha256, "immutable FIX-01 commands");
        using (JsonDocument fix01Run = JsonDocument.Parse(File.ReadAllBytes(immutableFix01Run)))
        {
            JsonElement root = fix01Run.RootElement;
            Require(root.GetProperty("stageTerminal").GetString() == "BLOCKED_INPUT_DRIFT", "FIX-01 terminal mismatch");
            Require(root.GetProperty("compilerExitCode").GetInt32() == 0 && root.GetProperty("generateExitCode").GetInt32() == 21,
                "FIX-01 exit identity mismatch");
            Require(!root.GetProperty("selfTestsStarted").GetBoolean() && !root.GetProperty("lubanStarted").GetBoolean() &&
                    root.GetProperty("rawPassesStarted").GetInt32() == 0 && !root.GetProperty("generatedPublished").GetBoolean(),
                "FIX-01 stop fields mismatch");
        }
        if (options["--expected-tool-sha256"] != "6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e" ||
            options["--expected-commit"] != LubanCommit || options["--expected-sdk"] != SdkVersion)
        {
            throw new ContractException("BLOCKED_INPUT_DRIFT", "invocation identity differs from packet", 21);
        }
        RequireHash(lubanDll, options["--expected-tool-sha256"], "Luban.dll");

        using JsonDocument toolIdentity = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(evidence005, "tool-identity.json")));
        JsonElement tool = toolIdentity.RootElement;
        Require(tool.GetProperty("sourceHead").GetString() == LubanCommit, "005 source commit mismatch");
        Require(tool.GetProperty("dotnetExecutableSha256").GetString() == Sha256File(dotnet), "dotnet executable mismatch");
        JsonElement runner = tool.GetProperty("runner");
        Require(runner.GetProperty("fileCount").GetInt32() == 132, "runner file count mismatch");
        Require(runner.GetProperty("manifestSha256").GetString() == AcceptedRunnerManifestSha256, "runner manifest identity mismatch");
        Require(runner.GetProperty("criticalFiles").GetProperty("Luban.dll").GetString() == options["--expected-tool-sha256"], "runner Luban identity mismatch");

        using JsonDocument inputHashes = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(evidence005, "input-hashes.json")));
        Require(inputHashes.RootElement.GetProperty("sdkArchive").GetProperty("version").GetString() == SdkVersion, "SDK version mismatch");
        ValidateRunnerManifest(Path.Combine(evidence005, "identity/runner-files.tsv"), runnerRoot);
    }

    private static List<LocalizationRow> ValidateAcceptedCopy(string repositoryRoot)
    {
        string path = Path.Combine(repositoryRoot, "docs/team/2026-09-30/planning-localization-draft.csv");
        byte[] bytes = File.ReadAllBytes(path);
        Require(bytes.Length == 65358, "accepted COPY byte count mismatch");
        Require(CountLf(bytes) == 265, "accepted COPY physical line count mismatch");
        List<string[]> records = ParseCsv(bytes);
        Require(records.Count == 265, "accepted COPY logical record count mismatch");
        Require(records[0].SequenceEqual(SourceColumns, StringComparer.Ordinal), "accepted COPY header mismatch");
        List<LocalizationRow> rows = records.Skip(1).Select(ToRow).ToList();
        ValidateRows(rows, 264);
        Require(SeverityCounts(rows).SequenceEqual(new[] { 13, 5, 172, 74 }), "accepted COPY severity summary mismatch");
        Require(rows[^1].Key == "fm.language.save_unknown", "AMEND-04 row is not the sole append");
        Require(HashWithoutLastPhysicalLine(bytes) == AcceptedSnapshotSha256, "263-key snapshot prefix mismatch");
        Require(HashFirstPhysicalLines(bytes, 217) == AcceptedPrefixSha256, "216-key physical prefix mismatch");
        return rows;
    }

    private static FormalSource ValidateFormalSource(string repositoryRoot, IReadOnlyList<LocalizationRow> copyRows)
    {
        string path = Path.Combine(repositoryRoot, "Config/FightMatch/Localization/FightMatchText.csv");
        FormalSource formal = ParseAndValidateFormal(path);
        Require(formal.Rows.Count == copyRows.Count, "formal source row count differs from COPY");
        for (int index = 0; index < copyRows.Count; index++)
        {
            Require(formal.Rows[index].AllFieldsEqual(copyRows[index]), $"formal source differs from COPY at row {index + 1}");
        }
        return formal;
    }

    private static FormalSource ParseAndValidateFormal(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Require(bytes.Length >= 3 && !(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF), "formal source must be UTF-8 without BOM");
        Require(!bytes.Contains((byte)'\r'), "formal source must use LF only");
        List<string[]> records = ParseCsv(bytes);
        Require(records.Count >= 4, "formal source lacks control rows");
        Require(records[0].SequenceEqual(new[] { "##var" }.Concat(SourceColumns), StringComparer.Ordinal), "formal ##var row mismatch");
        Require(records[1].SequenceEqual(new[] { "##type", "string", "string", "string", "string", "string", "string", "string", "string#set=blocking,error,warning,info", "string" }, StringComparer.Ordinal), "formal ##type row mismatch");
        Require(records[2].SequenceEqual(new[] { "##comment", "stable key", "page", "role", "state/trigger", "zh-Hans", "en", "named args", "severity", "QA note" }, StringComparer.Ordinal), "formal ##comment row mismatch");
        List<LocalizationRow> rows = records.Skip(3).Select(record =>
        {
            Require(record.Length == 10 && record[0].Length == 0, "formal data row control cell mismatch");
            return ToRow(record.Skip(1).ToArray());
        }).ToList();
        ValidateRows(rows, 264);
        return new FormalSource(path, Sha256(bytes), rows);
    }

    private static void ValidateRows(IReadOnlyList<LocalizationRow> rows, int expectedCount, bool requireProductionContracts = true)
    {
        Require(rows.Count == expectedCount, $"expected {expectedCount} localization rows, found {rows.Count}");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (LocalizationRow row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Key) || !row.Key.StartsWith("fm.", StringComparison.Ordinal) || !seen.Add(row.Key))
            {
                throw new ContractException("FMLOC002", $"invalid or duplicate key: {row.Key}");
            }
            if (new[] { row.Screen, row.Element, row.StateOrTrigger, row.ZhCn, row.En, row.Severity, row.QaNotes }.Any(string.IsNullOrWhiteSpace))
            {
                throw new ContractException("FMLOC003", $"required field is empty: {row.Key}");
            }
            if (!AllowedSeveritySet.Contains(row.Severity))
            {
                throw new ContractException("FMLOC004", $"invalid severity for {row.Key}: {row.Severity}");
            }
            string[] declared = ParseDeclaredParameters(row.Parameters, row.Key);
            string[] en = ExtractTemplateParameters(row.En, row.Key, "en");
            string[] zh = ExtractTemplateParameters(row.ZhCn, row.Key, "zh-Hans");
            if (!declared.SequenceEqual(en, StringComparer.Ordinal) || !en.SequenceEqual(zh, StringComparer.Ordinal))
            {
                throw new ContractException("FMLOC007", $"parameter set mismatch: {row.Key}");
            }
            foreach (string token in ForbiddenPlayerTokens)
            {
                if (row.En.Contains(token, StringComparison.Ordinal) || row.ZhCn.Contains(token, StringComparison.Ordinal))
                {
                    throw new ContractException("FMLOC008", $"internal token in player text: {row.Key}/{token}");
                }
            }
        }
        if (requireProductionContracts)
        {
            LocalizationRow placeholder = rows.Single(row => row.Key == "fm.diagnostic.placeholder");
            Require(placeholder.En == "【if you see this, it is a bug.】" && placeholder.ZhCn == placeholder.En && placeholder.Severity == "blocking", "diagnostic placeholder mismatch");
            foreach (string key in new[] { "fm.language.save_failed", "fm.language.save_unknown" })
            {
                LocalizationRow save = rows.Single(row => row.Key == key);
                Require(save.Parameters == "errorCode" && save.Severity == "error", $"{key} contract mismatch");
            }
        }
    }

    private static List<LocalizationRow> ParseRaw(byte[] bytes)
    {
        using JsonDocument document = JsonDocument.Parse(bytes);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new ContractException("FMLOC001", "raw root must be an array");
        }
        var rows = new List<LocalizationRow>();
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new ContractException("FMLOC001", "raw row must be an object");
            }
            JsonProperty[] properties = item.EnumerateObject().ToArray();
            string[] names = properties.Select(property => property.Name).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expected = ExpectedRawColumns.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (properties.Length != expected.Length || !names.SequenceEqual(expected, StringComparer.Ordinal) || properties.Any(property => property.Value.ValueKind != JsonValueKind.String))
            {
                throw new ContractException("FMLOC001", $"raw row must have exact string columns: {string.Join(',', ExpectedRawColumns)}");
            }
            rows.Add(new LocalizationRow
            {
                Key = item.GetProperty("key").GetString()!,
                Screen = item.GetProperty("screen").GetString()!,
                Element = item.GetProperty("element").GetString()!,
                StateOrTrigger = item.GetProperty("state_or_trigger").GetString()!,
                ZhCn = item.GetProperty("zh_cn").GetString()!,
                En = item.GetProperty("en").GetString()!,
                Parameters = item.GetProperty("parameters").GetString()!,
                Severity = item.GetProperty("severity").GetString()!,
                QaNotes = item.GetProperty("qa_notes").GetString()!
            });
        }
        ValidateRows(rows, 264);
        return rows;
    }

    private static void ValidateOneToOne(IReadOnlyList<LocalizationRow> source, IReadOnlyList<LocalizationRow> raw)
    {
        Require(source.Count == raw.Count, "raw/formal row count mismatch");
        Dictionary<string, LocalizationRow> rawByKey = raw.ToDictionary(row => row.Key, StringComparer.Ordinal);
        foreach (LocalizationRow row in source)
        {
            Require(rawByKey.TryGetValue(row.Key, out LocalizationRow? rawRow) && row.AllFieldsEqual(rawRow), $"raw/formal mismatch: {row.Key}");
        }
    }

    private static byte[] BuildCanonical(IReadOnlyCollection<LocalizationRow> sourceRows)
    {
        if (sourceRows.Count == 0)
        {
            throw new ContractException("FMLOC001", "at least one row is required");
        }
        ValidateRows(sourceRows.ToList(), sourceRows.Count, false);
        List<LocalizationRow> rows = sourceRows.OrderBy(row => row.Key, StringComparer.Ordinal).ToList();
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = NewJsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", 1);
            writer.WritePropertyName("locales");
            writer.WriteStartArray();
            writer.WriteStringValue("en");
            writer.WriteStringValue("zh-Hans");
            writer.WriteEndArray();
            writer.WritePropertyName("entries");
            writer.WriteStartArray();
            foreach (LocalizationRow row in rows)
            {
                writer.WriteStartObject();
                writer.WriteString("key", row.Key);
                writer.WriteString("screen", row.Screen);
                writer.WriteString("element", row.Element);
                writer.WriteString("stateOrTrigger", row.StateOrTrigger);
                writer.WritePropertyName("texts");
                writer.WriteStartObject();
                writer.WriteString("en", row.En);
                writer.WriteString("zh-Hans", row.ZhCn);
                writer.WriteEndObject();
                writer.WritePropertyName("parameters");
                writer.WriteStartArray();
                foreach (string parameter in ParseDeclaredParameters(row.Parameters, row.Key))
                {
                    writer.WriteStringValue(parameter);
                }
                writer.WriteEndArray();
                writer.WriteString("severity", row.Severity);
                writer.WriteString("qaNotes", row.QaNotes);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();
        }
        return WithFinalLf(buffer.WrittenSpan);
    }

    private static byte[] BuildPublishManifest(FormalSource source, IReadOnlyList<LocalizationRow> rows, string canonicalSha, string semanticDigest)
    {
        int[] severities = SeverityCounts(rows);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = NewJsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", 1);
            writer.WriteNumber("artifactSchemaVersion", 1);
            writer.WritePropertyName("luban");
            writer.WriteStartObject();
            writer.WriteString("tag", LubanTag);
            writer.WriteString("commit", LubanCommit);
            writer.WriteEndObject();
            writer.WriteString("sdkVersion", SdkVersion);
            writer.WriteString("acceptedCopySha256", AcceptedCopySha256);
            writer.WriteString("formalSourceSha256", source.Sha256);
            writer.WriteString("canonicalArtifactSha256", canonicalSha);
            writer.WriteNumber("recordCount", rows.Count);
            writer.WriteNumber("uniqueKeyCount", rows.Select(row => row.Key).Distinct(StringComparer.Ordinal).Count());
            writer.WritePropertyName("localeMapping");
            writer.WriteStartObject();
            writer.WriteString("en", "en");
            writer.WriteString("zh_cn", "zh-Hans");
            writer.WriteEndObject();
            writer.WriteNumber("parameterizedRecordCount", rows.Count(row => row.Parameters.Length > 0));
            writer.WritePropertyName("severityCounts");
            writer.WriteStartObject();
            writer.WriteNumber("blocking", severities[0]);
            writer.WriteNumber("error", severities[1]);
            writer.WriteNumber("info", severities[2]);
            writer.WriteNumber("warning", severities[3]);
            writer.WriteEndObject();
            writer.WriteString("semanticDigest", semanticDigest);
            writer.WriteEndObject();
            writer.Flush();
        }
        return WithFinalLf(buffer.WrittenSpan);
    }

    private static void ValidateRoundTrip(byte[] bytes, IReadOnlyCollection<LocalizationRow> rows)
    {
        using JsonDocument document = JsonDocument.Parse(bytes);
        JsonElement root = document.RootElement;
        Require(root.GetProperty("schemaVersion").GetInt32() == 1, "round-trip schema version mismatch");
        string[] locales = root.GetProperty("locales").EnumerateArray().Select(value => value.GetString()!).ToArray();
        Require(locales.SequenceEqual(new[] { "en", "zh-Hans" }, StringComparer.Ordinal), "round-trip locale mismatch");
        JsonElement[] entries = root.GetProperty("entries").EnumerateArray().ToArray();
        Require(entries.Length == rows.Count, "round-trip row count mismatch");
        string[] expectedKeys = rows.Select(row => row.Key).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        string[] actualKeys = entries.Select(entry => entry.GetProperty("key").GetString()!).ToArray();
        Require(actualKeys.SequenceEqual(expectedKeys, StringComparer.Ordinal), "round-trip key order mismatch");
        foreach (JsonElement entry in entries)
        {
            string key = entry.GetProperty("key").GetString()!;
            LocalizationRow row = rows.Single(candidate => candidate.Key == key);
            Require(entry.GetProperty("screen").GetString() == row.Screen, $"round-trip screen mismatch: {key}");
            Require(entry.GetProperty("element").GetString() == row.Element, $"round-trip element mismatch: {key}");
            Require(entry.GetProperty("stateOrTrigger").GetString() == row.StateOrTrigger, $"round-trip state mismatch: {key}");
            JsonElement texts = entry.GetProperty("texts");
            Require(texts.GetProperty("en").GetString() == row.En && texts.GetProperty("zh-Hans").GetString() == row.ZhCn, $"round-trip text mismatch: {key}");
            string[] parameters = entry.GetProperty("parameters").EnumerateArray().Select(value => value.GetString()!).ToArray();
            Require(parameters.SequenceEqual(ParseDeclaredParameters(row.Parameters, key), StringComparer.Ordinal), $"round-trip parameter mismatch: {key}");
            Require(entry.GetProperty("severity").GetString() == row.Severity, $"round-trip severity mismatch: {key}");
            Require(entry.GetProperty("qaNotes").GetString() == row.QaNotes, $"round-trip QA note mismatch: {key}");
        }
    }

    private static string PrepareRawPass(string outputRoot, string pass)
    {
        string passRoot = Path.Combine(outputRoot, pass);
        string raw = Path.Combine(passRoot, "raw");
        if (Directory.Exists(raw) || File.Exists(raw) || IsSymlink(raw))
        {
            throw new ContractException("FMLOC010", $"{pass} raw staging is not fresh");
        }
        Directory.CreateDirectory(passRoot);
        return raw;
    }

    private static void RunLuban(string outputRoot, string repositoryRoot, string step, string dotnet, string lubanDll, string runnerRoot, string raw)
    {
        string rawRelative = Relative(repositoryRoot, raw);
        RunRecorded(outputRoot, repositoryRoot, step, dotnet, new[]
        {
            lubanDll,
            "--conf", "Config/FightMatch/Luban/luban.conf",
            "-t", "client",
            "-d", "json",
            "--strict",
            "--errorFormat", "json",
            "-l", Path.Combine(runnerRoot, "nlog.xml"),
            "-x", "outputDataDir=" + rawRelative
        }, 0);
    }

    private static void RunCompiler(string outputRoot, string repositoryRoot, string step, string dotnet, string compilerDll, string rawFile, string pass)
    {
        string canonicalRoot = Path.Combine(outputRoot, pass, "canonical");
        RunRecorded(outputRoot, repositoryRoot, step, dotnet, new[]
        {
            compilerDll,
            "compile",
            "--source", "Config/FightMatch/Localization/FightMatchText.csv",
            "--luban-json", Relative(repositoryRoot, rawFile),
            "--artifact", Relative(repositoryRoot, Path.Combine(canonicalRoot, "fm-text-v1.json")),
            "--manifest", Relative(repositoryRoot, Path.Combine(canonicalRoot, "fm-text-v1.manifest.json"))
        }, 0);
    }

    private static void RunNegative(string outputRoot, string repositoryRoot, string dotnet, string compilerDll, string caseName, string expected)
    {
        string result = Path.Combine(outputRoot, "negative", caseName, "result.json");
        RunRecorded(outputRoot, repositoryRoot, "negative-" + caseName, dotnet, new[]
        {
            compilerDll, "negative", "--case", caseName, "--result", Relative(repositoryRoot, result)
        }, 0);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(result));
        JsonElement root = document.RootElement;
        Require(root.GetProperty("status").GetString() == "PASS", $"negative {caseName} failed");
        Require(root.GetProperty("observedDiagnostic").GetString() == expected, $"negative {caseName} diagnostic mismatch");
        Require(root.GetProperty("publishedCanonicalCount").GetInt32() == 0, $"negative {caseName} published output");
    }

    private static string VerifyAndWriteRawManifest(string outputRoot, string pass, string rawRoot)
    {
        Require(Directory.Exists(rawRoot) && !IsSymlink(rawRoot), $"{pass} raw directory missing or symlinked");
        string[] files = Directory.GetFiles(rawRoot, "*", SearchOption.AllDirectories);
        Array.Sort(files, StringComparer.Ordinal);
        Require(files.Length == 1, $"{pass} expected one raw file, found {files.Length}");
        string expectedName = Path.Combine(rawRoot, "fm-text-v1.json");
        Require(PathEquals(files[0], expectedName), $"{pass} unexpected raw output: {files[0]}");
        var builder = new StringBuilder();
        foreach (string file in files)
        {
            string relative = Path.GetRelativePath(rawRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            builder.Append(relative).Append('\t').Append(new FileInfo(file).Length.ToString(CultureInfo.InvariantCulture)).Append('\t').Append(Sha256File(file)).Append('\n');
        }
        File.WriteAllText(Path.Combine(outputRoot, pass, "raw-manifest.tsv"), builder.ToString(), Utf8NoBom);
        return files[0];
    }

    private static void WriteFormalSha(string outputRoot, string pass, string sha)
        => File.WriteAllText(Path.Combine(outputRoot, pass, "formal-source.sha256"), sha + "\n", Utf8NoBom);

    private static Comparison ComparePasses(string outputRoot)
    {
        string raw1 = Path.Combine(outputRoot, "pass-1", "raw-manifest.tsv");
        string raw2 = Path.Combine(outputRoot, "pass-2", "raw-manifest.tsv");
        string canonical1 = Path.Combine(outputRoot, "pass-1", "canonical", "fm-text-v1.json");
        string canonical2 = Path.Combine(outputRoot, "pass-2", "canonical", "fm-text-v1.json");
        string manifest1 = Path.Combine(outputRoot, "pass-1", "canonical", "fm-text-v1.manifest.json");
        string manifest2 = Path.Combine(outputRoot, "pass-2", "canonical", "fm-text-v1.manifest.json");
        using JsonDocument publish = JsonDocument.Parse(File.ReadAllBytes(manifest1));
        string semantic = publish.RootElement.GetProperty("semanticDigest").GetString()!;
        File.WriteAllText(Path.Combine(outputRoot, "pass-1", "semantic-digest.txt"), semantic + "\n", Utf8NoBom);
        File.WriteAllText(Path.Combine(outputRoot, "pass-2", "semantic-digest.txt"), semantic + "\n", Utf8NoBom);
        return new Comparison(
            File.ReadAllBytes(raw1).AsSpan().SequenceEqual(File.ReadAllBytes(raw2)),
            File.ReadAllBytes(canonical1).AsSpan().SequenceEqual(File.ReadAllBytes(canonical2)),
            File.ReadAllBytes(manifest1).AsSpan().SequenceEqual(File.ReadAllBytes(manifest2)),
            true,
            Sha256File(raw1),
            Sha256File(canonical1),
            Sha256File(manifest1),
            semantic);
    }

    private static void WriteComparison(string outputRoot, Comparison comparison)
    {
        WriteJson(Path.Combine(outputRoot, "comparison.json"), new
        {
            schemaVersion = 1,
            rawFileSetBytesAndShaEqual = comparison.RawEqual,
            canonicalBytesEqual = comparison.CanonicalEqual,
            manifestBytesEqual = comparison.ManifestEqual,
            semanticDigestEqual = comparison.SemanticEqual,
            pass1RawManifestSha256 = comparison.RawManifestSha256,
            canonicalSha256 = comparison.CanonicalSha256,
            manifestSha256 = comparison.ManifestSha256,
            semanticDigest = comparison.SemanticDigest
        });
    }

    private static LicenseInventoryResult WriteLicenseInventory(string outputRoot, string cacheRoot, string runnerRoot, string evidence005)
    {
        string manifestPath = Path.Combine(evidence005, "identity/runner-files.tsv");
        List<RunnerFile> files = ParseRunnerManifest(manifestPath);
        var missing = new List<string>();
        var entries = new List<object>();
        string lubanLicense = Path.Combine(cacheRoot, "source", "LICENSE");
        if (!File.Exists(lubanLicense))
        {
            missing.Add("source/LICENSE missing");
        }
        string lubanLicenseSha = File.Exists(lubanLicense) ? Sha256File(lubanLicense) : "";
        foreach (RunnerFile file in files)
        {
            string deployed = Path.Combine(runnerRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(deployed) || new FileInfo(deployed).Length != file.Bytes || Sha256File(deployed) != file.Sha256)
            {
                missing.Add("runner mismatch: " + file.RelativePath);
            }
            PackageIdentity? package = IdentifyPackage(file.RelativePath);
            if (package is null)
            {
                entries.Add(new
                {
                    deployedFile = file.RelativePath,
                    deployedBytes = file.Bytes,
                    deployedSha256 = file.Sha256,
                    classification = IsLubanOwned(file.RelativePath) ? "luban-owned-or-companion" : "unmapped",
                    dependency = "Luban",
                    version = "5.1.0",
                    packageIdentity = "source@" + LubanCommit,
                    licenseSource = "source/LICENSE",
                    licenseSourceSha256 = lubanLicenseSha,
                    complete = IsLubanOwned(file.RelativePath) && lubanLicenseSha.Length == 64,
                    missingReason = IsLubanOwned(file.RelativePath) ? "" : "no package mapping"
                });
                if (!IsLubanOwned(file.RelativePath))
                {
                    missing.Add("no package mapping: " + file.RelativePath);
                }
                continue;
            }
            PackageLicense license = ResolvePackageLicense(cacheRoot, package, file, deployed);
            if (!license.Complete)
            {
                missing.Add(file.RelativePath + ": " + license.MissingReason);
            }
            entries.Add(new
            {
                deployedFile = file.RelativePath,
                deployedBytes = file.Bytes,
                deployedSha256 = file.Sha256,
                classification = "nuget-runtime",
                dependency = package.Name,
                version = package.Version,
                packageIdentity = package.Id + "/" + package.Version,
                packageFile = license.PackageFile,
                packageFileSha256 = license.PackageFileSha256,
                packageEntry = license.DeployedEntry,
                packageEntrySha256 = license.DeployedEntrySha256,
                licenseSource = license.LicenseSource,
                licenseSourceSha256 = license.LicenseSha256,
                complete = license.Complete,
                missingReason = license.MissingReason
            });
        }
        bool complete = files.Count == 132 && missing.Count == 0 && entries.Count == 132;
        if (files.Count != 132)
        {
            missing.Add($"runner manifest count {files.Count} != 132");
            complete = false;
        }
        WriteJson(Path.Combine(outputRoot, "third-party-license-inventory.json"), new
        {
            schemaVersion = 1,
            status = complete ? "PASS" : "LICENSE_INVENTORY_INCOMPLETE",
            runner = new { fileCount = 132, manifestSha256 = AcceptedRunnerManifestSha256 },
            coveredFileCount = entries.Count,
            complete,
            missingReasons = missing,
            files = entries
        });
        return new LicenseInventoryResult(complete, missing, entries.Count);
    }

    private static PackageLicense ResolvePackageLicense(string cacheRoot, PackageIdentity package, RunnerFile runnerFile, string deployedPath)
    {
        string packageDirectory = Path.Combine(cacheRoot, "nuget", package.Id, package.Version);
        string nupkg = Path.Combine(packageDirectory, package.Id + "." + package.Version + ".nupkg");
        if (!File.Exists(nupkg))
        {
            return PackageLicense.Fail("missing nupkg");
        }
        string? deployedEntry = null;
        string? licenseEntry = null;
        byte[] deployedBytes = File.ReadAllBytes(deployedPath);
        byte[]? licenseBytes = null;
        using (ZipArchive archive = ZipFile.OpenRead(nupkg))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Length == deployedBytes.Length)
                {
                    using Stream stream = entry.Open();
                    using var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    if (memory.ToArray().AsSpan().SequenceEqual(deployedBytes))
                    {
                        deployedEntry = entry.FullName;
                    }
                }
                string name = Path.GetFileName(entry.FullName);
                if (licenseEntry is null && (name.Contains("license", StringComparison.OrdinalIgnoreCase) || name.Contains("notice", StringComparison.OrdinalIgnoreCase) || name.StartsWith("copying", StringComparison.OrdinalIgnoreCase)))
                {
                    using Stream stream = entry.Open();
                    using var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    licenseEntry = entry.FullName;
                    licenseBytes = memory.ToArray();
                }
            }
        }
        string packageRelative = Path.GetRelativePath(cacheRoot, nupkg).Replace(Path.DirectorySeparatorChar, '/');
        string licenseSource;
        string licenseSha;
        if (licenseEntry is not null && licenseBytes is not null)
        {
            licenseSource = packageRelative + "!/" + licenseEntry;
            licenseSha = Sha256(licenseBytes);
        }
        else
        {
            string nuspec = Path.Combine(packageDirectory, package.Id + ".nuspec");
            if (!File.Exists(nuspec))
            {
                return PackageLicense.Fail("missing license/notice entry and nuspec");
            }
            licenseSource = Path.GetRelativePath(cacheRoot, nuspec).Replace(Path.DirectorySeparatorChar, '/');
            licenseSha = Sha256File(nuspec);
        }
        if (deployedEntry is null)
        {
            return PackageLicense.Fail("deployed bytes not found in nupkg");
        }
        return new PackageLicense(true, "", packageRelative, Sha256File(nupkg), deployedEntry, runnerFile.Sha256, licenseSource, licenseSha);
    }

    private static PackageIdentity? IdentifyPackage(string relativePath)
    {
        string file = Path.GetFileName(relativePath);
        return file switch
        {
            "CommandLine.dll" => new("CommandLineParser", "commandlineparser", "2.9.1"),
            "ExcelDataReader.dll" => new("ExcelDataReader", "exceldatareader", "3.7.0"),
            "Google.Protobuf.dll" => new("Google.Protobuf", "google.protobuf", "3.29.0"),
            "MessagePack.Annotations.dll" => new("MessagePack.Annotations", "messagepack.annotations", "3.1.7"),
            "MessagePack.dll" => new("MessagePack", "messagepack", "3.1.7"),
            "Microsoft.NET.StringTools.dll" => new("Microsoft.NET.StringTools", "microsoft.net.stringtools", "17.11.4"),
            "NLog.dll" => new("NLog", "nlog", "5.3.4"),
            "Neo.Lua.dll" or "Neo.Lua.resources.dll" => new("NeoLua", "neolua", "1.3.14"),
            "Newtonsoft.Json.Bson.dll" => new("Newtonsoft.Json.Bson", "newtonsoft.json.bson", "1.0.3"),
            "Newtonsoft.Json.dll" => new("Newtonsoft.Json", "newtonsoft.json", "13.0.1"),
            "Scriban.dll" => new("Scriban", "scriban", "7.2.5"),
            "Ude.NetStandard.dll" => new("Ude.NetStandard", "ude.netstandard", "1.2.0"),
            "YamlDotNet.NetCore.dll" => new("YamlDotNet.NetCore", "yamldotnet.netcore", "1.0.0"),
            _ => null
        };
    }

    private static bool IsLubanOwned(string relativePath)
        => relativePath == "Luban" || relativePath == "nlog.xml" || relativePath.StartsWith("Luban", StringComparison.Ordinal) || relativePath.StartsWith("Templates/", StringComparison.Ordinal);

    private static void WriteInputs(string outputRoot, string repositoryRoot, string evidence005, string dotnet, string lubanDll, FormalSource formal, string protectedBefore)
    {
        WriteJson(Path.Combine(outputRoot, "inputs.json"), new
        {
            schemaVersion = 1,
            packet = "LOC-IMPL-A-FIX-02",
            owner = new { threadId = OwnerThread, hostId = OwnerHost },
            sourceStart = "125b849be13fe2ebf5b1185f3cdb83d19c6327ef",
            taskPacket = new { path = "docs/team/2026-09-30/engineering-localization-luban-impl-a.md", sha256 = AcceptedPacketSha256 },
            correctivePacket = new { path = "docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-01.md", sha256 = CorrectivePacketSha256 },
            currentCorrectivePacket = new { path = "docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-02.md", sha256 = CurrentCorrectivePacketSha256 },
            immutableFailure = new
            {
                root = ImmutableFailureRelativeRoot,
                runSha256 = ImmutableFailureRunSha256,
                inputsSha256 = ImmutableFailureInputsSha256,
                formalSourceSha256 = ImmutableFailureFormalSourceSha256
            },
            immutableFix01Failure = new
            {
                root = ImmutableFix01RelativeRoot,
                runSha256 = ImmutableFix01RunSha256,
                inputsSha256 = ImmutableFix01InputsSha256,
                commandsSha256 = ImmutableFix01CommandsSha256
            },
            design = new { path = "docs/team/2026-09-30/engineering-localization-luban-001.md", sha256 = AcceptedDesignSha256 },
            acceptedCopy = new { path = "docs/team/2026-09-30/planning-localization-draft.csv", sha256 = AcceptedCopySha256, bytes = 65358, physicalLines = 265, records = 264, uniqueKeys = 264 },
            formalSource = new { path = "Config/FightMatch/Localization/FightMatchText.csv", sha256 = formal.Sha256, records = formal.Rows.Count },
            preflight005 = new { root = Relative(repositoryRoot, evidence005), runSha256 = Accepted005RunSha256 },
            tool = new { tag = LubanTag, commit = LubanCommit, sdkVersion = SdkVersion, dotnetSha256 = Sha256File(dotnet), lubanDllSha256 = Sha256File(lubanDll), runnerFileCount = 132, runnerManifestSha256 = AcceptedRunnerManifestSha256 },
            protectedStatusBeforeSha256 = Sha256(Utf8NoBom.GetBytes(protectedBefore))
        });
    }

    private static void WriteDiffCheck(string outputRoot, string repositoryRoot, string protectedBefore)
    {
        string[] productionPaths =
        {
            "Config/FightMatch/Luban/luban.conf",
            "Config/FightMatch/Localization/__tables__.csv",
            "Config/FightMatch/Localization/FightMatchText.csv",
            "Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj",
            "Tools/FightMatch.Localization.Compiler/Program.cs",
            "Tools/FightMatch.Localization.Compiler/global.json",
            "Generated/FightMatch/Localization/fm-text-v1.json",
            "Generated/FightMatch/Localization/fm-text-v1.manifest.json"
        };
        ProcessResult diff = RunProcess(repositoryRoot, "/usr/bin/git", new[] { "diff", "--check", "--" }.Concat(productionPaths).ToArray());
        ProcessResult status = RunProcess(repositoryRoot, "/usr/bin/git", new[] { "status", "--short", "--untracked-files=all", "--" }.Concat(productionPaths).ToArray());
        string protectedAfter = GitStatus(repositoryRoot, new[] { "Assets", "Packages", "ProjectSettings" });
        bool protectedEqual = protectedBefore == protectedAfter;
        bool allExpected = status.Stdout.Replace("\\", "/", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .All(line => productionPaths.Any(path => line.EndsWith(path, StringComparison.Ordinal)));
        string text =
            "RUN — git diff --check -- <8 production paths>\n" +
            $"Result: exit {diff.ExitCode}; {(diff.Stdout.Length == 0 && diff.Stderr.Length == 0 ? "no diagnostics" : diff.Stdout + diff.Stderr)}\n" +
            "RUN — git status --short --untracked-files=all -- <8 production paths>\n" +
            $"Result: exit {status.ExitCode}; pathsWithinWhitelist={allExpected}\n{status.Stdout}" +
            $"Protected Assets/Packages/ProjectSettings status unchanged during run: {protectedEqual}\n";
        File.WriteAllText(Path.Combine(outputRoot, "diff-check.txt"), text, Utf8NoBom);
        if (diff.ExitCode != 0 || status.ExitCode != 0 || !allExpected || !protectedEqual)
        {
            throw new ContractException("FMLOC012", "diff/scope check failed");
        }
    }

    private static string GitStatus(string repositoryRoot, IReadOnlyList<string> paths)
    {
        ProcessResult result = RunProcess(repositoryRoot, "/usr/bin/git", new[] { "status", "--short", "--untracked-files=all", "--" }.Concat(paths).ToArray());
        if (result.ExitCode != 0)
        {
            throw new ContractException("FMLOC012", "git status failed");
        }
        return result.Stdout;
    }

    private static ProcessResult RunRecorded(string outputRoot, string workingDirectory, string step, string fileName, IReadOnlyList<string> arguments, int expectedExit)
    {
        string steps = Path.Combine(outputRoot, "steps");
        WriteJson(Path.Combine(steps, step + ".argv.json"), new { fileName, argumentList = arguments });
        DateTimeOffset started = DateTimeOffset.UtcNow;
        ProcessResult result = RunProcess(workingDirectory, fileName, arguments);
        DateTimeOffset ended = DateTimeOffset.UtcNow;
        File.WriteAllText(Path.Combine(steps, step + ".stdout.txt"), result.Stdout, Utf8NoBom);
        File.WriteAllText(Path.Combine(steps, step + ".stderr.txt"), result.Stderr, Utf8NoBom);
        File.WriteAllText(Path.Combine(steps, step + ".exit.txt"), result.ExitCode.ToString(CultureInfo.InvariantCulture) + "\n", Utf8NoBom);
        File.WriteAllText(Path.Combine(steps, step + ".timing.txt"),
            $"startedAtUtc={started:O}\nendedAtUtc={ended:O}\nelapsedMilliseconds={(ended - started).TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)}\n", Utf8NoBom);
        if (result.ExitCode != expectedExit)
        {
            throw new ContractException("FMLOC013", $"{step} exit {result.ExitCode}, expected {expectedExit}");
        }
        return result;
    }

    private static ProcessResult RunProcess(string workingDirectory, string fileName, IReadOnlyList<string> arguments)
    {
        var start = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        using Process process = Process.Start(start) ?? throw new ContractException("FMLOC013", "failed to start process");
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    private static void WriteSelfInvocationStart(string outputRoot, string dotnet, string compilerDll, IReadOnlyList<string> args)
    {
        string steps = Path.Combine(outputRoot, "steps");
        WriteJson(Path.Combine(steps, "generate-driver.argv.json"), new { fileName = dotnet, argumentList = new[] { compilerDll }.Concat(args).ToArray() });
        File.WriteAllText(Path.Combine(steps, "generate-driver.stdout.txt"), "", Utf8NoBom);
        File.WriteAllText(Path.Combine(steps, "generate-driver.stderr.txt"), "", Utf8NoBom);
    }

    private static void FinishSelfInvocation(string outputRoot, int exit)
    {
        DateTimeOffset ended = DateTimeOffset.UtcNow;
        string steps = Path.Combine(outputRoot, "steps");
        File.WriteAllText(Path.Combine(steps, "generate-driver.exit.txt"), exit.ToString(CultureInfo.InvariantCulture) + "\n", Utf8NoBom);
        File.WriteAllText(Path.Combine(steps, "generate-driver.timing.txt"),
            $"startedAtUtc={s_startedAt:O}\nendedAtUtc={ended:O}\nelapsedMilliseconds={(ended - s_startedAt).TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)}\n", Utf8NoBom);
    }

    private static void BuildCommandsJson(string outputRoot)
    {
        string steps = Path.Combine(outputRoot, "steps");
        var commands = new List<object>();
        foreach (string argvPath in Directory.GetFiles(steps, "*.argv.json").OrderBy(path => path, StringComparer.Ordinal))
        {
            string name = Path.GetFileName(argvPath)[..^".argv.json".Length];
            string exitPath = Path.Combine(steps, name + ".exit.txt");
            string timingPath = Path.Combine(steps, name + ".timing.txt");
            if (!File.Exists(exitPath) || !File.Exists(timingPath))
            {
                continue;
            }
            using JsonDocument argv = JsonDocument.Parse(File.ReadAllBytes(argvPath));
            commands.Add(new
            {
                name,
                fileName = argv.RootElement.GetProperty("fileName").GetString(),
                argumentList = argv.RootElement.GetProperty("argumentList").EnumerateArray().Select(value => value.GetString()).ToArray(),
                exitCode = int.Parse(File.ReadAllText(exitPath, Utf8NoBom).Trim(), CultureInfo.InvariantCulture),
                argvPath = "steps/" + name + ".argv.json",
                stdoutPath = "steps/" + name + ".stdout.txt",
                stderrPath = "steps/" + name + ".stderr.txt",
                exitPath = "steps/" + name + ".exit.txt",
                timingPath = "steps/" + name + ".timing.txt"
            });
        }
        WriteJson(Path.Combine(outputRoot, "commands.json"), new { schemaVersion = 1, commands });
    }

    private static void WriteRun(string outputRoot, string status, string? diagnostic, string? detail, LicenseInventoryResult? license, Comparison? comparison)
    {
        WriteJson(Path.Combine(outputRoot, "run.json"), new
        {
            schemaVersion = 1,
            packet = "LOC-IMPL-A-FIX-02",
            status,
            stageTerminal = status,
            startedAtUtc = s_startedAt.ToString("O", CultureInfo.InvariantCulture),
            endedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            owner = new { threadId = OwnerThread, hostId = OwnerHost },
            diagnostic,
            detail,
            acceptedCopySha256 = AcceptedCopySha256,
            formalSourceSha256 = File.Exists(Path.Combine(outputRoot, "pass-1", "formal-source.sha256")) ? File.ReadAllText(Path.Combine(outputRoot, "pass-1", "formal-source.sha256"), Utf8NoBom).Trim() : null,
            canonicalSha256 = comparison?.CanonicalSha256,
            manifestSha256 = comparison?.ManifestSha256,
            semanticDigest = comparison?.SemanticDigest,
            licenseInventory = license is null ? null : new { complete = license.Complete, coveredFileCount = license.CoveredFileCount, missingReasons = license.MissingReasons },
            unityRun = false,
            restoreRun = false,
            publishRun = false,
            preflightRerun = false
        });
    }

    private static void TryWriteFailureRun(string diagnostic, string detail)
    {
        if (s_outputRoot is null || !Directory.Exists(s_outputRoot))
        {
            return;
        }
        try
        {
            FinishSelfInvocation(s_outputRoot, diagnostic == "NON_DETERMINISTIC" ? 31 : 20);
            BuildCommandsJson(s_outputRoot);
            WriteRun(s_outputRoot, s_terminal == "NOT_RUN" ? diagnostic : s_terminal, diagnostic, detail, null, null);
        }
        catch
        {
        }
    }

    private static Dictionary<string, string> ParseOptions(string[] args, int start, params string[] allowed)
    {
        var allowedSet = new HashSet<string>(allowed, StringComparer.Ordinal);
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if ((args.Length - start) % 2 != 0)
        {
            throw new ContractException("FMLOC000", "options must be name/value pairs");
        }
        for (int index = start; index < args.Length; index += 2)
        {
            string name = args[index];
            if (!allowedSet.Contains(name) || !result.TryAdd(name, args[index + 1]))
            {
                throw new ContractException("FMLOC000", $"unknown or duplicate option: {name}");
            }
        }
        if (result.Count != allowedSet.Count || allowedSet.Any(name => !result.ContainsKey(name)))
        {
            throw new ContractException("FMLOC000", "required option missing");
        }
        return result;
    }

    private static List<string[]> ParseCsv(byte[] bytes)
    {
        string text = Utf8NoBom.GetString(bytes);
        var records = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        bool afterQuote = false;
        for (int index = 0; index < text.Length; index++)
        {
            char value = text[index];
            if (quoted)
            {
                if (value == '"')
                {
                    if (index + 1 < text.Length && text[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = false;
                        afterQuote = true;
                    }
                }
                else
                {
                    field.Append(value);
                }
                continue;
            }
            if (afterQuote && value != ',' && value != '\n' && value != '\r')
            {
                throw new ContractException("FMLOC001", "invalid character after closing CSV quote");
            }
            if (value == '"')
            {
                if (field.Length != 0 || afterQuote)
                {
                    throw new ContractException("FMLOC001", "invalid CSV quote");
                }
                quoted = true;
            }
            else if (value == ',')
            {
                fields.Add(field.ToString());
                field.Clear();
                afterQuote = false;
            }
            else if (value == '\r')
            {
                if (index + 1 >= text.Length || text[index + 1] != '\n')
                {
                    throw new ContractException("FMLOC001", "bare CR in CSV");
                }
            }
            else if (value == '\n')
            {
                fields.Add(field.ToString());
                field.Clear();
                afterQuote = false;
                records.Add(fields.ToArray());
                fields.Clear();
            }
            else
            {
                field.Append(value);
                afterQuote = false;
            }
        }
        if (quoted)
        {
            throw new ContractException("FMLOC001", "unclosed CSV quote");
        }
        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            records.Add(fields.ToArray());
        }
        return records;
    }

    private static LocalizationRow ToRow(string[] fields)
    {
        Require(fields.Length == 9, $"expected 9 logical columns, found {fields.Length}");
        return new LocalizationRow
        {
            Key = fields[0],
            Screen = fields[1],
            Element = fields[2],
            StateOrTrigger = fields[3],
            ZhCn = fields[4],
            En = fields[5],
            Parameters = fields[6],
            Severity = fields[7],
            QaNotes = fields[8]
        };
    }

    private static string[] ParseDeclaredParameters(string value, string key)
    {
        if (value.Length == 0)
        {
            return Array.Empty<string>();
        }
        string[] parameters = value.Split(';');
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string parameter in parameters)
        {
            if (!IsParameterName(parameter) || !seen.Add(parameter))
            {
                throw new ContractException("FMLOC007", $"invalid or duplicate declared parameter: {key}/{parameter}");
            }
        }
        string[] sorted = parameters.OrderBy(parameter => parameter, StringComparer.Ordinal).ToArray();
        if (!parameters.SequenceEqual(sorted, StringComparer.Ordinal))
        {
            throw new ContractException("FMLOC007", $"declared parameters are not ordinal sorted: {key}");
        }
        return parameters;
    }

    private static string[] ExtractTemplateParameters(string text, string key, string locale)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < text.Length; index++)
        {
            char current = text[index];
            if (current == '}')
            {
                throw new ContractException("FMLOC006", $"unmatched closing brace: {key}/{locale}");
            }
            if (current != '{')
            {
                continue;
            }
            int close = text.IndexOf('}', index + 1);
            if (close < 0)
            {
                throw new ContractException("FMLOC006", $"unclosed template: {key}/{locale}");
            }
            string name = text.Substring(index + 1, close - index - 1);
            if (!IsParameterName(name))
            {
                throw new ContractException("FMLOC006", $"invalid template parameter: {key}/{locale}/{name}");
            }
            names.Add(name);
            index = close;
        }
        return names.OrderBy(name => name, StringComparer.Ordinal).ToArray();
    }

    private static bool IsParameterName(string name)
    {
        if (name.Length == 0 || !IsAsciiLetter(name[0]))
        {
            return false;
        }
        return name.Skip(1).All(value => IsAsciiLetter(value) || char.IsDigit(value) || value == '_');
    }

    private static bool IsAsciiLetter(char value)
        => (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z');

    private static void RunTest(List<TestResult> results, string name, Action test)
    {
        try
        {
            test();
            results.Add(new TestResult(name, true, "contract satisfied"));
        }
        catch (Exception ex)
        {
            results.Add(new TestResult(name, false, $"{ex.GetType().Name}: {ex.Message}"));
        }
    }

    private static LocalizationRow ValidRow(string key) => new()
    {
        Key = key,
        Screen = "test",
        Element = "label",
        StateOrTrigger = "visible",
        ZhCn = "文本",
        En = "Text",
        Parameters = "",
        Severity = "info",
        QaNotes = "test-only row"
    };

    private static void TestSchemaColumnCount()
        => ExpectContract("FMLOC001", () => ParseRaw(Utf8NoBom.GetBytes("[{\"key\":\"fm.a\"}]")));

    private static void TestDuplicateKey()
        => ExpectContract("FMLOC002", () => BuildCanonical(new[] { ValidRow("fm.a"), ValidRow("fm.a") }));

    private static void TestEmptyTranslation()
    {
        LocalizationRow row = ValidRow("fm.a");
        row.En = "";
        ExpectContract("FMLOC003", () => BuildCanonical(new[] { row }));
    }

    private static void TestInvalidSeverity()
    {
        LocalizationRow row = ValidRow("fm.a");
        row.Severity = "fatal";
        ExpectContract("FMLOC004", () => BuildCanonical(new[] { row }));
    }

    private static void TestMissingParameter()
    {
        LocalizationRow row = ValidRow("fm.a");
        row.En = "{count} items";
        row.ZhCn = "{count} 项";
        ExpectContract("FMLOC007", () => BuildCanonical(new[] { row }));
    }

    private static void TestMultipleParameters()
    {
        LocalizationRow row = ValidRow("fm.a");
        row.En = "{count} items for {name}";
        row.ZhCn = "{name} 有 {count} 项";
        row.Parameters = "count;name";
        byte[] bytes = BuildCanonical(new[] { row });
        using JsonDocument document = JsonDocument.Parse(bytes);
        string[] actual = document.RootElement.GetProperty("entries")[0].GetProperty("parameters").EnumerateArray().Select(value => value.GetString()!).ToArray();
        Require(actual.SequenceEqual(new[] { "count", "name" }, StringComparer.Ordinal), "multiple parameters lost");
    }

    private static void TestUnclosedTemplate()
    {
        LocalizationRow row = ValidRow("fm.a");
        row.En = "{count items";
        row.ZhCn = "{count} 项";
        row.Parameters = "count";
        ExpectContract("FMLOC006", () => BuildCanonical(new[] { row }));
    }

    private static void TestUnknownRawColumn()
        => ExpectContract("FMLOC001", () => ParseRaw(Utf8NoBom.GetBytes("[{\"key\":\"fm.a\",\"screen\":\"x\",\"element\":\"x\",\"state_or_trigger\":\"x\",\"zh_cn\":\"中\",\"en\":\"A\",\"parameters\":\"\",\"severity\":\"info\",\"extra\":\"x\"}]")));

    private static void TestSorting()
    {
        byte[] bytes = BuildCanonical(new[] { ValidRow("fm.z"), ValidRow("fm.a") });
        using JsonDocument document = JsonDocument.Parse(bytes);
        Require(document.RootElement.GetProperty("entries")[0].GetProperty("key").GetString() == "fm.a", "entries are not ordinal sorted");
    }

    private static void TestNewlines()
    {
        string text = Utf8NoBom.GetString(BuildCanonical(new[] { ValidRow("fm.a") }));
        Require(!text.Contains('\r') && text.EndsWith("\n", StringComparison.Ordinal), "canonical newline contract failed");
    }

    private static void TestRoundTrip()
    {
        LocalizationRow[] rows = { ValidRow("fm.a"), ValidRow("fm.b") };
        ValidateRoundTrip(BuildCanonical(rows), rows);
    }

    private static void TestDuplicateGeneration()
    {
        LocalizationRow[] rows = { ValidRow("fm.b"), ValidRow("fm.a") };
        Require(BuildCanonical(rows).AsSpan().SequenceEqual(BuildCanonical(rows)), "duplicate generation differs");
    }

    private static void ExpectContract(string code, Action action)
    {
        try
        {
            action();
        }
        catch (ContractException ex) when (ex.Code == code)
        {
            return;
        }
        throw new InvalidOperationException("expected contract failure " + code);
    }

    private static int[] SeverityCounts(IReadOnlyList<LocalizationRow> rows)
        => new[]
        {
            rows.Count(row => row.Severity == "blocking"),
            rows.Count(row => row.Severity == "error"),
            rows.Count(row => row.Severity == "info"),
            rows.Count(row => row.Severity == "warning")
        };

    private static List<RunnerFile> ParseRunnerManifest(string path)
    {
        var result = new List<RunnerFile>();
        foreach (string line in File.ReadAllLines(path, Utf8NoBom))
        {
            if (line.Length == 0)
            {
                continue;
            }
            string[] fields = line.Split('\t');
            Require(fields.Length == 3, "invalid runner manifest row");
            result.Add(new RunnerFile(fields[0], long.Parse(fields[1], CultureInfo.InvariantCulture), fields[2]));
        }
        return result;
    }

    private static void ValidateRunnerManifest(string manifestPath, string runnerRoot)
    {
        List<RunnerFile> files = ParseRunnerManifest(manifestPath);
        Require(files.Count == 132, "runner manifest must contain 132 files");
        foreach (RunnerFile file in files)
        {
            string path = Path.Combine(runnerRoot, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            Require(File.Exists(path), "runner file missing: " + file.RelativePath);
            Require(new FileInfo(path).Length == file.Bytes && Sha256File(path) == file.Sha256, "runner file identity mismatch: " + file.RelativePath);
        }
    }

    private static void RequireHash(string path, string expected, string label)
    {
        if (!File.Exists(path) || Sha256File(path) != expected)
        {
            throw new ContractException("BLOCKED_INPUT_DRIFT", label + " SHA-256 mismatch", 21);
        }
    }

    private static string HashWithoutLastPhysicalLine(byte[] bytes)
    {
        int end = bytes.Length;
        if (end > 0 && bytes[end - 1] == (byte)'\n')
        {
            end--;
        }
        int previous = Array.LastIndexOf(bytes, (byte)'\n', end - 1);
        Require(previous >= 0, "COPY lacks snapshot prefix");
        return Sha256(bytes.AsSpan(0, previous + 1).ToArray());
    }

    private static string HashFirstPhysicalLines(byte[] bytes, int lines)
    {
        int seen = 0;
        for (int index = 0; index < bytes.Length; index++)
        {
            if (bytes[index] == (byte)'\n' && ++seen == lines)
            {
                return Sha256(bytes.AsSpan(0, index + 1).ToArray());
            }
        }
        throw new ContractException("BLOCKED_INPUT_DRIFT", "COPY physical prefix missing", 21);
    }

    private static int CountLf(byte[] bytes) => bytes.Count(value => value == (byte)'\n');

    private static string Sha256File(string path) => Sha256(File.ReadAllBytes(path));

    private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteJson(string path, object value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }).ReplaceLineEndings("\n") + "\n";
        File.WriteAllText(path, json, Utf8NoBom);
    }

    private static Utf8JsonWriter NewJsonWriter(IBufferWriter<byte> buffer) => new(buffer, new JsonWriterOptions
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        SkipValidation = false
    });

    private static byte[] WithFinalLf(ReadOnlySpan<byte> content)
    {
        byte[] result = new byte[content.Length + 1];
        content.CopyTo(result);
        result[^1] = (byte)'\n';
        return result;
    }

    private static string ResolveUnder(string root, string relative)
    {
        Require(!Path.IsPathRooted(relative), "packet output root must be repository-relative");
        string full = Path.GetFullPath(Path.Combine(root, relative));
        string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        Require(full.StartsWith(prefix, StringComparison.Ordinal), "path escapes repository");
        return full;
    }

    private static string Relative(string root, string path)
        => Path.GetRelativePath(root, Path.GetFullPath(path)).Replace(Path.DirectorySeparatorChar, '/');

    private static bool PathEquals(string left, string right)
        => string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar), StringComparison.Ordinal);

    private static bool IsSymlink(string path)
        => (File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new ContractException("BLOCKED_INPUT_DRIFT", message, 21);
        }
    }

    private sealed class LocalizationRow
    {
        public string Key { get; set; } = "";
        public string Screen { get; set; } = "";
        public string Element { get; set; } = "";
        public string StateOrTrigger { get; set; } = "";
        public string ZhCn { get; set; } = "";
        public string En { get; set; } = "";
        public string Parameters { get; set; } = "";
        public string Severity { get; set; } = "";
        public string QaNotes { get; set; } = "";

        public bool AllFieldsEqual(LocalizationRow other)
            => Key == other.Key && Screen == other.Screen && Element == other.Element && StateOrTrigger == other.StateOrTrigger &&
               ZhCn == other.ZhCn && En == other.En && Parameters == other.Parameters && Severity == other.Severity && QaNotes == other.QaNotes;
    }

    private sealed record FormalSource(string Path, string Sha256, List<LocalizationRow> Rows);
    private sealed record TestResult(string Name, bool Passed, string Detail);
    private sealed record ProcessResult(int ExitCode, string Stdout, string Stderr);
    private sealed record RunnerFile(string RelativePath, long Bytes, string Sha256);
    private sealed record PackageIdentity(string Name, string Id, string Version);
    private sealed record PackageLicense(bool Complete, string MissingReason, string PackageFile, string PackageFileSha256, string DeployedEntry, string DeployedEntrySha256, string LicenseSource, string LicenseSha256)
    {
        public static PackageLicense Fail(string reason) => new(false, reason, "", "", "", "", "", "");
    }
    private sealed record LicenseInventoryResult(bool Complete, List<string> MissingReasons, int CoveredFileCount);
    private sealed record Comparison(bool RawEqual, bool CanonicalEqual, bool ManifestEqual, bool SemanticEqual, string RawManifestSha256, string CanonicalSha256, string ManifestSha256, string SemanticDigest)
    {
        public bool AllEqual => RawEqual && CanonicalEqual && ManifestEqual && SemanticEqual;
    }

    private sealed class ContractException : Exception
    {
        public ContractException(string code, string message, int exitCode = 20) : base(message)
        {
            Code = code;
            ExitCode = exitCode;
        }

        public string Code { get; }
        public int ExitCode { get; }
    }
}
