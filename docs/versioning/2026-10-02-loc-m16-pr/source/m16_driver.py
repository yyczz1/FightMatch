#!/opt/homebrew/bin/python3
"""M16 offline source preparation and separately authorized material execution.
Derived from sealed M15 helpers. Preparation never launches children or network.
"""
import argparse
import ast
import base64
import copy
import csv
import hashlib
import io
import json
import os
from pathlib import Path, PurePosixPath
import re
import stat
import sys
import time
import xml.etree.ElementTree as ET
import zipfile
from datetime import datetime, timezone

REPO = Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
DOC = REPO / 'docs/team/2026-09-30'
BASE = REPO / 'TestArtifacts/FightMatch/LOC-LIC-ALT-M'
def temp(n):
    return Path(f'/private/tmp/fightmatch-loc-lic-alt-m{n:02}/loc-lic-alt-yamldotnet-16.3.0-m{n:02}')
def evidence(n):
    return BASE / f'loc-lic-alt-yamldotnet-16.3.0-m{n:02}'
T09, T13, T = temp(9), temp(13), temp(16)
E13, E14, E = evidence(13), evidence(14), evidence(16)
M = Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m16')
G = BASE / 'license-gap-bodies-15'
DRIVER = E / 'source/m16_driver.py'
TABLE = DOC / 'engineering-loc-license-alt-m-materials-continuation-10-preflight.tsv'
SUPPORT = T09 / 'support/m09_support.py'
SOURCE = T09 / 'replay/m01/source-patched'
DOTNET = Path('/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/dotnet-x64/dotnet')
OWNER = {'threadId': '01a0f40d-b0c5-7bc0-a2b2-be9d1213648a', 'hostId': 'local',
         'turnId': '01a0f70e-ccb6-7ed0-ba29-f94ca6de4fdc'}
AUTHORITY = {'threadId': '01a0e401-511d-79f2-b47f-3ab0ade1681b', 'hostId': 'local',
             'activationItem': 'fco_01a0f70e-ccc0-77f2-97f5-f5e3a9e0c0cd',
             'scope': 'SOURCE_PREPARATION_ONLY'}
PREP = ('source/m16_driver.py', 'source/m16-inputs.tsv', 'source/m16-write-set.tsv',
        'authority/prepare.json', 'authority/offline-checks.json', 'authority/source-freeze.json')
PROTECTED = ('HOME', 'home', 'CODEX_HOME')

class GateError(RuntimeError):
    pass

def require(condition, reason):
    if not condition:
        raise GateError(reason)

def fail(code, message):
    raise GateError(code + ': ' + message)

def digest(data):
    return hashlib.sha256(data).hexdigest()

def safe_relative(value):
    require(isinstance(value, str) and value and '\\' not in value and
            not any(c in value for c in '\r\n\t\0'), 'unsafe relative path')
    p = PurePosixPath(value)
    require(not p.is_absolute() and str(p) == value and
            all(v not in ('', '.', '..') for v in p.parts), 'relative path traversal')
    return value

def raw(path, runtime=False):
    path = Path(path)
    if not runtime:
        for item in (path, *path.parents):
            require(not item.is_symlink(), 'symlink input: ' + str(item))
    require(path.is_file(), 'missing/nonregular input: ' + str(path))
    return path.read_bytes()

def identity(path, runtime=False):
    data = raw(path, runtime)
    return {'path': str(path), 'bytes': len(data), 'sha256': digest(data)}

def fixed(path, count, sha, runtime=False):
    item = identity(path, runtime)
    require((count is None or item['bytes'] == int(count)) and item['sha256'] == sha,
            'BLOCKED_INPUT_DRIFT: ' + str(path))
    return item

def files(root):
    require(root.is_dir() and not root.is_symlink(), 'missing/non-directory root: ' + str(root))
    found = []
    for here, dirs, names in os.walk(root, followlinks=False):
        for name in dirs + names:
            p = Path(here) / name
            mode = p.lstat().st_mode
            require(stat.S_ISREG(mode) or stat.S_ISDIR(mode), 'nonregular tree entry: ' + str(p))
        found.extend(Path(here) / name for name in names)
    return sorted(found, key=lambda p: p.relative_to(root).as_posix().encode())

def rows(path):
    data = raw(path)
    require(not data.startswith(b'\xef\xbb\xbf') and b'\r' not in data and data.endswith(b'\n'),
            'noncanonical TSV: ' + str(path))
    reader = csv.DictReader(io.StringIO(data.decode('utf-8')), delimiter='\t')
    require(reader.fieldnames and len(set(reader.fieldnames)) == len(reader.fieldnames), 'bad TSV header')
    result = list(reader)
    require(all(None not in x and all(v is not None for v in x.values()) for x in result), 'bad TSV field count')
    return result

def json_data(data):
    require(not data.startswith(b'\xef\xbb\xbf'), 'JSON BOM')
    def pairs(items):
        d = {}
        for key, value in items:
            require(key not in d, 'duplicate JSON key')
            d[key] = value
        return d
    def invalid(value):
        raise GateError('nonfinite JSON number: ' + value)
    return json.loads(data.decode('utf-8'), object_pairs_hook=pairs, parse_constant=invalid)

def jload(path):
    return json_data(raw(path))

def tsv_bytes(header, body):
    output = io.StringIO(newline='')
    writer = csv.writer(output, delimiter='\t', lineterminator='\n')
    writer.writerow(header)
    for row in body:
        require(all(not any(c in str(x) for c in '\t\r\n') for x in row), 'invalid TSV cell')
        writer.writerow(row)
    return output.getvalue().encode()

def json_bytes(value):
    return (json.dumps(value, indent=2, ensure_ascii=False, sort_keys=True, allow_nan=False) + '\n').encode()



ANCHORS = [
    (DOC/'engineering-loc-material-source-continuation-15.md',3676,'c0e9bbc3fd0f47ac5efb3adc2810ad231d5812671b34ce9f5b7585edea9b813f'),
    (DOC/'engineering-loc-material-restore-continuation-14.md',15676,'ca29cd4c575f2286307d386136f43d70ff2d33da868108acbc6dfba9c696e4d6'),
    (DOC/'engineering-loc-license-gap-resolution-15.md',6876,'4876606a646887b042e65b725025509d2c6cd88571c6530fdbace5cd6c1ecbc2'),
    (TABLE,23812,'b2e58332a0868ded905e3aae516c5e97ad3faea17c225e1de3a8a1f984634b16'),
    (E13/'result.json',30391,'53117ca8e44f113f3cf256b25d6da77c6d2cd241650d582e3b6f652f2ae9c481'),
    (E13/'evidence-files.tsv',879,'e424ab5f55763072c5e983a07cc839c26333e0b9db5382f62080ebb4f5d6559d'),
    (E13/'acquisition-files.tsv',10380,'4ae40c1194acd37e3c9db48f180ee58b1b6f9e9ab0410f7aa4557ff42f49e3ed'),
    (E13/'authority/signature-handoff.json',31253,'f0ac0e45c5058aa7e11dd0e00c67521c4ae7500fc72574f6d80ecc2e1cd18ed0'),
    (T13/'preflight/m13-inputs.tsv',19832,'93064be8f56eaaed29d00d10860fc10d5d70cddf99d847aaa48f3e2586d97f88'),
    (DOC/'testing-loc-signatures-005-receipt.md',6856,'c96c6579e9d2297dce6afa12a7b53273045b07b3dc8a219fcec5e06d4f8b6d70'),
    (BASE/'cloud-signature-005-received/retrieved-outputs.json',97891,'0d44d64993a5db23ae6cc6b2a7a120c36b0f722ad628859a7026582e387b0db5'),
    (G/'files.tsv',None,'375966074d1221862c70e10bee51aa49d2abf1830355b07a16e921ccecb9deed'),
    (G/'receipt.json',9870,'7af32c1887d18ce3e94982af2da8015b9a8c7433da005279f51a3faf58df28d4'),
    (E14/'authority/prepare.json',7163,'d15c5ec8618c1c9d26eacbb7136dfe68d44990817b6213632d6b8a914ef2945b'),
    (E14/'authority/offline-checks.json',4406,'57143860f3d9a3e321f9f5ffd94af4d83899d4303787526196ed89e887f25b86'),
    (E14/'source/m14-inputs.tsv',3117,'52b18204e378444823175ff6e5c0144e4ddb07e1acce3db4bb0e82c7722336c0'),
    (E14/'authority/source-freeze.json',1247,'f61f8d9e4ce02a1eb0b2d34fb2e8b6b14910d5213cca97ff5f9577107c86167e'),
]


def packages(selected):
    paths=[Path(p) for p in selected if p.endswith('.nupkg') and
           (Path(p).parent==T09/'closure/tests-generation/feed' or Path(p).is_relative_to(T13/'acquisition/packages'))]
    require(len(paths)==31,'feed must contain 31 exact archives')
    result=[]
    for path in sorted(paths,key=lambda p:p.name):
        with zipfile.ZipFile(path) as z:
            names=[x.filename for x in z.infolist() if not x.is_dir()]
            require(len(names)==len(set(names)),'duplicate ZIP entry')
            for entry in z.infolist():
                # Fixed Scriban/analyzer archives contain doubled separators.
                # Preserve ZIP names for reads; canonicalize only output paths.
                require(not entry.filename.startswith('/') and '\\' not in entry.filename and
                        not any(x in ('.','..') for x in entry.filename.split('/')), 'unsafe ZIP path')
                safe_relative(PurePosixPath(entry.filename.rstrip('/')).as_posix())
                require(not stat.S_ISLNK(entry.external_attr>>16),'ZIP symlink')
            canonical_names=[PurePosixPath(n).as_posix() for n in names]
            require(len(canonical_names)==len(set(canonical_names)),'normalized ZIP entry collision')
            specs=[n for n in names if n.endswith('.nuspec')]
            require(len(specs)==1,'exactly one nuspec required')
            spec=z.read(specs[0]);root=ET.fromstring(spec)
            d={x.tag.split('}')[-1]:x for x in root.iter()}
            package_id=d['id'].text;version=d['version'].text
            require(path.name==f'{package_id.lower()}.{version}.nupkg','package path mismatch')
            repo=d.get('repository');license_node=d.get('license')
            body_names=[n for n in names if Path(n).name.lower().startswith(('license','copying','notice','third-party'))]
            result.append({'id':package_id,'version':version,'path':str(path),'identity':identity(path),
                'key':f'{package_id.lower()}.{version}','sha512Base64':base64.b64encode(hashlib.sha512(raw(path)).digest()).decode(),
                'nuspecEntry':specs[0],'nuspecBytes':len(spec),'nuspecSha256':digest(spec),
                'repository':repo.attrib if repo is not None else {},
                'declaredLicense':license_node.text if license_node is not None else None,
                'embeddedBodies':[{'entry':n,'bytes':len(z.read(n)),'sha256':digest(z.read(n))} for n in body_names],
                'entries':canonical_names})
    require(len({p['key'] for p in result})==31,'duplicate package key')
    return result


def source_graphs():
    saved=jload(T09/'state/graphs.json'); result={}
    for name,counts in [('runner',(23,56)),('tests',(13,29))]:
        visited=set();edges=set();active=set()
        def visit(rel):
            require(rel not in active,'project cycle');
            if rel in visited:
                return
            active.add(rel);tree=ET.fromstring(raw(SOURCE/safe_relative(rel)))
            for node in tree.iter():
                if node.tag.split('}')[-1]=='ProjectReference':
                    include=node.attrib['Include'].replace('\\','/')
                    child=(SOURCE/rel).parent.joinpath(include).resolve().relative_to(SOURCE).as_posix()
                    edges.add((rel,child));visit(child)
            active.remove(rel);visited.add(rel)
        visit(saved[name]['root'])
        require((len(visited),len(edges))==counts and visited==set(saved[name]['nodes']),'graph closure mismatch')
        existing=rows(T09/f'stage-material/identity/{name}-project-edges.tsv')
        require(edges=={(x['parentProjectRelativePath'],x['childProjectRelativePath']) for x in existing},'complete edges differ')
        result[name]={'nodes':sorted(visited),'edgeCount':len(edges),'root':saved[name]['root']}
    manifest=rows(T09/'comparison/patched-source-base.tsv'); require(len(manifest)==931,'source count')
    changed=[];before=temp(1)/'source-official'
    old=b'<PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />'
    new=b'<PackageReference Include="YamlDotNet" Version="16.3.0" />'
    for row in manifest:
        rel=row[next(iter(row))];a=raw(before/rel);b=raw(SOURCE/rel)
        if a!=b:
            require(a.count(old)==1 and a.replace(old,new)==b,'extra source patch')
            changed.append(rel)
    require(set(changed)=={'src/Luban/Luban.csproj','src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj'},'two patch targets')
    return result,manifest

def child_environment(layout):
    child_env = os.environ.copy()
    updates={'DOTNET_CLI_HOME':str(layout['dotnet_home']),'NUGET_PACKAGES':str(layout['packages']),
        'NUGET_HTTP_CACHE_PATH':str(layout['http']),'NUGET_PLUGINS_CACHE_PATH':str(layout['plugins']),
        'NUGET_SCRATCH':str(layout['scratch']),'TMPDIR':str(layout['tmp']),
        'DOTNET_GENERATE_ASPNET_CERTIFICATE':'false','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH':'false'}
    require(not set(updates).intersection(PROTECTED),'protected environment assignment')
    child_env.update(updates)
    equality={k:(k in child_env)==(k in os.environ) and child_env.get(k)==os.environ.get(k) for k in PROTECTED}
    require(all(equality.values()),'parent environment inheritance differs')
    return child_env,equality

def phase_layout(phase):
    require(phase in ('generation','verification'),'phase invalid')
    base=T/f'tests-{phase}'
    return {'base':base,'source':base/'source','feed':T/'local-feed','config':base/'NuGet.Config',
            'packages':base/'packages','dotnet_home':base/'dotnet-home','http':base/'http-cache',
            'plugins':base/'plugins-cache','scratch':base/'scratch','tmp':base/'tmp'}

def restore_argv(phase):
    l=phase_layout(phase)
    props={'RestoreConfigFile':l['config'],'RestorePackagesPath':l['packages'],'RestoreSources':l['feed'],
        'RestoreFallbackFolders':'','RestoreDisableParallel':'true','RestoreNoHttpCache':'true',
        'RestoreIgnoreFailedSources':'false','RestorePackagesWithLockFile':'true','NuGetAudit':'false'}
    props['RestoreForceEvaluate' if phase=='generation' else 'RestoreLockedMode']='true'
    return [str(DOTNET),'msbuild',str(l['source']/'src/Luban.Tests/Luban.Tests.csproj'),
            '-target:Restore','-maxCpuCount:1','-nodeReuse:false','-noAutoResponse','-verbosity:minimal']+[
            f'-property:{key}={value}' for key,value in props.items()]

def sdk_patterns(phase):
    require(phase in ('generation','verification'),'invalid SDK pattern phase')
    return [(f'tests-{phase}/dotnet-home/.dotnet/TelemetryStorageService/'+r'[0-9]{14}_[0-9a-f]{32}\.trn',500,1024*1024),
            (f'tests-{phase}/scratch/lock/'+r'[0-9a-f]{40}',4096,64*1024)]

def sandbox_profile(exact_files,phase=None):
    """Candidate child policy, never installed by preparation; OS proof pending.

    Deny all network and writes, then grant only individually listed files and
    ancestor directories. No subpath write grants. Child forks are prohibited.
    """
    paths={str(Path(p)) for p in exact_files}
    require(all(Path(p).is_relative_to(T) and not any(x in p for x in ('\n','\r','\0')) for p in paths),
            'sandbox file outside task temp')
    dirs={str(parent) for p in paths for parent in Path(p).parents if parent.is_relative_to(T)}
    lines=['(version 1)','(allow default)','(deny network*)','(deny file-write*)',
           '(deny process-fork)','(deny process-exec)',
           '(allow process-exec (literal '+json.dumps(str(DOTNET))+'))']
    lines += ['(allow file-write* (literal '+json.dumps(p)+'))' for p in sorted(paths)]
    lines += ['(allow file-write-create (literal '+json.dumps(p)+'))' for p in sorted(dirs)]
    if phase is not None:
        for relative,_,_ in sdk_patterns(phase):
            # Prefix escaped literally; only the two fixed SDK suffixes vary.
            prefix,suffix=relative.rsplit('/',1)
            pattern='^'+re.escape(str(T/prefix))+'/'+suffix+'$'
            lines.append('(allow file-write* (regex #'+json.dumps(pattern)+'))')
            parent=T/prefix
            for directory in (parent,*[p for p in parent.parents if p.is_relative_to(T)]):
                lines.append('(allow file-write-create (literal '+json.dumps(str(directory))+'))')
    return '\n'.join(lines)+'\n'

