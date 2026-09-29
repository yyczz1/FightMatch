[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][ValidateSet('Compile','Tests','Prepare','Publish','ActivateExport','VerifyRelease')][string]$Mode,
    [Parameter(Mandatory=$true)][string]$ExpectedScriptSha256,
    [ValidateSet('025-P2A','025-P2B-PREPARE','025-P2B-PREPARE-C1','025-P2B-PREPARE-C2','025-P2B-PUBLISH','025-P2C','CONT-A','CONT-B','CONT-B-CODE-C1','CONT-C-MAC-R1','DEMO-028-MAC')][string]$Stage = '025-P2A'
)
$ErrorActionPreference = 'Stop'
$project = 'D:/Unity/UnityProj/FightMatch'
$demo028 = $Stage -eq 'DEMO-028-MAC'; $contMac = $Stage -eq 'CONT-C-MAC-R1'; $mac = $contMac -or $demo028
if ($mac) { $project = '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch' }
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
if ($mac) {
    if (-not $IsMacOS -or $Mode -notin @('Compile','Tests')) { throw 'CONT-C-MAC-R1 requires macOS Compile/Tests.' }
    $evidence = 'TestArtifacts/FMDemoCONT/cont-c-mac-r1'
    if ($demo028) { $evidence = 'TestArtifacts/FMDemo028/mac-r1' }
    $executable = '/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity'
}
$scriptPath = 'Tools/Invoke-FM025P2Validation.ps1'
function Path-InProject([string]$path) {
    if ([string]::IsNullOrEmpty($path) -or $path -match '[\\:*?\x00]' -or $path.StartsWith('/') -or
        @($path.Split('/') | Where-Object { $_ -in @('', '.', '..') }).Count) { throw "Invalid relative path: $path" }
    $root = [IO.Path]::GetFullPath($project)
    $full = [IO.Path]::GetFullPath((Join-Path $root $path))
    $prefix = if ($mac) { $root+'/' } else { $root+'\' }
    $comparison = if ($mac) { [StringComparison]::Ordinal } else { [StringComparison]::OrdinalIgnoreCase }
    if (-not $full.StartsWith($prefix, $comparison)) { throw "Outside project: $path" }
    $at = $root
    foreach ($part in $path.Split('/')) {
        $at = Join-Path $at $part
        if ($mac) {
            $item=$null
            try { $item=Get-Item -LiteralPath $at -Force -ErrorAction Stop } catch [System.Management.Automation.ItemNotFoundException] { }
            if ($item.LinkType -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Linked path: $path" }
        } elseif ((Test-Path -LiteralPath $at) -and ((Get-Item -LiteralPath $at).Attributes -band [IO.FileAttributes]::ReparsePoint)) {
            throw "Reparse path: $path"
        }
    }
    return $full
}
function Identity([string]$path) {
    $full = Path-InProject $path
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { return [pscustomobject]@{path=$path; exists=$false} }
    $f = if ($mac) { Get-Item -LiteralPath $full -Force } else { Get-Item -LiteralPath $full }
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
if ($contMac) {
    $scopePath = 'docs/system-design/2026-09-17/demo-cont-c-design-c1-scope.json'
    if ((Identity $scopePath).sha256 -cne 'dd9a2557dff4db1bb652877f1f643201d1515c311f9f62ca106abf85f20d6cb7') { throw 'Mac C1 scope SHA mismatch' }
    $continuity = Get-Content -Raw -LiteralPath (Path-InProject $scopePath) | ConvertFrom-Json
    $sourcePaths = @($continuity.proposedImplementation.futureImplementationPaths)
    $assetPaths = @($continuity.proposedImplementation.futureAssetPaths)
    $protectedPaths = @($continuity.proposedImplementation.futureExportInputAndImplementationPaths) + @('.gitignore','.gitattributes','docs/system-design/2026-09-17/demo-cont-c-delivery.md','docs/system-design/2026-09-17/demo-cont-c-code-scope.json')
    $dllPaths = @($continuity.proposedImplementation.toolProposal.inherited36DllMetadata.path)
    $contentPaths = @($assetPaths | Where-Object { $_ -match '^Assets/(FightMatchContent|StreamingAssets)/' })
    $platformPlanPath = 'docs/system-design/2026-09-17/demo-cont-c-mac-platform-plan.json'
    $platformC1Path = 'docs/system-design/2026-09-17/demo-cont-c-mac-platform-c1-plan.json'
    if ((Identity $platformPlanPath).sha256 -cne '072a87610890fd8eca9d66be5803ce5875a19135adae5a24ab9d929f72b4a0d7' -or
        (Identity $platformC1Path).sha256 -cne '49651fb0a711c6e7821213635f502e420e10e233f5ab519c1547eb2a9f4bdbe7') { throw 'Mac platform plan identity mismatch' }
    $platformPlan = Get-Content -Raw -LiteralPath (Path-InProject $platformPlanPath) | ConvertFrom-Json
    $platformC1 = Get-Content -Raw -LiteralPath (Path-InProject $platformC1Path) | ConvertFrom-Json
    $sourcePaths += @($platformPlan.newAssets); $assetPaths += @($platformPlan.newAssets)
    if ($sourcePaths.Count -ne 808 -or $assetPaths.Count -ne 838 -or $protectedPaths.Count -ne 925 -or $dllPaths.Count -ne 36) { throw 'Mac finite membership mismatch' }
    if ((Identity 'Packages/manifest.json').sha256 -cne '8b35703a20ded99f05e847753312b1014a9a0666cc9e2c641ef87979d1596baf' -or
        (Identity 'Packages/packages-lock.json').sha256 -cne '25367af624aad0b94d396484422db4e74d19844da974598da51398a89ab356c0') { throw 'Mac package input changed' }
}
$rootIdentity = Get-Content -Raw -LiteralPath (Path-InProject ($evidence+'/root-identity.json')) | ConvertFrom-Json
if ($demo028) {
    $demoPlanPath = 'docs/system-design/2026-09-17/demo-028-mac-r1-plan.json'; $demoPlanIdentity = Identity $demoPlanPath
    if ($demoPlanIdentity.bytes -ne 205569 -or $demoPlanIdentity.sha256 -cne '037692abd5c262d7cd4efa40dcc93a2d74235fcd9f02c6c46e83ae5ea8725fe6') { throw '028 plan identity mismatch' }
    $demoPlan = Get-Content -Raw -LiteralPath (Path-InProject $demoPlanPath) | ConvertFrom-Json
    if ($rootIdentity.stageId -cne $Stage -or $rootIdentity.projectRoot -cne $project -or $rootIdentity.evidenceRoot -cne $evidence -or
        $rootIdentity.authorTaskId -cne '01a0e404-d89d-7ab2-bece-3cd1df3fbc52' -or $rootIdentity.authorTurnId -cne '01a0e7e1-94a9-76c3-89cc-3a00f910b2c7' -or
        $rootIdentity.authorTurnStartedAt -ne 1790596715 -or $rootIdentity.plan.sha256 -cne $demoPlanIdentity.sha256 -or
        (Identity ($evidence+'/root-identity.json')).sha256 -cne 'a1d036a804bd716dfebf769d54b10df7ed8a1b061089c527f89ec9389a7e4482') { throw '028 evidence owner mismatch' }
    $packetText = [IO.File]::ReadAllText((Path-InProject 'docs/system-design/2026-09-17/system-task-packets.md')).Replace("`r`n","`n")
    $packet = [regex]::Match($packetText,'(?ms)^## 434\..*?^<!-- DEMO-028-MAC-R1-PACKET-END -->$'); $sha = [Security.Cryptography.SHA256]::Create()
    try { $packetBytes=[Text.Encoding]::UTF8.GetBytes($packet.Value.TrimEnd("`n")+"`n"); $digest=[Convert]::ToHexString($sha.ComputeHash($packetBytes)).ToLowerInvariant() } finally { $sha.Dispose() }
    if (-not $packet.Success -or $packetBytes.Length -ne 13458 -or $digest -cne '08988b13282b1dafa4fcc0a095b66721a0a6bd98c79532c57d9543da1e11d06c' -or
        -not $packetText.Substring($packet.Index+$packet.Length).Contains('新准确作者turn'+$rootIdentity.authorTurnId+'，startedAt1790596715')) { throw '028 frozen packet/actual turn mapping mismatch' }
    if ($demoPlan.canonicalRuntime.unityExecutable -cne $executable -or $demoPlan.canonicalRuntime.powerShellExecutable -cne (Join-Path $PSHOME 'pwsh')) { throw '028 runtime path mismatch' }
    $sourcePaths=@($demoPlan.futureImplementationPaths); $assetPaths=@($demoPlan.futureAssetPaths); $dllPaths=@($demoPlan.dllPaths); $protectedPaths=@($demoPlan.protectedInputPaths)
    $contentPaths=@($assetPaths | Where-Object { $_ -match '^Assets/(FightMatchContent|StreamingAssets)/' }); $allowed=@($demoPlan.evidence.exactAllowedPaths)
    if ($sourcePaths.Count -ne 828 -or $assetPaths.Count -ne 858 -or $dllPaths.Count -ne 36 -or $protectedPaths.Count -ne 933 -or
        $allowed.Count -ne 128 -or @($allowed | Sort-Object -Unique -CaseSensitive).Count -ne 128 -or
        @(Compare-Object -CaseSensitive ($allowed | Sort-Object) ($rootIdentity.exactAllowedEvidencePaths | Sort-Object)).Count) { throw '028 finite membership mismatch' }
    foreach ($path in $allowed) { $null=Path-InProject $path }
    foreach ($frozen in @($rootIdentity.inputs)+@($demoPlan.frozenGoldenInputs)+@($demoPlan.inheritedIo.baselineInventory,$demoPlan.baseline.namedTestsXml)) {
        $actual=Identity $frozen.path; if ($actual.bytes -ne $frozen.bytes -or $actual.sha256 -cne $frozen.sha256) { throw ('028 frozen input changed: '+$frozen.path) }
    }
    function Mac-RuntimeInputs {
        if (@($demoPlan.canonicalRuntime.exactFileIdentities).Count -ne 6) { throw '028 runtime membership mismatch' }
        foreach ($frozen in $demoPlan.canonicalRuntime.exactFileIdentities) {
            $f=Get-Item -LiteralPath $frozen.path -Force -ErrorAction Stop; $digest=(Get-FileHash -LiteralPath $frozen.path -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($f.PSIsContainer -or $f.LinkType -or ($f.Attributes -band [IO.FileAttributes]::ReparsePoint) -or $f.Length -ne $frozen.bytes -or $digest -cne $frozen.sha256) { throw '028 runtime/resource changed' }
            [pscustomobject]@{path=$frozen.path; exists=$true; bytes=$f.Length; sha256=$digest; lastWriteUtc=$f.LastWriteTimeUtc.ToString('o')}
        }
    }
    function Demo-IO {
        $ioRoot=Path-InProject $demoPlan.inheritedIo.root; $baseline=Get-Content -Raw -LiteralPath (Path-InProject $demoPlan.inheritedIo.beforeInventory) | ConvertFrom-Json
        $rows=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal); $oldRows=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::Ordinal)
        foreach ($r in $baseline.entries) { $oldRows.Add($r.relativePath,$r) }
        $cases=@(Get-ChildItem -LiteralPath $ioRoot -Force); $files=0; $links=0; $dirs=0
        if ($cases.Count -gt 4096 -or @($cases | Where-Object { -not $_.PSIsContainer -or $_.LinkType -or $_.Name -cnotmatch '^[0-9a-f]{32}$' }).Count) { throw '028 invalid IO case root/capacity' }
        $queue=[Collections.Generic.Stack[string]]::new(); $queue.Push($ioRoot)
        while ($queue.Count) { foreach ($f in Get-ChildItem -LiteralPath $queue.Pop() -Force) {
            $relative=$f.FullName.Substring($ioRoot.Length+1); $row=[ordered]@{relativePath=$relative}
            if ($f.LinkType -or ($f.Attributes -band [IO.FileAttributes]::ReparsePoint)) { $links++; $row.type='symlink'; $row.target=[string]$f.LinkTarget }
            elseif ($f.PSIsContainer) { $dirs++; $row.type='directory'; $queue.Push($f.FullName) }
            else { $files++; $row.type='file'; $row.bytes=$f.Length; $row.sha256=(Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
            if ($files+$links -gt 40000) { throw '028 IO file/link capacity exceeded' }; $rows.Add($relative,[pscustomobject]$row)
        } }
        foreach ($r in $baseline.entries) {
            $actual=$rows[$r.relativePath]
            if ($null -eq $actual -or $actual.type -cne $r.type -or $actual.bytes -ne $r.bytes -or $actual.sha256 -cne $r.sha256 -or $actual.target -cne $r.target) { throw ('028 old IO entry changed: '+$r.relativePath) }
        }
        foreach ($path in $rows.Keys) { if ($path.Split('/')[0] -cin $baseline.caseDirectories -and -not $oldRows.ContainsKey($path)) { throw '028 entry added inside old case' } }
        if ($Mode -eq 'Tests' -and -not $process -and (4096-$cases.Count -lt 768 -or 40000-$files-$links -lt 8192)) { throw '028 insufficient full-test IO headroom' }
        return [pscustomobject]@{root=$demoPlan.inheritedIo.root; cases=$cases.Count; directories=$dirs; files=$files; symlinks=$links; originalEntriesUnchanged=$baseline.entries.Count; newEntries=$rows.Count-$oldRows.Count}
    }
    function Demo-CheckFiles($snapshot) {
        $mutable=@($demoPlan.modify.production.path)+@($scriptPath)+@($demoPlan.coordinatorOnlyMutableMetadata)
        foreach ($name in @('implementation','assets','inputs')) {
            $baseline=Get-Content -Raw -LiteralPath (Path-InProject ($evidence+'/'+$name+'-before.json')) | ConvertFrom-Json
            foreach ($oldFile in $baseline.files) { if ($oldFile.path -cin $mutable) { continue }; $actual=Identity $oldFile.path
                if ($actual.exists -ne $oldFile.exists -or $actual.bytes -ne $oldFile.bytes -or $actual.sha256 -cne $oldFile.sha256) { throw ('028 protected file changed: '+$oldFile.path) }
            }
        }
        $missing=@($snapshot.sourceFiles | Where-Object { -not $_.exists }); $newMeta=@($demoPlan.create.naturalMeta)
        if (@($missing | Where-Object { $Mode -ne 'Compile' -or $_.path -cnotin $newMeta }).Count) { throw '028 required implementation file missing' }
        $guids=@($snapshot.assetFiles | Where-Object { $_.exists -and $_.path.EndsWith('.meta') } | ForEach-Object {
            $text=[IO.File]::ReadAllText((Path-InProject $_.path)); $match=[regex]::Match($text,'(?m)^guid: ([0-9a-f]{32})\r?$')
            if (-not $match.Success) { throw '028 invalid GUID' }; $match.Groups[1].Value
        })
        if ($guids.Count -ne 451-$missing.Count -or @($guids | Sort-Object -Unique -CaseSensitive).Count -ne $guids.Count) { throw '028 GUID count/uniqueness mismatch' }
        return $guids.Count
    }
    $null=@(Mac-RuntimeInputs); $platformPlan=[pscustomobject]@{goldenInputs=$demoPlan.frozenGoldenInputs}
} elseif ($contMac) {
    if ($rootIdentity.stageId -cne $Stage -or $rootIdentity.rootProjectRelativePath -cne $evidence -or
        $rootIdentity.authorTaskId -cne '01a0e404-d89d-7ab2-bece-3cd1df3fbc52' -or $rootIdentity.authorTurnId -cne '01a0e410-3e6f-7400-b58c-34cebde8d28c' -or
        $rootIdentity.sourcePacketRangeSha256 -cne '7e472c5e1d554e8b15df50847e84aa3db993d43edec563d61ca10bae0ff34e71') { throw 'Mac evidence ownership mismatch' }
    $rcvPath = 'docs/system-design/2026-09-17/demo-cont-c-rcv1-code-scope.json'
    if ((Identity $rcvPath).sha256 -cne 'c75c1c37216754d04864d53b6f9a6d6a1df266aba79a425e4365d94550eb61a3') { throw 'RCV1 scope SHA mismatch' }
    $allowed = @((Get-Content -Raw -LiteralPath (Path-InProject $rcvPath) | ConvertFrom-Json).exactAllowedEvidencePaths | ForEach-Object { $_.Replace('cont-c-rcv1/','cont-c-mac-r1/') })
    $allowed += @('platform-environment.json','before-text/Assets/Scripts/FightMatch/Application/PlayerNavigationRecovery.cs.txt') | ForEach-Object { $evidence+'/'+$_ }
    $allowed += @('Models','Query','Session','Recovery') | ForEach-Object { $evidence+'/before-text/Assets/Scripts/FightMatch/Application/PlayerNavigation'+$_+'.cs.meta.txt' }
    foreach ($prefix in @('before-text','after-text')) { $allowed += @('manifest.json','packages-lock.json') | ForEach-Object { $evidence+'/'+$prefix+'/Packages/'+$_+'.txt' } }
    if ($allowed.Count -ne 156 -or @($rootIdentity.exactAllowedEvidencePaths).Count -ne 156 -or
        @(Compare-Object ($allowed | Sort-Object -Unique) ($rootIdentity.exactAllowedEvidencePaths | Sort-Object -Unique)).Count) { throw 'Mac evidence path mismatch' }
    $runExtensionPath = 'docs/system-design/2026-09-17/demo-cont-c-mac-r1-run-extension-plan.json'
    $runExtensionIdentity = Identity $runExtensionPath
    if ($runExtensionIdentity.bytes -ne 3862 -or $runExtensionIdentity.sha256 -cne
        'a4b02650313488f4b3679daa5c1d73a78e9dfc77d3c327da2489b5597db31b55') { throw 'Mac run extension plan identity mismatch' }
    $runExtension = Get-Content -Raw -LiteralPath (Path-InProject $runExtensionPath) | ConvertFrom-Json
    $extensionRootIdentity = Identity ($evidence+'/root-identity.json')
    if ($runExtension.status -cne 'APPROVED_AFTER_RUN008_TERMINAL' -or $runExtension.authoritySection -ne 417 -or
        $runExtension.stageId -cne $Stage -or $runExtension.projectRoot -cne $project -or $runExtension.evidenceRoot -cne $evidence -or
        $runExtension.authorTaskId -cne $rootIdentity.authorTaskId -or $runExtension.originalAuthorTurnId -cne $rootIdentity.authorTurnId -or
        $runExtension.currentResumedTurnId -cne '01a0e63c-ec16-7551-b71c-dee6bd67c93e' -or
        $runExtension.rootIdentity.path -cne $extensionRootIdentity.path -or
        $runExtension.rootIdentity.bytes -ne $extensionRootIdentity.bytes -or
        $runExtension.rootIdentity.sha256 -cne $extensionRootIdentity.sha256 -or
        $runExtension.originalAllowedEvidencePathCount -ne 156 -or $runExtension.effectiveAllowedEvidencePathCount -ne 180 -or
        $runExtension.effectiveManifestMaxRowsExcludingSelf -ne 179 -or $runExtension.maximumRunNumber -ne 10 -or
        $runExtension.newRunModes.'009' -cne 'Compile' -or $runExtension.newRunModes.'010' -cne 'Tests') { throw 'Mac run extension owner/scope mismatch' }
    $runTemplate = @($allowed | Where-Object { $_.StartsWith($evidence+'/runs/008/', [StringComparison]::Ordinal) })
    $additionalPaths = @('009','010' | ForEach-Object { $slot=$_; $runTemplate | ForEach-Object { $_.Replace('/runs/008/','/runs/'+$slot+'/') } })
    if ($runTemplate.Count -ne 12 -or @($runExtension.addedAllowedEvidencePaths).Count -ne 24 -or
        @(Compare-Object -CaseSensitive ($additionalPaths | Sort-Object -Unique -CaseSensitive) ($runExtension.addedAllowedEvidencePaths | Sort-Object -Unique -CaseSensitive)).Count) { throw 'Mac extension path mismatch' }
    $allowed += $additionalPaths
    if ($allowed.Count -ne 180 -or @($allowed | Sort-Object -Unique -CaseSensitive).Count -ne 180) { throw 'Mac effective evidence membership mismatch' }
    $runExtension2Path = 'docs/system-design/2026-09-17/demo-cont-c-mac-r1-run-extension2-plan.json'
    $runExtension2Identity = Identity $runExtension2Path
    if ($runExtension2Identity.bytes -ne 5232 -or $runExtension2Identity.sha256 -cne
        '34c9d372fe8da2c551de46d126e29326577e176d40bb01f7ce7d40952fbf34ed') { throw 'Mac run extension2 plan identity mismatch' }
    $runExtension2 = Get-Content -Raw -LiteralPath (Path-InProject $runExtension2Path) | ConvertFrom-Json
    if ($runExtension2.status -cne 'APPROVED_AFTER_RUN010_HOST_RESTART' -or $runExtension2.authoritySection -ne 425 -or
        $runExtension2.stageId -cne $Stage -or $runExtension2.projectRoot -cne $project -or $runExtension2.evidenceRoot -cne $evidence -or
        $runExtension2.authorTaskId -cne $rootIdentity.authorTaskId -or $runExtension2.originalAuthorTurnId -cne $rootIdentity.authorTurnId -or
        $runExtension2.currentResumedTurnId -cne '01a0e6d3-38f8-7b92-9c3c-6a3df6b258fe' -or
        $runExtension2.rootIdentity.sha256 -cne $extensionRootIdentity.sha256 -or
        $runExtension2.previousExtensionPlan.sha256 -cne $runExtensionIdentity.sha256 -or
        $runExtension2.originalAllowedEvidencePathCount -ne 156 -or $runExtension2.previousEffectiveAllowedEvidencePathCount -ne 180 -or
        $runExtension2.effectiveAllowedEvidencePathCount -ne 204 -or $runExtension2.effectiveManifestMaxRowsExcludingSelf -ne 203 -or
        $runExtension2.maximumRunNumber -ne 12 -or $runExtension2.newRunModes.'011' -cne 'Compile' -or
        $runExtension2.newRunModes.'012' -cne 'Tests') { throw 'Mac run extension2 owner/scope mismatch' }
    foreach ($frozen in @($runExtension2.interruptedRun.runDescriptor, $runExtension2.interruptedRun.failureEvidence)) {
        $actual=Identity $frozen.path
        if ($actual.bytes -ne $frozen.bytes -or $actual.sha256 -cne $frozen.sha256) { throw 'Interrupted run evidence changed' }
    }
    $additionalPaths2 = @('011','012' | ForEach-Object { $slot=$_; $runTemplate | ForEach-Object { $_.Replace('/runs/008/','/runs/'+$slot+'/') } })
    if (@($runExtension2.addedAllowedEvidencePaths).Count -ne 24 -or
        @(Compare-Object -CaseSensitive ($additionalPaths2 | Sort-Object -Unique -CaseSensitive) ($runExtension2.addedAllowedEvidencePaths | Sort-Object -Unique -CaseSensitive)).Count) { throw 'Mac extension2 path mismatch' }
    $allowed += $additionalPaths2
    if ($allowed.Count -ne 204 -or @($allowed | Sort-Object -Unique -CaseSensitive).Count -ne 204) { throw 'Mac extension2 effective membership mismatch' }
    $runExtension3Path = 'docs/system-design/2026-09-17/demo-cont-c-mac-r1-run-extension3-plan.json'
    $runExtension3Identity = Identity $runExtension3Path
    if ($runExtension3Identity.bytes -ne 10804 -or $runExtension3Identity.sha256 -cne '01baa41ea6420835a1ed830c80b0eac76d9bb6a1d25f00b0aa282158f60d6016') { throw 'Mac extension3 plan identity mismatch' }
    $runExtension3 = Get-Content -Raw -LiteralPath (Path-InProject $runExtension3Path) | ConvertFrom-Json
    if ($runExtension3.status -cne 'APPROVED_AFTER_RUN012_TOOL_DIRECTORY_RELOCATION' -or $runExtension3.authoritySection -ne 428 -or
        $runExtension3.stageId -cne $Stage -or $runExtension3.projectRoot -cne $project -or $runExtension3.evidenceRoot -cne $evidence -or
        $runExtension3.authorTaskId -cne $rootIdentity.authorTaskId -or $runExtension3.originalAuthorTurnId -cne $rootIdentity.authorTurnId -or
        $runExtension3.continuationReferenceTurnId -cne '01a0e6d3-38f8-7b92-9c3c-6a3df6b258fe' -or
        $runExtension3.rootIdentity.sha256 -cne $extensionRootIdentity.sha256 -or $runExtension3.originalAllowedEvidencePathCount -ne 156 -or
        $runExtension3.previousEffectiveAllowedEvidencePathCount -ne 204 -or $runExtension3.effectiveAllowedEvidencePathCount -ne 240 -or
        $runExtension3.effectiveManifestMaxRowsExcludingSelf -ne 239 -or $runExtension3.maximumRunNumber -ne 15 -or
        $runExtension3.newRunModes.'013' -cne 'Compile' -or $runExtension3.newRunModes.'014' -cne 'Tests' -or $runExtension3.newRunModes.'015' -cne 'Tests' -or
        $runExtension3.canonicalRuntime.unityExecutable -cne $executable -or $runExtension3.canonicalRuntime.powerShellExecutable -cne (Join-Path $PSHOME 'pwsh')) { throw 'Mac extension3 owner/runtime/scope mismatch' }
    $editorProbeFilter = 'FlowPuzzle.Tests.Editor.FlowCompletionUndoAndDropTests.ApplyCompletionResult_Solved_IsOneUndoableCommand'
    if ($runExtension3.newTestProfiles.'014'.testFilter -cne $editorProbeFilter -or $runExtension3.newTestProfiles.'014'.purpose -cne 'diagnostic-editor-resource-probe' -or
        $runExtension3.newTestProfiles.'014'.diagnosticOnly -ne $true -or $runExtension3.newTestProfiles.'014'.requiredTotal -ne 1 -or
        $runExtension3.newTestProfiles.'015'.purpose -cne 'full-unfiltered-acceptance' -or $runExtension3.newTestProfiles.'015'.diagnosticOnly -ne $false -or
        $null -ne $runExtension3.newTestProfiles.'015'.testFilter -or $runExtension3.newTestProfiles.'015'.requiresSuccessfulDiagnosticRun -cne '014') { throw 'Mac diagnostic profile mismatch' }
    foreach ($frozen in @($runExtension3.previousExtensionPlans) + @('runDescriptor','result','failure','log','stdout','stderr' | ForEach-Object { $runExtension3.failedRun012.$_ })) {
        $actual=Identity $frozen.path
        if ($actual.bytes -ne $frozen.bytes -or $actual.sha256 -cne $frozen.sha256) { throw 'Mac extension3 historical evidence changed' }
    }
    $additionalPaths3 = @('013','014','015' | ForEach-Object { $slot=$_; $runTemplate | ForEach-Object { $_.Replace('/runs/008/','/runs/'+$slot+'/') } })
    if (@($runExtension3.addedAllowedEvidencePaths).Count -ne 36 -or
        @(Compare-Object -CaseSensitive ($additionalPaths3 | Sort-Object -Unique -CaseSensitive) ($runExtension3.addedAllowedEvidencePaths | Sort-Object -Unique -CaseSensitive)).Count) { throw 'Mac extension3 path mismatch' }
    $allowed += $additionalPaths3
    if ($allowed.Count -ne 240 -or @($allowed | Sort-Object -Unique -CaseSensitive).Count -ne 240) { throw 'Mac extension3 effective membership mismatch' }
    function Mac-RuntimeInputs {
        if (@($runExtension3.canonicalRuntime.exactFileIdentities).Count -ne 6) { throw 'Mac runtime membership mismatch' }
        foreach ($frozen in $runExtension3.canonicalRuntime.exactFileIdentities) {
            $f=Get-Item -LiteralPath $frozen.path -Force -ErrorAction Stop
            if ($f.PSIsContainer -or $f.LinkType -or ($f.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Invalid Mac runtime file' }
            $digest=(Get-FileHash -LiteralPath $frozen.path -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($f.Length -ne $frozen.bytes -or $digest -cne $frozen.sha256) { throw 'Frozen Mac runtime/resource changed' }
            [pscustomobject]@{path=$frozen.path; exists=$true; bytes=$f.Length; sha256=$digest; lastWriteUtc=$f.LastWriteTimeUtc.ToString('o')}
        }
    }
    $null=@(Mac-RuntimeInputs)
    foreach ($path in $allowed) { $null = Path-InProject $path }
    $platformOwner = Get-Content -Raw -LiteralPath (Path-InProject ($platformPlan.platformRoot+'/root-identity.json')) | ConvertFrom-Json
    if ($platformOwner.authorTaskId -cne $rootIdentity.authorTaskId -or $platformOwner.authorTurnId -cne $rootIdentity.authorTurnId -or
        $platformOwner.sourcePacketRangeSha256 -cne '61dc54bd8949d5408a663371a13b9a6276aaf5f8d72bbfd73a55dc0301045f43' -or
        $platformOwner.c1PacketSha256 -cne '31574c112594376e40e603e35b379b5c46f6c7abb455790ef779d9ec89e533bc') { throw 'Mac platform owner mismatch' }
    foreach ($path in $platformC1.exactAllowedEvidencePaths) { $null=Path-InProject $path }
    $packetText = [IO.File]::ReadAllText((Path-InProject 'docs/system-design/2026-09-17/system-task-packets.md')).Replace("`r`n","`n")
    foreach ($binding in @(@(380,'CONT-C-MAC-R1','7e472c5e1d554e8b15df50847e84aa3db993d43edec563d61ca10bae0ff34e71'),
        @(383,'CONT-C-MAC-PLATFORM','61dc54bd8949d5408a663371a13b9a6276aaf5f8d72bbfd73a55dc0301045f43'),
        @(386,'CONT-C-MAC-PLATFORM-C1','31574c112594376e40e603e35b379b5c46f6c7abb455790ef779d9ec89e533bc'),
        @(417,'CONT-C-MAC-R1-RUN-EXT1','723b8c25a546b6fbcfff7a223694965c2d83a6a77747fc618a5487e914d2040e'),
        @(425,'CONT-C-MAC-R1-RUN-EXT2','b8e5f55095fc25b72d7e4a50ff9ee824da96ecb30d0e6f9b229a26efda5ae9d3'),
        @(428,'CONT-C-MAC-R1-RUN-EXT3','e22ee1c8b6f5af0dd0fba1b54ee05d7cb55c9dcacd052279d0e188e353ad5cb8'))) {
        $match=[regex]::Match($packetText,'(?ms)^## '+$binding[0]+'\..*?^<!-- '+$binding[1]+'-PACKET-END -->$')
        $sha=[Security.Cryptography.SHA256]::Create()
        try { $digest=[Convert]::ToHexString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($match.Value.TrimEnd("`n")+"`n"))).ToLowerInvariant() }
        finally { $sha.Dispose() }
        if (-not $match.Success -or $digest -cne $binding[2]) { throw 'Frozen Mac packet changed' }
    }
} elseif ($Stage -eq '025-P2A') {
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
    if ($mac) {
        $rows = & /bin/ps -axo 'pid=,comm='
        if ($LASTEXITCODE -ne 0 -or @($rows).Count -eq 0) { throw 'Mac process inspection failed; cannot authorize Unity launch.' }
        return @($rows | ForEach-Object {
            if ($_ -match '^\s*(\d+)\s+(.+)$') {
                $processId = [int]$Matches[1]; $path = $Matches[2].Trim()
                if ($path -eq 'Unity' -or $path.EndsWith('/Unity.app/Contents/MacOS/Unity', [StringComparison]::Ordinal)) {
                    $details = & /bin/ps -p $processId -o 'lstart=,args='
                    if ($LASTEXITCODE -ne 0) { throw 'Mac Unity process identity unavailable.' }
                    [pscustomobject]@{Id=$processId; Path=$path; StartAndArguments=($details -join "`n")}
                }
            }
        })
    }
    return @(Get-Process Unity -ErrorAction SilentlyContinue | Select-Object Id, Path, MainWindowTitle, StartTime)
}
function Capture {
    $snapshot = [ordered]@{capturedAtUtc=[DateTime]::UtcNow.ToString('o'); sourceFiles=@($sourcePaths | ForEach-Object { Identity $_ });
        script=(Identity $scriptPath); dllFiles=@($dllPaths | ForEach-Object { Identity $_ });
        contentFiles=@($contentPaths | ForEach-Object { Identity $_ }); assetFiles=@($assetPaths | ForEach-Object { Identity $_ });
        protectedFiles=@($protectedPaths | ForEach-Object { Identity $_ }); unityProcesses=@(Unity-Processes)}
    if ($Stage -eq '025-P2B-PUBLISH') { $snapshot.publicationFiles = @($storePaths | ForEach-Object { Identity $_ }) }
    if ($Stage -eq 'CONT-B-CODE-C1') { $snapshot.hubAndLicenseProcesses = @(Get-Process 'Unity Hub','Unity.Licensing.Client' -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, Path, StartTime) }
    if ($contMac) { $snapshot.platformInputs = @('Packages/manifest.json','Packages/packages-lock.json','START_HERE.md','.agent/PROJECT_CONTEXT.md',($evidence+'/platform-environment.json'),$platformPlanPath,$platformC1Path,$platformPlan.newTool,$runExtensionPath,$runExtension2Path,$runExtension3Path | ForEach-Object { Identity $_ }); $snapshot.runtimeFiles=@(Mac-RuntimeInputs) }
    if ($demo028) { $snapshot.platformInputs=@($demoPlanPath,($evidence+'/root-identity.json') | ForEach-Object { Identity $_ }); $snapshot.runtimeFiles=@(Mac-RuntimeInputs); $snapshot.guidCount=Demo-CheckFiles $snapshot; $snapshot.inheritedIo=Demo-IO }
    return $snapshot
}
function Same-Files($left, $right) {
    if (@($left).Count -ne @($right).Count) { return $false }
    for ($n=0; $n -lt @($left).Count; $n++) {
        if ($left[$n].path -cne $right[$n].path -or $left[$n].exists -ne $right[$n].exists -or
            $left[$n].bytes -ne $right[$n].bytes -or $left[$n].sha256 -cne $right[$n].sha256) { return $false }
    }
    return $true
}
$processes = @(Unity-Processes)
if ($processes.Count) { throw 'Unity is already running; no batch process was started.' }
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'Approved Unity executable is missing.' }
$runNumber = $null
$maxRuns = if ($Stage -in @('025-P2B-PREPARE-C1','025-P2B-PREPARE-C2','025-P2B-PUBLISH')) { 8 } else { 16 }
if ($Stage -in @('025-P2B-PUBLISH','025-P2C','CONT-A')) { $maxRuns = 12 }
if ($Stage -eq 'CONT-B') { $maxRuns = 13 }
if ($Stage -eq 'CONT-B-CODE-C1') { $maxRuns = 5 }
if ($mac) { $maxRuns = 15 }
if ($demo028) { $maxRuns = 6 }
for ($i=1; $i -le $maxRuns; $i++) {
    $candidate = '{0:d3}' -f $i
    if ($Stage -eq 'CONT-B-CODE-C1') { $candidate = @('001','004','005','002','003')[$i-1] }
    if (-not (Test-Path -LiteralPath (Path-InProject ($evidence+'/runs/'+$candidate)))) { $runNumber=$candidate; break }
}
if ($null -eq $runNumber) { throw "The authorized $maxRuns run slots are exhausted." }
if ($demo028) {
    $runDirs=@(Get-ChildItem -LiteralPath (Path-InProject ($evidence+'/runs')) -Directory -ErrorAction SilentlyContinue | Sort-Object Name)
    $previous=@($runDirs | Sort-Object { (Get-Content -Raw -LiteralPath (Join-Path $_.FullName 'run.json') | ConvertFrom-Json).startedAtUtc } | Select-Object -Last 1)
    if ($previous.Count) {
        $previousRoot=$evidence+'/runs/'+$previous[0].Name; $previousResult=Identity ($previousRoot+'/result.json'); $previousFailure=Identity ($previousRoot+'/failure.json')
        if (-not $previousResult.exists -and -not $previousFailure.exists) { throw '028 preceding run lacks a recorded terminal result' }
        $lastResult=if ($previousResult.exists) { Get-Content -Raw -LiteralPath (Path-InProject $previousResult.path) | ConvertFrom-Json } else { $null }
        $number=([int]$runDirs[-1].Name)+1
        if ($Mode -eq 'Tests' -and -not (Test-Path -LiteralPath (Path-InProject ($evidence+'/runs/002')))) { $number=2 }
        elseif ($number -eq 2) { $number=3 }
        if ($number -gt 6) { throw '028 six run slots exhausted' }; $runNumber='{0:d3}' -f $number
        if ($lastResult.passed -and $lastResult.kind -eq 'tests') {
            $priorAfter=Get-Content -Raw -LiteralPath (Path-InProject ($previousRoot+'/after.json')) | ConvertFrom-Json
            if ($priorAfter.script.sha256 -ceq $identity.sha256 -and (Same-Files $priorAfter.sourceFiles @($sourcePaths | ForEach-Object { Identity $_ }))) { throw '028 unchanged successful tests must not be repeated' }
        }
    }
}
if ($Stage -eq 'CONT-B-CODE-C1' -and (($runNumber -eq '002') -ne ($Mode -eq 'Compile'))) { throw 'CONT-B-CODE-C1 requires Tests, Compile, Tests in that order.' }
if ($mac -and $runNumber -eq '001' -and $Mode -ne 'Compile') { throw 'Mac source must compile before testing.' }
if ($mac -and $runNumber -in @('009','010') -and $Mode -cne $runExtension.newRunModes.$runNumber) { throw 'Mac extension requires 009 Compile then 010 Tests.' }
if ($mac -and $runNumber -in @('011','012') -and $Mode -cne $runExtension2.newRunModes.$runNumber) { throw 'Mac extension2 requires 011 Compile then 012 Tests.' }
if ($mac -and $runNumber -in @('013','014','015') -and $Mode -cne $runExtension3.newRunModes.$runNumber) { throw 'Mac extension3 requires 013 Compile, 014 probe, 015 full Tests.' }
$diagnosticOnly = $contMac -and $runNumber -eq '014'
$testFilter = if ($diagnosticOnly) { $editorProbeFilter } else { $null }
$purpose = if ($diagnosticOnly) { 'diagnostic-editor-resource-probe' } elseif ($Mode -eq 'Tests') { 'full-unfiltered-acceptance' } else { 'compile' }
if ($mac -and $Mode -eq 'Tests') {
    $compiled=$null
    $compileSearchStart=if ($demo028) { 6 } else { [int]$runNumber-1 }
    for ($i=$compileSearchStart; $i -ge 1; $i--) {
        $prior=$evidence+'/runs/'+('{0:d3}' -f $i)
        if (-not (Test-Path -LiteralPath (Path-InProject ($prior+'/result.json')))) { continue }
        $r=Get-Content -Raw -LiteralPath (Path-InProject ($prior+'/result.json')) | ConvertFrom-Json
        if ($r.kind -eq 'compile' -and $r.passed) { $compiled=Get-Content -Raw -LiteralPath (Path-InProject ($prior+'/after.json')) | ConvertFrom-Json; break }
    }
    $current=Capture
    if ($null -eq $compiled -or -not (Same-Files $compiled.sourceFiles $current.sourceFiles) -or
        -not (Same-Files $compiled.assetFiles $current.assetFiles) -or -not (Same-Files $compiled.dllFiles $current.dllFiles) -or
        $compiled.script.sha256 -cne $current.script.sha256) { throw 'Tests require unchanged successful Compile source, assets, tool and DLLs' }
    if ($contMac -and $runNumber -eq '015') {
        $probeRoot=$evidence+'/runs/014'; $probeResultPath=$probeRoot+'/result.json'
        $probe=Get-Content -Raw -LiteralPath (Path-InProject $probeResultPath) | ConvertFrom-Json
        $receipt=(Get-Content -Raw -LiteralPath (Path-InProject ($evidence+'/scope-audit.json')) | ConvertFrom-Json).continuationAfter012.diagnostic014Outcome
        [xml]$probeXml=Get-Content -Raw -LiteralPath (Path-InProject ($probeRoot+'/tests.xml')); $probeCases=@($probeXml.SelectNodes('//test-case'))
        $probeAfter=Get-Content -Raw -LiteralPath (Path-InProject ($probeRoot+'/after.json')) | ConvertFrom-Json
        if (-not $probe.passed -or $probe.actualExitCode -ne 0 -or $probe.diagnosticOnly -ne $true -or $probe.purpose -cne 'diagnostic-editor-resource-probe' -or
            $probe.testFilter -cne $editorProbeFilter -or $probeCases.Count -ne 1 -or $probeCases[0].fullname -cne $editorProbeFilter -or $probeCases[0].result -cne 'Passed' -or
            $probe.tests.identity.sha256 -cne (Identity ($probeRoot+'/tests.xml')).sha256 -or $null -eq $receipt -or $receipt.wrapperActualExitCode -ne 0 -or
            $receipt.resultIdentity.sha256 -cne (Identity $probeResultPath).sha256 -or $probeAfter.script.sha256 -cne $current.script.sha256 -or
            -not (Same-Files $probeAfter.sourceFiles $current.sourceFiles) -or -not (Same-Files $probeAfter.assetFiles $current.assetFiles) -or
            -not (Same-Files $probeAfter.dllFiles $current.dllFiles)) { throw 'Full Tests require the same-version successful exact diagnostic and actual wrapper receipt.' }
    }
    foreach ($input in $platformPlan.goldenInputs) {
        $actual=Identity $input.path
        if (-not $actual.exists -or $actual.bytes -ne $input.bytes -or $actual.sha256 -cne $input.sha256) { throw 'Frozen test input has not been restored' }
    }
    if ($demo028 -and -not (Same-Files $compiled.runtimeFiles $current.runtimeFiles)) { throw '028 Tests require the same six compiled runtime inputs' }
}
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
if ($diagnosticOnly) { $arguments += @('-testFilter',$editorProbeFilter) }
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
$startedAt = [DateTime]::UtcNow
$process = $null
try {
    $before = Capture
    Json-New ($runRoot+'/before.json') $before
    # Arguments are fixed above; commands are never loaded from evidence manifests.
    $launch = @{FilePath=$executable; ArgumentList=$arguments; PassThru=$true; RedirectStandardOutput=(Path-InProject $stdout); RedirectStandardError=(Path-InProject $stderr)}
    if (-not $mac) { $launch.WindowStyle = 'Hidden' }
    $process = Start-Process @launch
    $handle = $process.Handle
    $runRecord = [ordered]@{stageId=$Stage; run=$runNumber; kind=$kind;
        executable=$executable; arguments=$arguments; stdout=$stdout; stderr=$stderr; script=$identity;
        startedAtUtc=$startedAt.ToString('o'); processId=$process.Id; processStartUtc=$process.StartTime.ToUniversalTime().ToString('o')}
    if ($Stage -eq 'CONT-B') { $runRecord.authorTurnId = '01a0e09d-d68a-73c3-a25a-4ad32024a662' }
    if ($Stage -eq 'CONT-B-CODE-C1') { $runRecord.authorTurnId = '01a0e1bd-5b28-7bc0-bd42-f86b25c95fd9' }
    if ($mac) { $runRecord.authorTurnId = $rootIdentity.authorTurnId; $runRecord.authorTaskId = $rootIdentity.authorTaskId }
    if ($mac) { $runRecord.purpose=$purpose; $runRecord.diagnosticOnly=$diagnosticOnly; $runRecord.testFilter=$testFilter }
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
    if ($mac -and $Mode -eq 'Tests') { $passed = $passed -and (Same-Files $before.sourceFiles $after.sourceFiles) -and (Same-Files $before.assetFiles $after.assetFiles) -and (Same-Files $before.dllFiles $after.dllFiles) -and $before.script.sha256 -ceq $after.script.sha256 }
    if ($Mode -eq 'Tests') { $passed = $passed -and $null -ne $testSummary -and $testSummary.result -eq 'Passed' -and $testSummary.failed -eq 0 -and $testSummary.other -eq 0 }
    if ($diagnosticOnly) { $passed = $passed -and $cases.Count -eq 1 -and $cases[0].fullname -ceq $editorProbeFilter }
    if ($demo028) {
        $existing=@($before.assetFiles | Where-Object { $_.exists -and ($Mode -ne 'Compile' -or $_.path -cnotin $demoPlan.create.naturalMeta) })
        $afterExisting=@($after.assetFiles | Where-Object { $_.path -cin $existing.path })
        $passed=$passed -and $after.guidCount -eq 451 -and (Same-Files $before.runtimeFiles $after.runtimeFiles) -and (Same-Files $existing $afterExisting) -and $before.script.sha256 -ceq $after.script.sha256
        if ($Mode -eq 'Tests' -and $null -ne $testSummary) {
            [xml]$oldXml=Get-Content -Raw -LiteralPath (Path-InProject $demoPlan.baseline.namedTestsXml.path)
            $oldCases=@($oldXml.SelectNodes('//test-case')); $counts=[Collections.Generic.Dictionary[string,int]]::new([StringComparer]::Ordinal)
            foreach ($case in $cases) { if (-not $counts.ContainsKey($case.fullname)) { $counts[$case.fullname]=0 }; $counts[$case.fullname]++ }
            $missingOriginal=0
            foreach ($case in $oldCases) { if (-not $counts.ContainsKey($case.fullname) -or $counts[$case.fullname] -le 0) { $missingOriginal++ } else { $counts[$case.fullname]-- } }
            $passed=$passed -and $oldCases.Count -eq 3956 -and $missingOriginal -eq 0; $testSummary.originalOccurrences=3956; $testSummary.missingOriginalOccurrences=$missingOriginal; $testSummary.addedOccurrences=$cases.Count-3956
        }
    }
    $result = [ordered]@{kind=$kind; run=$runNumber; processId=$process.Id; actualExitCode=$exitCode; passed=$passed;
        startedAtUtc=$startedAt.ToString('o'); endedAtUtc=$endedAt.ToString('o'); durationSeconds=($endedAt-$startedAt).TotalSeconds;
        compilerErrors=$errors; tests=$testSummary; log=(Identity $log); stdout=(Identity $stdout); stderr=(Identity $stderr);
        scriptUnchanged=($before.script.sha256 -eq $after.script.sha256)}
    if ($mac) { $result.purpose=$purpose; $result.diagnosticOnly=$diagnosticOnly; $result.testFilter=$testFilter }
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
