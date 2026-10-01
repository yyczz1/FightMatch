
"""FIX22 offline fixtures: real binding/main functions; only object/file I/O and launch are in memory."""
import copy
import importlib.util
import json
import pathlib
import sys
import time
import traceback
import xml.etree.ElementTree as ET
sys.dont_write_bytecode=True
HERE=pathlib.Path(__file__).resolve()
ROOT=HERE.parents[5]
RUNNER=HERE.with_name('runner.py')
spec=importlib.util.spec_from_file_location('fix22_runner',RUNNER)
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
CONTRACT_PATH='TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source/binding-contract.json'
CONTRACT=json.loads((ROOT/CONTRACT_PATH).read_text())
N='TestArtifacts/FightMatch/UGUI-01/q4-correction-22-source/'
A='docs/versioning/2026-10-01-ugui-pr/fix22/'
S='TestArtifacts/FightMatch/UGUI-01/q4-correction-20-source/'
E='TestArtifacts/FightMatch/UGUI-01/q4-correction-20/'

def jb(data):return (json.dumps(data,sort_keys=True,ensure_ascii=False)+'\n').encode()
def row(path,data):return dict(path=path,**m.raw_identity(data))

class MemoryAdapter:
    def __init__(self,files,objects):
        self.data=files;self.objects=objects;self.links=set();self.environment={};self.metadata={}
        self.localHead='125b849be13fe2ebf5b1185f3cdb83d19c6327ef'
    def guard_environment(self):m.validate_git_environment(self.environment,self.metadata)
    def prefetch(self,oids):pass
    def object(self,oid):
        if oid not in self.objects:raise m.BindingFailure('BLOCKED_OBJECT_MISSING',oid)
        return self.objects[oid]
    def read(self,path):
        m.safe_relative(path)
        m.binding_need(path not in self.links,'BLOCKED_SYMLINK',path)
        m.binding_need(path in self.data,'BLOCKED_FILE_MISSING',path)
        return self.data[path]
    def files(self,prefix):
        return sorted(p for p in self.data if p.startswith(prefix+'/'))
    def path_kind(self,path):
        if path in self.links:return 'symlink'
        if path in self.data:return 'file'
        if any(p.startswith(path+'/') for p in self.data):return 'directory'
        return 'missing'
    def read_absolute(self,path):
        raise AssertionError('Fixture must not read external executables: '+str(path))

def git_object(objects,kind,raw):
    oid=m.git_oid(kind,raw);objects[oid]=(kind,raw);return oid
def tree(objects,files):
    root={}
    for path,data in files.items():
        parts=path.split('/');node=root
        for part in parts[:-1]:node=node.setdefault(part,{})
        node[parts[-1]]=data
    def encode(node):
        data=b''
        for name,value in sorted(node.items(),key=lambda kv:kv[0]+('/' if isinstance(kv[1],dict) else '')):
            if isinstance(value,dict):mode='40000';oid=encode(value)
            else:mode='100644';oid=git_object(objects,'blob',value)
            data+=mode.encode()+b' '+name.encode()+b'\0'+bytes.fromhex(oid)
        return git_object(objects,'tree',data)
    return encode(root)
def commit(objects,tid,message):
    raw=('tree '+tid+'\nauthor Fixture <fixture@example.invalid> 1 +0000\ncommitter Fixture <fixture@example.invalid> 1 +0000\n\n'+message).encode()
    return git_object(objects,'commit',raw)
def xml(cases):
    passed=sum(c['result']=='Passed' for c in cases)
    root=ET.Element('test-run',result='Passed' if passed==len(cases) else 'Failed',total=str(len(cases)),passed=str(passed),failed=str(len(cases)-passed),skipped='0',inconclusive='0')
    for c in cases:ET.SubElement(root,'test-case',c)
    return ET.tostring(root)
