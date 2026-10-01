#!/opt/homebrew/bin/python3
"""Offline M13 material preparation, derived from frozen M12 helpers."""
from __future__ import annotations
import base64, copy, csv, datetime, difflib, hashlib, json, os, re, stat, sys, zipfile
import urllib.parse
import xml.etree.ElementTree as ET
from pathlib import Path, PurePosixPath
REPO=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
ROOT=Path('/private/tmp/fightmatch-loc-lic-alt-m13/loc-lic-alt-yamldotnet-16.3.0-m13')
E=REPO/'TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m13'
T12=Path('/private/tmp/fightmatch-loc-lic-alt-m12/loc-lic-alt-yamldotnet-16.3.0-m12')
E12=E.parent/'loc-lic-alt-yamldotnet-16.3.0-m12'
R13=E.parent/'xunit-license-research-13'
ACQ=ROOT/'acquisition'
DRIVER=ROOT/'preflight/m13_driver.py'
SOURCE=T12/'preflight/m12_driver.py'
M=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m13')
PACKAGES=[
 {'id':'System.Reflection.Metadata','version':'1.6.0','license':'MIT'},
 {'id':'System.Collections.Immutable','version':'1.5.0','license':'MIT'},
 {'id':'xunit.abstractions','version':'2.0.3','license':'Apache-2.0'}]
SENTINELS=('N/A_OFFICIAL_BINARY_PACKAGE','N/A_EMBEDDED_LICENSE_ENTRY')
XUNIT_SHA='d03d72fc2df8880448f7a81bddb00e1bcb5c18f323c7e7cc69b4cfa727469403'
NUSPEC_SHA='e59191df9e3047dd953b695c886597786ac2771f66897baa40877f643da9f7b1'
COMMIT='ffee51ac171a66896c42d00486bc792470ed6da2'
BODY_SHA='e9dcf94cc4864ba47aeaaec879cfabaf3feadb813c77af480c3ad14c6de0ed79'
LICENSE_URL='https://api.github.com/repos/xunit/xunit/contents/license.txt?ref='+COMMIT
PROJECT_URL='https://github.com/xunit/xunit'
LEGACY_URL='https://raw.githubusercontent.com/xunit/xunit/master/license.txt'
LANE='binary-package/official-version-license-snapshot'
BODY_REL='licenses/official/xunit.abstractions/2.0.3/license.txt'
VERSION_REL='identity/package-version/xunit.abstractions.2.0.3.json'
STARTED=datetime.datetime.now(datetime.timezone.utc)
class Blocked(RuntimeError): pass

def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def write_json(path: Path, value) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes((json.dumps(value, ensure_ascii=False, sort_keys=True, indent=2) + '\n').encode())

def read_jsonl(path: Path) -> list[dict]:
    if not path.exists():
        return []
    return [json.loads(x) for x in path.read_text(encoding='utf-8').splitlines() if x]

def parse_registration(data: dict, expected_id: str, expected_version: str) -> dict:
    if not isinstance(data, dict):
        raise Blocked('BLOCKED_REGISTRATION_SHAPE:not-object')
    entry = data.get('catalogEntry')
    if isinstance(entry, str):
        catalog_url = entry
        shape = 'string'
    elif isinstance(entry, dict):
        catalog_url = entry.get('@id')
        shape = 'object'
    else:
        raise Blocked(f'BLOCKED_REGISTRATION_SHAPE:catalogEntry:{type(entry).__name__}')
    if not isinstance(catalog_url, str) or not catalog_url:
        raise Blocked('BLOCKED_REGISTRATION_SHAPE:catalogEntry-empty')
    package_content = data.get('packageContent')
    if not isinstance(package_content, str) or not package_content:
        raise Blocked('BLOCKED_REGISTRATION_SHAPE:packageContent')
    lower = expected_id.lower()
    version = expected_version.lower()
    expected_reg = f'https://api.nuget.org/v3/registration5-semver1/{lower}/{version}.json'
    expected_pkg = f'https://api.nuget.org/v3-flatcontainer/{lower}/{version}/{lower}.{version}.nupkg'
    if data.get('@id') != expected_reg or package_content != expected_pkg:
        raise Blocked(f'BLOCKED_REGISTRATION_IDENTITY:{expected_id}')
    parsed = urllib.parse.urlsplit(catalog_url)
    if parsed.scheme != 'https' or parsed.hostname != 'api.nuget.org':
        raise Blocked(f'BLOCKED_CATALOG_URL:{catalog_url}')
    return {'id': expected_id, 'version': expected_version, 'shape': shape, 'catalogUrl': catalog_url, 'packageContent': package_content}

def elem_first(root, name: str):
    for elem in root.iter():
        if elem.tag.split('}')[-1] == name:
            return (elem.text or '').strip(), elem.attrib
    return '', {}

