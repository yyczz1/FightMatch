
"""FIX21 offline fixtures: real binding/main functions; only object/file I/O and launch are in memory."""
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
spec=importlib.util.spec_from_file_location('fix21_runner',RUNNER)
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
CONTRACT_PATH='TestArtifacts/FightMatch/UGUI-01/q4-correction-21-source/binding-contract.json'
CONTRACT=json.loads((ROOT/CONTRACT_PATH).read_text())
N='TestArtifacts/FightMatch/UGUI-01/q4-correction-21-source/'
A='docs/versioning/2026-10-01-ugui-pr/fix21/'
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
def fixture(omit_chain=False,traversal=False,duplicate_mapping=False):
    c=copy.deepcopy(CONTRACT);files={};objects={};tracked={}
    for proof in c['dependencyProof']:
        data=(ROOT/proof['path']).read_bytes();files[proof['path']]=data;tracked[proof['path']]=data
    for p in c['externalTreeInputs']:files[p]=b'fixture external input';tracked[p]=files[p]
    for dependency in c.get('externalDigestInputs',[]):files[dependency['path']]=(ROOT/dependency['path']).read_bytes()
    files['Assets/Outside92.cs']=b'class Outside92 {}';tracked['Assets/Outside92.cs']=files['Assets/Outside92.cs']
    files['Assets/UI/Fixture.asset']=b'fixture resource';tracked['Assets/UI/Fixture.asset']=files['Assets/UI/Fixture.asset']
    for p in ['Tools/Tracked.ps1','Config/Tracked.json','Generated/Tracked.json']:
        files[p]=b'tracked';tracked[p]=files[p]
    for p in c['excludedWip']:files[p]=b'independent LOC WIP'
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
    files[S+'delta-manifest.json']=jb(delta)
    peers=[row(p,files[p]) for p in sorted(files) if p.startswith(S)]
    static=jb(dict(peerEvidence=peers))
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
    files[CONTRACT_PATH]=jb(c);tracked[A+'binding-contract.json']=files[CONTRACT_PATH]
    newtid=tree(objects,tracked);newhid=commit(objects,newtid,'new binding source fixture')
    authority=dict(runId='fixture-run',mode='SINGLE_GRAPHICS_EXACT12',repo='yyczz1/FightMatch',pr=1,head=newhid,tree=newtid,
                   currentHead=newhid,active=True,owner=dict(threadId='fixture-C',hostId='local',turnId='fixture-turn'))
    authority['activationPath']='TestArtifacts/FightMatch/UGUI-01/fixture-run/activation.json'
    authority['activation']=dict(runId=authority['runId'],owner=authority['owner'],prBinding=dict(head=newhid,tree=newtid))
    files[authority['activationPath']]=jb(authority['activation'])
    selfpaths=dict(runner=N+'validation-tools/runner.py',tests=N+'validation-tools/test_review_binding.py',contract=CONTRACT_PATH)
    post=dict(authority,mode='FIX21_SOURCE_AND_POSTHOC_ONLY',head=hid,tree=tid,currentHead=hid,receipt=c['receiptIdentity'])
    return MemoryAdapter(files,objects),authority,post,selfpaths,c

def run_all():
    begin=time.monotonic();cases=[]
    def run_case(name,mutate=None,expected=None,posthoc=False,hook=None,options=None,live_mutate=None,postrun=False):
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
