#!/usr/bin/env python3
"""M04 offline fixtures only. Never import or execute a runner module."""
import ast,copy,datetime,difflib,hashlib,json,os,pathlib,sys,time,traceback
START=time.monotonic()
E=pathlib.Path(__file__).resolve().parent
R=E.parents[3]
M03=R/'TestArtifacts/FightMatch/HOST-NEXT-001/M03'
OWNER={'thread':'01a0fdbc-bf1e-7780-8f7f-dec13d6d590c','host':'local','turn':'01a1178c-c850-7df1-a100-c1517709d229'}
ALLOW={'runner.py','replay-check.py','replay-results.json','correction.patch','receipt.json'}
external_attempts=[]
def audit(event,args):
    if event in {'subprocess.Popen','os.system','os.kill','os.killpg','socket.connect','socket.bind','os.remove','os.rename','os.mkdir','os.rmdir'}:
        external_attempts.append(event); raise RuntimeError('Offline replay forbids '+event)
    if event=='open':
        path,mode,flags=args
        if flags & (os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
            if not isinstance(path,(str,bytes,os.PathLike)) or pathlib.Path(path).parent.resolve()!=E or pathlib.Path(path).name not in ALLOW:
                external_attempts.append('write outside five leaves'); raise RuntimeError('Offline write boundary')
sys.addaudithook(audit)
def require(value,why):
    if not value: raise AssertionError(why)
def identity(path):
    data=path.read_bytes(); return {'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}
def write(name,value):
    require(name in ALLOW,'five-leaf whitelist')
    with (E/name).open('x') as f: json.dump(value,f,ensure_ascii=False,indent=2); f.write('\n')
def functions(tree):
    return {x.name:x for x in tree.body if isinstance(x,ast.FunctionDef)}
def named(call,name):
    return isinstance(call,ast.Call) and isinstance(call.func,ast.Name) and call.func.id==name
class NormalizeCalls(ast.NodeTransformer):
    def visit_Call(self,node):
        node=self.generic_visit(node)
        if named(node,'monitor'): node.args=node.args[:1]
        elif named(node,'snapshot_consumers'):
            node=ast.Call(func=ast.Name(id='consumer_guard',ctx=ast.Load()),args=[ast.Name(id='rows',ctx=ast.Load())],keywords=[])
        return node
report={'task':'HOST-NEXT-001/M04-source','owner':OWNER,'status':'RUNNING','round':1,'cases':[],'static':{},'fixturesAreNotRawPs':True,'nativeRuns':0,'realPsCalls':0,'adbCommands':0,'realSignals':0,'sourceSyncs':0}
failure=None
try:
    require(not (E/'replay-results.json').exists() and not (E/'receipt.json').exists(),'replay first unused round')
    oldpath=M03/'runner.py'; newpath=E/'runner.py'; probe=M03/'S/FightMatchHostFirstFrameProbe.cs'
    require(identity(oldpath)=={'bytes':41254,'sha256':'e2c7c25f1b79902197b1c109c85dc033acd4a7d6e595b79f3c14f78920d93ed0'},'fixed M03 runner')
    require(identity(probe)=={'bytes':40558,'sha256':'11a0dce7bfa6f11e0b2dda3a0397b3e17196cb26c106fce1d648bd8d6d55d920'},'unchanged M03 probe')
    oldtext=oldpath.read_text(); newtext=newpath.read_text(); before=ast.parse(oldtext); after=ast.parse(newtext)
    compile(after,str(newpath),'exec'); compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec')
    oldfn=functions(before); newfn=functions(after); changed={name for name in oldfn if ast.dump(oldfn[name])!=ast.dump(newfn[name])}
    require(changed=={'monitor','closure','run_stage'} and set(newfn)-set(oldfn)=={'snapshot_consumers'},'only monitor/callers and narrow snapshot helper')
    require([ast.dump(x) for x in before.body if not isinstance(x,ast.FunctionDef)]==[ast.dump(x) for x in after.body if not isinstance(x,ast.FunctionDef)],'top-level identities/budgets/entry unchanged')
    for name in changed:
        normalized=copy.deepcopy(newfn[name])
        if name=='monitor':
            normalized.args.args=[x for x in normalized.args.args if x.arg not in {'rows','stage','rootpid'}]
        if name=='closure':
            finalIndex=next(i for i,x in enumerate(normalized.body) if isinstance(x,ast.Expr) and named(x.value,'monitor') and x.value.args[0].value=='closure-final')
            require(ast.dump(normalized.body[finalIndex-1])==ast.dump(ast.parse('rows=ps()').body[0]),'fresh final snapshot at caller')
            require(ast.dump(normalized.body[finalIndex+1])==ast.dump(ast.parse('remaining=[p for p in owned if alive(rows,p)]').body[0]),'remaining uses same final snapshot')
            del normalized.body[finalIndex+1]; del normalized.body[finalIndex-1]
        normalized=NormalizeCalls().visit(normalized)
        if name=='monitor':
            for call in ast.walk(normalized):
                if named(call,'consumer_guard'): call.args=[ast.Call(func=ast.Name(id='ps',ctx=ast.Load()),args=[],keywords=[])]
        require(ast.dump(normalized)==ast.dump(oldfn[name]),'all other logic unchanged in '+name)
    require(not any(named(x,'ps') for name in ['monitor','snapshot_consumers'] for x in ast.walk(newfn[name])),'no second ps inside monitor/classification helper')
    callsites={}
    for caller in ['run_stage','closure']:
        for call in ast.walk(newfn[caller]):
            if named(call,'monitor'):
                phase=call.args[0].value
                require(len(call.args)==4 and isinstance(call.args[1],ast.Name) and call.args[1].id=='rows','explicit snapshot at '+phase)
                callsites[phase]=copy.deepcopy(call)
    require(set(callsites)=={'running','natural-closure','term-confirmation','closure-final'},'four actual monitor call sites')
    termCall=next(copy.deepcopy(x) for x in ast.walk(newfn['closure']) if named(x,'snapshot_consumers'))
    require(isinstance(termCall.args[0],ast.Name) and termCall.args[0].id=='rows','TERM preclassification same snapshot')
    report['static']={'changedFunctions':sorted(changed),'newFunction':'snapshot_consumers','allOtherFunctionsAstEqual':True,'identitiesAndTopLevelAstEqual':True,'changedFunctionsNormalizeToOnlyApprovedDelta':True,'actualMonitorCallsites':sorted(callsites),'termSnapshotExplicit':True,'observationPassedUnchanged':ast.dump(oldfn['observation_passed'])==ast.dump(newfn['observation_passed']),'probeUnchanged':identity(probe),'runnerNonblankLines':sum(bool(x.strip()) for x in newtext.splitlines())}
    procpath=M03/'process-after.json'; eventpath=M03/'process-events.jsonl'; actpath=M03/'activation.json'
    process=json.loads(procpath.read_text()); activation=json.loads(actpath.read_text()); events=[json.loads(x) for x in eventpath.read_text().splitlines()]
    root=copy.deepcopy(next(x for x in process['owned'] if x['pid']==36300)); child=copy.deepcopy(next(x for x in process['owned'] if x['pid']==36391))
    require(child['ppid']==root['pid'] and child['rootPid']==root['pid'] and root['argv']==activation['stages'][0]['argv'] and root['cwd']==activation['cwd'],'saved actual parent/argv/cwd')
    oldReceipt=json.loads((M03/'receipt.json').read_text())
    require(oldReceipt['status']=='FAILED' and len(oldReceipt['evidence'])==42,'M03 failure evidence preserved')
    originalIdentities={name:identity(M03/name) for name in oldReceipt['evidence']}
    require(originalIdentities==oldReceipt['evidence'],'M03 42 indexed leaves exact')
    originalReceiptIdentity=identity(M03/'receipt.json')
    rowkeys=('ppid','start','stat','exe')
    rootrow={k:root[k] for k in rowkeys}; childrow={k:child[k] for k in rowkeys}
    report['sourceFacts']={'processAfter':{'path':str(procpath),**identity(procpath)},'events':{'path':str(eventpath),**identity(eventpath)},'activation':{'path':str(actpath),**identity(actpath)},'root':root,'newChild':child,'monitorFailure':process['monitorErrors'][0],'laterRegistration':next(x for x in events if x['kind']=='owned_discovered' and x['process']['pid']==child['pid'])}
    report['fixtureDefinition']='Each fixture is a minimal derived subset of saved M03 facts (root36300 and child36391), not a complete or fresh ps listing. Foreign-parent/start/exe/root-identity/incomplete-details variants are explicitly synthetic.'
    selected=['check','alive','register','discover','consumer_guard','recorded_chain','monitor']
    class Clock:
        def monotonic(self): return 100.0
    def replay(name,expected,phase='running',mode='new',variant=None,last=0.0,boundary_failure=None):
        rows={root['pid']:copy.deepcopy(rootrow),child['pid']:copy.deepcopy(childrow)}; initial={root['pid']:copy.deepcopy(root)}
        detailfacts={x['pid']:{k:copy.deepcopy(x[k]) for k in ('argv','cwd','cwdProbeExit')} for x in [root,child]}
        if variant=='foreign-parent': rows[child['pid']]['ppid']=999999
        if variant=='pid-reuse': initial[child['pid']]=copy.deepcopy(child); rows[child['pid']]['start']='Thu Oct 8 02:01:16 2026'
        if variant=='exe-mismatch': initial[child['pid']]=copy.deepcopy(child); rows[child['pid']]['exe']='/fixture/unrelated/bee_backend'
        if variant=='root-argv-mismatch': initial[root['pid']]['argv']=root['argv']+['fixture-unapproved']
        if variant=='root-cwd-mismatch': initial[root['pid']]['cwd']='/fixture/unapproved-project'
        if variant=='incomplete-child-argv': detailfacts[child['pid']]['argv']=[]
        if variant=='incomplete-child-cwd': detailfacts[child['pid']]['cwd']=None; detailfacts[child['pid']]['cwdProbeExit']=1
        boundary={'ps':0,'details':[],'resources':0,'projection':0}; emitted=[]; traces=[]
        def fixture_ps(): boundary['ps']+=1; return rows
        def fixture_details(pid): boundary['details'].append(pid); return copy.deepcopy(detailfacts[pid])
        def fixture_check(which):
            boundary[which]+=1
            if boundary_failure==which: raise RuntimeError('fixture '+which+' failure')
        env={'pathlib':pathlib,'json':json,'time':Clock(),'owned':initial,'A':activation,'P':pathlib.Path(activation['cwd']),'utc':lambda:'fixture-time-not-native','details':fixture_details,'ps':fixture_ps,'event':lambda kind,**kw:emitted.append({'kind':kind,**kw}),'adb_exception':lambda snapshot:-1,'sdk_adb_exception':lambda snapshot:set(),'resources':lambda:fixture_check('resources'),'projection_guard':lambda:fixture_check('projection'),'last_monitor':last,'monitor_errors':[],'monitor_cycles':[],'rows':rows,'stage':'P','sid':'P','rootpid':root['pid']}
        sourcefn=oldfn if mode=='old' else newfn; names=selected+([] if mode=='old' else ['snapshot_consumers'])
        module=ast.Module(body=[copy.deepcopy(sourcefn[x]) for x in names],type_ignores=[])
        codefile=str(oldpath if mode=='old' else newpath)
        exec(compile(ast.fix_missing_locations(module),codefile,'exec'),env)
        def trace(frame,event,arg):
            if event=='call' and frame.f_code.co_filename==codefile and frame.f_code.co_name in {'discover','snapshot_consumers','consumer_guard'}:
                traces.append({'function':frame.f_code.co_name,'usesSuppliedClassificationSnapshot':frame.f_locals.get('rows') is rows})
        raised=None
        sys.setprofile(trace)
        try:
            if mode=='old':
                env['discover']({root['pid']:copy.deepcopy(rootrow)},'P',root['pid'])
                env['monitor']('running',strict=True)
            else:
                node=termCall if mode=='term' else callsites[phase]
                exec(compile(ast.fix_missing_locations(ast.Module(body=[ast.Expr(value=copy.deepcopy(node))],type_ignores=[])),str(newpath)+'::actual-callsite','exec'),env)
        except Exception as ex: raised=str(ex)
        finally: sys.setprofile(None)
        actual='REJECT' if raised or env['monitor_errors'] else 'SKIP' if mode=='new' and not env['monitor_cycles'] else 'ACCEPT'
        item={'name':name,'fixture':True,'mode':mode,'phase':phase if mode!='term' else 'term-preclassification','variant':variant,'expected':expected,'actual':actual,'passed':actual==expected,'raised':raised,'monitorErrors':env['monitor_errors'],'boundaryCalls':boundary,'actualFunctionTrace':traces,'childRegistered':child['pid'] in env['owned'],'childRecordedIdentity':env['owned'].get(child['pid'])}
        report['cases'].append(item); require(item['passed'],'replay '+name)
        if mode!='old':
            require(all(x['usesSuppliedClassificationSnapshot'] for x in traces),'same object snapshot at '+name)
            require(boundary['ps']==0,'no nested ps on replay path '+name)
        if expected=='ACCEPT':
            require(item['childRegistered'] and all(item['childRecordedIdentity'][k]==child[k] for k in ['pid','start','exe','ppid','argv','cwd','rootPid']),'complete saved child identity/parent chain '+name)
        if expected=='SKIP': require(boundary['resources']==0 and boundary['projection']==0 and not traces,'2s cadence unchanged')
        if boundary_failure: require(boundary['resources']==1 and boundary['projection']==1 and any(x['function']=='consumer_guard' for x in traces),'all protection checks retained after failure')
    replay('old-two-snapshot-race-reproduces-rejection','REJECT',mode='old')
    for phase in callsites:
        replay(phase+'-saved-new-child-accepted','ACCEPT',phase=phase)
        replay(phase+'-same-name-foreign-parent-rejected','REJECT',phase=phase,variant='foreign-parent')
    for variant in ['pid-reuse','exe-mismatch','root-argv-mismatch','root-cwd-mismatch','incomplete-child-argv','incomplete-child-cwd']:
        replay(variant+'-rejected','REJECT',variant=variant)
    replay('TERM-same-snapshot-discovers-legitimate-child','ACCEPT',mode='term')
    replay('TERM-same-name-foreign-parent-rejected','REJECT',mode='term',variant='foreign-parent')
    replay('under-two-seconds-skips-monitor','SKIP',last=99)
    replay('exactly-two-seconds-runs-monitor','ACCEPT',last=98)
    replay('resource-failure-retained-and-other-checks-run','REJECT',boundary_failure='resources')
    replay('source-failure-retained-and-other-checks-run','REJECT',boundary_failure='projection')
    require(not external_attempts,'no forbidden external attempts')
    require(identity(M03/'receipt.json')==originalReceiptIdentity and all(identity(M03/name)==v for name,v in originalIdentities.items()),'M03 evidence remains exact')
    require(identity(probe)==report['static']['probeUnchanged'],'probe remains exact')
    report['M03EvidenceUnchanged']=True
    report['status']='SOURCE_REPLAY_PASS'
except BaseException as ex:
    failure=str(ex); report['status']='SOURCE_REPLAY_FAILED'; report['failure']=failure; report['traceback']=traceback.format_exc()
finally:
    report['elapsedMechanicalSeconds']=time.monotonic()-START
    report['externalAttempts']=external_attempts
    if report['elapsedMechanicalSeconds']>30: report['status']='SOURCE_REPLAY_FAILED'; report['failure']='30 second mechanical budget exceeded'
    sourceold=(M03/'runner.py').read_text(); sourcenew=(E/'runner.py').read_text()
    patch=''.join(difflib.unified_diff(sourceold.splitlines(True),sourcenew.splitlines(True),fromfile='M03/runner.py',tofile='M04-source/runner.py'))
    with (E/'correction.patch').open('x') as f: f.write(patch)
    write('replay-results.json',report)
    inputs={str(path):identity(path) for path in [M03/'runner.py',M03/'S/FightMatchHostFirstFrameProbe.cs',M03/'receipt.json',M03/'process-after.json',M03/'process-events.jsonl',M03/'activation.json',R/'docs/team/2026-09-30/central-host-next-001-m04-source.md']}
    receipt={'task':report['task'],'status':report['status'],'owner':OWNER,'soleCompletionReceiver':'01a0e401-511d-79f2-b47f-3ab0ade1681b/local','finishedUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'inputs':inputs,'evidence':{name:identity(E/name) for name in ['runner.py','replay-check.py','replay-results.json','correction.patch']},'replayRound':1,'passedCases':sum(x['passed'] for x in report['cases']),'totalCases':len(report['cases']),'elapsedMechanicalSeconds':time.monotonic()-START,'failure':report.get('failure'),'historicalM03Status':'FAILED unchanged','nativeRuns':0,'nativeResultForM04':None,'probeCompiledInM04':False,'realPsCalls':0,'adbCommands':0,'signals':0,'sourceSynchronizations':0,'POrKWrites':0,'gitWrites':0,'authorIntegrationVerdict':None,'oldM03BindingRetainedInUnexecutedRunner':True,'futureExecutionRequiresFreshActivation':True}
    require(set(x.name for x in E.iterdir())==ALLOW-{'receipt.json'},'four pre-receipt leaves only')
    require(sum(x.stat().st_size for x in E.iterdir())+len(json.dumps(receipt).encode())+10000<=5*1024**2,'five-leaf5MiB budget')
    write('receipt.json',receipt)
    print(json.dumps({'status':report['status'],'cases':len(report['cases']),'passed':sum(x['passed'] for x in report['cases']),'mechanicalSeconds':receipt['elapsedMechanicalSeconds'],'failure':report.get('failure'),'receipt':identity(E/'receipt.json'),'realPsCalls':0,'nativeRuns':0}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
