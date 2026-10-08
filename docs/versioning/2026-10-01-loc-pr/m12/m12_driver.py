#!/opt/homebrew/bin/python3
"""M12 acquisition-only delta derived from frozen M11; no restore or signature execution."""
from __future__ import annotations
import base64, copy, csv, datetime, difflib, hashlib, io, json, os, re, signal, stat, sys, time
import urllib.error, urllib.parse, urllib.request, zipfile
import xml.etree.ElementTree as ET
from pathlib import Path, PurePosixPath
ROOT = Path('/private/tmp/fightmatch-loc-lic-alt-m12/loc-lic-alt-yamldotnet-16.3.0-m12')
REPO = Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
E = REPO / 'TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m12'
M = Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m12')
T11 = Path('/private/tmp/fightmatch-loc-lic-alt-m11/loc-lic-alt-yamldotnet-16.3.0-m11')
E11 = E.parent / 'loc-lic-alt-yamldotnet-16.3.0-m11'
DOC = REPO / 'docs/team/2026-09-30'
CHECKER = DOC / 'engineering-loc-license-alt-m-materials-continuation-11-preflight.py'
TABLE = CHECKER.with_suffix('.tsv')
ACQ = ROOT / 'acquisition'
LEDGER = ACQ / 'm12-network-requests.jsonl'
CHAIN = ACQ / 'combined-network-requests.jsonl'
SOURCE = T11 / 'preflight/m11_acquire.py'
DRIVER = ROOT / 'preflight/m12_driver.py'
TOTAL_CAP = 5 * 1024 * 1024
PACKAGES = [
    {'id': 'System.Reflection.Metadata', 'version': '1.6.0', 'license': 'MIT', 'cap': 2*1024*1024},
    {'id': 'System.Collections.Immutable', 'version': '1.5.0', 'license': 'MIT', 'cap': 1024*1024},
    {'id': 'xunit.abstractions', 'version': '2.0.3', 'license': 'Apache-2.0', 'cap': 256*1024},
]
SENTINELS = ('N/A_OFFICIAL_BINARY_PACKAGE', 'N/A_EMBEDDED_LICENSE_ENTRY')
class Blocked(RuntimeError): pass
class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        raise Blocked(f'BLOCKED_NETWORK_REDIRECT:{code}:{newurl}')
OPENER = urllib.request.build_opener(NoRedirect())
def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def write_json(path: Path, value) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes((json.dumps(value, ensure_ascii=False, sort_keys=True, indent=2) + '\n').encode())

def read_jsonl(path: Path) -> list[dict]:
    if not path.exists():
        return []
    return [json.loads(x) for x in path.read_text(encoding='utf-8').splitlines() if x]

