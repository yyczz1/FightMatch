# FIX22: fixed full progress path and single-callback candidate derivation; inherited FIX21 binding.
import ast
import collections
import datetime
import difflib
import hashlib
import json
import os
import pathlib
import plistlib
import re
import shlex
import shutil
import signal
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = pathlib.Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
EDITOR = '/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity'
OUT = None  # Future external activation selects the create-once output root.
SOURCE_ONLY = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-20-source'
R17 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf/q4-correction-17'
R15 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-15'
R18 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-18'
R19 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-19'
PREDECESSOR = ROOT / 'docs/versioning/2026-10-01-ugui-pr/fix21/validation-tools/runner.py'
R17_RUNNER = R17 / 'validation-tools/runner.py'
pre = None  # Loaded from verified bytes only after the binding gate.
FONT = ROOT / 'Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset'
TIMEOUT = 360
MAX_LOG = 8 * 1024 * 1024
MAX_EVIDENCE = 192 * 1024 * 1024
MIN_FREE = 2 * 1024 * 1024 * 1024
OWNER = None  # Supplied by the separately authorized execution activation.


# FIX21 binding gate. Caller authority must come from the central out-of-band
# activation, never from binding-contract.json or candidate activation.json.
# A trusted launcher must verify these source bytes before importing this module.
import argparse
import io
import stat

BINDING_STATE = None
BINDING_AUTHORITY = None
BINDING_ADAPTER = None
BINDING_CONTRACT = None
BINDING_CHECK_AUTHORITY = None

class BindingFailure(Exception):
    def __init__(self, code, path='', expected=None, actual=None):
        self.detail = dict(code=code, path=str(path), expected=expected, actual=actual)
        super().__init__(json.dumps(self.detail, ensure_ascii=False))

def binding_need(ok, code, path='', expected=None, actual=None):
    if not ok:
        raise BindingFailure(code, path, expected, actual)

def raw_identity(data):
    return dict(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())

def git_oid(kind, data):
    return hashlib.sha1(kind.encode('ascii') + b' ' + str(len(data)).encode('ascii') + b'\0' + data).hexdigest()

def safe_relative(value):
    binding_need(isinstance(value, str) and value and '\\' not in value and '\0' not in value,
                 'BLOCKED_PATH', value)
    path = pathlib.PurePosixPath(value)
    binding_need(not path.is_absolute() and all(p not in ('', '.', '..') for p in value.split('/')),
                 'BLOCKED_PATH', value)
    return value

def json_bytes(data, path):
    try:
        def pairs(items):
            result = {}
            for k,v in items:
                binding_need(k not in result, 'BLOCKED_DUPLICATE_MAPPING', path, actual=k)
                result[k] = v
            return result
        return json.loads(data.decode('utf-8'), object_pairs_hook=pairs)
    except BindingFailure:
        raise
    except (ValueError, UnicodeError) as exc:
        raise BindingFailure('BLOCKED_JSON', path, actual=str(exc))

def checked_object(adapter, oid, kind):
    binding_need(isinstance(oid,str) and re.fullmatch('[0-9a-f]{40}',oid),
                 'BLOCKED_OBJECT_ID', oid)
    actual_kind, raw = adapter.object(oid)
    binding_need(actual_kind == kind, 'BLOCKED_OBJECT_TYPE', oid, kind, actual_kind)
    binding_need(git_oid(actual_kind,raw) == oid, 'BLOCKED_OBJECT_HASH', oid, oid, git_oid(actual_kind,raw))
    return raw

def read_git_tree(adapter, head, expected_tree):
    commit = checked_object(adapter,head,'commit')
    headers = commit.split(b'\n\n',1)[0].splitlines()
    trees = [line[5:].decode('ascii') for line in headers if line.startswith(b'tree ')]
    binding_need(trees == [expected_tree], 'BLOCKED_HEAD_TREE', head, expected_tree, trees)
    found = {}
    pending = [('',expected_tree)]
    depth = 0
    while pending:
        depth += 1
        binding_need(depth <= 128, 'BLOCKED_TREE_DEPTH')
        adapter.prefetch([oid for _,oid in pending])
        next_level = []
        for prefix,oid in pending:
            raw = checked_object(adapter,oid,'tree'); pos = 0; seen = set()
            while pos < len(raw):
                space = raw.find(b' ',pos); nul = raw.find(b'\0',space+1)
                binding_need(space > pos and nul > space and nul+21 <= len(raw), 'BLOCKED_TREE_FORMAT',oid)
                try:
                    mode=raw[pos:space].decode('ascii'); name=raw[space+1:nul].decode('utf-8')
                except UnicodeError:
                    raise BindingFailure('BLOCKED_TREE_FORMAT',oid)
                pos=nul+21; child=raw[nul+1:pos].hex()
                binding_need(name not in seen, 'BLOCKED_DUPLICATE_MAPPING',prefix+name); seen.add(name)
                binding_need('/' not in name, 'BLOCKED_PATH',prefix+name)
                path=safe_relative(prefix+name)
                if mode=='40000':
                    next_level.append((path+'/',child))
                else:
                    binding_need(mode in ('100644','100755'), 'BLOCKED_NONREGULAR_TREE',path,actual=mode)
                    binding_need(path not in found, 'BLOCKED_DUPLICATE_MAPPING',path)
                    found[path]=dict(path=path,mode=mode,oid=child)
        pending=next_level
    return found,dict(head=head,tree=expected_tree,commit=raw_identity(commit),verifiedGitObjectId=head)

def validate_git_environment(environment, metadata):
    bad=sorted(k for k in environment if k.startswith('GIT_') and k not in ('GIT_PAGER','GIT_OPTIONAL_LOCKS','GIT_NO_REPLACE_OBJECTS'))
    binding_need(not bad,'BLOCKED_GIT_ENV',actual=bad)
    binding_need(not metadata.get('redirected'),'BLOCKED_GIT_REDIRECT')
    binding_need(not metadata.get('replace'),'BLOCKED_GIT_REPLACE')

class GitDiskAdapter:
    """Only raw, read-only Git object access. No refs/index mutation, filters or checkout."""
    def __init__(self, root):
        self.root=pathlib.Path(root)
        self.cache={}
        self.guard_environment()
    def guard_environment(self):
        binding_need(str(self.root)==str(ROOT) and self.root.resolve()==self.root, 'BLOCKED_REPOSITORY',self.root)
        validate_git_environment(os.environ,{})
        for path in [self.root, self.root/'.git', self.root/'.git/objects']:
            binding_need(path.is_dir() and not path.is_symlink(),'BLOCKED_GIT_REDIRECT',path)
        for directory in ['objects/info','objects/pack','refs']:
            binding_need(not (self.root/'.git'/directory).is_symlink(),'BLOCKED_GIT_REDIRECT',directory)
        for leaf in ['commondir','objects/info/alternates','objects/info/http-alternates']:
            p=self.root/'.git'/leaf
            binding_need(not p.exists() and not p.is_symlink(),'BLOCKED_GIT_REDIRECT',p)
        replace=self.root/'.git/refs/replace'
        binding_need(not replace.is_symlink() and (not replace.exists() or not any(replace.rglob('*'))),'BLOCKED_GIT_REPLACE',replace)
        packed=self.root/'.git/packed-refs'
        if packed.exists():
            binding_need(re.search(rb'\srefs/replace/',packed.read_bytes()) is None,'BLOCKED_GIT_REPLACE',packed)
    def prefetch(self, oids):
        needed=list(dict.fromkeys(o for o in oids if o not in self.cache))
        if not needed:return
        for oid in needed:
            binding_need(re.fullmatch('[0-9a-f]{40}',oid),'BLOCKED_OBJECT_ID',oid)
        self.guard_environment()
        env=dict(os.environ,GIT_NO_REPLACE_OBJECTS='1',GIT_OPTIONAL_LOCKS='0',GIT_TERMINAL_PROMPT='0',GIT_NO_LAZY_FETCH='1')
        proc=subprocess.run(['git','--no-replace-objects','-c','protocol.allow=never','--git-dir='+str(self.root/'.git'),'cat-file','--batch'],
                            input=('\n'.join(needed)+'\n').encode(),capture_output=True,env=env,timeout=60)
        binding_need(proc.returncode==0,'BLOCKED_OBJECT_READ',actual=proc.stderr.decode(errors='replace')[:1000])
        pos=0
        for oid in needed:
            end=proc.stdout.find(b'\n',pos); header=proc.stdout[pos:end].split();pos=end+1
            binding_need(len(header)==3 and header[0].decode()==oid,'BLOCKED_OBJECT_MISSING',oid,actual=b' '.join(header).decode(errors='replace'))
            kind=header[1].decode();size=int(header[2]);raw=proc.stdout[pos:pos+size];pos+=size
            binding_need(len(raw)==size and proc.stdout[pos:pos+1]==b'\n','BLOCKED_OBJECT_READ',oid)
            pos+=1
            binding_need(git_oid(kind,raw)==oid,'BLOCKED_OBJECT_HASH',oid,oid,git_oid(kind,raw))
            self.cache[oid]=(kind,raw)
        binding_need(pos==len(proc.stdout),'BLOCKED_OBJECT_READ',actual='trailing batch bytes')
    def object(self, oid):
        self.prefetch([oid]);return self.cache[oid]
    def path_kind(self, relative):
        path=self.root/safe_relative(relative)
        if path.is_symlink():return 'symlink'
        if not path.exists():return 'missing'
        if path.is_file():return 'file'
        if path.is_dir():return 'directory'
        return 'other'
    def read(self, relative):
        relative=safe_relative(relative);path=self.root/relative
        return self.read_absolute(path)
    def read_absolute(self,path):
        path=pathlib.Path(path)
        for parent in [path]+list(path.parents):
            binding_need(not parent.is_symlink(),'BLOCKED_SYMLINK',path)
        try:
            fd=os.open(path,os.O_RDONLY|getattr(os,'O_NOFOLLOW',0))
            with os.fdopen(fd,'rb') as f:
                before=os.fstat(f.fileno())
                binding_need(stat.S_ISREG(before.st_mode),'BLOCKED_NONREGULAR_FILE',path)
                data=f.read();after=os.fstat(f.fileno())
            binding_need((before.st_ino,before.st_size,before.st_mtime_ns)==(after.st_ino,after.st_size,after.st_mtime_ns),
                         'BLOCKED_READ_DRIFT',path)
            return data
        except (FileNotFoundError,IsADirectoryError) as exc:
            raise BindingFailure('BLOCKED_FILE_MISSING',path,actual=str(exc))
    def files(self,prefix):
        prefix=safe_relative(prefix);base=self.root/prefix
        binding_need(base.is_dir() and not base.is_symlink(),'BLOCKED_DIRECTORY',prefix)
        paths=[]
        for current,dirs,files in os.walk(base,followlinks=False):
            for name in dirs:
                binding_need(not (pathlib.Path(current)/name).is_symlink(),'BLOCKED_SYMLINK',pathlib.Path(current)/name)
            for name in files:
                p=pathlib.Path(current)/name
                binding_need(not p.is_symlink() and p.is_file(),'BLOCKED_NONREGULAR_FILE',p)
                paths.append(str(p.relative_to(self.root)))
        return sorted(paths)

def check_authority(authority, live, mode):
    binding_need(isinstance(authority,dict) and authority.get('runId') and authority.get('owner',{}).get('turnId'),
                 'BLOCKED_AUTHORITY')
    binding_need(authority.get('repo')=='yyczz1/FightMatch' and authority.get('pr')==1,'BLOCKED_AUTHORITY')
    binding_need(authority.get('active') and live.get('active'),'BLOCKED_AUTHORITY_REVOKED')
    binding_need(not authority.get('expired') and not live.get('expired'),'BLOCKED_AUTHORITY_EXPIRED')
    if authority.get('expiresUtc'):
        binding_need(datetime.datetime.now(datetime.timezone.utc)<datetime.datetime.fromisoformat(authority['expiresUtc']),
                     'BLOCKED_AUTHORITY_EXPIRED')
    binding_need(live.get('head')==authority.get('head') and live.get('currentHead')==authority.get('head'),
                 'BLOCKED_AUTHORITY_HEAD',expected=authority.get('head'),actual=live.get('currentHead'))
    binding_need(live.get('runId')==authority.get('runId') and live.get('owner')==authority.get('owner'),
                 'BLOCKED_AUTHORITY_IDENTITY')
    expected='FIX21_SOURCE_AND_POSTHOC_ONLY' if mode=='posthoc' else 'SINGLE_GRAPHICS_EXACT12'
    binding_need(authority.get('mode')==expected,'BLOCKED_AUTHORITY_MODE',expected=expected,actual=authority.get('mode'))