def parse_nuspec(path: Path) -> dict:
    with zipfile.ZipFile(path) as z:
        names = z.namelist()
        nuspecs = [x for x in names if x.lower().endswith('.nuspec')]
        if len(nuspecs) != 1:
            raise Blocked(f'BLOCKED_NUSPEC_COUNT:{path.name}:{len(nuspecs)}')
        root = ET.fromstring(z.read(nuspecs[0]))
        package_id, _ = elem_first(root, 'id')
        version, _ = elem_first(root, 'version')
        license_text, license_attrs = elem_first(root, 'license')
        license_url, _ = elem_first(root, 'licenseUrl')
        _, repo_attrs = elem_first(root, 'repository')
        groups = []
        for elem in root.iter():
            if elem.tag.split('}')[-1] == 'group':
                deps = [{'id': c.attrib.get('id', ''), 'version': c.attrib.get('version', '')} for c in elem if c.tag.split('}')[-1] == 'dependency']
                groups.append({'targetFramework': elem.attrib.get('targetFramework', ''), 'dependencies': deps})
        return {
            'id': package_id, 'version': version,
            'licenseExpression': license_text if license_attrs.get('type') == 'expression' else '',
            'licenseUrl': license_url,
            'repository': {'type': repo_attrs.get('type', ''), 'url': repo_attrs.get('url', ''), 'commit': repo_attrs.get('commit', '')},
            'dependencyGroups': groups, 'entries': names, 'nuspecPath': nuspecs[0],
        }

def require(ok, cause):
    if not ok: raise Blocked(cause)

def identity(p):
    return {'bytes': p.stat().st_size, 'sha256': sha256(p.read_bytes())}

def fixed(p, digest, size=None):
    require(p.is_file() and not p.is_symlink(), f'BLOCKED_INPUT_FILE:{p}')
    actual = identity(p)
    require(actual['sha256'] == digest and (size is None or actual['bytes'] == size), f'BLOCKED_INPUT_IDENTITY:{p}')
    return actual

def json_once(p, obj):
    require(not p.exists(), f'BLOCKED_ALREADY_WRITTEN:{p}')
    write_json(p, obj)

def tsv_once(p, header, rows):
    require(not p.exists(), f'BLOCKED_ALREADY_WRITTEN:{p}')
    p.parent.mkdir(parents=True, exist_ok=True)
    with p.open('x', encoding='utf-8', newline='') as f:
        w=csv.writer(f, delimiter='\t', lineterminator='\n'); w.writerow(header); w.writerows(rows)

def regular_files(root):
    if not root.exists(): return []
    result=[]
    for p in sorted(root.rglob('*'), key=lambda p: p.relative_to(root).as_posix().encode()):
        require(not p.is_symlink(), f'BLOCKED_SYMLINK:{p}')
        if p.is_file(): result.append(p)
    return result

def aggregate(root):
    files=regular_files(root); rows=[(p.relative_to(root).as_posix(), *identity(p).values()) for p in files]
    return {'exists':root.exists(), 'files':len(rows), 'bytes':sum(x[1] for x in rows),
            'sha256':sha256(''.join(f'{a}\t{b}\t{c}\n' for a,b,c in rows).encode())}

def classify_license(raw):
    s=' '.join(raw.decode('utf-8-sig',errors='strict').lower().split())
    mit=('permission is hereby granted, free of charge, to any person obtaining a copy',
         'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell',
         'the above copyright notice and this permission notice shall be included',
         'the software is provided "as is", without warranty of any kind',
         'in no event shall the authors or copyright holders be liable')
    apache=('apache license','version 2.0, january 2004','terms and conditions for use, reproduction, and distribution',
            '1. definitions.','2. grant of copyright license.','3. grant of patent license.',
            '4. redistribution.','5. submission of contributions.','6. trademarks.',
            '7. disclaimer of warranty.','8. limitation of liability.','9. accepting warranty or additional liability.',
            'end of terms and conditions')
    if all(x in s for x in mit): return 'MIT'
    if all(x in s for x in apache): return 'Apache-2.0'
    raise Blocked('BLOCKED_LICENSE_EVIDENCE:unclassified-complete-body')

def safe_entry(name):
    p=PurePosixPath(name)
    require(bool(name) and not p.is_absolute() and '..' not in p.parts and '\\' not in name and ':' not in name and '|' not in name, 'BLOCKED_ZIP_PATH:'+name)
    return p

def project_license(package, entry, expression):
    r=package
    parts=['binary-embedded','tests',r['id'],r['version'],r['registrationReceiptRelativePath'],
           r['nupkgUrl'],r['catalogUrl'],r['catalogReceiptRelativePath'],r['catalogSha512'],
           r['nupkgSha256'],entry['entryPath'],entry['sha256'],r['signatureReceiptRelativePath']]
    require(all(isinstance(x,str) and x and '|' not in x and x.isascii() for x in parts),'BLOCKED_RELATION_FORMAT')
    return {'component':r['id'],'version':r['version'],'sourceUrl':r['nupkgUrl'],
            'sourceTag':SENTINELS[0],'sourceCommit':SENTINELS[1],'sourcePath':entry['entryPath'],
            'localRelativePath':entry['localRelativePath'],'bytes':entry['bytes'],'sha256':entry['sha256'],
            'expression':expression,'packageRelationshipEvidence':'|'.join(parts),
            'packageSha256':r['nupkgSha256'],'evidenceLane':'binary-package/embedded-license',
            'signatureVerdict':'SIGNATURE_PENDING'}