def install_child_sandbox(profile):
    """Future child-only preexec hook; preparation never calls this function.

    Applying a profile is a hard launch prerequisite, not an assertion that
    static profile text proves OS enforcement. The child cannot fork; dotnet
    may be the only exec, and file/network restrictions persist across exec.
    Failure raises before dotnet is launched. No sandbox-exec helper process.
    """
    import ctypes
    require(sys.platform=='darwin','BLOCKED_OS_ISOLATION_PLATFORM')
    library=ctypes.CDLL('/usr/lib/libsandbox.dylib',use_errno=True)
    library.sandbox_init.argtypes=[ctypes.c_char_p,ctypes.c_uint64,ctypes.POINTER(ctypes.c_char_p)]
    library.sandbox_init.restype=ctypes.c_int
    library.sandbox_free_error.argtypes=[ctypes.c_char_p]
    error=ctypes.c_char_p()
    result=library.sandbox_init(profile.encode(),0,ctypes.byref(error))
    if result!=0:
        if error.value:
            library.sandbox_free_error(error)
        raise GateError('BLOCKED_OS_ISOLATION_INIT')


def check_activation(activation,freeze,freeze_sha,source_sha,argv):
    """Pure data gate; absence of any authority/review binding fails closed."""
    require(isinstance(activation,dict),'BLOCKED_EXECUTION_NOT_AUTHORIZED')
    require(freeze.get('sourceReady') is True and freeze.get('executorComplete') is True and
            freeze.get('materialWriteSetComplete') is True,'BLOCKED_SOURCE_NOT_READY')
    require(activation.get('centralThread')==AUTHORITY['threadId'] and
            activation.get('scope')=='EXECUTE_M16' and activation.get('ownerThread')==OWNER['threadId'] and
            activation.get('ownerHost')=='local' and bool(activation.get('ownerTurn')),'BLOCKED_ACTIVATION_IDENTITY')
    require(re.fullmatch('[0-9a-f]{40}',activation.get('reviewedHead','')) is not None and
            activation.get('reviewedHead')==activation.get('executionHead') and
            activation.get('reviewStatus')=='COMPLETED_NO_UNRESOLVED_FINDINGS','BLOCKED_REVIEW_HEAD')
    require(activation.get('sourceFreezeSha256')==freeze_sha and
            activation.get('driverSha256')==source_sha and activation.get('argv')==argv,
            'BLOCKED_ACTIVATION_SOURCE_BINDING')
    require(activation.get('restoreLimits')=={'runner':0,'testsGeneration':1,'testsVerification':1},
            'BLOCKED_RESTORE_LIMITS')
    return True

def restore_budget_contract():
    return {'wallSeconds':1800,'eachRestoreSeconds':180,'killGraceSeconds':5,
            'stdoutMaxBytes':4*1024*1024,'stderrMaxBytes':4*1024*1024,
            'tempMaxBytes':768*1024*1024,'evidenceMaxBytes':16*1024*1024,
            'materialMaxBytes':512*1024*1024,'materialMaxFiles':25000,
            'freeFloorBytes':2*1024*1024*1024,'maxConcurrentRestores':1,
            'maxTopLevelCommands':64,'maxProcessStarts':20000,'maxPythonStarts':8,
            'runnerRestores':0,'testsGenerationRestores':1,'testsVerificationRestores':1}

def run_isolated_restore(phase,exact_output_files,activation,freeze,freeze_sha,source_sha,run_state):
    """Guarded restore primitive, reachable only after explicit execute authority.

    Caller must have completed material/network/output prerequisites before
    reaching this primitive. It never retries and only terminates its owned
    process group. Bounded streams are returned to the caller for receipts.
    The source-preparation fixtures do not spawn or invoke libsandbox.
    """
    import selectors
    import signal
    import subprocess
    argv=restore_argv(phase)
    check_activation(activation,freeze,freeze_sha,source_sha,
                     ['/opt/homebrew/bin/python3','-B',str(DRIVER),'execute'])
    require(run_state.get('allMaterialPrerequisitesPassed') is True,'BLOCKED_MATERIAL_PREREQUISITES')
    require(run_state.get('nextPhase')==phase,'BLOCKED_RESTORE_ORDER_OR_RETRY')
    require(time.monotonic()-run_state['activationMonotonic']<=1800,'BLOCKED_WALL_BUDGET')
    require(not run_state.get('running') and phase not in run_state.get('attempted',[]),'BLOCKED_RESTORE_RETRY')
    layout=phase_layout(phase);child,equalities=child_environment(layout)
    allowed_records=[r for r in rows(E/'source/m16-write-set.tsv') if r['root']=='T16' and
                     r['matchKind']=='exact' and r['relativePath'].startswith(f'tests-{phase}/') and
                     r['role']=='restore-output']
    require({str(p) for p in exact_output_files}=={str(T/r['relativePath']) for r in allowed_records},
            'BLOCKED_RESTORE_OUTPUT_SCOPE')
    profile=sandbox_profile(exact_output_files,phase)
    run_state.setdefault('attempted',[]).append(phase);run_state['running']=True
    started=time.monotonic();process=None;selector=selectors.DefaultSelector()
    streams={'stdout':bytearray(),'stderr':bytearray()};failure=None
    try:
        process=subprocess.Popen(argv,cwd=layout['source'],env=child,stdin=subprocess.DEVNULL,
            stdout=subprocess.PIPE,stderr=subprocess.PIPE,start_new_session=True,
            preexec_fn=lambda:install_child_sandbox(profile))
        for label,stream in [('stdout',process.stdout),('stderr',process.stderr)]:
            os.set_blocking(stream.fileno(),False);selector.register(stream,selectors.EVENT_READ,label)
        last_budget=started
        while selector.get_map():
            require(time.monotonic()-started<=180,'BLOCKED_RESTORE_TIMEOUT')
            require(time.monotonic()-run_state['activationMonotonic']<=1800,'BLOCKED_WALL_BUDGET')
            if time.monotonic()-last_budget>=1:
                budget_gate(run_state['writer'],run_state['activationMonotonic']);last_budget=time.monotonic()
            for key,_ in selector.select(0.1):
                chunk=os.read(key.fd,65536)
                if not chunk:
                    selector.unregister(key.fileobj);key.fileobj.close();continue
                target=streams[key.data]
                room=4*1024*1024-len(target)
                target.extend(chunk[:room]);require(len(chunk)<=room,'BLOCKED_LOG_BUDGET')
        exit_code=process.wait(timeout=max(0.001,180-(time.monotonic()-started)))
        require(exit_code==0,'BLOCKED_RESTORE_EXIT')
        combined=bytes(streams['stdout'])+bytes(streams['stderr'])
        require(b'NU1101' not in combined and b'NU1605' not in combined,'BLOCKED_PACKAGE_CLOSURE')
        run_state['nextPhase']='verification' if phase=='generation' else 'seal'
    except Exception as error:
        failure=type(error).__name__+': '+str(error)
        run_state['nextPhase']='stopped'
    finally:
        if process is not None and process.poll() is None:
            os.killpg(process.pid,signal.SIGTERM)
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                os.killpg(process.pid,signal.SIGKILL);process.wait(timeout=5)
        selector.close();run_state['running']=False
    return {'phase':phase,'argv':argv,'exitCode':None if process is None else process.returncode,
        'durationSeconds':time.monotonic()-started,'failure':failure,
        'environmentEquality':equalities,'sandboxProfileSha256':digest(profile.encode()),
        'sandboxAppliedBeforeDotnetExec':process is not None,'externalNetworkProbesRun':False,
        'stdout':bytes(streams['stdout']),'stderr':bytes(streams['stderr'])}

def package_signature_projections(pkgs):
    bundle=jload(BASE/'cloud-signature-005-received/retrieved-outputs.json')
    indexed={x['id']:x for x in bundle['items']}
    triples=[('system.reflection.metadata.1.6.0','exec-661d9557-e8a8-4079-8d02-a3ede2c3a745','cc429ca95a6d41d4402ac1c8f6ae973bcfb35e776187ee977e859f6aec8656e7'),
             ('system.collections.immutable.1.5.0','exec-9a07ffe2-7121-4662-8603-c6968529d72a','c557f9a649986b94097fee91b284a3aba7aa1070bbd4a740e41010d73fe6eb66'),
             ('xunit.abstractions.2.0.3','exec-b93b4890-ddc4-41b9-8254-e11fb8cf410d','b3e0820f8385be573c9d0abb451c303fa7c5f7832c85a5927e8edfb81988a380')]
    bykey={p['key']:p for p in pkgs};result=[]
    for key,item_id,sha in triples:
        original=BASE/f'cloud-signature-005-received/{key}.observed-receipt.json'
        d=jload(original);out=d['stdout'].encode();err=d.get('stderr','').encode()
        require(d['exit']==0 and digest(out)==sha and digest(out)==d['stdout_sha256'] and
                len(out)==d['stdout_bytes'] and digest(err)==d['stderr_sha256'] and len(err)==d['stderr_bytes']==0,
                'signature projection raw identity drift')
        text=out.decode();require('Finished with 0 errors and 8 warnings.' in text and
                text.count('NU3018')==4 and text.count('NU3028')==4 and
                'Successfully verified package' in text,'signature bounded verdict differs')
        item=indexed[item_id];require(sha in item['output']['text'] and item['exitCode']==0,'signature tool projection differs')
        pkg=bykey[key]
        with zipfile.ZipFile(pkg['path']) as z:
            sig=z.read('.signature.p7s')
        observed_sig=T13/f'acquisition/packages/{pkg["id"].lower()}/{pkg["version"]}/entries/.signature.p7s'
        require(sig==raw(observed_sig),'signature original export mismatch')
        result.append({'kind':'local-projection','packageKey':key,'packageSha256':pkg['identity']['sha256'],
            'signatureSha256':digest(sig),'sourceToolItem':item_id,'sourceReceipt':identity(original),
            'sourceQA':str(DOC/'testing-loc-signatures-005-receipt.md'),'errorsFromActualSummary':0,
            'warnings':{'NU3018':4,'NU3028':4},'historicalSummaryFieldErrors':d.get('errors'),
            'signatureReverified':False,'onlineRevocationProven':False,'sourceEnvironmentOmittedProtectedKeys':True,
            'cloudAggregateRetrieved':False,'stdoutBytes':len(out),'stdoutSha256':sha,'stderrBytes':0})
    return result

def canonical_tools():
    # Reuse the five exact M09 core functions, not its flawed negative-fixture wrapper.
    fixed(SUPPORT,42884,'d84d050a9d59d0caa482457c567b2779826d527af10770015b47fa57295fe7c6')
    text=raw(SUPPORT).decode(); tree=ast.parse(text)
    names={'strict_json','path_token','transform','scan_residual','fixture_canonical'}
    nodes=[x for x in tree.body if isinstance(x,ast.FunctionDef) and x.name in names]
    require({n.name for n in nodes}==names,'M09 canonical core unavailable')
    context={'json':json,'copy':copy,'fail':fail}
    exec(compile(ast.Module(body=nodes,type_ignores=[]),str(SUPPORT),'exec'),context)
    source='\n'.join(ast.get_source_segment(text,n) for n in nodes).encode()
    return context,digest(source)

H16=BASE/'license-closure-bodies-16'
H17=BASE/'license-catalog-bindings-17'
H17_AUTH={'receiptSha256':'14aba5d22286131679429bff73f7721b63d828439b506bb3b726be62c30dce4c'}
H18=BASE/'license-catalog-bindings-18'
H18_AUTH={'receiptBytes':6171,'receiptSha256':'f1e118f290c0e69d8f888a880c592472ff8beba6abd1d2998de6ce5c27cea201'}
E15=evidence(15)
OLD_TOOL=DOTNET.parent.parent
BROOT=Path('/private/tmp/fightmatch-loc-lic-alt-b01/loc-lic-alt-yamldotnet-16.3.0-b01')
LROOT=Path('/private/tmp/fightmatch-loc-lic-alt-l01/loc-lic-alt-yamldotnet-16.3.0-l01')
STAGE=T/'stage-material'
ROOTS={'T16':T,'E16':E,'M16':M}
DOTNET_SHA='e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686'
PYTHON_SHA='a708f6e9f4803b806b29146c4e0feecfd9bf2d9eb60f3e15b850cd7cb56f200b'
FIX02=REPO/'TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02'
P005=REPO/'TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005'
SCHEMAS={
 'upstream-source.tsv':'component versionOrTag repositoryUrl fullCommit treeId transportUrl transportBytes transportSha256 localRoot fileManifestSha256',
 'source-files.tsv':'component relativePath bytes sha256 gitObjectId role',
 'patch.tsv':'targetRelativePath preBytes preSha256 postBytes postSha256 oldPackageId oldVersion newPackageId newVersion patchSha256',
 'package-files.tsv':'graph packageId version relativePath bytes sha256 catalogSha512 catalogUrl immutablePackageUrl owner signaturePath signatureSha256 signatureVerdict',
 'package-entries.tsv':'graph projectRelativePath packageId version entryPath assetKind targetFramework bytes sha256 selected expectedDeploymentPath',
 'package-closure.tsv':'graph projectRelativePath parentId parentVersion packageId version declaredRange resolutionRule dependencyKind targetFramework includeAssets privateAssets lockEdge',
 'license-files.tsv':'component version expression sourceUrl sourceTag sourceCommit sourcePath localRelativePath bytes sha256 packageRelationshipEvidence',
 'expected-deployment.tsv':'ownerKind ownerId ownerVersion sourceEntryOrProject expectedRunnerRelativePath assetKind licenseRelativePath licenseSha256',
 'official-test-project.tsv':'projectRelativePath bytes sha256 targetFramework referenceKind referenceId versionOrPath includeAssets privateAssets',
 'old-roots.tsv':'root relativePath bytes sha256 phase',
 'material-files.tsv':'relativePath bytes sha256 kind authorityId sourceReceiptId',
 'tests-projects.tsv':'projectRelativePath csprojBytes csprojSha256 targetFramework isRoot',
 'tests-project-edges.tsv':'parentProjectRelativePath childProjectRelativePath includeLiteral referenceOutputAssembly',
 'tests-lock-set.tsv':'projectRelativePath lockRelativePath lockBytes lockSha256 rawAssetsRelativePath rawAssetsBytes rawAssetsSha256 canonicalAssetsRelativePath canonicalAssetsBytes canonicalAssetsSha256 directPackageCount transitivePackageCount projectReferenceCount',
 'restore-locked-mode.tsv':'graph phase projectRelativePath present type value rawAssetsSha256 verdict',
}

def attach_input(inputs,path,count,sha,authority,verify=True):
    path=Path(path)
    if verify:
        info=fixed(path,count,sha,path in (DOTNET,Path('/opt/homebrew/bin/python3')))
        count=info['bytes']
    key=str(path);row=[key,int(count),sha,authority]
    require(key not in inputs or inputs[key][1:3]==row[1:3],'input authority disagreement')
    inputs[key]=row

def sealed_tree(inputs,root,receipt_sha,manifest_name='files.tsv'):
    receipt=fixed(root/'receipt.json',None,receipt_sha);d=jload(root/'receipt.json')
    manifest=d['manifest']
    attach_input(inputs,root/'receipt.json',receipt['bytes'],receipt_sha,'central accepted receipt')
    attach_input(inputs,root/manifest_name,manifest['bytes'],manifest['sha256'],str(root/'receipt.json'))
    expected={root/'receipt.json',root/manifest_name}
    for row in rows(root/manifest_name):
        p=root/safe_relative(row['relativePath']);expected.add(p)
        attach_input(inputs,p,row['bytes'],row['sha256'],str(root/manifest_name))
    require(set(files(root))==expected,'sealed input set drift: '+str(root))
    return d