class BoundInputs:
    def __init__(self,adapter,entries,head):
        self.adapter=adapter;self.entries=entries;self.head=head
        self.files={};self.records={};self.excluded=[]
    def pin(self,path,raw,role,via):
        safe_relative(path)
        if path in self.files:
            binding_need(self.files[path]==raw,'BLOCKED_DUPLICATE_MAPPING',path)
            self.records[path]['roles']=sorted(set(self.records[path]['roles']+[role]))
            return raw
        self.files[path]=raw;self.records[path]=dict(path=path,**raw_identity(raw),roles=[role],via=via)
        return raw
    def tree_file(self,path,tree_path=None,role='tree input'):
        tree_path=safe_relative(tree_path or path);safe_relative(path)
        binding_need(tree_path in self.entries,'BLOCKED_DEPENDENCY_UNBOUND',tree_path)
        entry=self.entries[tree_path];raw=checked_object(self.adapter,entry['oid'],'blob');actual=self.adapter.read(path)
        binding_need(actual==raw,'BLOCKED_FILE_IDENTITY',path,raw_identity(raw),raw_identity(actual))
        return self.pin(path,raw,role,dict(head=self.head,treePath=tree_path,blobOid=entry['oid'],mode=entry['mode']))
    def digest_file(self,row,role,chain):
        path=safe_relative(row['path']);raw=self.adapter.read(path)
        binding_need(raw_identity(raw)=={k:row[k] for k in ['bytes','sha256']},'BLOCKED_FILE_IDENTITY',path,
                     {k:row[k] for k in ['bytes','sha256']},raw_identity(raw))
        return self.pin(path,raw,role,dict(digestChain=chain))
    def parsed(self,path):
        binding_need(path in self.files,'BLOCKED_DEPENDENCY_UNBOUND',path)
        return json_bytes(self.files[path],path)
    def summary(self):
        return dict(files=[self.records[p] for p in sorted(self.records)],excludedWip=self.excluded)