def fixture(omit_chain=False,traversal=False,duplicate_mapping=False,full=False,scenario=None):
    c=copy.deepcopy(CONTRACT);files={};objects={};tracked={}
    for proof in c['dependencyProof']:
        data=(ROOT/proof['path']).read_bytes();files[proof['path']]=data;tracked[proof['path']]=data
    for p in c['externalTreeInputs']:files[p]=b'fixture external input';tracked[p]=files[p]
    for saved in c.get('predecessorFiles',[]):
        p=saved['path'];files[p]=(ROOT/p).read_bytes();tracked[p]=files[p]
    for dependency in c.get('externalDigestInputs',[]):files[dependency['path']]=(ROOT/dependency['path']).read_bytes()
    files['Assets/Outside92.cs']=b'class Outside92 {}';tracked['Assets/Outside92.cs']=files['Assets/Outside92.cs']
    files['Assets/UI/Fixture.asset']=b'fixture resource';tracked['Assets/UI/Fixture.asset']=files['Assets/UI/Fixture.asset']
    for p in ['Tools/Tracked.ps1','Config/Tracked.json','Generated/Tracked.json']:
        files[p]=b'tracked';tracked[p]=files[p]
    for p in c['excludedWip']:files[p]=b'independent LOC WIP'
    if full:
        cb=m.CALLBACK_PATH
        new_cb=(ROOT/cb).read_bytes()
        old_cb=new_cb.replace(b'        private const string RelativeProgressPath = "'+m.FIX22_PROGRESS.encode()+b'";\n',b'').replace(
            b'                string expectedPath = Path.Combine(projectRoot, RelativeProgressPath);',
            b'                string expectedPath = Path.Combine(projectRoot,\n                    "TestArtifacts/FightMatch/UGUI-01/q4-correction-20/test-events.jsonl");').replace(
            b'string.Equals(arguments[index + 1], expectedPath,',b'string.Equals(Path.GetFullPath(arguments[index + 1]), expectedPath,').replace(b'FIX22',b'FIX20')
        assert m.raw_identity(old_cb)==dict(bytes=13090,sha256='5a6c3a1ba9a51ddb061cff0aa9b18eff620849070789a48ba0413321f63f8d14')
        files[cb]=old_cb;tracked[cb]=old_cb
        files[cb+'.meta']=(ROOT/(cb+'.meta')).read_bytes();tracked[cb+'.meta']=files[cb+'.meta']
        source_paths=sorted(p for p in files if p.startswith('Assets/') and p.endswith('.cs') and p!=cb)
        for i in range(53-len(source_paths)):
            p='Assets/FixtureSources/Source%02d.cs'%i;files[p]=('class Source%d {}'%i).encode();tracked[p]=files[p];source_paths.append(p)
        source_paths.sort()
        font=str(m.FONT.relative_to(ROOT));files[font]=(ROOT/font).read_bytes();tracked[font]=files[font]
        resource_paths=['Assets/UI/Fixture.asset',font]
        for i in range(35):
            p='Assets/FixtureResources/Resource%02d.asset'%i;files[p]=b'resource';tracked[p]=files[p];resource_paths.append(p)
        source_rows=[row(p,files[p]) for p in source_paths]
        product_rows=source_rows[:29];resource_rows=[row(p,files[p]) for p in sorted(resource_paths)]
    protected=[row(p,files[p]) for p in sorted(files) if p.startswith(('Assets/','Packages/','ProjectSettings/','Tools/','Config/','Generated/'))]
    # A real fixture retains the full-set semantics, with fewer ordinary tree files.
    if not any(p.startswith('ProjectSettings/') for p in tracked):
        p='ProjectSettings/ProjectVersion.txt';files[p]=b'fixture version';tracked[p]=files[p];protected.append(row(p,files[p]));protected.sort(key=lambda r:r['path'])
    selected=[dict(fullname=cname+'.Case'+str(i),classname=cname,result='Passed' if i<3 else 'Failed') for cname in c['classnames'] for i in range(6)]
    remainder=[dict(fullname='Fixture.Other.Case'+str(i),classname='Fixture.Other',result='Passed') for i in range(178)]
    host=[dict(fullname='Fixture.Host.Case'+str(i),classname='Fixture.Host',result='Passed') for i in range(30)]
    exact=[dict(fullname='Fixture.Exact.Case'+str(i),classname='Fixture.Exact',result='Passed') for i in range(2)]
    latest=[dict(t,result='Passed') for t in selected]
    files[c['oldCoreXml']]=xml(selected+remainder);files[c['hostXml']]=xml(host);files[c['exact2Xml']]=xml(exact)
    for p in c['requiredHistoricalInputs']:
        if not p.startswith(S) and p not in files:files[p]=b'{}\n'
    old_prefixes=sorted({p.rsplit('/',1)[0] for p in [c['oldCoreXml']]})
    # Use the same exact roots named by the contract, including R17/preflight and runner.
    r17=next(p.rsplit('/',1)[0] for p in c['requiredHistoricalInputs'] if p.endswith('/preflight.json'))
    r15=c['exact2Xml'].split('/exact-2-graphics/')[0]
    r19=c['oldCoreXml'].rsplit('/',1)[0]
    if full:
        protected=[row(p,files[p]) for prefix in ['Assets','Packages','ProjectSettings','Config','Generated','Tools'] for p in sorted(files) if p.startswith(prefix+'/')]
        core_names=[t['fullname'] for t in selected+remainder]
        groups0=dict(exact2=[t['fullname'] for t in exact],host30=[t['fullname'] for t in host],retained178=sorted(t['fullname'] for t in remainder),exact12=sorted(t['fullname'] for t in selected))
        files[r17+'/static/source-manifest.json']=jb(source_rows)
        files[r17+'/static/product-only-manifest.json']=jb(product_rows)
        files[r17+'/static/resources-manifest-before.json']=jb(resource_rows)
        files[r17+'/core-190-graphics/expected-fullnames.json']=jb(core_names)
        files[r17+'/static/partition-proof.json']=b'{}'
        files[r17+'/preflight.json']=jb(dict(resourcesBefore=resource_rows,expectedTests2=groups0['exact2'],
            expectedHost30=groups0['host30'],expectedCore190=core_names,expectedTests222=sum(groups0.values(),[]),
            expectedTests28=groups0['retained178'][:28],expectedTests4=groups0['retained178'][28:32],oldRoots=[]))
        files[r19+'/run.json']=jb(dict(rootBirthIdentity=dict(pid=88)))
        files[r19+'/quiescence.json']=jb(dict(ownedIdentities={}))
    roots=[]
    for prefix in [r17,r15,r19]:
        rows=[row(p,files[p]) for p in sorted(files) if p.startswith(prefix+'/')]
        roots.append(dict(root=str(ROOT/prefix),fileCount=len(rows),totalBytes=sum(r['bytes'] for r in rows),
                          manifestSha256=m.canonical(rows)))
    if omit_chain:roots=[x for x in roots if not x['root'].endswith('/q4-correction-17')]
    # FIX20 source peer chain; contents are parsed only after raw object/digest validation.
    groups=dict(exact2=[t['fullname'] for t in exact],host30=[t['fullname'] for t in host],retained178=sorted(t['fullname'] for t in remainder),exact12=sorted(t['fullname'] for t in selected))
    expected=dict(groups=groups,expected222=sum(groups.values(),[]))
    selector=('^(?:'+'|'.join(m.re.escape(n) for n in groups['exact12'])+')$').encode()
    files[S+'activation.json']=b'{}\n';files[S+'expected-fullnames.json']=jb(expected)
    files[S+'partition-proof.json']=b'{}\n';files[S+'source.patch']=b'fixture patch'
    files[S+'test-filter.txt']=selector;files[S+'validation-tools/runner.py']=b'old frozen runner'
    delta=dict(fixedInputs=[row(c['oldCoreXml'],files[c['oldCoreXml']])],oldEvidenceBefore=roots)
    if full:
        added=[row(cb,old_cb),row(cb+'.meta',files[cb+'.meta'])]
        diag=sorted(source_rows+resource_rows+added,key=lambda r:r['path'])
        delta.update(addedFiles=added,protectedCandidate=protected,candidateSources=source_rows,codeAfter=[],
                     originalProduct29=product_rows,originalResources37=resource_rows,diagnosticCandidate=dict(count=92,sha256=m.canonical(diag)))
    files[S+'delta-manifest.json']=jb(delta)
    peers=[row(p,files[p]) for p in sorted(files) if p.startswith(S)]
    static=jb(dict(peerEvidence=peers,status='SOURCE_READY',mechanicalSourceChecks=dict(fixture=True),runner=row(S+'validation-tools/runner.py',files[S+'validation-tools/runner.py'])))
    files[S+'static-result.json']=static;tracked[c['baselineStatic']['tree']]=static
    tracked[c['baselineRunner']['tree']]=files[S+'validation-tools/runner.py']
    files[E+'validation-tools/runner.py']=files[S+'validation-tools/runner.py']
    snapshot=dict(source=[],products=[],resources=[],delta=[],protectedClosedSet=protected,oldEvidence=roots,gitFiles=[],font={})
    files[E+'before.json']=jb(snapshot);files[E+'after.json']=jb(snapshot)
    owner=dict(threadId='fixture-C',hostId='local',turnId='fixture-original-run')
    files[E+'run.json']=jb(dict(editorPid=77,owner=owner,exitCode=0,timedOut=False,launchToExitObservationSeconds=75))
    files[E+'quiescence.json']=jb(dict(rootExitCode=0,finalObservation=dict(ownedProcesses=[],unityProcesses=[])))
    files[E+'tests.xml']=xml(latest);files[E+'test-filter.txt']=selector
    ev=[dict(eventName='RunStarted',fullName='Fixture',result='',pid=77)]
    for t in latest:
        ev.extend([dict(eventName='TestStarted',fullName=t['fullname'],result='',pid=77),dict(eventName='TestFinished',fullName=t['fullname'],result='Passed',pid=77)])
    ev.append(dict(eventName='RunFinished',fullName='Fixture',result='Passed',pid=77))
    for i,t in enumerate(ev):t['seq']=i+1
    files[E+'test-events.jsonl']=b''.join(jb(t) for t in ev)
    evidence=[row(p,files[p]) for p in sorted(files) if p.startswith(E)]
    files[c['receiptPath']]=jb(dict(evidence=evidence,owner=owner))
    c['receiptIdentity']=m.raw_identity(files[c['receiptPath']])
    tid=tree(objects,tracked);hid=commit(objects,tid,'historical fixture')
    c['historical']=dict(head=hid,tree=tid)
    if traversal:c['baselineStatic']['actual']='../escape'
    if duplicate_mapping:c['selfMappings']['tests']=c['selfMappings']['runner']
    files[c['futurePacket']]=b'fixture activated packet';tracked[c['futurePacket']]=files[c['futurePacket']]
    files[N+'validation-tools/runner.py']=RUNNER.read_bytes();tracked[A+'validation-tools/runner.py']=files[N+'validation-tools/runner.py']
    files[N+'validation-tools/test_review_binding.py']=HERE.read_bytes();tracked[A+'validation-tools/test_review_binding.py']=files[N+'validation-tools/test_review_binding.py']
    new_delta=json.loads((ROOT/(N+'execution-delta.json')).read_text())
    if full:
        files[cb]=new_cb;tracked[cb]=new_cb
        new_delta['historical']=c['historical']
        new_delta['changes']=[dict(path=cb,before=row(cb,old_cb),after=row(cb,new_cb))]
        candidate=copy.deepcopy(delta)
        for k in ['addedFiles','protectedCandidate']:
            candidate[k]=[row(cb,new_cb) if r['path']==cb else r for r in candidate[k]]
        candidate['diagnosticCandidate']=dict(count=92,sha256=m.canonical(sorted(source_rows+resource_rows+candidate['addedFiles'],key=lambda r:r['path'])))
        new_delta['derivedCandidate']={k:candidate[k] for k in ['addedFiles','protectedCandidate','diagnosticCandidate']}
        if scenario:scenario(files,tracked,c,new_delta,dict(delta,oldCallbackBytes=old_cb))
    files[N+'execution-delta.json']=jb(new_delta);tracked[A+'execution-delta.json']=files[N+'execution-delta.json']
    files[CONTRACT_PATH]=jb(c);tracked[A+'binding-contract.json']=files[CONTRACT_PATH]
    newtid=tree(objects,tracked);newhid=commit(objects,newtid,'new binding source fixture')
    authority=dict(runId='fixture-run',mode='SINGLE_GRAPHICS_EXACT12',repo='yyczz1/FightMatch',pr=1,head=newhid,tree=newtid,
                   currentHead=newhid,active=True,owner=dict(threadId='fixture-C',hostId='local',turnId='fixture-turn'))
    authority['activationPath']='TestArtifacts/FightMatch/UGUI-01/fixture-run/activation.json'
    authority['activation']=dict(runId=authority['runId'],owner=authority['owner'],prBinding=dict(head=newhid,tree=newtid))
    files[authority['activationPath']]=jb(authority['activation'])
    selfpaths=dict(runner=N+'validation-tools/runner.py',tests=N+'validation-tools/test_review_binding.py',contract=CONTRACT_PATH,delta=N+'execution-delta.json')
    post=dict(authority,mode='FIX21_SOURCE_AND_POSTHOC_ONLY',head=hid,tree=tid,currentHead=hid,receipt=c['receiptIdentity'])
    if full:
        authority['owner']=dict(threadId='01a0e404-d89d-7ab2-bece-3cd1df3fbc52',hostId='local',turnId='offline-C-fixture')
        authority.update(outputRoot=m.FIX22_OUTPUT,progressPath=str(ROOT/m.FIX22_PROGRESS),
                         wipSnapshot=[dict(path=r['path'],**m.raw_identity(files[r['path']])) for r in protected if r['path'] in c['excludedWip']])
        files.pop(authority['activationPath'])
        authority['activationPath']=m.FIX22_OUTPUT+'/activation.json'
        formal=m.FIX22_OUTPUT+'/validation-tools/runner.py'
        files[formal]=files[N+'validation-tools/runner.py'];selfpaths['runner']=formal
        authority['selfPaths']=selfpaths
        authority['activation']=dict(runId=authority['runId'],owner=authority['owner'],mode=authority['mode'],
            prBinding=dict(head=newhid,tree=newtid),outputRoot=m.FIX22_OUTPUT,progressPath=authority['progressPath'],
            preparedNew=True,preparationRunId=authority['runId'],runner=row(formal,files[formal]),
            packet=row(c['futurePacket'],files[c['futurePacket']]))
        files[authority['activationPath']]=jb(authority['activation'])
        files['.git/HEAD']=b'ref: refs/heads/master\n';files['.git/refs/heads/master']=b'125b849be13fe2ebf5b1185f3cdb83d19c6327ef\n'
    return MemoryAdapter(files,objects),authority,post,selfpaths,c

