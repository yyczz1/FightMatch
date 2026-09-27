[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][ValidateSet('Compile','Tests','Prepare','Publish','ActivateExport','VerifyRelease')][string]$Mode,
    [Parameter(Mandatory=$true)][string]$ExpectedScriptSha256,
    [ValidateSet('025-P2A','025-P2B-PREPARE','025-P2B-PREPARE-C1','025-P2B-PREPARE-C2','025-P2B-PUBLISH','025-P2C','CONT-A','CONT-B','CONT-B-CODE-C1')][string]$Stage = '025-P2A'
)
$ErrorActionPreference = 'Stop'
$project = 'D:/Unity/UnityProj/FightMatch'
$evidence = 'TestArtifacts/FMDemo025P2/p2a'
if ($Stage -eq '025-P2B-PREPARE') { $evidence = 'TestArtifacts/FMDemo025P2/p2b-prepare' }
if ($Stage -eq '025-P2B-PREPARE-C1') { $evidence = 'TestArtifacts/FMDemo025P2/p2b-prepare-c1' }
if ($Stage -eq '025-P2B-PREPARE-C2') { $evidence = 'TestArtifacts/FMDemo025P2/p2b-prepare-c2' }
if ($Stage -eq '025-P2B-PUBLISH') { $evidence = 'TestArtifacts/FMDemo025P2/p2b-publish' }
if ($Stage -eq '025-P2C') { $evidence = 'TestArtifacts/FMDemo025P2/p2c'; if ($Mode -notin @('Compile','Tests')) { throw 'P2C permits Compile/Tests only.' } }
if ($Stage -eq 'CONT-A') { $evidence = 'TestArtifacts/FMDemoCONT/cont-a'; if ($Mode -notin @('Compile','Tests')) { throw 'CONT-A permits Compile/Tests only.' } }
if ($Stage -eq 'CONT-B') {
    $evidence = 'TestArtifacts/FMDemoCONT/cont-b'
    if ($Mode -notin @('Compile','Tests')) { throw 'CONT-B permits Compile/Tests only.' }
}
if ($Stage -eq 'CONT-B-CODE-C1') { $evidence = 'TestArtifacts/FMDemoCONT/cont-b-code-c1'; if ($Mode -notin @('Compile','Tests')) { throw 'CONT-B-CODE-C1 permits Compile/Tests only.' } }
if ($Mode -in @('Publish','ActivateExport','VerifyRelease') -and $Stage -ne '025-P2B-PUBLISH') { throw 'Publication modes require P2B-PUBLISH.' }
if ($Mode -eq 'Prepare' -and $Stage -notin @('025-P2B-PREPARE','025-P2B-PREPARE-C2')) { throw 'Prepare is authorized only for P2B-PREPARE/C2.' }
$executable = 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe'
$scriptPath = 'Tools/Invoke-FM025P2Validation.ps1'
function Path-InProject([string]$path) {
    if ([string]::IsNullOrEmpty($path) -or $path -match '[\\:*?\x00]' -or $path.StartsWith('/') -or
        @($path.Split('/') | Where-Object { $_ -in @('', '.', '..') }).Count) { throw "Invalid relative path: $path" }
    $root = [IO.Path]::GetFullPath($project)
    $full = [IO.Path]::GetFullPath((Join-Path $root $path))
    if (-not $full.StartsWith($root+'\', [StringComparison]::OrdinalIgnoreCase)) { throw "Outside project: $path" }
    $at = $root
    foreach ($part in $path.Split('/')) {
        $at = Join-Path $at $part
        if ((Test-Path -LiteralPath $at) -and ((Get-Item -LiteralPath $at).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Reparse path: $path"
        }
    }
    return $full
}
function Identity([string]$path) {
    $full = Path-InProject $path
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { return [pscustomobject]@{path=$path; exists=$false} }
    $f = Get-Item -LiteralPath $full
    $stream = [IO.File]::Open($full, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
    $sha = [Security.Cryptography.SHA256]::Create()
    try { $digest = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose(); $stream.Dispose() }
    return [pscustomobject]@{path=$path; exists=$true; bytes=$f.Length;
        sha256=$digest; lastWriteUtc=$f.LastWriteTimeUtc.ToString('o')}
}
function Json-New([string]$path, $value) {
    $full = Path-InProject $path
    if (Test-Path -LiteralPath $full) { throw "Evidence must not be overwritten: $path" }
    [IO.File]::WriteAllText($full, ($value | ConvertTo-Json -Depth 40)+[Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}
$identity = Identity $scriptPath
if ($identity.sha256 -cne $ExpectedScriptSha256) { throw 'Script SHA mismatch' }
$old = Get-Content -Raw -LiteralPath (Path-InProject 'docs/system-design/2026-09-17/demo-025-p1-c1-scope.json') | ConvertFrom-Json
if ((Identity 'docs/system-design/2026-09-17/demo-025-p1-c1-scope.json').sha256 -ne 'f8ba3e2e25fa3d7d87fe1fd2c75fbddbc92c253d131b198fccb82ae34edbb677') { throw 'Baseline SHA mismatch' }
$design = Get-Content -Raw -LiteralPath (Path-InProject 'docs/system-design/2026-09-17/demo-product-continuity-c1-scope.json') | ConvertFrom-Json
if ((Identity 'docs/system-design/2026-09-17/demo-product-continuity-c1-scope.json').sha256 -ne 'da3eabc8fc96834bdcf3f61805afc2e606e3aa741b82cf5c97982d4b582ec058') { throw 'Design SHA mismatch' }
$part = $design.proposedImplementation.stages | Where-Object id -eq $Stage
$sourcePaths = @($old.implementation668.files.path) + @($part.create.production) + @($part.create.tests) + @($part.create.naturalMeta)
$protectedPaths = @($old.protectedInputs.after.files.path)
$dllPaths = @($old.dll.files.path)
$contentPaths = @()
$assetPaths = @()
if ($Stage -eq '025-P2B-PREPARE') {
    $priorPath = 'docs/system-design/2026-09-17/demo-025-p2a-scope.json'
    if ((Identity $priorPath).sha256 -ne '22b6ff2ac2d1261428461f837d8765490e4c21a9a48fde065a5bd00e379d5e70') { throw 'P2A scope SHA mismatch' }
    $prior = Get-Content -Raw -LiteralPath (Path-InProject $priorPath) | ConvertFrom-Json
    $sourcePaths = @($prior.implementation680.files.path) + @($part.create.production) + @($part.create.tests) + @($part.create.naturalMeta | Where-Object { $_ -match '^Assets/(Scripts|Tests)/' })
    $contentPaths = @($part.create.assets) + @($part.create.naturalMeta | Where-Object { $_ -notmatch '^Assets/(Scripts|Tests)/' })
    $assetPaths = @($prior.allAssets.files.path) + @($part.create.production) + @($part.create.tests) + @($part.create.assets) + @($part.create.naturalMeta)
    $protectedPaths = @($prior.protectedInputs.files.path)
    $dllPaths = @($prior.validation.dllCurrent.path)
}
if ($Stage -in @('025-P2B-PREPARE-C1','025-P2B-PREPARE-C2','025-P2B-PUBLISH')) {
    $priorPath = 'docs/system-design/2026-09-17/demo-025-p2b-prepare-scope.json'
    $priorSha = 'f616044fcded0082dd1610a673cb224b1633576723419a30799908a33a0386ae'
    if ($Stage -eq '025-P2B-PREPARE-C2') { $priorPath = 'docs/system-design/2026-09-17/demo-025-p2b-prepare-c1-scope.json'; $priorSha = 'e13fb09957fa081335a7c1b3fd9155c51f4f89078aa6b4cb93dc2d4f48b96e25' }
    if ($Stage -eq '025-P2B-PUBLISH') { $priorPath = 'docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-scope.json'; $priorSha = 'da2a19b47161f0cc7d9f2a5cff5ce96b93d8dcc67ce6f1c102f6fb1493993269' }
    if ((Identity $priorPath).sha256 -ne $priorSha) { throw 'P2B scope SHA mismatch' }
    $prior = Get-Content -Raw -LiteralPath (Path-InProject $priorPath) | ConvertFrom-Json
    $sourcePaths = @($prior.implementation700.files.path)
    $contentPaths = @('Assets/FightMatchContent/demo-r1.source.json','Assets/FightMatchContent/demo-r1.source.json.meta','Assets/FightMatchContent.meta')
    $assetPaths = @($prior.allAssets.files.path)
    $protectedPaths = @($prior.protectedInputs.files.path)
    $dllPaths = @($prior.validation.dllCurrent.path)
}
$storePaths = @()
if ($Stage -eq '025-P2B-PUBLISH') {
    $newCode = @('Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs','Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs')
    $newCodeMeta = @($newCode | ForEach-Object { $_+'.meta' })
    $releaseFiles = @('first-release.fmsource.json','first-release.fmpackage.bytes','first-release.fmvalidation.bytes','first-release.fmreview.json','first-release.fmpublish.json','first-release.fmrelease.json' | ForEach-Object { 'Assets/StreamingAssets/FightMatch/'+$_ })
    $releaseMeta = @('Assets/StreamingAssets.meta','Assets/StreamingAssets/FightMatch.meta') + @($releaseFiles | ForEach-Object { $_+'.meta' })
    $sourcePaths += $newCode + $newCodeMeta
    $assetPaths += $newCode + $newCodeMeta + $releaseFiles + $releaseMeta
    $contentPaths += $releaseFiles
    $planPath = $evidence+'/publication-plan.json'
    if ((Identity $planPath).sha256 -cne 'f6413092542b757dbc0c2b9ef81d8e268741b9a0ba2d367f3775dc31149af27c' -or
        (Identity 'docs/system-design/2026-09-17/demo-025-p2b-content-review.json').sha256 -cne '16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75') { throw 'Publication plan/review identity mismatch' }
    $plan = Get-Content -Raw -LiteralPath (Path-InProject $planPath) | ConvertFrom-Json
    $storePaths = @($evidence+'/publication-store/writer.lock') + @($plan.keys | ForEach-Object { $evidence+'/publication-store/'+$_.key+'.work'; $evidence+'/publication-store/'+$_.key+'.blob' })
}
if ($Stage -eq '025-P2C') {
    $priorPath = 'docs/system-design/2026-09-17/demo-025-p2b-publish-scope.json'
    if ((Identity $priorPath).sha256 -cne '692f345f1e1cc6697967ba648192bb399293d46706d94835c88aaa12f882492d') { throw 'PUBLISH scope SHA mismatch' }
    $prior = Get-Content -Raw -LiteralPath (Path-InProject $priorPath) | ConvertFrom-Json
    $newAssets = @($part.create.production) + @($part.create.tests) + @($part.create.naturalMeta)
    $sourcePaths = @($prior.implementation704.files.path) + $newAssets
    $assetPaths = @($prior.allAssets.files.path) + $newAssets
    $contentPaths = @('Assets/FightMatchContent/demo-r1.source.json','Assets/FightMatchContent/demo-r1.source.json.meta','Assets/FightMatchContent.meta') + @('first-release.fmsource.json','first-release.fmpackage.bytes','first-release.fmvalidation.bytes','first-release.fmreview.json','first-release.fmpublish.json','first-release.fmrelease.json' | ForEach-Object { 'Assets/StreamingAssets/FightMatch/'+$_ })
    $protectedPaths = @($prior.protectedInputs.files.path); $dllPaths = @($prior.validation.dllCurrent.path)
    if ($sourcePaths.Count -ne 722 -or $assetPaths.Count -ne 752 -or $dllPaths.Count -ne 36) { throw 'P2C finite input membership mismatch' }
}
if ($Stage -in @('CONT-A','CONT-B','CONT-B-CODE-C1')) {
    $scopePath = 'docs/system-design/2026-09-17/demo-cont-a-design-scope.json'
    $scopeSha = '2c9068d6e105ec789d2e6fb6476e9a000cb8652aa1fcc80b275f69e6aad67652'
    if ($Stage -in @('CONT-B','CONT-B-CODE-C1')) {
        $scopePath = 'docs/system-design/2026-09-17/demo-cont-b-design-c1-scope.json'
        $scopeSha = '40076c0a359215ea6b83a221c067460e49b6246d22cbd58df0ed8fdcb495ef12'
    }
    if ((Identity $scopePath).sha256 -cne $scopeSha) { throw 'Continuity design scope SHA mismatch' }
    $continuity = Get-Content -Raw -LiteralPath (Path-InProject $scopePath) | ConvertFrom-Json
    $sourcePaths = @($continuity.proposedImplementation.futureImplementationPaths)
    $assetPaths = @($continuity.proposedImplementation.futureAssetPaths)
    $protectedPaths = @($continuity.proposedImplementation.futureExportInputAndImplementationPaths)
    $dllPaths = @($continuity.proposedImplementation.toolProposal.inherited36DllMetadata.path)
    $contentPaths = @($assetPaths | Where-Object { $_ -match '^Assets/(FightMatchContent(/demo-r1.source.json(\.meta)?)?(\.meta)?|StreamingAssets/FightMatch/first-release\.[^.]+\.(json|bytes))$' })
    if ($Stage -eq 'CONT-A' -and ($sourcePaths.Count -ne 744 -or $assetPaths.Count -ne 774 -or $protectedPaths.Count -ne 838 -or $dllPaths.Count -ne 36)) { throw 'CONT-A finite input membership mismatch' }
    if ($Stage -in @('CONT-B','CONT-B-CODE-C1') -and ($sourcePaths.Count -ne 776 -or $assetPaths.Count -ne 806 -or $protectedPaths.Count -ne 880 -or $dllPaths.Count -ne 36)) { throw 'CONT-B finite input membership mismatch' }
}
if ($Stage -eq 'CONT-B-CODE-C1') { $protectedPaths += @('docs/system-design/2026-09-17/demo-cont-b-delivery.md','docs/system-design/2026-09-17/demo-cont-b-scope.json','docs/system-design/2026-09-17/demo-cont-b-code-review.md','TestArtifacts/FMDemoCONT/cont-b/root-identity.json','TestArtifacts/FMDemoCONT/cont-b/read-manifest.json') }
$rootIdentity = Get-Content -Raw -LiteralPath (Path-InProject ($evidence+'/root-identity.json')) | ConvertFrom-Json
if ($Stage -eq '025-P2A') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.relativeRoot -ne $evidence -or
        $rootIdentity.turnId -ne '01a0d207-3f3f-7801-a775-692b0b60d52d') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq '025-P2B-PREPARE-C1') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d3c8-8128-7510-a5b4-2a1a09a4f30c' -or
        $rootIdentity.sourcePacketRangeSha256 -ne 'db77b27ee9ec96581b66ca07b99c6c35100952d209f1c2f7daa2b63ebe2983b7') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq '025-P2B-PREPARE-C2') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d3fd-4b73-7102-b92c-a59b0d45d96a' -or
        $rootIdentity.sourcePacketRangeSha256 -ne '1f0e1639396e90d77851e092ee5f66a370b636c6cc01405803357934978fed9a') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq '025-P2B-PUBLISH') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d43a-d905-7780-a1aa-f1c979d9c751' -or
        $rootIdentity.sourcePacketRangeSha256 -ne '39a2c272b5c55f51e71bb681e9340f5a819f5937d747b7816f914d2c4779d7b5') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq '025-P2C') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d666-8097-7780-b1a6-e70fe6d71e7a' -or
        $rootIdentity.sourcePacketRangeSha256 -ne 'a0bfb5f579ccdc5d6d1c574af2e98179aa63b404210994db69dae9b3e3913655') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq 'CONT-A') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d75f-7c26-7d13-9eff-0f2a1c757f17' -or
        $rootIdentity.sourcePacketRangeSha256 -ne '4be90b50aac817e8949a96fe4aaeb30d82dda9ef2e042d07545e3da2d9c25917') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq 'CONT-B') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or
        $rootIdentity.authorTurnId -ne '01a0d8ff-a35c-7bd3-9df1-0d882cc88946' -or
        $rootIdentity.sourcePacketRangeSha256 -ne '0dbc27d8d828bff99ac32d4514ec9c531da94d5d35655b90d6a296a96f571bb1') { throw 'Evidence root ownership mismatch' }
} elseif ($Stage -eq 'CONT-B-CODE-C1') {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTaskId -ne '01a0c403-bfa1-7e90-b503-c0fcd61f23c1' -or $rootIdentity.authorTurnId -ne '01a0e11c-7272-7e22-a9a1-4b3fbb24235b' -or
        $rootIdentity.sourcePacketRangeSha256 -ne 'c46de13b841c8e82a40a42a0b93d7b8b4a72d0cedfbbd1e82d103380e9d0ffd9') { throw 'Evidence root ownership mismatch' }
} else {
    if ($rootIdentity.stageId -ne $Stage -or $rootIdentity.rootProjectRelativePath -ne $evidence -or
        $rootIdentity.authorTurnId -ne '01a0d274-f73b-78f1-9055-f770107acc4b') { throw 'Evidence root ownership mismatch' }
    $resume = Get-Content -Raw -LiteralPath (Path-InProject ($evidence+'/resume-001.json')) | ConvertFrom-Json
    if ((Identity ($evidence+'/resume-001.json')).sha256 -ne '1816b1a5c353655e5c0a2b3ad4bf314813a510a2911be5125fad6e8125c54cbb' -or
        $resume.resumedTurn -ne '01a0d338-e586-7973-9050-a705cf6a4789') { throw 'Resume ownership mismatch' }
}
function Unity-Processes {
    return @(Get-Process Unity -ErrorAction SilentlyContinue | Select-Object Id, Path, MainWindowTitle, StartTime)
}
function Capture {
    $snapshot = [ordered]@{capturedAtUtc=[DateTime]::UtcNow.ToString('o'); sourceFiles=@($sourcePaths | ForEach-Object { Identity $_ });
        script=(Identity $scriptPath); dllFiles=@($dllPaths | ForEach-Object { Identity $_ });
        contentFiles=@($contentPaths | ForEach-Object { Identity $_ }); assetFiles=@($assetPaths | ForEach-Object { Identity $_ });
        protectedFiles=@($protectedPaths | ForEach-Object { Identity $_ }); unityProcesses=@(Unity-Processes)}
    if ($Stage -eq '025-P2B-PUBLISH') { $snapshot.publicationFiles = @($storePaths | ForEach-Object { Identity $_ }) }
    if ($Stage -eq 'CONT-B-CODE-C1') { $snapshot.hubAndLicenseProcesses = @(Get-Process 'Unity Hub','Unity.Licensing.Client' -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, Path, StartTime) }
    return $snapshot
}
$processes = @(Unity-Processes)
if ($processes.Count) { throw 'Unity is already running; no batch process was started.' }
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Approved Unity executable is missing.' }
$runNumber = $null
$maxRuns = if ($Stage -in @('025-P2B-PREPARE-C1','025-P2B-PREPARE-C2','025-P2B-PUBLISH')) { 8 } else { 16 }
if ($Stage -in @('025-P2B-PUBLISH','025-P2C','CONT-A')) { $maxRuns = 12 }
if ($Stage -eq 'CONT-B') { $maxRuns = 13 }
if ($Stage -eq 'CONT-B-CODE-C1') { $maxRuns = 5 }
for ($i=1; $i -le $maxRuns; $i++) {
    $candidate = '{0:d3}' -f $i
    if ($Stage -eq 'CONT-B-CODE-C1') { $candidate = @('001','004','005','002','003')[$i-1] }
    if (-not (Test-Path -LiteralPath (Path-InProject ($evidence+'/runs/'+$candidate)))) { $runNumber=$candidate; break }
}
if ($null -eq $runNumber) { throw "The authorized $maxRuns run slots are exhausted." }
if ($Stage -eq 'CONT-B-CODE-C1' -and (($runNumber -eq '002') -ne ($Mode -eq 'Compile'))) { throw 'CONT-B-CODE-C1 requires Tests, Compile, Tests in that order.' }
$runRoot = $evidence+'/runs/'+$runNumber
[IO.Directory]::CreateDirectory((Path-InProject $runRoot)) | Out-Null
$kind = $Mode.ToLowerInvariant(); if ($Mode -eq 'ActivateExport') { $kind = 'activate-export' }; if ($Mode -eq 'VerifyRelease') { $kind = 'verify-release' }
$log = $runRoot+'/'+$kind+'.log'
$stdout = $runRoot+'/'+$kind+'.stdout.txt'
$stderr = $runRoot+'/'+$kind+'.stderr.txt'
$xmlPath = $runRoot+'/tests.xml'
$arguments = @('-batchmode','-nographics')
if ($Mode -in @('Compile','Prepare','Publish','ActivateExport','VerifyRelease')) { $arguments += '-quit' }
$arguments += @('-projectPath', $project)
if ($Mode -eq 'Tests') { $arguments += @('-runTests','-testPlatform','EditMode','-testResults', (Path-InProject $xmlPath)) }
if ($Mode -eq 'Prepare') {
    $authoring = $evidence+'/authoring'
    $names = @('source-copy.json','prepared.fmpackage.bytes','prepared.fmvalidation.bytes','prepared-descriptor.json','field-map.json','geometry.json','replay.json','review-template.json','result.json')
    foreach ($name in $names) { if (Test-Path -LiteralPath (Path-InProject ($authoring+'/'+$name))) { throw 'Authoring evidence exists; request precise new version paths before retry.' } }
    $arguments += @('-executeMethod','FightMatch.Content.PublishedContentAuthoring.Run','-fmMode','prepare',
        '-fmSource',(Path-InProject 'Assets/FightMatchContent/demo-r1.source.json'),'-fmEvidenceRoot',(Path-InProject $authoring))
}
if ($Mode -in @('Publish','ActivateExport','VerifyRelease')) {
    $arguments += @('-executeMethod','FightMatch.Content.PublishedContentAuthoring.Run','-fmMode',$kind,'-fmEvidenceRoot',(Path-InProject $evidence))
    if ($Mode -ne 'VerifyRelease') { $arguments += @('-fmStoreRoot',(Path-InProject ($evidence+'/publication-store')),'-fmOperationId','publish:fightmatch-demo-r1:1') }
    if ($Mode -eq 'Publish') { $arguments += @('-fmSource',(Path-InProject 'Assets/FightMatchContent/demo-r1.source.json'),'-fmReview',(Path-InProject 'docs/system-design/2026-09-17/demo-025-p2b-content-review.json')) }
    else { $arguments += @('-fmScope','player','-fmReleaseSetId','release-set:fightmatch-demo-r1','-fmReleaseRoot',(Path-InProject 'Assets/StreamingAssets/FightMatch')) }
}
$arguments += @('-logFile', (Path-InProject $log))
$before = Capture
Json-New ($runRoot+'/before.json') $before
$startedAt = [DateTime]::UtcNow
$process = $null
try {
    # Arguments are fixed above; commands are never loaded from evidence manifests.
    $process = Start-Process -FilePath $executable -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Path-InProject $stdout) -RedirectStandardError (Path-InProject $stderr)
    $handle = $process.Handle
    $runRecord = [ordered]@{stageId=$Stage; run=$runNumber; kind=$kind;
        executable=$executable; arguments=$arguments; stdout=$stdout; stderr=$stderr; script=$identity;
        startedAtUtc=$startedAt.ToString('o'); processId=$process.Id; processStartUtc=$process.StartTime.ToUniversalTime().ToString('o')}
    if ($Stage -eq 'CONT-B') { $runRecord.authorTurnId = '01a0e09d-d68a-73c3-a25a-4ad32024a662' }
    if ($Stage -eq 'CONT-B-CODE-C1') { $runRecord.authorTurnId = '01a0e1bd-5b28-7bc0-bd42-f86b25c95fd9' }
    Json-New ($runRoot+'/run.json') $runRecord
    $process.WaitForExit()
    $process.Refresh()
    $exitCode = $process.ExitCode
    $endedAt = [DateTime]::UtcNow
    $after = Capture
    Json-New ($runRoot+'/after.json') $after
    $errors = @()
    if (Test-Path -LiteralPath (Path-InProject $log)) {
        $errors = @(Select-String -LiteralPath (Path-InProject $log) -Pattern 'error CS|Compilation failed|Unhandled Exception' | ForEach-Object { $_.Line })
    }
    $testSummary = $null
    if ($Mode -eq 'Tests' -and (Test-Path -LiteralPath (Path-InProject $xmlPath))) {
        [xml]$xml = Get-Content -Raw -LiteralPath (Path-InProject $xmlPath)
        $cases = @($xml.SelectNodes('//test-case'))
        $testSummary = [ordered]@{result=$xml.'test-run'.result; total=$cases.Count;
            passed=@($cases | Where-Object result -eq 'Passed').Count;
            failed=@($cases | Where-Object result -eq 'Failed').Count;
            other=@($cases | Where-Object result -notin @('Passed','Failed')).Count; identity=(Identity $xmlPath)}
    }
    $passed = ($null -ne $exitCode -and $exitCode -eq 0 -and $errors.Count -eq 0)
    if ($Mode -eq 'Tests') { $passed = $passed -and $null -ne $testSummary -and $testSummary.result -eq 'Passed' -and $testSummary.failed -eq 0 -and $testSummary.other -eq 0 }
    $result = [ordered]@{kind=$kind; run=$runNumber; processId=$process.Id; actualExitCode=$exitCode; passed=$passed;
        startedAtUtc=$startedAt.ToString('o'); endedAtUtc=$endedAt.ToString('o'); durationSeconds=($endedAt-$startedAt).TotalSeconds;
        compilerErrors=$errors; tests=$testSummary; log=(Identity $log); stdout=(Identity $stdout); stderr=(Identity $stderr);
        scriptUnchanged=($before.script.sha256 -eq $after.script.sha256)}
    Json-New ($runRoot+'/result.json') $result
    if (-not $passed) { Json-New ($runRoot+'/failure.json') $result }
    $result | ConvertTo-Json -Depth 6
    if (-not $passed) { exit 1 }
}
catch {
    if (-not (Test-Path -LiteralPath (Path-InProject ($runRoot+'/failure.json')))) {
        Json-New ($runRoot+'/failure.json') ([ordered]@{kind=$kind; startedAtUtc=$startedAt.ToString('o');
            endedAtUtc=[DateTime]::UtcNow.ToString('o'); processId=$process.Id; actualExitCode=$exitCode; error=$_.Exception.Message; status='HarnessFailure'})
    }
    throw
}