def validate_projection(row, package, raw, stage):
    require(row['evidenceLane']=='binary-package/embedded-license','BLOCKED_SENTINEL_LANE')
    require((row['component'],row['version']) in {(p['id'],p['version']) for p in PACKAGES},'BLOCKED_SENTINEL_PACKAGE')
    require((row['sourceTag'],row['sourceCommit'])==SENTINELS,'BLOCKED_SENTINEL_VALUES')
    require(raw and len(raw)==row['bytes'] and sha256(raw)==row['sha256'],'BLOCKED_LICENSE_BODY')
    expected=project_license(package,{'entryPath':row['sourcePath'],'localRelativePath':row['localRelativePath'],
                                     'bytes':len(raw),'sha256':sha256(raw)},row['expression'])
    require(row==expected,'BLOCKED_RELATION_JOIN')
    require(package.get('signatureBytes',0)>0 and re.fullmatch('[0-9a-f]{64}',package.get('signatureSha256','')),'BLOCKED_SIGNATURE_ENTRY')
    if stage!='acquisition':
        raise Blocked('BLOCKED_SIGNATURE_RECEIPT:independent accepted signature required')

def load(p):
    return json.loads(p.read_text(encoding='utf-8'))

def safe_copy(src,dst):
    require(src.is_file() and not src.is_symlink() and not dst.exists(),'BLOCKED_COPY_SCOPE')
    dst.parent.mkdir(parents=True,exist_ok=True); dst.write_bytes(src.read_bytes())
    require(identity(src)==identity(dst),'BLOCKED_COPY_IDENTITY')

def read_manifest(p,key):
    with p.open(newline='') as f: rows=list(csv.DictReader(f,delimiter='\t'))
    require(rows and key in rows[0] and len({r[key] for r in rows})==len(rows),'BLOCKED_MANIFEST_SCHEMA')
    for r in rows: safe_entry(r[key])
    return rows

def check_manifest(root,manifest,key,subroot,exclude=()):
    rows=read_manifest(manifest,key)
    require({p.relative_to(root).as_posix() for p in regular_files(subroot)}==
            {r[key] for r in rows}|set(exclude),'BLOCKED_MANIFEST_SET:'+str(root))
    for r in rows: fixed(root/r[key],r['sha256'],int(r['bytes']))
    return rows

def preflight():
    require(not E.exists() and not M.exists(),'BLOCKED_ROOT_PRESENT')
    require({p.name for p in regular_files(ROOT)}=={'activation.json','m13_driver.py'},'BLOCKED_TEMP_ENTRY_SET')
    a=load(ROOT/'preflight/activation.json')
    require(a['reviewedHead'] is None and a['stopAt']=='WAITING_SIGNATURE_REVIEW','BLOCKED_ACTIVATION')
    require(a['entryPrecheck']['status']=='PASS' and all(a['entryPrecheck']['rootsAbsent'].values()),'BLOCKED_ENTRY_CHECK')
    packet=REPO/a['packet']['path']; fixed(packet,a['packet']['sha256'],a['packet']['bytes'])
    fixed(SOURCE,'6759dd14fc5dd77af385e0c3268a7e48dbdfd56e2b5ea4cc048bdb061de97b45',42263)
    fixed(E12/'result.json','b2eb0ed1f1fd59cb72557f1686a4bc7e07729609535c11af1dfeb19534370b21',23708)
    fixed(E12/'acquisition-files.tsv','ab57c50010e832f063d726c56cb3d9ea9b115a9de885945463bbfc7d2a818de5',6336)
    fixed(T12/'preflight/m12-inputs.tsv','703d54f52ceb3b5f8f1fa607f88d3f26e183fc5f3c8054d7a1fd7f5714b84ae1',34598)
    before={str(p):aggregate(p) for p in (T12,E12,R13)}
    require(before==a['entryPrecheck']['oldRoots'],'BLOCKED_OLD_ROOT_DRIFT')
    for key,p in [('researchReceipt',R13/'research-receipt.json'),('researchManifest',R13/'research-input-files.tsv'),
                  ('m12Preflight',T12/'preflight/preflight.json'),('m12Fixtures',T12/'preflight/fixtures.json')]:
        fixed(p,**{'digest':a['entryPrecheck'][key]['sha256'],'size':a['entryPrecheck'][key]['bytes']})
    acqrows=check_manifest(T12,E12/'acquisition-files.tsv','relativePath',T12/'acquisition')
    check_manifest(E12,E12/'evidence-files.tsv','relativePath',E12,('evidence-files.tsv',))
    rr=load(R13/'research-receipt.json')
    fixed(R13/'research-input-files.tsv',rr['payloadManifest']['sha256'],rr['payloadManifest']['bytes'])
    research=check_manifest(R13,R13/'research-input-files.tsv','path',R13,('research-input-files.tsv','research-receipt.json'))
    require(rr['requestCount']==12 and rr['fullResponseBytes']==393902,'BLOCKED_RESEARCH_LEDGER')
    E.mkdir(parents=True)
    json_once(E/'authority/activation.json',a)
    imports=[]
    for row in acqrows:
        rel=row['relativePath']
        require(rel.startswith('acquisition/'),'BLOCKED_IMPORT_SCOPE')
        safe_copy(T12/rel,ROOT/rel)
        imports.append(('M12 acquisition manifest',str(T12/rel),rel,row['bytes'],row['sha256']))
    for p in regular_files(R13):
        rel=p.relative_to(R13).as_posix(); target=ACQ/'research-13'/rel
        safe_copy(p,target)
        imports.append(('R13 receipt/manifest',str(p),target.relative_to(ROOT).as_posix(),p.stat().st_size,identity(p)['sha256']))
    # M12 has no imported-runner leaves; do not invent an import or rerun it.
    require(not (T12/'imported-runner').exists(),'BLOCKED_UNMANIFESTED_RUNNER')
    tsv_once(ROOT/'preflight/m13-inputs.tsv',['authority','sourcePath','targetRelativePath','bytes','sha256'],imports+
             [('M13 activation',str(packet),'',packet.stat().st_size,identity(packet)['sha256']),
              ('M12 derivation input',str(SOURCE),'',SOURCE.stat().st_size,identity(SOURCE)['sha256'])])
    diff=''.join(difflib.unified_diff(SOURCE.read_text().splitlines(True),DRIVER.read_text().splitlines(True),fromfile='m12_driver.py',tofile='m13_driver.py')).encode()
    pre={'status':'PASS','ownerTurn':a['ownerTurn'],'actualArgv':sys.argv,'oldRootsBefore':before,
         'sourceDriver':identity(SOURCE),'driverFrozen':identity(DRIVER),'derivationDiffSha256':sha256(diff),
         'derivationDiffBytes':len(diff),'inputs':identity(ROOT/'preflight/m13-inputs.tsv'),
         'importedAcquisitionLeaves':len(acqrows),'importedResearchLeaves':len(regular_files(R13)),
         'importedRunnerLeaves':0,'runnerReason':'M12 input manifest contains no imported-runner leaves; accepted M09 evidence retained upstream',
         'networkNew':0,'restoreCounts':[0,0,0],'researchReceipt':identity(R13/'research-receipt.json')}
    json_once(ROOT/'preflight/preflight.json',pre); json_once(E/'authority/preflight.json',pre)
    return pre