def m16_inputs():
    inputs={}
    fixed(E15/'authority/source-freeze.json',5150,'a9b591becccf4596a07f04568fb68e9b8d1043b0e186f569fb96826f2387471e')
    seal=jload(E15/'authority/source-freeze.json')
    attach_input(inputs,E15/'authority/source-freeze.json',5150,'a9b591becccf4596a07f04568fb68e9b8d1043b0e186f569fb96826f2387471e','central M16 activation')
    for item in seal['filesExcludingThisSeal']:
        attach_input(inputs,item['path'],item['bytes'],item['sha256'],'M15 sealed snapshot')
    require({p.relative_to(E15).as_posix() for p in files(E15)}==set(seal['expectedSixLeaves']),'M15 set drift')
    # Reuse the previously derived finite set, not the obsolete M10 checker.
    for row in rows(E15/'source/m15-inputs.tsv'):
        attach_input(inputs,row['sourcePath'],row['bytes'],row['sha256'],row['authority'])
    attach_input(inputs,DOC/'engineering-loc-material-source-continuation-16.md',3381,
                 '306eb4a52d5d65e3e61d92c02b91339cd3ff8333d69639d688a7cc5c3690e2d6','M16 activation')
    h=sealed_tree(inputs,H16,'3e2cfb8b0671cf713cfd5626ec38920fd4aa8a3fa2608009e23694909079280d')
    require(h['counts']['packages']==31 and h['counts']['fullBodySetsAvailable']==31,'H16 coverage drift')
    fixed(H16/'standard/Apache-2.0.txt',11358,'cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30')
    pending=[]
    if H17_AUTH is None:
        pending=[{'kind':'CENTRAL_H17_RECEIPT_NOT_BOUND','keys':[x['key'] for x in h['remainingInputGaps']]}]
    else:
        sealed_tree(inputs,H17,H17_AUTH['receiptSha256'])
    if H18_AUTH is not None:
        attach_input(inputs,H18/'receipt.json',H18_AUTH['receiptBytes'],H18_AUTH['receiptSha256'],'central accepted J18 receipt')
        r=jload(H18/'receipt.json');mf=r['manifest']
        attach_input(inputs,H18/'files.tsv',mf['bytes'],mf['sha256'],'central accepted J18 manifest')
        for row in rows(H18/'files.tsv'):
            if row['relativePath'] in ('bindings.json','observations.json'):
                attach_input(inputs,H18/row['relativePath'],row['bytes'],row['sha256'],'central authorized four-leaf J18 overlay')
    else:
        pending.append({'kind':'CENTRAL_H18_RECEIPT_NOT_BOUND','keys':[
            'messagepackanalyzer.3.1.7','microsoft.net.stringtools.17.11.4','ude.netstandard.1.2.0',
            'neolua.1.3.15','exceldatareader.3.7.0','google.protobuf.3.29.0','messagepack.3.1.7',
            'messagepack.annotations.3.1.7','nlog.5.3.4','scriban.5.10.0']})
    attach_input(inputs,Path('/private/tmp/yamldotnet-v16.3.0.tar.gz'),397462,
                 '4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce','H16 exact source archive relationship')
    return inputs,pending

def body_source(hprov,body,observations):
    src=body['source'];kind=src['kind']
    if kind=='G-reused-body':
        p=jload(G/hprov['reusedG']['provenance'])
        require(p['sourceCommit']==hprov['sourceCommit'] and p['licenseBody']['sha256']==body['sha256'],
                'G reused source/body relationship differs')
        return p['sourceUrl'],p['sourcePath']
    if kind=='nupkg-entry':
        return hprov['package']['immutablePackageUrl'],src['entry']
    if kind=='existing-source-archive-entry':
        relative=src['entry'].split('/',1)[1]
        return hprov['sourceRepository']+'/blob/'+hprov['sourceCommit']+'/'+relative,relative
    require(kind=='github-blob','unsupported H16 source kind')
    observation=observations[src['observationId']]
    require(observation['expectedBlob']==body['gitBlobSha1'],'source blob differs')
    return observation['url'],observation['path']

def license_model(pkgs,pending):
    coverage=jload(H16/'coverage-31.json');bykey={p['key']:p for p in pkgs}
    require({p['key'] for p in coverage['packages']}==set(bykey),'31-package license join')
    observations={x['id']:x for x in jload(H16/'source-observations.json')['observations']}
    result={}
    for item in coverage['packages']:
        key=item['key'];pkg=bykey[key];out={'key':key,'packageSha256':pkg['identity']['sha256'],
            'packageSha512':pkg['sha512Base64'],'origin':item['origin'],'bodies':[]}
        if item['origin']=='G15_REUSED':
            p=jload(G/f'provenance/{key}.json');pp=p['package']
            require(pp['sha256']==pkg['identity']['sha256'] and pp['nuspecSha256']==pkg['nuspecSha256'],'G own package drift')
            body=p['licenseBody'];fixed(G/body['relativePath'],body['bytes'],body['sha256'])
            out.update({'expression':p['licenseExpression'],'provenance':p,'provenanceRelativePath':f'licenses/G15/provenance/{key}.json'})
            out['bodies'].append({'localRelativePath':'licenses/G15/'+body['relativePath'],
                'sourceInput':str(G/body['relativePath']),'sourceUrl':p['sourceUrl'],'sourcePath':p['sourcePath'],
                'sourceTag':p.get('sourceTag','N/A_EXACT_COMMIT'),'sourceCommit':p['sourceCommit'],
                'expression':p['licenseExpression'],'bytes':body['bytes'],'sha256':body['sha256'],'role':'full composite license'})
        elif item['origin']=='M13_REUSED':
            out.update({'expression':item['inheritedRowsUnchanged'][0]['expression'],'provenance':item,
                        'provenanceRelativePath':'licenses/H16/coverage-31.json'})
            for row in item['inheritedRowsUnchanged']:
                source=T13/'acquisition'/row['localRelativePath'];fixed(source,row['bytes'],row['sha256'])
                out['bodies'].append({**{k:row[k] for k in ('sourceUrl','sourceTag','sourceCommit','sourcePath','expression','sha256')},
                    'bytes':int(row['bytes']),'localRelativePath':'licenses/M13/'+row['localRelativePath'],
                    'sourceInput':str(source),'role':row['expression']})
        else:
            require(item['origin']=='H16','unexpected license origin')
            p=jload(H16/f'provenance/{key}.json');pp=p['package']
            require(pp['sha256']==pkg['identity']['sha256'] and pp['sha512Base64']==pkg['sha512Base64'] and
                    pp['nuspecSha256']==pkg['nuspecSha256'],'H16 own package drift')
            out.update({'expression':p['licenseExpression'],'provenance':p,'provenanceRelativePath':f'licenses/H16/provenance/{key}.json'})
            for body in p['licenseBodies']:
                b=raw(H16/body['relativePath']);require(digest(b)==body['sha256'] and len(b)==body['bytes'],'H16 body drift')
                url,sourcepath=body_source(p,body,observations)
                # Ude's .NOTICE slot contains LGPL terms, not a NOTICE verdict.
                role=body['role'];expression='NOTICE' if role=='third-party notice' else p['licenseExpression']
                out['bodies'].append({'sourceInput':str(H16/body['relativePath']),
                    'localRelativePath':'licenses/H16/'+body['relativePath'],'sourceUrl':url,'sourcePath':sourcepath,
                    'sourceTag':p.get('sourceTag','N/A_OFFICIAL_BINARY_PACKAGE' if p['evidenceLane']=='binary-package/embedded-license' else 'N/A_EXACT_COMMIT'),
                    'sourceCommit':p['sourceCommit'],'expression':expression,'bytes':len(b),'sha256':digest(b),'role':role})
        require(out['bodies'],'missing full body set')
        out['standardCompanion']={'path':'licenses/H16/standard/Apache-2.0.txt',
            'sha256':'cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30',
            'scope':'standard terms only; does not change the package/source relationship'}
        result[key]=out
    return result

def aggregate_identity(root):
    if not root.exists():
        return {'exists':False,'files':0,'bytes':0,'sha256':digest(b'')}
    entries=[]
    for p in files(root):
        i=identity(p);entries.append([p.relative_to(root).as_posix(),i['bytes'],i['sha256']])
    b=''.join('\t'.join(map(str,x))+'\n' for x in entries).encode()
    return {'exists':True,'files':len(entries),'bytes':sum(x[1] for x in entries),'sha256':digest(b)}

def preservation_contract(inputs):
    # Successor receipts already bind complete roots; never refreeze observed
    # bytes as though they were historical authority.
    m13pre=E13/'authority/preflight.json';m13a=E13/'authority/activation.json'
    require(str(m13pre) in inputs and str(m13a) in inputs,'M13 evidence receipt not bound')
    a=jload(m13a);expected=dict(a['entryPrecheck']['oldRoots'])
    m12pre=temp(12)/'preflight/preflight.json'
    info=a['entryPrecheck']['m12Preflight'];attach_input(inputs,m12pre,info['bytes'],info['sha256'],'M13 entry precheck')
    expected.update(jload(m12pre)['oldRootsBefore'])
    # M13 itself is bound by manifest + its result temp seal, not by new hashes.
    r=jload(E13/'result.json');expected[str(T13)]=r['temp']
    for n in (12,13,14,15):
        material=Path(str(M).replace('-m16',f'-m{n:02}'))
        expected[str(material)]={'exists':False,'files':0,'bytes':0,'sha256':digest(b'')}
    expected[str(temp(14))]={'exists':False,'files':0,'bytes':0,'sha256':digest(b'')}
    expected[str(temp(15))]={'exists':False,'files':0,'bytes':0,'sha256':digest(b'')}
    # These exact signed manifests protect newer roots and the old tool cache.
    protected=[]
    names={'old-cache':OLD_TOOL,'preflight-005':P005,
           'loc-fix-02':FIX02,'loc-fix-01':Path(str(FIX02).replace('-fix-02','-fix-01')),
           'loc-original':Path(str(FIX02).replace('-fix-02',''))}
    for name,root in names.items():
        hashes=temp(1)/f'identity/{name}.hashes';sizes=temp(1)/f'identity/{name}.sizes'
        require(str(hashes) in inputs and str(sizes) in inputs,'unbound protected manifest')
        hs=dict(line.rsplit('|',1) for line in raw(hashes).decode().splitlines())
        ss=dict(line.rsplit('|',1) for line in raw(sizes).decode().splitlines())
        require(set(hs)==set(ss),'old hash/size set differs')
        for rel in sorted(hs):
            protected.append([str(root),safe_relative(rel),int(ss[rel]),hs[rel]])
            attach_input(inputs,root/rel,int(ss[rel]),hs[rel],str(hashes)+' + '+str(sizes),verify=False)
    production=evidence(1)/'before/production-paths.tsv'
    attach_input(inputs,production,1005,'e5789ba9899f9073b46ce15c7f9d81b452bc3d1b19765e742f04d3cd839c8d74',
                 'immutable M01 evidence manifest + inherited root seal')
    for row in rows(production):
        rel=row[next(iter(row))];count=int(row['bytes']);sha=row['sha256']
        protected.append([str(REPO),rel,count,sha]);attach_input(inputs,REPO/rel,count,sha,str(production))
    return {'aggregateRoots':expected,'explicitLeaves':protected,
            'manifestBoundRoots':[str(E13),str(E14),str(E15),str(G),str(H16)]+
                ([str(H17)] if H17_AUTH else [])+([str(H18)] if H18_AUTH else [])}

def verify_preservation(contract,inputs):
    for root,expected in contract['aggregateRoots'].items():
        observed=aggregate_identity(Path(root))
        wanted={k:expected[k] for k in observed}
        require(observed==wanted,'BLOCKED_OLD_ROOT_DRIFT: '+root)
    for root,rel,count,sha in contract['explicitLeaves']:
        fixed(Path(root)/rel,count,sha)
    for p,count,sha,_ in inputs.values():
        fixed(Path(p),count,sha,Path(p) in (DOTNET,Path('/opt/homebrew/bin/python3')))
    return {'equal':True,'aggregateRootCount':len(contract['aggregateRoots']),
            'explicitLeafCount':len(contract['explicitLeaves']),'inputCount':len(inputs)}

def graph_package_keys(graph):
    keys=set()
    nodes=jload(T09/'state/graphs.json')[graph]['nodes']
    for project in nodes:
        lock=jload(T09/'stage-material/identity'/f'{graph}-locks'/Path(project).parent/'packages.lock.json')
        for deps in lock['dependencies'].values():
            for name,value in deps.items():
                if value['type'].lower()!='project':
                    keys.add(name.lower()+'.'+value['resolved'])
    return keys

def planned_copies(inputs,pkgs,graphs,licenses):
    copies={}
    def add(destination,source):
        source=Path(source);destination=Path(destination)
        require(str(source) in inputs or source==DRIVER,'copy input not frozen: '+str(source))
        require(destination not in copies or copies[destination]==source,'copy destination collision')
        copies[destination]=source
    def material(rel,source): add(STAGE/safe_relative(rel),source)
    runner=graph_package_keys('runner')
    for row in rows(T09/'comparison/patched-source-base.tsv'):
        rel=row[next(iter(row))]
        for phase in ('generation','verification'):
            add(phase_layout(phase)['source']/rel,SOURCE/rel)
        material('source-patched/'+rel,SOURCE/rel)
        material('source-official/'+rel,temp(1)/'source-official'/rel)
    for p in pkgs:
        source=Path(p['path']);name=source.name
        add(T/'local-feed'/name,source);material('packages/'+name,source);material('feed-tests/'+name,source)
        if p['key'] in runner: material('feed-runner/'+name,source)
    for source in [Path(p) for p in inputs]:
        if source.is_relative_to(T13/'acquisition'):
            rel=source.relative_to(T13).as_posix();add(T/rel,source)
            material('licenses/M13/'+source.relative_to(T13/'acquisition').as_posix(),source)
        if source.is_relative_to(temp(1)/'acquisition') and source.suffix!='.nupkg':
            material('licenses/legacy-acquisition/'+source.relative_to(temp(1)/'acquisition').as_posix(),source)
        for root,label in [(G,'G15'),(H16,'H16')]+([(H17,'H17')] if H17_AUTH else [])+([(H18,'H18')] if H18_AUTH else []):
            if source.is_relative_to(root):
                rel=source.relative_to(root).as_posix()
                add(T/'acquisition'/label/rel,source);material('licenses/'+label+'/'+rel,source)
        if source.is_relative_to(T09/'stage-material'):
            rel=source.relative_to(T09/'stage-material').as_posix()
            if 'runner' in rel or rel=='identity/canonicalizer-fixtures.json':
                add(T/'imported-runner'/rel,source);material(rel,source)
        elif source.is_relative_to(T09) and ('runner' in source.name or source.name=='owner-preflight.json'):
            rel=source.relative_to(T09).as_posix()
            add(T/'imported-runner'/rel,source);material('verification/runner-provenance/'+rel,source)
    for leaf in ('m16_driver.py','m16-inputs.tsv','m16-write-set.tsv'):
        source=E/'source'/leaf
        copies[T/'preflight'/leaf]=source
    material('harness/m16_driver.py',DRIVER)
    material('patch/patch.diff',temp(1)/'identity/patch.diff')
    material('licenses/Luban/LICENSE',temp(1)/'source-official/LICENSE')
    for name in ('commands.json','inputs.json'):
        # The frozen old root manifest, not current observations, supplies identity.
        if str(FIX02/name) in inputs: material('harness/loc-fix02-'+name,FIX02/name)
    for source in [Path(p) for p in inputs]:
        if source.is_relative_to(P005/'scratch/positive'):
            material('harness/fixtures/preflight-005/'+source.relative_to(P005/'scratch/positive').as_posix(),source)
        if source.is_relative_to(P005/'scratch'):
            material('harness/preflight-005/'+source.relative_to(P005).as_posix(),source)
    material('harness/m09-canonicalizer-source.py',SUPPORT)
    return copies