def append_jsonl(path: Path, row: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('ab') as f:
        f.write((json.dumps(row, ensure_ascii=False, sort_keys=True, separators=(',', ':')) + '\n').encode())

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

def repo_from(catalog: dict, nuspec: dict) -> dict:
    candidates = []
    crepo = catalog.get('repository')
    if isinstance(crepo, dict):
        candidates.append({'type': crepo.get('type', ''), 'url': crepo.get('url', ''), 'commit': crepo.get('commit', '')})
    if any(nuspec['repository'].values()):
        candidates.append(nuspec['repository'])
    candidates = [x for x in candidates if x.get('url') and x.get('commit')]
    if not candidates:
        raise Blocked(f"BLOCKED_LICENSE_CLOSURE:no exact repository identity:{nuspec['id']}")
    first = candidates[0]
    for other in candidates[1:]:
        if (other['url'].rstrip('/').lower(), other['commit'].lower()) != (first['url'].rstrip('/').lower(), first['commit'].lower()):
            raise Blocked(f"BLOCKED_LICENSE_CLOSURE:repository disagreement:{nuspec['id']}")
    if not re.fullmatch(r'[0-9a-fA-F]{40}', first['commit']):
        raise Blocked(f"BLOCKED_LICENSE_CLOSURE:repository commit:{nuspec['id']}")
    parsed = urllib.parse.urlsplit(first['url'])
    parts = [x for x in parsed.path.strip('/').split('/') if x]
    if parsed.hostname not in ('github.com', 'www.github.com') or len(parts) != 2:
        raise Blocked(f"BLOCKED_LICENSE_CLOSURE:repository URL:{nuspec['id']}")
    return {'type': 'git', 'url': f'https://github.com/{parts[0]}/{parts[1].removesuffix(".git")}', 'owner': parts[0], 'name': parts[1].removesuffix('.git'), 'commit': first['commit'].lower()}

def license_path(nuspec: dict) -> str:
    candidates = []
    for name in nuspec['entries']:
        if not name.endswith('/') and Path(name).name.lower().startswith(('license', 'copying')):
            candidates.append(name)
    if candidates:
        return sorted(candidates, key=lambda x: (len(Path(x).parts), x.lower()))[0]
    parsed = urllib.parse.urlsplit(nuspec.get('licenseUrl', ''))
    if parsed.hostname in ('raw.githubusercontent.com', 'raw.github.com'):
        parts = [x for x in parsed.path.split('/') if x]
        if len(parts) >= 4:
            return '/'.join(parts[3:])
    raise Blocked(f"BLOCKED_LICENSE_CLOSURE:no metadata-derived source path:{nuspec['id']}")


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

def old_roots():
    out={}
    for n in range(1,12):
        for p in (Path(f'/private/tmp/fightmatch-loc-lic-alt-m{n:02d}/loc-lic-alt-yamldotnet-16.3.0-m{n:02d}'),
                  E.parent/f'loc-lic-alt-yamldotnet-16.3.0-m{n:02d}',
                  M.parent/f'5.1.0-fm-yamldotnet-16.3.0-m{n:02d}'):
            out[str(p)]=aggregate(p)
    return out

def inherited_checks():
    namespace={'__name__':'m12_readonly_check_functions'}
    exec(compile(CHECKER.read_bytes(), str(CHECKER), 'exec'), namespace)
    rows=namespace['read_table'](TABLE); results=[]; groups={}
    for index,row in enumerate(rows,1):
        groups.setdefault((row['setId'],row['mode'],row['root']),[]).append((index,row))
    for (_,mode,_),members in groups.items():
        if members[0][1]['setId']=='m11-roots':
            for i,row in members:
                results.append({'sourceRow':i,'setId':row['setId'],'mode':'entry-absence-proof',
                                'path':row['root'],'actual':'PASS',
                                'reason':'M12 replaces obsolete M11 absent gate with activation entry T/E/M absent proof'})
            continue
        rs=[r for _,r in members]
        if mode=='exact-file-set':
            details,_=namespace['check_exact_file_set'](rs)
        elif mode in ('manifest7','manifest3','aggregate-file-set'):
            require(len(rs)==1,'BLOCKED_TABLE_GROUP')
            detail,_=namespace[{'manifest7':'check_manifest7','manifest3':'check_manifest3','aggregate-file-set':'check_aggregate_file_set'}[mode]](rs[0])
            details=[detail]
        elif mode in ('file','runtime-file'):
            details=[namespace['compare_file'](r,allow_symlink=mode=='runtime-file') for r in rs]
        elif mode=='absent':
            details=[]
            for r in rs:
                p=namespace['target_for'](r); require(not p.exists() and not p.is_symlink(),f'BLOCKED_EXPECTED_ABSENT:{p}')
                details.append('absent')
        else: raise Blocked('BLOCKED_TABLE_MODE:'+mode)
        for (i,r),detail in zip(members,details):
            results.append({'sourceRow':i,'setId':r['setId'],'mode':mode,'path':str(namespace['target_for'](r)),
                            'actual':detail,'reason':'unchanged frozen M11 content gate'})
    require(len(rows)==111 and len(results)==111,'BLOCKED_PREFLIGHT_ROW_COUNT')
    return sorted(results,key=lambda x:x['sourceRow'])

def preflight():
    require(not (ROOT/'preflight/preflight.json').exists(),'BLOCKED_PREFLIGHT_ALREADY_RUN')
    activation=json.loads((ROOT/'preflight/activation.json').read_text())
    require(all(activation['entryCheck']['rootsAbsent'].values()),'BLOCKED_ENTRY_ROOTS')
    require(min(activation['entryCheck']['freeBytes'].values())>=2*1024**3,'BLOCKED_FREE_SPACE')
    require(activation['reviewedHead'] is None and not M.exists(),'BLOCKED_REVIEW_OR_MATERIAL_STATE')
    require(not E.exists(),'BLOCKED_EVIDENCE_ROOT_EXISTS')
    E.mkdir(parents=True)
    json_once(E/'authority/activation.json',activation)
    fixed(CHECKER,'3ba975931686729800f5704cd3658b5bf5c20a20791f83a7a854a175f4c8ceaf',16586)
    fixed(TABLE,'3e51974a8237b7cc8168123fd7f5e4bd5a1d05383e0d4fbd715afcfe4819651b',27149)
    fixed(SOURCE,'f71c3adc0475f068206e350648865f5a1b8b9627503d1821103c0547f1f7b49d',20663)
    additions=[]
    for p,h,b in [
        (DOC/'engineering-loc-license-alt-m-materials-continuation-12.md',activation['packet']['sha256'],activation['packet']['bytes']),
        (DOC/'engineering-localization-legacy-package-license-evidence-correction.md','86941725f20168e5c51583ca17af4578ef25b89850d351d0d6c4ff0bfc85c870',13648),
        (DOC/'engineering-loc-license-alt-m-materials-continuation-11.md','88cfffa8cf0c8f29f6480501e4583eb4bab5027459f390fb506bd5431f7e2496',6407),
        (E11/'result.json','bf9c0d4b856cb8e29c2109e71296c8521f9d6d99d27603867c569ddac7d1dfc0',3471),
        (E11/'evidence-files.tsv','d1c8e8e83bd5c6e4061d6a96a4c604fe00c6e93cf02cdf5e9cf073093e2957b0',500)]:
        additions.append({'path':str(p),**fixed(p,h,b),'source':'M12 section 1'})
    expected={
        'acquisition/combined-network-requests.jsonl':(1813,'36470f78fd6593e6cdc5c70e65c2aca8bf07f0b34069c14b37c3dbd4e6abebc4'),
        'acquisition/m11-network-requests.jsonl':(1105,'855a96552fba9e92ede13579e3e45fcbece251d86b21e3d25c3b398b905357db'),
        'acquisition/packages/system.reflection.metadata/1.6.0/catalog.json':(11516,'ee0ebaa566d832502f2e388eeddff89619286c78b5e6cd3719ed98978290f918'),
        'acquisition/packages/system.reflection.metadata/1.6.0/registration.json':(791,'9ae28ac42b0927fdc3930ff72abaf60fc4f00086ab60d408949c9ec9d5146d0b'),
        'acquisition/packages/system.reflection.metadata/1.6.0/system.reflection.metadata.1.6.0.nupkg':(852113,'2497e068f6afed47c4878c9101074684b645d5baebd2d5163e5eaa99f356abf1'),
        'preflight/m11_acquire.py':(20663,'f71c3adc0475f068206e350648865f5a1b8b9627503d1821103c0547f1f7b49d'),
        'preflight/owner-preflight.json':(2247,'82d182eedc0d955a15f17e8ebee214602b8ed1c3e872a5fb57590f3885451bda'),
        'preflight/registration-fixtures/fixture-receipt.json':(2263,'a69e557bb2a5dbc47adbf3d6e1f6780e98eedf494861b1b2fbb816950e2fc417'),
        'preflight/registration-fixtures/registration-object.json':(800,'1de942b646be2a27b0c30a4d16076efc33d134e1fea65b5d3dfd518fe9b8c189'),
        'preflight/registration-fixtures/registration-string.json':(791,'9ae28ac42b0927fdc3930ff72abaf60fc4f00086ab60d408949c9ec9d5146d0b')}
    require({p.relative_to(T11).as_posix() for p in regular_files(T11)}==set(expected),'BLOCKED_M11_TEMP_SET')
    for rel,(b,h) in expected.items():
        additions.append({'path':str(T11/rel),**fixed(T11/rel,h,b),'source':'M11 sealed execution receipt / M12 section 1'})
    with (E11/'evidence-files.tsv').open() as f: oldrows=list(csv.DictReader(f,delimiter='\t'))
    require({p.relative_to(E11).as_posix() for p in regular_files(E11)}=={r['relativePath'] for r in oldrows}|{'evidence-files.tsv'},'BLOCKED_M11_EVIDENCE_SET')
    for r in oldrows:
        additions.append({'path':str(E11/r['relativePath']),**fixed(E11/r['relativePath'],r['sha256'],int(r['bytes'])),'source':'M11 fixed evidence manifest'})
    rows=inherited_checks()
    tsv_once(ROOT/'preflight/m12-inputs.tsv',['source','mode','path','actual','reason'],
             [(f'M11 table row {r["sourceRow"]}',r['mode'],r['path'],r['actual'],r['reason']) for r in rows]+
             [(r['source'],'file',r['path'],f'{r["bytes"]}:{r["sha256"]}','new M12 fixed input') for r in additions])
    diff=''.join(difflib.unified_diff(SOURCE.read_text().splitlines(True),DRIVER.read_text().splitlines(True),fromfile='m11_acquire.py',tofile='m12_driver.py')).encode()
    receipt={'status':'PASS','inheritedRows':len(rows),'obsoleteAbsenceRowsReplaced':3,
             'newFixedInputs':additions,'table':identity(TABLE),'checker':identity(CHECKER),
             'driver':identity(DRIVER),'sourceDriver':identity(SOURCE),'derivationDiffSha256':sha256(diff),
             'derivationDiffBytes':len(diff),'inputTable':identity(ROOT/'preflight/m12-inputs.tsv'),
             'oldRootsBefore':old_roots(),'networkRequests':0,'runnerRestoreCount':0,'testsRestoreCount':0,
             'actualArgv':sys.argv,'ownerTurn':activation['ownerTurn']}
    json_once(ROOT/'preflight/preflight.json',receipt)
    json_once(E/'authority/preflight.json',receipt)
    print(json.dumps({'status':'PREFLIGHT_PASS','inheritedRows':len(rows),'additionalInputs':len(additions),'oldRoots':len(receipt['oldRootsBefore']),'network':0}))

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

def extract_package(spec, directory):
    lower=spec['id'].lower(); version=spec['version']
    reg=directory/'registration.json'; cat=directory/'catalog.json'; pkg=directory/f'{lower}.{version}.nupkg'
    leaf=parse_registration(json.loads(reg.read_bytes()),spec['id'],version)
    catalog=json.loads(cat.read_bytes())
    require(str(catalog.get('id','')).lower()==lower and catalog.get('version')==version and catalog.get('packageHashAlgorithm')=='SHA512','BLOCKED_CATALOG_IDENTITY:'+lower)
    raw=pkg.read_bytes(); digest512=base64.b64encode(hashlib.sha512(raw).digest()).decode()
    require(digest512==catalog.get('packageHash'),'BLOCKED_CATALOG_HASH:'+lower)
    nuspec=parse_nuspec(pkg)
    require(nuspec['id']==spec['id'] and nuspec['version']==version,'BLOCKED_NUSPEC_IDENTITY:'+lower)
    r={'id':spec['id'],'version':version,'graph':'tests','registrationUrl':json.loads(reg.read_bytes())['@id'],
       'registrationPackageContent':leaf['packageContent'],'registrationCatalogEntry':leaf['catalogUrl'],
       'catalogUrl':leaf['catalogUrl'],'catalogId':catalog['id'],'catalogVersion':catalog['version'],
       'catalogHashAlgorithm':'SHA512','catalogSha512':digest512,'nupkgUrl':leaf['packageContent'],
       'nupkgRelativePath':pkg.relative_to(ROOT).as_posix(),'nupkgBytes':len(raw),'nupkgSha256':sha256(raw),
       'registrationIdentity':identity(reg),'catalogIdentity':identity(cat),'nuspec':nuspec,
       'registrationReceiptRelativePath':f'identity/package-registration/{lower}.{version}.json',
       'catalogReceiptRelativePath':f'identity/package-catalog/{lower}.{version}.json',
       'signatureReceiptRelativePath':f'verification/package-signatures/{lower}.{version}.json',
       'signatureVerdict':'SIGNATURE_PENDING','licenseStatus':'PENDING','licenses':[],'notices':[]}
    entries=[]; licenses=[]; notices=[]; extracted={}
    with zipfile.ZipFile(pkg) as z:
        names=z.namelist(); require(len(names)==len(set(names)),'BLOCKED_ZIP_DUPLICATE')
        require(sum(i.file_size for i in z.infolist())<32*1024**2,'BLOCKED_ZIP_EXPANSION')
        for info in z.infolist():
            safe_entry(info.filename)
            require(stat.S_IFMT(info.external_attr>>16)!=stat.S_IFLNK,'BLOCKED_ZIP_SYMLINK')
            if info.is_dir(): continue
            data=z.read(info.filename)
            entry={'entryPath':info.filename,'bytes':len(data),'sha256':sha256(data)}
            entries.append(entry)
            base=PurePosixPath(info.filename).name.lower()
            islicense=base.startswith(('license','copying'))
            isnotice=base.startswith(('notice','third-party-notices','third_party_notices'))
            if islicense or isnotice or info.filename=='.signature.p7s' or info.filename==nuspec['nuspecPath']:
                target=directory/'entries'/info.filename
                target.parent.mkdir(parents=True,exist_ok=True); target.write_bytes(data)
                entry={**entry,'localRelativePath':f'licenses/embedded/{lower}/{version}/{info.filename}',
                       'actualLocalPath':str(target)}
                extracted[info.filename]=entry
                if islicense: licenses.append(entry)
                if isnotice: notices.append(entry)
        require('.signature.p7s' in extracted,'BLOCKED_SIGNATURE_ENTRY:'+lower)
    sig=extracted['.signature.p7s']; r.update(signatureBytes=sig['bytes'],signatureSha256=sig['sha256'],signatureLocalPath=sig['actualLocalPath'])
    r['notices']=notices
    r['entriesManifestRelativePath']=(directory/'package-entries.tsv').relative_to(ROOT).as_posix()
    tsv_once(directory/'package-entries.tsv',['entryPath','bytes','sha256'],[(x['entryPath'],x['bytes'],x['sha256']) for x in entries])
    json_once(directory/'nuspec.json',nuspec)
    json_once(ACQ/r['registrationReceiptRelativePath'],{'id':r['id'],'version':version,'registrationUrl':r['registrationUrl'],**leaf,'rawIdentity':identity(reg)})
    json_once(ACQ/r['catalogReceiptRelativePath'],{'id':catalog['id'],'version':catalog['version'],'packageHashAlgorithm':'SHA512','packageHash':digest512,'catalogUrl':r['catalogUrl'],'rawIdentity':identity(cat)})
    if licenses:
        try:
            for entry in licenses:
                body=Path(entry['actualLocalPath']).read_bytes()
                expression=classify_license(body)
                require(expression==spec['license'],'BLOCKED_LICENSE_EVIDENCE:expected-'+spec['license'])
                row=project_license(r,entry,expression); validate_projection(row,r,body,'acquisition'); r['licenses'].append(row)
            for entry in notices:
                body=Path(entry['actualLocalPath']).read_bytes()
                row=project_license(r,entry,'NOTICE'); validate_projection(row,r,body,'acquisition'); r['licenses'].append(row)
            r.update(licenseStatus='EMBEDDED_BODY_VERIFIED_SIGNATURE_PENDING',evidenceLane='binary-package/embedded-license')
        except (Blocked,UnicodeError) as exc:
            r['licenseBlocker']=str(exc)
    else:
        r['licenseBlocker']='BLOCKED_LICENSE_EVIDENCE:no-embedded-license'
        try:
            repo=repo_from(catalog,nuspec); path=license_path(nuspec)
            allowed={'System.Reflection.Metadata':{'dotnet'},'System.Collections.Immutable':{'dotnet'},'xunit.abstractions':{'xunit'}}[spec['id']]
            require(repo['owner'].lower() in allowed,'BLOCKED_RAW_REPOSITORY_OWNER')
            safe_entry(path)
            r['rawCandidate']={'repository':repo,'sourcePath':path,'url':f'https://raw.githubusercontent.com/{repo["owner"]}/{repo["name"]}/{repo["commit"]}/{path}'}
        except Blocked as exc: r['licenseBlocker']=str(exc)
    json_once(directory/'package-evidence.json',r)
    return r

def fixtures():
    pre=json.loads((ROOT/'preflight/preflight.json').read_text())
    require(pre['status']=='PASS' and not LEDGER.exists() and not CHAIN.exists(),'BLOCKED_FIXTURE_PREREQUISITE')
    outputs=[]
    for kind in ('string','object'):
        p=T11/f'preflight/registration-fixtures/registration-{kind}.json'
        outputs.append(parse_registration(json.loads(p.read_bytes()),'System.Reflection.Metadata','1.6.0'))
    require(all(outputs[0][k]==outputs[1][k] for k in ('catalogUrl','packageContent','id','version')),'BLOCKED_PARSER_FIXTURE')
    old=json.loads((T11/'preflight/registration-fixtures/fixture-receipt.json').read_text())
    require(old['status']=='PASS' and outputs[0]==old['stringFixture']['output'] and outputs[1]==old['objectFixture']['output'],'BLOCKED_OLD_FIXTURE')
    for name in ('registration.json','catalog.json','system.reflection.metadata.1.6.0.nupkg'):
        src=T11/'acquisition/packages/system.reflection.metadata/1.6.0'/name
        dst=ACQ/'packages/system.reflection.metadata/1.6.0'/name
        dst.parent.mkdir(parents=True,exist_ok=True); dst.write_bytes(src.read_bytes())
    p=extract_package(PACKAGES[0],ACQ/'packages/system.reflection.metadata/1.6.0')
    require(p['licenseStatus']=='EMBEDDED_BODY_VERIFIED_SIGNATURE_PENDING' and len(p['licenses'])==2,'BLOCKED_SRM_LICENSE_FIXTURE')
    license_row=next(r for r in p['licenses'] if r['expression']=='MIT')
    with zipfile.ZipFile(ACQ/p['nupkgRelativePath'].removeprefix('acquisition/')) as z:
        raw=z.read('LICENSE.TXT'); notice=z.read('THIRD-PARTY-NOTICES.TXT')
    require(len(raw)==1139 and sha256(raw)=='d7a68596ab69b06f51ca278a6545148e4269a9381c26d597c13df5d88e08cf5b','BLOCKED_SRM_LICENSE_HASH')
    require(len(notice)==15835 and sha256(notice)=='7864a01e2fdef7e8fdf81b906efb1466f083206affea7ba7e6dadea429754765','BLOCKED_SRM_NOTICE_HASH')
    rejected=[]
    for label,field,value,body,phase in [
        ('sentinel-in-source-lane','evidenceLane','source-rebuild',raw,'acquisition'),
        ('sentinel-other-package','component','Other.Package',raw,'acquisition'),
        ('missing-body',None,None,b'','acquisition'),
        ('missing-relationship','packageRelationshipEvidence','',raw,'acquisition'),
        ('missing-independent-signature',None,None,raw,'material')]:
        row=copy.deepcopy(license_row)
        if field: row[field]=value
        try: validate_projection(row,p,body,phase)
        except Blocked: rejected.append(label)
        else: raise Blocked('BLOCKED_NEGATIVE_FIXTURE:'+label)
    missing=copy.deepcopy(p); missing['signatureBytes']=0
    try: validate_projection(license_row,missing,raw,'acquisition')
    except Blocked: rejected.append('missing-signature-entry')
    else: raise Blocked('BLOCKED_NEGATIVE_FIXTURE:signature-entry')
    original=T11/'acquisition/combined-network-requests.jsonl'
    (ACQ/'m11-original-combined-network-requests.jsonl').write_bytes(original.read_bytes())
    for row in read_jsonl(original):
        imported={**row,'newNetworkRequest':False,'reusedFromM11':True,'originalLedger':str(original),
                  'originalLedgerSha256':identity(original)['sha256']}
        append_jsonl(CHAIN,imported)
    receipt={'status':'PASS','parserOutputs':outputs,'negativeFixturesRejected':rejected,
             'embeddedLicenseAndNotice':'PASS','signatureState':'SIGNATURE_PENDING',
             'networkRequestsBeforeFixturePass':0,'driver':identity(DRIVER),'sourceDriver':identity(SOURCE),
             'm11FixtureReceipt':identity(T11/'preflight/registration-fixtures/fixture-receipt.json')}
    json_once(ROOT/'preflight/fixtures.json',receipt)
    print(json.dumps({'status':'FIXTURES_PASS','negativeFixtures':len(rejected),'network':0,'reusedRequests':3}))

def headers_bytes(headers):
    return ''.join(f'{k.lower().strip()}: {" ".join(v.strip().split())}\n' for k,v in sorted(headers.items())).encode()

def request(url, cap, target, kind, request_id):
    parsed=urllib.parse.urlsplit(url)
    require(parsed.scheme=='https' and parsed.hostname in {'api.nuget.org','raw.githubusercontent.com'} and not parsed.username and not parsed.query and not parsed.fragment,'BLOCKED_NETWORK_URL')
    prior=read_jsonl(LEDGER); chain=read_jsonl(CHAIN)
    require(len(prior)<8 and len(chain)<11 and url not in {r['url'] for r in chain},'BLOCKED_NETWORK_COUNT_OR_DUPLICATE')
    remaining=TOTAL_CAP-sum(r['bytes'] for r in chain)
    require(remaining>0 and not target.exists(),'BLOCKED_NETWORK_BUDGET_OR_TARGET')
    cap=min(cap,remaining)
    started=time.monotonic(); at=datetime.datetime.now(datetime.timezone.utc).isoformat()
    data=b''; hb=b''; status=None; redirect=0; error=None
    def timeout(signum,frame): raise Blocked('BLOCKED_NETWORK_TIMEOUT')
    previous=signal.signal(signal.SIGALRM,timeout); signal.setitimer(signal.ITIMER_REAL,60)
    try:
        req=urllib.request.Request(url,headers={'Accept-Encoding':'identity','User-Agent':'FightMatch-LOC-LIC-ALT-M12/1'})
        with OPENER.open(req,timeout=60) as response:
            status=response.status; hb=headers_bytes(response.headers)
            require(status==200 and response.geturl()==url,'BLOCKED_NETWORK_RESPONSE')
            data=response.read(cap+1)
            require(len(data)<=cap,'BLOCKED_NETWORK_BYTE_CAP')
    except Exception as exc:
        error=f'{type(exc).__name__}:{exc}'
        if isinstance(exc,urllib.error.HTTPError):
            status=exc.code; hb=headers_bytes(exc.headers)
        if 'REDIRECT' in error: redirect=1
    finally:
        signal.setitimer(signal.ITIMER_REAL,0); signal.signal(signal.SIGALRM,previous)
    target.parent.mkdir(parents=True,exist_ok=True); target.write_bytes(data)
    row={'requestId':request_id,'kind':kind,'url':url,'host':parsed.hostname,'status':status,
         'startedAt':at,'durationMs':round((time.monotonic()-started)*1000),'redirectCount':redirect,
         'bytes':len(data),'sha256':sha256(data),'responseHeadersSha256':sha256(hb),
         'localRelativePath':target.relative_to(ROOT).as_posix(),'newNetworkRequest':True,'error':error}
    (target.parent/(target.name+'.headers')).write_bytes(hb)
    append_jsonl(LEDGER,row); append_jsonl(CHAIN,row)
    require(error is None,'BLOCKED_NETWORK_REQUEST:'+str(error))
    return row

def closure(packages):
    by={p['id']:p for p in packages}
    def group(pid,aliases):
        rows=[g for g in by[pid]['nuspec']['dependencyGroups'] if g['targetFramework'].lower() in aliases]
        require(len(rows)==1,'BLOCKED_DEPENDENCY_GROUP:'+pid)
        return rows[0]
    srm=group('System.Reflection.Metadata',('.netstandard2.0','netstandard2.0'))
    require(srm['dependencies']==[{'id':'System.Collections.Immutable','version':'1.5.0'}],'BLOCKED_DEPENDENCY_SRM')
    immutable=group('System.Collections.Immutable',('.netstandard2.0','netstandard2.0'))
    require(not immutable['dependencies'],'BLOCKED_DEPENDENCY_IMMUTABLE')
    xunit=group('xunit.abstractions',('.netstandard2.0','netstandard2.0'))
    require(not xunit['dependencies'],'BLOCKED_DEPENDENCY_XUNIT')
    feed=Path('/private/tmp/fightmatch-loc-lic-alt-m09/loc-lic-alt-yamldotnet-16.3.0-m09/closure/tests-generation/feed')
    parents=[]
    for filename,dep,version in [('microsoft.testplatform.objectmodel.17.11.1.nupkg','System.Reflection.Metadata','1.6.0'),
                                ('xunit.extensibility.core.2.9.2.nupkg','xunit.abstractions','2.0.3')]:
        n=parse_nuspec(feed/filename)
        selected=[g for g in n['dependencyGroups'] if g['targetFramework'].lower() in ('.netstandard2.0','netstandard2.0','.netstandard1.1','netstandard1.1')]
        require(any(any(d['id']==dep and d['version'] in (version,'['+version+']','['+version+', )') for d in g['dependencies']) for g in selected),'BLOCKED_PARENT_DEPENDENCY:'+filename)
        parents.append({'package':filename,'identity':identity(feed/filename),'compatibleGroups':selected})
    return {'status':'PASS','selection':'net8.0 -> netstandard2.0 nearest compatible group for all three new packages',
            'newPackageIds':list(by),'selectedGroups':[srm,immutable,xunit],
            'parentEdges':parents,'newExternalPackageCount':3,'fourthPackageRequired':False,
            'frameworkSuppressed':'NETStandard.Library is not selected in the netstandard2.0 groups'}

def budgets():
    actual=read_jsonl(LEDGER); chain=read_jsonl(CHAIN)
    return {'macNewRequests':len(actual),'macNewBytes':sum(r['bytes'] for r in actual),
            'macChainRequests':len(chain),'macChainBytes':sum(r['bytes'] for r in chain),
            'redirects':sum(r['redirectCount'] for r in chain),'cloudRequests':0,'cloudBytes':0,
            'combinedRequests':len(chain),'combinedBytes':sum(r['bytes'] for r in chain),
            'limits':{'macNewMaxRequests':8,'macNewMaxBytes':4378460,'macChainMaxRequests':11,
                      'macChainMaxBytes':TOTAL_CAP,'conditionalCloudMaxRequests':3,
                      'conditionalCloudMaxBytes':TOTAL_CAP,'combinedMaxRequests':14,'combinedMaxBytes':2*TOTAL_CAP}}

def seal(status, packages, failure=None):
    pre=json.loads((ROOT/'preflight/preflight.json').read_text()) if (ROOT/'preflight/preflight.json').exists() else {}
    equality={'before':pre.get('oldRootsBefore'),'after':old_roots()}
    equality['equal']=equality['before']==equality['after']
    json_once(E/'old-root-equality.json',equality)
    net=budgets(); json_once(E/'network-ledgers.json',{'counts':net,'actual':read_jsonl(LEDGER),'chain':read_jsonl(CHAIN)})
    tsv_once(E/'acquisition-files.tsv',['relativePath','bytes','sha256'],[(p.relative_to(ROOT).as_posix(),*identity(p).values()) for p in regular_files(ACQ)])
    tsv_once(E/'process/completed-commands.tsv',['phase','argv','status'],
             [('preflight',f'{sys.executable} {DRIVER} preflight','PASS' if pre else 'FAIL'),
              ('fixtures',f'{sys.executable} {DRIVER} fixtures','PASS' if (ROOT/'preflight/fixtures.json').exists() else 'NOT_RUN'),
              ('acquire',f'{sys.executable} {DRIVER} acquire',status)])
    tsv_once(E/'process/not-run-commands.tsv',['phase','count','reason'],[(s,0,'not authorized by acquisition activation') for s in ('signature verification','runner restore','tests generation restore','tests locked restore','build','test','Luban','Unity','Git','material finalization','005','B','L')])
    require(not M.exists(),'BLOCKED_MATERIAL_PRESENT')
    if not equality['equal']: status='BLOCKED_OLD_ROOT_DRIFT'; failure=failure or status
    temp=aggregate(ROOT); evid=aggregate(E)
    elapsed=(datetime.datetime.now(datetime.timezone.utc)-datetime.datetime.fromisoformat(json.loads((ROOT/'preflight/activation.json').read_text())['entryCheck']['at'])).total_seconds()
    free={str(p):os.statvfs(p).f_bavail*os.statvfs(p).f_frsize for p in (Path('/private/tmp'),M.parent)}
    if temp['bytes']>768*1024**2 or evid['bytes']>16*1024**2 or elapsed>1800 or min(free.values())<2*1024**3:
        status='BLOCKED_RESOURCE_BUDGET'; failure=failure or status
    result={'status':status,'phase':'acquisition','ownerThread':'01a0f40d-b0c5-7bc0-a2b2-be9d1213648a','ownerHost':'local',
            'ownerTurn':json.loads((ROOT/'preflight/activation.json').read_text())['ownerTurn'],
            'completionReceiver':'01a0e401-511d-79f2-b47f-3ab0ade1681b/local','reviewedHead':None,
            'network':net,'packages':packages,'firstFailure':failure,'driver':identity(DRIVER),
            'inputTable':identity(ROOT/'preflight/m12-inputs.tsv') if (ROOT/'preflight/m12-inputs.tsv').exists() else None,
            'oldRootEquality':equality['equal'],'materialAbsent':not M.exists(),
            'restoreCounts':{'runner':0,'testsGeneration':0,'testsLocked':0},
            'temp':temp,'evidenceBeforeResultAndManifest':evid,'elapsedWallSeconds':elapsed,'freeBytesEnd':free}
    if failure:
        json_once(E/'authority/first-failure.json',{'firstFailure':failure,'status':status,'retryCount':0})
        (E/'FAILED_EVIDENCE').write_text(status+'\n',encoding='utf-8')
        json_once(E/'result.json',result)
    else:
        json_once(E/'authority/signature-handoff.json',result)
    tsv_once(E/'evidence-files.tsv',['relativePath','bytes','sha256'],[(p.relative_to(E).as_posix(),*identity(p).values()) for p in regular_files(E)])
    print(json.dumps({'status':status,'packages':len(packages),'network':net,'evidence':str(E),'oldRootEquality':equality['equal']}))
    return status

def acquire():
    fixture=json.loads((ROOT/'preflight/fixtures.json').read_text())
    require(fixture['status']=='PASS' and fixture['driver']==identity(DRIVER),'BLOCKED_FROZEN_DRIVER')
    require(not LEDGER.exists() and not (E/'authority/signature-handoff.json').exists() and not (E/'result.json').exists(),'BLOCKED_ACQUIRE_ALREADY_RUN')
    packages=[json.loads((ACQ/'packages/system.reflection.metadata/1.6.0/package-evidence.json').read_text())]
    try:
        for index,spec in enumerate(PACKAGES[1:]):
            lower=spec['id'].lower(); version=spec['version']; directory=ACQ/'packages'/lower/version
            base=4+index*3
            reg=f'https://api.nuget.org/v3/registration5-semver1/{lower}/{version}.json'
            request(reg,256*1024,directory/'registration.json','registration',f'req-{base:02d}')
            leaf=parse_registration(json.loads((directory/'registration.json').read_bytes()),spec['id'],version)
            request(leaf['catalogUrl'],256*1024,directory/'catalog.json','catalog',f'req-{base+1:02d}')
            cat=json.loads((directory/'catalog.json').read_bytes())
            require(cat.get('id','').lower()==lower and cat.get('version')==version and cat.get('packageHashAlgorithm')=='SHA512','BLOCKED_CATALOG_IDENTITY')
            request(leaf['packageContent'],spec['cap'],directory/f'{lower}.{version}.nupkg','nupkg',f'req-{base+2:02d}')
            packages.append(extract_package(spec,directory))
        for p in packages:
            if p['licenseStatus']=='PENDING' and p.get('rawCandidate'):
                c=p['rawCandidate']; target=ACQ/'raw-licenses'/p['id']/p['version']/PurePosixPath(c['sourcePath']).name
                request(c['url'],128*1024,target,'raw-license',f'req-{len(read_jsonl(CHAIN))+1:02d}')
                expression=classify_license(target.read_bytes())
                require(expression==next(s['license'] for s in PACKAGES if s['id']==p['id']),'BLOCKED_RAW_LICENSE')
                p.update(licenseStatus='RAW_BODY_VERIFIED_SIGNATURE_PENDING',evidenceLane='binary-package/raw-repository',
                         rawLicense={**c,**identity(target),'expression':expression,'actualLocalPath':str(target)})
        close=closure(packages); json_once(ACQ/'closure.json',close)
        missing=[{'id':p['id'],'reason':p.get('licenseBlocker')} for p in packages if p['licenseStatus']=='PENDING']
        require(not missing,'BLOCKED_LICENSE_EVIDENCE:'+json.dumps(missing,sort_keys=True))
        rows=[r for p in packages for r in p['licenses']]
        require(len({(r['component'],r['version'],r['packageSha256'],r['sourcePath']) for r in rows})==len(rows),'BLOCKED_LICENSE_JOIN')
        fields=['component','version','sourceUrl','sourceTag','sourceCommit','sourcePath','localRelativePath','bytes','sha256','expression','packageRelationshipEvidence','packageSha256','evidenceLane','signatureVerdict']
        tsv_once(ACQ/'identity/license-files.tsv',fields,[[r[f] for f in fields] for r in rows])
        tsv_once(ACQ/'identity/package-files.tsv',['packageId','version','bytes','sha256','sourceUrl'],[(p['id'],p['version'],p['nupkgBytes'],p['nupkgSha256'],p['nupkgUrl']) for p in packages])
        json_once(ACQ/'identity/sbom.json',{'schemaVersion':1,'status':'SIGNATURE_PENDING','components':packages})
        b=budgets()
        require(6<=b['macNewRequests']<=8 and 9<=b['macChainRequests']<=11 and b['macChainBytes']<=TOTAL_CAP and b['redirects']==0,'BLOCKED_NETWORK_TOTALS')
        seal('WAITING_SIGNATURE_REVIEW',packages)
    except Exception as exc:
        seal('BLOCKED_LICENSE_EVIDENCE' if 'LICENSE' in str(exc) else 'BLOCKED_ACQUISITION',packages,f'{type(exc).__name__}:{exc}')
        raise

def main():
    require(len(sys.argv)==2 and sys.argv[1] in ('preflight','fixtures','acquire'),'usage: m12_driver.py preflight|fixtures|acquire')
    {'preflight':preflight,'fixtures':fixtures,'acquire':acquire}[sys.argv[1]]()
if __name__=='__main__': main()