def validate_package(spec):
    directory=ACQ/'packages'/spec['id'].lower()/spec['version']
    p=load(directory/'package-evidence.json')
    require((p['id'],p['version'])==(spec['id'],spec['version']),'BLOCKED_PACKAGE_KEY')
    package=ROOT/p['nupkgRelativePath']; fixed(package,p['nupkgSha256'],p['nupkgBytes'])
    require(p['signatureVerdict']=='SIGNATURE_PENDING','BLOCKED_SIGNATURE_STATE')
    leaf=parse_registration(load(directory/'registration.json'),p['id'],p['version'])
    cat=load(directory/'catalog.json'); pkgraw=package.read_bytes()
    digest=base64.b64encode(hashlib.sha512(pkgraw).digest()).decode()
    require(leaf['packageContent']==p['nupkgUrl'] and leaf['catalogUrl']==p['catalogUrl'] and
            cat['id']==p['id'] and cat['version']==p['version'] and cat['packageHashAlgorithm']=='SHA512' and
            cat['packageHash']==p['catalogSha512']==digest,'BLOCKED_PACKAGE_RELATION')
    regreceipt=load(ACQ/p['registrationReceiptRelativePath']); catreceipt=load(ACQ/p['catalogReceiptRelativePath'])
    require(regreceipt['packageContent']==leaf['packageContent'] and regreceipt['catalogUrl']==leaf['catalogUrl'] and
            regreceipt['id']==p['id'] and regreceipt['version']==p['version'],'BLOCKED_REGISTRATION_RECEIPT')
    require(catreceipt['id']==p['id'] and catreceipt['version']==p['version'] and
            catreceipt['packageHash']==digest and catreceipt['packageHashAlgorithm']=='SHA512','BLOCKED_CATALOG_RECEIPT')
    rows=read_manifest(directory/'package-entries.tsv','entryPath'); by={r['entryPath']:r for r in rows}
    with zipfile.ZipFile(package) as z:
        names=[n for n in z.namelist() if not n.endswith('/')]
        require(len(names)==len(set(names)) and set(names)==set(by),'BLOCKED_PACKAGE_ENTRY_SET')
        for name in names:
            safe_entry(name); raw=z.read(name); row=by[name]
            require(len(raw)==int(row['bytes']) and sha256(raw)==row['sha256'],'BLOCKED_PACKAGE_ENTRY_HASH')
            extracted=directory/'entries'/name
            if extracted.exists(): require(extracted.read_bytes()==raw,'BLOCKED_EXTRACTED_BODY')
        sig=z.read('.signature.p7s')
        require(len(sig)==p['signatureBytes'] and sha256(sig)==p['signatureSha256'],'BLOCKED_SIGNATURE_ENTRY')
        nuspec=z.read(p['nuspec']['nuspecPath'])
        nr=ET.fromstring(nuspec)
        require(elem_first(nr,'id')[0]==p['id'] and elem_first(nr,'version')[0]==p['version'],'BLOCKED_NUSPEC')
        p.update(nuspecBytes=len(nuspec),nuspecSha256=sha256(nuspec),
                 projectUrl=elem_first(nr,'projectUrl')[0],licenseUrl=elem_first(nr,'licenseUrl')[0])
        if spec['id']!='xunit.abstractions':
            require(p['licenseStatus']=='EMBEDDED_BODY_VERIFIED_SIGNATURE_PENDING','BLOCKED_EMBEDDED_STATUS')
            expected_entries={n for n in names if PurePosixPath(n).name.lower().startswith(('license','copying','notice','third-party-notices','third_party_notices'))}
            require({r['sourcePath'] for r in p['licenses']}==expected_entries,'BLOCKED_NOTICE_COVERAGE')
            for row in p['licenses']:
                data=z.read(row['sourcePath']); validate_projection(row,p,data,'acquisition')
                if row['expression']!='NOTICE': require(classify_license(data)==spec['license']==row['expression'],'BLOCKED_EMBEDDED_CLASSIFICATION')
                dst=ACQ/row['localRelativePath']; safe_copy(directory/'entries'/row['sourcePath'],dst)
    p['nupkgLocalPath']=str(package); p['signatureLocalPath']=str(directory/'entries/.signature.p7s')
    p['importedEvidenceRelativePath']=(directory/'package-evidence.json').relative_to(ROOT).as_posix()
    return p,rows