def build_write_set(copies,pkgs,graphs):
    rules={}
    def add(path,role):
        for root_id,root in ROOTS.items():
            if path.is_relative_to(root):
                rel=safe_relative(path.relative_to(root).as_posix());rules[(root_id,rel,'exact')]=role;return
        raise GateError('planned write outside roots: '+str(path))
    def material(rel):
        add(STAGE/rel,'material-generated');add(M/rel,'sealed-material-copy')
    for target in copies:
        add(target,'frozen-input-copy')
        if target.is_relative_to(STAGE): add(M/target.relative_to(STAGE),'sealed-material-copy')
    for rel in PREP: add(E/rel,'prepare-only')
    for rel in ('authority/activation.json','authority/preflight.json','authority/signature-projections.json',
        'process/completed-commands.tsv','process/not-run-commands.tsv','acquisition-files.tsv','feed-files.tsv',
        'material-files.tsv','temp-files.tsv','evidence-files.tsv','network-ledgers.json','old-root-equality.json','result.json'):
        add(E/rel,'execution-evidence')
    for phase in ('generation','verification'):
        layout=phase_layout(phase)
        add(layout['config'],'local-config')
        for suffix in ('json','stdout.txt','stderr.txt'):add(E/f'process/tests-{phase}.{suffix}','process-evidence')
        for project in graphs['tests']['nodes']:
            parent=Path(project).parent;name=Path(project).name
            for leaf in ('packages.lock.json','obj/project.assets.json','obj/project.nuget.cache',
                         f'obj/{name}.nuget.dgspec.json',f'obj/{name}.nuget.g.props',f'obj/{name}.nuget.g.targets'):
                add(layout['source']/parent/leaf,'restore-output')
        for pkg in pkgs:
            prefix=layout['packages']/pkg['id'].lower()/pkg['version'];name=Path(pkg['path']).name
            for entry in pkg['entries']+[name,name+'.sha512','.nupkg.metadata',pkg['id'].lower()+'.nuspec']:
                add(prefix/entry,'restore-output')
        for leaf in ('8.0.425_MachineId.dotnetUserLevelCache','8.0.425.dotnetFirstUseSentinel','MachineId.v1.dotnetUserLevelCache',
                     '8.0.425.toolpath.sentinel','8.0.425.aspNetCertificateSentinel','8.0.425_IsDockerContainer.dotnetUserLevelCache'):
            add(layout['dotnet_home']/'.dotnet'/leaf,'restore-output')
        add(layout['dotnet_home']/'.nuget/NuGet/NuGet.Config','restore-output')
        for pattern,maxfiles,maxbytes in sdk_patterns(phase):
            rules[('T16',pattern,'regex-fullmatch')]=f'restore-output;maxFiles={maxfiles};maxBytes={maxbytes}'
    for name in SCHEMAS: material('identity/'+name)
    for name in ('commands.json','process-budget.json','temp-budget.json','sbom.json','canonicalizer-runtime.json'):
        material('identity/'+name)
    for name in ('upstream-source-files.tsv','material-contract.json','network-ledgers.json'):
        material('identity/'+name)
    for rel in fixture_material():material(rel)
    for rel in yaml_source_files():material('source-yamldotnet/'+rel)
    material('verification/tests-comparison.tsv')
    for project in graphs['tests']['nodes']:
        parent=Path(project).parent.as_posix()
        for prefix,leaf in [('identity/tests-locks','packages.lock.json'),('identity/tests-assets','project.assets.json'),
            ('identity/tests-assets-canonical','project.assets.canonical.json'),('verification/tests-assets','project.assets.json'),
            ('verification/tests-assets-canonical','project.assets.canonical.json')]:material(prefix+'/'+parent+'/'+leaf)
    for p in pkgs:
        material('identity/package-catalog/'+p['key']+'.json')
        if '.signature.p7s' in p['entries']:material('signatures/'+p['key']+'.p7s')
    for key in ('system.reflection.metadata.1.6.0','system.collections.immutable.1.5.0','xunit.abstractions.2.0.3'):
        for suffix in ('json','stdout.txt','stderr.txt'):material('verification/package-signatures/'+key+'.'+suffix)
    return [[*key,role] for key,role in sorted(rules.items())]

class ScopedWriter:
    def __init__(self,write_rows,memory=False):
        self.rules=write_rows;self.memory=memory;self.written={}
        self.exact={(r[0],r[1]):r for r in write_rows if r[2]=='exact'}
        self.patterns=[(r,re.compile(r[1])) for r in write_rows if r[2]=='regex-fullmatch']
        require(len(self.exact)+len(self.patterns)==len(write_rows),'duplicate writer rules')
    def authorize(self,path):
        path=Path(path)
        matches=[]
        for root_id,root in ROOTS.items():
            if path.is_relative_to(root):
                rel=path.relative_to(root).as_posix();safe_relative(rel)
                entry=self.exact.get((root_id,rel));matches=([entry] if entry else [])
                matches.extend(r for r,pattern in self.patterns if r[0]==root_id and pattern.fullmatch(rel))
        require(len(matches)==1,'BLOCKED_WRITE_SET: '+str(path));return matches[0]
    def write(self,path,data):
        path=Path(path);self.authorize(path);require(isinstance(data,bytes),'writer bytes required')
        require(path not in self.written,'duplicate writer output: '+str(path))
        if self.memory:
            self.written[path]=data;return
        require(not path.exists(),'create-once path exists: '+str(path))
        for parent in path.parents:
            require(not parent.is_symlink(),'symlink output ancestor')
        path.parent.mkdir(parents=True,exist_ok=True)
        with path.open('xb') as f:f.write(data)
        self.written[path]=[len(data),digest(data)]
    def copy(self,dest,source): self.write(dest,raw(source))
    def json(self,path,obj):self.write(path,json_bytes(obj))
    def tsv(self,path,header,body):self.write(path,tsv_bytes(header,body))

def budget_gate(writer,started):
    require(time.monotonic()-started<=1800,'BLOCKED_WALL_BUDGET')
    limits={T:768*1024**2,E:16*1024**2,M:512*1024**2}
    for root,maximum in limits.items():
        if root.exists():
            found=files(root);require(sum(p.stat().st_size for p in found)<=maximum,'BLOCKED_STORAGE_BUDGET')
            if root==M: require(len(found)<=25000,'BLOCKED_MATERIAL_FILE_BUDGET')
            for p in found: writer.authorize(p)
    for phase in ('generation','verification'):
        for pattern,maxfiles,maxbytes in sdk_patterns(phase):
            parent=T/pattern.rsplit('/',1)[0]
            if parent.exists():
                matches=[p for p in files(parent) if re.fullmatch(pattern,p.relative_to(T).as_posix())]
                require(len(matches)<=maxfiles and all(p.stat().st_size<=maxbytes for p in matches),'BLOCKED_SDK_FILE_BUDGET')
    for location in (Path('/private/tmp'),M.parent):
        usage=os.statvfs(location)
        require(usage.f_bavail*usage.f_frsize>=2*1024**3,'BLOCKED_FREE_SPACE')

def canonical_phase(path,phase):
    # Execute the sealed M09 function bodies with only the approved root adapter.
    text=raw(SUPPORT).decode();tree=ast.parse(text)
    names={'strict_json','path_token','transform','scan_residual','roots_for','canonicalize'}
    nodes=[x for x in tree.body if isinstance(x,ast.FunctionDef) and x.name in names]
    context={'json':json,'copy':copy,'Path':Path,'fail':fail,
             'phase_layout':lambda graph,mode:phase_layout(mode)}
    exec(compile(ast.Module(body=nodes,type_ignores=[]),str(SUPPORT),'exec'),context)
    return context['canonicalize'](path,'tests',phase)

def collect_phase(phase,graphs,pkgs):
    layout=phase_layout(phase);result={};allowed={p['id'].lower()+'/'+p['version']:p for p in pkgs}
    expected_projects=set(graphs['tests']['nodes']);seen_refs=set()
    for project in graphs['tests']['nodes']:
        parent=Path(project).parent;lock_path=layout['source']/parent/'packages.lock.json'
        assets_path=layout['source']/parent/'obj/project.assets.json'
        lock_bytes=raw(lock_path);assets_bytes=raw(assets_path);lock=json_data(lock_bytes);assets=json_data(assets_bytes)
        require('net8.0' in lock['dependencies'],'lock target framework drift')
        for name,item in lock['dependencies']['net8.0'].items():
            if item['type'].lower()=='project':continue
            key=name.lower()+'/'+item['resolved'];require(key in allowed and 'yamldotnet.netcore' not in key,'unlisted lock package')
            pkg=allowed[key];installed=layout['packages']/name.lower()/item['resolved']/Path(pkg['path']).name
            fixed(installed,pkg['identity']['bytes'],pkg['identity']['sha256'])
            # NuGet contentHash excludes signature-dependent ZIP details and is
            # NOT the catalog's SHA512 of the signed archive. Keep both domains.
            content_hash=raw(Path(str(installed)+'.sha512')).decode('ascii').strip()
            require(item['contentHash']==content_hash,'lock/installed NuGet content hash differs')
        canonical,present,marker=canonical_phase(assets_path,phase)
        for entry in assets.get('logs',[]):
            require(entry.get('level','').lower() not in ('error','warning'),'unapproved restore diagnostic')
        refs=assets['project']['restore']['frameworks']['net8.0']['projectReferences']
        children=set()
        for ref in refs:
            relative=Path(ref).relative_to(layout['source']).as_posix();require(relative in expected_projects,'assets project outside graph')
            children.add(relative);seen_refs.add((project,relative))
        result[project]={'lock':lock_bytes,'assets':assets_bytes,'canonical':canonical,'parsedLock':lock,
                         'parsedAssets':assets,'markerPresent':present,'marker':marker,'projectReferences':len(children)}
    expected=rows(T09/'stage-material/identity/tests-project-edges.tsv')
    require(seen_refs=={(r['parentProjectRelativePath'],r['childProjectRelativePath']) for r in expected},'13/29 asset project graph differs')
    return result

def compare_phases(generation,verification):
    require(set(generation)==set(verification) and len(generation)==13,'13-lock set required')
    compared=[]
    for project in sorted(generation):
        g=generation[project];v=verification[project]
        require(g['lock']==v['lock'] and g['canonical']==v['canonical'],'BLOCKED_LOCK_OR_CANONICAL_DRIFT: '+project)
        compared.append([project,digest(g['lock']),digest(v['lock']),digest(g['canonical']),digest(v['canonical']),'PASS'])
    return compared

def catalog_model(inputs,pkgs):
    bykey={p['key']:p for p in pkgs};found={};gaps=[]
    def accept(key,catalog,url,authority,package_url):
        require(key in bykey,'catalog names unlisted package');pkg=bykey[key]
        require(catalog['id'].lower()==pkg['id'].lower() and catalog['version']==pkg['version'] and
                catalog['packageHashAlgorithm'].upper()=='SHA512' and catalog['packageHash']==pkg['sha512Base64'],
                'catalog package SHA512 mismatch')
        if 'packageSize' in catalog:require(catalog['packageSize']==pkg['identity']['bytes'],'catalog package size differs')
        entry={'catalogUrl':url,'catalogSha512':catalog['packageHash'],'immutablePackageUrl':package_url,
               'authority':authority,'kind':'official-catalog','packageSha256':pkg['identity']['sha256']}
        if key in found:require(found[key]['catalogSha512']==entry['catalogSha512'],'conflicting catalog')
        found[key]=entry
    for path in sorted(inputs):
        if not (path.endswith('.catalog.json') or path.endswith('/catalog.json')):continue
        value=jload(Path(path));key=value.get('id','').lower()+'.'+value.get('version','')
        if key not in bykey:continue
        url=value['@id'];package_url=value.get('packageContent')
        # The URL locator alone is never substituted for catalog hash authority.
        if not package_url:
            registration=Path(path.replace('.catalog.json','.registration.json')) if path.endswith('.catalog.json') else Path(path).with_name('registration.json')
            require(str(registration) in inputs,'catalog registration not bound')
            leaf=jload(registration);entry=leaf['catalogEntry'];entry=entry if isinstance(entry,str) else entry['@id']
            require(entry==url,'catalog link differs');package_url=leaf['packageContent']
        accept(key,value,url,path,package_url)
    for root,auth in [(H17,H17_AUTH)]:
        if not auth:continue
        document=jload(root/'bindings.json')
        bindings=document.get('requiredBindings',document.get('bindings',[]))
        observations={x['id']:x for x in jload(root/'observations.json')['observations']}
        for row in bindings:
            if row.get('status')!='OFFICIAL_CATALOG_BOUND':continue
            cat=row['catalog'];reg=row['registration'];key=row['key']
            cobs=observations[cat['observationId']];robs=observations[reg['observationId']]
            craw=observed_json(cobs,cat['url']);rraw=observed_json(robs,reg['url'])
            require(craw['@id']==cat['url'] and rraw['catalogEntry']==cat['url'] and
                    rraw['packageContent']==reg['packageContent'],'H17 raw response URL join differs')
            require(reg['catalogEntry']==cat['url'],'overlay registration link differs')
            require(row['archive']['sha256']==bykey[key]['identity']['sha256'],'overlay archive differs')
            accept(key,cat,cat['url'],str(root/'bindings.json')+'#'+key,reg['packageContent'])
    if H18_AUTH:
        observations={x['observationId']:x for x in jload(H18/'observations.json')['observations']}
        for row in jload(H18/'bindings.json')['boundPackages']:
            require(row['status']=='OFFICIAL_REGISTRATION_CATALOG_BOUND','H18 binding status')
            cat=observed_json(observations[row['catalogObservationId']],row['catalogUrl'])
            reg=observed_json(observations[row['registrationObservationId']],row['registrationUrl'])
            require(reg['catalogEntry']==row['catalogUrl'] and cat['@id']==row['catalogUrl'] and
                    reg['packageContent']==row['packageContent'],'H18 response URL chain differs')
            require(row['packageIdentity']['sha256']==bykey[row['key']]['identity']['sha256'],'H18 archive differs')
            accept(row['key'],cat,row['catalogUrl'],str(H18/'bindings.json')+'#'+row['key'],row['packageContent'])
    for key in bykey:
        if key not in found:gaps.append({'kind':'PACKAGE_CATALOG_NOT_BOUND','key':key})
    return found,gaps

def observed_json(observation,url):
    import zlib
    require(observation.get('status')==200 and observation.get('bodyComplete') is True,'incomplete/unknown observation selected')
    require(observation.get('requestUrl',observation.get('url'))==url,'observation request URL differs')
    if 'bodyZlibLevel9Base64' in observation:
        data=zlib.decompress(base64.b64decode(observation['bodyZlibLevel9Base64'],validate=True))
    else:data=base64.b64decode(observation['bodyBase64'],validate=True)
    require(len(data)==observation['bodyBytes'] and digest(data)==observation['bodySha256'],'observed body identity differs')
    return json_data(data)