def run_original35():
    begin=time.monotonic();cases=[]
    def original_case(name,mutate=None,expected=None,posthoc=False,hook=None,options=None,live_mutate=None,postrun=False):
        m.binding_need(time.monotonic()-begin<120,'OFFLINE_BUDGET')
        adapter,auth,post,paths,c=fixture(**(options or {}))
        if posthoc:auth=post
        live=copy.deepcopy(auth);launches=[]
        if mutate:mutate(adapter,auth,paths,c)
        if live_mutate:live_mutate(live)
        def launch(ctx):launches.append(1);return dict(exitCode=0,fakeLauncher=True)
        actual=None;result=None
        try:
            result=m.binding_main(['--posthoc-fix20'] if posthoc else ['--execute'],adapter=adapter,authority=auth,
                                  authority_check=lambda:live,launcher=launch,self_paths=paths,
                                  hook=(lambda stage:hook(stage,adapter,auth)) if hook else None)
            if postrun:
                assert result['status']=='BLOCKED_POSTRUN_BINDING' and not result['bindingEstablished'] and len(launches)==1
            elif expected:
                raise AssertionError('Expected '+expected+'; gate returned success')
            else:
                assert result['bindingEstablished'] and len(launches)==(0 if posthoc else 1)
        except m.BindingFailure as error:
            actual=error.detail['code']
            assert expected==actual,(name,expected,error.detail)
            assert len(launches)==0,(name,'unexpected fake launch')
        cases.append(dict(case=name,passed=True,expectedReason=expected,actualReason=actual,fakeLaunches=len(launches),
                          mode='posthoc' if posthoc else 'execute',resultStatus=result.get('status') if result else None))
    def run_case(name,*args,**kwargs):
        try:original_case(name,*args,**kwargs)
        except Exception as exc:
            cases.append(dict(case=name,passed=False,error=str(exc),traceback=traceback.format_exc()))
    run_case('valid_raw_objects_complete_closure_HEAD_differs_and_only_explicit_WIP')
    run_case('valid_posthoc_archive_mapping_has_zero_launches',posthoc=True)
    def unknown(ad,a,p,c):
        ad.objects.pop(a['head'])
    run_case('arbitrary_nonexistent_40hex_object',unknown,'BLOCKED_OBJECT_MISSING')
    run_case('old_but_real_wrong_head',lambda ad,a,p,c:a.update(head=c['historical']['head']),'BLOCKED_AUTHORITY_HEAD')
    run_case('wrong_commit_object_type',lambda ad,a,p,c:ad.objects.__setitem__(a['head'],('blob',ad.objects[a['head']][1])),'BLOCKED_OBJECT_TYPE')
    run_case('raw_commit_hash_corruption',lambda ad,a,p,c:ad.objects.__setitem__(a['head'],('commit',ad.objects[a['head']][1]+b'x')),'BLOCKED_OBJECT_HASH')
    run_case('authorized_head_tree_mismatch',lambda ad,a,p,c:a.update(tree='f'*40),'BLOCKED_HEAD_TREE')
    run_case('source_single_byte_drift',lambda ad,a,p,c:ad.data.__setitem__('Assets/Outside92.cs',ad.data['Assets/Outside92.cs']+b'x'),'BLOCKED_FILE_IDENTITY')
    run_case('resource_single_byte_drift',lambda ad,a,p,c:ad.data.__setitem__('Assets/UI/Fixture.asset',ad.data['Assets/UI/Fixture.asset']+b'x'),'BLOCKED_FILE_IDENTITY')
    run_case('non92_source_change_is_not_ignored',lambda ad,a,p,c:ad.data.__setitem__('Assets/Outside92.cs',b'different'),'BLOCKED_FILE_IDENTITY')
    run_case('new_imported_file',lambda ad,a,p,c:ad.data.__setitem__('Assets/Added.cs',b'new'),'BLOCKED_INPUT_SET')
    run_case('deleted_imported_file',lambda ad,a,p,c:ad.data.pop('Assets/Outside92.cs'),'BLOCKED_INPUT_SET')
    run_case('new_runner_drift',lambda ad,a,p,c:ad.data.__setitem__(p['runner'],b'other code'),'BLOCKED_FILE_IDENTITY')
    run_case('auxiliary_R17_runner_drift',lambda ad,a,p,c:ad.data.__setitem__(next(x for x in c['requiredHistoricalInputs'] if x.endswith('/q4-correction-17/validation-tools/runner.py')),b'drift'),'BLOCKED_DEPENDENCY_UNBOUND')
    def tamper(ad,a,p,c):
        ad.data[c['deltaPath']]+=b' '
        static=m.json_bytes(ad.data[S+'static-result.json'],'fixture')
        for r in static['peerEvidence']:
            if r['path']==c['deltaPath']:r.update(m.raw_identity(ad.data[c['deltaPath']]))
        ad.data[S+'static-result.json']=jb(static)
    run_case('tampered_S_manifest_plus_self_filled_SHA',tamper,'BLOCKED_FILE_IDENTITY')
    run_case('R17_preflight_drift',lambda ad,a,p,c:ad.data.__setitem__(next(x for x in c['requiredHistoricalInputs'] if x.endswith('/preflight.json')),b'drift'),'BLOCKED_DEPENDENCY_UNBOUND')
    run_case('missing_dependency_chain',expected='BLOCKED_DEPENDENCY_UNBOUND',options=dict(omit_chain=True))
    run_case('anchored_external_CSV_drift',lambda ad,a,p,c:ad.data.__setitem__(c['externalDigestInputs'][0]['path'],b'drift'),'BLOCKED_FILE_IDENTITY')
    run_case('selector_drift',lambda ad,a,p,c:ad.data.__setitem__(c['selectorPath'],b'.*'),'BLOCKED_FILE_IDENTITY')
    run_case('activation_mismatch',lambda ad,a,p,c:ad.data.__setitem__(a['activationPath'],jb(dict(a['activation'],runId='foreign'))),'BLOCKED_ACTIVATION')
    run_case('path_traversal',expected='BLOCKED_PATH',options=dict(traversal=True))
    run_case('duplicate_self_mapping',expected='BLOCKED_DUPLICATE_MAPPING',options=dict(duplicate_mapping=True))
    run_case('symlink_input',lambda ad,a,p,c:ad.links.add('Assets/Outside92.cs'),'BLOCKED_SYMLINK')
    run_case('Git_replace',lambda ad,a,p,c:ad.metadata.update(replace=True),'BLOCKED_GIT_REPLACE')
    run_case('Git_object_environment_redirection',lambda ad,a,p,c:ad.environment.update(GIT_OBJECT_DIRECTORY='/foreign'),'BLOCKED_GIT_ENV')
    run_case('Git_alternate_objects_redirection',lambda ad,a,p,c:ad.metadata.update(redirected=True),'BLOCKED_GIT_REDIRECT')
    def prespawn(stage,ad,a):
        if stage=='before-spawn':ad.data['Assets/Outside92.cs']+=b'drift'
    run_case('immediately_before_spawn_drift',expected='BLOCKED_FILE_IDENTITY',hook=prespawn)
    run_case('authority_revoked',expected='BLOCKED_AUTHORITY_REVOKED',live_mutate=lambda live:live.update(active=False))
    run_case('authority_expired',expected='BLOCKED_AUTHORITY_EXPIRED',live_mutate=lambda live:live.update(expired=True))
    run_case('PR_moved',expected='BLOCKED_AUTHORITY_HEAD',live_mutate=lambda live:live.update(currentHead='e'*40))
    for label,leaf in [('XML','tests.xml'),('before','before.json'),('after','after.json'),('receipt','final-receipt.json')]:
        run_case('posthoc_'+label+'_identity_drift',lambda ad,a,p,c,leaf=leaf:ad.data.__setitem__(E+leaf,ad.data[E+leaf]+b' '),'BLOCKED_FILE_IDENTITY',posthoc=True)
    def afterrun(stage,ad,a):
        if stage=='after-run':ad.data['Assets/Outside92.cs']+=b'drift'
    run_case('after_run_drift_keeps_run_facts_but_invalidates_binding',hook=afterrun,postrun=True)
    elapsed=time.monotonic()-begin
    assert elapsed<=120
    return dict(status='OFFLINE_PASS',caseCount=len(cases),cases=cases,elapsedSeconds=elapsed,budgetSeconds=120,
                realUnityLaunches=0,realDotnetOrCompilerLaunches=0,gitWrites=0,networkCalls=0,
                fixture='Memory raw Git object store and file adapter; real binding_main/bind_inputs/verify_posthoc, not a mock runner.',
                runner=m.raw_identity(RUNNER.read_bytes()),testScript=m.raw_identity(HERE.read_bytes()))