def snapshot_context(package):
    require(package['id']=='xunit.abstractions' and package['version']=='2.0.3' and
            package['nupkgSha256']==XUNIT_SHA and package['nupkgBytes']==75155,'BLOCKED_SNAPSHOT_PACKAGE')
    require(package['nuspecSha256']==NUSPEC_SHA and package['nuspecBytes']==1385 and
            package['nuspec']['nuspecPath']=='xunit.abstractions.nuspec' and
            package['projectUrl']==PROJECT_URL and package['licenseUrl']==LEGACY_URL,'BLOCKED_SNAPSHOT_NUSPEC')
    r=ACQ/'research-13'; c3=load(r/'root-03.response.json'); c4=load(r/'root-04.response.json')
    commit=c3['result']['structuredContent']['commit']
    require(c3['args']=={'commit_sha':COMMIT,'repo_full_name':'xunit/xunit'} and commit['sha']==COMMIT and
            commit['repository_full_name']=='xunit/xunit' and commit['url']=='https://api.github.com/repos/xunit/xunit/commits/'+COMMIT,
            'BLOCKED_VERSION_COMMIT')
    files={f['filename']:f['patch'] for f in commit['files']}
    require(len(files)==len(commit['files']),'BLOCKED_VERSION_DIFF_DUPLICATE')
    for name,old,new in [
        ('src/xunit.core/xunit.core.csproj','<PackageReference Include="xunit.abstractions" Version="2.0.2" />','<PackageReference Include="xunit.abstractions" Version="2.0.3" />'),
        ('src/xunit.extensibility.core.nuspec','<dependency id="xunit.abstractions" version="2.0.2" />','<dependency id="xunit.abstractions" version="2.0.3" />')]:
        lines=files[name].splitlines()
        require(any(l.startswith('-') and old in l for l in lines) and any(l.startswith('+') and new in l for l in lines),'BLOCKED_VERSION_DIFF')
    content=c4['result']['structuredContent']
    require(c4['url']==LICENSE_URL and content['url']==LICENSE_URL and content['title']=='license.txt','BLOCKED_LICENSE_RESPONSE_URL')
    raw=(r/'xunit-xunit-ffee51ac-license.txt').read_bytes()
    require(content['content'].encode('utf-8')==raw and len(raw)==2357 and sha256(raw)==BODY_SHA,'BLOCKED_SNAPSHOT_BODY')
    require('Licensed under the Apache License, Version 2.0' in content['content'] and
            'Both sets of code are covered by the following license:' in content['content'] and
            'The MIT License (MIT)' in content['content'],'BLOCKED_SNAPSHOT_SCOPE')
    return {'sourceUrl':LICENSE_URL,'sourceCommit':COMMIT,'sourcePath':'license.txt','raw':raw,
            'versionResponse':identity(r/'root-03.response.json'),'licenseResponse':identity(r/'root-04.response.json'),
            'representation':load(r/'research-receipt.json')['representation']}

def snapshot_row(p,c):
    parts=['binary-official-snapshot','tests',p['id'],p['version'],p['registrationReceiptRelativePath'],
           p['nupkgUrl'],p['catalogUrl'],p['catalogReceiptRelativePath'],p['catalogSha512'],p['nupkgSha256'],
           p['nuspec']['nuspecPath'],p['nuspecSha256'],p['projectUrl'],p['licenseUrl'],
           c['sourceCommit'],c['sourcePath'],sha256(c['raw']),VERSION_REL,p['signatureReceiptRelativePath']]
    require(all(isinstance(v,str) and v and v.isascii() and '|' not in v for v in parts),'BLOCKED_SNAPSHOT_RELATION_FORMAT')
    for path in (p['registrationReceiptRelativePath'],p['catalogReceiptRelativePath'],VERSION_REL,p['signatureReceiptRelativePath']): safe_entry(path)
    return {'component':p['id'],'version':p['version'],'packageSha256':p['nupkgSha256'],
            'sourceUrl':c['sourceUrl'],'sourceCommit':c['sourceCommit'],'sourcePath':c['sourcePath'],
            'sourceTag':'N/A_LICENSE_SNAPSHOT_COMMIT','sourceCommitRole':'license-snapshot-not-package-build',
            'licenseEvidenceKind':'official-license-notice','expression':'Apache-2.0','evidenceLane':LANE,
            'localRelativePath':BODY_REL,'bytes':len(c['raw']),'sha256':sha256(c['raw']),
            'signatureVerdict':'SIGNATURE_PENDING','packageRelationshipEvidence':'|'.join(parts)}