def future_command_contract(inputs):
    commands=[]
    def command(identifier,stage,argv,cwd,reads,creates,timeout,child_cap,expected=0,depends=()):
        envroot=(BROOT if stage=='B' else LROOT)/'environment'/identifier
        env={'inheritParent':True,'protectedKeysUnmodified':list(PROTECTED),'updates':{
             'DOTNET_CLI_HOME':str(envroot/'dotnet-home'),'NUGET_PACKAGES':str(envroot/'packages'),
             'NUGET_HTTP_CACHE_PATH':str(envroot/'http'),'NUGET_PLUGINS_CACHE_PATH':str(envroot/'plugins'),
             'NUGET_SCRATCH':str(envroot/'scratch'),'TMPDIR':str(envroot/'tmp'),
             'DOTNET_GENERATE_ASPNET_CERTIFICATE':'false','DOTNET_ADD_GLOBAL_TOOLS_TO_PATH':'false'}}
        commands.append({'id':identifier,'stage':stage,'purpose':identifier,'executableAbsolutePath':str(DOTNET),
             'executableSha256':DOTNET_SHA,'argv':argv,'cwd':str(cwd),'env':env,'networkPolicy':'OS-denied; local feed only',
             'expectedExit':expected,'timeoutMs':timeout*1000,'killGraceMs':5000,'stdoutMaxBytes':4194304,'stderrMaxBytes':4194304,
             'reads':[str(x) for x in reads],'creates':[str(x) for x in creates],
             'stdoutPath':str((BROOT if stage=='B' else LROOT)/'logs'/f'{identifier}.stdout.txt'),
             'stderrPath':str((BROOT if stage=='B' else LROOT)/'logs'/f'{identifier}.stderr.txt'),
             'maxTopLevelStarts':1,'maxChildStarts':child_cap,'maxConcurrentTreeCount':1,'dependsOn':list(depends)})
    for name,graph in [('pass-1','runner'),('pass-2','runner'),('official-tests','tests')]:
        root=BROOT/name;project=root/'source'/('src/Luban/Luban.csproj' if graph=='runner' else 'src/Luban.Tests/Luban.Tests.csproj')
        restore=[str(DOTNET),'msbuild',str(project),'-target:Restore','-maxCpuCount:1','-nodeReuse:false','-noAutoResponse','-verbosity:minimal',
                 '-property:RestoreConfigFile='+str(root/'NuGet.Config'),'-property:RestorePackagesPath='+str(root/'nuget'),
                 '-property:RestoreSources='+str(M/f'feed-{graph}'),'-property:RestoreFallbackFolders=',
                 '-property:RestoreDisableParallel=true','-property:RestoreNoHttpCache=true','-property:RestoreIgnoreFailedSources=false',
                 '-property:RestorePackagesWithLockFile=true','-property:RestoreLockedMode=true','-property:NuGetAudit=false']
        command('B-'+name+'-restore','B',restore,root/'source',[M,project],[root/'nuget',root/'source'],180,0)
        commands[-1]['env']['updates']['NUGET_PACKAGES']=str(root/'nuget')
        verb='publish' if graph=='runner' else 'build'
        argv=[str(DOTNET),verb,str(project),'-c','Release','--no-restore','--disable-build-servers',
              '-p:ContinuousIntegrationBuild=true','-p:Deterministic=true','-p:UseSharedCompilation=false',
              '-maxCpuCount:1','-nodeReuse:false','-p:PathMap='+str(root/'source')+'=/_/luban-v5.1.0']
        if verb=='publish':argv+=['-o',str(root/'runner')]
        command('B-'+name+'-'+verb,'B',argv,root/'source',[M,project],[root],600,128,depends=['B-'+name+'-restore'])
        commands[-1]['env']['updates']['NUGET_PACKAGES']=str(root/'nuget')
        if graph=='tests':
            command('B-official-tests-test','B',[str(DOTNET),'test',str(project),'-c','Release','--no-restore','--no-build',
                    '--disable-build-servers','--logger','trx;LogFileName=official-tests.trx','--results-directory',str(root/'results')],
                    root/'source',[root/'source'],[root/'results',root/'source'],600,128,depends=['B-official-tests-build'])
            commands[-1]['env']['updates']['NUGET_PACKAGES']=str(root/'nuget')
            commands[-1]['env']['updates'].update({'MSBUILDDISABLENODEREUSE':'1','DOTNET_CLI_USE_MSBUILD_SERVER':'0','UseSharedCompilation':'false'})
            commands[-1]['env']['pathPolicy']={'prependExactDirectory':str(DOTNET.parent),'retainParentSuffix':True,
                'resolvedDotnetMustEqual':str(DOTNET),'purpose':'unchanged CsharpCompileHarness FileName=dotnet'}
    # Full direct DUT argv for the finite old/candidate behavior matrix.
    for scenario,configuration,target,expected in [
        ('yaml',M/'source-patched/tests/fixtures/loaders/yml/luban.conf','all',0),
        ('unity-asset',M/'harness/fixtures/unity-asset/luban.conf','all',0),
        ('malformed-yaml',M/'harness/fixtures/malformed-yaml/luban.conf','all','nonzero'),
        ('fightmatch',REPO/'Config/FightMatch/Luban/luban.conf','client',0)]:
        for side,runner in [('oracle',OLD_TOOL/'runner'),('candidate-1',BROOT/'pass-1/runner'),('candidate-2',BROOT/'pass-2/runner')]:
            dest=BROOT/'controls'/scenario/side
            command('B-'+scenario+'-'+side,'B',[str(DOTNET),str(runner/'Luban.dll'),'--conf',str(configuration),
                '-t',target,'-d','yaml' if scenario=='yaml' else 'json','--strict','--errorFormat','json','-l',str(runner/'nlog.xml'),'-x','outputDataDir='+str(dest)],
                REPO,[configuration,runner],[dest],300,0,expected,depends=['B-pass-1-publish','B-pass-2-publish'])
    # The accepted 005 direct .NET DAG is replayed with only root/runner
    # adapters. Shell wrappers and their unscoped env are deliberately not run.
    for side,runner in [('oracle',OLD_TOOL/'runner'),('candidate-1',BROOT/'pass-1/runner'),('candidate-2',BROOT/'pass-2/runner')]:
        root=BROOT/'controls/005'/side
        for oldcommand in jload(P005/'commands.json')['commands']:
            original=oldcommand['argv']
            if str(DOTNET) not in original:continue
            argv=original[original.index(str(DOTNET)):]
            argv=[a.replace(str(P005),str(root)).replace(str(OLD_TOOL/'runner'),str(runner)) for a in argv]
            identifier='B-005-'+side+'-'+oldcommand['name']
            command(identifier,'B',argv,REPO,[M/'harness/preflight-005',runner],[root],300,0,
                    oldcommand['exitCode'],depends=['B-pass-1-publish','B-pass-2-publish'])
    old=jload(FIX02/'commands.json')['commands']
    compiler=next(x for x in old if x['name']=='compiler-compile')
    old_root=str(FIX02);new_root=str(LROOT)
    compile_args=[]
    for arg in compiler['argumentList']:
        a=arg.replace(old_root,new_root).replace(str(FIX02.relative_to(REPO)),new_root)
        if a=='Tools/FightMatch.Localization.Compiler/Program.cs':a=str(LROOT/'source/Program.cs')
        compile_args.append(a)
    command('L-compiler-compile','L',[str(DOTNET),*compile_args],REPO,[LROOT/'source/Program.cs',DOTNET.parent],
            [LROOT/'compiler'],300,0)
    for row in old:
        if row['name'] not in ('compiler-self-tests','negative-duplicate','negative-parameters','negative-severity','compile-pass-1','compile-pass-2'):continue
        argv=[str(DOTNET)]+[a.replace(old_root,new_root).replace(str(FIX02.relative_to(REPO)),new_root) for a in row['argumentList']]
        command('L-'+row['name'],'L',argv,REPO,[LROOT/'compiler'],[LROOT],300,0,depends=['L-compiler-compile'])
        if row['name'].startswith('negative-'):commands[-1]['expectedExit']=row['exitCode']
    for number in (1,2):
        runner=BROOT/'pass-1/runner';dest=LROOT/f'pass-{number}/raw'
        command(f'L-pass-{number}-luban','L',[str(DOTNET),str(runner/'Luban.dll'),'--conf',str(REPO/'Config/FightMatch/Luban/luban.conf'),
            '-t','client','-d','json','--strict','--errorFormat','json','-l',str(runner/'nlog.xml'),'-x','outputDataDir='+str(dest)],
            REPO,[runner,REPO/'Config/FightMatch'],[dest],300,0)
    for scenario,expected,datatype in [('yaml',0,'yaml'),('malformed-yaml','nonzero','json')]:
        configuration=M/('source-patched/tests/fixtures/loaders/yml/luban.conf' if scenario=='yaml' else 'harness/fixtures/malformed-yaml/luban.conf')
        runner=BROOT/'pass-1/runner';dest=LROOT/'controls'/scenario
        command('L-'+scenario,'L',[str(DOTNET),str(runner/'Luban.dll'),'--conf',str(configuration),'-t','all','-d',datatype,
            '--strict','--errorFormat','json','-l',str(runner/'nlog.xml'),'-x','outputDataDir='+str(dest)],REPO,[runner,configuration],
            [dest],300,0,expected)
    # Actual Python gate argv; the B receipt supplies future build hashes by
    # design, never invented here. These commands perform no runner launches.
    for case in ('accepted-b','tampered-yaml','missing-yaml-license','changed-fallback-license'):
        runner=BROOT/'pass-1/runner' if case=='accepted-b' else LROOT/'controls'/case/'runner'
        identifier='L-'+case
        commands.append({'id':identifier,'stage':'L','purpose':'bounded B receipt/runner/license identity gate',
            'executableAbsolutePath':'/opt/homebrew/bin/python3','executableSha256':PYTHON_SHA,
            'argv':['/opt/homebrew/bin/python3','-B',str(M/'harness/m16_driver.py'),'check-b',
                    '--receipt',str(BROOT/'identity/accepted-b.json'),'--runner',str(runner),
                    '--expect','pass' if case=='accepted-b' else 'reject'],
            'cwd':str(REPO),'env':{'inheritParent':True,'updates':{},'protectedKeysUnmodified':list(PROTECTED)},
            'networkPolicy':'none','expectedExit':0 if case=='accepted-b' else 20,'timeoutMs':60000,'killGraceMs':5000,
            'stdoutMaxBytes':4194304,'stderrMaxBytes':4194304,'reads':[str(BROOT/'identity/accepted-b.json'),str(runner)],
            'creates':[],'maxTopLevelStarts':1,'maxChildStarts':0,'maxConcurrentTreeCount':1,
            'stdoutPath':str(LROOT/'logs'/f'{identifier}.stdout.txt'),'stderrPath':str(LROOT/'logs'/f'{identifier}.stderr.txt'),
            'dependsOn':[]})
    for stage,root in [('B',BROOT),('L',LROOT)]:
        controls=[c['id'] for c in commands if c['stage']==stage]
        commands.append({'id':stage+'-assert-controls','stage':stage,'purpose':'check actual DUT receipts; no DUT starts',
            'executableAbsolutePath':'/opt/homebrew/bin/python3','executableSha256':PYTHON_SHA,
            'argv':['/opt/homebrew/bin/python3','-B',str(M/'harness/m16_driver.py'),'assert-controls','--stage',stage],
            'cwd':str(REPO),'env':{'inheritParent':True,'updates':{},'protectedKeysUnmodified':list(PROTECTED)},
            'networkPolicy':'none','expectedExit':0,'timeoutMs':60000,'killGraceMs':5000,'stdoutMaxBytes':4194304,
            'stderrMaxBytes':4194304,'reads':[str(root/'logs')],'creates':[],'maxTopLevelStarts':1,'maxChildStarts':0,
            'maxConcurrentTreeCount':1,'stdoutPath':str(root/'logs'/f'{stage}-assert-controls.stdout.txt'),
            'stderrPath':str(root/'logs'/f'{stage}-assert-controls.stderr.txt'),'dependsOn':controls})
    for c in commands:
        c['role']='harness' if c['id'].endswith('-assert-controls') else 'DUT'
        c['receiptPath']=str((BROOT if c['stage']=='B' else LROOT)/'logs'/(c['id']+'.json'))
    return {'schemaVersion':1,'commands':commands,'executionAuthorized':False,
        'setup':{'B':'Fresh source and per-project locks copied from M for each of three roots; local-only NuGet.Config. Copy harness/preflight-005 scratch into each of three controls/005 roots before its fixed DAG.',
            'L':'Copy compiler runtimeconfig and three negative inputs from fixed FIX02 receipts into L. Bind accepted B receipt hash in separate L authority before identity adaptation.',
            'BOfficialHarness':'Unchanged CsharpCompileHarness runs in testhost; its GeneratedConsumer builds use the isolated TMPDIR and local config at that root; no network or global fallback.'},
        'officialFixtureMatrix':{'authority':'source-patched/src/Luban.Tests; all fixtures retained','topLevelTests':1,'noFiltering':True},
        'futureBReceiptContract':{'path':str(BROOT/'identity/accepted-b.json'),'requiredStatus':'B-ACCEPTED',
            'requiredFields':['materialManifestSha256','runnerFiles','licenseFiles','reviewedHead','independentAcceptance'],
            'filesSchema':['relativePath','bytes','sha256'],'noFutureBuildBytesInvented':True},
        'LSourceContract':'Future L owns private identity adaptation of source/Program.cs from accepted B receipt; no source modification authorized by M.',
        'controlAssertions':{'malformed-yaml':'both DUT nonzero; diagnostics nonempty; no output files',
            'otherBControls':'oracle/candidate semantic, raw-byte and canonical publication comparisons all required',
            'LNegatives':{'duplicate':'FMLOC002','parameters':'FMLOC007','severity':'FMLOC004'},
            'selfTests':12,'LCanonicalCleanPasses':2,'negativeDutVersusHarnessExitsKeptSeparate':True}}


def yaml_source_files():
    import tarfile
    path=Path('/private/tmp/yamldotnet-v16.3.0.tar.gz')
    fixed(path,397462,'4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce')
    result={}
    with tarfile.open(path,'r:gz') as archive:
        require(archive.pax_headers.get('comment')=='ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73','Yaml archive commit differs')
        for entry in archive.getmembers():
            require(entry.isdir() or entry.isfile(),'unsupported Yaml archive entry')
            if not entry.isfile():continue
            rel=safe_relative(entry.name.split('/',1)[1]);require(rel not in result,'duplicate archive leaf')
            result[rel]=archive.extractfile(entry).read()
    return result


def fixture_material():
    fixture=SOURCE/'tests/fixtures/loaders/yml'
    conf=raw(fixture/'luban.conf');schema=raw(fixture/'Defines/schema.xml')
    data=raw(fixture/'Data/item.yml').decode('utf-8-sig')
    return {
        'harness/fixtures/unity-asset/luban.conf':conf,
        'harness/fixtures/unity-asset/Defines/schema.xml':schema.replace(b'item.yml',b'item.asset'),
        'harness/fixtures/unity-asset/Data/item.asset':('MonoBehaviour:\n'+''.join('  '+line+'\n' for line in data.splitlines())).encode(),
        'harness/fixtures/malformed-yaml/luban.conf':conf,
        'harness/fixtures/malformed-yaml/Defines/schema.xml':schema,
        'harness/fixtures/malformed-yaml/Data/item.yml':b'[unterminated\n',
    }


def future_process_budget(inputs,contract):
    sdk=DOTNET.parent/'sdk/8.0.425'
    images=[sdk/'MSBuild.dll',sdk/'Roslyn/bincore/csc.dll',sdk/'Roslyn/bincore/VBCSCompiler.dll',
            sdk/'vstest.console.dll',sdk/'testhost.dll']
    identities={str(p):{'executableAbsolutePath':str(DOTNET),'executableSha256':DOTNET_SHA,
                        'managedImageAbsolutePath':str(p),'managedImageSha256':inputs[str(p)][2]} for p in images}
    command_budgets=[]
    for c in contract['commands']:
        children=[]
        def child(image,max_starts,purpose):
            children.append({**identities[str(image)],'maxStarts':max_starts,'maxConcurrent':1,
                'parentExecutable':str(DOTNET),'purpose':purpose})
        if c['id'].endswith('-publish') or c['id']=='B-official-tests-build':
            child(sdk/'MSBuild.dll',1,'bounded MSBuild CLI child if emitted')
            child(sdk/'Roslyn/bincore/csc.dll',23 if c['id'].endswith('-publish') else 13,'one compile per project; no shared compiler')
        elif c['id']=='B-official-tests-test':
            child(sdk/'vstest.console.dll',1,'official test runner')
            child(sdk/'testhost.dll',1,'fixed SDK testhost if selected')
            child(sdk/'MSBuild.dll',32,'GeneratedConsumer implicit builds; cap includes all fixture cases')
            child(sdk/'Roslyn/bincore/csc.dll',32,'GeneratedConsumer compilers')
            child(sdk/'Roslyn/bincore/VBCSCompiler.dll',1,'bounded unexpected server, must quiesce before next command')
            children.append({'executableAbsolutePath':str(DOTNET),'executableSha256':DOTNET_SHA,
                'argvContract':['build','<TMPDIR>/luban-csharp-harness/<32-lower-hex>/GeneratedConsumer.csproj','-c','Release','-v','q'],
                'maxStarts':32,'maxConcurrent':1,'parentExecutable':str(DOTNET),
                'purpose':'exact unchanged official CsharpCompileHarness RunDotnet call; implicit restore is counted',
                'implicitRestoreLocalFeedOnly':True})
            pkgpath=T09/'closure/tests-generation/feed/microsoft.testplatform.testhost.17.11.1.nupkg'
            with zipfile.ZipFile(pkgpath) as archive:
                entries=[n for n in archive.namelist() if n.endswith('/testhost.dll')]
                require(set(entries)=={'lib/netcoreapp3.1/testhost.dll','build/netcoreapp3.1/x64/testhost.dll'} and
                        len({digest(archive.read(n)) for n in entries})==1,'testhost variants differ')
                children.append({'executableAbsolutePath':str(DOTNET),'executableSha256':DOTNET_SHA,
                    'managedImageSource':'packages/'+pkgpath.name+'!'+entries[0],
                    'managedImageSha256':digest(archive.read(entries[0])),'maxStarts':1,'maxConcurrent':1,
                    'parentExecutable':str(DOTNET),'purpose':'package testhost; shared max one with SDK variant'})
        require(sum(x['maxStarts'] for x in children)<=c['maxChildStarts'],'child budget exceeds command limit')
        command_budgets.append({'commandId':c['id'],'maxStarts':1,'maxConcurrent':1,'children':children,
                               'timeoutMs':c['timeoutMs'],'stdoutMaxBytes':4194304,'stderrMaxBytes':4194304})
    return {'schemaVersion':1,'maxTopLevelCommandCount':len(contract['commands'])+2,
        'maxTopLevelProcessTreeCount':1,'maxConcurrentTopLevelTrees':1,'maxTotalProcessStarts':20000,
        'M':restore_budget_contract(),'B':{'wallClockMs':7200000,'topLevelCommands':sum(c['stage']=='B' for c in contract['commands'])},
        'L':{'wallClockMs':1800000,'topLevelCommands':sum(c['stage']=='L' for c in contract['commands'])},
        'commands':command_budgets,'executables':[{'path':str(DOTNET),'sha256':DOTNET_SHA,'maxStarts':512},
            {'path':'/opt/homebrew/bin/python3','sha256':PYTHON_SHA,'maxStarts':8}],
        'unlistedExecutablePolicy':'stop owned tree; no retrospective budget expansion',
        'officialTestsSourceUnmodified':True,'futureBuiltAssemblies':'in-process or fixed-launcher managed inputs; hash before load/start in B',
        'BAndLRequireSeparateAuthority':True}