# FIX22 genuine execute-path fixtures follow.

import builtins
import contextlib
import io
import types
from unittest.mock import patch

class FixtureSpawnReached(Exception):
    pass

class VirtualIO:
    """Only path/process I/O substitutions. Every semantic production function stays intact."""
    def __init__(self,adapter,auth=None,contract=None,mutate_before_spawn=None,argv_fault=None):
        self.adapter=adapter;self.auth=auth;self.contract=contract
        self.mutate_before_spawn=mutate_before_spawn;self.argv_fault=argv_fault
        self.fake_spawns=[];self.process_queries=0;self.writes=[];self.checkpoints=[]
        self.dirs=set();self.mutated=False
        self.root=str(ROOT)
        self.info=str(pathlib.PurePosixPath(m.EDITOR).parents[1]/'Info.plist')
        self.external={self.info:m.plistlib.dumps(dict(CFBundleVersion='2022.3.18f1'))}
    def key(self,p):
        value=str(p)
        if value==self.root:return ''
        if value.startswith(self.root+'/'):return value[len(self.root)+1:]
        return value
    def descendants(self,key):
        return sorted(set(p for p in list(self.adapter.data)+list(self.dirs) if p.startswith(key+'/')))
    def is_dir(self,key):
        return key in self.dirs or key in ('','/') or self.root.startswith(key+'/') or bool(self.descendants(key))
    def read(self,key):
        if key in self.external:return self.external[key]
        if key not in self.adapter.data:raise FileNotFoundError(key)
        return self.adapter.data[key]
    def profile(self,frame,event,arg):
        if event=='call' and frame.f_globals is m.__dict__:
            name=frame.f_code.co_name
            if name in ['execute_from_bound_inputs','execute','preflight','derive_execution_candidate',
                        'validate_execution_candidate','build_execution_argv','binding_checkpoint','bind_inputs']:
                self.checkpoints.append(name)
            elif name=='validate_fixed_progress':
                self.checkpoints.append(name+':'+str(frame.f_locals['stage']))
        return self.profile
    def __enter__(self):
        env=self;base=type(ROOT)
        class MemoryPath(base):
            def read_bytes(self):return env.read(env.key(self))
            def read_text(self,encoding=None,errors=None):return self.read_bytes().decode(encoding or 'utf-8',errors or 'strict')
            def exists(self,**kwargs):
                k=env.key(self);return k in env.adapter.data or k in env.external or env.is_dir(k)
            def is_symlink(self):return env.key(self) in env.adapter.links
            def is_file(self):return env.key(self) in env.adapter.data or env.key(self) in env.external
            def is_dir(self):return env.is_dir(env.key(self))
            def resolve(self,*args,**kwargs):return self
            def stat(self,*args,**kwargs):
                return types.SimpleNamespace(st_size=len(self.read_bytes()),st_ino=1,st_mtime_ns=0)
            def mkdir(self,*args,**kwargs):
                k=env.key(self)
                if self.exists() and not kwargs.get('exist_ok',False):raise FileExistsError(k)
                env.dirs.add(k)
            def rglob(self,pattern):
                assert pattern=='*'
                return iter(MemoryPath(env.root+'/'+p) for p in env.descendants(env.key(self)))
            def iterdir(self):
                k=env.key(self);prefix=k+'/' if k else ''
                names={p[len(prefix):].split('/')[0] for p in env.descendants(k)}
                return iter(self/n for n in sorted(names))
            def write_text(self,value,encoding=None,errors=None,**kwargs):
                with self.open('w',encoding=encoding or 'utf-8') as f:return f.write(value)
            def open(self,mode='r',*args,**kwargs):
                key=env.key(self)
                if key.endswith('/editor.stdout.log') and not env.mutated:
                    env.mutated=True
                    if env.mutate_before_spawn:env.mutate_before_spawn(env.adapter,env.auth)
                writing=any(c in mode for c in 'wxa+')
                if writing:
                    assert key.startswith(m.FIX22_OUTPUT+'/'),('out-of-scope fixture write',key)
                    if 'x' in mode and key in env.adapter.data:raise FileExistsError(key)
                    env.writes.append(key)
                    if 'x' in mode or 'w' in mode:env.adapter.data[key]=b''
                data=env.read(key)
                binary='b' in mode
                stream=io.BytesIO(data) if binary else io.StringIO(data.decode(kwargs.get('encoding') or 'utf-8'))
                original_close=stream.close
                def close():
                    if writing and not stream.closed:
                        value=stream.getvalue();env.adapter.data[key]=value if binary else value.encode('utf-8')
                    original_close()
                stream.close=close
                if 'a' in mode:stream.seek(0,2)
                return stream
        self.MemoryPath=MemoryPath
        def run_process(argv,**kwargs):
            assert argv==['/bin/ps','-axo','pid=,ppid=,comm='],('unexpected subprocess',argv)
            self.process_queries+=1
            return types.SimpleNamespace(stdout='',stderr='',returncode=0)
        def fake_spawn(argv,**kwargs):
            actual=list(argv)
            if self.argv_fault:
                self.argv_fault(actual)
                # Fault injection at the final spawn I/O seam still uses the production gate.
                m.validate_fixed_progress(m.BINDING_STATE,self.auth,self.contract,'spawn',actual)
            self.fake_spawns.append(actual)
            raise FixtureSpawnReached()
        self.stack=contextlib.ExitStack()
        for name in ['ROOT','SOURCE_ONLY','R17','R15','R18','R19','PREDECESSOR','R17_RUNNER','FONT']:
            self.stack.enter_context(patch.object(m,name,MemoryPath(str(getattr(m,name)))))
        self.stack.enter_context(patch.object(m,'OUT',MemoryPath(str(ROOT/m.FIX22_OUTPUT))))
        self.stack.enter_context(patch.object(m,'__file__',str(ROOT/m.FIX22_OUTPUT/'validation-tools/runner.py')))
        self.stack.enter_context(patch.object(m.pathlib,'Path',MemoryPath))
        self.stack.enter_context(patch.object(builtins,'open',lambda path,*a,**kw:MemoryPath(path).open(*a,**kw)))
        self.stack.enter_context(patch.object(m.subprocess,'run',run_process))
        self.stack.enter_context(patch.object(m.subprocess,'Popen',fake_spawn))
        self.stack.enter_context(patch.object(m.shutil,'disk_usage',lambda p:types.SimpleNamespace(free=8*1024**3)))
        self.stack.enter_context(patch.object(m.os,'kill',lambda *args:(_ for _ in ()).throw(AssertionError('signal forbidden'))))
        self.old_profile=sys.getprofile();sys.setprofile(self.profile)
        self.stack.enter_context(contextlib.redirect_stdout(io.StringIO()))
        return self
    def __exit__(self,*exc):
        sys.setprofile(self.old_profile)
        return self.stack.__exit__(*exc)

