#!/opt/homebrew/bin/python3
"""M17 SOURCE_ONLY: deterministic clean material plan and consumption gates.

External M/B/L actions are frozen command contracts for a separately authorized
host. This source-stage program starts no children, performs no HTTP requests,
and never restores historical temporary trees.
"""
import argparse
import ast
import base64
import copy
import csv
import difflib
import hashlib
import io
import json
import os
from pathlib import Path, PurePosixPath
import posixpath
import re
import sys
import time
import xml.etree.ElementTree as ET

REPO=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
BASE=REPO/'TestArtifacts/FightMatch/LOC-LIC-ALT-M'
DOC=REPO/'docs/team/2026-09-30'
OLD17=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17'
OLD17F1=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix01'
OLD17F2=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix02'
E=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix03'
W=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix03')
M=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix03')
TOOL=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0')
SOURCE=TOOL/'source'
DOTNET=TOOL/'dotnet-x64/dotnet'
SDK=TOOL/'dotnet-x64'
F02=BASE/'loc-lic-alt-yamldotnet-16.3.0-m16-fix02'
H16=BASE/'license-closure-bodies-16'
G15=BASE/'license-gap-bodies-15'
H17=BASE/'license-catalog-bindings-17'
H18=BASE/'license-catalog-bindings-18'
DRIVER=E/'source/m17_driver.py'
PREP=('source/m17_driver.py','source/m17-inputs.tsv','source/m17-write-set.tsv','source/source.patch',
      'authority/prepare.json','authority/offline-checks.json','authority/source-freeze.json','authority/source-receipt.json')
OWNER={'threadId':'01a0f40d-b0c5-7bc0-a2b2-be9d1213648a','hostId':'local','turnId':'01a11a84-fb46-79f1-9bd3-9fc6b1d1bf8b'}
ISSUER={'threadId':'01a0e401-511d-79f2-b47f-3ab0ade1681b','hostId':'local','turnId':'01a11a60-d890-7660-b7ee-1accdb3ffe0c'}
DISPATCH='fco_01a11a84-fb55-7622-8a99-66369ed9403d'
CENTRAL='01a0e401-511d-79f2-b47f-3ab0ade1681b'
TRUSTED_CENTRAL_ACTIVATION=None
FIXED_INDEX=(4916277,'043588329ad559fcb33d9578fbe3b584e55cbac265fe8a603b82f42b2dc3d381')
FIXED_HELPER=(204239,'5df4a9acd58b3242c18945c66ab9e7dd0fb9f21b295801be78b18fa1b71d377f')
PATCH=BASE/'loc-lic-alt-yamldotnet-16.3.0-m01/acquisition/patch.diff'
PATCH_ID=(1847,'c89deeb856e1dabbde055a3fa1ec6d901443321fb883ea1ee9ba252c90545c3c')
PATCHED={
 'src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj':(1804,'dcb87ce47a0d9789703c2e578be1aed63bdc63bbac2ac56aa4a1134bd0651911'),
 'src/Luban/Luban.csproj':(4194,'77cf0ebbcd5a8a9e967924db71e6079c034d0a8750bda7f01c75557ebd0eddf6')}
ROOTS={'evidence':E,'work':W,'material':M}
INPUT_HEADER=['class','role','path','bytes','sha256','authority']
WRITE_HEADER=['root','relativePathOrPattern','match','stage','writer','maxFiles','maxBytes','purpose']

class GateError(RuntimeError):
    pass

def require(value,reason):
    if not value:raise GateError(reason)

def sha(data):
    return hashlib.sha256(data).hexdigest()

