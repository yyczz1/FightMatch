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
OLD17F3=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix03'
OLD17F4=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix04'
OLD17F5=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix05'
E=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review01'
W=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05')
M=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix05')
W4=W.with_name('m17-fix04')
M4=M.with_name('5.1.0-fm-yamldotnet-16.3.0-m17-fix04')
M06_E=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-m06-resume1'
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
OWNER={'threadId':'01a0f40d-b0c5-7bc0-a2b2-be9d1213648a','hostId':'local','turnId':'01a12c28-d01f-7731-a6e9-51fc7ac522f4'}
ISSUER={'threadId':'01a0e401-511d-79f2-b47f-3ab0ade1681b','hostId':'local','turnId':'01a12bf9-e665-7d20-ab65-31ca1dfc8613'}
DISPATCH='fco_01a12c28-d034-7030-aa27-ac19eeaae2af'
REUSED_ROOT=W.with_name('m17-fix03')
REUSED_KEYS=frozenset(('microsoft.codecoverage.17.11.1','microsoft.net.test.sdk.17.11.1',
    'microsoft.testplatform.objectmodel.17.11.1','microsoft.testplatform.testhost.17.11.1','newtonsoft.json.13.0.3'))
FIX04_DOCUMENTS=(('engineering-loc-after-m05-next.md',10972,'5b7d1ce655d889a9d1aba10a4cdcb3126fe9d554493bda45273498f4ab03c8bc'),
    ('central-loc-m17-fix04-source.md',2062,'4af0a1b074c7ac14c91ba9f9a67b5a3679e4315f5751178b433d1cdadac3cc1d'))
FIX05_DOCUMENTS=(
    ('engineering-loc-after-m06-interruption.md',8979,'0ce328ffbc982df5f5f3a50277eb92651c338ae8039eb1371cfe1b9d4fc0ebe6'),
    ('central-loc-m17-fix05-source.md',3654,'3e85dd8e5332718ba4cc912113806cea6fc87b6ffa807cfb3be7668ccb5ad10d'),
    ('loc-m17-m06-interruption-receipt.json',3060,'28a5d17a5a47cc8493b54fad9f7f492427b7b36c1265bae2a5c07e518e43f9dc'),
    ('central-loc-m06-process-observation.json',1424,'ce5c29ad70909ac5339edd28a19bf9c320b6be46c7d98efee9cca8096c50af85'))
FIX05_BASE_MODEL='fefbc7a1002760a2231922069bd839864084421f888576e99a10518c5b7c6605'
REVIEW01_DOCUMENTS=(
    ('central-loc-m17-fix05-identity01.md',1498,'e7dff4bb80e409295d90c8c65027ff79a18d35a933f2d58253987a41631261d3'),
    ('central-loc-m17-fix05-seal01.md',1445,'5544b3063290194e20e99c93a06e26e51dd3f0265f2a3dbeb2d80c178853223d'),
    ('central-loc-m17-fix05-review01.md',3826,'3cf1312d5d3309ce9ea51d702c2b3f13a34d183ef25e370debfeaad257594354'),
    ('central-loc-m17-fix05-seal-success.json',18336,'ea5b2c865b7f7b239f52767f5de7d79ab8e71225252ddc53229e85866a8db69b'))
REVIEW01_SIDECAR=DOC/'loc-m17-fix05-review01-command-receipt.json'
PARTIAL_CAPTURE_OWNER={'threadId':'01a0f40d-b0c5-7bc0-a2b2-be9d1213648a','hostId':'local',
    'turnId':'01a12c14-512d-7331-8acc-fd240ca1de4a'}
PARTIAL_CAPTURE_SHA='cd1fc4983e3d83bd48d897c86fe35cb016d4dad4509adf6a801df597c6203cb6'
EXTERNAL_SOURCE_RECEIPT={'schemaVersion':1,'path':str(REVIEW01_SIDECAR),'maxBytes':65536,
    'location':'outside the sealed eight leaves','capture':'read_thread original completed commandExecution after tool return',
    'requiredFor':['independent central SOURCE acceptance','any subsequent material activation'],
    'requiredBindings':['source author thread/host/turn','actual sealing thread/host/turn','tool ID',
        'complete command and untruncated original output','exitCode 0','full invocation clock <=30 seconds',
        'final source-freeze bytes/SHA256','all eight leaf identities'],
    'independentReceiver':'01a0e401-511d-79f2-b47f-3ab0ade1681b',
    'includeAsOwnSourceInput':False,'receiptAbsenceBlocksAcceptanceAndActivation':True}
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
        if key in REUSED_KEYS:
            candidate=REUSED_ROOT/'acquire'/(key+'.nupkg')
            require(basis.exists(candidate),'required reused archive missing: '+key)
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

def model(reader=read,exists=lambda p:p.is_file(),partial=None):
    # The accepted F4 model, not a dynamic SDK/filesystem search, is the sole
    # baseline. execute_material still fresh-hashes every existing input.
    return _fix05_source_model(reader,partial)

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
    if m['archive']['class']=='official-get':
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
    items=[*m['packages'],m['archive']]
    require(len(items)==32 and all(p['class']=='existing' for p in items),'FIX05 requires 32 existing archives; no GET fallback')
    require(not any(c['id'].startswith('M-get-') or isinstance(c['network'],dict) for c in m['commands']),
        'FIX05 command table must have zero GETs')
    return []

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
        require(self.bodyBytes+row['bytes']<=0,'aggregate archive body budget');self.gets[path]={'bodyBytes':None,'savedBytes':0,'expected':row}
    def end_get(self,command,stdout,saved,success):
        path=command['argv'][command['argv'].index('--output')+1];record=self.gets[path]
        require(not record.get('accounted'),'GET body counted twice');record['accounted']=True
        matches=re.findall(rb'^M17_BODY_BYTES=([0-9]+)\r?$',stdout,re.M)
        record['savedBytes']=saved;record['bodyBytes']=int(matches[0]) if len(matches)==1 else None
        if record['bodyBytes'] is not None:self.bodyBytes+=record['bodyBytes']
        require(record['bodyBytes'] is not None and saved<=record['bodyBytes']<=self.allowed[path]['bytes']
            and self.bodyBytes<=0,'missing/over-limit actual archive body measurement')
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
        'officialGets':download_contract(m),'maxArchiveResponseBodyBytes':0,
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

def _nupkg_relative_name(raw):
    # Only verified nupkg filenames may fold internal empty segments.
    require(isinstance(raw,str) and raw and not raw.startswith('/') and not raw.endswith('/'),'invalid relative file endpoints')
    require(not re.match(r'^[A-Za-z]:',raw) and '\\' not in raw and
            not any(ord(c)<32 or ord(c)==127 for c in raw),'absolute/drive/backslash/control filename')
    parts=raw.split('/')
    require(not any(part in ('.','..') for part in parts),'dot segment in package filename')
    return safe('/'.join(part for part in parts if part))