def bind_inputs(adapter,contract,authority,live,mode,self_paths):
    check_authority(authority,live,mode);adapter.guard_environment()
    entries,objects=read_git_tree(adapter,authority['head'],authority['tree'])
    historical=contract['historical']
    if mode=='posthoc':
        binding_need((authority['head'],authority['tree'])==(historical['head'],historical['tree']),
                     'BLOCKED_HISTORICAL_AUTHORITY')
        old_entries=entries
    else:
        old_entries,_=read_git_tree(adapter,historical['head'],historical['tree'])
    ctx=BoundInputs(adapter,entries,authority['head'])
    # Complete import closure; no path may be silently omitted by a 92-file manifest.
    imported=[p for p in entries if any(p.startswith(prefix+'/') for prefix in contract['importRoots'])]
    actual=sum([adapter.files(prefix) for prefix in contract['importRoots']],[])
    binding_need(collections.Counter(imported)==collections.Counter(actual),'BLOCKED_INPUT_SET',
                 expected=sorted(imported),actual=sorted(actual))
    adapter.prefetch([entries[p]['oid'] for p in imported])
    for p in imported:ctx.tree_file(p,role='complete Unity import closure')
    extra=[p for p in entries if any(p.startswith(prefix+'/') for prefix in contract['extraRoots'])]
    wip=contract['excludedWip'];extra_actual=sum([adapter.files(prefix) for prefix in contract['extraRoots']],[])
    binding_need(set(extra_actual)==set(extra)|set(wip) and not set(extra)&set(wip),'BLOCKED_EXTRA_INPUT_SET',
                 expected=sorted(set(extra)|set(wip)),actual=sorted(extra_actual))
    adapter.prefetch([entries[p]['oid'] for p in extra])
    for p in extra:ctx.tree_file(p,role='tracked tools/config closure')
    for proof in contract['dependencyProof']:
        raw=ctx.tree_file(proof['path'],role='dependency proof source')
        # Exact source identity must remain the analysed one: altered calls require a new scoped proof.
        binding_need(raw_identity(raw)==proof['identity'],'BLOCKED_DEPENDENCY_UNBOUND',proof['path'],
                     proof['identity'],raw_identity(raw))
    for p in contract['externalTreeInputs']:ctx.tree_file(p,role='explicit test/activation dependency')
    for saved in contract.get('predecessorFiles',[]):
        binding_need(raw_identity(ctx.files[saved['path']])=={k:saved[k] for k in ['bytes','sha256']},
                     'BLOCKED_PREDECESSOR_IDENTITY',saved['path'])
    for dependency in contract.get('externalDigestInputs',[]):
        source=ctx.files.get(dependency['anchorSource'])
        binding_need(source is not None,'BLOCKED_DEPENDENCY_UNBOUND',dependency['anchorSource'])
        source=source.decode('utf-8');path=safe_relative(dependency['path'])
        sizes=re.findall(r'internal const int ApprovedByteCount = (\d+);',source)
        hashes=re.findall(r'internal const string ApprovedSha256 = "([0-9a-f]{64})";',source)
        binding_need(len(sizes)==len(hashes)==1 and '"../'+path+'"' in source,'BLOCKED_DEPENDENCY_UNBOUND',path)
        ctx.digest_file(dict(path=path,bytes=int(sizes[0]),sha256=hashes[0]),'external test CSV',
                        [dependency['anchorSource'],'ReadApprovedDraft/ApprovedByteCount/ApprovedSha256'])
    oldctx=BoundInputs(adapter,old_entries,historical['head'])
    static_path=contract['baselineStatic']['actual']
    raw=oldctx.tree_file(static_path,contract['baselineStatic']['tree'],'historical raw static anchor')
    ready=json_bytes(raw,static_path)
    # No parsing of S or any old helper before a raw-tree or chained-digest check.
    for row in ready['peerEvidence']:
        oldctx.digest_file(row,'historical peer',static_path)
    oldctx.tree_file(contract['baselineRunner']['source'],contract['baselineRunner']['tree'],'historical runner source')
    oldctx.tree_file(contract['baselineRunner']['actual'],contract['baselineRunner']['tree'],'historical archive-to-runner mapping')
    delta=oldctx.parsed(contract['deltaPath'])
    for row in delta['fixedInputs']:oldctx.digest_file(row,'fixed historical dependency',contract['deltaPath'])
    for saved in delta['oldEvidenceBefore']:
        prefix=str(pathlib.PurePosixPath(saved['root']).relative_to(pathlib.PurePosixPath(contract['repository'])))
        safe_relative(prefix)
        rows=[];raw_files={}
        for path in adapter.files(prefix):
            # Match the sealed legacy manifest algorithm exactly; AppleDouble is not a consumed input.
            if pathlib.PurePosixPath(path).name.startswith('._'):continue
            data=adapter.read(path);raw_files[path]=data;rows.append(dict(path=path,**raw_identity(data)))
        digest=hashlib.sha256(json.dumps(rows,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()).hexdigest()
        actual_summary=dict(fileCount=len(rows),totalBytes=sum(r['bytes'] for r in rows),manifestSha256=digest)
        binding_need(actual_summary=={k:saved[k] for k in actual_summary},'BLOCKED_DEPENDENCY_UNBOUND',prefix,
                     {k:saved[k] for k in actual_summary},actual_summary)
        for row in rows:oldctx.pin(row['path'],raw_files[row['path']],'historical directory member',dict(digestChain=[contract['deltaPath'],prefix,digest]))
    for path in contract['requiredHistoricalInputs']:
        binding_need(path in oldctx.files,'BLOCKED_DEPENDENCY_UNBOUND',path)
    # The authority, not a locally edited manifest, fixes historical receipt identity.
    receipt_path=contract['receiptPath']
    if mode=='posthoc':
        binding_need(authority['receipt']==contract['receiptIdentity'],'BLOCKED_RECEIPT_AUTHORITY')
    oldctx.digest_file(dict(path=receipt_path,**contract['receiptIdentity']),'historical receipt','central FIX21 activation and approved packet')
    receipt=oldctx.parsed(receipt_path)
    for row in receipt['evidence']:oldctx.digest_file(row,'historical run evidence',receipt_path)
    for p,data in oldctx.files.items():
        if p in ctx.files:binding_need(ctx.files[p]==data,'BLOCKED_DEPENDENCY_UNBOUND',p)
        else:ctx.files[p]=data;ctx.records[p]=oldctx.records[p]
    before=ctx.parsed(contract['historicalRoot']+'/before.json')
    after=ctx.parsed(contract['historicalRoot']+'/after.json')
    binding_need(before['protectedClosedSet']==after['protectedClosedSet'],'BLOCKED_HISTORICAL_DRIFT','protectedClosedSet')
    protected={row['path']:row for row in before['protectedClosedSet']}
    unbound=set(protected)-set(old_entries)
    binding_need(unbound==set(wip),'BLOCKED_DEPENDENCY_UNBOUND',expected=sorted(wip),actual=sorted(unbound))
    for p,row in protected.items():
        if p in wip:
            ctx.excluded.append(dict(path=p,historicalBefore=row,historicalAfter=row,current=raw_identity(adapter.read(p)),
                                     reason=wip[p]))
        else:
            # Verify recorded original bytes against historical tree, not current receipt booleans.
            obj=checked_object(adapter,old_entries[p]['oid'],'blob')
            binding_need(raw_identity(obj)=={k:row[k] for k in ['bytes','sha256']},'BLOCKED_HISTORICAL_BLOB',p,
                         raw_identity(obj),row)
    if mode!='posthoc':
        ctx.tree_file(contract['futurePacket'],role='current execution packet')
        binding_need(len(set(self_paths.values()))==len(self_paths) and len(set(contract['selfMappings'].values()))==len(contract['selfMappings']),'BLOCKED_DUPLICATE_MAPPING')
        binding_need(set(self_paths)==set(contract['selfMappings']),'BLOCKED_SELF_MAPPING')
        for role,p in self_paths.items():
            ctx.tree_file(p,contract['selfMappings'][role],role='new runner self binding')
        activation=authority['activation'];activation_path=authority['activationPath']
        raw=adapter.read(activation_path)
        binding_need(json_bytes(raw,activation_path)==activation,'BLOCKED_ACTIVATION',activation_path)
        binding_need(activation['prBinding']['head']==authority['head'] and activation['prBinding']['tree']==authority['tree']
                     and activation['owner']==authority['owner'] and activation['runId']==authority['runId'],
                     'BLOCKED_ACTIVATION',activation_path)
        ctx.pin(activation_path,raw,'external activation','central out-of-band authority')
    return ctx,dict(objects=objects,importedCount=len(imported),trackedExtraCount=len(extra),
                    historicalBoundProtectedCount=len(protected)-len(wip),historicalProtectedCount=len(protected),
                    excludedCount=len(wip),mode=mode)

def verify_posthoc(ctx,contract):
    e=contract['historicalRoot'];receipt=ctx.parsed(contract['receiptPath'])
    run=ctx.parsed(e+'/run.json');before=ctx.parsed(e+'/before.json');after=ctx.parsed(e+'/after.json')
    def xml(path):
        root=ET.fromstring(ctx.files[path]);cases=list(root.iter('test-case'))
        return root,[dict(fullname=t.get('fullname'),classname=t.get('classname'),result=t.get('result')) for t in cases]
    root,cases=xml(e+'/tests.xml');oldroot,old=xml(contract['oldCoreXml'])
    binding_need(root.get('result')=='Passed' and root.get('total')=='12' and root.get('passed')=='12' and all(int(root.get(k,'0'))==0 for k in ['failed','skipped','inconclusive','errors']),'BLOCKED_XML_RESULTS')
    selected=sorted(t['fullname'] for t in old if t['classname'] in contract['classnames'])
    binding_need(all(sum(t['classname']==c for t in old)==6 for c in contract['classnames']),'BLOCKED_COVERAGE_COUNTS')
    remainder=[t for t in old if t['fullname'] not in selected]
    _,host=xml(contract['hostXml']);_,exact=xml(contract['exact2Xml'])
    expected=ctx.parsed(contract['expectedPath'])
    binding_need(len(selected)==12 and len(remainder)==178 and len(host)==30 and len(exact)==2,
                 'BLOCKED_COVERAGE_COUNTS')
    binding_need(collections.Counter(t['fullname'] for t in cases)==collections.Counter(selected),'BLOCKED_EXACT12_COUNTER')
    allcases=remainder+cases+host+exact
    binding_need(len(allcases)==222 and all(t['result']=='Passed' for t in allcases),'BLOCKED_XML_RESULTS')
    binding_need(collections.Counter(t['fullname'] for t in allcases)==collections.Counter(expected['expected222']),
                 'BLOCKED_222_COUNTER')
    selector='^(?:'+'|'.join(re.escape(n) for n in selected)+')$'
    binding_need(ctx.files[contract['selectorPath']]==selector.encode() and ctx.files[e+'/test-filter.txt']==selector.encode(),
                 'BLOCKED_SELECTOR')
    event_data=ctx.files[e+'/test-events.jsonl']
    binding_need(event_data.endswith(b'\n'),'BLOCKED_EVENTS')
    events=[json_bytes(line,e+'/test-events.jsonl') for line in event_data.splitlines()]
    binding_need([v['seq'] for v in events]==list(range(1,len(events)+1)) and len(events)==26,'BLOCKED_EVENTS')
    binding_need(events[0]['eventName']=='RunStarted' and events[-1]['eventName']=='RunFinished'
                 and events[-1]['result']=='Passed','BLOCKED_EVENTS')
    opened=collections.Counter();finished=[];started=[]
    for v in events:
        binding_need(v['pid']==run['editorPid'],'BLOCKED_EVENTS')
        if v['eventName']=='TestStarted':opened[v['fullName']]+=1;started.append(v['fullName'])
        elif v['eventName']=='TestFinished':
            binding_need(opened[v['fullName']]>0 and v['result']=='Passed','BLOCKED_EVENTS')
            opened[v['fullName']]-=1;finished.append(v['fullName'])
    binding_need(not +opened and collections.Counter(started)==collections.Counter(selected)==collections.Counter(finished),
                 'BLOCKED_EVENTS')
    quiescence=ctx.parsed(e+'/quiescence.json')
    binding_need(run['exitCode']==0 and not run['timedOut'] and run['launchToExitObservationSeconds']<=360 and
                 quiescence['rootExitCode']==0 and not quiescence['finalObservation']['ownedProcesses'] and
                 not quiescence['finalObservation']['unityProcesses'],'BLOCKED_EXIT_OR_CLEANUP')
    for key in ['source','products','resources','delta','protectedClosedSet','oldEvidence','gitFiles','font']:
        binding_need(before[key]==after[key],'BLOCKED_HISTORICAL_DRIFT',key)
    binding_need(receipt['owner']==run['owner'],'BLOCKED_HISTORICAL_OWNER')
    external={}
    for info in quiescence.get('ownedIdentities',{}).values():
        for saved in info.get('tools',{}).get('files',[]):
            path=saved['path']
            binding_need(path.startswith(str(pathlib.Path(EDITOR).parents[1])+'/'),'BLOCKED_EXTERNAL_TOOL',path)
            if path not in external:
                actual=raw_identity(ctx.adapter.read_absolute(pathlib.Path(path)))
                binding_need(actual=={k:saved[k] for k in ['bytes','sha256']},'BLOCKED_EXTERNAL_TOOL',path,saved,actual)
                external[path]=dict(path=path,**actual,chain=e+'/quiescence.json -> verified FIX20 receipt')
    return dict(exact12=12,retained178=178,host30=30,exact2=2,union222=222,oldCoreFailed=sum(t['result']=='Failed' for t in old),
                exitCode=run['exitCode'],launchToExitObservationSeconds=run['launchToExitObservationSeconds'],
                originalOwner=run['owner'],events=len(events),ownedClear=True,externalTools=list(external.values()))

def binding_entry(adapter, contract, authority, authority_check, mode, self_paths, launcher=None, hook=None):
    """Production entry shared by fixture and CLI; fake fixtures replace only I/O and launch."""
    check=lambda:bind_inputs(adapter,contract,authority,authority_check(),mode,self_paths)
    first,proof=check()
    if mode=='posthoc':
        facts=verify_posthoc(first,contract)
        return dict(status='POSTHOC_BINDING_SUPPLEMENT',bindingEstablished=True,proof=proof,historicalFacts=facts,
                    inputs=first.summary(),newUnityLaunches=0,
                    limitation='The original runner had no real prelaunch Git binding. This is a posthoc association of sealed evidence with independent Git objects, not new evidence from the original time or a new Unity run.')
    binding_need(launcher is not None,'BLOCKED_TRUSTED_LAUNCHER_REQUIRED')
    if hook:hook('before-spawn')
    latest,latest_proof=check()
    binding_need(first.records==latest.records,'BLOCKED_PRESPAWN_DRIFT')
    result=launcher(latest)
    if hook:hook('after-run')
    try:
        after,_=check()
        binding_need(latest.records==after.records,'BLOCKED_POSTRUN_DRIFT')
    except BindingFailure as error:
        return dict(status='BLOCKED_POSTRUN_BINDING',bindingEstablished=False,runFacts=result,mismatch=error.detail)
    return dict(status='BOUND_RUN_COMPLETED',bindingEstablished=True,runFacts=result,proof=latest_proof)

def bound_bytes(path):
    relative=str(pathlib.Path(path).relative_to(ROOT))
    binding_need(BINDING_STATE is not None and relative in BINDING_STATE.files,'BLOCKED_DEPENDENCY_UNBOUND',relative)
    return BINDING_STATE.files[relative]

def bound_text(path):
    return bound_bytes(path).decode('utf-8')

def binding_checkpoint():
    global BINDING_STATE
    fresh,_=bind_inputs(BINDING_ADAPTER,BINDING_CONTRACT,BINDING_AUTHORITY,BINDING_CHECK_AUTHORITY(),
                        'execute',BINDING_AUTHORITY['selfPaths'])
    binding_need(fresh.records==BINDING_STATE.records,'BLOCKED_PRESPAWN_DRIFT')
    binding_need(fresh.excluded==BINDING_STATE.excluded,'BLOCKED_WIP_ACTIVATION_SNAPSHOT')
    BINDING_STATE=fresh

def binding_main(argv=None, *, adapter=None, authority=None, authority_check=None, launcher=None, self_paths=None, hook=None):
    parser=argparse.ArgumentParser()
    parser.add_argument('--posthoc-fix20',action='store_true')
    parser.add_argument('--execute',action='store_true')
    parser.add_argument('--authority-json')
    args=parser.parse_args(argv)
    binding_need(args.posthoc_fix20 != args.execute,'BLOCKED_MODE')
    # A CLI JSON is allowed only for this explicit read-only task. Execution must be
    # invoked by a trusted external launcher with a live authority-check callback.
    if authority is None:
        binding_need(args.posthoc_fix20 and args.authority_json,'BLOCKED_TRUSTED_AUTHORITY_REQUIRED')
        authority=json_bytes(args.authority_json.encode(),'external command-line authority')
    if args.execute:
        binding_need(authority_check is not None and launcher is not None,'BLOCKED_TRUSTED_LAUNCHER_REQUIRED')
    check=authority_check or (lambda:dict(authority))
    adapter=adapter or GitDiskAdapter(ROOT)
    adapter.guard_environment()
    contract_path='TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source/binding-contract.json'
    # For execution, validate the contract's raw Git blob before parsing its rules.
    if args.execute:
        check_authority(authority,check(),'execute')
        tree,_=read_git_tree(adapter,authority['head'],authority['tree'])
        tp='docs/versioning/2026-10-01-ugui-pr/fix22/binding-contract.json'
        binding_need(tp in tree,'BLOCKED_DEPENDENCY_UNBOUND',tp)
        blob=checked_object(adapter,tree[tp]['oid'],'blob')
        binding_need(adapter.read(contract_path)==blob,'BLOCKED_FILE_IDENTITY',contract_path)
        contract=json_bytes(blob,contract_path)
    else:
        # This task's contract is source under development; the historical trust root
        # is the independently supplied head/tree/receipt, then raw H20 static bytes.
        contract=json_bytes(adapter.read(contract_path),contract_path)
    result=binding_entry(adapter,contract,authority,check,'posthoc' if args.posthoc_fix20 else 'execute',
                         self_paths or {},launcher,hook)
    return result

def sha(data):
    return hashlib.sha256(data).hexdigest()

def identity(path):
    path = pathlib.Path(path)
    data = path.read_bytes()
    return dict(path=str(path.relative_to(ROOT)), bytes=len(data), sha256=sha(data))

def now():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()

def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n')

def resources():
    return [identity(ROOT / x['path']) for x in pre['resourcesBefore']]

def font_structure():
    text = bound_text(FONT)
    fields = {}
    names = ['m_AtlasPopulationMode', 'm_SourceFontFileGUID', 'm_SourceFontFile_EditorRef',
             'm_SourceFontFile', 'm_AtlasWidth', 'm_AtlasHeight', 'm_AtlasPadding', 'm_AtlasRenderMode',
             'm_IsMultiAtlasTexturesEnabled', 'm_ClearDynamicDataOnBuild']
    for key in names:
        match = re.search(r'^  ' + key + r': (.*)$', text, re.M)
        fields[key] = match.group(1) if match else None
    glyph_text = text.split('  m_GlyphTable:\n', 1)[1].split('  m_CharacterTable:\n', 1)[0]
    glyphs = {m.group(1): m.group(0) for m in re.finditer(r'(?ms)^  - m_Index: (\d+)\n.*?(?=^  - m_Index:|\Z)', glyph_text)}
    characters = dict(re.findall(r'm_Unicode: (\d+)\n    m_GlyphIndex: (\d+)', text))
    return dict(font=identity(FONT), fields=fields, characterCount=len(characters), glyphCount=len(glyphs),
                characters=characters, glyphs=glyphs,
                glyphsPerAtlas=dict(collections.Counter(re.findall(r'^    m_AtlasIndex: (\d+)$', glyph_text, re.M))),
                textureCount=len(re.findall(r'^--- !u!28 ', text, re.M)),
                materialCount=len(re.findall(r'^--- !u!21 ', text, re.M)),
                atlasTextureIndex=int(re.search(r'^  m_AtlasTextureIndex: (\d+)$', text, re.M).group(1)),
                externalGuids=sorted(set(re.findall(r'guid: ([0-9a-f]{32})', text))))

def processes():
    result = subprocess.run(['/bin/ps', '-axo', 'pid=,ppid=,comm='], capture_output=True, text=True, check=True)
    rows = []
    for line in result.stdout.splitlines():
        parts = line.strip().split(None, 2)
        if len(parts) == 3:
            rows.append(dict(pid=int(parts[0]), ppid=int(parts[1]), comm=parts[2]))
    return rows

def unity_rows(rows):
    result=[]
    editor_contents=str(pathlib.Path(EDITOR).parents[1])+'/'
    names={'Unity','Unity Hub','UnityShaderCompiler','UnityAutoQuitter','UnityCrashHandler',
           'bee_backend','UnityPackageManager','Unity.Licensing.Client'}
    for row in rows:
        name=pathlib.Path(row['comm']).name
        if name in names or row['comm'].startswith(editor_contents) or 'unity-mcp' in row['comm'].lower() or 'Unity Hub.app' in row['comm']:
            result.append(row)
        elif name=='dotnet' and editor_contents in current_identity(row['pid'])['command']:
            result.append(row)
    return result

def no_unity():
    assert not unity_rows(processes()), 'Existing Unity family process'

def root_identity(pid):
    birth=subprocess.run(['/bin/ps','-p',str(pid),'-o','lstart='],capture_output=True,text=True)
    command=subprocess.run(['/bin/ps','-p',str(pid),'-o','command='],capture_output=True,text=True)
    comm=subprocess.run(['/bin/ps','-p',str(pid),'-o','comm='],capture_output=True,text=True)
    return dict(pid=pid,birth=birth.stdout.strip(),command=command.stdout.strip(),comm=comm.stdout.strip(),queryExitCodes=[birth.returncode,command.returncode,comm.returncode],observedUtc=now())

def create_json(path, data):
    with path.open('x', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write('\n')

def canonical(data):
    return sha(json.dumps(data, sort_keys=True, separators=(',', ':'), ensure_ascii=False).encode())

def jsonl(path, data):
    with path.open('a', encoding='utf-8') as f:
        f.write(json.dumps(data, ensure_ascii=False, separators=(',', ':')) + '\n')
        f.flush()

def assert_non_link(path):
    for part in [path] + list(path.parents):
        assert part.exists() and not part.is_symlink(), 'Missing path or symlink: ' + str(part)

def frozen_now(items):
    return [identity(ROOT / row['path']) for row in items]

def tree_manifest(directory):
    return [identity(p) for p in sorted(directory.rglob('*')) if p.is_file() and not p.name.startswith('._')]

def old_tree_snapshots():
    paths = sorted(set([x['root'] for x in pre['oldRoots']] + [str(R17), str(R15), str(R18), str(R19), str(ROOT/'TestArtifacts/FightMatch/UGUI-01/q4-correction-19-source'), str(ROOT/'TestArtifacts/FightMatch/UGUI-01/q4-correction-18-source'), str(SOURCE_ONLY)]))
    return [dict(root=p, files=tree_manifest(pathlib.Path(p))) for p in paths]

def compact_trees(trees):
    return [dict(root=t['root'], fileCount=len(t['files']),
                 totalBytes=sum(x['bytes'] for x in t['files']), manifestSha256=canonical(t['files'])) for t in trees]

def protected_files():
    return [identity(p) for d in ['Assets', 'Packages', 'ProjectSettings', 'Config', 'Generated', 'Tools']
            for p in sorted((ROOT / d).rglob('*')) if p.is_file() and not p.name.startswith('._')]

def xml_result(path, expected):
    result = dict(exists=path.exists(), expectedCount=len(expected), actualFullnames=[], cases=[], passed=False)
    if not result['exists']:
        return result
    result['identity'] = identity(path)
    if path.stat().st_size > MAX_LOG:
        result['error'] = 'XML exceeds byte limit'
        return result
    try:
        xml = ET.fromstring(bound_bytes(path)) if path != OUT/'tests.xml' else ET.parse(path).getroot()
        tests = list(xml.iter('test-case'))
        result.update(statistics=xml.attrib, cases=[
            dict(fullname=t.attrib.get('fullname'), result=t.attrib.get('result'),
                 output=t.findtext('output') or '', message=t.findtext('failure/message'),
                 stack=t.findtext('failure/stack-trace')) for t in tests])
        result['actualFullnames'] = [t['fullname'] for t in result['cases']]
        result['counterExact'] = collections.Counter(result['actualFullnames']) == collections.Counter(expected)
        result['zeroNonPass'] = all(t['result'] == 'Passed' for t in result['cases'])
        result['statisticsExact'] = (int(xml.attrib.get('total', '-1')) == len(expected) and
            int(xml.attrib.get('passed', '-1')) == len(expected) and
            all(int(xml.attrib.get(k, '0')) == 0 for k in ['failed', 'skipped', 'inconclusive', 'errors']) and
            xml.attrib.get('result') == 'Passed')
        result['passed'] = result['counterExact'] and result['zeroNonPass'] and result['statisticsExact']
    except Exception as exc:
        result['error'] = type(exc).__name__ + ': ' + str(exc)
    return result

def current_identity(pid):
    result = root_identity(pid)
    # -ww prevents long exact12 selectors from being truncated by ps.
    command = subprocess.run(['/bin/ps', '-ww', '-p', str(pid), '-o', 'command='],
                             capture_output=True, text=True)
    result['command'] = command.stdout.strip()
    result['queryExitCodes'][1] = command.returncode
    return result


def supervise_root(poll, compile_error, observe, wait_step, elapsed, budget):
    """Return the first terminal fact; callers never replace a failure with cleanup results."""
    while True:
        error = compile_error()
        exit_code = poll()
        observed_at = elapsed()
        if observed_at > budget or (observed_at >= budget and exit_code is None):
            return dict(kind='TIMEOUT', elapsedSeconds=observed_at, exitCode=exit_code)
        if error:
            return dict(kind='COMPILE_FAILURE', elapsedSeconds=observed_at, exitCode=exit_code, compilerError=error)
        if exit_code is not None:
            return dict(kind='ROOT_EXIT_ZERO' if exit_code == 0 else 'ROOT_EXIT_NONZERO',
                        elapsedSeconds=observed_at, exitCode=exit_code)
        observe()
        wait_step(min(1, max(0, budget-elapsed())))

def settle_owned(reason, root_pid, snapshot, verify, gentle, terminate, wait_step, elapsed):
    """At most 60 seconds of gentle/natural cleanup, then guarded SIGTERM and 30 seconds."""
    began = elapsed()
    gentle_attempts = set()
    actions = []
    signals = []
    final = {}
    def finish(status):
        return dict(status=status, reason=reason, startedElapsedSeconds=began,
                    durationSeconds=elapsed()-began, gentleLimitSeconds=60, terminationLimitSeconds=30,
                    actions=actions, signalsSent=signals, finalObservation=final)
    while True:
        final = snapshot()
        if final.get('unownedUnityProcesses'):
            return finish('BLOCKED_UNOWNED_PROCESS')
        remaining = final['ownedProcesses']
        if not remaining:
            return finish('COMPLETE')
        if elapsed() >= began + 60:
            break
        # Re-evaluate on each sample: transient children can vanish after root exit.
        if reason != 'TIMEOUT' and len(remaining) == 1 and remaining[0].get('compilerServer'):
            row = remaining[0]
            if row['pid'] not in gentle_attempts:
                proof = verify(row)
                if proof.get('gone'):
                    continue
                if not proof['allowed']:
                    actions.append(dict(action='identity-rejected', pid=row['pid'], proof=proof))
                    return finish('BLOCKED_PROCESS_IDENTITY')
                gentle_attempts.add(row['pid'])
                result=gentle(row, proof)
                actions.append(dict(action='gentle', pid=row['pid'], result=result))
                if result.get('status')=='IDENTITY_NO_LONGER_MATCHES':
                    return finish('BLOCKED_PROCESS_IDENTITY')
        wait_step(min(1, max(0, began + 60-elapsed())))
    candidates = [r for r in final['ownedProcesses'] if reason != 'TIMEOUT' or r['pid'] == root_pid]
    proofs = []
    for row in candidates:
        if elapsed() >= began + 90:
            return finish('BLOCKED_MANUAL_PROCESS_CLEANUP')
        proof = verify(row)
        if proof.get('gone'):
            continue
        if not proof['allowed']:
            actions.append(dict(action='identity-rejected', pid=row['pid'], proof=proof))
            return finish('BLOCKED_PROCESS_IDENTITY')
        proofs.append((row, proof))
    # Verify the whole candidate set before sending any signal; terminate also rechecks immediately.
    for row, proof in proofs:
        if elapsed() >= began + 90:
            return finish('BLOCKED_MANUAL_PROCESS_CLEANUP')
        result = terminate(row, proof)
        actions.append(dict(action='SIGTERM', pid=row['pid'], result=result))
        if result.get('sent'):
            signals.append(result)
        elif not result.get('gone'):
            return finish('BLOCKED_PROCESS_IDENTITY')
    while elapsed() < began + 90:
        final = snapshot()
        if final.get('unownedUnityProcesses'):
            return finish('BLOCKED_UNOWNED_PROCESS')
        if not final['ownedProcesses']:
            return finish('COMPLETE')
        wait_step(min(1, max(0, began + 90-elapsed())))
    final = snapshot()
    return finish('COMPLETE' if not final['ownedProcesses'] and not final.get('unownedUnityProcesses')
                  else 'BLOCKED_MANUAL_PROCESS_CLEANUP')


def owned_tool_identity(info):
    command = shlex.split(info['command'])
    executable = pathlib.Path(info['comm'])
    if info['comm'] == 'dotnet':
        executable = pathlib.Path(EDITOR).parents[1] / 'NetCoreRuntime/dotnet'
    executable.relative_to(pathlib.Path(EDITOR).parents[1])
    assert executable.is_file(), 'Owned executable is missing'
    loaded = subprocess.run(['/usr/sbin/lsof','-a','-p',str(info['pid']),'-d','txt','-Fn'],
                            capture_output=True,text=True,timeout=5)
    assert loaded.returncode == 0 and 'n'+str(executable) in loaded.stdout.splitlines(), 'Executable identity unavailable'
    files = [executable]
    compiler = pathlib.Path(EDITOR).parents[1] / 'DotNetSdkRoslyn/VBCSCompiler.dll'
    is_compiler = len(command) >= 3 and command[1:3] == ['exec',str(compiler)]
    if is_compiler:
        files.append(compiler)
    identities = [dict(path=str(p),bytes=p.stat().st_size,sha256=sha(p.read_bytes())) for p in files]
    if is_compiler:
        expected = ast.literal_eval(re.search(r"tools_match = .* == (\[.*\])",bound_text(R17_RUNNER)).group(1))
        assert [(e['bytes'],e['sha256']) for e in identities] == expected, 'Compiler tool identity drift'
    return dict(files=identities,compilerServer=is_compiler and len(command)==4 and
                re.fullmatch(r'-pipename:[A-Za-z0-9_-]+',command[3]) is not None)

def verify_owned(row, saved, ancestry, root_pid):
    current = current_identity(row['pid'])
    if current['queryExitCodes'] == [1,1,1]:
        return dict(allowed=False,gone=True,current=current)
    proof = dict(allowed=False,gone=False,current=current,checks={})
    try:
        tools = owned_tool_identity(current)
        checks = dict(pid=current['pid']==saved['pid'] and current['pid']>0,
                      birth=bool(current['birth']) and current['birth']==saved['birth'],
                      command=current['command']==saved['command'],comm=current['comm']==saved['comm'],
                      queries=current['queryExitCodes']==saved['queryExitCodes']==[0,0,0],
                      tools=tools==saved.get('tools'),
                      ancestry=bool(ancestry) and ancestry['chain'][0]==row['pid'] and
                      ((ancestry['origin']=='Unity' and ancestry['chain'][-1]==root_pid) or
                       (ancestry['origin']=='cleanup-client' and ancestry['chain'][-1]==os.getpid())))
        final_identity=current_identity(row['pid'])
        checks['identityStableThroughToolCheck']=all(final_identity[k]==current[k] for k in ['pid','birth','command','comm','queryExitCodes'])
        proof.update(checks=checks,tools=tools,finalIdentity=final_identity,allowed=all(checks.values()))
    except Exception as exc:
        proof['error']=type(exc).__name__+': '+str(exc)
    return proof

def event_trace(pid, final=False):
    path = OUT / 'test-events.jsonl'
    result = dict(exists=path.exists(), rows=0, errors=[], runStarted=0, runFinished=0,
                  startedCount=0, finishedCount=0, domains=[], finished=[], lastFinished=None,
                  unmatchedStarted=[], unresolvedFullnames=[], repeatedStarts={}, partialTail=False)
    if not path.exists():
        result['errors'].append('missing progress file')
        return result
    if path.stat().st_size > 4 * 1024 * 1024:
        result['errors'].append('progress file exceeds 4 MiB')
        return result
    raw = path.read_bytes()
    lines = raw.splitlines(keepends=True)
    if len(lines) > 4096:
        result['errors'].append('progress file exceeds 4096 rows')
        return result
    starts = collections.defaultdict(list)
    start_counts = collections.Counter()
    last_start = {}
    last_finish = {}
    process_start = None
    domains = set()
    fields = {'seq', 'utc', 'pid', 'processStartUtc', 'domain', 'source', 'eventName',
              'testId', 'fullName', 'result', 'duration'}
    for i, line in enumerate(lines):
        if not line.endswith(b'\n'):
            result['partialTail'] = True
            if final:
                result['errors'].append('partial final row')
            break
        try:
            assert len(line) <= 16 * 1024, 'row exceeds 16 KiB'
            event = json.loads(line.decode('utf-8'))
            assert set(event) == fields, 'schema mismatch'
            assert type(event['seq']) is int and event['seq'] == i + 1, 'sequence gap'
            assert type(event['pid']) is int and event['pid'] == pid, 'foreign PID'
            assert re.fullmatch('[0-9a-f]{32}', event['domain']), 'invalid domain token'
            stamp = datetime.datetime.fromisoformat(event['utc'].replace('Z', '+00:00'))
            assert stamp.utcoffset() == datetime.timedelta(0), 'non-UTC timestamp'
            assert event['testId'] and event['fullName'], 'missing test identity'
            assert isinstance(event['duration'], (int, float)) and 0 <= event['duration'] < float('inf'), 'duration'
            if process_start is None:
                process_start = event['processStartUtc']
                datetime.datetime.fromisoformat(process_start.replace('Z', '+00:00'))
            assert event['processStartUtc'] == process_start, 'process start identity changed'
            kind = event['eventName']
            assert kind in ['RunStarted', 'RunFinished', 'TestStarted', 'TestFinished'], 'unknown event'
            assert event['source'] == ('editor' if kind.startswith('Run') else 'attribute'), 'source mismatch'
            assert not result['runFinished'], 'events after RunFinished'
            if i == 0:
                assert kind == 'RunStarted', 'missing initial RunStarted'
            if kind.endswith('Started'):
                assert event['result'] == '' and event['duration'] == 0, 'invalid start result'
            else:
                assert event['result'], 'missing result'
            domains.add(event['domain'])
            result['rows'] += 1
            if kind == 'RunStarted':
                result['runStarted'] += 1
            elif kind == 'RunFinished':
                result['runFinished'] += 1
                result['runResult'] = event['result']
            elif kind == 'TestStarted':
                result['startedCount'] += 1
                start_counts[event['fullName']] += 1
                starts[event['fullName']].append(event)
                last_start[event['fullName']] = event
            else:
                result['finishedCount'] += 1
                result['finished'].append(event)
                result['lastFinished'] = event
                last_finish[event['fullName']] = event
                assert starts[event['fullName']], 'finish without a preceding start'
                starts[event['fullName']].pop()
        except Exception as exc:
            result['errors'].append(dict(row=i + 1, error=type(exc).__name__ + ': ' + str(exc)))
            break
    result['domains'] = sorted(domains)
    result['processStartUtc'] = process_start
    result['repeatedStarts'] = {k: v for k, v in start_counts.items() if v > 1}
    # Raw repeated starts remain in the JSONL and unmatched list; no event is deduplicated.
    result['unmatchedStarted'] = sorted([e for values in starts.values() for e in values], key=lambda e: e['seq'])
    result['unresolvedFullnames'] = sorted(k for k, e in last_start.items()
                                         if last_finish.get(k, {}).get('seq', 0) < e['seq'])
    if final and not result['rows']:
        result['errors'].append('no progress events')
    return result


# FIX22 has exactly one execution destination. These are not caller-selectable prefixes.
FIX22_OUTPUT = 'TestArtifacts/FightMatch/UGUI-01/q4-correction-22'
FIX22_PROGRESS = FIX22_OUTPUT + '/test-events.jsonl'
CALLBACK_PATH = 'Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs'
FIX22_DELTA = 'TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source/execution-delta.json'

def expected_fix22_callback(old):
    replacements = [
        (b'        private const string Argument = "-fightMatchTestProgress";',
         b'        private const string Argument = "-fightMatchTestProgress";\n        private const string RelativeProgressPath = "'+FIX22_PROGRESS.encode()+b'";'),
        (b'                string expectedPath = Path.Combine(projectRoot,\n                    "TestArtifacts/FightMatch/UGUI-01/q4-correction-20/test-events.jsonl");',
         b'                string expectedPath = Path.Combine(projectRoot, RelativeProgressPath);'),
        (b'string.Equals(Path.GetFullPath(arguments[index + 1]), expectedPath,',
         b'string.Equals(arguments[index + 1], expectedPath,')]
    result=old
    for before,after in replacements:
        binding_need(result.count(before)==1,'BLOCKED_CALLBACK_PREIMAGE',CALLBACK_PATH)
        result=result.replace(before,after)
    return result.replace(b'FIX20',b'FIX22')

def derive_execution_candidate(ctx, contract, authority):
    old=ctx.parsed(contract['deltaPath'])
    delta=ctx.parsed(FIX22_DELTA)
    binding_need(isinstance(delta.get('changes'),list) and len(delta['changes'])==1 and
                 set(delta['changes'][0])=={'path','before','after'} and
                 delta['changes'][0]['path']==CALLBACK_PATH,'BLOCKED_EXECUTION_DELTA_SCOPE',FIX22_DELTA)
    binding_need(delta.get('historical')==contract['historical'],'BLOCKED_DELTA_HISTORY')
    change=delta['changes'][0]
    historical_entries,_=read_git_tree(ctx.adapter,contract['historical']['head'],contract['historical']['tree'])
    binding_need(CALLBACK_PATH in historical_entries,'BLOCKED_DEPENDENCY_UNBOUND',CALLBACK_PATH)
    original=checked_object(ctx.adapter,historical_entries[CALLBACK_PATH]['oid'],'blob')
    binding_need(change['before']==dict(path=CALLBACK_PATH,**raw_identity(original)),
                 'BLOCKED_CALLBACK_PREIMAGE',CALLBACK_PATH)
    actual=ctx.files.get(CALLBACK_PATH)
    binding_need(actual is not None and actual==expected_fix22_callback(original),
                 'BLOCKED_CALLBACK_DELTA',CALLBACK_PATH)
    new_row=dict(path=CALLBACK_PATH,**raw_identity(actual))
    binding_need(change['after']==new_row,'BLOCKED_CALLBACK_NEW_IDENTITY',CALLBACK_PATH,change['after'],new_row)
    binding_need(delta['oldProgressPath']=='TestArtifacts/FightMatch/UGUI-01/q4-correction-20/test-events.jsonl'
                 and delta['newProgressPath']==FIX22_PROGRESS,'BLOCKED_PROGRESS_CONTRACT')
    derived=json.loads(json.dumps(old))
    for field in ['addedFiles','protectedCandidate']:
        rows=derived[field];matches=[i for i,row in enumerate(rows) if row['path']==CALLBACK_PATH]
        binding_need(len(matches)==1 and rows[matches[0]]==change['before'],'BLOCKED_CANDIDATE_PREIMAGE',field)
        rows[matches[0]]=new_row
    # Preserve old SOURCE_READY sets; only the diagnostic callback is new.
    binding_need(len(derived['addedFiles'])==2 and derived['addedFiles'][1]['path']==CALLBACK_PATH+'.meta',
                 'BLOCKED_CANDIDATE_SET','addedFiles')
    original_diag=sorted(old['candidateSources']+old['originalResources37']+old['addedFiles'],key=lambda r:r['path'])
    binding_need(len(original_diag)==92 and canonical(original_diag)==old['diagnosticCandidate']['sha256'],
                 'BLOCKED_CANDIDATE_PREIMAGE','diagnosticCandidate')
    diagnostic=sorted(derived['candidateSources']+derived['originalResources37']+derived['addedFiles'],key=lambda r:r['path'])
    derived['diagnosticCandidate']=dict(count=92,sha256=canonical(diagnostic))
    for key in ['addedFiles','protectedCandidate','diagnosticCandidate']:
        binding_need(delta.get('derivedCandidate',{}).get(key)==derived[key],
                     'BLOCKED_DERIVED_CANDIDATE',key)
    # The eight unrelated LOC files are preservation snapshots, never execution inputs.
    wip=authority.get('wipSnapshot')
    current=[dict(path=row['path'],**row['current']) for row in ctx.excluded]
    binding_need(wip==current and {r['path'] for r in current}==set(contract['excludedWip']),
                 'BLOCKED_WIP_ACTIVATION_SNAPSHOT')
    by_path={row['path']:row for row in current}
    derived['executionProtectedSnapshot']=[by_path.get(row['path'],row) for row in derived['protectedCandidate']]
    derived['executionDelta']=delta
    return derived

def validate_execution_candidate(ctx, candidate, contract, authority):
    # This is the actual preflight gate, also exercised by the offline spawn-path fixtures.
    correct=derive_execution_candidate(ctx,contract,authority)
    for key in ['addedFiles','protectedCandidate','diagnosticCandidate','candidateSources','originalProduct29','originalResources37','executionProtectedSnapshot']:
        binding_need(candidate[key]==correct[key],'BLOCKED_DERIVED_CANDIDATE',key)
    for row in candidate['protectedCandidate']:
        if row['path'] in contract['excludedWip']:continue
        current=ctx.files.get(row['path'])
        binding_need(current is not None and raw_identity(current)=={k:row[k] for k in ['bytes','sha256']},
                     'BLOCKED_CURRENT_CANDIDATE',row['path'])
    binding_need(frozen_now(candidate['addedFiles'])==candidate['addedFiles'],'BLOCKED_CURRENT_CANDIDATE','addedFiles')
    binding_need(protected_files()==candidate['executionProtectedSnapshot'],'BLOCKED_CURRENT_CANDIDATE','protectedCandidate')
    binding_need(frozen_now(candidate['candidateSources'])==candidate['candidateSources'] and
                 frozen_now(candidate['originalProduct29'])==candidate['originalProduct29'] and
                 frozen_now(candidate['originalResources37'])==candidate['originalResources37'],
                 'BLOCKED_CURRENT_CANDIDATE','source53/product29/resources37')
    return candidate

def validate_fixed_progress(ctx, authority, contract, stage, argv=None):
    output=authority.get('outputRoot')
    absolute=str(ROOT/FIX22_PROGRESS)
    binding_need(output==FIX22_OUTPUT and contract.get('outputRoot')==FIX22_OUTPUT,
                 'BLOCKED_OUTPUT_ROOT',expected=FIX22_OUTPUT,actual=output)
    binding_need(contract.get('progressPath')==FIX22_PROGRESS and authority.get('progressPath')==absolute,
                 'BLOCKED_PROGRESS_CONTRACT',expected=absolute,actual=authority.get('progressPath'))
    binding_need(authority.get('activationPath')==FIX22_OUTPUT+'/activation.json',
                 'BLOCKED_ACTIVATION_PATH')
    activation=authority['activation']
    binding_need(activation.get('outputRoot')==FIX22_OUTPUT and activation.get('progressPath')==absolute,
                 'BLOCKED_ACTIVATION_PATH')
    binding_need(activation.get('preparedNew') is True and activation.get('preparationRunId')==authority['runId'],
                 'BLOCKED_OUTPUT_RESERVATION')
    callback=ctx.files[CALLBACK_PATH].decode('utf-8')
    constants=re.findall(r'^        private const string RelativeProgressPath = "([^"]+)";$',callback,re.M)
    binding_need(constants==[FIX22_PROGRESS] and
                 'string expectedPath = Path.Combine(projectRoot, RelativeProgressPath);' in callback and
                 re.search(r'string\.Equals\(arguments\[index \+ 1\], expectedPath,\s*StringComparison\.Ordinal\)',callback) and
                 'string.Equals(Path.GetFullPath(arguments[index + 1]), expectedPath,' not in callback,
                 'BLOCKED_CALLBACK_PROGRESS_PATH',CALLBACK_PATH,FIX22_PROGRESS,constants)
    paths=[FIX22_PROGRESS,FIX22_OUTPUT+'/activation.json',FIX22_OUTPUT+'/validation-tools/runner.py']
    for p in list(paths):
        paths.extend(str(parent) for parent in pathlib.PurePosixPath(p).parents if str(parent)!='.')
    for p in set(paths):
        kind=ctx.adapter.path_kind(p)
        binding_need(kind!='symlink','BLOCKED_OUTPUT_SYMLINK',p)
        binding_need(kind not in ('other',),'BLOCKED_OUTPUT_NONREGULAR',p)
    binding_need(ctx.adapter.path_kind(FIX22_OUTPUT)=='directory','BLOCKED_OUTPUT_ROOT',FIX22_OUTPUT)
    progress_kind=ctx.adapter.path_kind(FIX22_PROGRESS)
    if stage in ('prepared','allocated'):
        binding_need(progress_kind=='missing','BLOCKED_PROGRESS_NOT_NEW',FIX22_PROGRESS)
    elif stage=='spawn':
        binding_need(progress_kind=='file' and ctx.adapter.read(FIX22_PROGRESS)==b'',
                     'BLOCKED_PROGRESS_NOT_EMPTY',FIX22_PROGRESS)
    if stage=='prepared':
        binding_need(ctx.adapter.files(FIX22_OUTPUT)==sorted([FIX22_OUTPUT+'/activation.json',FIX22_OUTPUT+'/validation-tools/runner.py']),
                     'BLOCKED_OUTPUT_ROOT_NOT_FRESH',FIX22_OUTPUT)
    if argv is not None:
        binding_need(argv.count('-fightMatchTestProgress')==1,'BLOCKED_PROGRESS_ARGUMENT')
        at=argv.index('-fightMatchTestProgress')
        binding_need(at+1<len(argv) and argv[at+1]==absolute,'BLOCKED_PROGRESS_ARGUMENT',expected=absolute,
                     actual=argv[at+1] if at+1<len(argv) else None)
        binding_need(argv.count('-projectPath')==1 and argv[argv.index('-projectPath')+1]==str(ROOT),
                     'BLOCKED_PROJECT_ARGUMENT')
        binding_need(argv.count('-testFilter')==1 and argv[argv.index('-testFilter')+1]==ctx.files[contract['selectorPath']].decode('utf-8'),
                     'BLOCKED_SELECTOR')
    return absolute

def build_execution_argv(ctx, authority, contract, test_filter):
    argv=[EDITOR,'-batchmode','-releaseCodeOptimization','-projectPath',str(ROOT),
          '-runTests','-testPlatform','EditMode','-testFilter',test_filter,
          '-testResults',str(ROOT/FIX22_OUTPUT/'tests.xml'),'-fightMatchTestProgress',str(ROOT/FIX22_PROGRESS),
          '-logFile',str(ROOT/FIX22_OUTPUT/'unity.log')]
    binding_need(not set(['-quit','-nographics','-disableManagedDebugger','-buildTarget','-automated'])&set(argv),
                 'BLOCKED_ARGUMENT')
    validate_fixed_progress(ctx,authority,contract,'allocated',argv)
    return argv

def preflight():
    global OWNER
    assert OUT == ROOT / BINDING_AUTHORITY['outputRoot']
    assert_non_link(OUT)
    assert set(p.name for p in OUT.iterdir()) == {'activation.json', 'validation-tools'}, 'Formal root is not fresh'
    activation = json_bytes(bound_bytes(OUT/'activation.json'), str(OUT/'activation.json'))
    assert activation['mode']=='SINGLE_GRAPHICS_EXACT12'
    assert activation['prBinding']['head'] == BINDING_AUTHORITY['head'] and activation['prBinding']['tree'] == BINDING_AUTHORITY['tree'], 'FIX21 requires verified external authority'
    OWNER=activation['owner']
    assert OWNER['threadId']=='01a0e404-d89d-7ab2-bece-3cd1df3fbc52' and OWNER['hostId']=='local' and OWNER['turnId']
    assert activation['runner'] == identity(pathlib.Path(__file__))
    ready=json.loads(bound_text(SOURCE_ONLY/'static-result.json'))
    assert identity(SOURCE_ONLY/'validation-tools/runner.py') == ready['runner'], 'Historical FIX20 baseline drift'
    assert BINDING_STATE is not None, 'New runner must be bound to the externally authorized new head'
    assert identity(ROOT / activation['packet']['path']) == activation['packet']
    baseline = derive_execution_candidate(BINDING_STATE,BINDING_CONTRACT,BINDING_AUTHORITY)
    validate_execution_candidate(BINDING_STATE,baseline,BINDING_CONTRACT,BINDING_AUTHORITY)
    static = json.loads(bound_text(SOURCE_ONLY/'static-result.json'))
    assert static['status'] == 'SOURCE_READY' and all(static['mechanicalSourceChecks'].values())
    assert frozen_now(static['peerEvidence']) == static['peerEvidence']
    assert frozen_now(baseline['fixedInputs']) == baseline['fixedInputs']
    assert frozen_now(baseline['addedFiles']) == baseline['addedFiles']
    sources = json.loads(bound_text(R17/'static/source-manifest.json'))
    products = json.loads(bound_text(R17/'static/product-only-manifest.json'))
    res = json.loads(bound_text(R17/'static/resources-manifest-before.json'))
    assert [len(sources), len(products), len(res)] == [53, 29, 37]
    original_sources = sources
    sources = baseline['candidateSources']
    changed_paths = {r['path'] for r in baseline['codeAfter']}
    assert {r['path'] for r in sources} == {r['path'] for r in original_sources}
    assert [r for r in sources if r['path'] not in changed_paths] == [r for r in original_sources if r['path'] not in changed_paths]
    assert frozen_now(sources) == sources and frozen_now(products) == products and frozen_now(res) == res
    assert protected_files() == baseline['executionProtectedSnapshot'], 'Prepared execution candidate drift'
    assert pre['resourcesBefore'] == res
    core190 = json.loads(bound_text(R17/'core-190-graphics/expected-fullnames.json'))
    exact2 = pre['expectedTests2']; host = pre['expectedHost30']; all222 = pre['expectedTests222']
    assert core190 == pre['expectedCore190']
    old_core = xml_result(R19/'tests.xml', core190)
    assert old_core['counterExact'] and len(old_core['cases']) == 190
    old_xml = ET.fromstring(bound_bytes(R19/'tests.xml'))
    classnames = ['FightMatch.Core.Tests.UguiSceneCompositionTests', 'FightMatch.Core.Tests.LocalizedTextBindingTests']
    selected = [t for t in old_xml.iter('test-case') if t.get('classname') in classnames]
    core = sorted(t.get('fullname') for t in selected)
    assert len(core) == 12 and all(sum(t.get('classname') == c for t in selected) == 6 for c in classnames)
    retained178 = [t for t in old_core['cases'] if t['fullname'] not in core]
    assert len(retained178) == 178 and all(t['result'] == 'Passed' for t in retained178)
    groups = dict(exact2=exact2, host30=host, retained178=sorted(t['fullname'] for t in retained178), exact12=core)
    assert [len(v) for v in groups.values()] == [2, 30, 178, 12] and len(all222) == 222
    prepared = json.loads(bound_text(SOURCE_ONLY/'expected-fullnames.json'))
    assert prepared == dict(groups=groups, expected222=all222)
    assert all(len(v) == len(set(v)) for v in list(groups.values()) + [all222])
    pairs = [(a,b) for i,a in enumerate(groups) for b in list(groups)[i+1:]]
    assert all(not(set(groups[a]) & set(groups[b])) for a,b in pairs)
    assert collections.Counter(exact2 + host + groups['retained178'] + core) == collections.Counter(all222)
    retained = xml_result(R15 / 'exact-2-graphics/tests.xml', exact2)
    host_result = xml_result(R17 / 'host-30-graphics/tests.xml', host)
    assert retained['passed'] and host_result['passed'], 'Retained XML gate failed'
    raw_filter = bound_bytes(SOURCE_ONLY/'test-filter.txt')
    literal = '^(?:' + '|'.join(re.escape(n) for n in core) + ')$'
    assert raw_filter == literal.encode('utf-8') and not any(';' in n for n in core)
    assert all(re.fullmatch(literal, n) for n in core)
    assert not any(re.fullmatch(literal, n) for n in exact2 + host + groups['retained178'])
    assert not any(re.fullmatch(literal, n + '_not_a_test') for n in core)
    no_unity()
    rows = processes()
    prior_run = json.loads(bound_text(R19/'run.json'))
    previous_birth = prior_run['rootBirthIdentity']
    prior_alive = [r for r in rows if r['pid'] == previous_birth['pid']]
    prior_identity = current_identity(previous_birth['pid']) if prior_alive else None
    previous_identities=json.loads(bound_text(R19/'quiescence.json'))['ownedIdentities']
    prior_owned_matches=[]
    for row in rows:
        saved=previous_identities.get(str(row['pid']))
        if saved and saved.get('comm')==row['comm']:
            current=current_identity(row['pid'])
            if current['birth']==saved['birth'] and current['command']==saved['command']:
                prior_owned_matches.append(current)
    assert not prior_owned_matches, 'Prior owned child identity remains: '+repr(prior_owned_matches)
    assert prior_identity is None or (prior_identity['birth'], prior_identity['command']) != (previous_birth['birth'], previous_birth['command']), 'Prior Unity owner still alive'
    assert plistlib.load(open(pathlib.Path(EDITOR).parents[1] / 'Info.plist', 'rb'))['CFBundleVersion'] == '2022.3.18f1'
    free = shutil.disk_usage(ROOT).free
    assert free >= MIN_FREE
    font = font_structure()
    assert font['characterCount'] == font['glyphCount'] == 398 and font['textureCount'] == 4
    protected = protected_files()
    old_trees = old_tree_snapshots()
    diagnostic = sorted(sources + res + baseline['addedFiles'], key=lambda r:r['path'])
    assert len({r['path'] for r in diagnostic}) == 92 and canonical(diagnostic) == baseline['diagnosticCandidate']['sha256']
    (OUT / 'static').mkdir()
    create_json(OUT / 'static/inputs.json', dict(fixed=baseline['fixedInputs'],
                sourceOnly=tree_manifest(SOURCE_ONLY), predecessorPreflight=identity(R17/'preflight.json'),
                predecessorPartition=identity(R17/'static/partition-proof.json')))
    create_json(OUT / 'static/delta-manifest.json', dict(sources=sources, products=products, resources=res,
                delta=baseline['addedFiles'], diagnosticCandidate=baseline['diagnosticCandidate']))
    create_json(OUT / 'static/expected-fullnames.json', dict(groups=groups, expected222=all222))
    ownership = {}
    for label,names in [('fixed28', pre['expectedTests28']), ('lifecycle4', pre['expectedTests4']),
                        ('parameterized6', [n for n in pre['expectedTests28'] if '(' in n])]:
        ownership[label] = {n: next(g for g,values in groups.items() if n in values) for n in names}
    create_json(OUT / 'static/partition-proof.json', dict(counts={k:len(v) for k,v in groups.items()},
                hashes={k:canonical(v) for k,v in groups.items()}, expected222Sha256=canonical(all222),
                pairwiseDisjoint=True, counterUnionExact222=True, coverageOwnership=ownership,
                retainedExact2=retained, retainedHost30=host_result,
                retainedCore178=dict(identity=identity(R19/'tests.xml'),fullnames=groups['retained178'],passed=True),
                selector=dict(identity=identity(SOURCE_ONLY/'test-filter.txt'), escaping='re.escape; anchored literal alternation; mechanically selected by two classnames', positiveCount=len(core), excludedCount=len(host+exact2+retained178))))
    create_json(OUT/'static/static-result.json', static)
    with (OUT/'test-filter.txt').open('xb') as f:
        f.write(raw_filter)
    before = dict(utc=now(), source=sources, products=products, resources=res, delta=baseline['addedFiles'],
                  font=font, protectedClosedSet=protected, oldEvidence=compact_trees(old_trees),
                  freeBytes=free, gitFiles=[identity(ROOT/'.git/HEAD'),identity(ROOT/'.git/refs/heads/master')],
                  sceneTemplateSettingsAbsent=not (ROOT/'ProjectSettings/SceneTemplateSettings.json').exists())
    create_json(OUT/'before.json', before)
    old_code = bound_text(PREDECESSOR)
    current_code = bound_text(pathlib.Path(__file__))
    diff = ''.join(difflib.unified_diff(old_code.splitlines(keepends=True), current_code.splitlines(keepends=True),
                                      fromfile=str(PREDECESSOR.relative_to(ROOT)), tofile=str(pathlib.Path(__file__).relative_to(ROOT))))
    proof = dict(status='PREFLIGHT_PASS', utc=now(), owner=OWNER, prBinding=activation['prBinding'],
                 originalCounts=dict(source=len(sources), product=len(products), resources=len(res)),
                 addedCount=len(baseline['addedFiles']), processPreflight=dict(psSucceeded=True,noUnityFamily=True,previousRootIdentity=prior_identity),
                 resourceLimits=dict(testSeconds=TIMEOUT,naturalWaitSeconds=60,sigtermWaitSeconds=30,
                                     logBytesEach=MAX_LOG,evidenceBytes=MAX_EVIDENCE,freeBytesFloor=MIN_FREE),
                 runnerDerivation=dict(predecessor=identity(PREDECESSOR),current=identity(pathlib.Path(__file__)),
                                       diffSha256=sha(diff.encode()),diff=diff),
                 sourceOnlyStaticResult=identity(SOURCE_ONLY/'static-result.json'), freeBytes=free,
                 priorOwnedProcessesAbsent=True)
    create_json(OUT/'preflight.json', proof)
    print(json.dumps(dict(status=proof['status'],groups={k:len(v) for k,v in groups.items()},freeGiB=round(free/1024**3,2))),flush=True)
    return before,old_trees,core,exact2,host,all222,retained,host_result,retained178

def execute():
    before,old_trees,expected,exact2,host,all222,retained,host_result,retained178 = preflight()
    test_filter = (OUT/'test-filter.txt').read_bytes().decode('utf-8')
    argv = build_execution_argv(BINDING_STATE,BINDING_AUTHORITY,BINDING_CONTRACT,test_filter)
    no_unity()
    assert frozen_now(before['source']+before['resources']+before['delta']) == before['source']+before['resources']+before['delta']
    for name in ['test-events.jsonl','process-events.jsonl','progress.jsonl']:
        with (OUT/name).open('xb'):
            pass
    run = dict(task='FIX22',owner=OWNER,argv=argv,exactCommand=shlex.join(argv),
               startedUtc=now(),timeoutSeconds=TIMEOUT,expectedTestCount=len(expected),unityLaunchCount=0,
               timedOut=False,signalsSent=[],exitCode=None,passed=False)
    create_json(OUT/'run.json',run)
    owned = set()
    owned_identities = {}
    owned_ancestry = {}
    cleanup_clients = {}
    cleanup_client_exits = set()
    signalled_pids = set()
    samples = []
    progress_times = []
    last_progress = -10
    log_offset = 0
    last_line = None
    xml_first_seen = None
    budget_violations = []
    start = time.monotonic()
    def process_event(kind, **data):
        payload=dict(event=kind,utc=now(),elapsedSeconds=time.monotonic()-start)
        payload.update(data)
        jsonl(OUT/'process-events.jsonl',payload)
    def observe(force=False):
        nonlocal last_progress,log_offset,last_line,xml_first_seen
        rows = processes()
        if process.poll() is not None and not run.get('processExitObservedUtc'):
            run.update(exitCode=process.returncode,processExitObservedUtc=now(),
                       launchToExitObservationSeconds=time.monotonic()-start)
            process_event('process-exit',pid=process.pid,exitCode=process.returncode,afterDeadline=run['timedOut'])
        for pid,client in cleanup_clients.items():
            if client.poll() is not None and pid not in cleanup_client_exits:
                cleanup_client_exits.add(pid)
                process_event('compiler-shutdown-client-exit',pid=pid,exitCode=client.returncode)
        def remember_owned(pid):
            if pid not in owned_identities:
                info=current_identity(pid)
                try:
                    info['tools']=owned_tool_identity(info)
                except Exception as exc:
                    info['toolIdentityError']=type(exc).__name__+': '+str(exc)
                owned_identities[pid]=info
        for row in rows:
            if row['pid'] in owned:
                remember_owned(row['pid'])
        for _ in range(20):
            children=[r for r in rows if r['ppid'] in owned and r['pid'] not in owned]
            adopted=[]
            for row in children:
                saved=owned_identities.get(row['ppid'],{})
                parent_now=current_identity(row['ppid'])
                # Never extend an ancestry chain through a reused or unverifiable parent PID.
                if not (saved.get('birth') and parent_now['birth']==saved['birth'] and
                        parent_now['command']==saved['command'] and
                        parent_now['queryExitCodes']==saved['queryExitCodes']==[0,0,0]):
                    continue
                owned.add(row['pid'])
                parent=owned_ancestry[row['ppid']]
                owned_ancestry[row['pid']]=dict(origin=parent['origin'],chain=[row['pid']]+parent['chain'])
                remember_owned(row['pid'])
                adopted.append(row['pid'])
            if not adopted:
                break
        snapshot = dict(utc=now(),ownedProcesses=[r for r in rows if r['pid'] in owned],unityProcesses=unity_rows(rows))
        elapsed = time.monotonic()-start
        if force or elapsed-last_progress >= 5:
            last_progress = elapsed
            progress_times.append(elapsed)
            log_path=OUT/'unity.log'; xml_path=OUT/'tests.xml'
            log_info=dict(exists=log_path.exists())
            if log_path.exists():
                size=log_path.stat().st_size
                if size<=MAX_LOG:
                    with log_path.open('rb') as f:
                        f.seek(log_offset); added=f.read(MAX_LOG+1)
                    nonempty=[s for s in added.decode('utf-8',errors='replace').splitlines() if s.strip()]
                    if nonempty:
                        last_line=dict(byteRangeStart=log_offset,byteRangeEnd=size,text=nonempty[-1])
                    log_offset=size
                log_info.update(bytes=size,mtimeUtc=datetime.datetime.fromtimestamp(log_path.stat().st_mtime,datetime.timezone.utc).isoformat(),lastObservedNonemptyLine=last_line)
            xml_info=dict(exists=xml_path.exists())
            if xml_path.exists():
                if xml_first_seen is None: xml_first_seen=now()
                xml_info.update(bytes=xml_path.stat().st_size,mtimeUtc=datetime.datetime.fromtimestamp(xml_path.stat().st_mtime,datetime.timezone.utc).isoformat())
            trace=event_trace(process.pid)
            short_trace={k:trace[k] for k in ['rows','errors','runStarted','runFinished','startedCount','finishedCount','domains','lastFinished','unresolvedFullnames','partialTail']}
            record=dict(utc=now(),elapsedSeconds=elapsed,unityPid=process.pid,argv=argv,
                        **{k:snapshot[k] for k in ['ownedProcesses','unityProcesses']},log=log_info,xml=xml_info,progress=short_trace)
            jsonl(OUT/'progress.jsonl',record)
            samples.append(snapshot)
            for name in ['unity.log','editor.stdout.log','editor.stderr.log','tests.xml']:
                p=OUT/name
                if p.exists() and p.stat().st_size>MAX_LOG:
                    violation=dict(path=name,bytes=p.stat().st_size)
                    if violation not in budget_violations: budget_violations.append(violation)
            evidence_bytes=sum(p.stat().st_size for p in OUT.rglob('*') if p.is_file())
            free=shutil.disk_usage(ROOT).free
            if evidence_bytes>MAX_EVIDENCE or free<MIN_FREE:
                violation=dict(evidenceBytes=evidence_bytes,freeBytes=free)
                if violation not in budget_violations: budget_violations.append(violation)
        return snapshot

    def compile_failure():
        for name in ['unity.log','editor.stdout.log','editor.stderr.log']:
            path=OUT/name
            if path.exists():
                with path.open('rb') as f:
                    text=f.read(MAX_LOG+1).decode('utf-8',errors='replace')
                for line in text.splitlines():
                    if re.search(r'error CS\d+|Compilation failed|Scripts have compiler errors',line):
                        return dict(file=name,line=line)
        return None
    def wait_step(seconds):
        if seconds<=0:
            return
        if process.poll() is None:
            try: process.wait(timeout=seconds)
            except subprocess.TimeoutExpired: pass
        else:
            time.sleep(seconds)
    def remaining_snapshot():
        state=observe()
        state['unownedUnityProcesses']=[r for r in state['unityProcesses'] if r['pid'] not in owned]
        state['ownedProcesses']=[dict(r,compilerServer=owned_identities.get(r['pid'],{}).get('tools',{}).get('compilerServer',False))
                                 for r in state['ownedProcesses']]
        return state
    def verify(row):
        return verify_owned(row,owned_identities.get(row['pid'],{}),owned_ancestry.get(row['pid']),process.pid)
    def gentle(row, proof):
        immediate=verify(row)
        if not immediate.get('allowed'):
            return dict(status='IDENTITY_NO_LONGER_MATCHES',proof=immediate)
        args=shlex.split(immediate['current']['command'])
        command=[str(pathlib.Path(EDITOR).parents[1]/'NetCoreRuntime/dotnet'),
                 'exec',str(pathlib.Path(EDITOR).parents[1]/'DotNetSdkRoslyn/VBCSCompiler.dll'),args[3],'-shutdown']
        client=subprocess.Popen(command,cwd=ROOT,stdout=stdout,stderr=stderr)
        owned.add(client.pid)
        owned_ancestry[client.pid]=dict(origin='cleanup-client',chain=[client.pid,os.getpid()])
        cleanup_clients[client.pid]=client
        process_event('compiler-server-graceful-shutdown',pid=row['pid'],clientPid=client.pid,argv=command,identityProof=immediate)
        return dict(status='REQUEST_SENT',pid=row['pid'],clientPid=client.pid,argv=command,signalsSent=False)
    def terminate(row, proof):
        immediate=verify(row)
        if not immediate.get('allowed'):
            return dict(sent=False,gone=immediate.get('gone',False),pid=row['pid'],proof=immediate)
        assert row['pid'] not in signalled_pids, 'Repeated signal prohibited'
        signalled_pids.add(row['pid'])
        try:
            os.kill(row['pid'],signal.SIGTERM)
        except ProcessLookupError:
            return dict(sent=False,gone=True,pid=row['pid'])
        event=dict(sent=True,signal='SIGTERM',number=int(signal.SIGTERM),pid=row['pid'],utc=now(),identityProof=immediate)
        run['signalsSent'].append(event)
        process_event('signal',**event)
        return event
    with (OUT/'editor.stdout.log').open('x') as stdout,(OUT/'editor.stderr.log').open('x') as stderr:
        binding_checkpoint()
        validate_fixed_progress(BINDING_STATE,BINDING_AUTHORITY,BINDING_CONTRACT,'spawn',argv)
        process=subprocess.Popen(argv,cwd=ROOT,stdout=stdout,stderr=stderr)
        owned.add(process.pid)
        owned_ancestry[process.pid]=dict(origin='Unity',chain=[process.pid])
        run.update(unityLaunchCount=1,editorPid=process.pid,firstFailure=None)
        birth=current_identity(process.pid)
        run['rootBirthIdentity']=birth
        assert birth['command']==' '.join(argv) and birth['comm']==EDITOR and birth['queryExitCodes']==[0,0,0], 'Launched root identity mismatch'
        process_event('launch',pid=process.pid,argv=argv,birth=birth)
        process_event('deadline',deadlineUtc=(datetime.datetime.fromisoformat(run['startedUtc'])+datetime.timedelta(seconds=TIMEOUT)).isoformat(),timeoutSeconds=TIMEOUT)
        write(OUT/'run.json',run)
        print(json.dumps(dict(status='RUNNING',pid=process.pid,output=str(OUT),timeoutSeconds=TIMEOUT)),flush=True)
        outcome=supervise_root(process.poll,compile_failure,observe,wait_step,lambda:time.monotonic()-start,TIMEOUT)
        run['monitorOutcome']=outcome
        if outcome['kind']!='ROOT_EXIT_ZERO':
            run['firstFailure']=dict(outcome,observedUtc=now())
            process_event('first-failure',failure=run['firstFailure'])
            # Persist the first reason before cleanup, even if a later cleanup operation fails.
            write(OUT/'run.json',run)
        if outcome['kind']=='TIMEOUT':
            run.update(timedOut=True,timeoutUtc=now(),timeoutElapsedSeconds=outcome['elapsedSeconds'])
            process_event('timeout',pid=process.pid)
            snapshot=observe(True)
            run['timeoutSnapshot']=dict(utc=now(),elapsedSeconds=time.monotonic()-start,
                        source=frozen_now(before['source']),resources=resources(),delta=frozen_now(before['delta']),
                        processTree=snapshot,lastProgress=event_trace(process.pid),rootBirthIdentity=birth)
            write(OUT/'run.json',run)
        process_event('cleanup-start',reason=outcome['kind'],gentleSeconds=60,terminationSeconds=30)
        cleanup=settle_owned(outcome['kind'],process.pid,remaining_snapshot,verify,gentle,terminate,
                             wait_step,lambda:time.monotonic()-start)
        process_event('cleanup-end',status=cleanup['status'],durationSeconds=cleanup['durationSeconds'])
        run['cleanup']=cleanup
    final=observe(True)
    quiescent=not final['ownedProcesses'] and not final['unityProcesses'] and cleanup['status']=='COMPLETE'
    run['exitCode']=process.poll()
    if process.poll() is None:
        process_event('manual-cleanup-required',pid=process.pid)
    create_json(OUT/'quiescence.json',dict(utc=now(),quiescent=quiescent,ownedPids=sorted(owned),
                ownedIdentities=owned_identities,ownedAncestry=owned_ancestry,
                finalObservation=final,rootExitCode=process.poll(),signalsSent=run['signalsSent'],cleanup=cleanup,
                firstFailure=run['firstFailure'],totalObservationSeconds=time.monotonic()-start))
    run['observationCompletedSeconds']=time.monotonic()-start
    trace=event_trace(process.pid,True)
    logs={}
    for name in ['unity.log','editor.stdout.log','editor.stderr.log']:
        p=OUT/name
        logs[name]=p.read_bytes()[:MAX_LOG+1].decode('utf-8',errors='replace') if p.exists() else ''
    log=logs['unity.log']
    combined_logs='\n'.join(logs.values())
    callback_errors=[s for s in combined_logs.splitlines() if 'BLOCKED_PROGRESS_EVIDENCE' in s]
    compiler_errors=[s for s in combined_logs.splitlines() if re.search(r'error CS\d+|Compilation failed|Unhandled Exception',s)]
    runtime_errors=[s for s in combined_logs.splitlines() if not s.startswith('##utp:') and re.search(
        r'MissingReferenceException|SerializationException|Failed to deserialize|NullReferenceException|ObjectDisposedException|AssertionException|Assertion failed|Cannot change the sibling position|Cannot set the parent|while activating or deactivating|Error[^\n]*serializ|serialized file[^\n]*(?:corrupt|invalid)|referenced script[^\n]*(?:missing|unknown)|Missing \(Mono Script\)',s,re.I)]
    binding_error=None
    try: binding_checkpoint()
    except BindingFailure as exc: binding_error=exc.detail
    current_xml=xml_result(OUT/'tests.xml',expected)
    after=dict(utc=now(),source=frozen_now(before['source']),products=frozen_now(before['products']),
               resources=resources(),delta=frozen_now(before['delta']),font=font_structure(),
               protectedClosedSet=protected_files(),oldEvidence=compact_trees(old_tree_snapshots()),
               freeBytes=shutil.disk_usage(ROOT).free,
               gitFiles=[identity(ROOT/'.git/HEAD'),identity(ROOT/'.git/refs/heads/master')],
               sceneTemplateSettingsAbsent=not (ROOT/'ProjectSettings/SceneTemplateSettings.json').exists())
    create_json(OUT/'after.json',after)
    valid_prefix=not trace['errors'] and not callback_errors and trace['rows']>0
    finished_counter=collections.Counter(e['fullName'] for e in trace['finished'])
    checks=dict(reviewBindingUnchanged=binding_error is None,exitZero=process.returncode==0,within360=not run['timedOut'] and run.get('launchToExitObservationSeconds',float('inf'))<=TIMEOUT,
                xmlExact12Passed=current_xml['passed'],progressPrefixValid=valid_prefix,
                finishedCounterExact12=finished_counter==collections.Counter(expected),
                finishedAllPassed=all(e['result']=='Passed' for e in trace['finished']),
                startedCounterExact12=trace['startedCount']==len(expected) and not trace['repeatedStarts'] and not trace['unmatchedStarted'] and finished_counter==collections.Counter(expected),
                realRunStarted=trace['runStarted']==1,
                realRunFinished=trace['runFinished']==1 and trace.get('runResult')=='Passed',
                noUnresolvedFullnames=not trace['unresolvedFullnames'],quiescent=quiescent,cleanupComplete=cleanup['status']=='COMPLETE',
                source53Unchanged=after['source']==before['source'],product29Unchanged=after['products']==before['products'],
                resources37Unchanged=after['resources']==before['resources'],diagnosticDeltaUnchanged=after['delta']==before['delta'],
                font398Glyph398Atlas4=after['font']==before['font'] and after['font']['characterCount']==after['font']['glyphCount']==398 and after['font']['textureCount']==4,
                protectedClosedSetUnchanged=after['protectedClosedSet']==before['protectedClosedSet'],
                oldEvidenceUnchanged=after['oldEvidence']==before['oldEvidence'],gitFilesUnchanged=after['gitFiles']==before['gitFiles'],
                noCompilerErrors=not compiler_errors,noRuntimeErrors=not runtime_errors,
                graphicsMetal=bool(re.search(r'(?m)^Initializing Metal device caps: Apple ',log)) and not bool(re.search(r'Forcing GfxDevice: Null|NullGfxDevice:|Renderer: Null Device|Renderer:[^\n]*(?:software|swiftshader|llvmpipe)',log,re.I)),
                budgets=not budget_violations and after['freeBytes']>=MIN_FREE,
                sceneTemplateSettingsAbsent=after['sceneTemplateSettingsAbsent'])
    progress_intervals=[b-a for a,b in zip(progress_times,progress_times[1:])]
    run.update(reviewBindingError=binding_error,checks=checks,passed=all(checks.values()),testEvents=trace,xml=current_xml,
               callbackErrors=callback_errors,compilerErrors=compiler_errors,runtimeErrors=runtime_errors,
               resourceBudgetViolations=budget_violations,xmlFirstObservedUtc=xml_first_seen,
               maximumProgressIntervalSeconds=max(progress_intervals,default=0),
               graphicsEvidence=[s for s in log.splitlines() if re.search(r'GfxDevice|^Using device |^Initializing Metal device caps:|^\s*Renderer:',s)],
               runFinishedLogMarkers=[s for s in log.splitlines() if 'Saving results to:' in s or 'RunFinished' in s])
    all_cases=retained['cases']+host_result['cases']+retained178+current_xml['cases']
    actual_counter=collections.Counter(t['fullname'] for t in all_cases)
    expected_counter=collections.Counter(all222)
    grouped=dict(passed=run['passed'] and actual_counter==expected_counter and all(t['result']=='Passed' for t in all_cases),
                 actualCount=len(all_cases),expectedCount=len(all222),counterExact=actual_counter==expected_counter,
                 missing=list((expected_counter-actual_counter).elements()),unexpected=list((actual_counter-expected_counter).elements()),
                 results={t['fullname']:t['result'] for t in all_cases},
                 evidence=[retained.get('identity'),host_result.get('identity'),identity(R19/'tests.xml'),current_xml.get('identity')],
                 reusedDifferentRuns=True,retainedCore190PassedSubsetCount=len(retained178),replacedOldClassTestsCount=12,
                 releaseOptimizationInFIX19AndThisRun=True,originalFIX19SixFailuresPreserved=True,
                 originalExact222TimeoutRemainsFailure=True,nativeReopenRun=False)
    by_name={t['fullname']:t for t in all_cases}
    grouped['coverage']={label:{n:by_name.get(n,{}).get('result','NOT_OBSERVED') for n in names}
                         for label,names in [('fixed28',pre['expectedTests28']),('lifecycle4',pre['expectedTests4']),
                                            ('parameterized6',[n for n in pre['expectedTests28'] if '(' in n])]}
    b16=next((t for t in all_cases if '.B16B04_Cancel' in t['fullname']),None)
    wire=next((t for t in all_cases if '.UGUI_COPY_WIRE01_' in t['fullname']),None)
    grouped['b16Matrix']=bool(b16 and b16['result']=='Passed' and all('uGUI cancellation: '+cause in b16['output'] for cause in ['cancel','captureout','detach','close','geometry','transform','selection','outside']))
    grouped['wireTerminalRetry']=bool(wire and wire['result']=='Passed' and 'FIX13 battle-dispose + pending: original operation committed once; terminal view remains unbound; BattleViewDisposed visible.' in wire['output'])
    grouped['passed']=grouped['passed'] and grouped['b16Matrix'] and grouped['wireTerminalRetry']
    create_json(OUT/'grouped-exact222-coverage.json',grouped)
    geometry_failures = []
    business_failures = []
    teardown_failures = []
    for case in current_xml['cases']:
        if case['result'] == 'Passed':
            continue
        message = case.get('message') or ''; stack = case.get('stack') or ''
        detail = dict(fullname=case['fullname'],message=message,stack=stack,output=case.get('output') or '')
        if 'TearDown' in message or 'ExitControlledPlayModeAfterFailure' in stack:
            teardown_failures.append(detail)
        if ('Board geometry is unavailable' in message or
            any(marker in stack for marker in ['AssertBoardReady', 'UguiHostRig+<Ready>', 'UguiHostRig.Ready']) or
            ('.BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical' in case['fullname'] and
             'Expected: greater than 0' in message)):
            geometry_failures.append(detail)
        else:
            business_failures.append(detail)
    run['failureBranches'] = dict(activationOrGeometry=geometry_failures,otherTestFailures=business_failures,teardown=teardown_failures)
    first_kind=(run.get('firstFailure') or {}).get('kind')
    if binding_error: status='BLOCKED_POSTRUN_BINDING'
    elif first_kind=='COMPILE_FAILURE': status='BLOCKED_COMPILE'
    elif geometry_failures: status='BLOCKED_PRODUCT_ACTIVATION_OR_GEOMETRY'
    elif first_kind=='ROOT_EXIT_NONZERO': status='BLOCKED_ROOT_NONZERO'
    elif first_kind=='TIMEOUT': status='BLOCKED_EXACT12_TIMEOUT'
    elif not quiescent: status='BLOCKED_MANUAL_PROCESS_CLEANUP'
    elif not valid_prefix: status='BLOCKED_PROGRESS_EVIDENCE'
    elif run['passed'] and grouped['passed']: status='EXACT12_PASS_GROUPED222_EVIDENCE_READY'
    else: status='BLOCKED_EXACT12_VALIDATION'
    run['status']=status
    write(OUT/'run.json',run)
    boundary=('compilation before test execution' if first_kind=='COMPILE_FAILURE' else
              'root exited nonzero' if first_kind=='ROOT_EXIT_NONZERO' else
              'progress evidence unavailable or invalid' if not valid_prefix else
              'unfinished test boundary' if trace['unresolvedFullnames'] else
              'runner finalization boundary' if trace['finishedCount']==len(expected) and not (trace['runFinished'] and current_xml['exists'] and process.poll() is not None) else
              'next scheduling or domain-resume boundary' if not run['passed'] else 'completed')
    receipt=dict(status=status,owner=OWNER,returnThread='01a0e401-511d-79f2-b47f-3ab0ade1681b/local',
                 prBinding=json_bytes(bound_bytes(OUT/'activation.json'),str(OUT/'activation.json'))['prBinding'],
                 endedUtc=now(),unityLaunchCount=1,exitCode=run['exitCode'],timedOut=run['timedOut'],
                 reviewBindingError=binding_error,firstFailure=run.get('firstFailure'),cleanup=cleanup,failureBranches=run['failureBranches'],
                 launchToExitObservationSeconds=run.get('launchToExitObservationSeconds'),totalWallSeconds=time.monotonic()-start,
                 boundary=boundary,lastFinished=trace['lastFinished'],unmatchedStarted=trace['unmatchedStarted'],
                 unresolvedFullnames=trace['unresolvedFullnames'],finishedCount=trace['finishedCount'],eventRows=trace['rows'],
                 domainCount=len(trace['domains']),checks=checks,failedChecks=[k for k,v in checks.items() if not v],
                 grouped222Passed=grouped['passed'],allOwnedProcessesExited=quiescent,signalsSent=run['signalsSent'],
                 resourceCounts=dict(characters=after['font']['characterCount'],glyphs=after['font']['glyphCount'],atlases=after['font']['textureCount']),
                 evidence=[identity(p) for p in sorted(OUT.rglob('*')) if p.is_file()],
                 notRun=['extra compile','Core190','Host30','exact2','split runs','native reopen','APK','source repair','Git'],
                 review='GitHub review is independent and pending; this receipt is not author acceptance.')
    create_json(OUT/'final-receipt.json',receipt)
    print(json.dumps({k:receipt[k] for k in ['status','exitCode','timedOut','finishedCount','eventRows','domainCount','boundary','lastFinished','unresolvedFullnames','failedChecks','allOwnedProcessesExited']},ensure_ascii=False),flush=True)
    return 0 if status=='EXACT12_PASS_GROUPED222_EVIDENCE_READY' else 3


def execute_from_bound_inputs(ctx, authority, authority_check, adapter, contract):
    # Only a future trusted launcher may call this after self/contract/raw-object binding.
    global BINDING_STATE, BINDING_AUTHORITY, BINDING_CHECK_AUTHORITY, BINDING_ADAPTER, BINDING_CONTRACT, OUT, pre
    BINDING_STATE=ctx;BINDING_AUTHORITY=authority;BINDING_CHECK_AUTHORITY=authority_check
    BINDING_ADAPTER=adapter;BINDING_CONTRACT=contract
    validate_fixed_progress(ctx,authority,contract,'prepared')
    OUT=ROOT/FIX22_OUTPUT
    pre=json_bytes(bound_bytes(R17/'preflight.json'),str(R17/'preflight.json'))
    return execute()

if __name__=='__main__':
    raise SystemExit('FIX22 real execution requires a separately authorized external launcher; this source artifact does not activate a run.')