def jb(value):
    return (json.dumps(value,sort_keys=True,ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode()

def strict(data):
    def pairs(items):
        result={}
        for key,value in items:
            require(key not in result,'duplicate JSON key');result[key]=value
        return result
    def bad(value):raise GateError('nonfinite JSON value')
    require(not data.startswith(b'\xef\xbb\xbf'),'JSON BOM')
    return json.loads(data.decode(),object_pairs_hook=pairs,parse_constant=bad)

def read(path):
    path=Path(path)
    require(not any(p.is_symlink() for p in (path,*path.parents)),'symlink input')
    require(path.is_file(),'missing input: '+str(path))
    return path.read_bytes()

def safe(rel):
    p=PurePosixPath(rel)
    require(isinstance(rel,str) and rel and not p.is_absolute() and str(p)==rel and
            all(v not in ('..','.','') for v in p.parts) and not any(c in rel for c in '\\\t\r\n\0'),'unsafe relative path')
    return rel

def identity(path,data=None):
    b=read(path) if data is None else data
    return {'path':str(path),'bytes':len(b),'sha256':sha(b)}

def check_bytes(data,expected,label):
    require((len(data),sha(data))==tuple(expected),'identity mismatch: '+label)
    return data

def tsv(header,rows):
    output=io.StringIO(newline='');writer=csv.writer(output,delimiter='\t',lineterminator='\n')
    writer.writerow(header)
    for row in rows:
        require(all(not any(c in str(v) for c in '\r\n\t') for v in row),'invalid TSV cell')
        writer.writerow(row)
    return output.getvalue().encode()

def parse_tsv(data):
    require(data.endswith(b'\n') and b'\r' not in data,'noncanonical TSV')
    reader=csv.DictReader(io.StringIO(data.decode()),delimiter='\t')
    result=list(reader)
    require(reader.fieldnames and len(set(reader.fieldnames))==len(reader.fieldnames) and
            all(None not in r and all(v is not None for v in r.values()) for r in result),'bad TSV shape')
    return result

def helper_functions(reader=read):
    # Only these pure F02 bodies are reused. No old globals, prepare, T09 helper,
    # dynamic filesystem model, subprocess, network, or activation body executes.
    b=check_bytes(reader(F02/'source/m16_driver.py'),FIXED_HELPER,'F02 pure helper')
    names={'require','digest','safe_relative','json_data','json_bytes','yaml_fixture_semantics',
           'compare_control_outputs','receipt_file_map','file_map_digest','check_b_data'}
    tree=ast.parse(b)
    nodes=[n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name in names]
    require({n.name for n in nodes}==names,'pinned helper functions missing')
    ns={'hashlib':hashlib,'json':json,'re':re,'Path':Path,'PurePosixPath':PurePosixPath,'GateError':GateError}
    exec(compile(ast.Module(body=nodes,type_ignores=[]),'pinned-F02-pure-functions','exec'),ns)
    return ns

def old_seals(reader=read):
    expected=[
        (BASE/'loc-lic-alt-yamldotnet-16.3.0-m16',2666,'0c9960ae9b861e023b26a8ff914ace2e6cd0021edf8784650261076ab1abaf50'),
        (BASE/'loc-lic-alt-yamldotnet-16.3.0-m16-fix01',3881,'d84ef5066b7a7e9450c372fa46e252a78864dfad75b2902b111bc238aa7ca0dc'),
        (F02,6033,'db19910e7cbfe207a604c394304c539f73f50c222a3f2edc33d881ce64921880'),
        (OLD17,2765,'d572872d038a19418c75c776cef3ae7299616a69118b6a63619c774f0b2fb244'),
        (OLD17F1,2806,'8c8e50f8af094918bdac323ab4080c942616e646d4c4a56a12eaf6a8b74cc2d1')]
    result=[]
    for root,count,digest in expected:
        sealpath=root/'authority/source-freeze.json'
        seal=strict(check_bytes(reader(sealpath),(count,digest),'old seal'))
        entries=seal['filesExcludingThisSeal']+[{'path':str(sealpath),'bytes':count,'sha256':digest}]
        for item in entries:
            check_bytes(reader(Path(item['path'])),(item['bytes'],item['sha256']),'old leaf')
            result.append(item)
        require({str(p) for p in root.rglob('*') if p.is_file()}=={i['path'] for i in entries},'old sealed leaf set changed')
    require(len(result)==38,'old 38-leaf count')
    return result

class Basis:
    """Bounded reads of immutable evidence descriptors and real present inputs."""
    def __init__(self,reader=read,exists=lambda p:p.is_file()):
        self.read=reader;self.exists=exists;self.inputs={}
        b=check_bytes(reader(F02/'source/m16-inputs.tsv'),FIXED_INDEX,'F02 index')
        self.index={r['sourcePath']:r for r in parse_tsv(b)}
        self.add(F02/'source/m16-inputs.tsv',b,'existing','identity-descriptor','accepted F02 index, descriptors only')
        helper=check_bytes(reader(F02/'source/m16_driver.py'),FIXED_HELPER,'F02 helper')
        self.add(F02/'source/m16_driver.py',helper,'existing','pure-helper','fixed F02 pure function selection')
    def add(self,path,b,kind,role,authority):
        row=[kind,role,str(path),len(b),sha(b),authority]
        key=(kind,role,str(path))
        require(key not in self.inputs or self.inputs[key]==row,'input declaration collision')
        self.inputs[key]=row
        return b
    def fixed(self,path,role='provenance'):
        p=Path(path);r=self.index.get(str(p));require(r is not None,'unbound evidence: '+str(p))
        b=check_bytes(self.read(p),(int(r['bytes']),r['sha256']),str(p))
        return self.add(p,b,'existing',role,'fixed F02 input '+r['sha256'])
    def obj(self,path):return strict(self.fixed(path))
    def expected(self,path,count,digest,kind,role,authority):
        self.inputs[(kind,role,str(path))]=[kind,role,str(path),count,digest,authority]

def graph_model(source):
    result={}
    for role,root,counts in [('runner','src/Luban/Luban.csproj',(23,56)),('tests','src/Luban.Tests/Luban.Tests.csproj',(13,29))]:
        nodes=set();edges=set();direct={};active=set()
        def visit(rel):
            require(rel not in active,'project cycle')
            if rel in nodes:return
            require(rel in source,'missing project source: '+rel)
            active.add(rel);tree=ET.fromstring(source[rel]);deps=[]
            for element in tree.iter():
                tag=element.tag.split('}')[-1]
                if tag=='ProjectReference':
                    child=posixpath.normpath(str(PurePosixPath(rel).parent)+'/'+element.attrib['Include'].replace('\\','/'))
                    safe(child);edges.add((rel,child));visit(child)
                if tag=='PackageReference':
                    require('Condition' not in element.attrib and 'Version' in element.attrib,'unmodeled conditional/central package reference')
                    deps.append({'id':element.attrib['Include'],'requested':element.attrib['Version']})
            direct[rel]=sorted(deps,key=lambda x:x['id']);active.remove(rel);nodes.add(rel)
        visit(root)
        require((len(nodes),len(edges))==counts,'fixed project graph drift')
        result[role]={'root':root,'nodes':sorted(nodes),'edges':[list(e) for e in sorted(edges)],'direct':direct}
    return result

def package_model(basis,coverage):
    hi=basis.obj(H16/'inputs.json');gi=basis.obj(G15/'inputs.json')
    hrecords={p['key']:p for p in hi['localPackages']}
    grecords={p['key']:p for p in gi['localPackages']}
    j17=basis.obj(H17/'bindings.json');j18=basis.obj(H18/'bindings.json')
    overlays={p['key']:{'url':p['registration']['packageContent'],'sha512':p['catalog']['packageHash']}
              for p in j17['requiredBindings']}
    overlays.update({p['key']:{'url':p['packageContent'],'sha512':p['catalogIdentity']['packageHash']} for p in j18['boundPackages']})
    packages=[];licenses=[]
    for item in coverage['packages']:
        key=item['key'];origin=item['origin'];bodies=[];url=None
        if origin=='H16':
            p=basis.obj(H16/'provenance'/(key+'.json'));record=hrecords[key]
            ident=p['package'];name=p['component'];version=p['version'];sha512=ident['sha512Base64']
            url=ident.get('immutablePackageUrl')
            for entry in p.get('catalog',{}).get('records',[]):
                if entry.get('fields',{}).get('packageContent'):url=entry['fields']['packageContent']
            for body in p['licenseBodies']:
                path=H16/safe(body['relativePath']);basis.fixed(path,'license')
                bodies.append({'path':str(path),'bytes':body['bytes'],'sha256':body['sha256'],'kind':'existing',
                    'output':'licenses/'+key+'/'+Path(body['relativePath']).name,'role':body['role']})
            relation={'origin':origin,'provenance':str(H16/'provenance'/(key+'.json')),'limits':p['limits']}
        elif origin=='G15_REUSED':
            p=basis.obj(G15/'provenance'/(key+'.json'));record=grecords[key]
            ident=p['package'];name=p['component'];version=p['version'];sha512=ident['sha512Base64']
            body=p['licenseBody'];path=G15/safe(body['relativePath']);basis.fixed(path,'license')
            bodies.append({'path':str(path),'bytes':body['bytes'],'sha256':body['sha256'],'kind':'existing',
                'output':'licenses/'+key+'/'+Path(body['relativePath']).name,'role':'full composite license'})
            relation={'origin':origin,'provenance':str(G15/'provenance'/(key+'.json')),'limits':p['limits']}
        else:
            require(origin=='M13_REUSED','unknown license relation')
            rows=item['inheritedRowsUnchanged'];name=rows[0]['component'];version=rows[0]['version']
            relation={'origin':origin,'provenance':str(H16/'coverage-31.json')+'#'+key,'signature':item['signature'],
                'limits':'QA005 limited acceptance retained; offline/fallback warnings, not a source build proof'}
            tokens=rows[0]['packageRelationshipEvidence'].split('|');url=tokens[5];sha512=tokens[8]
            ident={'sha256':rows[0]['packageSha256']}
            matches=[r for r in basis.index.values() if r['sourcePath'].endswith('/'+key+'.nupkg')]
            values={(int(r['bytes']),r['sha256']) for r in matches};require(len(values)==1,'missing fixed M13 archive identity')
            count,digest=next(iter(values));ident.update(bytes=count,sha256=digest)
            for row in rows:
                count=int(row['bytes']);digest=row['sha256']
                candidates=sorted(r['sourcePath'] for r in basis.index.values()
                    if int(r['bytes'])==count and r['sha256']==digest and not r['sourcePath'].startswith('/private/tmp/') and basis.exists(Path(r['sourcePath'])))
                if candidates:
                    path=Path(candidates[0]);basis.fixed(path,'license')
                    body={'path':str(path),'kind':'existing'}
                else:
                    require(row['evidenceLane']=='binary-package/embedded-license','missing non-rebuildable license: '+key)
                    body={'path':str(W/'derived-licenses'/key/safe(row['sourcePath'])),'kind':'generated',
                          'archiveKey':key,'entry':safe(row['sourcePath'])}
                body.update(bytes=count,sha256=digest,output='licenses/'+key+'/'+Path(row['sourcePath']).name,
                            role=row['expression'],ownRelationship=row['packageRelationshipEvidence'])
                bodies.append(body)
        require(name.lower()+'.'+version==key,'package ID/version relation drift')
        if key in overlays:
            require(overlays[key]['sha512']==sha512,'official catalog SHA512 mismatch')
            url=overlays[key]['url']
        require(isinstance(url,str) and url=='https://api.nuget.org/v3-flatcontainer/'+name.lower()+'/'+version+'/'+key+'.nupkg',
                'official exact package URL unbound: '+key)
        count=int(ident['bytes']);digest=ident['sha256']
        candidate=TOOL/'nuget'/name.lower()/version/(key+'.nupkg')
        kind='existing' if basis.exists(candidate) else 'official-get'
        path=candidate if kind=='existing' else W/'acquire'/(key+'.nupkg')
        if kind=='existing':
            b=check_bytes(basis.read(path),(count,digest),key)
            require(base64.b64encode(hashlib.sha512(b).digest()).decode()==sha512,'present archive SHA512 mismatch')
            basis.add(path,b,kind,'nupkg',url)
        else:basis.expected(path,count,digest,kind,'nupkg',url)
        for body in bodies:
            if body['kind']=='generated':basis.expected(Path(body['path']),body['bytes'],body['sha256'],'generated','license',key+'#'+body['entry'])
        packages.append({'key':key,'id':name,'version':version,'bytes':count,'sha256':digest,'sha512':sha512,
                         'path':str(path),'class':kind,'url':url,'licenses':bodies,'relationship':relation})
        licenses.extend(bodies)
    require(len(packages)==31 and len({p['key'] for p in packages})==31,'31-package closure required')
    require(not any('yamldotnet.netcore' in p['key'] for p in packages),'old YAML package forbidden')
    for path,output in [(SOURCE/'LICENSE','licenses/Luban/LICENSE'),
                        (H16/'standard/Apache-2.0.txt','licenses/standard/Apache-2.0.txt'),
                        (SDK/'LICENSE.txt','licenses/sdk/LICENSE.txt'),(SDK/'ThirdPartyNotices.txt','licenses/sdk/ThirdPartyNotices.txt')]:
        b=basis.fixed(path,'license');licenses.append({'path':str(path),'kind':'existing','bytes':len(b),'sha256':sha(b),'output':output,'role':'retained full terms'})
    return sorted(packages,key=lambda p:p['key']),licenses

def model(reader=read,exists=lambda p:p.is_file()):
    basis=Basis(reader,exists)
    source={};source_rows=[]
    prefix='/private/tmp/fightmatch-loc-lic-alt-m01/loc-lic-alt-yamldotnet-16.3.0-m01/source-official/'
    for row in basis.index.values():
        if not row['sourcePath'].startswith(prefix):continue
        rel=safe(row['sourcePath'][len(prefix):]);path=SOURCE/rel
        b=check_bytes(reader(path),(int(row['bytes']),row['sha256']),rel)
        basis.add(path,b,'existing','official-source','Luban df34215d2e22035073d1af83d44e3ac0744dc3b3')
        if rel in PATCHED:
            old=b'<PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />'
            new=b'<PackageReference Include="YamlDotNet" Version="16.3.0" />'
            require(b.count(old)==1,'two approved replacement sites required')
            b=check_bytes(b.replace(old,new),PATCHED[rel],'patched '+rel)
        source[rel]=b
        source_rows.append({'relativePath':rel,'bytes':len(b),'sha256':sha(b),'changed':rel in PATCHED})
    require(len(source)==931,'931-source inventory required')
    basis.add(PATCH,check_bytes(reader(PATCH),PATCH_ID,'persistent original patch'),'existing','patch','approved two-reference substitution')
    graphs=graph_model(source)
    coverage=basis.obj(H16/'coverage-31.json');packages,licenses=package_model(basis,coverage)
    keys={p['key'] for p in packages};ids={p['id'].lower() for p in packages}
    require(all(d['id'].lower() in ids for g in graphs.values() for ds in g['direct'].values() for d in ds),'direct package omitted from closure')
    yaml=basis.obj(H16/'provenance/yamldotnet.16.3.0.json')['sourceArchive']
    yaml_url='https://codeload.github.com/aaubry/YamlDotNet/tar.gz/refs/tags/v16.3.0'
    require(yaml_url.encode() in reader(F02/'source/m16_driver.py'),'Yaml official archive URL lacks fixed prior record')
    archive={'class':'official-get','role':'yaml-source-archive','path':str(W/'acquire/yamldotnet-v16.3.0.tar.gz'),
        'url':yaml_url,'bytes':yaml['bytes'],'sha256':yaml['sha256'],'commit':yaml['paxHeaders']['comment']}
    basis.expected(Path(archive['path']),archive['bytes'],archive['sha256'],'official-get','yaml-source-archive',yaml_url)
    basis.fixed(DOTNET,'tool')
    # SDK/runtime/targeting packs are real build inputs. Their full exact finite
    # identities are retained; the separately authorized host must fresh-hash
    # every leaf before launching. Source checks do not claim that later gate.
    for row in basis.index.values():
        p=Path(row['sourcePath'])
        if p.is_relative_to(SDK):
            require(exists(p),'missing required SDK leaf: '+str(p))
            basis.expected(p,int(row['bytes']),row['sha256'],'existing','sdk-frozen-leaf','accepted SDK 8.0.425 identity; fresh full hash required before launch')
    # Oracle, preflight 005 and compiler source/negative fixtures remain fixed
    # independent inputs. Only the finite inherited manifests are used as IDs.
    preflight=REPO/'TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005'
    locfix=REPO/'TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02'
    for row in basis.index.values():
        p=Path(row['sourcePath'])
        if p.is_relative_to(TOOL/'runner') or p.is_relative_to(preflight/'scratch') or p==preflight/'commands.json' or p==locfix/'commands.json' or (p.is_relative_to(locfix) and p.name.endswith('.runtimeconfig.json')):
            basis.fixed(p,'oracle-or-control')
    for path in (REPO/'Tools/FightMatch.Localization.Compiler/Program.cs',):
        basis.fixed(path,'localization-compiler')
    for row in basis.index.values():
        p=Path(row['sourcePath'])
        if p.is_relative_to(REPO/'Config/FightMatch'):basis.fixed(p,'localization-config')
    contract=DOC/'engineering-loc-m17-clean-source.md'
    basis.add(contract,reader(contract),'existing','authority','final M/B/L corrected M17 contract')
    contract=DOC/'engineering-loc-m17-stage-budget-fix01.md'
    basis.add(contract,reader(contract),'existing','authority','M-only 900-second FIX01 contract')
    for name in ('engineering-loc-m17-timer-shadow-fix02.md','engineering-loc-m17-fix02-review-host-amendment.md'):
        contract=DOC/name;basis.add(contract,reader(contract),'existing','authority','approved FIX02 and same-turn amendment')
    contract=DOC/'engineering-loc-m17-fix03-path.md'
    basis.add(contract,check_bytes(reader(contract),(7921,'1e083a841f946fc762af1d1291170d4cab7160f0a9f83d20739ba02e9dd80b01'),'FIX03 contract'),'existing','authority','approved FIX03 exact aliases and future roots')
    authority=DOC/'central-resume-scene-loc-2026-10-04.json'
    basis.fixed(authority,'authority')
    basis.add(Path('/usr/bin/curl'),reader(Path('/usr/bin/curl')),'existing','tool','current host curl; fixed before future exact GET')
    python=Path('/opt/homebrew/bin/python3').resolve()
    basis.add(python,reader(python),'existing','tool','resolved Python for future read-only B/L gates; must resolve identically at dispatch')
    basis.add(DRIVER,reader(DRIVER),'existing','source-layer','no generated E17 outputs included')
    result={'owner':OWNER,'issuer':ISSUER,'roots':{k:str(v) for k,v in ROOTS.items()},
        'inputs':sorted(basis.inputs.values()),'sources':sorted(source_rows,key=lambda r:r['relativePath']),
        'graphs':graphs,'packages':packages,'licenses':licenses,'archive':archive,
        'sdkVersion':'8.0.425','sourceCommit':'df34215d2e22035073d1af83d44e3ac0744dc3b3',
        'stages':['M','B','L'],'oracle':str(TOOL/'runner'),'preflight005':str(preflight),'localizationBaseline':str(locfix),
        'historicalEvidenceOnly':{'missingTmpRoots':[1,2,3,4,7,8,9,10,11,12,13],
            'notBuildInputs':True,'restorationRequired':False,'pastFailuresAnd23And46And25Retained':True},
        'states':{'source':'PREPARED_FOR_VERIFICATION','material':'NOT_READY','review':'NEW_HEAD_REQUIRED','execute':'CLOSED_NO_TRUSTED_PIN'}}
    result['commands']=command_plan(result,reader)
    result['writes']=write_plan(result)
    result['generated']=generated_contract(result)
    for r in result['generated']:
        result['inputs'].append(['generated',r['role'],r['path'],'N/A_NOT_GENERATED','N/A_NOT_GENERATED',r['validator']])
    result['inputs'].sort()
    validate_model(result)
    return result

def phase_root(name):return W/'phases'/name

def phase_env(name):
    root=phase_root(name)/'env'
    return {'inheritParent':True,'preserve':['HOME','home','CODEX_HOME'],
        'updates':{'DOTNET_CLI_HOME':str(root/'dotnet-home'),'NUGET_PACKAGES':str(root/'packages'),
            'NUGET_HTTP_CACHE_PATH':str(root/'http'),'NUGET_PLUGINS_CACHE_PATH':str(root/'plugins'),
            'NUGET_SCRATCH':str(root/'scratch'),'TMPDIR':str(root/'tmp'),
            'DOTNET_GENERATE_ASPNET_CERTIFICATE':'false','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH':'false',
            'DOTNET_MULTILEVEL_LOOKUP':'0','DOTNET_CLI_USE_MSBUILD_SERVER':'0','MSBUILDDISABLENODEREUSE':'1'},
        'pathPrepend':str(SDK),'pathSuffix':'unchanged parent PATH; resolved dotnet must equal fixed DOTNET'}

def command_plan(m,reader=read):
    commands=[]
    def add(name,stage,argv,cwd,seconds,children=0,expected=0,depends=()):
        commands.append({'id':name,'stage':stage,'role':'DUT','argv':list(map(str,argv)),'cwd':str(cwd),
            'env':phase_env(name),'timeoutSeconds':seconds,'killGraceSeconds':5,'maxStarts':1,'maxChildren':children,
            'stdoutMaxBytes':4194304,'stderrMaxBytes':4194304,'expectedExit':expected,'dependsOn':list(depends),
            'receiptPath':str(W/'receipts'/(name+'.json')),'stdoutPath':str(W/'logs'/(name+'.stdout')),
            'stderrPath':str(W/'logs'/(name+'.stderr')),'network':'OS denied; local feed only',
            'writer':name,'validation':['exact argv/role','quiescent process tree','direct DUT exit','bounded outputs']})
    for item in [*m['packages'],m['archive']]:
        if item['class']!='official-get':continue
        name='M-get-'+(item.get('key') or 'yaml-source')
        add(name,'M',['/usr/bin/curl','--disable','--fail','--silent','--show-error','--proto','=https','--noproxy','*','--max-redirs','0',
            '--connect-timeout','15','--max-time','120','--retry','0','--max-filesize',str(item['bytes']),
            '--output',item['path'],'--write-out','M17_BODY_BYTES=%{size_download}\\n',item['url']],W,125)
        commands[-1].update(network={'method':'GET','exactUrl':item['url'],'maxAttempts':1,'maxResponseBytes':item['bytes'],
            'redirects':0},validation=['exact archive bytes/SHA256; SHA512 for package','no retry or partial-file promotion'])
    phases=[('M-runner-generation','M','runner',False),('M-tests-generation','M','tests',False),
            ('M-tests-verification','M','tests',True),('B-pass-1','B','runner',True),
            ('B-pass-2','B','runner',True),('B-official-tests','B','tests',True)]
    for name,stage,graph,locked in phases:
        root=phase_root(name);project=root/'source'/m['graphs'][graph]['root']
        argv=[DOTNET,'msbuild',project,'-target:Restore','-maxCpuCount:1','-nodeReuse:false','-noAutoResponse','-verbosity:minimal',
            '-property:RestoreConfigFile='+str(root/'NuGet.Config'),'-property:RestorePackagesPath='+str(root/'env/packages'),
            '-property:RestoreSources='+str(W/'feed'),'-property:RestoreFallbackFolders=',
            '-property:RestoreDisableParallel=true','-property:RestoreNoHttpCache=true','-property:RestoreIgnoreFailedSources=false',
            '-property:RestorePackagesWithLockFile=true','-property:RestoreLockedMode='+str(locked).lower(),'-property:NuGetAudit=false']
        add(name+'-restore',stage,argv,root/'source',180,0)
        commands[-1]['env']=phase_env(name)
        commands[-1]['validation']+=['validate_restore: complete project/edge set, package bytes and lock-content hash',
            'all selected IDs/versions within exact 31-package union','fresh local cache; no fallback',
            'locked-mode marker true' if locked else 'new locks generated; never pretend to reuse lost locks']
        if stage=='M':continue
        verb='build' if graph=='tests' else 'publish'
        argv=[DOTNET,verb,project,'-c','Release','--no-restore','--disable-build-servers',
            '-p:ContinuousIntegrationBuild=true','-p:Deterministic=true','-p:UseSharedCompilation=false',
            '-maxCpuCount:1','-nodeReuse:false','-p:PathMap='+str(root/'source')+'=/_/luban-v5.1.0']
        if graph=='runner':argv+=['-o',root/'runner']
        add(name+'-'+verb,'B',argv,root/'source',600,128,depends=[name+'-restore'])
        commands[-1]['env']=phase_env(name)
        if graph=='tests':
            add(name+'-test','B',[DOTNET,'test',project,'-c','Release','--no-restore','--no-build','--disable-build-servers',
                '--logger','trx;LogFileName=official-tests.trx','--results-directory',root/'results'],root/'source',600,128,
                depends=[name+'-build'])
            commands[-1]['env']=phase_env(name)
            commands[-1]['validation']+=['all unchanged official tests, no filter','TRX completion/zero failed/zero skipped; timeout is failure',
                'CsharpCompileHarness retained; generated consumer NuGet.Config inside isolated TMPDIR']
    sides={'oracle':TOOL/'runner','candidate-1':phase_root('B-pass-1')/'runner','candidate-2':phase_root('B-pass-2')/'runner'}
    for scenario,conf,target,kind,expected in [
        ('yaml',M/'source-patched/tests/fixtures/loaders/yml/luban.conf','all','yaml',0),
        ('unity-asset',M/'harness/unity-asset/luban.conf','all','json',0),
        ('malformed-yaml',M/'harness/malformed-yaml/luban.conf','all','json','nonzero'),
        ('fightmatch',REPO/'Config/FightMatch/Luban/luban.conf','client','json',0)]:
        for side,runner in sides.items():
            name='B-'+scenario+'-'+side;out=W/'B/controls'/scenario/side
            add(name,'B',[DOTNET,runner/'Luban.dll','--conf',conf,'-t',target,'-d',kind,'--strict','--errorFormat','json',
                '-l',runner/'nlog.xml','-x','outputDataDir='+str(out)],REPO,300,0,expected,
                ['B-pass-1-publish','B-pass-2-publish'])
            commands[-1]['outputRoot']=str(out)
    preflight=Path(m['preflight005'])
    for side,runner in sides.items():
        destination=W/'B/controls/005'/side
        for old in strict(reader(preflight/'commands.json'))['commands']:
            if str(DOTNET) not in old['argv']:continue
            argv=old['argv'][old['argv'].index(str(DOTNET)):]
            argv=[a.replace(str(preflight),str(destination)).replace(str(TOOL/'runner'),str(runner)) for a in argv]
            add('B-005-'+side+'-'+old['name'],'B',argv,REPO,300,0,old['exitCode'],
                ['B-pass-1-publish','B-pass-2-publish'])
    # L remains a separate accepted-B downstream stage. It is not LOC-IMPL-B.
    baseline=Path(m['localizationBaseline']);new=W/'L'
    for old in strict(reader(baseline/'commands.json'))['commands']:
        if old['name'] not in ('compiler-compile','compiler-self-tests','negative-duplicate','negative-parameters',
                              'negative-severity','compile-pass-1','compile-pass-2'):continue
        argv=[str(DOTNET)]
        for value in old['argumentList']:
            a=value.replace(str(baseline),str(new)).replace(str(baseline.relative_to(REPO)),str(new))
            if a=='Tools/FightMatch.Localization.Compiler/Program.cs':a=str(new/'source/Program.cs')
            argv.append(a)
        add('L-'+old['name'],'L',argv,REPO,300,0,old.get('exitCode',0))
        commands[-1]['requires']='independent B acceptance pin; private L identity adaptation separately scoped/checked before compile'
    for n in (1,2):
        runner=phase_root('B-pass-1')/'runner'
        add('L-luban-'+str(n),'L',[DOTNET,runner/'Luban.dll','--conf',REPO/'Config/FightMatch/Luban/luban.conf',
            '-t','client','-d','json','--strict','--errorFormat','json','-l',runner/'nlog.xml',
            '-x','outputDataDir='+str(new/f'pass-{n}/raw')],REPO,300)
    for scenario,kind,expected in [('yaml','yaml',0),('malformed-yaml','json','nonzero')]:
        conf=M/('source-patched/tests/fixtures/loaders/yml/luban.conf' if scenario=='yaml' else 'harness/malformed-yaml/luban.conf')
        runner=phase_root('B-pass-1')/'runner';out=new/'controls'/scenario
        add('L-'+scenario,'L',[DOTNET,runner/'Luban.dll','--conf',conf,'-t','all','-d',kind,'--strict','--errorFormat','json',
            '-l',runner/'nlog.xml','-x','outputDataDir='+str(out)],REPO,300,0,expected)
        commands[-1]['outputRoot']=str(out)
    for control in ('accepted-b','tampered-yaml','missing-yaml-license','changed-fallback-license'):
        runner=phase_root('B-pass-1')/'runner' if control=='accepted-b' else new/'controls'/control/'runner'
        add('L-'+control,'L',['/opt/homebrew/bin/python3','-B',DRIVER,'check-b','--receipt',W/'receipts/b-acceptance.json',
            '--runner',runner,'--expect','pass' if control=='accepted-b' else 'reject'],REPO,60,0,0 if control=='accepted-b' else 20)
    for stage in ('B','L'):
        depends=[c['id'] for c in commands if c['stage']==stage]
        add(stage+'-assert-controls',stage,['/opt/homebrew/bin/python3','-B',DRIVER,'assert-controls','--stage',stage],REPO,60,0,0,depends)
        commands[-1]['role']='harness'
    return commands

def generated_contract(m):
    result=[]
    for name,graph,locked in [('M-runner-generation','runner',False),('M-tests-generation','tests',False),
                              ('M-tests-verification','tests',True),('B-pass-1','runner',True),
                              ('B-pass-2','runner',True),('B-official-tests','tests',True)]:
        for project in m['graphs'][graph]['nodes']:
            parent=phase_root(name)/'source'/Path(project).parent
            for rel,role in [('packages.lock.json','lock'),('obj/project.assets.json','assets')]:
                result.append({'path':str(parent/rel),'role':role,'phase':name,'graph':graph,'locked':locked,
                    'validator':'validate_restore + compare_restore + 31-package union; NuGet contentHash is not signed catalog SHA512'})
    return result

def write_plan(m):
    rules=[]
    def add(path,stage,writer,files=1,size=16777216,purpose='generated exact leaf',pattern=False):
        path=Path(path)
        root=next((key for key,value in ROOTS.items() if path.is_relative_to(value)),None)
        require(root is not None,'declared write outside roots')
        rel=path.relative_to(ROOTS[root]).as_posix()
        if not pattern:safe(rel)
        rules.append([root,rel,'bounded-pattern' if pattern else 'exact',stage,writer,files,size,purpose])
    for rel in PREP:add(E/rel,'SOURCE','prepare',size=16*1024**2,purpose='only current eight source leaves')
    for pkg in m['packages']:
        if pkg['class']=='official-get':add(Path(pkg['path']),'M','M-get-'+pkg['key'],size=pkg['bytes'],purpose='fixed official archive')
        add(W/'feed'/(pkg['key']+'.nupkg'),'M','stage-inputs',size=pkg['bytes'],purpose='exact verified package copy')
        add(M/'packages'/(pkg['key']+'.nupkg'),'M','seal-material',size=pkg['bytes'],purpose='31-package closed material feed')
    add(Path(m['archive']['path']),'M','M-get-yaml-source',size=m['archive']['bytes'],purpose='fixed commit-bound source tar')
    for body in m['licenses']:
        if body['kind']=='generated':add(Path(body['path']),'M','derive-license',size=body['bytes'],purpose='exact pinned archive entry, not a new license choice')
        add(M/body['output'],'M','seal-material',size=body['bytes'],purpose='full license/NOTICE body')
    for src in m['sources']:
        for prefix in ('source-official','source-patched'):
            add(M/prefix/src['relativePath'],'M','stage-inputs',size=max(src['bytes'],4096)+64,purpose='fixed official source or approved two-reference patch')
    phases=['M-runner-generation','M-tests-generation','M-tests-verification','B-pass-1','B-pass-2','B-official-tests']
    for name in phases:
        stage=name[0];root=phase_root(name)
        for src in m['sources']:add(root/'source'/src['relativePath'],stage,'stage-'+name,size=max(src['bytes'],4096)+64,purpose='fresh fixed patched source')
        add(root/'NuGet.Config',stage,'stage-'+name,size=4096,purpose='clear all sources/fallbacks; exact local feed')
        graph='runner' if name in ('M-runner-generation','B-pass-1','B-pass-2') else 'tests'
        for project in m['graphs'][graph]['nodes']:
            parent=root/'source'/Path(project).parent
            if name not in ('M-runner-generation','M-tests-generation'):
                add(parent/'packages.lock.json',stage,'stage-'+name,size=16*1024**2,purpose='byte-identical lock seed from verified generation')
            for rel in ('packages.lock.json','obj/project.assets.json','obj/project.nuget.cache',
                        'obj/'+Path(project).name+'.nuget.dgspec.json','obj/'+Path(project).name+'.nuget.g.props',
                        'obj/'+Path(project).name+'.nuget.g.targets'):
                add(parent/rel,stage,name+'-restore',size=16*1024**2,purpose='exact project restore output')
            if stage=='B':
                build_writer=name+('-build' if graph=='tests' else '-publish')
                add(parent/'obj/Release/net8.0/{safe-leaf}',stage,build_writer,files=2000,size=128*1024**2,
                    purpose='finite build outputs under this project/configuration',pattern=True)
                add(parent/'bin/Release/net8.0/{safe-leaf}',stage,build_writer,files=5000,size=256*1024**2,
                    purpose='finite build outputs under this project/configuration',pattern=True)
        for pkg in m['packages']:
            add(root/'env/packages'/pkg['id'].lower()/pkg['version']/'{safe-leaf}',stage,name+'-restore',files=5000,size=128*1024**2,
                purpose='only entries of this fixed archive plus named NuGet metadata; enumerate and hash after acquisition before any restore',pattern=True)
        for folder,count,size in [('dotnet-home',128,8*1024**2),('http',32,1024**2),('plugins',32,1024**2),
                                  ('scratch',512,8*1024**2),('tmp',20000,512*1024**2)]:
            add(root/'env'/folder/'{safe-leaf}',stage,name,files=count,size=size,
                    purpose='isolated SDK bookkeeping; exact derived manifest/refined patterns required by host activation',pattern=True)
        if name in ('B-pass-1','B-pass-2'):add(root/'runner/{safe-leaf}','B',name+'-publish',files=5000,size=512*1024**2,
            purpose='complete runtime deployment from validated assets, no three-file subset',pattern=True)
        if name=='B-official-tests':add(root/'results/official-tests.trx','B',name+'-test',size=32*1024**2)
    add(M/'source-yamldotnet/{safe-leaf}','M','stage-yaml-source',files=10000,size=32*1024**2,
        purpose='only safe regular entries of pinned tar with exact commit; derive closed entry set before writes',pattern=True)
    for name in ('manifest.json','packages.json','licenses.json','graphs.json','expected-deployment.json','commands.json','validation.json'):
        add(M/'identity'/name,'M','seal-material',size=16*1024**2)
    add(M/'identity/patch.diff','M','seal-material',size=PATCH_ID[0])
    for graph in ('runner','tests'):
        for project in m['graphs'][graph]['nodes']:
            for name in ('packages.lock.json','project.assets.json','canonical-assets.json'):
                add(M/'identity'/graph/Path(project).parent/name,'M','seal-material',size=16*1024**2)
    for fixture in ('unity-asset','malformed-yaml'):
        for rel in ('luban.conf','Defines/schema.xml','Data/item.asset' if fixture=='unity-asset' else 'Data/item.yml'):
            add(M/'harness'/fixture/rel,'M','stage-control-fixtures',size=1048576)
    for side in ('oracle','candidate-1','candidate-2'):
        add(W/'B/controls/005'/side/'{safe-leaf}','B','stage-005-and-direct-DUTs',files=1000,size=32*1024**2,
            purpose='fixed 005 scratch and its declared direct .NET outputs only',pattern=True)
        for scenario in ('yaml','unity-asset','fightmatch','malformed-yaml'):
            add(W/'B/controls'/scenario/side/'{safe-leaf}','B','B-'+scenario+'-'+side,files=256,size=16*1024**2,
                purpose='bounded actual outputs; malformed YAML must publish zero leaves',pattern=True)
    add(W/'L/{safe-leaf}','L','independently-authorized-L',files=2000,size=64*1024**2,
        purpose='private compiler identity adapter + fixed negative fixtures + canonical outputs; accepted-B pin first',pattern=True)
    for command in m['commands']:
        if not any(command['env']==phase_env(n) for n in phases):
            for folder in ('dotnet-home','packages','http','plugins','scratch','tmp'):
                add(phase_root(command['id'])/'env'/folder/'{safe-leaf}',command['stage'],command['id'],files=512,size=8*1024**2,
                    purpose='isolated command environment; no HOME replacement',pattern=True)
        for key,size in [('receiptPath',65536),('stdoutPath',4194304),('stderrPath',4194304)]:
            add(Path(command[key]),command['stage'],command['id'],size=size,purpose='actual bounded process evidence')
    for name in ('activation.json','acquisition.json','material-validation.json','b-acceptance.json','l-acceptance.json','root-equality.json'):
        add(W/'receipts'/name,'M/B/L','trusted-host',size=16*1024**2)
    # A path can have multiple stage roles but not contradictory concrete rules.
    unique={tuple(r):r for r in rules}
    return sorted(unique.values())

def authorize_write(m,path,stage,writer):
    p=Path(path);require(p.is_absolute() and '..' not in p.parts,'unsafe output path')
    matches=[]
    for root,rel,kind,role,owner,files,size,purpose in m['writes']:
        if role!=stage or owner!=writer:continue
        if kind=='exact':hit=p==ROOTS[root]/rel
        else:
            prefix=rel.removesuffix('{safe-leaf}')
            parent=ROOTS[root]/prefix
            hit=p.is_relative_to(parent)
            if hit:safe(p.relative_to(parent).as_posix())
        if hit:matches.append((files,size))
    require(len(matches)==1,'write not authorized for exact stage/writer: '+str(p))
    return matches[0]

def validate_model(m):
    require(m['owner']==OWNER and m['roots']=={k:str(v) for k,v in ROOTS.items()},'owner/root drift')
    require(len(m['packages'])==31 and len({p['key'] for p in m['packages']})==31,'missing package closure')
    require(len(m['sources'])==931 and sum(r['changed'] for r in m['sources'])==2,'source/patch set drift')
    require(all(p['licenses'] and p['sha512'] and p['url'] for p in m['packages']),'package identity/license relation missing')
    for row in m['inputs']:
        require(row[0] in ('existing','official-get','generated'),'unknown input class')
        require(not row[2].startswith('/private/tmp/'),'historical temporary input leaked into build')
        if Path(row[2]).is_relative_to(E):require(Path(row[2])==DRIVER,'self-hash cycle')
    for command in m['commands']:
        require(command['stage'] in ('M','B','L') and command['maxStarts']==1,'stage/start drift')
        require(not set(command['env']['updates']).intersection(('HOME','home','CODEX_HOME')),'protected environment replacement')
        require(all('/private/tmp/' not in value for value in command['argv']),'old temp argument leaked')
        require(command['cwd'].startswith(str(REPO)) or command['cwd'].startswith(str(W)),'cwd outside declared roots')
    for graph,count,edges in [('runner',23,56),('tests',13,29)]:
        require(len(m['graphs'][graph]['nodes'])==count and len(m['graphs'][graph]['edges'])==edges,'project dependency graph drift')
    return True

def canonical_assets(assets,phase):
    """Normalize only isolated phase paths and the explicit locked-mode flag."""
    value=copy.deepcopy(assets)
    restore=value['project']['restore']
    marker=restore.get('restoreLockProperties',{})
    require(set(marker).issubset({'restorePackagesWithLockFile','restoreLockedMode'}),'unknown lock properties')
    require(marker.get('restorePackagesWithLockFile') in (True,'true'),'lock generation not enabled')
    locked=marker.get('restoreLockedMode',False)
    require(locked in (True,False,'true','false'),'invalid lock marker')
    marker['restoreLockedMode']='NORMALIZED_LOCK_MODE_ONLY'
    prefix=str(phase_root(phase))+'/'
    def visit(item):
        if isinstance(item,dict):
            pairs=[(visit(k),visit(v)) for k,v in item.items()]
            require(len({k for k,v in pairs})==len(pairs),'normalized key collision')
            return dict(pairs)
        if isinstance(item,list):return [visit(v) for v in item]
        if isinstance(item,str):
            require('/private/tmp/' not in item,'historical tmp in generated assets')
            return '/M17_PHASE/'+item[len(prefix):] if item.startswith(prefix) else item
        return item
    return jb(visit(value)),locked in (True,'true')

def verify_archive(item,data):
    check_bytes(data,(item['bytes'],item['sha256']),'archive '+item.get('key','yaml-source'))
    if 'sha512' in item:
        require(base64.b64encode(hashlib.sha512(data).digest()).decode()==item['sha512'],'archive catalog SHA512')
    return True

def validate_restore(m,phase,graph,files,installed):
    """Pure post-restore checker. Both mappings must contain actual read bytes.

    installed maps (lowercase ID/version) to (signed nupkg bytes, NuGet .sha512
    sidecar bytes). The two hash domains are deliberately never interchanged.
    """
    expected=m['graphs'][graph];require(set(files)==set(expected['nodes']),'complete project lock/assets set required')
    allowed={p['id'].lower()+'/'+p['version']:p for p in m['packages']}
    result={};seen=set();edges=set()
    locked_expected=phase not in ('M-runner-generation','M-tests-generation')
    for project in expected['nodes']:
        lock_raw,assets_raw=files[project];lock=strict(lock_raw);assets=strict(assets_raw)
        require(set(lock['dependencies'])=={'net8.0'},'unexpected lock frameworks')
        require(set(assets['targets'])=={'net8.0'},'unexpected assets framework/RID')
        for log in assets.get('logs',[]):require(log.get('level','').lower() not in ('error','warning'),'restore diagnostic')
        entries=lock['dependencies']['net8.0'];selected={}
        for name,entry in entries.items():
            if entry['type'].lower()=='project':continue
            key=name.lower()+'/'+entry['resolved'];require(key in allowed,'lock package outside 31 closure')
            require(key in installed,'missing installed package')
            archive,sidecar=installed[key];verify_archive(allowed[key],archive)
            content=sidecar.decode('ascii').strip()
            require(len(base64.b64decode(content,validate=True))==64 and entry['contentHash']==content,'lock NuGet contentHash mismatch')
            selected[key]=content;seen.add(key)
        for direct in expected['direct'][project]:
            match=[v for k,v in entries.items() if k.lower()==direct['id'].lower()]
            require(len(match)==1 and match[0]['type']=='Direct','missing direct package reference')
            # Fixed sources use plain exact authored versions; the resolved
            # closed feed must not silently upgrade them.
            require(match[0]['resolved']==direct['requested'],'direct version changed')
        libraries=assets['libraries'];asset_packages={}
        for key,entry in libraries.items():
            if entry['type']=='package':
                normalized=key.lower().split('/')[0]+'/'+key.split('/')[1]
                require(normalized in selected,'assets package absent from lock')
                require(entry['sha512']==selected[normalized],'assets/lock contentHash differs')
                asset_packages[normalized]=entry
        require(set(asset_packages)==set(selected),'assets/lock package set differs')
        target_packages={k.lower().split('/')[0]+'/'+k.split('/')[1] for k,v in assets['targets']['net8.0'].items() if v['type']=='package'}
        require(target_packages==set(selected),'target package closure differs')
        restore=assets['project']['restore'];root=phase_root(phase)
        require(restore['projectPath']==str(root/'source'/project),'assets project path drift')
        require(restore['packagesPath']==str(root/'env/packages'),'assets package path drift')
        require(set(restore['sources'])=={str(W/'feed')},'unexpected NuGet source')
        require(set(assets['packageFolders'])=={str(root/'env/packages')+'/'},'unexpected fallback package folder')
        refs=restore['frameworks']['net8.0']['projectReferences']
        for path,entry in refs.items():
            p=Path(path);require(p.is_relative_to(root/'source'),'project reference escape')
            child=p.relative_to(root/'source').as_posix();safe(child)
            require(child in expected['nodes'] and entry['projectPath']==path,'unknown reference')
            edges.add((project,child))
        canonical,locked=canonical_assets(assets,phase)
        require(locked==locked_expected,'restore locked-mode mismatch')
        result[project]={'lock':lock_raw,'canonical':canonical,'assets':assets,'packages':sorted(selected)}
    require(edges=={tuple(e) for e in expected['edges']},'restored dependency edges differ')
    return {'projects':result,'packages':sorted(seen),'graph':graph}

def compare_restore(generation,verification):
    require(generation['graph']==verification['graph'] and generation['packages']==verification['packages'],'restore closure drift')
    require(set(generation['projects'])==set(verification['projects']),'restore project drift')
    for project,g in generation['projects'].items():
        v=verification['projects'][project]
        require(g['lock']==v['lock'] and g['canonical']==v['canonical'],'lock/canonical assets changed: '+project)
    return True

def complete_package_union(m,runner,tests):
    require(set(runner['packages'])|set(tests['packages'])=={p['id'].lower()+'/'+p['version'] for p in m['packages']},
            'runner/test union must account for all 31 fixed packages')
    return True

def deployment_from_assets(m,validated,entry_bytes,source_bytes):
    """Derive full expected deployment before observing candidate output.

    entry_bytes is the complete safe entry map read from each hash-verified
    archive by the future host; it is not a candidate supplied file subset.
    Unknown RID/content rules fail closed and need an explicit scoped adapter.
    """
    require(validated['graph']=='runner','runner graph required')
    graph=m['graphs']['runner'];require(set(validated['projects'])==set(graph['nodes']),'deployment project closure missing')
    packages={p['id'].lower()+'/'+p['version']:p for p in m['packages']}
    runner={};licenses={};owners=set()
    def add(path,value):
        safe(path);require(path not in runner or runner[path]==value,'deployment collision')
        runner[path]=value
    for project in graph['nodes']:
        tree=ET.fromstring(source_bytes[project]);assembly=Path(project).stem
        require(not tree.findall('.//AssemblyName'),'unmodeled assembly-name override')
        for suffix in ('.dll','.pdb'):add(assembly+suffix,None)
        for item in tree.iter():
            copied=item.find('CopyToOutputDirectory')
            if copied is None or copied.text not in ('Always','PreserveNewest'):continue
            authored=item.attrib.get('Update',item.attrib.get('Include','')).replace('\\','/')
            require(authored and not any(c in authored for c in '*?;$') and not item.findall('Link'),'unmodeled project content')
            rel=posixpath.normpath(str(Path(project).parent)+'/'+authored);safe(rel)
            # SDK None Update of a nonexistent file is a no-op, not content.
            if rel in source_bytes:add(safe(authored),(len(source_bytes[rel]),sha(source_bytes[rel])))
    for name in ('Luban','Luban.deps.json','Luban.runtimeconfig.json'):add(name,None)
    root_assets=validated['projects'][graph['root']]['assets']
    for key,entry in root_assets['targets']['net8.0'].items():
        if entry['type']!='package':continue
        normalized=key.split('/')[0].lower()+'/'+key.split('/')[1]
        require(normalized in packages and normalized in entry_bytes,'deployment archive missing')
        owners.add(normalized)
        require(not entry.get('runtimeTargets') and not entry.get('contentFiles'),'unmodeled RID/content deployment')
        for kind in ('runtime','native','resource'):
            for rel,attributes in entry.get(kind,{}).items():
                safe(rel)
                if Path(rel).name=='_._':continue
                require(rel in entry_bytes[normalized],'selected archive entry absent')
                b=entry_bytes[normalized][rel]
                target=(attributes['locale']+'/' if kind=='resource' else '')+Path(rel).name
                add(target,(len(b),sha(b)))
    require(owners==set(validated['packages']),'root runner package closure incomplete')
    # Keep ALL fixed terms, including fallback/source/SDK notices; no minimal
    # license subset inferred from only the three executable entry points.
    for body in m['licenses']:
        rel=safe(body['output']);value=(body['bytes'],body['sha256'])
        require(rel not in licenses or licenses[rel]==value,'license target collision');licenses[rel]=value
    require(licenses and not set(runner)&set(licenses),'deployment/license collision')
    result={'runnerFiles':runner,'licenseFiles':licenses,
            'packageKeys':[key.split('/') for key in sorted(owners)]}
    result['sha256']=sha(jb(result))
    return result

def control_groups():
    groups=[];sides=('oracle','candidate-1','candidate-2')
    for scenario in ('yaml','unity-asset','fightmatch'):
        groups.append({'id':scenario,'format':'yaml-fixture' if scenario=='yaml' else 'json',
            'outputs':{s:{'rawRoot':str(W/'B/controls'/scenario/s)} for s in sides}})
    for n in (1,2):
        groups.append({'id':'005-pass-'+str(n),'format':'json','outputs':{s:{
            'rawRoot':str(W/'B/controls/005'/s/f'pass-{n}/raw'),
            'canonicalPath':str(W/'B/controls/005'/s/f'pass-{n}/canonical.json'),
            'semanticDigestPath':str(W/'B/controls/005'/s/f'pass-{n}/semantic-digest.txt')} for s in sides}})
    return groups

def validate_process(command,receipt,stdout,stderr,enumerate_files):
    require(receipt.get('role')=='DUT' and receipt.get('argv')==command['argv'] and
            receipt.get('cwd')==command['cwd'] and receipt.get('env')==command['env'],'DUT process identity drift')
    require(receipt.get('quiescent') is True and receipt.get('timedOut') is False and
            receipt.get('startCount')==1 and type(receipt.get('childStarts')) is int and
            0<=receipt['childStarts']<=command['maxChildren'],'DUT process completion/budget')
    require(0<=receipt.get('elapsedSeconds',-1)<=command['timeoutSeconds'],'DUT time budget')
    require(len(stdout)<=command['stdoutMaxBytes'] and len(stderr)<=command['stderrMaxBytes'],'DUT log budget')
    actual=receipt.get('exitCode');require(type(actual) is int,'DUT exit missing')
    require(actual!=0 if command['expectedExit']=='nonzero' else actual==command['expectedExit'],'DUT exit differs')
    if 'malformed-yaml' in command['id']:
        require(stderr and not enumerate_files(Path(command['outputRoot'])),'malformed diagnostic missing or output published')
    markers={'L-negative-duplicate':b'FMLOC002','L-negative-parameters':b'FMLOC007','L-negative-severity':b'FMLOC004'}
    if command['id'] in markers:require(markers[command['id']] in stdout+stderr,'negative diagnostic differs')
    return True

SELF_TEST_NAMES={'schema-column-count','duplicate-key','empty-translation','invalid-severity','missing-parameter',
    'multiple-parameters','unclosed-template','unknown-raw-column','sorting','newlines','round-trip','duplicate-generation'}

def validate_l_outputs(reader,enumerate_files):
    root=W/'L';self_tests=strict(reader(root/'self-tests/results.json'))
    require(self_tests.get('status')=='PASS' and self_tests.get('total')==12 and self_tests.get('passed')==12,'actual 12 self-tests required')
    tests=self_tests['tests'];require(len(tests)==12 and {t['name'] for t in tests}==SELF_TEST_NAMES and
        all(t['passed'] is True for t in tests),'self-test names/results differ')
    for name,marker in [('duplicate','FMLOC002'),('parameters','FMLOC007'),('severity','FMLOC004')]:
        result=strict(reader(root/'negative'/name/'result.json'))
        require(result.get('caseName')==name and result.get('status')=='PASS' and result.get('expectedDiagnostic')==marker and
                result.get('observedDiagnostic')==marker,'actual negative result differs')
        require(result.get('publishedCanonicalCount')==0 and set(enumerate_files(root/'negative'/name))=={'result.json'},'negative canonical publication')
    left=root/'pass-1';right=root/'pass-2'
    for folder,leaves in [('raw',{'fm-text-v1.json'}),('canonical',{'fm-text-v1.json','fm-text-v1.manifest.json'})]:
        require(set(enumerate_files(left/folder))==set(enumerate_files(right/folder))==leaves,'L complete raw/canonical set required')
        for rel in sorted(leaves):
            a=reader(left/folder/rel);b=reader(right/folder/rel)
            require(a==b and strict(a)==strict(b),'L actual bytes/semantics differ')
    return True

def validate_official_trx(data):
    tree=ET.fromstring(data);ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
    counters=tree.find('.//t:Counters',ns);results=tree.findall('.//t:UnitTestResult',ns)
    require(counters is not None and int(counters.attrib['total'])>0,'official test discovery missing')
    total=int(counters.attrib['total'])
    require(int(counters.attrib['executed'])==int(counters.attrib['passed'])==total and
            all(int(counters.attrib.get(k,0))==0 for k in ('failed','error','timeout','aborted','notExecuted','inconclusive')),'official tests incomplete')
    require(len(results)==total and all(r.attrib['outcome']=='Passed' for r in results),'official test result set differs')
    return True

def frozen_material_json(name,reader=read):
    manifest_bytes=reader(M/'identity/manifest.json');manifest=strict(manifest_bytes)
    path=M/'identity'/safe(name);rows=[r for r in manifest['filesExcludingThisManifest'] if r['path']==str(path)]
    require(len(rows)==1,'material leaf absent/duplicated');r=rows[0]
    return strict(check_bytes(reader(path),(r['bytes'],r['sha256']),'material leaf')),sha(manifest_bytes)

def check_b_receipt(receipt_path,runner,reader=read,enumerate_files=None):
    deployment,manifest_sha=frozen_material_json('expected-deployment.json',reader)
    for key in ('runnerFiles','licenseFiles'):
        deployment[key]={rel:None if value is None else tuple(value) for rel,value in deployment[key].items()}
    helper=helper_functions(reader)
    if enumerate_files is None:enumerate_files=lambda root:[p.relative_to(root).as_posix() for p in root.rglob('*') if p.is_file()]
    return helper['check_b_data'](strict(reader(receipt_path)),runner,reader,enumerate_files,manifest_sha,deployment)

def assert_controls(stage,reader=read,enumerate_files=None):
    contract,_=frozen_material_json('commands.json',reader)
    if enumerate_files is None:enumerate_files=lambda root:[p.relative_to(root).as_posix() for p in root.rglob('*') if p.is_file()]
    require(contract['comparisons']==control_groups(),'control comparison scope changed')
    checked=[]
    for command in contract['commands']:
        if command['stage']!=stage or command['role']!='DUT':continue
        validate_process(command,strict(reader(Path(command['receiptPath']))),reader(Path(command['stdoutPath'])),
            reader(Path(command['stderrPath'])),enumerate_files)
        checked.append(command['id'])
    if stage=='B':
        helper_functions(reader)['compare_control_outputs'](contract['comparisons'],reader,enumerate_files)
        validate_official_trx(reader(phase_root('B-official-tests')/'results/official-tests.trx'))
    elif stage=='L':validate_l_outputs(reader,enumerate_files)
    else:raise GateError('unsupported control stage')
    return {'stage':stage,'actualDutReceiptsChecked':checked,'status':'CONTROL_GATES_PASSED_NOT_INDEPENDENT_ACCEPTANCE'}

def download_contract(m):
    rows=sorted([{k:p[k] for k in ('path','url','bytes','sha256')} for p in [*m['packages'],m['archive']]
        if p['class']=='official-get'],key=lambda p:p['path'])
    require(len(rows)==18 and sum(r['bytes'] for r in rows)==25172764 and
        sha(jb(rows))=='c4f17bfb0e405cd2bc43e4ad1c3537374dff338feab58c9ace7743ca643f2de3','fixed 18-archive contract drift')
    return rows

class MStage:
    def __init__(self,clock=time.monotonic):
        self.clock=clock;self.started=clock();self.work=self.started+870;self.end=self.started+900
        self.closing=False;self.arm=lambda seconds:None;self.gets={};self.bodyBytes=0;self.allowed={}
    def left(self,cleanup=False):
        value=(self.end if cleanup else self.work)-self.clock()
        require(value>0 and (cleanup or not self.closing),'M stage deadline exhausted');return value
    def close(self):
        self.closing=True;self.arm(self.left(True))
    def wait(self,limit,runner,stop=None,cleanup=False):
        try:
            require(limit>0,'command deadline exhausted')
            value=runner(min(limit,self.left(cleanup)));self.left(cleanup);return value
        except BaseException:
            self.close()
            if stop:stop(self)
            raise
    def begin_get(self,command):
        self.left();path=command['argv'][command['argv'].index('--output')+1];row=self.allowed.get(path)
        require(command['stage']=='M' and row and command['argv'][-1]==row['url'] and path not in self.gets,'unlisted/repeated GET')
        require(self.bodyBytes+row['bytes']<=25172764,'aggregate archive body budget');self.gets[path]={'bodyBytes':None,'savedBytes':0,'expected':row}
    def end_get(self,command,stdout,saved,success):
        path=command['argv'][command['argv'].index('--output')+1];record=self.gets[path]
        require(not record.get('accounted'),'GET body counted twice');record['accounted']=True
        matches=re.findall(rb'^M17_BODY_BYTES=([0-9]+)\r?$',stdout,re.M)
        record['savedBytes']=saved;record['bodyBytes']=int(matches[0]) if len(matches)==1 else None
        if record['bodyBytes'] is not None:self.bodyBytes+=record['bodyBytes']
        require(record['bodyBytes'] is not None and saved<=record['bodyBytes']<=self.allowed[path]['bytes']
            and self.bodyBytes<=25172764,'missing/over-limit actual archive body measurement')
        if success:require(saved==record['bodyBytes']==self.allowed[path]['bytes'],'incomplete archive response body')

def consume_m_stage(stage,work,finish,watchdog=True):
    # The production signal covers CPU work, hashing, decoding and blocking I/O,
    # not only checkpoints. Tests replace clock/runner, never the production pin.
    import signal
    old=None;result=None;failure=None
    def alarm(signum,frame):raise GateError('M stage deadline interrupt')
    if watchdog:
        require(signal.getitimer(signal.ITIMER_REAL)==(0.0,0.0),'existing host timer must not be replaced')
        old=signal.signal(signal.SIGALRM,alarm);stage.arm=lambda seconds:signal.setitimer(signal.ITIMER_REAL,seconds)
    try:
        stage.arm(stage.left())
        try:result=work(stage);stage.left()
        except BaseException as error:failure=type(error).__name__+': '+str(error)
        stage.close();finish(stage,result,failure);stage.left(True)
    finally:
        if old is not None:signal.setitimer(signal.ITIMER_REAL,0);signal.signal(signal.SIGALRM,old)
    require(failure is None,'M stopped: '+str(failure));return result

def execution_budget(m):
    return {'maxConcurrentTrees':1,'M':{'wallSeconds':900,'workSeconds':870,'cleanupReserveSeconds':30,
        'officialGets':download_contract(m),'maxArchiveResponseBodyBytes':25172764,
        'runnerGenerationRestores':1,'testsGenerationRestores':1,'testsVerificationRestores':1,
        'explicitDeltaFromM16':'runner restore 0 -> 1 to regenerate the lost runner locks from fixed source/feed'},
        'B':{'runnerLockedRestores':2,'testsLockedRestores':1,'publishes':2,'officialBuilds':1,'unfilteredOfficialTests':1},
        'L':{'requiresIndependentBAcceptance':True,'compilerSelfTests':12,'cleanCanonicalPasses':2},
        'commands':{c['id']:{k:c[k] for k in ('timeoutSeconds','killGraceSeconds','maxStarts','maxChildren','stdoutMaxBytes','stderrMaxBytes')} for c in m['commands']},
        'maxCumulativeCommandSeconds':sum(c['timeoutSeconds']+c['killGraceSeconds'] for c in m['commands']),
        'sourceMaxBytes':16*1024**2,'driverMaxBytes':256*1024,'materialMaxBytes':512*1024**2,
        'materialMaxFiles':25000,'workMaxBytes':4*1024**3,'workMaxFiles':200000,'freeFloorBytes':2*1024**3}

def verify_activation(pin,record_bytes,caller,freeze_bytes,m,argv):
    """Pure seam only. A caller cannot install the production trust pin."""
    require(isinstance(pin,dict) and Path(pin.get('path','')).is_absolute(),'missing independent central pin')
    check_bytes(record_bytes,(pin['bytes'],pin['sha256']),'central record')
    record=strict(record_bytes);require(caller==record,'caller cannot replace central record')
    for key in ('centralThread','centralHost','centralTurn','centralItem','ownerThread','ownerHost','ownerTurn'):
        require(record.get(key)==pin.get(key) and isinstance(pin.get(key),str) and pin[key],'central identity missing/different')
    require(record['centralThread']==CENTRAL and record['centralHost']=='local','wrong central authority')
    require(record['ownerThread']==OWNER['threadId'] and record['ownerHost']=='local','wrong execution owner')
    for key in ('centralTurn','centralItem','ownerTurn'):
        require(re.fullmatch(r'[0-9a-f]{8}(?:-[0-9a-f]{4}){3}-[0-9a-f]{12}',record[key]),'missing real message identity')
    require(record.get('scope')=='EXECUTE_M17' and record.get('argv')==argv,'activation scope/argv')
    require(record.get('sourceFreezeSha256')==sha(freeze_bytes) and record.get('modelSha256')==sha(jb(m)), 'activation source/model')
    require(record.get('driverSha256')==next(r[4] for r in m['inputs'] if r[2]==str(DRIVER)), 'activation driver')
    require(record.get('budgets')==execution_budget(m),'activation budget')
    require(record.get('reviewStatus')=='COMPLETED_NO_UNRESOLVED_FINDINGS' and
            re.fullmatch('[0-9a-f]{40}',record.get('reviewedHead','')) and
            record['reviewedHead']==record.get('executionHead'),'review gate')
    require(record.get('stage')=='M','M-only activation; B/L require independent execution')
    require(record.get('hostExecutorSha256')==record['driverSha256'] and
            record.get('refinedWriteSetSha256')==sha(source_tables(m)['source/m17-write-set.tsv']),'host executor/refined write gate')
    if record['stage']!='M':require(record.get('independentMaterialReceiptSha256'),'material acceptance missing')
    if record['stage']=='L':require(record.get('independentBReceiptSha256'),'independent B acceptance missing')
    return True

def execute(argv,reader=read):
    # Must precede stdin, caller-selected paths, source consumption, external
    # commands or network. This reviewed SOURCE_ONLY snapshot is never a host.
    stage=MStage();state={}
    require(TRUSTED_CENTRAL_ACTIVATION is not None,'BLOCKED_SOURCE_ONLY_NO_TRUSTED_CENTRAL_ACTIVATION')
    def work(stage):
        pin=TRUSTED_CENTRAL_ACTIVATION;frozen=reader(E/'authority/source-freeze.json')
        m=consume_source(frozen,reader);record_bytes=reader(Path(pin['path']))
        verify_activation(pin,record_bytes,strict(record_bytes),frozen,m,argv);state['m']=m
        stage.allowed={r['path']:r for r in download_contract(m)};stage.left()
        return execute_material(m,reader,frozen,argv,stage)
    def finish(stage,result,failure):
        report={'status':'M_FAILED' if failure else 'M_CANDIDATE_NOT_ACCEPTED','failure':failure,'result':result,
            'elapsedSeconds':stage.clock()-stage.started,'gets':stage.gets,'measuredArchiveBodyBytes':stage.bodyBytes,
            'bodyMeasurementComplete':all(r['bodyBytes'] is not None for r in stage.gets.values()),'tlsProtocolTrafficMeasured':False}
        if 'm' in state:MaterialWriter(state['m'],stage).put(W/'receipts/material-validation.json',jb(report),'M/B/L','trusted-host',cleanup=True)
        print(json.dumps(report,ensure_ascii=False))
    return consume_m_stage(stage,work,finish)

def source_tables(m):
    return {'source/m17-inputs.tsv':tsv(INPUT_HEADER,m['inputs']),
            'source/m17-write-set.tsv':tsv(WRITE_HEADER,m['writes'])}

def archive_entries(pkg,data):
    """Future M-only memory decoding, never called by SOURCE prepare."""
    import zipfile
    verify_archive(pkg,data);result={};total=0;folded=set();aliases={}
    if (pkg.get('key'),pkg['bytes'],pkg['sha256'])==('messagepackanalyzer.3.1.7',178005,'2566ef3915962caddb7063cf606ae06021650284f619192fc44b9a2f03f0f4ee'):
        aliases={'analyzers/roslyn4.3/cs//MessagePack.Analyzers.CodeFixes.dll':'analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll',
                 'analyzers/roslyn4.3/cs//MessagePack.SourceGenerator.dll':'analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll'}
    with zipfile.ZipFile(io.BytesIO(data)) as archive:
        for entry in archive.infolist():
            if entry.is_dir():continue
            rel=safe(aliases.get(entry.filename,entry.filename))
            require(rel.casefold() not in folded and len(result)<5000,'duplicate/oversized archive entry set')
            require((entry.external_attr>>16)&0o170000 in (0,0o100000),'nonregular package entry')
            total+=entry.file_size;require(total<=128*1024**2,'package expansion budget')
            b=archive.read(entry);require(len(b)==entry.file_size,'package length differs')
            result[rel]=b;folded.add(rel.casefold())
    require(result,'empty archive')
    return result

def yaml_entries(item,data):
    import tarfile
    verify_archive(item,data);result={};total=0
    with tarfile.open(fileobj=io.BytesIO(data),mode='r:gz') as archive:
        require(archive.pax_headers.get('comment')==item['commit'],'YAML archive commit differs')
        for entry in archive.getmembers():
            require(entry.isdir() or entry.isfile(),'nonregular YAML source entry')
            if entry.isdir():continue
            require('/' in entry.name,'YAML archive root missing')
            rel=safe(entry.name.split('/',1)[1]);total+=entry.size
            require(rel not in result and len(result)<10000 and total<=32*1024**2,'YAML archive budget/duplicate')
            b=archive.extractfile(entry).read();require(len(b)==entry.size,'YAML entry length')
            result[rel]=b
    require(result,'empty YAML source')
    return result

class MaterialWriter:
    """Only used after trusted M activation. Never used in offline fixtures."""
    def __init__(self,m,stage):self.m=m;self.outputs={};self.stage=stage
    def path(self,path):
        p=Path(path);require(p.is_absolute() and '..' not in p.parts,'write path escape')
        require(not any(q.is_symlink() for q in (p,*p.parents)),'write symlink')
        return p
    def put(self,path,b,stage,writer,cleanup=False):
        self.stage.left(cleanup)
        p=self.path(path);count,maximum=authorize_write(self.m,p,stage,writer)
        require(len(b)<=maximum and not p.exists(),'write budget/existing leaf: '+str(p))
        p.parent.mkdir(parents=True,exist_ok=True)
        with p.open('xb') as stream:stream.write(b)
        self.outputs[str(p)]=(len(b),sha(b));self.budget(cleanup)
    def budget(self,cleanup=False):
        import shutil
        bounds=execution_budget(self.m)
        self.stage.left(cleanup)
        for root,files,size in [(M,bounds['materialMaxFiles'],bounds['materialMaxBytes']),
                                 (W,bounds['workMaxFiles'],bounds['workMaxBytes'])]:
            if not root.exists():continue
            paths=list(root.rglob('*'));require(not any(p.is_symlink() for p in paths),'generated symlink')
            leaves=[p for p in paths if p.is_file()]
            require(len(leaves)<=files and sum(p.stat().st_size for p in leaves)<=size,'aggregate root budget')
        require(shutil.disk_usage(W.parent).free>=bounds['freeFloorBytes'],'free space floor')

def command_write_rules(m,command):
    names={command['id']}
    if command['id'].endswith('-restore'):names.add(command['id'].removesuffix('-restore'))
    return [r for r in m['writes'] if r[3]==command['stage'] and r[4] in names]

def child_sandbox(rules,network):
    """No child process forks for M curl/restore commands; B is separate."""
    lines=['(version 1)','(allow default)','(deny file-write*)','(deny process-fork)']
    if network=='OS denied; local feed only':lines.append('(deny network*)')
    directories=set()
    for root,rel,kind,stage,writer,files,size,purpose in rules:
        p=ROOTS[root]/rel.removesuffix('{safe-leaf}')
        if kind=='exact':lines.append('(allow file-write* (literal '+json.dumps(str(p))+'))');parent=p.parent
        else:lines.append('(allow file-write* (regex '+json.dumps('^'+re.escape(str(p).rstrip('/'))+'/')+'))');parent=p
        while parent.is_relative_to(ROOTS[root]):directories.add(str(parent));parent=parent.parent
    for directory in sorted(directories):
        lines.append('(allow file-write* (require-all (vnode-type DIRECTORY) (literal '+json.dumps(directory)+')))')
    return '\n'.join(lines)

def install_sandbox(profile):
    import ctypes
    lib=ctypes.CDLL('/usr/lib/libsandbox.dylib');error=ctypes.c_char_p()
    lib.sandbox_init.argtypes=[ctypes.c_char_p,ctypes.c_uint64,ctypes.POINTER(ctypes.c_char_p)]
    if lib.sandbox_init(profile.encode(),0,ctypes.byref(error))!=0:os._exit(126)

def inspect_command_outputs(rules):
    """Fresh actual file enumeration against each finite rule and quota."""
    for root,rel,kind,stage,writer,files,size,purpose in rules:
        path=ROOTS[root]/rel.removesuffix('{safe-leaf}')
        leaves=([path] if path.is_file() else []) if kind=='exact' else ([p for p in path.rglob('*') if p.is_file()] if path.exists() else [])
        require(len(leaves)<=files and sum(p.stat().st_size for p in leaves)<=size,'command write quota exceeded')
        require(not any(p.is_symlink() for p in leaves),'command created symlink')

def run_material_command(m,command,writer,frozen,argv):
    """Future pinned M path only: bounded streams, no shell, no retry."""
    import selectors
    import signal
    import subprocess
    stage=writer.stage;stage.left()
    require(TRUSTED_CENTRAL_ACTIVATION is not None,'closed source-only command gate')
    pin=TRUSTED_CENTRAL_ACTIVATION;record=read(Path(pin['path']))
    verify_activation(pin,record,strict(record),frozen,m,argv)
    require(command['stage']=='M' and command['maxChildren']==0,'only M executor implemented here')
    require(command in m['commands'],'command differs from frozen M plan')
    require(command['id'] not in writer.outputs,'command retry forbidden')
    rules=command_write_rules(m,command);profile=child_sandbox(rules,command['network'])
    for root,rel,kind,rule_stage,owner,files,size,purpose in rules:
        path=ROOTS[root]/rel.removesuffix('{safe-leaf}')
        writer.path(path.parent if kind=='exact' else path).mkdir(parents=True,exist_ok=True)
    env=dict(os.environ);env.update(command['env']['updates'])
    env['PATH']=str(SDK)+os.pathsep+os.environ.get('PATH','')
    for key in command['env']['preserve']:require(env.get(key)==os.environ.get(key),'protected environment drift')
    for key in ('DOTNET_CLI_HOME','NUGET_PACKAGES','NUGET_HTTP_CACHE_PATH','NUGET_PLUGINS_CACHE_PATH','NUGET_SCRATCH','TMPDIR'):
        writer.path(env[key]).mkdir(parents=True,exist_ok=True)
    started=stage.clock();deadline=started+command['timeoutSeconds'];streams={'stdout':bytearray(),'stderr':bytearray()};process=None;failure=None
    selector=selectors.DefaultSelector();exit_code=None
    def stop_owned(s):
        if process is not None and process.poll() is None:
            os.killpg(process.pid,signal.SIGTERM)
            try:s.wait(command['killGraceSeconds'],lambda seconds:process.wait(timeout=seconds),cleanup=True)
            except subprocess.TimeoutExpired:os.killpg(process.pid,signal.SIGKILL);s.wait(5,lambda seconds:process.wait(timeout=seconds),cleanup=True)
    try:
        stage.left()
        if isinstance(command['network'],dict):stage.begin_get(command)
        process=subprocess.Popen(command['argv'],cwd=command['cwd'],env=env,stdin=subprocess.DEVNULL,
            stdout=subprocess.PIPE,stderr=subprocess.PIPE,start_new_session=True,preexec_fn=lambda:install_sandbox(profile))
        for label,stream in [('stdout',process.stdout),('stderr',process.stderr)]:
            os.set_blocking(stream.fileno(),False);selector.register(stream,selectors.EVENT_READ,label)
        budget_time=started
        while selector.get_map():
            require(stage.clock()<deadline,'process timeout');stage.left()
            if time.monotonic()-budget_time>=1:
                inspect_command_outputs(rules);writer.budget();budget_time=time.monotonic()
            for key,_ in stage.wait(min(.1,deadline-stage.clock()),selector.select,stop_owned):
                chunk=os.read(key.fd,65536)
                if not chunk:selector.unregister(key.fileobj);key.fileobj.close();continue
                limit=command[key.data+'MaxBytes'];room=limit-len(streams[key.data])
                streams[key.data].extend(chunk[:room]);require(len(chunk)<=room,'process log cap')
        exit_code=stage.wait(deadline-stage.clock(),lambda seconds:process.wait(timeout=seconds),stop_owned)
        require(exit_code==command['expectedExit'],'process exit failure')
        inspect_command_outputs(rules);writer.budget()
    except Exception as error:failure=type(error).__name__+': '+str(error)
    finally:
        if failure:stage.close()
        stop_owned(stage)
        selector.close()
    if process is not None:exit_code=process.returncode
    if isinstance(command['network'],dict):
        path=Path(command['argv'][command['argv'].index('--output')+1])
        try:stage.end_get(command,bytes(streams['stdout']),path.stat().st_size if path.exists() else 0,failure is None)
        except Exception as error:failure=(failure+'; ' if failure else '')+str(error);stage.close()
    receipt={'role':'DUT','argv':command['argv'],'cwd':command['cwd'],'env':command['env'],
        'exitCode':exit_code,'startCount':int(process is not None),'childStarts':0,
        'childrenDeniedBySandbox':True,'quiescent':process is not None and process.poll() is not None,
        'timedOut':failure is not None and any(w in failure.lower() for w in ('timeout','deadline')),'elapsedSeconds':time.monotonic()-started,
        'failure':failure,'sandboxProfileSha256':sha(profile.encode())}
    for key,data in [('stdoutPath',bytes(streams['stdout'])),('stderrPath',bytes(streams['stderr'])),('receiptPath',jb(receipt))]:
        writer.put(Path(command[key]),data,'M',command['id'],cleanup=bool(failure))
    writer.outputs[command['id']]=(0,'attempted')
    require(failure is None,'material command failed; no retry: '+str(failure))
    validate_process(command,receipt,bytes(streams['stdout']),bytes(streams['stderr']),lambda root:[])
    return receipt

def execute_material(m,reader,frozen,argv,stage):
    """Guarded future M implementation; B/L only remain frozen contracts."""
    require(TRUSTED_CENTRAL_ACTIVATION is not None,'closed M gate')
    require(not M.exists(),'new material root must be absent')
    allowed_existing={Path(p['path']) for p in [*m['packages'],m['archive']] if p['class']=='official-get'}
    require(not W.exists() or {p for p in W.rglob('*') if p.is_file()}.issubset(allowed_existing),'work root contamination')
    before=old_seals(reader)
    for kind,role,path,count,digest,authority in m['inputs']:
        if kind=='existing':check_bytes(reader(Path(path)),(int(count),digest),'fresh complete existing input')
    for item in [*m['packages'],m['archive']]:
        p=Path(item['path'])
        if item['class']=='official-get' and (p.exists() or p.is_symlink()):verify_archive(item,reader(p))
    stage.left();writer=MaterialWriter(m,stage);archive_data={};entry_maps={}
    for item in [*m['packages'],m['archive']]:
        p=Path(item['path'])
        if not p.exists():
            command=next(c for c in m['commands'] if c['id']=='M-get-'+item.get('key','yaml-source'))
            W.mkdir(parents=True,exist_ok=True)
            run_material_command(m,command,writer,frozen,argv)
        b=reader(p);verify_archive(item,b);archive_data[item.get('key','yaml-source')]=b
        if 'key' in item:entry_maps[item['id'].lower()+'/'+item['version']]=archive_entries(item,b)
    yaml=yaml_entries(m['archive'],archive_data['yaml-source'])
    source={};official={}
    for row in m['sources']:
        rel=row['relativePath'];b=reader(SOURCE/rel);official[rel]=b
        if row['changed']:b=b.replace(b'<PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />',b'<PackageReference Include="YamlDotNet" Version="16.3.0" />')
        check_bytes(b,(row['bytes'],row['sha256']),'staged source');source[rel]=b
        for prefix,data in [('source-official',official[rel]),('source-patched',b)]:writer.put(M/prefix/rel,data,'M','stage-inputs')
    for rel,b in yaml.items():writer.put(M/'source-yamldotnet'/rel,b,'M','stage-yaml-source')
    for pkg in m['packages']:
        b=archive_data[pkg['key']]
        writer.put(W/'feed'/(pkg['key']+'.nupkg'),b,'M','stage-inputs')
        writer.put(M/'packages'/(pkg['key']+'.nupkg'),b,'M','seal-material')
    for body in m['licenses']:
        if body['kind']=='generated':
            pkg=next(p for p in m['packages'] if p['key']==body['archiveKey'])
            b=entry_maps[pkg['id'].lower()+'/'+pkg['version']][body['entry']]
            check_bytes(b,(body['bytes'],body['sha256']),'derived full terms')
            writer.put(Path(body['path']),b,'M','derive-license')
        else:b=reader(Path(body['path']))
        check_bytes(b,(body['bytes'],body['sha256']),'full license body')
        if not (M/body['output']).exists():writer.put(M/body['output'],b,'M','seal-material')
    fixture='tests/fixtures/loaders/yml/'
    conf=source[fixture+'luban.conf'];schema=source[fixture+'Defines/schema.xml'];data=source[fixture+'Data/item.yml'].decode('utf-8-sig')
    for kind in ('unity-asset','malformed-yaml'):
        dest=M/'harness'/kind
        writer.put(dest/'luban.conf',conf,'M','stage-control-fixtures')
        writer.put(dest/'Defines/schema.xml',schema.replace(b'item.yml',b'item.asset') if kind=='unity-asset' else schema,'M','stage-control-fixtures')
        body=('MonoBehaviour:\n'+''.join('  '+line+'\n' for line in data.splitlines())).encode() if kind=='unity-asset' else b'[unterminated\n'
        writer.put(dest/('Data/item.asset' if kind=='unity-asset' else 'Data/item.yml'),body,'M','stage-control-fixtures')
    restored={};receipts=[]
    for phase,graph in [('M-runner-generation','runner'),('M-tests-generation','tests'),('M-tests-verification','tests')]:
        root=phase_root(phase)
        for rel,b in source.items():writer.put(root/'source'/rel,b,'M','stage-'+phase)
        config=('<?xml version="1.0" encoding="utf-8"?><configuration><packageSources><clear/><add key="m17-local" value="'+str(W/'feed')+'"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders><disabledPackageSources><clear/></disabledPackageSources></configuration>\n').encode()
        writer.put(root/'NuGet.Config',config,'M','stage-'+phase)
        if phase=='M-tests-verification':
            for project,info in restored['M-tests-generation']['projects'].items():
                writer.put(root/'source'/Path(project).parent/'packages.lock.json',info['lock'],'M','stage-'+phase)
        command=next(c for c in m['commands'] if c['id']==phase+'-restore')
        receipts.append(run_material_command(m,command,writer,frozen,argv))
        files={};installed={}
        for project in m['graphs'][graph]['nodes']:
            parent=root/'source'/Path(project).parent
            files[project]=(reader(parent/'packages.lock.json'),reader(parent/'obj/project.assets.json'))
        for pkg in m['packages']:
            path=root/'env/packages'/pkg['id'].lower()/pkg['version']/(pkg['key']+'.nupkg')
            if path.is_file():installed[pkg['id'].lower()+'/'+pkg['version']]=(reader(path),reader(Path(str(path)+'.sha512')))
        restored[phase]=validate_restore(m,phase,graph,files,installed)
    compare_restore(restored['M-tests-generation'],restored['M-tests-verification'])
    complete_package_union(m,restored['M-runner-generation'],restored['M-tests-generation'])
    deployment=deployment_from_assets(m,restored['M-runner-generation'],entry_maps,source)
    for role,phase in [('runner','M-runner-generation'),('tests','M-tests-generation')]:
        for project,item in restored[phase]['projects'].items():
            for name,data in [('packages.lock.json',item['lock']),('project.assets.json',jb(item['assets'])),('canonical-assets.json',item['canonical'])]:
                writer.put(M/'identity'/role/Path(project).parent/name,data,'M','seal-material')
    writer.put(M/'identity/patch.diff',reader(PATCH),'M','seal-material')
    for name,value in [('packages.json',m['packages']),('licenses.json',m['licenses']),('graphs.json',m['graphs']),
                       ('expected-deployment.json',deployment),('commands.json',{'commands':m['commands'],'comparisons':control_groups()}),
                       ('validation.json',{'stage':'M_MATERIAL_CANDIDATE','notIndependentAcceptance':True,'restores':receipts,
                            'sourceFreezeSha256':sha(frozen),'old38Leaves':before,'B':'NOT_RUN','L':'NOT_RUN'})]:
        writer.put(M/'identity'/name,jb(value),'M','seal-material')
    manifest=[identity(p,reader(p)) for p in sorted(M.rglob('*')) if p.is_file()]
    writer.put(M/'identity/manifest.json',jb({'filesExcludingThisManifest':manifest,'sourceFreezeSha256':sha(frozen),
        'status':'M_MATERIAL_CANDIDATE_NOT_ACCEPTED','independentReviewRequired':True}),'M','seal-material')
    require(old_seals(reader)==before,'historical evidence changed')
    writer.budget()
    return {'status':'M_MATERIAL_CANDIDATE_NOT_ACCEPTED','manifest':identity(M/'identity/manifest.json'),'B':'NOT_RUN','L':'NOT_RUN'}

def source_artifacts(m,driver_bytes,old_driver,checks,history):
    result={'source/m17_driver.py':driver_bytes,**source_tables(m)}
    patch=''.join(difflib.unified_diff(old_driver.decode().splitlines(True),driver_bytes.decode().splitlines(True),
        fromfile='M17-FIX02/source/m17_driver.py',tofile='M17-FIX03/source/m17_driver.py'))
    require(sum(line.startswith(('+','-')) and not line.startswith(('+++','---')) for line in patch.splitlines())<=160,'FIX03 source delta exceeds 160 lines')
    result['source/source.patch']=patch.encode()
    result['authority/prepare.json']=jb({'schemaVersion':1,'owner':OWNER,'issuer':ISSUER,'dispatchItem':DISPATCH,
        'modelSha256':sha(jb(m)),'model':m,'executionBudget':execution_budget(m),
        'inputClasses':{k:sum(r[0]==k for r in m['inputs']) for k in ('existing','official-get','generated')},
        'old38Leaves':history,'states':m['states'],
        'limitations':['No archive acquired or unpacked to disk; only existing ZIPs decoded in memory; no material roots created.',
            'Generated locks/assets have no hashes until actual isolated restore and independent comparison.',
            'Existing SDK leaf identities are declared; fresh full SDK hashing is a required pre-launch gate.',
            'Future M executor is present but unexecuted; its production trust pin is null. B/L are separately owned frozen command contracts.',
            'Package cache patterns are fixed to 31 IDs/versions; safe archive entries and per-command quotas are checked before/after future writes.']})
    result['authority/offline-checks.json']=jb(checks)
    result['authority/source-receipt.json']=jb({'status':'SOURCE_READY',
        'owner':OWNER,'issuer':ISSUER,'dispatchItem':DISPATCH,'modelSha256':sha(jb(m)),
        'sourceReady':True,'materialReady':False,'reviewReady':False,'executionAuthorized':False,'MOnlyBudget':execution_budget(m)['M'],
        'integrationVerdict':'NOT_ISSUED_AUTHOR_CANNOT_ACCEPT','historyPreserved':history,'validation':checks,
        'returnPath':ISSUER,'pendingArchives':[{k:p[k] for k in ('path','bytes','sha256','url')} for p in [*m['packages'],m['archive']] if p['class']=='official-get'],
        'generatedLocksAssets':len(m['generated']),
        'blockingSlot':'fixed missing archives + actual M restore/comparison + new reviewed head + independent central execution pin; B/L separate execution owners'})
    return result

def seal_source(m,artifacts):
    require(set(artifacts)==set(PREP)-{'authority/source-freeze.json'},'source seven-leaf seal set')
    require(len(artifacts['source/m17_driver.py'])<=256*1024,'driver budget')
    seal={'schemaVersion':1,'sourceReady':True,'executorComplete':True,'materialWriteSetComplete':True,
        'status':'SOURCE_READY','owner':OWNER,'issuer':ISSUER,
        'modelSha256':sha(jb(m)),'filesExcludingThisSeal':[identity(E/rel,b) for rel,b in sorted(artifacts.items())],
        'executionAuthorized':False,'productionTrustPin':None,'layering':'fixed inputs + driver -> model/tables -> seven artifacts -> seal; no backward generated dependency'}
    frozen=jb(seal);require(sum(map(len,artifacts.values()))+len(frozen)<=16*1024**2,'eight-leaf evidence budget')
    return frozen

def consume_source(freeze_bytes,reader=read,expected_model=None):
    seal=strict(freeze_bytes);paths={str(E/rel) for rel in PREP if rel!='authority/source-freeze.json'}
    require(seal.get('owner')==OWNER and seal.get('sourceReady') is True and seal.get('executorComplete') is True and
            seal.get('materialWriteSetComplete') is True and seal.get('productionTrustPin') is None,'source seal state/owner')
    rows=seal['filesExcludingThisSeal'];require(len(rows)==7 and {r['path'] for r in rows}==paths,'sealed file set differs')
    artifacts={}
    for row in rows:
        path=Path(row['path']);artifacts[path.relative_to(E).as_posix()]=check_bytes(reader(path),(row['bytes'],row['sha256']),'sealed source leaf')
    prepared=strict(artifacts['authority/prepare.json']);m=prepared['model']
    validate_model(m);require(prepared['modelSha256']==seal['modelSha256']==sha(jb(m)),'model seal mismatch')
    fresh=model(reader) if expected_model is None else expected_model
    require(jb(m)==jb(fresh),'generated model differs from consumed fixed basis')
    for name,b in source_tables(fresh).items():require(artifacts[name]==b,'generated/consumed table byte mismatch')
    require(prepared['executionBudget']==execution_budget(m),'execution budget drift')
    return m

def memory_tree(data):
    return lambda root:sorted(p.relative_to(root).as_posix() for p in data if p.is_relative_to(root))

def restore_fixture(phase):
    """Small, explicitly synthetic fixture of production lock/asset parsing."""
    archive=b'SYNTHETIC_SIGNED_ARCHIVE';content=base64.b64encode(bytes(range(64))).decode()
    pkg={'id':'YamlDotNet','version':'16.3.0','key':'yamldotnet.16.3.0','bytes':len(archive),'sha256':sha(archive),
         'sha512':base64.b64encode(hashlib.sha512(archive).digest()).decode()}
    project='src/Luban/Luban.csproj';child='src/Luban.Core/Luban.Core.csproj';nodes=[project,child]
    graph={'root':project,'nodes':nodes,'edges':[[project,child]],'direct':{project:[{'id':'YamlDotNet','requested':'16.3.0'}],child:[]}}
    files={};root=phase_root(phase)
    for p in nodes:
        lock={'dependencies':{'net8.0':{'YamlDotNet':{'type':'Direct' if p==project else 'Transitive','resolved':'16.3.0','contentHash':content}}}}
        assets={'targets':{'net8.0':{'YamlDotNet/16.3.0':{'type':'package','runtime':{'lib/net8.0/YamlDotNet.dll':{}}}}},
            'libraries':{'YamlDotNet/16.3.0':{'type':'package','sha512':content}},'packageFolders':{str(root/'env/packages')+'/':{}},
            'project':{'restore':{'projectPath':str(root/'source'/p),'packagesPath':str(root/'env/packages'),
                'sources':{str(W/'feed'):{}},'frameworks':{'net8.0':{'projectReferences':
                    {str(root/'source'/child):{'projectPath':str(root/'source'/child)}} if p==project else {}}},
                'restoreLockProperties':{'restorePackagesWithLockFile':True,'restoreLockedMode':phase!='M-runner-generation'}}}}
        files[p]=(jb(lock),jb(assets))
    license_bytes=b'SYNTHETIC full yaml license';fallback=b'SYNTHETIC full fallback license'
    m={'packages':[pkg],'graphs':{'runner':graph},'licenses':[
        {'output':'licenses/YamlDotNet/LICENSE','bytes':len(license_bytes),'sha256':sha(license_bytes)},
        {'output':'licenses/fallback/LICENSE','bytes':len(fallback),'sha256':sha(fallback)}]}
    source={project:b'<Project><ItemGroup><None Update="nlog.xml"><CopyToOutputDirectory>Always</CopyToOutputDirectory></None></ItemGroup></Project>',
            child:b'<Project/>','src/Luban/nlog.xml':b'<nlog/>'}
    return m,files,{'yamldotnet/16.3.0':(archive,content.encode())},source

def budget_checks(m,case):
    def scenario(elapsed,phase='work',wait=False):
        tick=[0.0];stage=MStage(lambda:tick[0]);events=[];receipts=[]
        def work(s):
            tick[0]=elapsed;events.append(phase)
            if wait:
                def runner(seconds):events.append(('allowedWait',seconds));tick[0]+=seconds
                s.wait(180,runner,lambda s:events.append('owned-termination'))
            else:s.left()
            events.append('next-action');return 'candidate'
        try:consume_m_stage(stage,work,lambda s,r,e:receipts.append({'failure':e,'elapsed':tick[0]}),watchdog=False)
        except GateError:pass
        return stage,events,receipts
    case('M normal same production consumer',lambda:require(scenario(1)[2]==[{'failure':None,'elapsed':1}],'normal stage'))
    case('M prior elapsed time is deducted',lambda:require(('allowedWait',50.0) in scenario(820,wait=True)[1],'clock reset/uncapped wait'))
    for elapsed,label in [(870,'exact work deadline'),(871,'input verification timeout'),(880,'in-memory decode timeout')]:
        def check(t=elapsed,n=label):
            s,e,r=scenario(t,n);require('next-action' not in e and r and r[0]['failure'],'deadline did not stop/retain failure')
        case('M '+label,check)
    def stopped():
        s,e,r=scenario(820,wait=True)
        require('owned-termination' in e and 'next-action' not in e and r[0]['failure'] and r[0]['elapsed']==870,'child deadline/receipt')
    case('M child wait capped, terminated, failure retained, no next command',stopped)
    stage=MStage(lambda:901);stage.started=0;stage.work=870;stage.end=900
    case('original 901-second MaterialWriter regression',lambda:MaterialWriter(m,stage).budget(),True)
    case('M hard deadline has no unlimited cleanup grace',lambda:stage.left(True),True)
    tick=[0.0];cleanup=MStage(lambda:tick[0]);tick[0]=870;cleanup.close()
    case('M failure receipt reserve survives work deadline',lambda:require(cleanup.left(True)==30,'cleanup reserve missing'))
    case('M no new work once cleanup begins',lambda:cleanup.left(),True)
    fixed=download_contract(m);commands=[c for c in m['commands'] if c['id'].startswith('M-get-')]
    def downloads():
        s=MStage(lambda:0);s.allowed={r['path']:r for r in fixed}
        for c in commands:
            s.begin_get(c);n=s.allowed[c['argv'][c['argv'].index('--output')+1]]['bytes']
            s.end_get(c,('M17_BODY_BYTES='+str(n)+'\n').encode(),n,True)
        require(len(s.gets)==18 and s.bodyBytes==25172764,'exact actual body total');return s
    case('18 fixed GETs actual archive body total synthetic',downloads)
    case('GET retry rejected',lambda:downloads().begin_get(commands[0]),True)
    cache=MStage(lambda:0);cache.allowed={r['path']:r for r in fixed}
    case('cache hits may reduce GET count to zero',lambda:require(cache.bodyBytes==0 and not cache.gets,'cache charged as transfer'))
    bad=copy.deepcopy(m);bad['archive']['bytes']+=1
    case('18-archive size/hash contract cannot expand',lambda:download_contract(bad),True)
    changed=copy.deepcopy(m);changed['archive']['url']+='?replacement'
    case('fixed official URL cannot change',lambda:download_contract(changed),True)
    c=commands[0];s=MStage(lambda:0);s.allowed={r['path']:r for r in fixed};s.begin_get(c)
    case('archive response aggregate overflow',lambda:s.end_get(c,b'M17_BODY_BYTES=25172765\n',25172765,True),True)
    s2=MStage(lambda:0);s2.allowed=s.allowed;s2.begin_get(c)
    case('partial transfer cannot fabricate complete body measurement',lambda:s2.end_get(c,b'',123,False),True)
    case('unmeasured partial body retained as lower bound',lambda:require(s2.gets[next(iter(s2.gets))]['savedBytes']==123 and s2.bodyBytes==0,'partial response lost'))
    case('B action cannot borrow M GET budget',lambda:MStage(lambda:0).begin_get(dict(c,stage='B')),True)

def offline_checks(m,cached,history):
    results=[]
    def case(name,action,reject=False):
        started=time.monotonic();error=None
        try:action()
        except (GateError,KeyError,ValueError,OSError,TypeError) as exc:error=type(exc).__name__+': '+str(exc)
        passed=(error is not None)==reject
        results.append({'name':name,'expected':'REJECT' if reject else 'PASS','passed':passed,'rawError':error,
                        'elapsedSeconds':round(time.monotonic()-started,6)})
    reader=lambda p:cached[Path(p)]
    case('actual model repeated byte equality',lambda:require(jb(model(reader))==jb(m),'model nondeterminism'))
    driver=reader(DRIVER);old=reader(OLD17F1/'source/m17_driver.py')
    artifacts=source_artifacts(m,driver,old,{'fixture':'not actual execution'},history)
    frozen=seal_source(m,artifacts)
    data={E/rel:b for rel,b in artifacts.items()}
    case('actual producer to actual consumer',lambda:consume_source(frozen,lambda p:data[p],m))
    case('actual producer deterministic seal',lambda:require(frozen==seal_source(m,source_artifacts(m,driver,old,{'fixture':'not actual execution'},history)),'seal nondeterminism'))
    for rel in ('source/m17_driver.py','source/m17-inputs.tsv','source/m17-write-set.tsv'):
        changed=dict(data);changed[E/rel]+=b'changed'
        case('sealed-byte tamper '+rel,lambda d=changed:consume_source(frozen,lambda p:d[p],m),True)
    changed=dict(artifacts);changed['source/m17-inputs.tsv']+=b'forged\n'
    resealed=seal_source(m,changed)
    case('rehashed table cannot replace deterministic table',lambda:consume_source(resealed,lambda p:changed[p.relative_to(E).as_posix()],m),True)
    bad=copy.deepcopy(m);bad['packages'].pop()
    case('missing dependency closure',lambda:validate_model(bad),True)
    badtmp=copy.deepcopy(m);badtmp['inputs'][0][2]='/private/tmp/obsolete/input'
    case('obsolete tmp cannot reenter model',lambda:validate_model(badtmp),True)
    case('write outside roots',lambda:authorize_write(m,REPO/'Assets/forbidden.cs','M','stage-inputs'),True)
    case('write wrong owner',lambda:authorize_write(m,M/'packages/x.nupkg','L','stage-inputs'),True)
    case('allowed exact evidence write',lambda:authorize_write(m,DRIVER,'SOURCE','prepare'))
    source_path=next(Path(r[2]) for r in m['inputs'] if r[1]=='official-source')
    modified=dict(cached);modified[source_path]+=b'changed'
    case('fixed source drift',lambda:model(lambda p:modified[Path(p)]),True)
    archive=b'synthetic';item={'bytes':len(archive),'sha256':sha(archive),'sha512':base64.b64encode(hashlib.sha512(archive).digest()).decode()}
    case('fixed archive domains',lambda:verify_archive(item,archive))
    case('archive bytes changed',lambda:verify_archive(item,archive+b'x'),True)
    item2=dict(item,sha512=base64.b64encode(bytes(64)).decode())
    case('archive catalog hash changed',lambda:verify_archive(item2,archive),True)
    a,files,installed,source=restore_fixture('M-runner-generation')
    generation=validate_restore(a,'M-runner-generation','runner',files,installed)
    a2,files2,installed2,_=restore_fixture('B-pass-1')
    verified=validate_restore(a2,'B-pass-1','runner',files2,installed2)
    case('independent synthetic phase normalize/compare',lambda:compare_restore(generation,verified))
    missing=dict(files);missing.pop(a['graphs']['runner']['nodes'][1])
    case('missing graph lock',lambda:validate_restore(a,'M-runner-generation','runner',missing,installed),True)
    corrupted=copy.deepcopy(files);project=a['graphs']['runner']['root'];lock=strict(corrupted[project][0])
    lock['dependencies']['net8.0']['YamlDotNet']['contentHash']=a['packages'][0]['sha512'];corrupted[project]=(jb(lock),corrupted[project][1])
    case('signed catalog hash cannot replace NuGet content hash',lambda:validate_restore(a,'M-runner-generation','runner',corrupted,installed),True)
    asset_bad=copy.deepcopy(files);assets=strict(asset_bad[project][1]);assets['project']['restore']['sources']={'https://unlisted.invalid':{}}
    asset_bad[project]=(asset_bad[project][0],jb(assets))
    case('unexpected restore network source',lambda:validate_restore(a,'M-runner-generation','runner',asset_bad,installed),True)
    case('missing installed dependency',lambda:validate_restore(a,'M-runner-generation','runner',files,{}),True)
    drift=copy.deepcopy(verified);drift['projects'][project]['lock']+=b' '
    case('raw lock mismatch despite possible semantic equality',lambda:compare_restore(generation,drift),True)
    case('complete fixed package union synthetic',lambda:complete_package_union(a,generation,verified))
    missing_union=dict(generation,packages=[])
    case('missing union dependency',lambda:complete_package_union(a,missing_union,missing_union),True)
    entries={'yamldotnet/16.3.0':{'lib/net8.0/YamlDotNet.dll':b'SYNTHETIC DLL'}}
    deployment=deployment_from_assets(a,generation,entries,source)
    case('full deployment includes project/resource/licenses',lambda:require(
        {'Luban.dll','Luban.Core.dll','nlog.xml','YamlDotNet.dll'}.issubset(deployment['runnerFiles']) and len(deployment['licenseFiles'])==2,'incomplete deployment'))
    case('missing selected runtime archive entry',lambda:deployment_from_assets(a,generation,{'yamldotnet/16.3.0':{}},source),True)
    helper=helper_functions(reader);runner=Path('/SYNTHETIC_M17/runner');tree={}
    for rel,value in deployment['runnerFiles'].items():
        tree[runner/rel]=entries['yamldotnet/16.3.0']['lib/net8.0/YamlDotNet.dll'] if rel=='YamlDotNet.dll' else source['src/Luban/nlog.xml'] if rel=='nlog.xml' else ('SYNTHETIC '+rel).encode()
    tree[runner/'licenses/YamlDotNet/LICENSE']=b'SYNTHETIC full yaml license'
    tree[runner/'licenses/fallback/LICENSE']=b'SYNTHETIC full fallback license'
    def file_rows(names):return [{'relativePath':n,'bytes':len(tree[runner/n]),'sha256':sha(tree[runner/n])} for n in names]
    receipt={'status':'B-ACCEPTED','materialManifestSha256':'d'*64,'reviewedHead':'a'*40,'authorThread':'SYNTHETIC_AUTHOR',
        'deploymentContractSha256':deployment['sha256'],'runnerPackageKeys':deployment['packageKeys'],
        'runnerFiles':file_rows(deployment['runnerFiles']),'licenseFiles':file_rows(deployment['licenseFiles']),
        'fallbackLicenseFiles':['licenses/fallback/LICENSE'],
        'independentAcceptance':{'verdict':'ACCEPT','ownerThread':'SYNTHETIC_REVIEWER','reviewedHead':'a'*40,'receiptPath':'/SYNTHETIC_M17/accept.json'}}
    independent={k:receipt[k] for k in ('materialManifestSha256','reviewedHead','deploymentContractSha256','runnerPackageKeys')}
    independent.update(verdict='ACCEPT',runnerFilesSha256=helper['file_map_digest'](helper['receipt_file_map'](receipt['runnerFiles'])),
        licenseFilesSha256=helper['file_map_digest'](helper['receipt_file_map'](receipt['licenseFiles'])))
    tree[Path('/SYNTHETIC_M17/accept.json')]=jb(independent);receipt['independentAcceptance']['receiptSha256']=sha(jb(independent))
    def check_deploy(t):return helper['check_b_data'](receipt,runner,lambda p:t[p],memory_tree(t),'d'*64,deployment)
    case('complete deployment actual byte gate synthetic',lambda:check_deploy(tree))
    for rel in ('Luban.Core.dll','nlog.xml','licenses/fallback/LICENSE'):
        missing=dict(tree);missing.pop(runner/rel)
        case('complete deployment missing '+rel,lambda d=missing:check_deploy(d),True)
    outputs={};groups=control_groups()
    for group in groups:
        for paths in group['outputs'].values():
            yml=group['format']=='yaml-fixture';body=b'- id: 1\n  name: sword\n  price: 10\n' if yml else b'[{"id":1}]\n'
            outputs[Path(paths['rawRoot'])/('item.yml' if yml else 'item.json')]=body
            if 'canonicalPath' in paths:
                outputs[Path(paths['canonicalPath'])]=jb({'item.json':[{'id':1}]})
                outputs[Path(paths['semanticDigestPath'])]=(sha(outputs[Path(paths['canonicalPath'])])+'\n').encode()
    def compare(t):return helper['compare_control_outputs'](groups,lambda p:t[p],memory_tree(t))
    case('oracle and two candidates actual bytes/semantics synthetic',lambda:compare(outputs))
    missing=dict(outputs);missing.pop(Path(groups[0]['outputs']['candidate-1']['rawRoot'])/'item.yml')
    case('missing control output',lambda:compare(missing),True)
    wrong=dict(outputs);wrong[Path(groups[1]['outputs']['candidate-2']['rawRoot'])/'item.json']=b'[{"id":2}]'
    case('different control output',lambda:compare(wrong),True)
    wrong2=dict(outputs);wrong2[Path(groups[3]['outputs']['candidate-1']['semanticDigestPath'])]=b'0'*64
    case('false canonical digest',lambda:compare(wrong2),True)
    command=next(c for c in m['commands'] if c['id']=='L-negative-duplicate')
    process={'role':'DUT','argv':command['argv'],'cwd':command['cwd'],'env':command['env'],'quiescent':True,'timedOut':False,
             'startCount':1,'childStarts':0,'elapsedSeconds':.01,'exitCode':command['expectedExit']}
    case('negative DUT diagnostic checked',lambda:validate_process(command,process,b'FMLOC002',b'',lambda r:[]))
    case('exit alone insufficient for negative',lambda:validate_process(command,process,b'',b'',lambda r:[]),True)
    process_bad=dict(process,timedOut=True)
    case('timeout is not pass',lambda:validate_process(command,process_bad,b'FMLOC002',b'',lambda r:[]),True)
    required_l={'L-yaml','L-malformed-yaml','L-accepted-b','L-tampered-yaml','L-missing-yaml-license','L-changed-fallback-license','L-assert-controls'}
    case('inherited L control matrix complete',lambda:require(required_l.issubset({c['id'] for c in m['commands']}),'L controls omitted'))
    ltree={W/'L/self-tests/results.json':jb({'status':'PASS','total':12,'passed':12,
        'tests':[{'name':name,'passed':True} for name in sorted(SELF_TEST_NAMES)]})}
    for name,marker in [('duplicate','FMLOC002'),('parameters','FMLOC007'),('severity','FMLOC004')]:
        ltree[W/'L/negative'/name/'result.json']=jb({'caseName':name,'status':'PASS','expectedDiagnostic':marker,'observedDiagnostic':marker,'publishedCanonicalCount':0})
    for n in (1,2):
        for folder,leaves in [('raw',['fm-text-v1.json']),('canonical',['fm-text-v1.json','fm-text-v1.manifest.json'])]:
            for rel in leaves:ltree[W/f'L/pass-{n}'/folder/rel]=jb({'SYNTHETIC':True})
    def lcheck(data):return validate_l_outputs(lambda p:data[p],memory_tree(data))
    case('L full 12 tests/negatives/two publications synthetic',lambda:lcheck(ltree))
    incomplete=dict(ltree);result=strict(incomplete[W/'L/self-tests/results.json']);result['tests'].pop()
    incomplete[W/'L/self-tests/results.json']=jb(result)
    case('L summary cannot hide missing self-test',lambda:lcheck(incomplete),True)
    pub=dict(ltree);pub[W/'L/pass-2/canonical/fm-text-v1.json']=jb({'changed':True})
    case('L published canonical mismatch',lambda:lcheck(pub),True)
    missing_l=dict(ltree);missing_l.pop(W/'L/pass-1/raw/fm-text-v1.json')
    case('L missing raw publication',lambda:lcheck(missing_l),True)
    negative_pub=dict(ltree);negative_pub[W/'L/negative/duplicate/forbidden.json']=b'{}'
    case('L negative output must remain unpublished',lambda:lcheck(negative_pub),True)
    trx=b'<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><ResultSummary><Counters total="1" executed="1" passed="1" failed="0" /></ResultSummary><Results><UnitTestResult outcome="Passed" /></Results></TestRun>'
    case('official TRX completion synthetic',lambda:validate_official_trx(trx))
    case('official skipped/incomplete TRX rejected',lambda:validate_official_trx(trx.replace(b'executed="1"',b'executed="0"')),True)
    uuid='11111111-1111-1111-1111-111111111111';argv=['/opt/homebrew/bin/python3','-B',str(DRIVER),'execute']
    record={'centralThread':CENTRAL,'centralHost':'local','centralTurn':uuid,'centralItem':uuid,'ownerThread':OWNER['threadId'],
        'ownerHost':'local','ownerTurn':OWNER['turnId'],'scope':'EXECUTE_M17','argv':argv,'stage':'M','sourceFreezeSha256':sha(frozen),
        'modelSha256':sha(jb(m)),'driverSha256':sha(driver),'budgets':execution_budget(m),'reviewStatus':'COMPLETED_NO_UNRESOLVED_FINDINGS',
        'reviewedHead':'a'*40,'executionHead':'a'*40,'hostExecutorSha256':sha(driver),'refinedWriteSetSha256':sha(source_tables(m)['source/m17-write-set.tsv'])}
    raw_record=jb(record);pin={k:record[k] for k in ('centralThread','centralHost','centralTurn','centralItem','ownerThread','ownerHost','ownerTurn')}
    pin.update(path='/SYNTHETIC_M17/central.json',bytes=len(raw_record),sha256=sha(raw_record))
    case('pure independent-pin seam synthetic only',lambda:verify_activation(pin,raw_record,record,frozen,m,argv))
    forged=dict(record,ownerTurn=uuid)
    case('forged caller activation rejected',lambda:verify_activation(pin,raw_record,forged,frozen,m,argv),True)
    case('absent independent pin rejected',lambda:verify_activation(None,raw_record,record,frozen,m,argv),True)
    calls=[]
    def forbidden_read(p):calls.append(str(p));raise GateError('reader must not run')
    case('production execute closed before any caller input or read',lambda:execute(argv,forbidden_read),True)
    case('no production read or pin mutation',lambda:require(not calls and TRUSTED_CENTRAL_ACTIVATION is None,'closed gate leaked'))
    for field,value in [('stage','B'),('stage','L'),('budgets',{}),('budgets',dict(record['budgets'],M=dict(record['budgets']['M'],wallSeconds=901)))]:
        bad=dict(record);bad[field]=value;body=jb(bad);bad_pin=dict(pin,bytes=len(body),sha256=sha(body))
        case('trusted-record wrong stage/missing/expanded budget '+str(value)[:20],lambda p=bad_pin,b=body,r=bad:verify_activation(p,b,r,frozen,m,argv),True)
    budget_checks(m,case)
    return results

def fix03_protection():
    frozen=check_bytes(read(OLD17F2/'authority/source-freeze.json'),(2806,'3eebabf254cb2e3fd39e45757551e396d8db2613ee03b9eebc100a1652892d7b'),'F2 seal')
    records=strict(frozen)['filesExcludingThisSeal']+[identity(OLD17F2/'authority/source-freeze.json',frozen)]
    oldrun=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-m04';receipt=oldrun/'receipt.json'
    raw=check_bytes(read(receipt),(15266,'3b42fd9e70d26f6d31ec66f8110a3e3fd106b1cbe080e1631a2cc4f178db1777'),'M04 receipt')
    records+=strict(raw)['evidence']['beforeAndRawLogs']+[identity(receipt,raw)]
    for rel,count,digest in [('source/m17_driver.py',120382,'4df707e09d4c75419f93b6177e7a0bbaf9bc436f82b6e913f3b1ccbad0ae3658'),
        ('authority/offline-checks.json',21144,'2d9ce6bcc6b5835c68301ff508b6eab914c671d77bd6d3d9652bb25f154de760'),
        ('authority/source-receipt.json',25648,'ef34cef601c2c9c1c1f3073dc55011cc504b2fd669472ffeb815b1448d0de3b9')]:
        p=E/'failed-prepare-01'/rel;records.append(identity(p,check_bytes(read(p),(count,digest),'failed prepare preserved')))
    oldwork=W.with_name('m17');failed=oldwork/'receipts/material-validation.json'
    records.append(identity(failed,check_bytes(read(failed),(254,'a51e3c3a362a3ed1df30401995ab9fa0fa5219ae744c322c4f12bfdfceb16a21'),'M04 failure')))
    for row in records:check_bytes(read(Path(row['path'])),(row['bytes'],row['sha256']),'protected old leaf')
    for root in (OLD17F2,oldrun,oldwork,E/'failed-prepare-01'):
        paths=list(root.rglob('*'));require(not any(p.is_symlink() for p in paths) and {str(p) for p in paths if p.is_file()}=={r['path'] for r in records if Path(r['path']).is_relative_to(root)},'protected leaf set')
    for root in (M,W,M.with_name('5.1.0-fm-yamldotnet-16.3.0-m17')):
        require(not os.path.lexists(root) and not any(p.is_symlink() for p in root.parents),'future/old material root occupied or linked')
    return records

def fix03_checks(m,cached,history):
    import zipfile,types,warnings,struct
    results=[]
    def case(name,action,reject=False):
        tick=time.monotonic();error=None
        try:action()
        except Exception as exc:error=exc
        results.append({'name':name,'expected':'REJECT' if reject else 'PASS','passed':isinstance(error,GateError) if reject else error is None,
            'rawError':None if error is None else type(error).__name__+': '+str(error),'elapsedSeconds':time.monotonic()-tick})
    nodes=[n for n in ast.parse(read(OLD17F2/'source/m17_driver.py')).body if isinstance(n,ast.FunctionDef) and n.name=='archive_entries']
    old=dict(globals());exec(compile(ast.Module(body=nodes,type_ignores=[]),'F2-original-archive-entries','exec'),old)
    pkg=next(p for p in m['packages'] if p['key']=='messagepackanalyzer.3.1.7');data=read(Path(pkg['path']))
    case('F2 fixed official ZIP reproduces unsafe relative path',lambda:old['archive_entries'](pkg,data),True)
    names=['analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll','analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll']
    def official():
        actual=archive_entries(pkg,data);require(len(actual)==8 and all(n in actual for n in names),'eight entries and two canonical names')
        with zipfile.ZipFile(io.BytesIO(data)) as z:
            for entry in z.infolist():
                if not entry.is_dir():
                    target=next((n for n in names if entry.filename==n.replace('/cs/','/cs//')),entry.filename)
                    require(actual[target]==z.read(entry),'original ZipInfo bytes changed')
    case('F3 fixed official ZIP eight entries and original bytes',official)
    for item in [p for p in m['packages'] if p['class']=='existing'][:5]:
        case('unchanged preceding package '+item['key'],lambda p=item:require(archive_entries(p,read(Path(p['path'])))==old['archive_entries'](p,read(Path(p['path']))),'unchanged entries'))
    for field,value in [('key','other.3.1.7'),('bytes',178006),('sha256','0'*64),('sha512','invalid')]:
        case('real archive wrong '+field,lambda k=field,v=value:archive_entries(dict(pkg,**{k:v}),data),True)
    def fixture(entries,mode=0o100644,oversize=False):
        buf=io.BytesIO()
        with warnings.catch_warnings():
            warnings.simplefilter('ignore',UserWarning)
            with zipfile.ZipFile(buf,'w') as z:
                for name in entries:
                    entry=zipfile.ZipInfo(name);entry.create_system=3;entry.external_attr=mode<<16;z.writestr(entry,b'SYNTHETIC')
        blob=buf.getvalue()
        if oversize:
            blob=bytearray(blob);struct.pack_into('<I',blob,blob.index(b'PK\x01\x02')+24,128*1024**2+1);blob=bytes(blob)
        ns=dict(globals());ns['verify_archive']=lambda p,b:require(p==pkg and b==blob,'synthetic fixture bytes/identity')
        return types.FunctionType(archive_entries.__code__,ns)(pkg,blob)
    for name in ('a//b','../outside','/absolute','a/../b','a\\b','a\nb','a\rb','a\tb'):
        case('synthetic unsafe path '+repr(name),lambda n=name:fixture([n]),True)
    alias=names[0].replace('/cs/','/cs//')
    for label,entries in [('same-name',['a','a']),('alias duplicate',[alias,alias]),('alias canonical',[alias,names[0]]),('alias casefold',[alias,names[0].upper()])]:
        case('synthetic collision '+label,lambda es=entries:fixture(es),True)
    case('synthetic nonregular entry',lambda:fixture(['link'],0o120777),True)
    case('synthetic expansion budget',lambda:fixture(['large'],oversize=True),True)
    case('synthetic entry count budget',lambda:fixture([str(i) for i in range(5001)]),True)
    return results

def prepare():
    started=time.monotonic();checks_path=E/'authority/offline-checks.json'
    previous_report=strict(read(checks_path)) if checks_path.exists() else {}
    previous=previous_report.get('attempts',[]);superseded=list(previous_report.get('supersededSourceSeals',[]))
    previous_seal=E/'authority/source-freeze.json'
    require(all(read(E/rel)==read(E/'failed-prepare-01'/rel) for rel in ('authority/offline-checks.json','authority/source-receipt.json')),'only sealed failed attempt may be superseded');previous=[]
    require(not previous and not previous_seal.exists(),'FIX03 single offline attempt exhausted')
    require(not M.exists() and not W.exists(),'SOURCE_ONLY must not create/use future roots')
    archived={str(E/'failed-prepare-01'/rel) for rel in ('source/m17_driver.py','authority/offline-checks.json','authority/source-receipt.json')}
    allowed={str(DRIVER),str(checks_path),str(E/'authority/source-receipt.json')}|archived
    require({str(p) for p in E.rglob('*') if p.is_file()}.issubset(allowed),'unexpected pre-seal leaves')
    command=list(sys.orig_argv);raw_results=[];failure=None;cached={};check_seconds=0.0;protected=fix03_protection()
    def caching_reader(p):
        p=Path(p)
        if p not in cached:cached[p]=read(p)
        return cached[p]
    try:
        history=old_seals(caching_reader);m=model(caching_reader)
        checked=time.monotonic();raw_results=fix03_checks(m,cached,history);check_seconds=time.monotonic()-checked
        require(check_seconds<=5,'FIX03 directed check time cap')
        require(all(c['passed'] for c in raw_results),'offline case failures')
        require(old_seals()==history,'old 38 leaves changed during source preparation')
    except Exception as error:
        import traceback
        failure={'type':type(error).__name__,'message':str(error),'traceback':traceback.format_exc()}
    elapsed=check_seconds
    attempt={'command':command,'ran':True,'mode':'SOURCE_ONLY_NO_CHILDREN_NO_NETWORK_NO_DISK_UNPACK',
        'sourceIdentity':identity(DRIVER),'elapsedSeconds':elapsed,'exitCode':0 if failure is None else 20,
        'rawCases':raw_results,'failure':failure}
    attempts=previous+[attempt]
    checks={'scope':'FIX03 one directed archive seam run; synthetic fixtures are not official materials','protectedBefore':protected,
        'attempts':attempts,'cumulativeSeconds':sum(a['elapsedSeconds'] for a in attempts),
        'redReproduction':previous_report.get('redReproduction'),
        'supersededSourceSeals':superseded,
        'caseCount':len(raw_results),'passed':sum(c['passed'] for c in raw_results),
        'oldHistoricalResults':'23/46/25/42/50/73 and FIX02 18 retained, not rerun',
        'futureRootsCreated':False,'dotnetStarts':0,'unityStarts':0,'networkRequests':0,'archivesUnpackedToDisk':0}
    require(checks['cumulativeSeconds']<=5,'offline cumulative time cap exceeded')
    checks['rawStdout']=json.dumps(checks,ensure_ascii=False);print(checks['rawStdout'],flush=True)
    checks_path.parent.mkdir(parents=True,exist_ok=True);checks_path.write_bytes(jb(checks))
    if failure is not None:
        print(json.dumps(checks,ensure_ascii=False));return 20
    artifacts=source_artifacts(m,cached[DRIVER],read(OLD17F2/'source/m17_driver.py'),checks,history)
    frozen=seal_source(m,artifacts)
    memory={E/rel:b for rel,b in artifacts.items()}
    consume_source(frozen,lambda p:memory[p],m)
    for rel,b in artifacts.items():
        path=E/rel
        if path==DRIVER:check_bytes(read(path),(len(b),sha(b)),'driver at final generation');continue
        require(path in (checks_path,E/'authority/source-receipt.json') or not path.exists(),'unexpected source output already exists')
        path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes(b)
    freeze_path=E/'authority/source-freeze.json';freeze_path.write_bytes(frozen)
    # Mandatory actual disk reread, including fresh deterministic input/model
    # production (not merely memory fixture or author-supplied ready fields).
    consume_source(read(freeze_path))
    require({str(p) for p in E.rglob('*') if p.is_file()}=={str(E/rel) for rel in PREP}|archived,'final eight plus three preserved leaf set')
    require(old_seals()==history and fix03_protection()==protected and not M.exists() and not W.exists(),'preservation/future-root invariant')
    require(time.monotonic()-started-check_seconds<=30,'FIX03 mechanical preparation time cap')
    print(json.dumps({'status':'SOURCE_READY_NOT_MATERIAL_READY','checks':checks['passed'],'attempts':len(attempts),
        'cumulativeCheckSeconds':checks['cumulativeSeconds'],'totalInvocationSeconds':time.monotonic()-started,
        'freeze':identity(freeze_path),'leaves':[identity(E/rel) for rel in PREP]},ensure_ascii=False))
    return 0

def main():
    # execute authorization is checked before parsing optional caller material.
    if len(sys.argv)>1 and sys.argv[1]=='execute':return execute([sys.executable,*sys.argv])
    parser=argparse.ArgumentParser();parser.add_argument('mode',choices=['prepare','check-b','assert-controls'])
    parser.add_argument('--receipt');parser.add_argument('--runner');parser.add_argument('--expect',choices=['pass','reject'])
    parser.add_argument('--stage',choices=['B','L']);args=parser.parse_args()
    if args.mode=='prepare':return prepare()
    if args.mode=='assert-controls':print(json.dumps(assert_controls(args.stage)));return 0
    require(args.receipt and args.runner and args.expect,'missing B gate arguments')
    try:
        check_b_receipt(Path(args.receipt),Path(args.runner));passed=True;reason=None
    except (GateError,OSError,KeyError,ValueError) as error:passed=False;reason=str(error)
    require(passed==(args.expect=='pass'),'B gate outcome differs from expectation')
    print(json.dumps({'role':'DUT','gatePassed':passed,'reason':reason,'notIndependentAcceptance':True}))
    return 0 if passed else 20

if __name__=='__main__':
    try:sys.exit(main())
    except GateError as error:print(json.dumps({'status':'BLOCKED','reason':str(error)}));sys.exit(20)