def validate_snapshot(row,p,c,stage='acquisition'):
    # This exact tuple is the only exception; generic MIT/Apache full-body classifier is unchanged.
    require(p['id']=='xunit.abstractions' and p['version']=='2.0.3' and p['nupkgSha256']==XUNIT_SHA and
            p['nupkgBytes']==75155 and p['nuspecSha256']==NUSPEC_SHA and p['nuspecBytes']==1385 and
            p['projectUrl']==PROJECT_URL and p['licenseUrl']==LEGACY_URL,'BLOCKED_SNAPSHOT_TUPLE')
    require(c['sourceCommit']==COMMIT and c['sourcePath']=='license.txt' and c['sourceUrl']==LICENSE_URL and
            len(c['raw'])==2357 and sha256(c['raw'])==BODY_SHA,'BLOCKED_SNAPSHOT_FIXED_INPUT')
    require(row==snapshot_row(p,c),'BLOCKED_SNAPSHOT_PROJECTION')
    require(p['signatureBytes']>0 and p['signatureSha256']=='6690990d24e2b4e1edad729d1779d37d22f148add95b90858fa8b71263a056a5',
            'BLOCKED_SNAPSHOT_SIGNATURE_ENTRY')
    require(stage=='acquisition','BLOCKED_SIGNATURE_RECEIPT:independent verification not yet received')

def fixtures(p,c):
    row=snapshot_row(p,c); validate_snapshot(row,p,c)
    rejected=[]
    cases=[('body','context','raw',c['raw']+b' '),('package-hash','package','nupkgSha256','0'*64),
           ('version','package','version','2.0.2'),('commit','context','sourceCommit','0'*40),
           ('path','context','sourcePath','LICENSE'),('source-url','context','sourceUrl',LEGACY_URL),
           ('project-url','package','projectUrl','https://github.com/other/repo'),
           ('license-url','package','licenseUrl','https://example.invalid/license.txt'),
           ('relationship','row','packageRelationshipEvidence',''),
           ('mit-misclassification','row','expression','MIT'),
           ('source-rebuild-misuse','row','evidenceLane','source-rebuild'),
           ('embedded-tag-misuse','row','sourceTag',SENTINELS[0]),
           ('embedded-commit-misuse','row','sourceCommit',SENTINELS[1]),
           ('missing-signature-entry','package','signatureBytes',0)]
    for label,where,key,value in cases:
        pp,cc,rr=copy.deepcopy(p),copy.deepcopy(c),copy.deepcopy(row)
        {'package':pp,'context':cc,'row':rr}[where][key]=value
        try: validate_snapshot(rr,pp,cc)
        except Blocked: rejected.append(label)
        else: raise Blocked('BLOCKED_NEGATIVE_FIXTURE:'+label)
    try: validate_snapshot(row,p,c,stage='material')
    except Blocked: rejected.append('missing-independent-signature')
    else: raise Blocked('BLOCKED_NEGATIVE_FIXTURE:signature')
    # The MIT appendix is retained, but this explicit lane must classify the exact snapshot as Apache.
    require(row['expression']=='Apache-2.0' and b'The MIT License (MIT)' in c['raw'],'BLOCKED_MIT_APPENDIX')
    receipt={'status':'PASS','positiveSnapshot':'Apache-2.0/SIGNATURE_PENDING','negativeFixturesRejected':rejected,
             'driver':identity(DRIVER),'networkNew':0,'genericClassifierUnchanged':True,
             'apacheStandardNineSectionsArchived':False,'snapshotBytes':len(c['raw']),'snapshotSha256':sha256(c['raw'])}
    json_once(ROOT/'preflight/fixtures.json',receipt)
    return row