def archive_entries(pkg,data):
    """Decode identity-verified nupkg entries in memory using original ZipInfo."""
    import zipfile
    verify_archive(pkg,data);result={};total=0;folded=set()
    with zipfile.ZipFile(io.BytesIO(data)) as archive:
        for entry in archive.infolist():
            if entry.is_dir():continue
            raw=entry.orig_filename;rel=None
            try:
                rel=_nupkg_relative_name(raw)
                require(rel.casefold() not in folded and len(result)<5000,'duplicate/oversized archive entry set')
                require((entry.external_attr>>16)&0o170000 in (0,0o100000),'nonregular package entry')
                total+=entry.file_size;require(total<=128*1024**2,'package expansion budget')
                b=archive.read(entry);require(len(b)==entry.file_size,'package length differs')
                result[rel]=b;folded.add(rel.casefold())
            except GateError as error:
                raise GateError('nupkg package='+repr(pkg.get('key'))+' raw='+repr(raw)+
                    ' canonical='+repr(rel)+' reason='+str(error)) from error
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
        fromfile='M17-FIX05/source/m17_driver.py',tofile='M17-FIX05-REVIEW01/source/m17_driver.py'))
    result['source/source.patch']=patch.encode()
    result['authority/prepare.json']=jb({'schemaVersion':1,'owner':OWNER,'issuer':ISSUER,'dispatchItem':DISPATCH,
        'modelSha256':sha(jb(m)),'model':m,'executionBudget':execution_budget(m),
        'externalCommandReceiptRequirement':EXTERNAL_SOURCE_RECEIPT,
        'inputClasses':{k:sum(r[0]==k for r in m['inputs']) for k in ('existing','official-get','generated')},
        'old38Leaves':history,'states':m['states'],
        'limitations':['All 32 archives reused read-only; no GET, restore, disk extraction or new material/work roots. Historical archive safety results reused, not rerun.',
            'Generated locks/assets have no hashes until actual isolated restore and independent comparison.',
            'Existing SDK leaf identities are declared; fresh full SDK hashing is a required pre-launch gate.',
            'Future M executor is present but unexecuted; its production trust pin is null. B/L are separately owned frozen command contracts.',
            'Package cache patterns are fixed to 31 IDs/versions; safe archive entries and per-command quotas are checked before/after future writes.']})
    result['authority/offline-checks.json']=jb(checks)
    result['authority/source-receipt.json']=jb({'status':'SOURCE_READY',
        'owner':OWNER,'issuer':ISSUER,'dispatchItem':DISPATCH,'modelSha256':sha(jb(m)),
        'sourceReady':True,'materialReady':False,'reviewReady':False,'executionAuthorized':False,'MOnlyBudget':execution_budget(m)['M'],
        'externalCommandReceiptRequirement':EXTERNAL_SOURCE_RECEIPT,'independentAcceptance':'BLOCKED_UNTIL_EXTERNAL_RECEIPT_AND_NEW_HEAD_REVIEW',
        'integrationVerdict':'NOT_ISSUED_AUTHOR_CANNOT_ACCEPT','historyPreserved':history,'validation':checks,
        'returnPath':ISSUER,'pendingArchives':[{k:p[k] for k in ('path','bytes','sha256','url')} for p in [*m['packages'],m['archive']] if p['class']=='official-get'],
        'generatedLocksAssets':len(m['generated']),
        'blockingSlot':'actual new M restore/comparison + new reviewed head + independent central execution pin; B/L remain independently gated'})
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
    require(isinstance(m.get('partialProtection'),dict),'FIX05 sealed partial protection missing')
    fresh=model(reader,partial=m['partialProtection']) if expected_model is None else expected_model
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
        require(len(s.gets)==13 and s.bodyBytes==9014649,'exact actual body total');return s
    case('13 fixed GETs actual archive body total synthetic',downloads)
    case('GET retry rejected',lambda:downloads().begin_get(commands[0]),True)
    cache=MStage(lambda:0);cache.allowed={r['path']:r for r in fixed}
    case('cache hits may reduce GET count to zero',lambda:require(cache.bodyBytes==0 and not cache.gets,'cache charged as transfer'))
    bad=copy.deepcopy(m);bad['archive']['bytes']+=1
    case('13-archive size/hash contract cannot expand',lambda:download_contract(bad),True)
    changed=copy.deepcopy(m);changed['archive']['url']+='?replacement'
    case('fixed official URL cannot change',lambda:download_contract(changed),True)
    c=commands[0];s=MStage(lambda:0);s.allowed={r['path']:r for r in fixed};s.begin_get(c)
    case('archive response aggregate overflow',lambda:s.end_get(c,b'M17_BODY_BYTES=9014650\n',9014650,True),True)
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