def run_execution_cases(begin):
    cases=[]
    def one(name,expected=None,scenario=None,mutate=None,late=None,argv_fault=None):
        observed=None;detail=None;error=None;vio=None
        try:
            assert time.monotonic()-begin<120
            ad,a,_,paths,c=fixture(full=True,scenario=scenario)
            live=copy.deepcopy(a)
            if mutate:mutate(ad,a,c)
            # Initial binding remains the real raw-object and full-closure code.
            ctx,proof=m.bind_inputs(ad,c,a,live,'execute',paths)
            historical={p:data for p,data in ad.data.items() if p.startswith((S,E,A.replace('fix22','fix21')))}
            with VirtualIO(ad,a,c,late,argv_fault) as vio:
                try:
                    m.execute_from_bound_inputs(ctx,a,lambda:live,ad,c)
                except FixtureSpawnReached:
                    observed='FAKE_SPAWN'
            assert all(ad.data[p]==data for p,data in historical.items()),'historical evidence changed'
        except m.BindingFailure as exc:
            observed=exc.detail['code'];detail=exc.detail
        except Exception as exc:
            observed=type(exc).__name__;error=traceback.format_exc()
        wanted=expected or 'FAKE_SPAWN'
        checkpoints=vio.checkpoints if vio else []
        launches=len(vio.fake_spawns) if vio else 0
        passed=observed==wanted and launches==(0 if expected else 1)
        if not expected:
            required=['execute_from_bound_inputs','execute','preflight','derive_execution_candidate',
                      'validate_execution_candidate','build_execution_argv','binding_checkpoint','validate_fixed_progress:spawn']
            passed=passed and all(p in checkpoints for p in required)
        cases.append(dict(case=name,passed=passed,expectedReason=wanted,actualReason=observed,detail=detail,
                          fakeLaunches=launches,productionCheckpoints=checkpoints,traceback=error,
                          fileWrites=sorted(set(vio.writes)) if vio else [],noRealOutputDirectory=True))
    def update_activation(ad,a,**values):
        a['activation'].update(values);ad.data[a['activationPath']]=jb(a['activation'])
    one('FIX22_full_production_preflight_candidate_argv_and_one_fake_spawn')
    for label,value in [
        ('FIX20','TestArtifacts/FightMatch/UGUI-01/q4-correction-20/test-events.jsonl'),
        ('other_basename','TestArtifacts/FightMatch/UGUI-01/another-root/test-events.jsonl'),
        ('same_basename_different_parent','Other/UGUI-01/q4-correction-22/test-events.jsonl'),
        ('prefix_lookalike','TestArtifacts/FightMatch/UGUI-01/q4-correction-22-extra/test-events.jsonl'),
        ('relative',m.FIX22_PROGRESS),
        ('dotdot',str(ROOT)+'/TestArtifacts/FightMatch/UGUI-01/../UGUI-01/q4-correction-22/test-events.jsonl'),
        ('case',str(ROOT)+'/'+m.FIX22_PROGRESS.replace('FightMatch','fightmatch'))]:
        supplied=value if label in ('relative','dotdot','case') else str(ROOT)+'/'+value
        one('progress_path_'+label,'BLOCKED_PROGRESS_CONTRACT',
            mutate=lambda ad,a,c,p=supplied:a.update(progressPath=p))
    one('output_root_mismatch','BLOCKED_OUTPUT_ROOT',mutate=lambda ad,a,c:a.update(outputRoot=m.FIX22_OUTPUT+'x'))
    one('activation_progress_mismatch','BLOCKED_ACTIVATION_PATH',
        mutate=lambda ad,a,c:update_activation(ad,a,progressPath=str(ROOT/'Elsewhere/test-events.jsonl')))
    one('activation_location_mismatch','BLOCKED_ACTIVATION_PATH',
        mutate=lambda ad,a,c:(ad.data.__setitem__(m.FIX22_OUTPUT+'/wrong-activation.json',ad.data[a['activationPath']]),
                             a.update(activationPath=m.FIX22_OUTPUT+'/wrong-activation.json')))
    one('contract_progress_mismatch','BLOCKED_PROGRESS_CONTRACT',
        scenario=lambda f,t,c,d,o:c.update(progressPath=m.FIX22_PROGRESS+'x'))
    one('contract_output_mismatch','BLOCKED_OUTPUT_ROOT',
        scenario=lambda f,t,c,d,o:c.update(outputRoot=m.FIX22_OUTPUT+'x'))
    one('not_create_new_reservation','BLOCKED_OUTPUT_RESERVATION',
        mutate=lambda ad,a,c:update_activation(ad,a,preparedNew=False))
    one('wrong_reservation_run','BLOCKED_OUTPUT_RESERVATION',
        mutate=lambda ad,a,c:update_activation(ad,a,preparationRunId='earlier-run'))
    one('already_used_output_root','BLOCKED_OUTPUT_ROOT_NOT_FRESH',
        mutate=lambda ad,a,c:ad.data.__setitem__(m.FIX22_OUTPUT+'/old-run.json',b'{}'))
    one('existing_nonempty_progress','BLOCKED_PROGRESS_NOT_NEW',
        mutate=lambda ad,a,c:ad.data.__setitem__(m.FIX22_PROGRESS,b'old events'))
    for label,path in [('parent',m.FIX22_OUTPUT),('ancestor','TestArtifacts/FightMatch'),('file',m.FIX22_PROGRESS)]:
        one('progress_symlink_'+label,'BLOCKED_OUTPUT_SYMLINK',mutate=lambda ad,a,c,p=path:ad.links.add(p))
    def callback_scenario(rewrite):
        def mutate(f,t,c,d,o):
            f[m.CALLBACK_PATH]=rewrite(f[m.CALLBACK_PATH]);t[m.CALLBACK_PATH]=f[m.CALLBACK_PATH]
        return mutate
    one('callback_full_constant_disagrees','BLOCKED_CALLBACK_PROGRESS_PATH',
        scenario=callback_scenario(lambda b:b.replace(m.FIX22_PROGRESS.encode(),b'Other/q4-correction-22/test-events.jsonl')))
    one('runner_only_change_old_callback','BLOCKED_CALLBACK_PROGRESS_PATH',
        scenario=lambda f,t,c,d,o:(f.__setitem__(m.CALLBACK_PATH,o['oldCallbackBytes']),t.__setitem__(m.CALLBACK_PATH,o['oldCallbackBytes'])))
    one('callback_other_source_edit','BLOCKED_CALLBACK_DELTA',
        scenario=callback_scenario(lambda b:b+b'\n// unrelated edit\n'))
    one('argv_missing_progress','BLOCKED_PROGRESS_ARGUMENT',
        argv_fault=lambda argv:argv.__delitem__(slice(argv.index('-fightMatchTestProgress'),argv.index('-fightMatchTestProgress')+2)))
    one('argv_duplicate_progress','BLOCKED_PROGRESS_ARGUMENT',
        argv_fault=lambda argv:argv.extend(['-fightMatchTestProgress',str(ROOT/m.FIX22_PROGRESS)]))
    one('argv_progress_disagrees','BLOCKED_PROGRESS_ARGUMENT',
        argv_fault=lambda argv:argv.__setitem__(argv.index('-fightMatchTestProgress')+1,str(ROOT/'Elsewhere/test-events.jsonl')))
    for field in ['addedFiles','protectedCandidate','diagnosticCandidate']:
        one('stale_'+field,'BLOCKED_DERIVED_CANDIDATE',
            scenario=lambda f,t,c,d,o,k=field:d['derivedCandidate'].__setitem__(k,copy.deepcopy(o[k])))
    one('delta_new_SHA_not_new_blob','BLOCKED_CALLBACK_NEW_IDENTITY',
        scenario=lambda f,t,c,d,o:d['changes'][0]['after'].update(sha256='0'*64))
    one('second_declared_source_delta','BLOCKED_EXECUTION_DELTA_SCOPE',
        scenario=lambda f,t,c,d,o:d['changes'].append(copy.deepcopy(d['changes'][0])))
    one('second_source_changed_in_new_head','BLOCKED_CURRENT_CANDIDATE',
        scenario=lambda f,t,c,d,o:(f.__setitem__('Assets/Outside92.cs',b'changed second source'),t.__setitem__('Assets/Outside92.cs',b'changed second source')))
    one('original_callback_meta_deleted','BLOCKED_CURRENT_CANDIDATE',
        scenario=lambda f,t,c,d,o:(f.pop(m.CALLBACK_PATH+'.meta'),t.pop(m.CALLBACK_PATH+'.meta')))
    one('historical_manifest_tampered','BLOCKED_FILE_IDENTITY',
        mutate=lambda ad,a,c:ad.data.__setitem__(c['deltaPath'],ad.data[c['deltaPath']]+b' '))
    one('source_drift_after_binding_immediately_before_spawn','BLOCKED_FILE_IDENTITY',
        late=lambda ad,a:ad.data.__setitem__(m.CALLBACK_PATH,ad.data[m.CALLBACK_PATH]+b' '))
    one('progress_drift_immediately_before_spawn','BLOCKED_PROGRESS_NOT_EMPTY',
        late=lambda ad,a:ad.data.__setitem__(m.FIX22_PROGRESS,b'unexpected event'))
    one('authorized_path_drift_immediately_before_spawn','BLOCKED_PROGRESS_CONTRACT',
        late=lambda ad,a:a.update(progressPath=str(ROOT/'Elsewhere/test-events.jsonl')))
    one('excluded_WIP_drift_immediately_before_spawn','BLOCKED_WIP_ACTIVATION_SNAPSHOT',
        late=lambda ad,a:ad.data.__setitem__(a['wipSnapshot'][0]['path'],b'new independent WIP'))
    return cases