def project(packages,all_entries,row,context):
    p=next(p for p in packages if p['id']=='xunit.abstractions')
    safe_copy(ACQ/'research-13/xunit-xunit-ffee51ac-license.txt',ACQ/BODY_REL)
    p.update(licenses=[row],licenseStatus='OFFICIAL_SNAPSHOT_VERIFIED_SIGNATURE_PENDING',evidenceLane=LANE,
             sourceCommitRole='license-snapshot-not-package-build',licenseEvidenceKind='official-license-notice',
             sourceCommit=COMMIT,licenseBody={'relativePath':BODY_REL,**identity(ACQ/BODY_REL)},
             licenseApplicability='Central-approved inference for exact official binary; not build provenance',
             researchResponseIdentities={'version':context['versionResponse'],'license':context['licenseResponse']},
             researchSavedRepresentation=context['representation'],
             rawHttpBytesVerified=False,remoteGitBlobVerified=False,reproducibleBuildProven=False,
             apacheStandardNineSectionsArchived=False)
    p.pop('licenseBlocker',None)
    json_once(ACQ/VERSION_REL,{'packageId':p['id'],'version':p['version'],'packageSha256':p['nupkgSha256'],
              'nuspecSha256':p['nuspecSha256'],'projectUrl':p['projectUrl'],'licenseUrl':p['licenseUrl'],
              'commit':COMMIT,'sourceCommitRole':'license-snapshot-not-package-build',
              'versionDiffResponseRelativePath':'research-13/root-03.response.json','versionDiffResponse':context['versionResponse'],
              'licenseResponseRelativePath':'research-13/root-04.response.json','licenseResponse':context['licenseResponse'],
              'savedRepresentation':context['representation'],'licenseBodyRelativePath':BODY_REL,
              'licenseBody':identity(ACQ/BODY_REL),'inferenceAuthorized':True,'rawHttpBytesVerified':False,'remoteGitBlobVerified':False})
    all_rows=[]
    for p in packages:
        # Imported evidence stays byte-identical; active handoff uses rebased paths.
        for entry in p.get('notices',[]):
            entry['actualLocalPath']=str(ACQ/entry['localRelativePath'])
        json_once(ACQ/p['signatureReceiptRelativePath'],
                  {'status':'SIGNATURE_PENDING','packageId':p['id'],'version':p['version'],'nupkgBytes':p['nupkgBytes'],
                   'nupkgSha256':p['nupkgSha256'],'catalogSha512':p['catalogSha512'],
                   'signatureBytes':p['signatureBytes'],'signatureSha256':p['signatureSha256'],
                   'verificationExecuted':False,'acceptedSignatureReceipt':None})
        all_rows.extend(p['licenses'])
    package_keys={(p['id'],p['version'],p['nupkgSha256']) for p in packages}
    require(len(package_keys)==len(packages)==3,'BLOCKED_PACKAGE_JOIN')
    require({(r['component'],r['version'],r['packageSha256']) for r in all_rows}==package_keys,'BLOCKED_LICENSE_PACKAGE_JOIN')
    require({(r['packageId'],r['version'],r['packageSha256']) for r in all_entries}==package_keys,'BLOCKED_ENTRY_PACKAGE_JOIN')
    require(len({(r['component'],r['version'],r['packageSha256'],r['sourcePath']) for r in all_rows})==len(all_rows)==5,'BLOCKED_LICENSE_EXACT_ONCE')
    for r in all_rows: fixed(ACQ/r['localRelativePath'],r['sha256'],r['bytes'])
    columns=['component','version','sourceUrl','sourceTag','sourceCommit','sourceCommitRole','sourcePath','localRelativePath',
             'bytes','sha256','expression','licenseEvidenceKind','packageRelationshipEvidence','packageSha256','evidenceLane','signatureVerdict']
    tsv_once(ACQ/'identity/license-files.tsv',columns,[[r.get(c,'') for c in columns] for r in all_rows])
    tsv_once(ACQ/'identity/package-files.tsv',['packageId','version','bytes','sha256','sourceUrl'],
              [(p['id'],p['version'],p['nupkgBytes'],p['nupkgSha256'],p['nupkgUrl']) for p in packages])
    tsv_once(ACQ/'identity/package-entries.tsv',['packageId','version','packageSha256','entryPath','bytes','sha256'],
              [[r[c] for c in ['packageId','version','packageSha256','entryPath','bytes','sha256']] for r in all_entries])
    json_once(ACQ/'identity/sbom.json',{'schemaVersion':1,'status':'SIGNATURE_PENDING','components':packages,
              'joinKey':['packageId','version','packageSha256'],'exactlyOncePackageJoin':True,
              'licenseEntryCount':len(all_rows),'apacheStandardTerms':'Deferred to separately approved distribution-notice input'})

