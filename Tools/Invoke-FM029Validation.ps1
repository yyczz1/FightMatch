[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('PrepareAssets','Compile','HostTests','FullTests','QaBuild','DemoBuild')][string]$Mode,
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{64}$')][string]$ExpectedScriptSha256
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$project = '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch'
$stage = 'DEMO-029-MAC-R1'
$authorTurn = '01a0eaf3-31c0-7d83-9dfc-31c24012946f'
$planPath = 'docs/system-design/2026-09-17/demo-029-mac-r1-plan.json'
$evidence = 'TestArtifacts/FMDemo029/mac-r1'
Set-Location -LiteralPath $project
function Require($condition, [string]$message) { if (-not $condition) { throw "FM029: $message" } }
function Read-Json([string]$path) { Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -AsHashtable }
function Identity([string]$path) {
    $exists = [IO.File]::Exists($path)
    $value = [ordered]@{ path=$path; exists=$exists }
    if ($exists) {
        $entry = Get-Item -LiteralPath $path -Force
        Require (-not $entry.LinkType) "Symbolic link file: $path"
        $value.bytes = $entry.Length
        $value.sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    return $value
}
function Verify($expected) {
    $actual = Identity $expected.path
    Require ($actual.exists -and $actual.bytes -eq $expected.bytes -and $actual.sha256 -eq $expected.sha256) "Identity differs: $($expected.path)"
}
function Json([string]$path, $value) {
    Require ($plan.evidence.exactAllowedFilePaths -contains $path) "Unlisted evidence: $path"
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes(($value | ConvertTo-Json -Depth 80) + "`n")
    $maximum = if ($Mode -in @('QaBuild','DemoBuild') -and $path.EndsWith('/build-report.json')) { 16777216 }
        elseif ($path -match '/(host-)?io-(before|after)\.json$') { $plan.evidence.ioInventoryMaxBytes } else { $plan.evidence.jsonMaxBytes }
    Require ($bytes.Length -le $maximum) "JSON budget: $path"
    [IO.File]::WriteAllBytes((Join-Path $project $path), $bytes)
}
function Same-Files($a, $b, [string]$label) {
    Require ($a.Count -eq $b.Count) "$label count"
    $map = @{}
    foreach ($row in $b) { $map[$row.path] = $row }
    foreach ($row in $a) {
        $other = $map[$row.path]
        Require ($null -ne $other -and $row.exists -eq $other.exists -and $row.bytes -eq $other.bytes -and $row.sha256 -eq $other.sha256) "$label changed: $($row.path)"
    }
}
function Packet([int]$number, [string]$marker, [int]$length, [string]$hash) {
    $document = [IO.File]::ReadAllText('docs/system-design/2026-09-17/system-task-packets.md')
    $pattern = '(?ms)^## ' + $number + '\..*?^<!-- ' + $marker + ' -->$'
    $found = [regex]::Match($document, $pattern)
    $bytes = [Text.Encoding]::UTF8.GetBytes($found.Value + "`n")
    $actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    Require ($found.Success -and $bytes.Length -eq $length -and $actual -eq $hash) "Frozen section $number"
}
function Inventory([string]$relativeRoot) {
    $physical = Join-Path $project $relativeRoot
    $rows = [Collections.Generic.List[object]]::new()
    $cases = @()
    if ([IO.Directory]::Exists($physical)) {
        Require (-not (Get-Item -LiteralPath $physical).LinkType) "I/O root link: $relativeRoot"
        $cases = @(Get-ChildItem -LiteralPath $physical -Force | Sort-Object Name)
        foreach ($case in $cases) { Require ($case.PSIsContainer -and -not $case.LinkType -and $case.Name -cmatch '^[0-9a-f]{32}$') "Invalid I/O case: $($case.Name)" }
        $pending = [Collections.Generic.Stack[string]]::new()
        $pending.Push($physical)
        while ($pending.Count -gt 0) {
            foreach ($item in Get-ChildItem -LiteralPath $pending.Pop() -Force) {
                $row = [ordered]@{ relativePath=[IO.Path]::GetRelativePath($physical,$item.FullName) }
                if ($item.LinkType) { $row.type='symlink'; $row.target=$item.LinkTarget }
                elseif ($item.PSIsContainer) { $row.type='directory'; $pending.Push($item.FullName) }
                else {
                    $file = Identity $item.FullName
                    $row.type='file'; $row.bytes=$file.bytes; $row.sha256=$file.sha256
                }
                $rows.Add($row)
            }
        }
    }
    $entries = @($rows | Sort-Object relativePath -CaseSensitive)
    return [ordered]@{ capturedAtUtc=[DateTime]::UtcNow.ToString('O'); root=$relativeRoot; walkPolicy='No link traversal or cleanup';
        caseDirectoryCount=$cases.Count; caseDirectories=@($cases.Name); entries=$entries;
        filesAndLinks=@($entries | Where-Object type -ne 'directory').Count }
}
function Preserve-Inventory($before, $after) {
    $map = @{}
    foreach ($row in $after.entries) { $map[$row.relativePath] = $row }
    foreach ($row in $before.entries) {
        $other = $map[$row.relativePath]
        Require ($null -ne $other -and $row.type -eq $other.type -and $row.bytes -eq $other.bytes -and
            $row.sha256 -eq $other.sha256 -and $row.target -ceq $other.target) "Old I/O changed: $($before.root)/$($row.relativePath)"
    }
}
function Check-Io($original, $hostCases) {
    Require ($original.caseDirectoryCount -le $plan.inheritedAndHostIo.maxCaseDirectories -and
        $original.filesAndLinks -le $plan.inheritedAndHostIo.maxFilesAndLinks) 'Inherited I/O cap'
    Require ($hostCases.caseDirectoryCount -le $plan.inheritedAndHostIo.maxNewHostCases -and
        $hostCases.filesAndLinks -le $plan.inheritedAndHostIo.maxNewHostFilesAndLinks) 'Host I/O cap'
}
function Snapshot {
    $assets = @(Get-ChildItem -LiteralPath Assets -Force -Recurse | Where-Object { -not $_.PSIsContainer } | ForEach-Object {
        Identity ([IO.Path]::GetRelativePath($project,$_.FullName))
    } | Sort-Object path -CaseSensitive)
    $implementation = @($implementationPaths | ForEach-Object { Identity $_ } | Where-Object exists | Sort-Object path -CaseSensitive)
    $dll = @(Get-ChildItem -LiteralPath Library/ScriptAssemblies -File | Where-Object {
        $_.Name -match '^(FightMatch\..+|FlowPuzzle\..+|QFramework)\.(dll|pdb)$'
    } | ForEach-Object { Identity ([IO.Path]::GetRelativePath($project,$_.FullName)) } | Sort-Object path -CaseSensitive)
    $tools = @('Tools/Invoke-FM029Validation.ps1','Tools/FM029AndroidTestSettings.json','ProjectSettings/ProjectSettings.asset' | ForEach-Object { Identity $_ })
    $versionBytes = [Text.Encoding]::UTF8.GetBytes((@($assets + $implementation + $tools) | ConvertTo-Json -Depth 10 -Compress))
    return [ordered]@{ capturedAtUtc=[DateTime]::UtcNow.ToString('O'); stageId=$stage; authorTurnId=$authorTurn;
        target=$target; implementation=$implementation; assets=$assets; dll=$dll; tools=$tools;
        sourceFingerprint=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($versionBytes)).ToLowerInvariant() }
}
function Protected {
    foreach ($row in $plan.protectedTrackedFiles) {
        if ($plan.coordinatorOnlyMutableMetadata -notcontains $row.path) { Verify $row }
    }
    foreach ($row in $rootIdentity.oldEvidenceFiles) { Verify $row }
    foreach ($row in $plan.frozenGoldenInputs) { Verify $row }
    foreach ($root in $plan.oldEvidenceRoots) {
        $expected = @($rootIdentity.oldEvidenceFiles | Where-Object { $_.path.StartsWith($root + '/') })
        $actual = @(Get-ChildItem -LiteralPath $root -Recurse -File -Force)
        Require ($expected.Count -eq $actual.Count) "Old evidence tree differs: $root"
    }
}
function Check-Scope($snapshot, [bool]$complete) {
    foreach ($row in $assetBefore.files) { Verify $row }
    foreach ($row in $snapshot.assets) { Require ($allowedAssets -contains $row.path) "Unexpected asset: $($row.path)" }
    foreach ($rule in @($plan.assetsCreate) + @($plan.otherCreate)) {
        if (-not [IO.File]::Exists($rule.path)) { continue }
        if ($rule.maxLines) { Require ([IO.File]::ReadAllLines($rule.path).Length -le $rule.maxLines) "Line budget: $($rule.path)" }
        if ($rule.maxBytes) { Require ((Get-Item -LiteralPath $rule.path).Length -le $rule.maxBytes) "Byte budget: $($rule.path)" }
    }
    $settings = [IO.File]::ReadAllText('ProjectSettings/ProjectSettings.asset')
    $expected = $protectedBefore.projectSettingsOriginalText.Replace('  applicationIdentifier: {}', "  applicationIdentifier:`n    Android: com.yyczz1.fightmatch")
    $expected = $expected.Replace('  scriptingBackend: {}', "  scriptingBackend:`n    Android: 1")
    $expected = $expected.Replace('  AndroidTargetArchitectures: 1','  AndroidTargetArchitectures: 2')
    $expected = $expected.Replace('  AndroidTargetSdkVersion: 0','  AndroidTargetSdkVersion: 32')
    $expected = $expected.Replace("  managedStrippingLevel:`n", "  managedStrippingLevel:`n    Android: 4`n")
    Require ($settings -ceq $expected) 'Only the five approved permanent Android fields may differ'
    $guids = @($snapshot.assets | Where-Object { $_.path.EndsWith('.meta') } | ForEach-Object {
        $match = [regex]::Match([IO.File]::ReadAllText($_.path),'(?m)^guid: ([0-9a-f]{32})$')
        Require $match.Success "Missing GUID: $($_.path)"
        $match.Groups[1].Value
    })
    Require (@($guids | Select-Object -Unique).Count -eq $guids.Count) 'Duplicate asset GUID'
    if ($complete) {
        Require ($snapshot.assets.Count -eq 913 -and $snapshot.implementation.Count -eq 868 -and $guids.Count -eq 482) 'Complete Assets/implementation/GUID inventory'
    }
}
function Processes {
    $lines = @(& /bin/ps -axo pid=,ppid=,lstart=,command=)
    Require ($LASTEXITCODE -eq 0) 'Process inventory unavailable'
    return $lines
}
function Latest-Pass([string]$kind, [string]$fingerprint) {
    $matches = @($priorRuns | Where-Object { $_.run.mode -eq $kind -and $_.result.passed -and $_.after.sourceFingerprint -eq $fingerprint })
    Require ($matches.Count -gt 0) "A passed $kind on this source is required"
    return $matches[-1]
}
function Check-Apk([string]$path) {
    $archive = [IO.Compression.ZipFile]::OpenRead((Join-Path $project $path))
    $contents = [Collections.Generic.List[object]]::new()
    try {
        $published = @($plan.protectedTrackedFiles | Where-Object {
            $_.path.StartsWith('Assets/StreamingAssets/FightMatch/') -and -not $_.path.EndsWith('.meta')
        })
        Require ($published.Count -eq 6) 'Six published inputs must be bound by the frozen plan'
        foreach ($golden in $published) {
            $name = 'assets/FightMatch/' + [IO.Path]::GetFileName($golden.path)
            $entry = $archive.GetEntry($name)
            Require ($null -ne $entry -and $entry.Length -eq $golden.bytes) "APK published file: $name"
            $stream = $entry.Open()
            try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
            finally { $stream.Dispose() }
            Require ($hash -eq $golden.sha256) "APK content differs: $name"
            $contents.Add(@{ path=$name; bytes=$entry.Length; sha256=$hash })
        }
        Require ($contents.Count -eq 6) 'All six fixed StreamingAssets required in APK'
        if ($Mode -eq 'DemoBuild') {
            $metadata = @($archive.Entries | Where-Object { $_.FullName.EndsWith('/Metadata/global-metadata.dat') })
            Require ($metadata.Count -eq 1 -and $metadata[0].Length -le 67108864) 'One bounded IL2CPP metadata entry'
            $memory = [IO.MemoryStream]::new()
            $stream = $metadata[0].Open()
            try { $stream.CopyTo($memory); $text = [Text.Encoding]::UTF8.GetString($memory.ToArray()) }
            finally { $stream.Dispose(); $memory.Dispose() }
            foreach ($forbidden in @('FightMatch.Android.Tests','FightMatch.Host.Tests','FightMatch.Core.Tests','nunit.framework','UnityEngine.TestRunner')) {
                Require (-not $text.Contains($forbidden)) "Normal APK contains QA/test metadata: $forbidden"
            }
        }
    }
    finally { $archive.Dispose() }
    return @($contents)
}
function Backup-Tree([string]$root) {
    $data = [ordered]@{ root=$root; exists=[IO.Directory]::Exists($root); files=@(); directories=0; bytes=0; complete=$false; failure=$null }
    $files = [Collections.Generic.List[object]]::new()
    try {
        if ($data.exists) {
            Require (-not (Get-Item -LiteralPath $root -Force).LinkType) "Backup root link: $root"
            $pending = [Collections.Generic.Stack[string]]::new()
            $pending.Push((Join-Path $project $root))
            while ($pending.Count) {
                $data.directories++
                $items = @(Get-ChildItem -LiteralPath $pending.Pop() -Force)
                for ($offset=0; $offset -lt $items.Count; $offset+=64) {
                    $batch = @($items[$offset..[Math]::Min($offset+63,$items.Count-1)])
                    $types = @(& /usr/bin/stat -f '%HT' @($batch.FullName))
                    Require ($LASTEXITCODE -eq 0 -and $types.Count -eq $batch.Count) 'Backup lstat inventory'
                    for ($index=0; $index -lt $batch.Count; $index++) {
                        $item = $batch[$index]
                        $kind = if ($item.PSIsContainer) { 'Directory' } else { 'Regular File' }
                        Require (-not $item.LinkType -and $types[$index] -ceq $kind) "Backup special/link: $($item.FullName)"
                        if ($item.PSIsContainer) { $pending.Push($item.FullName); continue }
                        Require ($files.Count -lt 20000 -and $item.Length -le 1073741824 -and $data.bytes+$item.Length -le 2147483648) "Backup budget: $($item.FullName)"
                        $file = Identity $item.FullName
                        $file.path = [IO.Path]::GetRelativePath((Join-Path $project $root),$item.FullName)
                        Require (-not $file.path.StartsWith('../') -and -not [IO.Path]::IsPathRooted($file.path)) 'Backup path escape'
                        $files.Add($file)
                        $data.bytes += $file.bytes
                    }
                }
            }
        }
        $data.complete = $true
    } catch { $data.failure = $_.Exception.Message }
    $data.files = @($files | Sort-Object path -CaseSensitive)
    $data.fileCount = $files.Count
    return $data
}
function Build-Outputs([string]$slot) {
    $base = 'Builds/FMDemo029/mac-r1'
    $current = [ordered]@{ apk=$null; backup=$null; beforeAbsent=$true; mode=$Mode; argv=$unityArgs }
    $summary = [ordered]@{ current=$current; roots=0; files=0; bytes=0; failure=$null }
    try {
        foreach ($ancestor in @('Builds','Builds/FMDemo029',$base,"$base/$slot")) {
            if (Test-Path $ancestor) {
                $entry = Get-Item -LiteralPath $ancestor -Force
                Require ($entry.PSIsContainer -and -not $entry.LinkType) "Build ancestor type: $ancestor"
            }
        }
        if (Test-Path $apkPath) { Require ((& /usr/bin/stat -f '%HT' $apkPath) -ceq 'Regular File' -and (Get-Item $apkPath).Length -le 1073741824) 'APK type/budget' }
        $current.apk = Identity $apkPath
        $current.backup = Backup-Tree $backupPath
        Require $current.backup.complete 'Current backup inventory incomplete'
        foreach ($directory in @(Get-ChildItem -LiteralPath $base -Force -ErrorAction SilentlyContinue)) {
            $prior = @($priorRuns | Where-Object { $_.run.slot -eq $directory.Name })
            $kind = if ($directory.Name -eq $slot) { $Mode } elseif ($prior.Count -eq 1) { $prior[0].run.mode } else { '' }
            Require ($directory.PSIsContainer -and -not $directory.LinkType -and $kind -in @('QaBuild','DemoBuild')) "Unknown build slot: $($directory.Name)"
            $stem = if ($kind -eq 'QaBuild') { 'fightmatch-qa' } else { 'fightmatch-demo' }
            $root = "$base/$($directory.Name)/${stem}_BackUpThisFolder_ButDontShipItWithYourGame"
            foreach ($item in Get-ChildItem -LiteralPath $directory.FullName -Force) {
                Require (-not $item.LinkType -and ($item.Name -ceq "$stem.apk" -or $item.Name -ceq "${stem}_BackUpThisFolder_ButDontShipItWithYourGame")) "Unknown build sibling: $($item.FullName)"
                $type = & /usr/bin/stat -f '%HT' $item.FullName
                Require ($LASTEXITCODE -eq 0 -and $type -ceq $(if ($item.Name.EndsWith('.apk')) { 'Regular File' } else { 'Directory' })) 'Build output type'
            }
            $tree = if ($directory.Name -eq $slot) { $current.backup } else { Backup-Tree $root }
            Require $tree.complete "Incomplete backup inventory: $root"
            if ($tree.exists) { $summary.roots++; $summary.files+=$tree.fileCount; $summary.bytes+=$tree.bytes }
            if ($directory.Name -ne $slot) {
                $old = (Read-Json "$($prior[0].base)/build-report.json").boundedOutputs.current
                Require ($old.backup.exists -eq $tree.exists) "Prior backup existence: $root"
                Same-Files $old.backup.files $tree.files "Prior backup $root"
            }
        }
        foreach ($prior in $priorRuns | Where-Object { $_.run.mode -in @('QaBuild','DemoBuild') }) {
            $old = (Read-Json "$($prior.base)/build-report.json").boundedOutputs.current
            if ($old.apk.exists) { Verify $old.apk }
            Require ($old.backup.exists -eq [IO.Directory]::Exists($old.backup.root)) 'Prior backup root disappeared'
        }
        Require ($summary.roots -le 6 -and $summary.files -le 120000 -and $summary.bytes -le 12884901888) 'Aggregate backup budget'
    } catch { $summary.failure = $_.Exception.Message }
    return $summary
}
Require ([IO.Path]::GetFullPath($PSCommandPath) -ceq "$project/Tools/Invoke-FM029Validation.ps1") 'Canonical validation entry'
Require ($PSVersionTable.PSVersion.ToString() -eq '7.6.6') 'Fixed PowerShell version'
Require ((Identity $PSCommandPath).sha256 -eq $ExpectedScriptSha256) 'Validation script hash'
Require ((Identity $planPath).sha256 -eq 'cfe1ef8ae52ba4fa9daea475d3f4571d88cdbfe47b87ce89ba7e5a8eb59bd6f1') 'Fixed plan hash'
$plan = Read-Json $planPath
$rootIdentity = Read-Json "$evidence/root-identity.json"
Require ($rootIdentity.stageId -eq $stage -and $rootIdentity.authorTurnId -eq $authorTurn) 'Current author/root binding'
Require ((Identity "$evidence/root-identity.json").sha256 -eq 'd5e08e04d029f6cbe18e9125f71882f89506107bc85a45cb6e3923b15652c68c') 'Root identity with signed clarifications'
Packet 443 'DEMO-029-MAC-R1-PACKET-END' 11901 '0feff8b12727395fdd5fee0dd4c798ed9813a0e13c2b45867251b0547af081b4'
Packet 445 'DEMO-029-MAC-R1-QA-CLARIFICATION-END' 4733 'd1e14b3bd755c605b9f1e1d43166b6229b0a9ceeab9f5ad2b751ca79e9493c9c'
Packet 446 'DEMO-029-MAC-R1-GRAPHICS-CLARIFICATION-END' 1659 'c2feb6f1db9315e5d26cc39354980f884dd57c1850f8a8dd03a81b10ddc0b06a'
Packet 450 'DEMO-029-MAC-R1-RESOURCE-SUPPLEMENT-END' 2663 '6706fd5a2b8a7bf742b3e002ddcce250a14d55842f068e1e34a562438dbec712'
Packet 451 'DEMO-029-MAC-R1-COMPILE-SUPPLEMENT-END' 2043 'c0a957881149bca0b51fd3406b4ba3541ced242154c8495f4a640f56a51b83e4'
Packet 452 'DEMO-029-MAC-R1-RESOURCE-FIX-SUPPLEMENT-END' 4985 '0e681814f5fcc19d5efa43881d218841e4805ad67002a8a35930d6556bd28cfb'
Packet 453 'DEMO-029-MAC-R1-SCENE-SUPPLEMENT-END' 2609 '92d2d6a3bb6e1d0b42d56e075ea9a5049f3173aaa841d982c463a1f6d5c966ca'
Packet 454 'DEMO-029-MAC-R1-IL2CPP-CACHE-SUPPLEMENT-END' 3062 '87795266e318c1c940aa9d89aa8757dd35dd7cdc08fd411e366eddf2b028af88'
Packet 456 'DEMO-029-MAC-R1-PRIVATE-SIGNING-CACHE-END' 5166 'a9670f2b35470500c1795cf7f6c26693beb6db2e5415df6ead267688978b2ddd'
Packet 458 'DEMO-029-MAC-R1-HOST-FIX-SUPPLEMENT-END' 2922 'e512192508a765f6c8bb36d2dc61db5b24fa8c7248d8083e7db87a7fba70a906'
Require ((& git rev-parse HEAD) -eq $plan.baselineCommit) 'Git baseline changed'
foreach ($file in $plan.canonicalRuntime.exactFileIdentities) { Verify $file }
$assetBefore = Read-Json "$evidence/assets-before.json"
$protectedBefore = Read-Json "$evidence/protected-before.json"
$implementationBefore = Read-Json "$evidence/implementation-before.json"
$allowedAssets = @($assetBefore.files.path) + @($plan.assetsCreate.path)
$directoryMetas = @('Assets/Scripts/FightMatch/Host.meta','Assets/Scripts/FightMatch/Host/Editor.meta','Assets/Tests/Android.meta','Assets/Tests/EditMode/FightMatchHost.meta')
$implementationPaths = @($implementationBefore.files.path) + @($plan.assetsCreate.path | Where-Object { $_ -match '\.(cs|asmdef)(\.meta)?$' -or $directoryMetas -contains $_ })
$target = if ($Mode -in @('QaBuild','DemoBuild')) { 'android' } else { 'osxuniversal' }
$resourceLeaves = @('Assets/Scenes/FightMatchDemo.unity','Assets/UI/FightMatch/FightMatchPanelSettings.asset',
    'Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset','Assets/Scenes/FightMatchDemo.unity.meta',
    'Assets/UI/FightMatch/FightMatchPanelSettings.asset.meta','Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset.meta')