def run_event_cases():
    results=[]
    def rows(cross):
        data=[]
        def add(kind,name,result=''):
            data.append(dict(seq=len(data)+1,utc='2026-10-01T00:00:00Z',pid=77,
                processStartUtc='2026-10-01T00:00:00Z',domain=('%032x'%(len(data)//4 if cross else 1)),
                source='editor' if kind.startswith('Run') else 'attribute',eventName=kind,
                testId=name,fullName=name,result=result,duration=0))
        add('RunStarted','Suite')
        for i in range(12):add('TestStarted','Test'+str(i));add('TestFinished','Test'+str(i),'Passed')
        add('RunFinished','Suite','Passed')
        return data
    for label,cross,fault in [('same_domain',False,None),('cross_domain',True,None),
                              ('foreign_pid',True,lambda r:r[5].update(pid=88)),
                              ('sequence_gap',True,lambda r:r[5].update(seq=9)),
                              ('invalid_domain',True,lambda r:r[5].update(domain='wrong'))]:
        events=rows(cross)
        if fault:fault(events)
        ad=MemoryAdapter({m.FIX22_PROGRESS:b''.join(jb(r) for r in events)},{})
        with VirtualIO(ad) as vio:trace=m.event_trace(77,final=True)
        passed=bool(trace['errors']) if fault else (not trace['errors'] and trace['rows']==26 and trace['startedCount']==trace['finishedCount']==12 and
            trace['runStarted']==trace['runFinished']==1 and not trace['unmatchedStarted'] and
            len(trace['domains'])==(7 if cross else 1))
        results.append(dict(case='event_'+label,passed=passed,errors=trace['errors'],domainCount=len(trace['domains']),fakeLaunches=0,
                            limitation='Python event parser only; no C# callback execution.'))
    return results

def run_all():
    begin=time.monotonic()
    original=run_original35()
    cases=original['cases']+run_execution_cases(begin)+run_event_cases()
    elapsed=time.monotonic()-begin
    return dict(status='OFFLINE_PASS' if all(c['passed'] for c in cases) and elapsed<=120 else 'OFFLINE_FAIL',
        caseCount=len(cases),original35Count=len(original['cases']),cases=cases,elapsedSeconds=elapsed,budgetSeconds=120,
        realUnityLaunches=0,realDotnetOrCompilerLaunches=0,gitWrites=0,networkCalls=0,
        fixture='Original 35 binding assertions retained. FIX22 cases call actual execute_from_bound_inputs, preflight, candidate and argv gates; only virtual filesystem/process I/O and final Popen are substituted. Malformed argv is injected at final spawn I/O and rechecked by the production gate before fake-launch counting.',
        limitations=['Synthetic raw Git commits/trees prove offline binding mechanics, not a newly published PR head.',
                     'No C# compilation, registration, or runtime callback execution; exact12 integration remains separately gated.'],
        runner=m.raw_identity(RUNNER.read_bytes()),testScript=m.raw_identity(HERE.read_bytes()))

if __name__=='__main__':
    start=time.monotonic()
    try:
        result=run_all()
    except Exception as error:
        result=dict(status='OFFLINE_FAIL',elapsedSeconds=time.monotonic()-start,realUnityLaunches=0,
                    errorType=type(error).__name__,error=str(error),traceback=traceback.format_exc())
    target=HERE.parents[1]/'offline-result.json'
    # First run creates this leaf. A single authorized repair rerun may append a second attempt.
    if target.exists():
        previous=json.loads(target.read_text())
        attempts=previous.get('attempts',[previous])
        assert len(attempts)==1,'Only one repair rerun is authorized.'
        result=dict(status=result['status'],attempts=attempts+[result],realUnityLaunches=0)
    with target.open('w',encoding='utf-8') as f:json.dump(result,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps({k:result[k] for k in result if k in ['status','caseCount','elapsedSeconds','error']}))
    sys.exit(0 if result['status']=='OFFLINE_PASS' else 3)