def finish(status,packages,pre,error=None):
    a=load(ROOT/'preflight/activation.json')
    before=a['entryPrecheck']['oldRoots']; after={str(p):aggregate(p) for p in (T12,E12,R13)}
    equality={'before':before,'after':after,'equal':before==after}
    if not equality['equal']: status='BLOCKED_OLD_ROOT_DRIFT'; error=error or status
    json_once(E/'old-root-equality.json',equality)
    original=load(E12/'network-ledgers.json')
    ledger={'m13NewRequests':0,'m13NewBytes':0,'sdkDownloads':0,'cloudRequestsThisStage':0,
            'macAcquisitionImported':{'source':str(E12/'network-ledgers.json'),'sourceIdentity':identity(E12/'network-ledgers.json'),
              'chainRequests':9,'chainBytes':1764095,'redirects':0,
              'ledgerRelativePath':'acquisition/combined-network-requests.jsonl','ledgerIdentity':identity(ACQ/'combined-network-requests.jsonl'),
              'newNetworkRequest':False},
            'researchImported':{'requests':12,'savedFullResponseBytes':393902,'httpWireBytes':None,'redirectCount':None,
              'ledgerRelativePath':'acquisition/research-13/research-receipt.json','newNetworkRequest':False,
              'representation':'Local serialized tool responses, not HTTP wire bytes'},
            'conditionalFutureCloud':{'maxFixedNupkgRequests':3,'maxBytes':5242880,'sdkDownloads':0},
            'acquisitionPlusConditionalCloudCap':{'requests':14,'bytes':10485760,'excludesIndependentResearch':True},
            'note':'No claim that total project requests are capped at 14; separate historical research used 12 tool calls.'}
    require(original['counts']['macChainRequests']==9 and original['counts']['macChainBytes']==1764095,'BLOCKED_OLD_NETWORK_COUNTS')
    json_once(E/'network-ledgers.json',ledger)
    tsv_once(E/'acquisition-files.tsv',['relativePath','bytes','sha256'],
              [(p.relative_to(ROOT).as_posix(),*identity(p).values()) for p in regular_files(ACQ)])
    tsv_once(E/'process/completed-commands.tsv',['phase','argv','status'],
              [('offline-material-preparation',' '.join(sys.argv),status)])
    tsv_once(E/'process/not-run-commands.tsv',['phase','count','reason'],
              [(p,0,'not authorized in M13 offline stage') for p in ('network','SDK download','signature verification','runner restore','tests generation restore','tests locked restore','build','test','Luban','Unity','Git','material','005','B','L')])
    temp=aggregate(ROOT); evidence=aggregate(E); elapsed=(datetime.datetime.now(datetime.timezone.utc)-STARTED).total_seconds()
    free={str(p):os.statvfs(p).f_bavail*os.statvfs(p).f_frsize for p in (Path('/private/tmp'),M.parent)}
    require(not M.exists() and not (M.parent/'5.1.0-fm-yamldotnet-16.3.0-m12').exists(),'BLOCKED_MATERIAL_PRESENT')
    require(temp['bytes']<=768*1024**2 and evidence['bytes']<=16*1024**2 and min(free.values())>=2*1024**3 and elapsed<=1800,'BLOCKED_BUDGET')
    result={'schemaVersion':1,'status':status,'ownerThread':a['ownerThread'],'ownerHost':'local','ownerTurn':a['ownerTurn'],
            'completionReceiver':a['completionReceiver'],'reviewedHead':None,'reviewStatus':'PENDING_SEPARATE_LOC_GITHUB_PR',
            'activation':identity(ROOT/'preflight/activation.json'),'driver':{'path':str(DRIVER),**identity(DRIVER)},
            'inputManifest':{'path':str(ROOT/'preflight/m13-inputs.tsv'),**identity(ROOT/'preflight/m13-inputs.tsv')},
            'packages':packages,'packageCount':len(packages),'signatureState':'SIGNATURE_PENDING',
            'licenseState':'COMPLETE_WITH_APPROVED_SCOPE' if not error else 'INCOMPLETE',
            'network':ledger,'restoreCounts':{'runner':0,'testsGeneration':0,'testsLocked':0},
            'materialAbsent':True,'oldRootsEqual':equality['equal'],'temp':temp,'evidenceBeforeFinalSeal':evidence,
            'freeBytesEnd':free,'executionWallSeconds':elapsed,'preparationStartAt':a['entryPrecheck']['checkedAt'],
            'actualWallSecondsSinceEntryCheck':(datetime.datetime.now(datetime.timezone.utc)-datetime.datetime.fromisoformat(a['entryPrecheck']['checkedAt'])).total_seconds(),
            'firstFailure':error,'limits':{'tempBytes':768*1024**2,'evidenceBytes':16*1024**2,'wallSeconds':1800},
            'rawHttpBytesVerified':False,'xunitRemoteGitBlobVerified':False,'xunitReproducibleBuildProven':False,
            'apacheStandardNineSectionsArchived':False,'apacheStandardTermsRequirement':'Separate later distribution notice archive; not supplied by the 2357-byte snapshot.'}
    require(result['actualWallSecondsSinceEntryCheck']<=1800,'BLOCKED_WALL_BUDGET')
    if error:
        json_once(E/'authority/first-failure.json',{'status':status,'error':error,'retryCount':0})
        (E/'FAILED_EVIDENCE').write_text(status+'\n')
    else:
        handoff={**result,'packageMaterialRoot':str(ACQ),'readiness':'All three exact packages and license relationships prepared; no signature verification performed.',
                 'licenseManifest':identity(ACQ/'identity/license-files.tsv'),'packageManifest':identity(ACQ/'identity/package-files.tsv'),
                 'packageEntryManifest':identity(ACQ/'identity/package-entries.tsv'),'sbom':identity(ACQ/'identity/sbom.json'),
                 'pendingSignatureReceiptMeaning':'Placeholders identify exact inputs only; require separate accepted Linux verification.'}
        json_once(E/'authority/signature-handoff.json',handoff)
    json_once(E/'result.json',result)
    tsv_once(E/'evidence-files.tsv',['relativePath','bytes','sha256'],
              [(p.relative_to(E).as_posix(),*identity(p).values()) for p in regular_files(E)])
    check_manifest(E,E/'evidence-files.tsv','relativePath',E,('evidence-files.tsv',))
    require(sum(p.stat().st_size for p in regular_files(E))<=16*1024**2,'BLOCKED_EVIDENCE_BUDGET')
    print(json.dumps({'status':status,'packages':len(packages),'newRequests':0,'newBytes':0,'oldRootsEqual':equality['equal'],
                      'evidence':str(E),'driver':str(DRIVER)}))

def main():
    require(len(sys.argv)==1,'BLOCKED_ARGUMENTS')
    packages=[]; pre={}
    try:
        pre=preflight()
        entries=[]
        for spec in PACKAGES:
            p,rs=validate_package(spec); packages.append(p)
            entries.extend({**r,'packageId':p['id'],'version':p['version'],'packageSha256':p['nupkgSha256']} for r in rs)
        # Reuse the sealed closure result; no restore/build/test is performed here.
        require(load(ACQ/'closure.json')['status']=='PASS','BLOCKED_CLOSURE_RECEIPT')
        xunit=next(p for p in packages if p['id']=='xunit.abstractions')
        context=snapshot_context(xunit); row=fixtures(xunit,context)
        require(identity(DRIVER)==pre['driverFrozen'],'BLOCKED_FROZEN_DRIVER')
        project(packages,entries,row,context)
        finish('WAITING_SIGNATURE_REVIEW',packages,pre)
    except Exception as exc:
        if not E.exists(): E.mkdir(parents=True)
        # Preserve a first failure; never rerun or mutate this driver after execution.
        if not (E/'result.json').exists():
            try: finish('BLOCKED_M13_OFFLINE_MATERIAL',packages,pre,f'{type(exc).__name__}:{exc}')
            except Exception as seal_error:
                if not (E/'authority/first-failure.json').exists():
                    json_once(E/'authority/first-failure.json',{'status':'BLOCKED_M13_OFFLINE_MATERIAL','error':str(exc),'sealError':str(seal_error),'retryCount':0})
                if not (E/'FAILED_EVIDENCE').exists(): (E/'FAILED_EVIDENCE').write_text(str(exc)+'\n')
        raise
if __name__=='__main__': main()

