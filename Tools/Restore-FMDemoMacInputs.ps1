[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedScriptSha256)
$ErrorActionPreference = 'Stop'
$project = '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch'
$root = $project+'/TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform'
$stage = $root+'/staging'
$scriptRelative = 'Tools/Restore-FMDemoMacInputs.ps1'
$planRelative = 'docs/system-design/2026-09-17/demo-cont-c-mac-platform-plan.json'
function Checked-Path([string]$base, [string]$relative) {
    if ([string]::IsNullOrEmpty($relative) -or $relative -match '[\\:\x00]' -or $relative.StartsWith('/') -or
        @($relative.Split('/') | Where-Object { $_ -in @('','.','..') }).Count) { throw 'Invalid relative input path' }
    $full = [IO.Path]::GetFullPath((Join-Path $base $relative))
    if (-not $full.StartsWith($base+'/', [StringComparison]::Ordinal)) { throw 'Input escaped fixed root' }
    for ($at=$full; $at; $at=[IO.Path]::GetDirectoryName($at)) {
        $item=$null
        try { $item=Get-Item -LiteralPath $at -Force -ErrorAction Stop } catch [System.Management.Automation.ItemNotFoundException] { }
        if ($null -ne $item -and ($item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint))) { throw 'Linked input path' }
    }
    return $full
}
function File-Identity([string]$path) {
    if (-not [IO.File]::Exists($path)) { return [pscustomobject]@{path=$path; exists=$false} }
    $item = Get-Item -LiteralPath $path
    return [pscustomobject]@{path=$path; exists=$true; bytes=$item.Length; sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
function Check-Identity([string]$path, $expected) {
    $actual = File-Identity $path
    if (-not $actual.exists -or $actual.bytes -ne $expected.bytes -or $actual.sha256 -cne $expected.sha256) { throw "Identity mismatch: $path" }
    return $actual
}
function New-Bytes([string]$path, [byte[]]$bytes) {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
    $stream = [IO.File]::Open($path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $stream.Write($bytes,0,$bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
}
function New-Json([string]$name, $value) {
    New-Bytes (Checked-Path $root $name) ([Text.UTF8Encoding]::new($false).GetBytes(($value | ConvertTo-Json -Depth 35)+"`n"))
}
function Run-Child([string]$name, [string[]]$arguments) {
    $entry=[ordered]@{executable=$mono; arguments=$arguments; workingDirectory=$stage; pid=$null; actualExitCode=$null; startedAtUtc=[DateTime]::UtcNow.ToString('o')}
    $process=[Diagnostics.Process]::new()
    $process.StartInfo=[Diagnostics.ProcessStartInfo]::new($mono)
    $process.StartInfo.UseShellExecute=$false
    $process.StartInfo.WorkingDirectory=$stage
    $process.StartInfo.RedirectStandardOutput=$true
    $process.StartInfo.RedirectStandardError=$true
    foreach ($argument in $arguments) { $process.StartInfo.ArgumentList.Add($argument) }
    try {
        if (-not $process.Start()) { throw 'Child did not start' }
        $entry.pid=$process.Id
        $outTask=$process.StandardOutput.ReadToEndAsync(); $errTask=$process.StandardError.ReadToEndAsync()
        $process.WaitForExit(); $entry.actualExitCode=$process.ExitCode
        $stdout=$outTask.GetAwaiter().GetResult(); $stderr=$errTask.GetAwaiter().GetResult()
        foreach ($log in @(@('stdout',$stdout),@('stderr',$stderr))) {
            $path=Checked-Path $root ($name+'.'+$log[0]+'.txt')
            $bytes=[Text.UTF8Encoding]::new($false).GetBytes($log[1])
            if ($bytes.Length -gt 4194304) { throw 'Child log exceeds signed budget' }
            New-Bytes $path $bytes
            $entry[$log[0]]=File-Identity $path
        }
    }
    catch { $entry.error=$_.Exception.ToString(); throw }
    finally { $entry.endedAtUtc=[DateTime]::UtcNow.ToString('o'); $record.processes+=,$entry; $process.Dispose() }
    if ($entry.actualExitCode -ne 0) { throw "Child $name failed with actual exit $($entry.actualExitCode)" }
    return $stdout
}
if (-not $IsMacOS -or $PSVersionTable.PSVersion.ToString() -ne '7.6.6') { throw 'Approved Mac PowerShell runtime required' }
$self=File-Identity (Checked-Path $project $scriptRelative)
if ($self.sha256 -cne $ExpectedScriptSha256) { throw 'Restore tool SHA mismatch' }
$monoPlan=Check-Identity (Checked-Path $project 'docs/system-design/2026-09-17/demo-cont-c-mac-platform-mono-plan.json') @{bytes=16595;sha256='3ea544f4cef042f61f8ad8a6253736eb209f0533e774e0f3aa4004fb92530b75'}
$plan=Get-Content -Raw -LiteralPath $monoPlan.path | ConvertFrom-Json
$correction=Check-Identity (Checked-Path $project 'docs/system-design/2026-09-17/demo-cont-c-mac-platform-mono-c1-plan.json') @{bytes=7515;sha256='683d2aa0358bc8b7281b6197ccd2b2dccf7315ed0d5b4050590907801d1fb74c'}
$allowed=(Get-Content -Raw -LiteralPath $correction.path | ConvertFrom-Json).exactAllowedEvidencePaths
if ($plan.projectPath -cne $project -or $allowed.Count -ne 64) { throw 'Fixed plan path membership mismatch' }
$owner=Get-Content -Raw -LiteralPath ($root+'/root-identity.json') | ConvertFrom-Json
if ($owner.authorTaskId -cne $plan.cThread -or $owner.authorTurnId -cne $plan.cTurnAtIssue) { throw 'Platform root ownership mismatch' }
if (Test-Path -LiteralPath ($root+'/restore-mono-result.json')) { throw 'Mono attempt already recorded; preserve it' }
$record=[ordered]@{status='Running'; purpose='Frozen input recovery only, not Unity test results'; authorTaskId=$plan.cThread; authorTurnId=$plan.cTurnAtIssue;
    startedAtUtc=[DateTime]::UtcNow.ToString('o'); script=$self; monoPlan=$monoPlan; correctionPlan=$correction; rootIdentity=(File-Identity ($root+'/root-identity.json'));
    frozenBefore=@(); configuration=@(); environment=@(); runnerSource=$null; runnerExe=$null; processes=@(); methods=@(); outputs=@(); restored=@(); afterChecks=@()}
try {
    $packets=[IO.File]::ReadAllText($project+'/docs/system-design/2026-09-17/system-task-packets.md').Replace("`r`n","`n")
    foreach ($packet in @(@(394,'CONT-C-MAC-MONO-PACKET-END','bdbfb79aeb1a0f0838c6445146c533baef12b65a318b8d3335b7481f469712f5'),@(395,'CONT-C-MAC-MONO-C1-PACKET-END','ab571161ad024e2988143b8745a439eed2b72b0e7ea681243093ef1faab02e9f'))) {
        $start=$packets.IndexOf('## '+$packet[0]+'.'); $end=$packets.IndexOf("`n"+$packet[1]+"`n",$start)+$packet[1].Length+2
        $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($packets.Substring($start,$end-$start)))).ToLowerInvariant()
        if ($hash -cne $packet[2]) { throw 'Frozen Mono packet changed' }
    }
    $record.packetSha256=@('bdbfb79aeb1a0f0838c6445146c533baef12b65a318b8d3335b7481f469712f5','ab571161ad024e2988143b8745a439eed2b72b0e7ea681243093ef1faab02e9f')
    $null=Check-Identity (Checked-Path $root 'before-text/Tools/Restore-FMDemoMacInputs.ps1.txt') $plan.sourceTool
    $record.frozenBefore+=@($record.rootIdentity,$monoPlan,$correction)
    $frozen=@($plan.basePlan,$plan.c1Plan)+@($plan.failedAttempt)+@($plan.compile003)+@($plan.generatorSources)+@($plan.managedAssemblies)+@($plan.sourceInputs)
    foreach ($file in $frozen) { $record.frozenBefore+=Check-Identity (Checked-Path $project $file.path) $file }
    $compiled=Get-Content -Raw -LiteralPath (Checked-Path $project $plan.compile003[1].path) | ConvertFrom-Json
    $compiledFiles=@($compiled.sourceFiles)+@($compiled.assetFiles)+@($compiled.dllFiles)
    foreach ($file in $compiledFiles) { $null=Check-Identity (Checked-Path $project $file.path) $file }
    foreach ($file in $plan.runtimeInputs) { $record.frozenBefore+=Check-Identity (Checked-Path ([IO.Path]::GetDirectoryName($file.path)) ([IO.Path]::GetFileName($file.path))) $file }
    $mono=$plan.runtimeInputs[0].path; $mcs=$plan.runtimeInputs[1].path
    $monoRoot=$mono.Substring(0,$mono.Length-'/bin/mono'.Length)
    foreach ($entry in [Environment]::GetEnvironmentVariables().GetEnumerator()) {
        if ($entry.Key -match '^(MONO_|DYLD_|LD_)|^(PATH|LANG|LC_ALL|HOME)$') { $record.environment+=@{name=$entry.Key;value=$entry.Value} }
        if ($entry.Key -match '^(MONO_|DYLD_|LD_)' -and $entry.Value) { throw "Runtime injection environment present: $($entry.Key)" }
    }
    $configs=@(($mcs+'.config'),([IO.Path]::GetDirectoryName($mcs)+'/mcs.rsp'),($root+'/restore-mono-runner.exe.config'),([Environment]::GetFolderPath('UserProfile')+'/.mono/config'))
    $configs+=@($plan.managedAssemblies | ForEach-Object { $project+'/'+$_.path+'.config' })
    foreach ($path in $configs) { $record.configuration+=File-Identity $path; if (Test-Path -LiteralPath $path) { throw "Unexpected runtime configuration: $path" } }
    foreach ($relative in @('etc/mono/config','etc/mono/4.5/machine.config','lib/mono/4.5/mscorlib.dll.config')) { $record.configuration+=File-Identity (Checked-Path $monoRoot $relative) }
    foreach ($input in $plan.sourceInputs) { $null=Check-Identity (Checked-Path $stage $input.path) $input }
    foreach ($input in $plan.goldenInputs) {
        if (Test-Path -LiteralPath (Checked-Path $stage $input.path)) { throw 'Staged output already exists; preserve it' }
        $target=Checked-Path $project $input.path
        if (Test-Path -LiteralPath $target) { $null=Check-Identity $target $input }
    }
    foreach ($leaf in $plan.additionalEvidencePaths | Select-Object -Skip 1) {
        if (Test-Path -LiteralPath (Checked-Path $root $leaf)) { throw "Prior Mono leaf exists: $leaf" }
    }
    $runner=@'
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
class FrozenInputRecovery
{
    const string Project = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch";
    const string Root = Project + "/TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform";
    const string Bcl = "__BCL__";
    static readonly string[] Assemblies = {
__ASSEMBLIES__
    };
    static readonly string[] Methods = {
__METHODS__
    };
    static string Hash(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var hash = new SHA256Managed())
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }
    static void Inspect(Assembly assembly)
    {
        string location = assembly.Location;
        bool allowed = location == Root + "/restore-mono-runner.exe" || location.StartsWith(Bcl, StringComparison.Ordinal);
        foreach (string row in Assemblies)
        {
            string[] parts = row.Split('|');
            if (location != Project + "/" + parts[0]) continue;
            if (new FileInfo(location).Length != long.Parse(parts[1]) || Hash(location) != parts[2])
                throw new InvalidOperationException("Assembly identity mismatch: " + location);
            allowed = true;
        }
        if (!allowed) throw new InvalidOperationException("Unapproved assembly: " + location);
        Console.WriteLine("ASSEMBLY\t" + location + "\t" + new FileInfo(location).Length + "\t" + Hash(location));
    }
    static int Main()
    {
        try
        {
            if (Directory.GetCurrentDirectory() != Root + "/staging" || Type.GetType("Mono.Runtime") == null)
                throw new InvalidOperationException("Fixed Mono working directory required");
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) Inspect(assembly);
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => Inspect(e.LoadedAssembly);
            Assembly tests = null;
            foreach (string row in Assemblies)
            {
                Assembly loaded = Assembly.LoadFrom(Project + "/" + row.Split('|')[0]);
                Inspect(loaded);
                if (loaded.GetName().Name == "FightMatch.Core.Tests") tests = loaded;
            }
            foreach (string name in Methods)
            {
                int split = name.LastIndexOf('.');
                Type type = tests.GetType(name.Substring(0, split), true);
                MethodInfo method = type.GetMethod(name.Substring(split + 1), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                if (method == null || method.ReturnType != typeof(void)) throw new InvalidOperationException("Original method missing: " + name);
                method.Invoke(Activator.CreateInstance(type), new object[0]);
                Console.WriteLine("COMPLETED\t" + name);
            }
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) Inspect(assembly);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.ToString()); return 1; }
    }
}
'@
    $runner=$runner.Replace('__BCL__',$monoRoot+'/lib/mono/').Replace('__ASSEMBLIES__',(($plan.managedAssemblies | ForEach-Object { '        "'+$_.path+'|'+$_.bytes+'|'+$_.sha256+'",' }) -join "`n"))
    $runner=$runner.Replace('__METHODS__',(($plan.methods | ForEach-Object { '        "'+$_+'",' }) -join "`n"))+"`n"
    if (($runner.TrimEnd("`n").Split("`n")).Count -gt 80) { throw 'Runner line budget exceeded' }
    $runnerPath=Checked-Path $root 'restore-mono-runner.cs'; $exe=Checked-Path $root 'restore-mono-runner.exe'
    New-Bytes $runnerPath ([Text.UTF8Encoding]::new($false).GetBytes($runner)); $record.runnerSource=File-Identity $runnerPath
    $null=Run-Child 'restore-mono-compile' @($mcs,'-noconfig','-target:exe','-debug-',('-out:'+$exe),$runnerPath)
    $record.runnerExe=File-Identity $exe
    if (-not $record.runnerExe.exists -or $record.runnerExe.bytes -gt 1048576) { throw 'Compiled runner missing or too large' }
    foreach ($path in @(($exe+'.mdb'),[IO.Path]::ChangeExtension($exe,'.pdb'))) { if (Test-Path -LiteralPath $path) { throw 'Unexpected debug output' } }
    $output=Run-Child 'restore-mono' @($exe)
    $record.methods=@($output.Split("`n") | Where-Object { $_.StartsWith("COMPLETED`t") } | ForEach-Object { $_.Substring(10).TrimEnd("`r") })
    if (($record.methods -join "`n") -cne ($plan.methods -join "`n")) { throw 'Four exact ordered completion markers required' }
    foreach ($file in @($record.frozenBefore)+@($compiledFiles)+@($self,$record.runnerSource,$record.runnerExe)) {
        $path=if ([IO.Path]::IsPathRooted($file.path)) { $file.path } else { Checked-Path $project $file.path }
        $null=Check-Identity $path $file
    }
    foreach ($input in $plan.goldenInputs) { $record.outputs+=Check-Identity (Checked-Path $stage $input.path) $input }
    foreach ($input in $plan.goldenInputs) {
        $target=Checked-Path $project $input.path
        if (Test-Path -LiteralPath $target) { $null=Check-Identity $target $input }
        else { New-Bytes $target ([IO.File]::ReadAllBytes((Checked-Path $stage $input.path))) }
        $record.restored+=Check-Identity $target $input
    }
    $record.status='FrozenInputsRestored'
}
catch { $record.status='Failed'; $record.error=$_.Exception.ToString(); throw }
finally {
    foreach ($file in @($record.frozenBefore)+@($compiledFiles)+@($self,$record.runnerSource,$record.runnerExe)+@($record.configuration | Where-Object exists)) {
        if ($null -eq $file) { continue }
        try { $path=if ([IO.Path]::IsPathRooted($file.path)) { $file.path } else { Checked-Path $project $file.path }; $null=Check-Identity $path $file }
        catch { $record.status='Failed'; $record.afterChecks+=$_.Exception.Message }
    }
    foreach ($file in $plan.sourceInputs) { try { $null=Check-Identity (Checked-Path $stage $file.path) $file } catch { $record.status='Failed'; $record.afterChecks+=$_.Exception.Message } }
    foreach ($path in $configs) { if (Test-Path -LiteralPath $path) { $record.status='Failed'; $record.afterChecks+="Runtime configuration appeared: $path" } }
    foreach ($file in Get-ChildItem -LiteralPath $root -File -Recurse -Force) {
        $relative=$file.FullName.Substring($project.Length+1)
        if (-not $relative.StartsWith($plan.evidenceRoot+'/io/',[StringComparison]::Ordinal) -and $relative -cnotin $allowed) { $record.status='Failed'; $record.afterChecks+="Unapproved leaf: $relative" }
    }
    $record.endedAtUtc=[DateTime]::UtcNow.ToString('o')
    New-Json 'restore-mono-result.json' $record
    if ($record.status -ceq 'FrozenInputsRestored') { New-Json 'golden-recovery.json' $record }
}
if ($record.status -cne 'FrozenInputsRestored') { throw 'Recovery postconditions failed; see preserved result' }