$missingAssets = @($plan.assetsCreate.path | Where-Object { -not [IO.File]::Exists($_) })
$preparationCompile = $Mode -eq 'Compile' -and $missingAssets.Count -gt 0
if ($preparationCompile) {
    Require (@($missingAssets | Where-Object { $resourceLeaves -notcontains $_ }).Count -eq 0) 'Only resource leaves/meta may be absent in preparation Compile'
}
$before = Snapshot
Protected
Check-Scope $before ($Mode -ne 'PrepareAssets' -and -not $preparationCompile)
$allProcesses = Processes
Require (@($allProcesses | Where-Object { $_ -match '/Unity\.app/Contents/MacOS/Unity(?: |$)' }).Count -eq 0) 'Another Unity Editor is active; serial validation only'
$priorRuns = @()
$slots = @(Get-ChildItem -LiteralPath "$evidence/runs" -Directory -ErrorAction SilentlyContinue | Sort-Object Name)
foreach ($slot in $slots) {
    Require ($slot.Name -match '^(00[1-9]|01[0-9]|020)$') 'Unexpected run slot'
    $base = "$evidence/runs/$($slot.Name)"
    $priorRuns += @{ run=(Read-Json "$base/run.json"); result=$(if (Test-Path "$base/result.json") { Read-Json "$base/result.json" });
        after=$(if (Test-Path "$base/after.json") { Read-Json "$base/after.json" }); base=$base }
}
$quotaNames = @{ PrepareAssets='ResourcePrepare'; FullTests='FinalFullEditMode'; DemoBuild='ReleaseBuild'; Compile='Compile'; HostTests='HostTests'; QaBuild='QaBuild' }
$modeQuota = if ($Mode -eq 'PrepareAssets') { 5 } elseif ($Mode -eq 'Compile') { 8 } else { $plan.boundedRuns[$quotaNames[$Mode]] }
Require ($slots.Count -lt 20 -and @($priorRuns | Where-Object { $_.run.mode -eq $Mode }).Count -lt $modeQuota) 'Bounded run quota exhausted'
Require (@($priorRuns | Where-Object { $_.run.mode -eq $Mode -and $_.result.passed -and $_.after.sourceFingerprint -eq $before.sourceFingerprint }).Count -eq 0) 'No repeat of a successful unchanged stage'
if ($Mode -eq 'PrepareAssets') {
    $compiled = Latest-Pass 'Compile' $before.sourceFingerprint
    Same-Files $compiled.after.dll $before.dll 'Preparation compile DLLs'
    $repairScene = 'Assets/Scenes/FightMatchDemo.unity'
    $repairBefore = Identity $repairScene
    Require ($repairBefore.bytes -eq 5513 -and $repairBefore.sha256 -eq '86872b3758377327dd3be4128a4343e35ceaafb60e7cd30178100417ea3d3808') 'Exact 009 scene binding repair only'
}
if ($Mode -in @('HostTests','FullTests')) {
    $compiled = Latest-Pass 'Compile' $before.sourceFingerprint
    Same-Files $compiled.after.dll $before.dll 'Mac compilation/test DLLs'
}
if ($Mode -eq 'FullTests') { $null = Latest-Pass 'HostTests' $before.sourceFingerprint }
if ($Mode -in @('QaBuild','DemoBuild')) { $null = Latest-Pass 'FullTests' $before.sourceFingerprint }
$ioBefore = Inventory $plan.inheritedAndHostIo.inheritedRoot
$hostIoBefore = Inventory $plan.inheritedAndHostIo.newHostRoot
foreach ($pair in @(@('io',$ioBefore),@('host-io',$hostIoBefore))) {
    $previous = if (Test-Path "$evidence/$($pair[0])-after.json") { "$evidence/$($pair[0])-after.json" } else { "$evidence/$($pair[0])-before.json" }
    Preserve-Inventory (Read-Json $previous) $pair[1]
}
Check-Io $ioBefore $hostIoBefore
if ($Mode -eq 'FullTests') {
    Require ($plan.inheritedAndHostIo.maxCaseDirectories - $ioBefore.caseDirectoryCount -ge 768 -and
        $plan.inheritedAndHostIo.maxFilesAndLinks - $ioBefore.filesAndLinks -ge 8192) 'Full regression I/O reserve'
}
$slotId = '{0:d3}' -f $(if ($slots.Count) { [int]$slots[-1].Name + 1 } else { 1 })
$runRoot = "$evidence/runs/$slotId"
Require (-not (Test-Path $runRoot)) 'Run directory must be new'
[IO.Directory]::CreateDirectory((Join-Path $project $runRoot)) | Out-Null
$unityArgs = @($plan.toolContract.commonUnityArgs) + @('-buildTarget',$target,'-logFile',"$project/$runRoot/unity.log") + @($plan.toolContract[$Mode])
if ($Mode -eq 'PrepareAssets') { $unityArgs += @('-fm029Stage',$stage,'-fm029Output',"$project/$runRoot/resource-preparation.json") }
if ($Mode -in @('HostTests','FullTests')) { $unityArgs += @('-testResults',"$project/$runRoot/tests.xml") }
$apkPath = $null
$backupPath = $null
if ($Mode -in @('QaBuild','DemoBuild')) {
    $leaf = if ($Mode -eq 'QaBuild') { 'fightmatch-qa.apk' } else { 'fightmatch-demo.apk' }
    $apkPath = "Builds/FMDemo029/mac-r1/$slotId/$leaf"
    Require ($plan.apkOutputs.path -contains $apkPath) 'APK whitelist'
    Require (-not (Test-Path $apkPath)) 'APK must be new'
    $backupPath = "Builds/FMDemo029/mac-r1/$slotId/" + [IO.Path]::GetFileNameWithoutExtension($leaf) + '_BackUpThisFolder_ButDontShipItWithYourGame'
    Require (-not (Test-Path $backupPath)) 'IL2CPP backup root must be absent'
    $unityArgs += @($(if ($Mode -eq 'QaBuild') { '-buildPlayerPath' } else { '-fm029Output' }), "$project/$apkPath")
    $outputsBefore = Build-Outputs $slotId
    Require (-not $outputsBefore.failure) 'Existing build outputs failed scope checks'
}
$run = [ordered]@{ stageId=$stage; authorTaskId=$plan.authorTaskId; authorTurnId=$authorTurn; hostId='local'; mode=$Mode;
    slot=$slotId; target=$target; executable=$plan.canonicalRuntime.unityExecutable; argv=$unityArgs;
    script=(Identity 'Tools/Invoke-FM029Validation.ps1'); plan=(Identity $planPath); rootIdentity=(Identity "$evidence/root-identity.json");
    createdAtUtc=[DateTime]::UtcNow.ToString('O'); beforeProcesses=$allProcesses; environment=$plan.environment.envEveryBuild;
    launchShell='/bin/sh'; launchPrefix=@('-c','umask 077; umask; exec "$@"','fm029'); inheritedUmask='077';
    inheritedCasesBefore=$ioBefore.caseDirectoryCount; hostCasesBefore=$hostIoBefore.caseDirectoryCount; status='Starting';
    preparationPrerequisiteCompile=$preparationCompile; missingApprovedAssets=$missingAssets;
    quotaOverride=@{ sections=@(450,451,452,453,458); ResourcePrepare=5; Compile=8; totalModeQuotas=24; totalRunLimit=20; directoryLimit=20 };
    resourceRepair=$(if ($Mode -eq 'PrepareAssets') { @{ authoritySection=453; before=$repairBefore; preserveAllExistingMetas=$true } }) }