def material_table(writer,name,body):
    # Sort complete rows ordinally; never rely on the host's collation/locale.
    header=SCHEMAS[name].split()
    require(all(len(row)==len(header) for row in body),'schema width: '+name)
    writer.tsv(STAGE/'identity'/name,header,sorted(body,key=lambda row:tuple(str(x).encode() for x in row)))


def graph_receipts(graph,tests):
    if graph=='tests':return tests
    result={}
    for row in rows(T09/'stage-material/identity/runner-lock-set.tsv'):
        base=T09/'stage-material';lock=raw(base/row['lockRelativePath']);assets=raw(base/row['rawAssetsRelativePath'])
        canonical=raw(base/row['canonicalAssetsRelativePath'])
        require(digest(lock)==row['lockSha256'] and digest(assets)==row['rawAssetsSha256'] and
                digest(canonical)==row['canonicalAssetsSha256'],'runner receipt identity drift')
        result[row['projectRelativePath']]={'lock':lock,'assets':assets,'canonical':canonical,
                                           'parsedLock':json_data(lock),'parsedAssets':json_data(assets)}
    require(len(result)==23,'runner lock count');return result


def package_tables(pkgs,graphs,tests,licenses,catalogs,signatures):
    """Join exact package entries to every project/target, without dropping edges."""
    bykey={p['key']:p for p in pkgs};package_rows=[];entry_rows=[];edge_rows=[];deployment=set();used=set()
    sig={x['packageKey']:x for x in signatures}
    for graph in ('runner','tests'):
        graph_used=set()
        for project,receipt in sorted(graph_receipts(graph,tests).items()):
            assets=receipt['parsedAssets'];lock=receipt['parsedLock']
            direct=assets['project']['frameworks']['net8.0'].get('dependencies',{})
            for target,items in assets['targets'].items():
                require(target=='net8.0','unexpected restore target')
                for identifier,item in items.items():
                    name,version=identifier.rsplit('/',1)
                    if item['type']=='project':continue
                    require(item['type']=='package','unrecognized target library kind')
                    key=name.lower()+'.'+version;require(key in bykey,'unlisted package target')
                    pkg=bykey[key];used.add(key);graph_used.add(key)
                    dependency=lock['dependencies']['net8.0'][name]
                    attrs=direct.get(name,{})
                    edge_rows.append([graph,project,project,'N/A_PROJECT',name,version,
                        dependency.get('requested','N/A_TRANSITIVE'), 'exact-lock-and-assets',dependency['type'],target,
                        attrs.get('include','all'),attrs.get('suppressParent','none'),json.dumps(dependency,sort_keys=True,separators=(',',':'))])
                    for child,requested in item.get('dependencies',{}).items():
                        matches=[(n,v) for n,v in lock['dependencies']['net8.0'].items() if n.lower()==child.lower()]
                        require(len(matches)==1,'target edge missing/ambiguous in lock')
                        childname,childlock=matches[0]
                        require(childlock['type'].lower()!='project','package edge resolves to project')
                        childkey=childname.lower()+'.'+childlock['resolved'];require(childkey in bykey,'dependency outside fixed feed')
                        edge_rows.append([graph,project,name,version,childname,childlock['resolved'],requested,
                            'exact-lock-and-assets','package-edge',target,'all','none',name+'->'+childname])
                    selected={}
                    for kind,value in item.items():
                        if kind in ('type','dependencies','frameworkReferences','frameworkAssemblies'):continue
                        require(isinstance(value,dict),'unsupported asset selector: '+kind)
                        for entry,metadata in value.items():
                            if entry.endswith('/_._'):continue
                            canonical=PurePosixPath(entry).as_posix()
                            require(canonical in pkg['entries'],'selected ZIP entry not present')
                            selected.setdefault(canonical,[]).append((kind,metadata))
                    with zipfile.ZipFile(pkg['path']) as archive:
                        names={PurePosixPath(n).as_posix():n for n in archive.namelist() if not n.endswith('/')}
                        for entry in sorted(names):
                            b=archive.read(names[entry]);choices=selected.get(entry,[('not-selected',{})])
                            for kind,metadata in choices:
                                output='N/A_NOT_DEPLOYED'
                                if graph=='runner' and kind in ('runtime','native','resource','runtimeTargets'):
                                    output=Path(entry).name
                                    if kind=='resource':output=metadata['locale']+'/'+output
                                    if kind=='runtimeTargets':output=entry
                                elif kind=='contentFiles' and metadata.get('copyToOutput') is True:
                                    output=metadata.get('outputPath',Path(entry).name)
                                if output!='N/A_NOT_DEPLOYED':
                                    safe_relative(output)
                                    for body in licenses[key]['bodies']:
                                        deployment.add(('package',name,version,entry,output,kind,
                                                        body['localRelativePath'],body['sha256']))
                                entry_rows.append([graph,project,name,version,entry,kind,target,len(b),digest(b),
                                                   str(entry in selected).lower(),output])
        for key in sorted(graph_used):
            p=bykey[key];catalog=catalogs[key];signature='';signature_sha=''
            if '.signature.p7s' in p['entries']:
                signature='signatures/'+key+'.p7s'
                with zipfile.ZipFile(p['path']) as archive:signature_sha=digest(archive.read('.signature.p7s'))
            verdict='IMPORTED_NOT_REVERIFIED; see immutable original signature records'
            if key in sig:verdict='QA005_LIMITED_ACCEPT; 0 errors; NU3018x4; NU3028x4; offline/fallback limitations'
            package_rows.append([graph,p['id'],p['version'],'packages/'+Path(p['path']).name,p['identity']['bytes'],
                p['identity']['sha256'],catalog['catalogSha512'],catalog['catalogUrl'],catalog['immutablePackageUrl'],
                'official binary package; source/license relationship separately recorded',signature,signature_sha,verdict])
        if graph=='runner':
            for project in graphs['runner']['nodes']:
                root=ET.fromstring(raw(SOURCE/project));assembly=Path(project).stem
                for node in root.iter():
                    if node.tag.split('}')[-1]=='AssemblyName':assembly=node.text
                deployment.add(('project',assembly,'v5.1.0+two-package-references',project,assembly+'.dll','project-output',
                                'licenses/Luban/LICENSE','a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841'))
                for node in root.iter():
                    if node.tag.split('}')[-1] not in ('Content','None'):continue
                    copy_to=next((c.text for c in node if c.tag.split('}')[-1]=='CopyToOutputDirectory'),None)
                    if copy_to not in ('Always','PreserveNewest'):continue
                    include=node.attrib.get('Include',node.attrib.get('Update','')).replace('\\','/')
                    # Expansion is against the frozen 931 source leaves only.
                    import fnmatch
                    base=PurePosixPath(project).parent
                    for source_row in rows(T09/'comparison/patched-source-base.tsv'):
                        rel=source_row[next(iter(source_row))]
                        relative=os.path.relpath(rel,str(base)).replace('\\','/')
                        if not fnmatch.fnmatchcase(relative,include):continue
                        link=next((c.text for c in node if c.tag.split('}')[-1]=='Link'),None)
                        output=link if link and '%' not in link else relative
                        if output.startswith('../'):output=Path(output).name
                        safe_relative(output)
                        deployment.add(('project',assembly,'v5.1.0',rel,output,'content',
                            'licenses/Luban/LICENSE','a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841'))
    require(used==set(bykey),'31-package closed union differs')
    require(not any('yamldotnet.netcore' in str(r).lower() for r in edge_rows),'old YAML edge')
    return {'package-files.tsv':package_rows,'package-entries.tsv':entry_rows,
            'package-closure.tsv':edge_rows,'expected-deployment.tsv':list(deployment)}


def source_tables():
    sources=[];patches=[];upstream=[];upstream_rows=[]
    blob=lambda b:hashlib.sha1(b'blob '+str(len(b)).encode()+b'\0'+b).hexdigest()
    for row in rows(T09/'comparison/patched-source-base.tsv'):
        rel=row[next(iter(row))];before=raw(temp(1)/'source-official'/rel);after=raw(SOURCE/rel)
        for component,prefix,b,role in [('Luban','source-official',before,'official'),('Luban-patched','source-patched',after,'two-reference patch')]:
            sources.append([component,prefix+'/'+rel,len(b),digest(b),blob(b),role])
        upstream_rows.append([rel,len(before),digest(before),blob(before)])
        if before!=after:
            patches.append([rel,len(before),digest(before),len(after),digest(after),'YamlDotNet.NetCore','1.0.0',
                            'YamlDotNet','16.3.0',digest(raw(temp(1)/'identity/patch.diff'))])
    for rel,b in yaml_source_files().items():sources.append(['YamlDotNet','source-yamldotnet/'+rel,len(b),digest(b),blob(b),'exact archive source'])
    manifest=tsv_bytes(['relativePath','bytes','sha256','gitObjectId'],upstream_rows)
    upstream.append(['Luban','v5.1.0','https://github.com/focus-creative-games/luban',
        'df34215d2e22035073d1af83d44e3ac0744dc3b3','a8f8243ef84b2d11156f869af76061863ffb6d6b',
        'N/A_REUSED_COMMIT_BOUND_LOCAL_SOURCE',0,'N/A_NO_NEW_TRANSPORT',str(M/'source-official'),digest(manifest)])
    # Tar comment is the inherited commit binding, not a new remote verification.
    yrows=[r for r in sources if r[0]=='YamlDotNet']
    upstream.append(['YamlDotNet','v16.3.0','https://github.com/aaubry/YamlDotNet',
        'ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73',yaml_tree_id(),
        'https://codeload.github.com/aaubry/YamlDotNet/tar.gz/refs/tags/v16.3.0',397462,
        '4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce',str(M/'source-yamldotnet'),
        digest(tsv_bytes(SCHEMAS['source-files.tsv'].split(),yrows))])
    return {'source-files.tsv':sources,'patch.tsv':patches,'upstream-source.tsv':upstream},manifest

def yaml_tree_id():
    import tarfile
    tree={}
    with tarfile.open('/private/tmp/yamldotnet-v16.3.0.tar.gz','r:gz') as archive:
        for member in archive.getmembers():
            if not member.isfile():continue
            parts=PurePosixPath(member.name).parts[1:];node=tree
            for part in parts[:-1]:node=node.setdefault(part,{})
            b=archive.extractfile(member).read();node[parts[-1]]=(b'100755' if member.mode&0o111 else b'100644',
                hashlib.sha1(b'blob '+str(len(b)).encode()+b'\0'+b).digest())
    def digest_tree(node):
        payload=b''
        for name,value in sorted(node.items(),key=lambda x:(x[0]+('/' if isinstance(x[1],dict) else '')).encode()):
            mode,sha=(b'40000',digest_tree(value)) if isinstance(value,dict) else value
            payload+=mode+b' '+name.encode()+b'\0'+sha
        return hashlib.sha1(b'tree '+str(len(payload)).encode()+b'\0'+payload).digest()
    return digest_tree(tree).hex()


def network_ledger():
    return {'newRequests':0,'newDownloadedBytes':0,'restorePolicy':'OS denied; no new certificate/signature work',
        'imported':[{'lane':'Mac acquisition','requests':9,'bytes':1764095},
                    {'lane':'research','toolCalls':12,'serializedBytes':393902,'wireBytes':'NOT_OBSERVED'},
                    {'lane':'cloud package GET','requests':3,'bytes':1731673},
                    {'lane':'M13','requests':0},
                    {'lane':'G15/H16/H17','countsAuthority':'copied original network ledgers and receipts',
                     'doNotSumSerializedBytesAsWireBytes':True}],
        'H18':{'authority':'licenses/H18/receipt.json','attempts':26,'addresses':20,'knownHTTP200':23,
               'unknownInitialCaptureStatus':3,'unknownStatusesNotReclassified':True,'completeCapturedResponseBytes':171862,
               'scope':'selected complete response identities only; not a total wire-byte measurement'},
        'certificateChainInternalNetwork':'NOT_MEASURED_BY_ORIGINAL_SIGNATURE_TOOL',
        'Apache':'H16 immutable original bytes copied; no future GET'}