def fix04_protection(reader=read):
    receipt=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-m05-resume1/receipt.json'
    raw=check_bytes(reader(receipt),(75244,'c9804dd75e44a9efbeefc9cbc57ff2e7833cb84a10fbd2cf49e2e076a062fb3b'),'final M05 receipt')
    run=strict(raw);post=run['postExecutionEvidence']
    records=post['protectedIdentities']+post['roots']['outerEvidence']['files']+post['roots']['work']['files']+[identity(receipt,raw)]
    for name,count,digest in (
        ('loc-m17-m05-native-verdict.json',2210,'1b3bb6eabba066de4a76bff5aed29e735eacdeba2d7a0d307517c84056cebae4'),
        ('testing-loc-m17-m05-native-receipt.json',5637,'850386cc76df7e3ccb51aba3e4440bbc3d08f4c832c54cd4b8488ededc12da99')):
        path=DOC/name;records.append(identity(path,check_bytes(reader(path),(count,digest),'final independent M05 evidence')))
    unique={}
    for row in records:
        path=row['path'];require(path not in unique or unique[path]==row,'protected identity collision')
        check_bytes(reader(Path(path)),(row['bytes'],row['sha256']),'protected FIX03/M05 leaf')
        unique[path]=row
    for root in (OLD17F3,receipt.parent,REUSED_ROOT,BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-m04'):
        paths=list(root.rglob('*'));require(not any(p.is_symlink() for p in paths) and
            {str(p) for p in paths if p.is_file()}=={p for p in unique if Path(p).is_relative_to(root)},'protected exact leaf set')
    return [unique[p] for p in sorted(unique)]

def _fix04_source_model(reader=read):
    """Rebuild changed contracts from sealed inventory, without a SOURCE SDK scan."""
    prepared=strict(check_bytes(reader(OLD17F3/'authority/prepare.json'),
        (5732267,'7ac926130888c6cd0ac90a07bd1a6e418ea0107c544532989ecd9104a0e1a9f1'),'sealed F3 model'))
    old=prepared['model'];require(sha(jb(old))==prepared['modelSha256']=='9bc598c1c6964285f47a642f9d53f98e5e596ce33d62f7ce1269ca53d22a4753','F3 model identity')
    mappings=[(str(OLD17F3),str(E)),(str(REUSED_ROOT),str(W)),(str(M.with_name('5.1.0-fm-yamldotnet-16.3.0-m17-fix03')),str(M))]
    def mapped(value):
        if isinstance(value,dict):return {k:mapped(v) for k,v in value.items()}
        if isinstance(value,list):return [mapped(v) for v in value]
        if isinstance(value,str):
            for a,b in mappings:value=value.replace(a,b)
        return value
    m=mapped(old);m['owner']=OWNER;m['issuer']=ISSUER
    rows={(r[0],r[1],r[2]):r for r in m['inputs'] if r[1]!='nupkg' and r[3]!='N/A_NOT_GENERATED'}
    basis=Basis(reader);m['packages'],m['licenses']=package_model(basis,basis.obj(H16/'coverage-31.json'))
    basis.add(DRIVER,reader(DRIVER),'existing','source-layer','no generated E17 outputs included')
    for name,count,digest in FIX04_DOCUMENTS:
        path=DOC/name;basis.add(path,check_bytes(reader(path),(count,digest),'FIX04 approved contract'),'existing','authority','approved FIX04 source-only contract')
    for row in fix04_protection(reader):
        path=Path(row['path']);basis.add(path,reader(path),'existing','preserved-evidence','FIX03/M05 immutable input')
    rows.update(basis.inputs);m['inputs']=sorted(rows.values())
    m['commands']=command_plan(m,reader);m['writes']=write_plan(m);m['generated']=generated_contract(m)
    for r in m['generated']:
        m['inputs'].append(['generated',r['role'],r['path'],'N/A_NOT_GENERATED','N/A_NOT_GENERATED',r['validator']])
    m['inputs'].sort();validate_model(m)
    return m

def fix04_checks():
    """One directed in-memory run. Never invoke the historical full suite."""
    import zipfile,types,warnings,struct,stat
    results=[];inventory=[];collapsed=[]
    def case(name,action,reject=False,contains=()):
        tick=time.monotonic();error=None
        try:action()
        except Exception as exc:error=exc
        passed=isinstance(error,GateError) if reject else error is None
        if reject and error is not None:passed=passed and all(s in str(error) for s in contains)
        results.append({'name':name,'expected':'REJECT' if reject else 'PASS','passed':passed,
            'rawError':None if error is None else type(error).__name__+': '+str(error),'elapsedSeconds':time.monotonic()-tick})
    protected=fix04_protection();cached={}
    def caching_reader(p):
        p=Path(p)
        if p not in cached:cached[p]=read(p)
        return cached[p]
    m=_fix04_source_model(caching_reader)
    old=dict(globals())
    nodes=[n for n in ast.parse(caching_reader(OLD17F3/'source/m17_driver.py')).body
           if isinstance(n,ast.FunctionDef) and n.name=='archive_entries']
    require(len(nodes)==1,'F3 archive seam missing')
    exec(compile(ast.Module(body=nodes,type_ignores=[]),'sealed-F3-archive-entries','exec'),old)
    scriban=next(p for p in m['packages'] if p['key']=='scriban.7.2.5')
    data=caching_reader(Path(scriban['path']))
    case('F3 full Scriban reproduces unsafe relative path',lambda:old['archive_entries'](scriban,data),True,('unsafe relative path',))
    for raw in ('build//Scriban.props','build//Scriban.targets'):
        case('F3 Scriban original path RED '+repr(raw),lambda raw=raw:safe(raw),True,('unsafe relative path',))
    for pkg in [p for p in m['packages'] if p['class']=='existing']:
        blob=caching_reader(Path(pkg['path']))
        def actual_package(p=pkg,b=blob):
            actual=archive_entries(p,b);entries=[]
            expected_aliases={
                'build//Scriban.props':'build/Scriban.props','build//Scriban.targets':'build/Scriban.targets',
                'analyzers/roslyn4.3/cs//MessagePack.Analyzers.CodeFixes.dll':'analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll',
                'analyzers/roslyn4.3/cs//MessagePack.SourceGenerator.dll':'analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll'}
            with zipfile.ZipFile(io.BytesIO(b)) as z:
                for e in z.infolist():
                    mode=(e.external_attr>>16)&0o170000
                    rel=None if e.is_dir() else expected_aliases.get(e.filename,e.filename)
                    entries.append({'rawName':e.filename,'canonicalName':rel,'reason':'directory skipped by unchanged policy' if e.is_dir() else None,
                        'type':'directory' if e.is_dir() else 'regular' if mode in (0,stat.S_IFREG) else 'nonregular',
                        'unixType':mode,'fileSize':e.file_size,'compressedSize':e.compress_size})
                    if e.is_dir():continue
                    require(actual[rel]==z.read(e),'original ZipInfo bytes differ: '+repr(e.filename))
                    if rel!=e.filename:collapsed.append({'package':p['key'],'rawName':e.filename,'canonicalName':rel})
            if p['key']=='scriban.7.2.5':require(len(entries)==184 and len(actual)==184,'Scriban 184 entries')
            else:require(actual==old['archive_entries'](p,b),'other present package output differs from F3')
            inventory.append({'package':{k:p[k] for k in ('key','path','bytes','sha256','sha512','url')},
                'identityVerified':True,'entryCount':len(entries),'entries':entries})
        case('verified official package original bytes '+pkg['key'],actual_package)
    def complete_inventory():
        require(len(inventory)==19 and sum(len(p['entries']) for p in inventory)==705,'all 19 packages / 705 entries required')
        require(sum(e['type']=='directory' for p in inventory for e in p['entries'])==10,'ten directory entries required')
        require(len(collapsed)==4,'exact four existing internal-empty-segment files')
    case('complete directory receipt including skipped directories',complete_inventory)
    def synthetic(entries,mode=0o100644,oversize=False,length=False,verify=True):
        buf=io.BytesIO()
        with warnings.catch_warnings():
            warnings.simplefilter('ignore',UserWarning)
            with zipfile.ZipFile(buf,'w') as z:
                for name in entries:
                    entry=zipfile.ZipInfo(name);entry.create_system=3;entry.external_attr=mode<<16;z.writestr(entry,b'SYNTHETIC')
        blob=buf.getvalue()
        if oversize or length:
            blob=bytearray(blob);struct.pack_into('<I',blob,blob.index(b'PK\x01\x02')+24,128*1024**2+1 if oversize else 10);blob=bytes(blob)
        package={'key':'synthetic-third.1','bytes':len(blob),'sha256':sha(blob),
            'sha512':base64.b64encode(hashlib.sha512(blob).digest()).decode()}
        ns=dict(globals())
        # Synthetic namespace only; real archives above always use production identity verification.
        if verify:ns['verify_archive']=lambda p,b:require(p==package and b==blob,'synthetic fixture identity')
        else:
            package['sha256']='0'*64
            def premature_name(raw):raise AssertionError('name conversion before identity')
            ns['_nupkg_relative_name']=premature_name
        return types.FunctionType(archive_entries.__code__,ns)(package,blob)
    for raw in ('a//b','a///b'):
        case('synthetic third package accepts internal separators '+repr(raw),
             lambda raw=raw:require(synthetic([raw])=={'a/b':b'SYNTHETIC'},'canonical key/bytes'))
    case('global safe still rejects double slash',lambda:safe('a//b'),True)
    case('production identity before name conversion',lambda:synthetic(['a//b'],verify=False),True,('identity mismatch',))
    for field,value in [('bytes',scriban['bytes']+1),('sha256','0'*64),('sha512','invalid')]:
        case('official incorrect identity '+field,lambda k=field,v=value:archive_entries(dict(scriban,**{k:v}),data),True)
    unsafe=['/a','//a','a/..','a/../b','a//../b','a/./b','./a','../a','C:/a','C:a','a\\b','a\nb','a\rb','a\tb','a\x01b','a\x7fb']
    for raw in unsafe:
        case('synthetic unsafe original name '+repr(raw),lambda raw=raw:synthetic([raw]),True,("package='synthetic-third.1'",'raw='+repr(raw)))
    # ZIP marks trailing slash names as directories; retain that policy. A non-directory entry with trailing slash must fail.
    class NonDirectory(zipfile.ZipInfo):
        def is_dir(self):return False
    def non_directory(raw):
        entry=NonDirectory(raw);entry.external_attr=0o100644<<16
        class Archive:
            def __enter__(self):return self
            def __exit__(self,*args):pass
            def infolist(self):return [entry]
            def read(self,e):raise AssertionError('invalid name reached body read')
        ns=dict(globals());ns['verify_archive']=lambda p,b:True
        fake=types.SimpleNamespace(ZipFile=lambda *args:Archive())
        builtin=dict(vars(__import__('builtins')));original=builtin['__import__']
        builtin['__import__']=lambda name,*args:fake if name=='zipfile' else original(name,*args)
        ns['__builtins__']=builtin
        return types.FunctionType(archive_entries.__code__,ns)({'key':'synthetic-third.1'},b'')
    for raw in ('','a/','a\0b'):
        case('synthetic non-directory invalid endpoint '+repr(raw),lambda raw=raw:non_directory(raw),True,
             ("package='synthetic-third.1'",'raw='+repr(raw)))
    for label,names in [('duplicate',['a','a']),('alias/canonical',['a//b','a/b']),
                        ('two aliases',['a//b','a///b']),('casefold',['a//b','A/B'])]:
        case('synthetic collision '+label,lambda names=names:synthetic(names),True,
             ("package='synthetic-third.1'",'raw='+repr(names[-1]),'canonical='))
    long_raw='q'*600+'/../secret'
    case('full untruncated diagnostic name',lambda:synthetic([long_raw]),True,('raw='+repr(long_raw),))
    case('synthetic nonregular type',lambda:synthetic(['link'],0o120777),True,("raw='link'","canonical='link'",'nonregular'))
    case('synthetic expansion budget',lambda:synthetic(['large'],oversize=True),True,('package expansion budget',))
    case('synthetic length mismatch',lambda:synthetic(['length'],length=True),True,('package length differs',))
    case('synthetic count budget',lambda:synthetic([str(i) for i in range(5001)]),True,('oversized archive entry set',))
    def reuse_failure(missing=False):
        def reader(p):
            p=Path(p)
            if p==REUSED_ROOT/'acquire/microsoft.codecoverage.17.11.1.nupkg':return b'wrong archived bytes'
            return caching_reader(p)
        basis=Basis(reader,lambda p:False if p==REUSED_ROOT/'acquire/microsoft.codecoverage.17.11.1.nupkg' and missing else p.is_file())
        return package_model(basis,basis.obj(H16/'coverage-31.json'))
    case('five-source bad hash never falls back to GET',reuse_failure,True,('identity mismatch',))
    case('five-source missing never falls back to GET',lambda:reuse_failure(True),True,('required reused archive missing',))
    def model_rules():
        rows=download_contract(m);budget=execution_budget(m)
        require(sum(p['class']=='existing' for p in m['packages'])==19 and sum(p['class']=='official-get' for p in m['packages'])==12,'19 existing plus 12 nupkg unknown')
        require(len(rows)==13 and sum(r['bytes'] for r in rows)==9014649,'13 archives / 9014649 bytes')
        gets=[c for c in m['commands'] if c['stage']=='M' and c['id'].startswith('M-get-')]
        require(len(gets)==13 and all(c['argv'][c['argv'].index('--retry')+1]=='0' and c['maxStarts']==1 for c in gets),'GET retry budget')
        require(sum(c['stage']=='M' and c['id'].endswith('-restore') for c in m['commands'])==3,'three restores')
        require((budget['M']['wallSeconds'],budget['M']['workSeconds'],budget['M']['cleanupReserveSeconds'])==(900,870,30),'M wall budget')
        for pkg in m['packages']:
            if pkg['key'] in REUSED_KEYS:
                require(pkg['path']==str(REUSED_ROOT/'acquire'/(pkg['key']+'.nupkg')) and pkg['class']=='existing','exact reuse source')
        require(TRUSTED_CENTRAL_ACTIVATION is None,'production pin must remain null')
    case('fixed package/network/restore/time model',model_rules)
    for root in (REUSED_ROOT,M,W):
        case('unknown or old output rejected '+str(root),lambda root=root:authorize_write(m,root/'unknown','M','trusted-host'),True)
    def occupied():
        # No filesystem writes. Reach the unchanged production root-contamination guard.
        fake=types.SimpleNamespace(exists=lambda:True)
        ns=dict(globals());ns['M']=fake;ns['TRUSTED_CENTRAL_ACTIVATION']={'synthetic':True}
        return types.FunctionType(execute_material.__code__,ns)(m,read,b'',[],MStage())
    case('occupied material root rejected before any writes',occupied,True,('new material root must be absent',))
    case('protected inputs preserved / future roots absent',
         lambda:require(fix04_protection()==protected and not os.path.lexists(M) and not os.path.lexists(W),'protection/root drift'))
    return {'scope':'FIX04 one directed memory-only validation; synthetic cases explicitly isolated',
        'cases':results,'caseCount':len(results),'passed':sum(c['passed'] for c in results),
        'packageInventory':inventory,'collapsedExistingFiles':collapsed,'protectedBefore':protected,
        'modelSha256':sha(jb(m)),'unknownArchives':[{k:p[k] for k in ('path','bytes','sha256','url')} for p in [*m['packages'],m['archive']] if p['class']=='official-get'],
        'unknownStatus':'UNKNOWN_NOT_DOWNLOADED_NOT_VALIDATED','productionTrustPin':None,
        'SDKScan':False,'fullOfflineSuite':False,'GETs':0,'restores':0,'native':0,'diskExtraction':0}

def _fix05_base(reader=read):
    prepared=strict(check_bytes(reader(OLD17F4/'authority/prepare.json'),
        (5731946,'5cd3fc921154b8538216c24c0f09c5f4056273282cf28129c9a017c422a26617'),'accepted F4 prepare'))
    require(prepared['modelSha256']==sha(jb(prepared['model']))==FIX05_BASE_MODEL,'accepted F4 model identity')
    return prepared['model']

def _fix05_roots_absent(occupied=os.path.lexists):
    require(not occupied(M) and not occupied(W),'FIX05 future M5/W5 roots must be absent')

def _fix05_existing(item,reader=read):
    require(item['class']=='existing','FIX05 archive must be existing; no fallback')
    data=reader(Path(item['path']))
    verify_archive(item,data)
    return data

def _fix05_download_receipts(old,reader=read):
    raw=check_bytes(reader(M06_E/'receipt.json'),
        (74084,'0d3facf1273c829bd830e8c27e71406bbdfb774dd1223f2997f0cf38c3c1d039'),'M06 interruption receipt')
    run=strict(raw);collection=run['readOnlyCollection']
    require(run['original']['toolRecord']['id']=='exec-177da352-5c1b-4a9e-a39e-82082f70591f' and
        run['original']['hostExitedEventObserved'] is False,'M06 original tool/uncertainty identity')
    items={p['path']:p for p in [*old['packages'],old['archive']] if p['class']=='official-get'}
    downloads=collection['downloads'];require(len(downloads)==len(items)==13 and
        {r['expected']['path'] for r in downloads}==set(items),'exact M06 13-archive receipt set')
    records=[identity(M06_E/'receipt.json',raw),collection['before'],*collection['originalLogs']]
    commands={c['id']:c for c in old['commands']}
    for row in downloads:
        item=items[row['expected']['path']];name='M-get-'+item.get('key','yaml-source')
        require(row['id']==name and row['expected']=={k:item[k] for k in ('path','bytes','sha256','url')},
            'M06 archive expected URL/identity differs')
        require(row['identityMatches'] is True and row['exactArgvMatches'] is True and
            row['recordedBodyBytes']==[item['bytes']],'M06 download evidence incomplete')
        if 'key' in item:require(row['catalogSha512Matches'] is True,'M06 catalog identity incomplete')
        for field in ('archive','receipt','stdout','stderr'):
            rec=row[field];require(rec.get('exists') is True,'missing M06 recorded leaf')
            data=check_bytes(reader(Path(rec['path'])),(rec['bytes'],rec['sha256']),'M06 recorded leaf')
            records.append(identity(rec['path'],data))
        actual=strict(reader(Path(row['receipt']['path'])));command=commands[name]
        require(actual['argv']==command['argv'] and actual['cwd']==command['cwd'] and actual['env']==command['env'],
            'M06 raw GET command identity differs')
        for key,value in (('exitCode',0),('startCount',1),('childStarts',0),('quiescent',True),('timedOut',False),('failure',None)):
            require(actual.get(key)==row['recordedProcess'].get(key)==value,'M06 successful/quiescent GET receipt required')
        stdout=reader(Path(row['stdout']['path']))
        require(re.findall(rb'^M17_BODY_BYTES=([0-9]+)\r?$',stdout,re.M)==[str(item['bytes']).encode()],
            'M06 raw body measurement missing')
    return records

def _fix05_protection(reader=read):
    old=_fix05_base(reader)
    sealpath=OLD17F4/'authority/source-freeze.json'
    raw=check_bytes(reader(sealpath),(2808,'9b5ea95beb2f148f6c3f99a55aff33df5bf3d192aa66403f7f7ba7d15213b7c8'),'F4 seal')
    seal=strict(raw)
    records=seal['filesExcludingThisSeal']+[identity(sealpath,raw)]
    records+=_fix05_download_receipts(old,reader)
    records+=_review01_documents(reader)+_review01_prior(reader)
    before=strict(reader(M06_E/'before.json'));precheck=before['precheck']
    records+=precheck['protectedExpectedIdsFromSealedSource']+before['preservationExpected']
    for key in ('binding','retryDisposition','preservedPreviousBinding','activation','host','template',
                'driver','sourceFreeze','sourceReceipt','writeSet','reviewReceipt','sourceVerdict','humanAuthorityRecord'):
        records.append(precheck[key])
    records+=precheck['preservedApprovalTimeoutEvidence']
    for name,count,digest in FIX05_DOCUMENTS:
        path=DOC/name;records.append(identity(path,check_bytes(reader(path),(count,digest),'FIX05 fixed document')))
    unique={}
    for entry in records:
        row={k:entry[k] for k in ('path','bytes','sha256')};path=row['path']
        require(path not in unique or unique[path]==row,'FIX05 protection identity collision')
        check_bytes(reader(Path(path)),(row['bytes'],row['sha256']),'FIX05 protected leaf')
        unique[path]=row
    for root,expected in ((OLD17F4,{str(OLD17F4/r) for r in PREP}),
            (M06_E,{str(M06_E/r) for r in ('before.json','stdout.log','stderr.log','receipt.json')})):
        paths=list(root.rglob('*'))
        require(not any(p.is_symlink() for p in [root,*root.parents,*paths]) and
            {str(p) for p in paths if p.is_file()}==expected,'F4/M06 exact evidence leaf set')
    return [unique[p] for p in sorted(unique)]

def _fix05_output(value):
    # Apply only to future-output fields, never old evidence or archive inputs.
    if isinstance(value,dict):return {k:_fix05_output(v) for k,v in value.items()}
    if isinstance(value,list):return [_fix05_output(v) for v in value]
    if isinstance(value,str):
        for before,after in ((OLD17F4,E),(W4,W),(M4,M)):
            value=value.replace(str(before),str(after))
    return value

def _fix05_source_model(reader=read,partial=None):
    _review01_documents(reader)  # Fail missing/changed authority before other material reads.
    _fix05_roots_absent()
    old=_fix05_base(reader);m=copy.deepcopy(old)
    m['owner']=OWNER;m['issuer']=ISSUER;m['roots']={k:str(v) for k,v in ROOTS.items()}
    m['externalCommandReceiptRequirement']=copy.deepcopy(EXTERNAL_SOURCE_RECEIPT)
    for body in m['licenses']:
        if body['kind']=='generated':body['path']=_fix05_output(body['path'])
    for pkg in m['packages']:
        # The frozen F4 path already names TOOL, W3, or W4 correctly.
        pkg['class']='existing'
        for body in pkg['licenses']:
            if body['kind']=='generated':body['path']=_fix05_output(body['path'])
    m['archive']['class']='existing'
    for item in [*m['packages'],m['archive']]:_fix05_existing(item,reader)
    rows={}
    for original in old['inputs']:
        row=list(original)
        if row[1] in ('nupkg','yaml-source-archive','source-layer') or row[3]=='N/A_NOT_GENERATED':continue
        if row[0]=='generated':row[2]=_fix05_output(row[2])
        rows[tuple(row[:3])]=row
    def add(rec,role,authority):
        row=['existing',role,rec['path'],rec['bytes'],rec['sha256'],authority]
        key=tuple(row[:3])
        if key in rows:
            require(rows[key][3:5]==row[3:5],'FIX05 input identity collision')
        else:rows[key]=row
    for item in [*m['packages'],m['archive']]:
        add(item,'nupkg' if 'key' in item else 'yaml-source-archive',item['url'])
    add(identity(DRIVER,reader(DRIVER)),'source-layer','no generated E17 outputs included')
    for row in _fix05_protection(reader):add(row,'preserved-evidence','F4/M06 immutable input; not material acceptance')
    if partial is not None:
        _fix05_partial_unchanged(partial,reader)
        m['partialProtection']=copy.deepcopy(partial)
        for row in partial['files']:add(row,'preserved-partial','new FIX05 read-only M4/W4 inventory, not historical receipt')
    m['inputs']=sorted(rows.values())
    m['commands']=command_plan(m,reader);m['writes']=write_plan(m);m['generated']=generated_contract(m)
    for row in m['generated']:
        m['inputs'].append(['generated',row['role'],row['path'],'N/A_NOT_GENERATED','N/A_NOT_GENERATED',row['validator']])
    m['inputs'].sort();validate_model(m);download_contract(m)
    return m

def _fix05_stamp(path):
    import stat
    path=Path(path)
    require(not any(p.is_symlink() for p in (path,*path.parents)),'partial symlink forbidden')
    value=path.lstat()
    require(stat.S_ISREG(value.st_mode) or stat.S_ISDIR(value.st_mode),'partial nonregular entry')
    require(not stat.S_ISREG(value.st_mode) or value.st_nlink==1,'partial hard link forbidden')
    return {'device':value.st_dev,'inode':value.st_ino,'mode':value.st_mode,'nlink':value.st_nlink,
        'size':value.st_size,'mtimeNs':value.st_mtime_ns,'ctimeNs':value.st_ctime_ns}

def _fix05_partial_inventory():
    """Exactly one enumeration of M4/W4. New evidence, not an M06 exit proof."""
    import datetime,stat
    files=[];directories=[]
    def visit(path):
        stamp=_fix05_stamp(path)
        require(stat.S_ISDIR(stamp['mode']),'partial root/directory required')
        directories.append({'path':str(path),'stat':stamp})
        with os.scandir(path) as entries:children=sorted(entries,key=lambda e:e.name)
        for entry in children:
            child=Path(entry.path);before=_fix05_stamp(child)
            if stat.S_ISDIR(before['mode']):visit(child)
            else:
                data=read(child);require(_fix05_stamp(child)==before,'partial changed while hashing')
                files.append(dict(identity(child,data),stat=before))
        require(_fix05_stamp(path)==stamp,'partial directory changed during enumeration')
    for root in (M4,W4):visit(root)
    result={'schemaVersion':1,'kind':'NEW_FIX05_READ_ONLY_PARTIAL_INVENTORY','owner':OWNER,
        'capturedAtUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'roots':[str(M4),str(W4)],'enumerationsPerRoot':1,'files':sorted(files,key=lambda r:r['path']),
        'directories':sorted(directories,key=lambda r:r['path']),'linksAccepted':0,
        'processEvidence':str(DOC/'central-loc-m06-process-observation.json'),
        'limitation':'Stable identities and current exact command-line observation do not establish historical exit, full lineage closure or material acceptance.'}
    _fix05_partial_unchanged(result)
    return result

def _fix05_partial_unchanged(snapshot,reader=read):
    import stat
    require(snapshot.get('schemaVersion')==1 and snapshot.get('kind')=='NEW_FIX05_READ_ONLY_PARTIAL_INVENTORY' and
        snapshot.get('owner')==PARTIAL_CAPTURE_OWNER and sha(jb(snapshot))==PARTIAL_CAPTURE_SHA and snapshot.get('roots')==[str(M4),str(W4)] and
        snapshot.get('enumerationsPerRoot')==1 and snapshot.get('linksAccepted')==0,'partial manifest identity')
    files=snapshot['files'];directories=snapshot['directories']
    allrows=files+directories;paths=[r['path'] for r in allrows]
    require(len(paths)==len(set(paths)),'partial path collision')
    dirpaths={r['path'] for r in directories};roots={str(M4),str(W4)}
    require(roots<=dirpaths,'partial roots missing')
    for row in allrows:
        p=Path(row['path']);require(p.is_absolute() and '..' not in p.parts and
            any(p==r or p.is_relative_to(r) for r in (M4,W4)),'partial path outside exact roots')
        require(str(p) in roots or str(p.parent) in dirpaths,'partial parent missing')
        require(_fix05_stamp(p)==row['stat'],'partial identity/stat changed')
    for row in files:
        require(stat.S_ISREG(row['stat']['mode']),'partial file type')
        check_bytes(reader(Path(row['path'])),(row['bytes'],row['sha256']),'preserved partial leaf')
        require(_fix05_stamp(row['path'])==row['stat'],'partial changed while verifying')
    for row in directories:
        require(stat.S_ISDIR(row['stat']['mode']) and _fix05_stamp(row['path'])==row['stat'],'partial directory changed')
    # Known directory stamps detect additions/removals without another traversal.
    return True

def _fix05_core_digest(m):
    core={k:v for k,v in m.items() if k!='partialProtection'}
    core['inputs']=[r for r in m['inputs'] if r[1]!='preserved-partial']
    return sha(jb(core))

def _fix05_delta(old_driver,new_driver):
    def functions(blob):
        return {n.name:ast.dump(n,include_attributes=False) for n in ast.parse(blob).body
            if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
    old=functions(old_driver);new=functions(new_driver)
    changed=sorted(k for k in old.keys()&new.keys() if old[k]!=new[k])
    allowed={'model','write_plan','download_contract','MStage','execution_budget','source_artifacts','consume_source','prepare'}
    require(set(changed)==allowed,'unexpected changed F4 function set')
    require(not old.keys()-new.keys(),'F4 functions removed')
    require(all(k.startswith('_fix05_') or k=='fix05_checks' for k in new.keys()-old.keys()),'unexpected new function')
    require(all(old[k]==new[k] for k in ('safe','read','verify_archive','_nupkg_relative_name','archive_entries','yaml_entries',
        'command_plan','validate_restore','compare_restore','complete_package_union','execute','execute_material','MaterialWriter',
        'consume_m_stage','run_material_command','verify_activation','child_sandbox','install_sandbox')),'old safety semantics changed')
    return {'changedExistingFunctions':changed,'newPrivateHelpers':sorted(new.keys()-old.keys()),
        'unchangedExistingFunctions':sorted(k for k in old.keys()&new.keys() if old[k]==new[k]),
        'patchBaseline':identity(OLD17F4/'source/m17_driver.py',old_driver),
        'changedFields':['future output roots E/W/M and derived paths','owner/issuer/dispatch actual identities',
            '13 archive input classes official-get -> existing; exact old input paths retained',
            'GET command/write rows removed; body budget 9014649 -> 0',
            'F4/M06 fixed evidence and new M4/W4 partial protection','source-only difference tests and sealing'],
        'unchangedFields':['31 package identities/versions/licenses and source archive commit',
            '931 official source identities/two patches','SDK identity ledger','project graphs/restore order',
            'archive contents/path safety/process supervision/activation gates/870+30-second M deadline']}

def fix05_checks():
    """One SOURCE delta run; never calls FIX04's 66-case suite or partial scan."""
    import types
    results=[];cached={}
    def caching_reader(p):
        p=Path(p)
        if p not in cached:cached[p]=read(p)
        return cached[p]
    def case(name,action,reject=False,contains=''):
        tick=time.monotonic();error=None
        try:action()
        except Exception as exc:error=exc
        passed=isinstance(error,GateError) if reject else error is None
        if reject and error is not None:passed=passed and contains in str(error)
        results.append({'name':name,'expected':'REJECT' if reject else 'PASS','passed':passed,
            'rawError':None if error is None else type(error).__name__+': '+str(error),'elapsedSeconds':time.monotonic()-tick})
    old=_fix05_base(caching_reader);m=model(caching_reader)
    old_driver=caching_reader(OLD17F4/'source/m17_driver.py');driver=caching_reader(DRIVER)
    protected=_fix05_protection(caching_reader)
    case('exact allowed AST delta; archive/restore/process/activation functions unchanged',
        lambda:_fix05_delta(old_driver,driver))
    def fixed_fields():
        allowed={'owner','issuer','roots','inputs','packages','licenses','archive','commands','writes','generated'}
        require(all(m[k]==v for k,v in old.items() if k not in allowed),'unchanged F4 model field drift')
        require(m['archive']==dict(old['archive'],**{'class':'existing'}),'Yaml source identity/commit/path drift')
        counts={'tool':0,'W3':0,'W4':0}
        for p,q in zip(m['packages'],old['packages']):
            expected=copy.deepcopy(q);expected['class']='existing'
            for body in expected['licenses']:
                if body['kind']=='generated':body['path']=_fix05_output(body['path'])
            require(p==expected,'package identity/path/version/license drift')
            path=Path(p['path'])
            if p['key'] in REUSED_KEYS:
                require(path==REUSED_ROOT/'acquire'/(p['key']+'.nupkg'),'five W3 source paths drift');counts['W3']+=1
            elif q['class']=='official-get':
                require(path==W4/'acquire'/(p['key']+'.nupkg'),'twelve W4 source paths drift');counts['W4']+=1
            else:
                require(path==TOOL/'nuget'/p['id'].lower()/p['version']/(p['key']+'.nupkg'),'original TOOL source path drift');counts['tool']+=1
        require(counts=={'tool':14,'W3':5,'W4':12},'31 package source counts')
        require([r for r in m['inputs'] if r[1]=='sdk-frozen-leaf']==[r for r in old['inputs'] if r[1]=='sdk-frozen-leaf'],
            'SDK declared identity ledger changed')
        expected_licenses=copy.deepcopy(old['licenses'])
        for body in expected_licenses:
            if body['kind']=='generated':body['path']=_fix05_output(body['path'])
        require(m['licenses']==expected_licenses,'license provenance changed')
    case('31+1 exact readonly mappings; SDK/source/patch/closure unchanged',fixed_fields)
    def commands_budget():
        expected=_fix05_output([c for c in old['commands'] if not c['id'].startswith('M-get-')])
        require(m['commands']==expected,'non-GET command contracts changed')
        require(download_contract(m)==[] and execution_budget(m)['M']['maxArchiveResponseBodyBytes']==0,'zero GET/body mismatch')
        require([c['id'] for c in m['commands'] if c['stage']=='M']==[
            'M-runner-generation-restore','M-tests-generation-restore','M-tests-verification-restore'],'M restore order/count')
        require(all(c['network']=='OS denied; local feed only' for c in m['commands']),'network reopened')
        b=execution_budget(m)['M'];require((b['wallSeconds'],b['workSeconds'],b['cleanupReserveSeconds'])==(900,870,30),'M clock changed')
        stage=MStage(lambda:100)
        require((stage.work,stage.end,stage.bodyBytes,stage.allowed,stage.gets)==(970,1000,0,{},{}),'M timer/GET initial state')
        require(TRUSTED_CENTRAL_ACTIVATION is None,'production pin opened')
    case('zero GET/zero body; unchanged three restores/network/time/closed pin',commands_budget)
    def write_delta():
        expected=[]
        for row in old['writes']:
            if row[4].startswith('M-get-'):continue
            expected.append(_fix05_output(row))
        require(m['writes']==sorted(expected),'write delta not limited to GET removal/future outputs')
        for row in m['writes']:
            target=ROOTS[row[0]]/row[1]
            require(not any(target==r or target.is_relative_to(r) for r in (REUSED_ROOT,W4,M4,OLD17F4,M06_E)),'old root entered write set')
    case('write set exact future outputs and removed GET writers',write_delta)
    for item in [*m['packages'],m['archive']]:
        name=item.get('key','yaml-source')
        def missing(p):raise GateError('synthetic missing archive')
        case(name+' missing source fails without fallback',lambda item=item:_fix05_existing(item,missing),True,'missing archive')
        case(name+' wrong bytes fail without fallback',lambda item=item:_fix05_existing(item,lambda p:b'wrong'),True,'identity mismatch')
    def link():
        fake=types.SimpleNamespace(parents=[],is_symlink=lambda:True)
        ns=dict(globals());ns['Path']=lambda p:fake
        guarded=types.FunctionType(read.__code__,ns)
        _fix05_existing(m['packages'][0],guarded)
    case('linked archive rejected by unchanged input guard before read',link,True,'symlink input')
    for root in (M,W):
        case('occupied future root rejected '+str(root),
            lambda root=root:_fix05_roots_absent(lambda p:p==root),True,'roots must be absent')
    for root in (REUSED_ROOT,W4,M4,OLD17F4,M06_E):
        case('old root write rejected '+str(root),lambda root=root:authorize_write(m,root/'unknown','M','stage-inputs'),True,'write not authorized')
    def forbidden_get():
        stage=MStage(lambda:100);stage.allowed={'/synthetic':{'url':'https://invalid.example','bytes':1}}
        stage.begin_get({'stage':'M','argv':['curl','--output','/synthetic','https://invalid.example']})
    case('even injected GET descriptor cannot spend positive body budget',forbidden_get,True,'aggregate archive body budget')
    def no_receipt():
        path=M06_E/'receipt.json'
        def altered(p):return b'{}' if p==path else caching_reader(p)
        return _fix05_download_receipts(old,altered)
    case('missing/wrong fixed M06 receipt fails',no_receipt,True,'identity mismatch')
    def bad_class():
        broken=copy.deepcopy(m);broken['archive']['class']='official-get';download_contract(broken)
    case('official-get reintroduction rejected',bad_class,True,'requires 32 existing')
    def unchanged_clock_ast():
        a=next(n for n in ast.parse(old_driver).body if isinstance(n,ast.ClassDef) and n.name=='MStage')
        b=next(n for n in ast.parse(driver).body if isinstance(n,ast.ClassDef) and n.name=='MStage')
        for n in ast.walk(a):
            if isinstance(n,ast.Constant) and n.value==9014649:n.value=0
        require(ast.dump(a,include_attributes=False)==ast.dump(b,include_attributes=False),'MStage changed beyond body zero budget')
    case('MStage AST differs only in two zero-body constants',unchanged_clock_ast)
    case('protected historical evidence stable; future roots absent',
        lambda:require(_fix05_protection()==protected and not os.path.lexists(M) and not os.path.lexists(W),'protection/root drift'))
    return {'scope':'FIX05 one directed SOURCE-only difference run; synthetic rejection seams, no material execution',
        'cases':results,'caseCount':len(results),'passed':sum(r['passed'] for r in results),
        'sourceIdentity':identity(DRIVER,driver),'coreModelSha256':_fix05_core_digest(m),
        'protectedBefore':protected,'archiveInventory':[{k:p[k] for k in ('path','bytes','sha256','url','class')} for p in [*m['packages'],m['archive']]],
        'archiveSources':{'toolPackages':14,'W3Packages':5,'W4Packages':12,'W4SourceTar':1},
        'baselineModelSha256':FIX05_BASE_MODEL,'historicalSafetyEvidence':{
            'path':str(OLD17F4/'authority/offline-checks.json'),'bytes':283614,
            'sha256':'fe97b1b962b4be3f4128b3f46ac6e70c00fc9f727d747b791f7822a881989218',
            'caseCount':66,'rerun':False,'reviewedHead':'816263eaacf4f3e276c0cef7703c5824af086cc6'},
        'partialInventory':'not collected in directed run; one new collection only during mechanical sealing',
        'productionTrustPin':None,'SDKScan':False,'fullOfflineSuite':False,'GETs':0,'restores':0,'native':0,'diskExtraction':0}

def _review01_documents(reader=read):
    return [identity(DOC/name,check_bytes(reader(DOC/name),(count,digest),'REVIEW01 fixed authority: '+name))
        for name,count,digest in REVIEW01_DOCUMENTS]

def _review01_prior(reader=read):
    sealpath=OLD17F5/'authority/source-freeze.json'
    raw=check_bytes(reader(sealpath),(2807,'504da445d54b7d08effc34146dfc0af1d78fba6e5952db9c47f6f58de9658956'),'F5 frozen evidence')
    seal=strict(raw);records=seal['filesExcludingThisSeal']+[identity(sealpath,raw)]
    require(len(records)==8 and {r['path'] for r in records}=={str(OLD17F5/r) for r in PREP},'exact F5 seal set')
    for row in records:check_bytes(reader(Path(row['path'])),(row['bytes'],row['sha256']),'immutable F5 leaf')
    receipt=strict(reader(OLD17F5/'authority/source-receipt.json'))
    records+=receipt['identityCorrection']['preservedBefore']+receipt['formatCorrection']['preservedSixLeaves']
    records.append(receipt['formatCorrection']['backupReceipt'])
    # This is a fixed, genuinely completed historical tool record, not an
    # invented ID for the current not-yet-completed sealing command.
    success=strict(reader(DOC/'central-loc-m17-fix05-seal-success.json'));tool=success['tool']
    require(tool['id']=='exec-0a6d03d4-489e-4f65-bd61-282109520212' and tool['status']=='completed' and
        tool['exitCode']==0 and tool['output']['truncated'] is False and tool['command'],'historical successful tool record')
    output=[strict(line.encode()) for line in tool['output']['text'].splitlines() if line.strip()]
    result=[r for r in output if r.get('status')=='SOURCE_READY_NOT_MATERIAL_READY']
    clocks=[r for r in output if r.get('event')=='FINAL_COMMAND_CLOCK']
    require(len(result)==len(clocks)==1 and clocks[0]['exitCode']==0 and
        0<=clocks[0]['fullInvocationSeconds']<=30,'historical complete successful clock')
    require(result[0]['freeze']==identity(sealpath,raw) and
        sorted(result[0]['leaves'],key=lambda r:r['path'])==sorted(seal['filesExcludingThisSeal']+[identity(sealpath,raw)],key=lambda r:r['path']),
        'historical success/final F5 seal mismatch')
    require(result[0]['modelSha256']==success['sourceModelSha256']==seal['modelSha256'],'historical success/model mismatch')
    for root in (OLD17F5,OLD17F5.with_name(OLD17F5.name+'-identity-before'),OLD17F5.with_name(OLD17F5.name+'-seal-before')):
        paths=list(root.rglob('*'))
        require(not any(p.is_symlink() for p in [root,*root.parents,*paths]) and
            {str(p) for p in paths if p.is_file()}=={r['path'] for r in records if Path(r['path']).is_relative_to(root)},
            'immutable F5/history leaf set')
    return records

def _review01_partial(reader=read):
    prepared=strict(check_bytes(reader(OLD17F5/'authority/prepare.json'),
        (8570376,'10ee667d11e9581d2ed97e790d574c17ee10f252875d196d46b74f51d6af704d'),'F5 partial manifest container'))
    partial=prepared['model']['partialProtection']
    require(sha(jb(partial))==PARTIAL_CAPTURE_SHA and len(partial['files'])==2576 and
        len(partial['directories'])==909 and partial['capturedAtUtc']=='2026-10-11T17:51:20.113573+00:00',
        'exact original F5 partial capture')
    return partial

def _review01_delta(old_driver,new_driver):
    def functions(blob):
        return {n.name:ast.dump(n,include_attributes=False) for n in ast.parse(blob).body
            if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
    old=functions(old_driver);new=functions(new_driver)
    changed=sorted(k for k in old.keys()&new.keys() if old[k]!=new[k])
    require(set(changed)=={'_fix05_protection','_fix05_source_model','_fix05_partial_unchanged','source_artifacts','prepare'},
        'REVIEW01 function delta outside source/evidence binding')
    require(not old.keys()-new.keys() and
        all(k.startswith('_review01_') or k=='review01_checks' for k in new.keys()-old.keys()),'unexpected helper change')
    return {'patchBaseline':identity(OLD17F5/'source/m17_driver.py',old_driver),
        'changedExistingFunctions':changed,'newPrivateHelpers':sorted(new.keys()-old.keys()),
        'unchangedExistingFunctions':sorted(k for k in old.keys()&new.keys() if old[k]==new[k]),
        'changedFields':['fixed four REVIEW01 authority/evidence identities','preserved F5 and its failure history',
            'SOURCE E root and actual author/dispatch identities','structured external command receipt requirement',
            'frozen partial capture owner independent of new source author','directed authority regression and fixed-list-only prepare'],
        'unchangedFields':['future M5/W5 paths','31+1 existing archive identities','0 GET/0 body','3 restore/870+30 seconds',
            'archive path/contents checks','process, activation, contamination and source consumption gates','original partial capture bytes/time']}

def review01_checks():
    """Single regression run; inherited 81/66 results are referenced, not rerun."""
    results=[];cached={}
    def caching_reader(p):
        p=Path(p)
        if p not in cached:cached[p]=read(p)
        return cached[p]
    def case(name,action,reject=False,contains=''):
        tick=time.monotonic();error=None
        try:action()
        except Exception as exc:error=exc
        passed=isinstance(error,GateError) if reject else error is None
        if reject and error is not None:passed=passed and contains in str(error)
        results.append({'name':name,'expected':'REJECT' if reject else 'PASS','passed':passed,
            'rawError':None if error is None else type(error).__name__+': '+str(error),'elapsedSeconds':time.monotonic()-tick})
    m=model(caching_reader,partial=_review01_partial(caching_reader));protected=_fix05_protection(caching_reader)
    old=strict(caching_reader(OLD17F5/'authority/prepare.json'))['model']
    old_driver=caching_reader(OLD17F5/'source/m17_driver.py');driver=caching_reader(DRIVER)
    delta=_review01_delta(old_driver,driver)
    def invariants():
        for field in ('packages','archive','sources','licenses','graphs','generated','writes','sdkVersion','sourceCommit','stages','states','partialProtection'):
            require(m[field]==old[field],'unchanged F5 model field: '+field)
        require(m['roots']['work']==old['roots']['work'] and m['roots']['material']==old['roots']['material'],'M5/W5 changed')
        expected=copy.deepcopy(old['commands'])
        def remap(value):
            if isinstance(value,dict):return {k:remap(v) for k,v in value.items()}
            if isinstance(value,list):return [remap(v) for v in value]
            return value.replace(str(OLD17F5),str(E)) if isinstance(value,str) else value
        require(m['commands']==remap(expected),'commands changed beyond source E references')
        require(download_contract(m)==[] and execution_budget(m)['M']==strict(caching_reader(OLD17F5/'authority/prepare.json'))['executionBudget']['M'],'M budget changed')
        require([r for r in m['inputs'] if r[1]=='sdk-frozen-leaf']==[r for r in old['inputs'] if r[1]=='sdk-frozen-leaf'],'SDK identity ledger changed')
        require(TRUSTED_CENTRAL_ACTIVATION is None,'activation remains closed')
    case('F5 product/material/safety contracts unchanged; only source E changes',invariants)
    def bindings():
        for row in _review01_documents(caching_reader):
            require(any(r[0]=='existing' and r[2:5]==[row['path'],row['bytes'],row['sha256']] for r in m['inputs']),
                'required authority missing from fixed model')
        require(m['externalCommandReceiptRequirement']==EXTERNAL_SOURCE_RECEIPT and
            not any(r[2]==str(REVIEW01_SIDECAR) for r in m['inputs']),'sidecar contract/self-reference')
        require(EXTERNAL_SOURCE_RECEIPT['receiptAbsenceBlocksAcceptanceAndActivation'] is True and
            EXTERNAL_SOURCE_RECEIPT['requiredFor']==['independent central SOURCE acceptance','any subsequent material activation'],
            'external evidence gate omitted')
    case('all four fixed documents present; external receipt required without self-cycle',bindings)
    # Use a sealed in-memory candidate to exercise actual consume_source,
    # not merely the low-level hash helper. No output files are generated here.
    artifacts=source_artifacts(m,driver,old_driver,{'scope':'in-memory authority rejection fixture'},old_seals(caching_reader))
    frozen=seal_source(m,artifacts);memory={E/rel:b for rel,b in artifacts.items()}
    def combined(p):return memory[p] if p in memory else caching_reader(p)
    case('production source consumer accepts unchanged bound authority',lambda:consume_source(frozen,combined))
    for name,count,digest in REVIEW01_DOCUMENTS:
        path=DOC/name
        def missing(p,path=path):
            if p==path:raise GateError('synthetic missing fixed authority: '+str(path))
            return combined(p)
        def tampered(p,path=path):
            return combined(p)+b' ' if p==path else combined(p)
        case('consume_source rejects missing '+name,lambda reader=missing:consume_source(frozen,reader),True,'missing fixed authority')
        case('consume_source rejects tampered '+name,lambda reader=tampered:consume_source(frozen,reader),True,'identity mismatch')
    case('fixed historical successful command matches exact F5 freeze',lambda:_review01_prior(caching_reader))
    case('original partial capture owner/time/hash retained',lambda:require(m['partialProtection']==old['partialProtection'],'partial recaptured'))
    case('no sidecar before actual successful tool return',lambda:require(not os.path.lexists(REVIEW01_SIDECAR),'sidecar collision'))
    case('protection unchanged and future roots absent',lambda:require(_fix05_protection()==protected and not os.path.lexists(M) and not os.path.lexists(W),'protection drift'))
    return {'scope':'FIX05 REVIEW01 single authority/receipt SOURCE regression','cases':results,'caseCount':len(results),
        'passed':sum(r['passed'] for r in results),'sourceIdentity':identity(DRIVER,driver),'modelSha256':sha(jb(m)),
        'coreModelSha256':_fix05_core_digest(m),'protectedBefore':protected,'sourceDelta':delta,
        'historicalEvidence':{'F5FinalChecks':identity(OLD17F5/'authority/offline-checks.json',caching_reader(OLD17F5/'authority/offline-checks.json')),
            'F5Cases':81,'F4Cases':66,'rerun':False,'F5SuccessfulTool':identity(DOC/'central-loc-m17-fix05-seal-success.json',caching_reader(DOC/'central-loc-m17-fix05-seal-success.json'))},
        'externalCommandReceiptRequirement':EXTERNAL_SOURCE_RECEIPT,'partialNewEnumerations':0,
        'SDKScan':False,'GETs':0,'restores':0,'native':0,'diskExtraction':0}

def prepare():
    """One new eight-leaf SOURCE seal; consume the existing partial list only."""
    started=time.monotonic();checks_path=E/'authority/offline-checks.json'
    _fix05_roots_absent()
    require(not os.path.lexists(REVIEW01_SIDECAR),'external command receipt already occupied')
    require({str(p) for p in E.rglob('*') if p.is_file()}=={str(DRIVER),str(checks_path)},
        'single REVIEW01 attempt / exact initial leaves')
    require(not any(p.is_symlink() for p in [E,*E.parents,*E.rglob('*')]),'SOURCE symlink forbidden')
    checks_bytes=read(checks_path);checks=strict(checks_bytes)
    require(checks_bytes==jb(checks),'canonical check metadata required before sealing')
    require(checks['status']=='DIRECTED_CHECKS_PASSED' and checks['attempts']==1 and checks['elapsedSeconds']<=10 and
        checks['caseCount']==checks['passed'],'one successful <=10-second REVIEW01 directed run required')
    require(checks['sourceIdentity']==identity(DRIVER),'source changed after directed checks')
    cached={}
    def caching_reader(p):
        p=Path(p)
        if p not in cached:cached[p]=read(p)
        return cached[p]
    protected=_fix05_protection(caching_reader)
    require(checks['protectedBefore']==protected,'protection differs from directed run')
    history=old_seals(caching_reader);partial=_review01_partial(caching_reader)
    m=model(caching_reader,partial=partial)
    require(sha(jb(m))==checks['modelSha256'] and _fix05_core_digest(m)==checks['coreModelSha256'],
        'directed/generated model identity differs')
    old_driver=caching_reader(OLD17F5/'source/m17_driver.py');driver=caching_reader(DRIVER)
    delta=_review01_delta(old_driver,driver);require(delta==checks['sourceDelta'],'source function delta differs')
    artifacts=source_artifacts(m,driver,old_driver,checks,history)
    receipt=strict(artifacts['authority/source-receipt.json'])
    receipt['sourceDelta']=delta
    receipt['mechanicalGeneration']={'entry':'m17_driver.py prepare','attempts':1,'maxSeconds':30,
        'sourceAuthor':OWNER,'SDKScan':False,'historicalSuitesRerun':False,'partialNewEnumerations':0,
        'externalCommandReceiptRequirement':EXTERNAL_SOURCE_RECEIPT}
    receipt['partialProtection']={'captureOwner':partial['owner'],'capturedAtUtc':partial['capturedAtUtc'],
        'files':len(partial['files']),'directories':len(partial['directories']),'manifestSha256':PARTIAL_CAPTURE_SHA,
        'rawManifest':'authority/prepare.json#/model/partialProtection','originalEnumerationsPerRoot':1,
        'newEnumerationsThisDelivery':0,'historicalExitAndFullLineageClosure':'UNKNOWN_NOT_INFERRED','materialAccepted':False}
    receipt['reviewFixes']={'4242623018':_review01_documents(caching_reader),
        '4242623020':{'historicalSuccessRecord':identity(DOC/'central-loc-m17-fix05-seal-success.json',
            caching_reader(DOC/'central-loc-m17-fix05-seal-success.json')),'currentExternalRequirement':EXTERNAL_SOURCE_RECEIPT}}
    receipt['modelPolicy']={'requestedModel':'gpt-6-astra','requestedEffort':'xhigh',
        'actualModelVisibleToSourceProcess':False,'actualAuthorTurn':OWNER['turnId']}
    artifacts['authority/source-receipt.json']=jb(receipt)
    # Validate every pre-existing leaf before any additional candidate write.
    require(artifacts['source/m17_driver.py']==read(DRIVER) and
        artifacts['authority/offline-checks.json']==checks_bytes,'immutable authored/checked input')
    frozen=seal_source(m,artifacts);memory={E/rel:b for rel,b in artifacts.items()}
    consume_source(frozen,lambda p:memory[p],m)
    for rel,b in artifacts.items():
        path=E/rel
        if path in (DRIVER,checks_path):continue
        require(not os.path.lexists(path),'new source leaf collision');path.parent.mkdir(parents=True,exist_ok=True)
        with path.open('xb') as stream:stream.write(b)
    freeze_path=E/'authority/source-freeze.json'
    with freeze_path.open('xb') as stream:stream.write(frozen)
    consume_source(read(freeze_path))
    require({str(p) for p in E.rglob('*') if p.is_file()}=={str(E/rel) for rel in PREP},'exact eight source leaves')
    require(_fix05_protection()==protected and old_seals()==history,'final historical preservation')
    _fix05_partial_unchanged(partial);_fix05_roots_absent()
    require(not os.path.lexists(REVIEW01_SIDECAR),'sidecar must be captured only after tool completion')
    leaves=[identity(E/rel) for rel in PREP];elapsed=time.monotonic()-started
    require(elapsed<=30,'REVIEW01 mechanical generation time cap')
    print(json.dumps({'status':'SOURCE_SEALED_EXTERNAL_RECEIPT_REQUIRED','sourceAuthor':OWNER,'checks':checks['passed'],
        'attempts':1,'directedSeconds':checks['elapsedSeconds'],'mechanicalFunctionSeconds':elapsed,
        'modelSha256':sha(jb(m)),'freeze':identity(freeze_path),'leaves':leaves,
        'totalBytes':sum(r['bytes'] for r in leaves),'sourceDelta':delta,
        'protectedCount':len(protected),'partialFiles':2576,'partialDirectories':909,'partialNewEnumerations':0,
        'GETs':0,'restores':0,'SDKScan':False,'historicalSuitesRerun':False,
        'externalCommandReceiptRequirement':EXTERNAL_SOURCE_RECEIPT},ensure_ascii=False))
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