Json "$runRoot/run.json" $run
Json "$runRoot/before.json" $before
$process = $null
$result = [ordered]@{ stageId=$stage; authorTurnId=$authorTurn; mode=$Mode; passed=$false }
try {
    $startInfo = [Diagnostics.ProcessStartInfo]::new('/bin/sh')
    foreach ($argument in @('-c','umask 077; umask; exec "$@"','fm029',$plan.canonicalRuntime.unityExecutable)) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.WorkingDirectory = $project
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in $unityArgs) { $startInfo.ArgumentList.Add([string]$argument) }
    foreach ($key in $plan.environment.envEveryBuild.Keys) { $startInfo.Environment[$key] = $plan.environment.envEveryBuild[$key] }
    $outStream = [IO.File]::Open("$project/$runRoot/stdout.txt",[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
    $errStream = [IO.File]::Open("$project/$runRoot/stderr.txt",[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
    $process = [Diagnostics.Process]::Start($startInfo)
    $outTask = $process.StandardOutput.BaseStream.CopyToAsync($outStream)
    $errTask = $process.StandardError.BaseStream.CopyToAsync($errStream)
    $run.unityPid = $process.Id
    $run.startedAtUtc = [DateTime]::UtcNow.ToString('O')
    $run.status = 'Running'
    Json "$runRoot/run.json" $run
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $diagnosed = $false
    $lastMinute = -1
    while (-not $process.WaitForExit(1000)) {
        $minute = [int][Math]::Floor($timer.Elapsed.TotalMinutes)
        if ($minute -ne $lastMinute) { Write-Output "FM029 $slotId $Mode running; PID $($process.Id); elapsed $minute minutes"; $lastMinute=$minute }
        if (-not $diagnosed -and $timer.Elapsed.TotalMinutes -ge $plan.toolContract.diagnosticAtMinutes[$Mode]) {
            $diagnosed = $true
            $run.diagnostic = @{ capturedAtUtc=[DateTime]::UtcNow.ToString('O'); processes=(Processes);
                logTail=@(Get-Content -LiteralPath "$runRoot/unity.log" -Tail 100);
                sample=@(& /usr/bin/sample $process.Id 1 1 -file /dev/stdout 2>&1 | ForEach-Object { $_.ToString() }) }
            Json "$runRoot/run.json" $run
            Write-Output 'One bounded read-only diagnostic captured; Unity remains running.'
        }
    }
    $null = $outTask.GetAwaiter().GetResult()
    $null = $errTask.GetAwaiter().GetResult()
    $outStream.Dispose()
    $errStream.Dispose()
    $run.finishedAtUtc = [DateTime]::UtcNow.ToString('O')
    $run.elapsedSeconds = $timer.Elapsed.TotalSeconds
    $run.exitCode = $process.ExitCode
    $run.status = 'Exited'
    if ($apkPath) {
        $outputs = Build-Outputs $slotId
        $report = if (Test-Path "$runRoot/build-report.json") { Read-Json "$runRoot/build-report.json" } else { @{ result='NotProduced'; actualBuildReport=$false } }
        $report.boundedOutputs = $outputs
        Json "$runRoot/build-report.json" $report
        Require (-not $outputs.failure) "Build output scope: $($outputs.failure)"
    }
    $after = Snapshot
    Json "$runRoot/after.json" $after
    Protected
    Check-Scope $after ($process.ExitCode -eq 0 -and -not $preparationCompile)
    foreach ($row in $before.assets) {
        if ($Mode -eq 'PrepareAssets' -and $row.path -ceq $repairScene) { continue }
        Verify $row
    }
    if ($Mode -ne 'PrepareAssets') { Same-Files $before.assets $after.assets 'Same-run assets'; Same-Files $before.implementation $after.implementation 'Same-run source' }
    Same-Files $before.tools $after.tools 'Same-run tools/settings'
    if ($Mode -in @('HostTests','FullTests')) { Same-Files $before.dll $after.dll 'Same-run Mac DLLs' }
    $ioAfter = Inventory $plan.inheritedAndHostIo.inheritedRoot
    $hostIoAfter = Inventory $plan.inheritedAndHostIo.newHostRoot
    Preserve-Inventory $ioBefore $ioAfter
    Preserve-Inventory $hostIoBefore $hostIoAfter
    Check-Io $ioAfter $hostIoAfter
    Json "$evidence/io-after.json" $ioAfter
    Json "$evidence/host-io-after.json" $hostIoAfter
    $run.inheritedCasesAdded = @($ioAfter.caseDirectories | Where-Object { $ioBefore.caseDirectories -notcontains $_ })
    $run.hostCasesAdded = @($hostIoAfter.caseDirectories | Where-Object { $hostIoBefore.caseDirectories -notcontains $_ })
    Require ($process.ExitCode -eq 0) "Unity exited $($process.ExitCode)"
    $log = [IO.File]::ReadAllText("$runRoot/unity.log")
    Require ($log -notmatch '(?m)error CS[0-9]+|Scripts have compiler errors|Aborting batchmode due to failure|Compilation failed') 'Unity compilation error'
    foreach ($leaf in @('unity.log','stdout.txt','stderr.txt')) { Require ((Get-Item "$runRoot/$leaf").Length -le $plan.evidence.logMaxBytes) "Log budget: $leaf" }
    if ($target -eq 'osxuniversal') { Require ($after.dll.Count -eq 42) 'Mac DLL count' } else { Require ($after.dll.Count -le 44) 'Android DLL bound' }
    if ($Mode -in @('HostTests','FullTests')) {
        [xml]$xml = [IO.File]::ReadAllText("$runRoot/tests.xml")
        $cases = @($xml.SelectNodes('//test-case'))
        Require ($xml.DocumentElement.GetAttribute('result') -eq 'Passed') 'Test run root must report Passed'
        Require ($cases.Count -gt 0 -and @($cases | Where-Object { $_.result -ne 'Passed' }).Count -eq 0) 'Every applicable test must pass, zero skipped'
        $result.tests = @{ total=$cases.Count; passed=$cases.Count; failed=0; skipped=0; xml=(Identity "$runRoot/tests.xml") }
        if ($Mode -eq 'FullTests') {
            $old = Read-Json "$evidence/named-tests-before.json"
            $counts = @{}
            foreach ($case in $cases) { $counts[[string]$case.fullname]++ }
            foreach ($case in $old.occurrences) { Require ($counts[$case.name] -gt 0) "Missing original occurrence: $($case.name)"; $counts[$case.name]-- }
            Json "$evidence/named-tests-after.json" @{ sourceXml=(Identity "$runRoot/tests.xml"); old4011Preserved=$true;
                occurrences=@($cases | ForEach-Object { @{ name=[string]$_.fullname; result=[string]$_.result } }); count=$cases.Count;
                androidQa='Excluded from this Mac target by UNITY_ANDROID; no Android execution claimed' }
        }
    }
    if ($apkPath) {
        $report = Read-Json "$runRoot/build-report.json"
        $temporary = Read-Json "$runRoot/temporary-settings.json"
        Require ($report.result -eq 'Succeeded' -and $report.output -ceq "$project/$apkPath" -and $temporary.restored) 'Actual build and restored settings required'
        Require ((Identity $apkPath).sha256 -eq $report.sha256 -and (Get-Item $apkPath).Length -le 1073741824) 'APK identity/budget'
        $result.apk = Identity $apkPath
        $result.publishedApkFiles = Check-Apk $apkPath
    }
    $result.passed = $true
}
catch {
    $result.failure = $_.Exception.Message
    Json "$runRoot/failure.json" @{ stageId=$stage; authorTurnId=$authorTurn; utc=[DateTime]::UtcNow.ToString('O');
        error=$_.Exception.Message; stack=$_.ScriptStackTrace; unityExited=($null -ne $process -and $process.HasExited) }
}
finally {
    $result.completedAtUtc = [DateTime]::UtcNow.ToString('O')
    Json "$runRoot/run.json" $run
    Json "$runRoot/result.json" $result
}
Write-Output ($result | ConvertTo-Json -Depth 12)
if (-not $result.passed) { exit 1 }