def produce_material(writer,model,generation,verification,compared,old_rows):
    inputs,pkgs,graphs,licenses,catalogs,signatures=model['inputs'],model['pkgs'],model['graphs'],model['licenses'],model['catalogs'],model['signatures']
    for target,source in model['copies'].items():
        if target.is_relative_to(STAGE):writer.copy(target,source)
    for rel,b in fixture_material().items():writer.write(STAGE/rel,b)
    for rel,b in yaml_source_files().items():writer.write(STAGE/'source-yamldotnet'/rel,b)
    tables,source_manifest=source_tables()
    writer.write(STAGE/'identity/upstream-source-files.tsv',source_manifest)
    tables.update(package_tables(pkgs,graphs,generation,licenses,catalogs,signatures))
    tables['license-files.tsv']=[['Luban','5.1.0','MIT','https://github.com/focus-creative-games/luban/blob/df34215d2e22035073d1af83d44e3ac0744dc3b3/LICENSE',
        'v5.1.0','df34215d2e22035073d1af83d44e3ac0744dc3b3','LICENSE','licenses/Luban/LICENSE',1088,
        'a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841','identity/source-files.tsv']]
    for p in pkgs:
        for body in licenses[p['key']]['bodies']:
            tables['license-files.tsv'].append([p['id'],p['version'],body['expression'],body['sourceUrl'],body['sourceTag'],
                body['sourceCommit'],body['sourcePath'],body['localRelativePath'],body['bytes'],body['sha256'],licenses[p['key']]['provenanceRelativePath']])
        writer.json(STAGE/f'identity/package-catalog/{p["key"]}.json',catalogs[p['key']])
        if '.signature.p7s' in p['entries']:
            with zipfile.ZipFile(p['path']) as z:writer.write(STAGE/f'signatures/{p["key"]}.p7s',z.read('.signature.p7s'))
    tables['license-files.tsv'].append(['xunit-family','snapshot-ffee51ac','Apache-2.0-standard-companion',
        'https://www.apache.org/licenses/LICENSE-2.0.txt','N/A_STANDARD_TEXT','N/A_STANDARD_TEXT','LICENSE-2.0.txt',
        'licenses/H16/standard/Apache-2.0.txt',11358,'cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30',
        'licenses/H16/receipt.json; applicability remains tied to individual xunit provenance'])
    for signature in signatures:
        key=signature['packageKey'];original=jload(Path(signature['sourceReceipt']['path']))
        writer.json(STAGE/f'verification/package-signatures/{key}.json',signature)
        writer.write(STAGE/f'verification/package-signatures/{key}.stdout.txt',original['stdout'].encode())
        writer.write(STAGE/f'verification/package-signatures/{key}.stderr.txt',original.get('stderr','').encode())
    tables['tests-projects.tsv']=[[r['projectRelativePath'],r['csprojBytes'],r['csprojSha256'],r['targetFramework'],r['isRoot']]
        for r in rows(T09/'stage-material/identity/tests-projects.tsv')]
    tables['tests-project-edges.tsv']=[list(r.values()) for r in rows(T09/'stage-material/identity/tests-project-edges.tsv')]
    tables['tests-lock-set.tsv']=[];tables['restore-locked-mode.tsv']=[];tables['official-test-project.tsv']=[]
    for project,g in sorted(generation.items()):
        v=verification[project];parent=Path(project).parent.as_posix()
        destinations=[('identity/tests-locks/'+parent+'/packages.lock.json',g['lock']),
            ('identity/tests-assets/'+parent+'/project.assets.json',g['assets']),
            ('identity/tests-assets-canonical/'+parent+'/project.assets.canonical.json',g['canonical']),
            ('verification/tests-assets/'+parent+'/project.assets.json',v['assets']),
            ('verification/tests-assets-canonical/'+parent+'/project.assets.canonical.json',v['canonical'])]
        for rel,b in destinations:writer.write(STAGE/rel,b)
        counts={kind:sum(x['type'].lower()==kind for x in g['parsedLock']['dependencies']['net8.0'].values()) for kind in ('direct','transitive','project')}
        tables['tests-lock-set.tsv'].append([project,*[x for rel,b in destinations[:3] for x in (rel,len(b),digest(b))],
            counts['direct'],counts['transitive'],g['projectReferences']])
        for phase,r in [('generation',g),('verification',v)]:
            require(r['markerPresent']==(phase=='verification') and (phase=='generation' or r['marker'] is True),'locked mode marker')
            tables['restore-locked-mode.tsv'].append(['tests',phase,project,str(r['markerPresent']).lower(),
                'boolean' if r['markerPresent'] else 'absent','true' if r['markerPresent'] else '',digest(r['assets']),'PASS'])
        source=raw(SOURCE/project);root=ET.fromstring(source)
        for node in root.iter():
            kind=node.tag.split('}')[-1]
            if kind not in ('PackageReference','ProjectReference'):continue
            tables['official-test-project.tsv'].append([project,len(source),digest(source),'net8.0',kind,
                node.attrib['Include'],node.attrib.get('Version',node.attrib['Include']),
                node.attrib.get('IncludeAssets','all'),node.attrib.get('PrivateAssets','none')])
    tables['old-roots.tsv']=old_rows
    for name,body in tables.items():material_table(writer,name,body)
    writer.tsv(STAGE/'verification/tests-comparison.tsv',
        ['projectRelativePath','generationLockSha256','verificationLockSha256','generationCanonicalSha256','verificationCanonicalSha256','verdict'],compared)
    commands=future_command_contract(inputs);writer.json(STAGE/'identity/commands.json',commands)
    writer.json(STAGE/'identity/process-budget.json',future_process_budget(inputs,commands))
    writer.json(STAGE/'identity/temp-budget.json',{'schemaVersion':1,'wallClockMs':1800000,
        'roots':[{'absolutePath':str(root),'maxBytes':maximum,'maxFiles':count,'freeSpaceFloorBytes':2147483648,
                  'logCapBytes':min(maximum,67108864),'ownerStage':'M'} for root,maximum,count in
                  [(T,805306368,50000),(E,16777216,10000),(M,536870912,25000)]],
        'B':{'wallClockMs':7200000,'absolutePath':str(BROOT),'maxBytes':2147483648,'maxFiles':50000,'freeSpaceFloorBytes':2147483648,
             'pass1MaxBytes':805306368,'pass2MaxBytes':805306368,'officialTestsAndControlsMaxBytes':536870912,'logCapBytes':67108864},
        'L':{'wallClockMs':1800000,'absolutePath':str(LROOT),'maxBytes':1073741824,'maxFiles':25000,'freeSpaceFloorBytes':2147483648,'logCapBytes':67108864},
        'futureFinalCacheMaxBytes':134217728,'futureEvidenceEachMaxBytes':134217728,'futureStagesRequireSeparateAuthority':True})
    _,core_sha=canonical_tools()
    writer.json(STAGE/'identity/canonicalizer-runtime.json',{'source':'harness/m09-canonicalizer-source.py','coreSha256':core_sha,
        'adapter':'M16 phase roots only; same M09 function AST','markersRemoved':['/project/restore/restoreLockProperties/restoreLockedMode'],
        'inheritedFixtureAuthority':str(E15/'authority/offline-checks.json'),'rawAssetsNeverRewritten':True})
    writer.json(STAGE/'identity/network-ledgers.json',network_ledger())
    writer.json(STAGE/'identity/sbom.json',{'schemaVersion':1,'packages':[
        {'id':p['id'],'version':p['version'],'archive':p['identity'],'catalog':catalogs[p['key']],
         'license':licenses[p['key']],'signature':next((s for s in signatures if s['packageKey']==p['key']),
            {'status':'IMPORTED_NOT_REVERIFIED','originalRecords':'licenses/legacy-acquisition'})} for p in pkgs],
         'network':network_ledger(),'sourceOnlyLimits':'License/source relations are not reproducible-package-build claims.'})
    writer.json(STAGE/'identity/material-contract.json',{'schemaVersion':1,'sourceFreeze':identity(E/'authority/source-freeze.json'),
        'graphs':graphs,'packageCount':31,'runnerRestores':0,'testsGenerationRestores':1,'testsVerificationRestores':1,
        'futureBReceiptContract':commands['futureBReceiptContract'],'ownerMayNotSelfAccept':True,
        'signatures':signatures,'allSourceAndLicenseBytesFromFrozenInputs':True})
    entries=[]
    material_paths=sorted([p for p in writer.written if p.is_relative_to(STAGE)],key=str) if writer.memory else files(STAGE)
    for p in material_paths:
        b=writer.written[p] if writer.memory else raw(p)
        entries.append([p.relative_to(STAGE).as_posix(),len(b),digest(b),'material',OWNER['threadId'],
                        'M16 source-freeze; M15/G15/H16/H17/H18/M13 exact input map'])
    material_table(writer,'material-files.tsv',entries)
    return {'packageCount':31,'licenseRows':len(tables['license-files.tsv']),'dependencyRows':len(tables['package-closure.tsv']),
            'entryRows':len(tables['package-entries.tsv']),'deploymentRows':len(tables['expected-deployment.tsv']),
            'materialFiles':len(entries)+1}


def check_b_data(receipt,runner,read,enumerate_files,manifest_sha):
    require(receipt.get('status')=='B-ACCEPTED','B receipt not accepted')
    require(receipt.get('materialManifestSha256')==manifest_sha,'B receipt material mismatch')
    require(re.fullmatch('[0-9a-f]{40}',receipt.get('reviewedHead','')) is not None,'B head missing')
    acceptance=receipt.get('independentAcceptance',{})
    require(acceptance.get('verdict')=='ACCEPT' and acceptance.get('reviewedHead')==receipt['reviewedHead'] and
            acceptance.get('ownerThread') and acceptance.get('ownerThread')!=receipt.get('authorThread') and
            acceptance.get('receiptSha256') and acceptance.get('receiptPath'),'B independent acceptance not bound')
    accepted_raw=read(Path(acceptance['receiptPath']))
    require(digest(accepted_raw)==acceptance['receiptSha256'],'B independent receipt drift')
    independent=json_data(accepted_raw)
    require(independent.get('verdict')=='ACCEPT' and independent.get('reviewedHead')==receipt['reviewedHead'],
            'B independent receipt does not accept this head')
    expected={};licenses=set()
    for group in ('runnerFiles','licenseFiles'):
        require(isinstance(receipt.get(group),list) and receipt[group],'B empty file set')
        for item in receipt[group]:
            rel=safe_relative(item['relativePath']);path=runner/rel;b=read(path)
            require(len(b)==item['bytes'] and digest(b)==item['sha256'],'B deployed bytes drift: '+rel)
            if rel in expected:require(expected[rel]==(len(b),digest(b)),'B duplicate identity conflict')
            expected[rel]=(len(b),digest(b))
            if group=='licenseFiles':licenses.add(rel)
    require(set(enumerate_files(runner))==set(expected),'B exact runner set differs')
    require('YamlDotNet.dll' in expected and not any('yamldotnet.netcore' in r.lower() for r in expected),'B YAML replacement missing')
    require(any('yaml' in r.lower() and 'license' in r.lower() for r in licenses),'B YAML licenses absent')
    require(receipt.get('fallbackLicenseFiles') and set(receipt['fallbackLicenseFiles']).issubset(licenses),'B fallback licenses absent')
    return True


def check_b(receipt_path,runner,expect):
    # A future L activation must additionally pin the independently accepted
    # receipt itself. This gate checks its contents; it grants no execution.
    receipt=jload(receipt_path);manifest=identity(M/'identity/material-files.tsv')['sha256']
    try:
        check_b_data(receipt,runner,raw,lambda root:[p.relative_to(root).as_posix() for p in files(root)],manifest)
        passed=True;reason=None
    except (GateError,OSError,ValueError,KeyError) as error:
        passed=False;reason=str(error)
    require(passed==(expect=='pass'),'B control outcome differs from expectation')
    print(json.dumps({'gatePassed':passed,'role':'DUT','exitCode':0 if passed else 20,'expected':expect,'reason':reason},ensure_ascii=False))
    return 0 if passed else 20

def assert_controls(stage):
    contract=jload(M/'identity/commands.json');checked=[]
    for command in contract['commands']:
        if command['stage']!=stage or command['role']=='harness':continue
        receipt=jload(Path(command['receiptPath']))
        require(receipt['role']=='DUT' and receipt['argv']==command['argv'] and receipt['quiescent'] is True,
                'actual DUT process receipt incomplete')
        actual=receipt['exitCode'];expected=command['expectedExit']
        require((actual!=0) if expected=='nonzero' else (actual==expected),'DUT exit differs: '+command['id'])
        if 'malformed-yaml' in command['id']:
            require(raw(Path(command['stderrPath'])),'malformed YAML lacks direct diagnostic')
            for root in map(Path,command['creates']):require(not root.exists() or not files(root),'malformed YAML published')
        if command['id'] in ('L-negative-duplicate','L-negative-parameters','L-negative-severity'):
            marker={'L-negative-duplicate':b'FMLOC002','L-negative-parameters':b'FMLOC007','L-negative-severity':b'FMLOC004'}[command['id']]
            require(marker in raw(Path(command['stdoutPath']))+raw(Path(command['stderrPath'])),'negative diagnostic differs')
        checked.append(command['id'])
    print(json.dumps({'role':'harness','exitCode':0,'checkedActualDutReceipts':checked}));return 0


def assemble_model():
    inputs,pending=m16_inputs();preservation=preservation_contract(inputs)
    pkgs=packages(inputs);graphs,source_manifest=source_graphs()
    licenses=license_model(pkgs,pending);catalogs,gaps=catalog_model(inputs,pkgs)
    pending+=gaps;signatures=package_signature_projections(pkgs)
    copies=planned_copies(inputs,pkgs,graphs,licenses);write_set=build_write_set(copies,pkgs,graphs)
    return {'inputs':inputs,'pending':pending,'preservation':preservation,'pkgs':pkgs,'graphs':graphs,
            'sourceManifest':source_manifest,'licenses':licenses,'catalogs':catalogs,'signatures':signatures,
            'copies':copies,'writeSet':write_set}


def pure_delta_fixtures(model):
    results=[]
    def case(name,action,reject=False):
        try:action()
        except (GateError,ValueError,KeyError,OSError) as error:
            require(reject,'fixture '+name+' unexpectedly failed: '+str(error))
            results.append({'name':name,'expected':'reject','status':'PASS'});return
        require(not reject,'fixture '+name+' accepted invalid input')
        results.append({'name':name,'expected':'pass','status':'PASS'})
    writer=ScopedWriter(model['writeSet'],memory=True)
    case('finite-write-accept',lambda:writer.write(T/'local-feed'/Path(model['pkgs'][0]['path']).name,b'fixture'))
    case('finite-write-reject-duplicate',lambda:writer.write(T/'local-feed'/Path(model['pkgs'][0]['path']).name,b'fixture'),True)
    case('finite-write-reject-production',lambda:writer.write(REPO/'Config/escape.txt',b''),True)
    case('finite-write-reject-traversal',lambda:writer.write(T/'stage-material/../escape',b''),True)
    case('sdk-pattern-accept',lambda:writer.authorize(T/'tests-generation/scratch/lock'/('a'*40)))
    case('sdk-pattern-reject-wide',lambda:writer.authorize(T/'tests-generation/scratch/lock/escape'),True)
    fixture_root=Path('/synthetic-b-runner');data={fixture_root/'YamlDotNet.dll':b'fixture-dll',
        fixture_root/'licenses/YamlDotNet/LICENSE':b'yaml-license',fixture_root/'licenses/fallback/LICENSE':b'fallback-license'}
    acceptance_path=Path('/synthetic-independent-acceptance.json');head='a'*40
    accepted=json_bytes({'verdict':'ACCEPT','reviewedHead':head});data[acceptance_path]=accepted
    receipt={'status':'B-ACCEPTED','materialManifestSha256':'b'*64,'reviewedHead':head,'authorThread':'author',
        'independentAcceptance':{'verdict':'ACCEPT','ownerThread':'independent','reviewedHead':head,
                                 'receiptPath':str(acceptance_path),'receiptSha256':digest(accepted)},
        'runnerFiles':[{'relativePath':'YamlDotNet.dll','bytes':11,'sha256':digest(b'fixture-dll')}],
        'licenseFiles':[{'relativePath':p.relative_to(fixture_root).as_posix(),'bytes':len(b),'sha256':digest(b)}
                        for p,b in data.items() if p.is_relative_to(fixture_root/'licenses')],
        'fallbackLicenseFiles':['licenses/fallback/LICENSE']}
    def gate(d=data,r=receipt):
        return check_b_data(r,fixture_root,lambda p:d[p],lambda root:[p.relative_to(root).as_posix() for p in d if p.is_relative_to(root)],'b'*64)
    case('B-receipt-exact-pass',gate)
    tampered=dict(data);tampered[fixture_root/'YamlDotNet.dll']=b'wrong'
    case('B-tampered-YAML-reject',lambda:gate(tampered),True)
    missing=dict(data);del missing[fixture_root/'licenses/YamlDotNet/LICENSE']
    case('B-missing-YAML-license-reject',lambda:gate(missing),True)
    fallback=dict(data);fallback[fixture_root/'licenses/fallback/LICENSE']=b'wrong'
    case('B-fallback-license-drift-reject',lambda:gate(fallback),True)
    own=copy.deepcopy(receipt);own['independentAcceptance']['ownerThread']='author'
    case('B-self-acceptance-reject',lambda:gate(data,own),True)
    extra=dict(data);extra[fixture_root/'extra.dll']=b'extra'
    case('B-extra-file-reject',lambda:gate(extra),True)
    case('activation-missing-reject',lambda:check_activation({}, {},'','',[]),True)
    freeze={'sourceReady':True,'executorComplete':True,'materialWriteSetComplete':True}
    activation={'centralThread':AUTHORITY['threadId'],'scope':'EXECUTE_M16','ownerThread':OWNER['threadId'],
        'ownerHost':'local','ownerTurn':'synthetic','reviewedHead':head,'executionHead':head,
        'reviewStatus':'COMPLETED_NO_UNRESOLVED_FINDINGS','sourceFreezeSha256':'f'*64,'driverSha256':'d'*64,
        'argv':['synthetic'],'restoreLimits':{'runner':0,'testsGeneration':1,'testsVerification':1}}
    case('activation-exact-pass',lambda:check_activation(activation,freeze,'f'*64,'d'*64,['synthetic']))
    drift=copy.deepcopy(activation);drift['executionHead']='c'*40
    case('activation-head-drift-reject',lambda:check_activation(drift,freeze,'f'*64,'d'*64,['synthetic']),True)
    case('environment-inheritance',lambda:require(all(child_environment(phase_layout('generation'))[1].values()),'env'))
    g={p:{'lock':b'lock','canonical':b'canonical'} for p in model['graphs']['tests']['nodes']}
    case('13-lock-compare-pass',lambda:compare_phases(g,copy.deepcopy(g)))
    bad=copy.deepcopy(g);bad[next(iter(bad))]['canonical']=b'drift'
    case('13-lock-compare-drift-reject',lambda:compare_phases(g,bad),True)
    command_contract=future_command_contract(model['inputs'])
    required={'executableAbsolutePath','executableSha256','argv','cwd','env','reads','creates','timeoutMs','killGraceMs','stdoutMaxBytes','stderrMaxBytes'}
    case('B-L-command-contract-complete',lambda:require(all(required.issubset(c) and c['argv'][0]==c['executableAbsolutePath']
        and c['timeoutMs']>0 for c in command_contract['commands']),'command schema'))
    case('B-L-unique-command-ids',lambda:require(len({c['id'] for c in command_contract['commands']})==len(command_contract['commands']),'ids'))
    # Source-only round trip exercises the whole producer with synthetic tests
    # locks. These never become material or claim an actual restore result.
    synthetic={};bykey={p['key']:p for p in model['pkgs']}
    for project in model['graphs']['tests']['nodes']:
        deps={p['id']:{'type':'Direct','resolved':p['version'],'requested':'['+p['version']+']','contentHash':p['sha512Base64']}
              for p in model['pkgs']} if project==model['graphs']['tests']['root'] else {}
        targets={p['id']+'/'+p['version']:{'type':'package'} for p in model['pkgs']} if deps else {}
        assets={'targets':{'net8.0':targets},'project':{'frameworks':{'net8.0':{'dependencies':{}}}},'libraries':{}}
        lock={'dependencies':{'net8.0':deps}}
        synthetic[project]={'lock':json_bytes(lock),'canonical':b'fixture-canonical','assets':json_bytes(assets),
            'parsedAssets':assets,'parsedLock':lock,'markerPresent':False,'marker':None,'projectReferences':0}
    ver=copy.deepcopy(synthetic)
    for r in ver.values():r['markerPresent']=True;r['marker']=True
    fixture_model=dict(model);fixture_model['catalogs']=dict(model['catalogs'])
    for key in bykey:
        fixture_model['catalogs'].setdefault(key,{'catalogSha512':bykey[key]['sha512Base64'],'catalogUrl':'synthetic-fixture-only',
            'immutablePackageUrl':'synthetic-fixture-only','scope':'not actual catalog evidence'})
    # Producer reads its future freeze only for identity. Supply a synthetic
    # identity at that single seam; all frozen-source/package reads remain real.
    original_identity=globals()['identity']
    def fixture_identity(path,runtime=False):
        if Path(path)==E/'authority/source-freeze.json':return {'path':'synthetic-freeze','bytes':0,'sha256':'0'*64}
        return original_identity(path,runtime)
    produced=ScopedWriter(model['writeSet'],memory=True)
    for leaf in ('m16-inputs.tsv','m16-write-set.tsv'):
        # These are execution-time preflight copies, not material producer inputs.
        require(not any(p.is_relative_to(STAGE) and s==E/'source'/leaf for p,s in model['copies'].items()),'self-dependent material input')
    try:
        globals()['identity']=fixture_identity
        case('full-material-producer-memory-roundtrip',lambda:produce_material(produced,fixture_model,synthetic,ver,
            compare_phases(synthetic,ver),[]))
    finally:globals()['identity']=original_identity
    manifest=json_bytes({'fixtureOnly':True,'files':len(produced.written)})
    case('material-schema-all-tables-present',lambda:require(all(STAGE/'identity'/name in produced.written for name in SCHEMAS),'schema missing'))
    case('material-exact-write-set-covered',lambda:require(set(produced.written)==
        {T/r[1] for r in model['writeSet'] if r[0]=='T16' and r[2]=='exact' and r[1].startswith('stage-material/')},'material write set incompleteness'))
    return {'status':'PASS','checks':results,'count':len(results),'childStarts':0,'networkRequests':0,
            'syntheticMaterialFiles':len(produced.written),'syntheticOutputNeverWritten':True,
            'OSIsolationActuallyExercised':False,'inherited30Checks':identity(E15/'authority/offline-checks.json')}


def prepare():
    started=time.monotonic()
    require(not T.exists() and not M.exists(),'source prep requires absent execution/material roots')
    require(set(files(E))=={DRIVER},'source prep cannot overwrite an existing snapshot')
    model=assemble_model();checks=pure_delta_fixtures(model)
    require(time.monotonic()-started<=120,'SOURCE_PREPARATION_120_SECOND_BUDGET')
    writer=ScopedWriter(model['writeSet'])
    writer.tsv(E/'source/m16-inputs.tsv',['sourcePath','bytes','sha256','authority'],sorted(model['inputs'].values()))
    writer.tsv(E/'source/m16-write-set.tsv',['root','relativePath','matchKind','role'],model['writeSet'])
    ready=not model['pending']
    prep={'schemaVersion':1,'owner':OWNER,'authority':AUTHORITY,'scope':'SOURCE_PREPARATION_ONLY',
        'status':'SOURCE_READY' if ready else 'SOURCE_COMPLETE_INPUTS_PENDING','sourceReady':ready,
        'executorComplete':True,'materialWriteSetComplete':True,'remainingInputs':model['pending'],
        'inputs':len(model['inputs']),'writeRules':len(model['writeSet']),'sourceFiles':931,'packageCount':31,
        'catalogCount':len(model['catalogs']),'licenseBodySets':len(model['licenses']),'graphs':model['graphs'],
        'preservation':model['preservation'],'restoreBudget':restore_budget_contract(),
        'futureExecutionArgv':['/opt/homebrew/bin/python3','-B',str(DRIVER),'execute'],
        'futureActivationInput':'stdin JSON, <=16384 bytes, central authority + reviewed/execution head + freeze hashes',
        'zeroExecution':{'dotnet':0,'restores':0,'Unity':0,'network':0,'GitWrites':0,'BLRootCreates':0,'childStarts':0},
        'networkIsolation':'policy source checked only; actual OS enforcement not yet exercised',
        'durationSeconds':time.monotonic()-started,
        'priorValidationAttempt':{'command':'prepare','exitCode':1,'durationSeconds':0.790955541,
            'reason':'unsupported H16 source kind','toolChunk':'3fc517','newLeavesWritten':0,
            'correction':'Added exact G-reused-body relation; no prior evidence overwritten'},
        'validationAttemptLimit':2,'currentValidationAttempt':2}
    writer.json(E/'authority/prepare.json',prep);writer.json(E/'authority/offline-checks.json',checks)
    seal={'schemaVersion':1,'owner':OWNER,'authority':AUTHORITY,'status':prep['status'],'sourceReady':ready,
        'executorComplete':True,'materialWriteSetComplete':True,'remainingInputs':model['pending'],
        'expectedSixLeaves':list(PREP),'filesExcludingThisSeal':[identity(E/rel) for rel in PREP[:-1]],
        'offlineChecks':{'count':checks['count'],'status':checks['status'],'seconds':time.monotonic()-started},
        'zeroExecution':prep['zeroExecution'],'executionStillRequiresCentralAndGitHubHeadReview':True,
        'completionReceiver':AUTHORITY['threadId']+'/local','createdAtUtc':datetime.now(timezone.utc).isoformat()}
    writer.json(E/'authority/source-freeze.json',seal)
    require({p.relative_to(E).as_posix() for p in files(E)}==set(PREP),'six-leaf source seal')
    require(not T.exists() and not M.exists(),'source stage wrote execution roots')
    print(json.dumps({'status':seal['status'],'checks':checks['count'],'elapsed':time.monotonic()-started,
                      'freeze':identity(E/'authority/source-freeze.json'),'remaining':model['pending']},ensure_ascii=False))


def manifest_rows(root,excludes=()):
    return [[p.relative_to(root).as_posix(),p.stat().st_size,digest(raw(p))]
            for p in files(root) if p.relative_to(root).as_posix() not in excludes]


def preserved_rows(contract,phase):
    result=[[root,rel,count,sha,phase] for root,rel,count,sha in contract['explicitLeaves']]
    for root,expected in contract['aggregateRoots'].items():
        if expected['exists']:
            result.extend([[root,*row,phase] for row in manifest_rows(Path(root))])
    return result


def execute():
    started=time.monotonic();freeze_path=E/'authority/source-freeze.json';freeze=jload(freeze_path)
    freeze_sha=digest(raw(freeze_path));source_sha=digest(raw(DRIVER))
    require(len(sys.argv)==2 and sys.argv[1]=='execute','exact execute argv required')
    authority_bytes=sys.stdin.buffer.read(16385);require(len(authority_bytes)<=16384,'activation too large')
    activation=json_data(authority_bytes)
    check_activation(activation,freeze,freeze_sha,source_sha,['/opt/homebrew/bin/python3','-B',str(DRIVER),'execute'])
    require(not T.exists() and not M.exists(),'execution roots must be absent; no same-root retry')
    require({p.relative_to(E).as_posix() for p in files(E)}==set(PREP),'entry evidence set differs')
    for item in freeze['filesExcludingThisSeal']:fixed(item['path'],item['bytes'],item['sha256'])
    model=assemble_model();require(not model['pending'],'required catalog/license input remains pending')
    require(raw(E/'source/m16-inputs.tsv')==tsv_bytes(['sourcePath','bytes','sha256','authority'],sorted(model['inputs'].values())),
            'frozen input map differs')
    require(raw(E/'source/m16-write-set.tsv')==tsv_bytes(['root','relativePath','matchKind','role'],model['writeSet']),
            'frozen write set differs')
    writer=ScopedWriter(model['writeSet']);state={'activationMonotonic':started,'nextPhase':'generation','writer':writer}
    completed=[];blocked=None;material_summary=None;old_after=None
    budget_gate(writer,started)
    writer.json(E/'authority/activation.json',activation)
    try:
        old_before=verify_preservation(model['preservation'],model['inputs'])
        old_rows=preserved_rows(model['preservation'],'before');budget_gate(writer,started)
        writer.json(E/'authority/preflight.json',{'sourceFreezeSha256':freeze_sha,'driverSha256':source_sha,
            'oldRootsBefore':old_before,'entryRootsAbsent':True,'owner':activation['ownerThread'],
            'ownerTurn':activation['ownerTurn'],'reviewedHead':activation['reviewedHead']})
        writer.json(E/'authority/signature-projections.json',model['signatures'])
        for target,source in model['copies'].items():
            if not target.is_relative_to(STAGE):writer.copy(target,source)
        require(len(files(T/'local-feed'))==31,'feed count')
        for phase in ('generation','verification'):
            config=('<?xml version="1.0" encoding="utf-8"?><configuration><packageSources><clear />'
                '<add key="local-only" value="'+str(T/'local-feed')+'" /></packageSources>'
                '<fallbackPackageFolders><clear /></fallbackPackageFolders><disabledPackageSources><clear />'
                '</disabledPackageSources></configuration>\n').encode()
            writer.write(phase_layout(phase)['config'],config)
        state['allMaterialPrerequisitesPassed']=True;results={}
        for phase in ('generation','verification'):
            budget_gate(writer,started)
            outputs=[T/r[1] for r in model['writeSet'] if r[0]=='T16' and r[2]=='exact' and
                     r[3]=='restore-output' and r[1].startswith('tests-'+phase+'/')]
            receipt=run_isolated_restore(phase,outputs,activation,freeze,freeze_sha,source_sha,state)
            stdout=receipt.pop('stdout');stderr=receipt.pop('stderr')
            receipt['stdout']={'bytes':len(stdout),'sha256':digest(stdout)};receipt['stderr']={'bytes':len(stderr),'sha256':digest(stderr)}
            writer.write(E/f'process/tests-{phase}.stdout.txt',stdout);writer.write(E/f'process/tests-{phase}.stderr.txt',stderr)
            writer.json(E/f'process/tests-{phase}.json',receipt);completed.append(receipt)
            require(receipt['failure'] is None,'restore '+phase+': '+str(receipt['failure']))
            results[phase]=collect_phase(phase,model['graphs'],model['pkgs']);budget_gate(writer,started)
            if phase=='generation':
                for project,r in results[phase].items():
                    writer.write(phase_layout('verification')['source']/Path(project).parent/'packages.lock.json',r['lock'])
        compared=compare_phases(results['generation'],results['verification'])
        old_after=verify_preservation(model['preservation'],model['inputs'])
        old_rows+=preserved_rows(model['preservation'],'after')
        material_summary=produce_material(writer,model,results['generation'],results['verification'],compared,old_rows)
        budget_gate(writer,started)
        stage_rows=rows(STAGE/'identity/material-files.tsv')
        expected={STAGE/r['relativePath'] for r in stage_rows}|{STAGE/'identity/material-files.tsv'}
        require(set(files(STAGE))==expected,'stage exact file set differs')
        for row in stage_rows:fixed(STAGE/row['relativePath'],row['bytes'],row['sha256'])
        require(not M.exists(),'material create-once root exists')
        for p in files(STAGE):writer.copy(M/p.relative_to(STAGE),p)
        require(manifest_rows(STAGE)==manifest_rows(M),'sealed material copy differs')
        budget_gate(writer,started)
    except Exception as error:
        blocked=type(error).__name__+': '+str(error)
    finally:
        # First failure retained; never restart a child or repair either root.
        try:
            after=verify_preservation(model['preservation'],model['inputs'])
            old_after=after
        except Exception as error:
            if blocked is None:blocked='preservation after: '+str(error)
            old_after={'equal':False,'failure':str(error)}
        writer.json(E/'old-root-equality.json',old_after)
        writer.json(E/'network-ledgers.json',network_ledger())
        writer.tsv(E/'process/completed-commands.tsv',['phase','exitCode','durationSeconds','failure'],
                   [[r['phase'],r['exitCode'],r['durationSeconds'],r['failure'] or ''] for r in completed])
        writer.tsv(E/'process/not-run-commands.tsv',['id','reason'],
                   [[phase,blocked or 'not reached'] for phase in ('generation','verification') if phase not in [r['phase'] for r in completed]]+
                   [['runner-restore','forbidden; reused runner evidence'],['B','separate authority required'],['L','separate authority required']])
        for root,name in [(T/'acquisition','acquisition-files.tsv'),(T/'local-feed','feed-files.tsv'),(M,'material-files.tsv'),(T,'temp-files.tsv')]:
            writer.tsv(E/name,['relativePath','bytes','sha256'],manifest_rows(root) if root.exists() else [])
        writer.json(E/'result.json',{'schemaVersion':1,'status':'BLOCKED_EXECUTION' if blocked else 'READY_FOR_INDEPENDENT_M_ACCEPTANCE',
            'blocker':blocked,'owner':{'threadId':activation['ownerThread'],'hostId':activation['ownerHost'],'turnId':activation['ownerTurn']},
            'authority':activation,'roots':{k:str(v) for k,v in ROOTS.items()},'completedCommands':completed,
            'material':material_summary,'oldRootEquality':old_after,'durationSeconds':time.monotonic()-started,
            'sourceFreezeSha256':freeze_sha,'zeroActionReceipt':{'Unity':0,'GitWrites':0,'networkRequests':0,'B':0,'L':0,'runnerRestores':0},
            'returnTo':AUTHORITY['threadId']+'/local','authorAcceptance':'NOT_GRANTED'})
        writer.tsv(E/'evidence-files.tsv',['relativePath','bytes','sha256'],manifest_rows(E,('evidence-files.tsv',)))
    print(json.dumps({'status':'BLOCKED_EXECUTION' if blocked else 'READY_FOR_INDEPENDENT_M_ACCEPTANCE','blocker':blocked,
                      'result':identity(E/'result.json')},ensure_ascii=False))
    return 1 if blocked else 0


def main():
    parser=argparse.ArgumentParser();sub=parser.add_subparsers(dest='mode',required=True)
    sub.add_parser('prepare');sub.add_parser('execute')
    assertions=sub.add_parser('assert-controls');assertions.add_argument('--stage',choices=('B','L'),required=True)
    gate=sub.add_parser('check-b');gate.add_argument('--receipt',type=Path,required=True)
    gate.add_argument('--runner',type=Path,required=True);gate.add_argument('--expect',choices=('pass','reject'),required=True)
    args=parser.parse_args()
    if args.mode=='prepare':prepare();return 0
    if args.mode=='execute':return execute()
    if args.mode=='assert-controls':return assert_controls(args.stage)
    return check_b(args.receipt,args.runner,args.expect)


if __name__=='__main__':
    try:sys.exit(main())
    except Exception as error:
        print(json.dumps({'status':'BLOCKED','error':type(error).__name__,'reason':str(error)},ensure_ascii=False),file=sys.stderr)
        sys.exit(1)
