#!/usr/bin/env python3
"""Combined contract: actual runner functions, retained FIX05 assertions, bounded owned offline fixtures."""
import ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types,shlex,re,stat
from collections import Counter
import xml.etree.ElementTree as ET
START=time.monotonic(); E=pathlib.Path(__file__).parent; R=E.parents[3]; BASE=R/'TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source'
RUNNER_FILENAME=str(E/'runner.py'); ROOT=E/'offline-fixtures'; forbidden=[]; cases=[]; traces=set(); fixture_files={}; fixture_links={}; fixture_dirs=[]; conflicts=[]
def audit(name,args):
    if name in {'subprocess.Popen','os.system','os.kill','os.killpg','socket.connect','socket.bind','os.rename','ctypes.dlopen','ctypes.dlsym'}: forbidden.append(name); raise RuntimeError('Offline boundary '+name)
    if name in ('os.mkdir','os.rmdir','os.remove','os.utime','os.symlink'):
        path=pathlib.Path(args[1] if name=='os.symlink' else args[0])
        permitted=(path in fixture_dirs if name in ('os.mkdir','os.rmdir') else path in fixture_files or path in fixture_links)
        if not permitted: forbidden.append(name+' '+str(path)); raise RuntimeError('Fixture mutation boundary')
    if name=='open' and isinstance(args[0],(str,bytes,os.PathLike)) and args[2] & (os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        path=pathlib.Path(args[0])
        if path!=E/'replay-results.json' and path not in fixture_files: forbidden.append('write '+str(path)); raise RuntimeError('Offline write boundary')
sys.addaudithook(audit)
def need(ok,why):
    if not ok: raise AssertionError(why)
def identity(path):
    data=path.read_bytes(); return {'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}
def byteid(data): return {'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}
def rejected(fn):
    try: fn()
    except (RuntimeError,OSError): return True
    return False
def case(name,fn):
    need(time.monotonic()-START+sum(x['seconds'] for x in history['rounds'])<30,'V02 Replay30'); fn(); cases.append({'name':name,'passed':True})
def bind(names,env):
    defaults={'pending_details':{},'process_snapshot':{},'snapshot_root':None,'closure_closed':None,'transfer_conflicts':set(),'probe_deadline':None,'execution_deadline':None,'work_deadline':None,'restore_deadline':None,'launch_counts':{},'stage_bindings':{},'stage_history':[],'current_stage':'I','D':{},'compiler_parked':[],'compiler_after':{},'compiler_restored':[],'copy':copy}
    for key,value in defaults.items():env.setdefault(key,value)
    if hasattr(env.get('P'),'fs'):env.setdefault('children',lambda path:path.iterdir())
    names=list(names)+[n for n in ('probe_timeout','transfer_slots','move_verified','compiler_paths','stage_environment','isolate_stage','checked','read_chunks','bounded_read','bounded_text','scan_tree','children','json_chunks','json_digest','csc_events') if n in F and n not in names and n not in env]
    exec(compile(ast.fix_missing_locations(ast.Module(body=[copy.deepcopy(F[n]) for n in names],type_ignores=[])),RUNNER_FILENAME,'exec'),env);return env
def profile(frame,event,arg):
    if event=='call' and frame.f_code.co_filename==RUNNER_FILENAME:traces.add(frame.f_code.co_name)
def directory(path):
    need(path==ROOT or ROOT in path.parents,'Owned fixture directory')
    if path in fixture_dirs:return path
    if path.parent!=E:directory(path.parent)
    need(not os.path.lexists(path),'Never adopt existing fixture directory')
    fixture_dirs.append(path);path.mkdir();return path
def fixture_file(path,data):
    directory(path.parent)
    if path in fixture_files:need(identity(path)==fixture_files[path] and not path.is_symlink(),'Owned fixture changed')
    else:need(not os.path.lexists(path),'Unknown fixture target')
    fixture_files[path]=byteid(data);path.write_bytes(data);return path
def fixture_link(path,target):
    directory(path.parent)
    if path in fixture_links:need(path.is_symlink() and os.readlink(path)==fixture_links[path],'Fixture link drift');path.unlink()
    else:need(not os.path.lexists(path),'Unknown link target')
    fixture_links[path]=str(target);os.symlink(str(target),path)
def cleanup():
    for path,target in list(fixture_links.items()):
        if path.is_symlink() and os.readlink(path)==target:path.unlink()
        else:conflicts.append(str(path))
    for path,value in list(fixture_files.items()):
        if path.is_file() and not path.is_symlink() and identity(path)==value:path.unlink()
        else:conflicts.append(str(path))
    for path in reversed(fixture_dirs):
        if path.is_dir() and not path.is_symlink() and not list(path.iterdir()):path.rmdir()
        else:conflicts.append(str(path))
history=json.loads((E/'replay-results.json').read_text())
need(history['task']=='RES-COMBINED-V02','Fresh correction history; V00 remains immutable')
need(len(history['rounds'])<2 and not any(x['status']=='SOURCE_REPLAY_PASS' for x in history['rounds']),'V02 maximum two rounds; cumulative30; first green stops')
report={'round':len(history['rounds'])+1,'status':'SOURCE_REPLAY_FAILED','cases':cases,'nativeRuns':0,'realPsCalls':0,'realSignals':0,'actualProjectionWrites':0,'fixtureIOOnly':True};failure=None
try:
    frozen=json.loads("{\"TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/preparation.json\":{\"bytes\":78308,\"sha256\":\"47e9ccc59d5af04934c77174432e7b81d16cc9d8a24b5089040884d77ced1244\"},\"TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/runner.py\":{\"bytes\":54760,\"sha256\":\"148cbc0b63a3bb6371343ed20d6205ae9b44a02cdfde8c1cd6ad9766e4e2266d\"},\"TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/replay-results.json\":{\"bytes\":188906,\"sha256\":\"cf56463f83d919696aeda238676fbf02ba2e1ebc28267ed675f3942ac06cc046\"},\"TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/correction.patch\":{\"bytes\":71680,\"sha256\":\"f0ac7065f4ba253e356d98ce483133fadc15251c80f0118c4aff015fd5858f90\"},\"TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/replay-check.py\":{\"bytes\":68363,\"sha256\":\"56c5bc6a604ba12f7a5b150f53dc2619c61726f96f07bb194e104dfa2ec888b0\"},\"docs/team/2026-09-30/resource-sdk-adopt-g02-central-receipt.json\":{\"bytes\":3131,\"sha256\":\"f67399effe031bbf2478d9ad86def792e73339177883c9e0b853f2a237ebcae5\"},\"TestArtifacts/FightMatch/RES-SDK-ADOPT-G02/group-freeze.json\":{\"bytes\":262970,\"sha256\":\"f952d7239b261099804021d3d6a2b4de9ebcaa8f4265ed3de7102d8c90622ca7\"},\"docs/team/2026-09-30/host-integrate-001-i01-fix05-source-verdict.json\":{\"bytes\":3037,\"sha256\":\"566b60deb4e204aa5cde0ec143888edf8859a862112a62891e2b545b22517a0b\"},\"TestArtifacts/FightMatch/RES-01A/P03/run.json\":{\"bytes\":43101,\"sha256\":\"b9bb096fc6eaeeaa77df33b84916ab2963e61b0191753e36600f3ebc97996d50\"},\"TestArtifacts/FightMatch/RES-01A/P02/resolved-package-graph.json\":{\"bytes\":56222,\"sha256\":\"1b696f4c41b763f4f4622fa83a36712dad7fc1b8d81084fa983cfc1c2bfe170c\"},\"docs/team/2026-09-30/testing-resource-combined-v01-cases.json\":{\"bytes\":24448,\"sha256\":\"d53c49167d522532a6cc87218bfa5e961ab3a096af06209fb694726fca2ed0e8\"},\"docs/team/2026-09-30/central-resource-combined-v01-design-handoff.md\":{\"bytes\":1243,\"sha256\":\"cd16c6c1b9a3ef02dc92d5ffb24dae0190f99be161933d2efd121dc15dac1c7a\"},\"docs/team/2026-09-30/engineering-i01-existing-compile-evidence.json\":{\"bytes\":21099,\"sha256\":\"9fd2ae56e81c5805b93637f27dab3ca67828cfdbd94ac290e211a601df5213f0\"},\"docs/team/2026-09-30/resource-shared-integration-source-verdict.json\":{\"bytes\":2294,\"sha256\":\"c02875ffc1bef9151e1d408fa09e5da83f9b555e62a570e74b1f8c3cd838b87f\"},\"docs/team/2026-09-30/engineering-resource-combined-v01.md\":{\"bytes\":10205,\"sha256\":\"c1f71459c8a34741bd5c5b075253d4df6266a777b801c6d7d6229748012d7062\"},\"docs/team/2026-09-30/central-resource-combined-v01-source.md\":{\"bytes\":4228,\"sha256\":\"49721b69b60a03845a38b464db200b8c12e04ab1a1b8d6f71d498d44e17d5b55\"},\"TestArtifacts/FightMatch/RES-COMBINED-V01/source-plan/inputs.json\":{\"bytes\":1241557,\"sha256\":\"ff088efd0e6871d751a1f553a46e77ae06789e9f01c5743411ca93e1fa087f1d\"}}")
    frozen['TestArtifacts/FightMatch/RES-COMBINED-V01/source-plan/inputs.json']={'bytes':1241588,'sha256':'30e2254fba0f1ee5f0ac82214d49690b6aab3401d151dee765ac9ddbbb0865fb'}
    def frozen_unchanged():
        for name,value in frozen.items():need(identity(R/name)==value,'Frozen reference '+name)
    frozen_unchanged()
    history_root=R/'TestArtifacts/FightMatch/RES-COMBINED-V01'; v02_base=history_root/'run'; v00=history_root/'V00-source'; v00prep=json.loads((v00/'preparation.json').read_text())
    for name,value in {**v00prep['sourceIdentities'],'preparation.json':{'bytes':330242,'sha256':'d9c90ce2e5724763ba69f478e03b8431fe62706db0823c362f9b06ccce22582c'}}.items():need(identity(v00/name)==value,'Immutable V00 '+name)
    v01=history_root/'V01-source';v01prep=json.loads((v01/'preparation.json').read_text())
    for name,value in {**v01prep['sourceIdentities'],'preparation.json':{'bytes':391631,'sha256':'2485b5fa707f20eb6dfd431677e973c88199e6210a6713134d9b3590bc53ec1a'}}.items():need(identity(v01/name)==value,'Immutable V01 '+name)
    need(identity(v01/'replay-results.json')==v01prep['sourceIdentities']['replay-results.json'],'Original138 history sealed')
    need(identity(v02_base/'replay-results.json')=={k:history['baseline'][k] for k in ('bytes','sha256')},'Original158 history sealed')
    amended=json.loads((v01/'inputs.json').read_text());amended['preparation']['budgetSeconds']=160;amended['limits'].update(mechanicalPreparationSeconds=160,finalizationSeconds=30)
    need(amended==json.loads((v02_base/'inputs.json').read_text()),'Original FIX02 three input fields preserved at historical source')
    need(identity(E/'inputs.json')=={'bytes':1241674,'sha256':'5042438e26cd9e7fea1c5decee0f689f30af05ee667c25dbec57c472d363e636'},'Exact V02 input seal')
    text=(E/'runner.py').read_text();tree=ast.parse(text);compile(tree,RUNNER_FILENAME,'exec');compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec');F={n.name:n for n in tree.body if isinstance(n,ast.FunctionDef)}
    oldfn={n.name:n for n in ast.parse((BASE/'runner.py').read_text()).body if isinstance(n,ast.FunctionDef)}
    unchanged=['move_verified','rename_exclusive','rename_swap','swap_verified','atomic_write','cached_reuse_proven','compilation_passed','recorded_chain','recover_root','discover','snapshot_consumers','monitor','details','adb_exception','sdk_adb_exception']
    need(all(ast.dump(F[n])==ast.dump(oldfn[n]) for n in unchanged),'Retained concurrency/ownership/provenance guards')
    class StripDeadlineChecks(ast.NodeTransformer):
        def visit_Call(self,node):
            node=self.generic_visit(node)
            if isinstance(node.func,ast.Name):
                if node.func.id=='checked':return node.args[0]
                if node.func.id=='children':return ast.Call(func=ast.Attribute(value=node.args[0],attr='iterdir',ctx=ast.Load()),args=[],keywords=[])
                if node.func.id in ('bounded_read','bounded_text'):return ast.Call(func=ast.Attribute(value=node.args[0],attr='read_bytes' if node.func.id=='bounded_read' else 'read_text',ctx=ast.Load()),args=[],keywords=node.keywords)
            return node
        def visit_Expr(self,node):return None if isinstance(node.value,ast.Call) and isinstance(node.value.func,ast.Name) and node.value.func.id=='probe_timeout' else self.generic_visit(node)
    for name in ('transfer','projection_guard'):need(ast.dump(StripDeadlineChecks().visit(copy.deepcopy(F[name])))==ast.dump(oldfn[name]),'Read/iteration bounds only; original '+name+' guards retained')
    need(ast.dump(StripDeadlineChecks().visit(copy.deepcopy(F['synchronize'])))==ast.dump(oldfn['synchronize']),'Sync only gains deadline checks; all prior operations retained')
    need(not any(isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id=='ps' for k in ('monitor','snapshot_consumers','discover','register','recover_root') for n in ast.walk(F[k])),'Same-snapshot classification')
    calls=[n for n in ast.walk(F['run_stage']) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='Popen'];need(len(calls)==1,'Single Popen site')
    need(F['run_stage'].body[1].value.func.id=='claim_stage','Single-use claim first')
    D=json.loads((E/'inputs.json').read_text());Q=json.loads((R/D['testCases']['path']).read_text())
    normal=bind(['check','basic','canonical','normalize_input','contract_guard'],{'hashlib':hashlib});N=normal['contract_guard'](D,Q)
    N['processFixture']=json.loads("{\"root\":{\"ppid\":36290,\"start\":\"Thu Oct 8 02:01:04 2026\",\"stat\":\"Ss\",\"exe\":\"/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity\",\"argv\":[\"/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity\",\"-batchmode\",\"-force-metal\",\"-buildTarget\",\"StandaloneOSX\",\"-projectPath\",\"/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/projection\",\"-executeMethod\",\"FightMatch.Host.Editor.FightMatchHostFirstFrameProbe.Execute\",\"-fmProbeInput\",\"/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-NEXT-001/M03/activation.json\",\"-fmProbeNonce\",\"hn03-636ff5b0d3304862974c0c4fbdae806a\",\"-fmHostSaveIsolationId\",\"bcf925874bc04deab23d7a0e20fb7e46\",\"-fmHostSaveIsolationCandidate\",\"313b992da1ecd2379def77be346bb7413f0c4343bea06a1e9fed71fb2fc989f8\",\"-logFile\",\"/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-NEXT-001/M03/P/editor.log\"],\"cwd\":\"/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/projection\",\"pid\":36300,\"stage\":\"P\",\"rootPid\":36300,\"firstObservedUtc\":\"2026-10-07T18:01:04.528754+00:00\",\"termSent\":false,\"cwdProbeExit\":0},\"child\":{\"ppid\":36300,\"start\":\"Thu Oct 8 02:01:15 2026\",\"stat\":\"S\",\"exe\":\"/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/bee_backend\",\"argv\":[\"/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/bee_backend\",\"--dont-print-to-structured-log\",\"--ipc\",\"--defer-dag-verification\",\"--dagfile=Library/Bee/200b0aEDbg.dag\",\"--continue-on-failure\",\"--profile=Library/Bee/backend1.traceevents\",\"ScriptAssemblies\"],\"cwd\":\"/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/projection\",\"pid\":36391,\"stage\":\"P\",\"rootPid\":36300,\"firstObservedUtc\":\"2026-10-07T18:01:15.869462+00:00\",\"termSent\":false,\"cwdProbeExit\":0}}")
    report['changedFunctions']=sorted(n for n in oldfn if ast.dump(F[n])!=ast.dump(oldfn[n]));report['newFunctions']=sorted(set(F)-set(oldfn))
    report['static']={'syntax':True,'runnerNonblankLines':sum(bool(x.strip()) for x in text.splitlines()),'protectedFunctionsAstEqual':unchanged,'singlePopenSite':True,'sameSnapshot':True}
    sys.setprofile(profile)
    root=N['processFixture']['root']; child=N['processFixture']['child']; rootid=root['pid']; childid=child['pid']; keys=('ppid','start','stat','exe')
    def monitor_case(phase,variant=None,entry='monitor'):
        rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)}; initial={rootid:copy.deepcopy(root)}
        if variant=='foreign':rows[childid]['ppid']=999999
        if variant=='reuse':initial[childid]=copy.deepcopy(child);rows[childid]['start']='synthetic reused PID'
        detail={x['pid']:{k:x[k] for k in ('argv','cwd','cwdProbeExit')} for x in (root,child)}; snapshots=[]; ps_calls=[]
        for bad,key,value in [('bad-argv','argv',[]),('bad-cwd','cwd',None),('bad-probe','cwdProbeExit',1)]:
            if variant==bad:detail[childid][key]=value
        def fixture_details(pid):
            if variant=='details-error' and pid==childid:raise RuntimeError('synthetic details unavailable after supplied snapshot')
            return copy.deepcopy(detail[pid])
        def forbidden_ps():ps_calls.append(True);raise AssertionError('No second process snapshot')
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:100.),'utc':lambda:'fixture','owned':initial,'A':{'stages':[{'id':root['stage'],'argv':root['argv']}]},'P':pathlib.Path(root['cwd']),'details':fixture_details,'ps':forbidden_ps,'event':lambda *a,**kw:None,'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'resources':lambda:None,'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.0}
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','recover_root','snapshot_consumers','monitor'],env)
        original=env['snapshot_consumers']
        def capture(given,stage,rpid):snapshots.append(given is rows);return original(given,stage,rpid)
        env['snapshot_consumers']=capture
        raised=None
        try:
            if entry=='monitor':env['monitor'](phase,rows,root['stage'],rootid,force=True)
            else:env[entry](rows,root['stage'],rootid)
        except RuntimeError as error:raised=str(error)
        need(bool(raised or env['monitor_errors'])==bool(variant),'Expected ownership verdict '+str(variant));need(not ps_calls,'Zero second ps on details failure')
        if entry=='monitor':need(snapshots==[True],'Actual supplied snapshot object')
        if variant=='details-error':
            reason=raised or env['monitor_errors'][0]['error'];need('Supplied-snapshot child identity failed' in reason and 'synthetic details unavailable' in reason,'Original details failure retained')
            need(childid not in env['owned'],'Failure leaves no partial owned child')
        if variant in ('bad-argv','bad-cwd','bad-probe'):need(childid not in env['owned'],'Invalid details not published')
        if not variant:need(childid in env['owned'] and env['owned'][childid]['argv']==child['argv'] and env['owned'][childid]['cwd']==child['cwd'],'Child full identity')
        return env,rows,detail,ps_calls
    for phase in ('running','natural-closure','term-confirmation','closure-final','final'):
        case(phase+' legitimate new child',lambda phase=phase:monitor_case(phase))
        case(phase+' same-name foreign child rejected',lambda phase=phase:monitor_case(phase,'foreign'))
    case('PID reuse rejected',lambda:monitor_case('running','reuse'))
    for entry in ('discover','snapshot_consumers','monitor'):
        case('details failure '+entry+' rejects without second ps',lambda entry=entry:monitor_case('running','details-error',entry))
    for variant in ('bad-argv','bad-cwd','bad-probe'):
        case(variant+' not published to owned',lambda variant=variant:monitor_case('running',variant,entry='discover'))
    def recovered_ownership(vanished=False):
        env,oldrows,detail,oldps=monitor_case('running','details-error'); validroot=copy.deepcopy(env['owned'][rootid]); fresh=copy.deepcopy(oldrows)
        if vanished:fresh.pop(childid)
        env['details']=lambda pid:copy.deepcopy(detail[pid]); env['snapshot_consumers'](fresh,root['stage'],rootid)
        need(env['owned'][rootid]==validroot and not oldps and env['monitor_errors'],'Original root and original failure preserved')
        if not vanished:need(all(env['owned'][childid][k]==child[k] for k in ('argv','cwd','cwdProbeExit','pid','ppid','start','exe','rootPid')),'Fresh snapshot retries complete child publication')
        else:need(childid not in env['owned'],'Fresh absent child is not invented')
        signals=[];cleanup_ps=[]; current=copy.deepcopy(fresh)
        def next_snapshot():cleanup_ps.append(True);return copy.deepcopy(current)
        def term(pid,sig):need(sig==15,'TERM only');signals.append(pid);current.pop(pid,None)
        env.update(ps=next_snapshot,os=types.SimpleNamespace(kill=term),signal=types.SimpleNamespace(SIGTERM=15),active=None)
        cleanup_clock=[100.];env['time']=types.SimpleNamespace(monotonic=lambda:cleanup_clock[0],sleep=lambda delta:cleanup_clock.__setitem__(0,cleanup_clock[0]+delta))
        env['A']['stopping']={'naturalGraceSeconds':60,'termGraceSeconds':30}
        bind(['closure'],env);env['closure'](root['stage'],rootid,'synthetic fast-forwarded cleanup')
        need(signals==([rootid] if vanished else [childid,rootid]) and not current,'Existing closure authorizes each fully identified process once')
        need(env['owned'][rootid]['argv']==validroot['argv'] and env['owned'][rootid]['cwd']==validroot['cwd'],'Root identity never poisoned')
        report.setdefault('ownershipRecovery',[]).append({'childVanishedInNextSnapshot':vanished,'firstCyclePopulationQueries':len(oldps),'freshSnapshotUsed':fresh is not oldrows,'simulatedTerms':signals,'realSignals':0,'originalFailureRetained':bool(env['monitor_errors'])})
    case('details retry publishes complete child and permits original cleanup guards',lambda:recovered_ownership())
    case('child absent in fresh snapshot cannot poison valid root cleanup',lambda:recovered_ownership(vanished=True))
    def preserve_existing():
        env,rows,detail,ps_calls=monitor_case('running','details-error'); before=copy.deepcopy(env['owned'][rootid])
        need(rejected(lambda:env['register'](rootid,rows[rootid],root['stage'],rootid)) and env['owned'][rootid]==before,'Already valid owned entry preserved')
    case('register cannot replace a valid owned root',preserve_existing)
    env=bind(['check','claim_stage','basic','same','validate_inputs','compilation_passed','cached_reuse_proven'],{'json':json,'launch_attempts':0,'N':N})
    def single():
        env['claim_stage']();need(env['launch_attempts']==1 and rejected(env['claim_stage']) and env['launch_attempts']==1,'I claim exactly once')
    case('I single-use real claim',single)
    case('input drift rejected',lambda:need(rejected(lambda:env['validate_inputs']({'a':byteid(b'x')},{'a':byteid(b'y')},'fixture')),'Input mismatch rejected'))
    proof={'assemblies':{name:{'proven':True} for name in N['requiredAssemblies']}}
    case('all required compilation evidence passes',lambda:need(env['compilation_passed'](proof),'Complete evidence'))
    case('missing assembly cannot pass',lambda:need(not env['compilation_passed']({'assemblies':{}}),'Missing compilation proof'))
    partial=copy.deepcopy(proof);partial['assemblies'][N['requiredAssemblies'][0]]['proven']=False
    case('unproved assembly cannot pass',lambda:need(not env['compilation_passed'](partial),'False proof'))
    binding={'complete':True,'response':{'path':'fixture.rsp',**byteid(b'rsp')},'sourceInputs':{'a.cs':byteid(b'source')},'references':{'b.dll':byteid(b'ref')},'defines':['-define:UNITY_EDITOR'],'compilerOutput':byteid(b'out'),'dll':byteid(b'dll')}
    prior={**copy.deepcopy(binding),'proven':True,'origin':'actualCsc'}
    case('complete unchanged cache chain passes',lambda:need(env['cached_reuse_proven'](binding,binding,prior),'Valid cache accepted'))
    case('missing prior cache chain rejected',lambda:need(not env['cached_reuse_proven'](binding,binding,{}),'Incomplete history'))
    for key in ('response','sourceInputs','references','defines','compilerOutput','dll'):
        changed=copy.deepcopy(binding);changed[key]='synthetic drift'
        case('cache '+key+' drift rejected',lambda changed=changed:need(not env['cached_reuse_proven'](binding,changed,prior),'Cache drift'))
    class FS:
        def __init__(self):self.files={};self.dirs={'/'};self.fail_write=None;self.fail_partial=None;self.fail_commit=None;self.collide_commit=None;self.partial_written=[];self.committed=[];self.race_source=None;self.race_again=False;self.race_value=b'concurrent first';self.swap_counts={};self.native_calls=[]
        def add(self,p,b):
            self.files[p]=b;q=pathlib.PurePosixPath(p).parent
            while str(q)!='/':self.dirs.add(str(q));q=q.parent
        def path(self,p):return Path(self,str(p))
        def commit(self,source,target,exclusive=False):
            if self.collide_commit==source.p:self.add(target.p,b'concurrent third party')
            if self.fail_commit==source.p:raise OSError('synthetic atomic commit refused')
            if exclusive and target.exists():raise FileExistsError('synthetic exclusive destination exists')
            payload=self.files.pop(source.p);self.add(target.p,payload);self.committed.append((source.p,target.p))
        def swap(self,source,target):
            if self.fail_commit==source.p:raise OSError('synthetic atomic swap refused')
            count=self.swap_counts.get(source.p,0)+1;self.swap_counts[source.p]=count
            if self.race_source==source.p:
                if count==1:self.add(target.p,self.race_value)
                elif count==2 and self.race_again:self.add(target.p,b'concurrent second')
            need(source.p in self.files and target.p in self.files,'Both swap nodes exist')
            self.files[source.p],self.files[target.p]=self.files[target.p],self.files[source.p];self.committed.append((source.p,target.p))
    class Path:
        def __init__(self,fs,p):self.fs=fs;self.p=str(pathlib.PurePosixPath(p))
        def __str__(self):return self.p
        def __fspath__(self):return self.p
        def __truediv__(self,v):return Path(self.fs,self.p+'/'+str(v))
        @property
        def parent(self):return Path(self.fs,str(pathlib.PurePosixPath(self.p).parent))
        def exists(self):return self.p in self.fs.files or self.p in self.fs.dirs
        def stat(self):return types.SimpleNamespace(st_dev=9)
        def mkdir(self,parents=False,exist_ok=False):
            q=pathlib.PurePosixPath(self.p)
            while str(q) not in self.fs.dirs:self.fs.dirs.add(str(q));q=q.parent
        def iterdir(self):return iter([Path(self.fs,p) for p in self.fs.files.keys()|self.fs.dirs if p!='/' and str(pathlib.PurePosixPath(p).parent)==self.p])
        def rmdir(self):need(not list(self.iterdir()),'fixture rmdir empty');self.fs.dirs.remove(self.p)
        def read_bytes(self):return self.fs.files[self.p]
        def read_text(self,**kw):return self.read_bytes().decode(errors=kw.get('errors','strict'))
        def write_bytes(self,b):self.fs.add(self.p,b);return len(b)
        def rename(self,target):
            need(not target.exists(),'fixture rename target absent');target.write_bytes(self.fs.files.pop(self.p));return target
        def open(self,mode):
            if self.fs.fail_write==self.p and any(flag in mode for flag in ('w','x')):raise RuntimeError('synthetic interrupted write before truncation')
            need(not ('x' in mode and self.exists()),'exclusive target')
            path=self;binary='b' in mode
            class Buffer(io.BytesIO if binary else io.StringIO):
                def write(stream,value):
                    if path.fs.fail_partial==path.p:
                        partial=value[:max(1,len(value)//2)];super().write(partial);path.fs.add(path.p,partial if binary else partial.encode());path.fs.partial_written.append(path.p);raise OSError('synthetic failure after partial bytes persisted')
                    return super().write(value)
                def fileno(stream):return -123
                def close(stream):
                    if not stream.closed and 'r' not in mode:
                        value=stream.getvalue();path.fs.add(path.p,value if binary else value.encode())
                    super().close()
            return Buffer(self.fs.files[self.p]) if 'r' in mode else Buffer()
    def restore_case(drift=False,interrupt=False,fault=None):
        fs=FS();n=copy.deepcopy(N);base={};target={}
        for p in n['overwritten']+n['parked']:base[p]=('before '+p).encode()
        stable='Assets/fixture-unchanged.cs';base[stable]=b'unchanged'
        for p in n['overwritten']+n['newPaths']:target[p]=('after '+p).encode()
        target[stable]=b'unchanged';original=copy.deepcopy(base)
        for p,b in base.items():fs.add('/P/'+p,b)
        for p,b in target.items():fs.add('/R/'+p,b)
        n['restoreBaseline']={p:byteid(b) for p,b in base.items()};n['files']={p:byteid(b) for p,b in target.items()};n['shared']=n['files'];outputs={}
        def sources(root):return {p[len(str(root))+1:]:byteid(b) for p,b in fs.files.items() if p.startswith(str(root)+'/')}
        errno=[0]
        class NativeRename:
            def __call__(self,src,dst,flags):
                fs.native_calls.append(flags);source=fs.path(src.decode());targetpath=fs.path(dst.decode())
                try:
                    if flags==2:fs.swap(source,targetpath)
                    elif flags==4:fs.commit(source,targetpath,True)
                    else:raise AssertionError('Unexpected rename flag')
                    return 0
                except OSError:errno[0]=5;return -1
        native=NativeRename();lib=types.SimpleNamespace(renamex_np=native);fake_ctypes=types.SimpleNamespace(CDLL=lambda *a,**kw:lib,c_char_p=object(),c_uint=object(),c_int=object(),get_errno=lambda:errno[0])

        env={'N':n,'P':fs.path('/P'),'R':fs.path('/R'),'E':fs.path('/E'),'B':{},'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda p:p.exists()),fsync=lambda fd:None,fsencode=lambda p:str(p).encode(),strerror=lambda code:'synthetic errno '+str(code)),'time':types.SimpleNamespace(monotonic=lambda:100.),'hashlib':hashlib,'json':json,'synced':False,'restored':False,'synchronized_paths':[],'parked_paths':[],'owned':{},'stages':[],'active':None,'ps':lambda:{},'consumer_guard':lambda rows:None,'projection_guard':lambda:None,'protection':lambda:None,'resources':lambda:None,'source_tree':sources,'no_links':lambda p:None,'ident':lambda p:byteid(p.read_bytes()),'ctypes':fake_ctypes,'atomic_conflicts':set(),'event':lambda *a,**kw:None,'write':lambda name,value:outputs.update({name:value})}
        bind(['check','basic','same','validate_inputs','transfer','rename_exclusive','rename_swap','swap_verified','atomic_write','synchronize','archive_and_restore'],env)
        selected=n['newPaths'][1] if fault and 'new' in fault else n['overwritten'][2]
        phase='restore' if fault and fault.startswith('restore') else 'sync';atomic='/E/atomic/'+phase+'/'+selected
        if interrupt:fs.fail_write='/E/atomic/sync/'+selected
        if fault and 'partial' in fault:fs.fail_partial=atomic
        if fault and 'commit' in fault:fs.fail_commit=atomic
        if fault=='new-exclusive-collision':fs.collide_commit=atomic
        racing=bool(fault and 'race' in fault)
        if racing:fs.race_source=atomic;fs.race_again='again' in fault;fs.race_value=target[selected] if 'same-as-post' in fault else b'concurrent first'
        sync_failure=interrupt or bool(fault and phase=='sync')
        failed=rejected(env['synchronize']);need(failed==sync_failure,'Synchronization expected failure');fs.fail_write=None
        if not sync_failure:need(sources(env['P'])==n['files'],'All 17+4 synchronized and 2 parked')
        if fault and phase=='sync':
            if 'partial' in fault:need(fs.partial_written==[atomic] and 0<len(fs.files[atomic])<len(target[selected]),'Partial write actually persisted only in E')
            if 'commit' in fault:need(fs.files[atomic]==target[selected],'Full staged postimage retained after failed commit')
            if fault!='new-exclusive-collision' and not racing:need(fs.files.get('/P/'+selected)==base.get(selected),'Failed sync did not create/truncate P')
            need(any(dst=='/P/'+n['overwritten'][0] for _,dst in fs.committed),'Earlier item actually committed')
        if drift:fs.files['/P/'+n['overwritten'][0]]=b'concurrent third party'
        restored=env['archive_and_restore']()
        expected={p:byteid(b) for p,b in original.items()}
        restore_failure=bool(fault and phase=='restore')
        collision=fault=='new-exclusive-collision'
        if restore_failure and not racing:
            expected[selected]=byteid(target[selected]);need(fs.files['/P/'+selected]==target[selected],'Failed restore leaves full postimage, never partial P')
            if 'partial' in fault:need(fs.partial_written==[atomic] and 0<len(fs.files[atomic])<len(base[selected]),'Partial restore retained in E')
            if 'commit' in fault:need(fs.files[atomic]==base[selected],'Complete restore temporary retained')
        if drift:expected[n['overwritten'][0]]=byteid(b'concurrent third party')
        if collision:expected[selected]=byteid(b'concurrent third party')
        if racing:
            expected[selected]=byteid(fs.race_value)
            need(fs.files['/E/atomic/displaced/'+phase+'/'+selected]==fs.race_value,'First displaced foreign value retained exactly')
            if fs.race_again:need(fs.files['/E/atomic/conflict/'+phase+'/'+selected]==b'concurrent second','Second drift retained exactly in bounded conflict leaf')
            need(fs.swap_counts[atomic]==2 and (phase,selected) in env['atomic_conflicts'],'Exactly one commit and one conflict-return swap; conflict stays blocked')
            need(fs.files['/E/restore/source/'+selected]==base[selected],'Original preimage still retained')
        need(sources(env['P'])==expected,'Actual ledger restored with unknown values preserved')
        need(restored['complete']==(not drift and not restore_failure and not collision and not racing),'Restore verdict')
        if not drift and not interrupt and not fault:need(len(restored['archived'])==21 and len(restored['returnedParked'])==2,'All 23 current source paths covered')
        if fault:report.setdefault('faultEvidence',[]).append({'fault':fault,'partialFiles':list(fs.partial_written),'atomicTemporaryRetained':atomic in fs.files,'restoredComplete':restored['complete'],'restoredEarlierItem':fs.files['/P/'+n['overwritten'][0]]==base[n['overwritten'][0]],'errors':restored['errors'],'raceAtPrimitive':racing,'swapCallsForAffectedSlot':fs.swap_counts.get(atomic,0),'firstConcurrentRetained':('/E/atomic/displaced/'+phase+'/'+selected) in fs.files,'secondConcurrentRetained':('/E/atomic/conflict/'+phase+'/'+selected) in fs.files,'nativeFlagsUsed':sorted(set(fs.native_calls))})
    case('23 current source paths synchronize and restore through actual functions',lambda:restore_case())
    case('mid-sync failure restores actual ledger',lambda:restore_case(interrupt=True))
    case('third-party target preserved with failed restoration verdict',lambda:restore_case(drift=True))
    for fault in ('sync-overwrite-partial','sync-new-partial','sync-overwrite-commit','sync-new-commit','restore-partial','restore-commit','new-exclusive-collision'):
        case(fault+' preserves P and previous commits',lambda fault=fault:restore_case(fault=fault))
    for fault in ('sync-race','restore-race','sync-race-again','restore-race-again','sync-race-same-as-post'):
        case(fault+' preserves displaced values and bounds conflict return',lambda fault=fault:restore_case(fault=fault))
    def launched_root_case(mode):
        fs=FS();p=fs.path(root['cwd']);e=fs.path('/synthetic-evidence');rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)};originalRoot=copy.deepcopy(rows[rootid]);signals=[];popens=[];snapshots=[];rootDetails=[];emitted=[];written={}
        argv=[root['exe'],'-batchmode','-nographics','-quit','-buildTarget','StandaloneOSX','-projectPath',str(p),'-logFile',str(e/'I/editor.log')]
        class Proc:
            pid=rootid
            returncode=None
            def poll(self):return self.returncode
        proc=Proc();root_clock=[100.]
        def fixture_ps():
            snapshot=copy.deepcopy(rows);snapshots.append(snapshot);return snapshot
        def fixture_details(pid):
            if pid==rootid:
                rootDetails.append(len(snapshots))
                if len(rootDetails)==1:
                    need(not env['owned'],'First root failure publishes no ownership')
                    if mode=='natural-exit':proc.returncode=0;rows.clear()
                    if mode in ('pid-reuse-live','pid-reuse-exited'):
                        rows[rootid]['start']='synthetic reused PID start'
                        if mode=='pid-reuse-exited':proc.returncode=0;rows.pop(childid,None)
                    if mode=='foreign-parent':rows[rootid]['ppid']+=999
                    if mode=='foreign-exe':rows[rootid]['exe']='/foreign/Unity'
                    raise RuntimeError('synthetic initial root details failure')
                if mode=='persistent-details':raise RuntimeError('synthetic continuing root details failure')
                value={'argv':argv+(['foreign'] if mode=='wrong-argv' else []),'cwd':'/foreign-project' if mode=='wrong-cwd' else str(p),'cwdProbeExit':0}
                if mode=='natural-during-recovery':proc.returncode=0;rows.clear()
                return value
            return {k:child[k] for k in ('argv','cwd','cwdProbeExit')}
        def popen(args,**kw):
            need(args==argv and str(kw['cwd'])==str(p),'Bound synthetic Popen argv/cwd');popens.append(True);return proc
        def term(pid,sig):
            need(sig==15 and pid in env['owned'],'Only fully owned TERM');signals.append(pid);rows.pop(pid,None)
            if pid==rootid:proc.returncode=0
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:root_clock[0],time=lambda:1791402560.,sleep=lambda delta:root_clock.__setitem__(0,root_clock[0]+delta)),'utc':lambda:'fixture-root-time','os':types.SimpleNamespace(environ={},getpid=lambda:originalRoot['ppid'],kill=term,path=types.SimpleNamespace(lexists=lambda p:p.exists())),'signal':types.SimpleNamespace(SIGTERM=15),'subprocess':types.SimpleNamespace(Popen=popen,STDOUT=-2),'P':p,'E':e,'A':{'stages':[{'id':'I','argv':argv,'timeoutSeconds':360}],'editor':{'path':root['exe']},'environmentOverrides':{},'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}},'owned':{},'active':None,'launched_root':None,'root_launch_epoch':None,'launch_attempts':0,'stages':[],'synced':True,'details':fixture_details,'ps':fixture_ps,'event':lambda kind,**kw:emitted.append({'kind':kind,**kw}),'write':lambda name,value:written.update({name:copy.deepcopy(value)}),'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'preflight':lambda:None,'resources':lambda:None,'projection_guard':lambda:None,'protection':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.0}
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','recover_root','snapshot_consumers','monitor','closure','claim_stage','run_stage','archive_and_restore'],env)
        result=env['run_stage']()
        need(len(popens)==1 and env['launch_attempts']==1 and result['runCount']==1 and result['status']=='FAILED','One simulated launch; original failed stage never becomes pass')
        need('synthetic initial root details failure' in result['failure'],'Original root failure preserved')
        need(len(rootDetails)==len(set(rootDetails)),'Registration/details retry only on later supplied snapshots')
        pending=mode in ('persistent-details','pid-reuse-live','foreign-parent','foreign-exe','wrong-argv','wrong-cwd')
        if mode=='recover':
            need(signals==[childid,rootid] and env['active'] is None and env['owned'][rootid]['argv']==argv and env['owned'][rootid]['cwd']==str(p),'Actual closure recovered root and child and completed TERM guards')
        else:need(not signals,'No signals to unverified/reused/naturally exited roots')
        if mode in ('natural-exit','natural-during-recovery','pid-reuse-exited'):need(env['active'] is None and proc.poll()==0 and rootid not in env['owned'],'Popen exit handled without adopting reused PID')
        if pending:
            need(env['active'] is proc and proc.poll() is None and rootid not in env['owned'],'Live unverified Popen retained, not partial owned')
            count=len(snapshots);need(rejected(env['archive_and_restore']) and len(snapshots)==count,'Restore rejected by live Popen gate before filesystem or another population query')
        endings=[x for x in emitted if x['kind']=='closure_end'];need(len(endings)==1 and endings[0]['popenStillLive']==pending,'Closure evidence explicitly identifies pending Popen')
        report.setdefault('launchedRootRecovery',[]).append({'mode':mode,'simulatedPopenCount':len(popens),'rootDetailsSnapshotIndexes':rootDetails,'suppliedSnapshots':len(snapshots),'rootPublished':rootid in env['owned'],'simulatedTerms':signals,'popenStillLive':proc.poll() is None,'restoreRootGateBlocked':pending,'originalFailurePreserved':True,'realPopenPsSignals':0,'naturalAndTermWindows':'Synthetic advancing clock (60/30); prior outcome assertions unchanged'})
    for mode in ('recover','natural-exit','natural-during-recovery','pid-reuse-live','pid-reuse-exited','foreign-parent','foreign-exe','wrong-argv','wrong-cwd','persistent-details'):
        case('launched root '+mode+' through actual run_stage/closure',lambda mode=mode:launched_root_case(mode))
    need(len(cases)==58 and all(c['passed'] for c in cases),'All original 58 assertions preserved')
    report['prior58Passed']=True
    def regression(name,fn):
        start=time.monotonic()
        try:fn();cases.append({'name':name,'passed':True,'newFIX04':True})
        except Exception as error:cases.append({'name':name,'passed':False,'newFIX04':True,'error':str(error),'traceback':traceback.format_exc()})
        need(time.monotonic()-START+sum(x['seconds'] for x in history['rounds'])<30,'V02 Replay30')
    def fifo_case(mode):
        import re,stat
        license_path=str(pathlib.Path(N['editor']['path']).parent.parent/'Frameworks/UnityLicensingClient.app/Contents/MacOS/Unity.Licensing.Client')
        rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)};initial={rootid:copy.deepcopy(root)}
        exe='/fixed/dotnet' if mode=='dotnet' else license_path
        rows[childid]['exe']=exe;detail={rootid:{k:root[k] for k in ('argv','cwd','cwdProbeExit')},childid:{'argv':[exe,'--namedPipe','fixture'],'cwd':root['cwd'],'cwdProbeExit':0}}
        if mode=='foreign-same-name':rows[childid]['ppid']=999999
        if mode=='foreign-exe':rows[childid]['exe']='/foreign/Unity.Licensing.Client'
        if mode=='ilpp-fifo':
            rows[childid]['exe']=str(pathlib.Path(N['editor']['path']).parent.parent/'Tools/ilpp/Unity.ILPP.Runner/Unity.ILPP.Runner');detail[childid]['argv']=[rows[childid]['exe'],'--namedPipe','fixture']
        if mode in ('reuse','wrong-ancestry'):
            initial[childid]={**copy.deepcopy(child),**detail[childid],**rows[childid]}
            if mode=='reuse':rows[childid]['start']='reused'
            else:initial[childid]['rootPid']=rootid+999
        reads=[];population=[];entries={};sequence=[]
        class FixturePath(pathlib.PurePosixPath):
            def lstat(self):return types.SimpleNamespace(st_mode=(stat.S_IFSOCK if mode in ('non-fifo','diagnostic-socket') else stat.S_IFIFO)|0o600,st_uid=202 if mode=='wrong-uid' else 101,st_gid=20,st_dev=9,st_ino=10,st_size=0)
        temporary=FixturePath('/synthetic-tmp');scan=FixturePath('/wrong-root') if mode=='wrong-root' else temporary
        filename='clr-debug-pipe-'+str(childid)+'-1791404087-in'
        if mode=='wrong-name':filename='foreign-pipe'
        if mode=='diagnostic-socket':filename='dotnet-diagnostic-'+str(childid)+'-1791404087-socket'
        def no_read(p):reads.append(str(p));raise AssertionError('FIFO content read')
        def no_ps():population.append(True);raise AssertionError('Second population query')
        env={'json':json,'re':re,'stat':stat,'pathlib':types.SimpleNamespace(Path=FixturePath),'time':types.SimpleNamespace(monotonic=lambda:100.),'utc':lambda:'fixture-fifo','os':types.SimpleNamespace(walk=lambda root,followlinks:[(str(scan),[],[filename])],getuid=lambda:101),'TMP':temporary,'owned':initial,'active':None,'A':{'editor':N['editor'],'stages':[{'id':root['stage'],'argv':root['argv']}]},'P':FixturePath(root['cwd']),'details':lambda pid:copy.deepcopy(detail[pid]),'ps':no_ps,'event':lambda kind,**kw:sequence.append(kind),'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'projection_guard':lambda:None,'no_links':lambda p:None,'ident':no_read,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.}
        env['scan_tree']=lambda root:env['os'].walk(root,followlinks=False)
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','recover_root','snapshot_consumers','monitor','tree_entries'],env)
        def resources():
            sequence.append('resources');entries.update(env['tree_entries'](scan))
        env['resources']=resources;env['monitor']('running',rows,root['stage'],rootid,force=True)
        positive=mode in ('license','dotnet')
        need(bool(env['monitor_errors'])!=positive,'Exact FIFO ownership verdict '+mode)
        need(not reads and not population,'Zero FIFO contents and zero second ps')
        if positive:
            need(entries[filename]['type']=='fifo' and entries[filename]['contentsRead'] is False,'FIFO metadata only')
            need(sequence.index('owned_discovered')<sequence.index('resources'),'Child registered before same-snapshot FIFO check')
        report.setdefault('fifoEvidence',[]).append({'mode':mode,'errors':copy.deepcopy(env['monitor_errors']),'contentReads':len(reads),'secondPs':len(population),'sequence':sequence})
    for mode in ('license','dotnet','wrong-uid','wrong-root','wrong-ancestry','foreign-same-name','foreign-exe','reuse','non-fifo','wrong-name'):
        regression('same snapshot FIFO '+mode,lambda mode=mode:fifo_case(mode))
    def closure_first(mode):
        rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)};details_by_pid={x['pid']:{k:x[k] for k in ('argv','cwd','cwdProbeExit')} for x in (root,child)}
        clock=[100.];snapshots=[];discoveries=[];detail_attempts=[];signals=[];emitted=[];resource_calls=[];projection_calls=[]
        transient=mode in ('absent','zombie','recover','persistent','reuse')
        def fixture_ps():
            snapshots.append(clock[0])
            if mode=='ps-always' or (mode=='ps-once' and len(snapshots)==1):raise RuntimeError('synthetic ps unavailable')
            if len(snapshots)>1:
                if mode=='absent':rows.pop(childid,None)
                if mode=='zombie' and childid in rows:rows[childid]['stat']='Z'
                if mode=='reuse' and childid in rows:rows[childid].update(start='reused child',exe='/foreign/unclassified')
            return copy.deepcopy(rows)
        def fixture_details(pid):
            detail_attempts.append((pid,len(snapshots)))
            if pid==childid and transient and (len(snapshots)==1 or mode=='persistent'):raise RuntimeError('synthetic child ps args exit 1')
            return copy.deepcopy(details_by_pid[pid])
        def resources():
            resource_calls.append(clock[0])
            if mode=='resources-always' or (mode=='resources-once' and len(resource_calls)==1):raise RuntimeError('synthetic resources unavailable')
        def projection():
            projection_calls.append(clock[0])
            if mode=='projection-always' or (mode=='projection-once' and len(projection_calls)==1):raise RuntimeError('synthetic projection unavailable')
        def term(pid,sig):
            need(sig==15 and pid in env['owned'] and pid not in env['pending_details'],'Fresh verified TERM only')
            signals.append(pid);rows.pop(pid,None)
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda delta:clock.__setitem__(0,clock[0]+delta)),'utc':lambda:'fixture-'+str(clock[0]),'os':types.SimpleNamespace(kill=term),'signal':types.SimpleNamespace(SIGTERM=15),'owned':{rootid:copy.deepcopy(root)},'active':None,'A':{'stages':[{'id':root['stage'],'argv':root['argv']}],'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}},'P':pathlib.Path(root['cwd']),'details':fixture_details,'ps':fixture_ps,'event':lambda kind,**kw:emitted.append({'kind':kind,'at':clock[0],**copy.deepcopy(kw)}),'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'resources':resources,'projection_guard':projection,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.}
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','recover_root','snapshot_consumers','monitor','closure','archive_and_restore'],env)
        actual_discover=env['discover']
        def discover_once(given,stage,rpid):
            discoveries.append(len(snapshots));return actual_discover(given,stage,rpid)
        env['discover']=discover_once;failure=None
        try:env['closure'](root['stage'],rootid,'original synthetic compile failure')
        except RuntimeError as error:failure=str(error)
        endings=[e for e in emitted if e['kind']=='closure_end']
        unresolved=mode in ('persistent','reuse','ps-always','resources-always','projection-always')
        need(len(endings)==1,'First-cycle error must reach closure_end')
        need(clock[0]-100==(90 if unresolved else 60),'Simulated original 60/30 windows')
        need(bool(failure)==unresolved,'Closure outcome requires a successful final sample')
        need(bool(env['monitor_errors']),'Original first-cycle failure retained')
        need(len(discoveries)==len(set(discoveries)),'One discovery per population snapshot')
        need(len(signals)==len(set(signals)),'At most one TERM per owned identity')
        if mode in ('persistent','reuse','ps-always'):
            need(not signals and childid not in env['owned'],'Unknown identities grant zero signal permission')
        else:
            need(signals==([rootid] if mode in ('absent','zombie') else [childid,rootid]),'Verified cleanup guard preserves expected TERM order')
        if mode in ('absent','zombie','recover'):need(not env['pending_details'],'Only later absent/zombie/full identity resolves pending')
        if mode in ('persistent','reuse'):
            need(childid in env['pending_details'],'Persistent/reused child remains pending')
            if mode=='reuse':need([pid for pid,index in detail_attempts if pid==childid]==[childid],'No detail adoption of reused PID')
        if unresolved:
            before=len(snapshots);need(rejected(env['archive_and_restore']) and len(snapshots)==before,'Unclosed gate prevents restore before another snapshot or filesystem access')
        else:need(env['closure_closed'] is True,'Successful final sample explicitly closes')
        report.setdefault('closureFirstEvidence',[]).append({'mode':mode,'simulatedSeconds':clock[0]-100,'populationSamples':len(snapshots),'discoverySampleIndexes':discoveries,'signals':signals,'pendingPids':list(env['pending_details']),'closureClosed':env['closure_closed'],'failure':failure,'firstFailure':env['monitor_errors'][0],'errorCount':len(env['monitor_errors']),'zeroRealSignals':True})
    for mode in ('absent','zombie','recover','persistent','reuse','ps-once','ps-always','resources-once','resources-always','projection-once','projection-always'):
        regression('closure first cycle '+mode,lambda mode=mode:closure_first(mode))
    def skipped_is_not_clear():
        env,rows,detail,population=monitor_case('running')
        env['owned'].clear();env.update(active=None,closure_closed=False,ps=lambda:{},signal=types.SimpleNamespace(SIGTERM=15),os=types.SimpleNamespace(kill=lambda *a:need(False,'No signals')))
        env['A']['stopping']={'naturalGraceSeconds':60,'termGraceSeconds':30}
        skip_clock=[100.];env['time']=types.SimpleNamespace(monotonic=lambda:skip_clock[0],sleep=lambda delta:skip_clock.__setitem__(0,skip_clock[0]+delta))
        calls=[];actual_monitor=env['monitor']
        def skip_until_final(phase,*args,**kw):
            calls.append(phase)
            if phase=='natural-closure':
                skip_clock[0]+=60
                return None
            return actual_monitor(phase,*args,**kw)
        env['monitor']=skip_until_final;bind(['closure'],env);env['closure'](root['stage'],rootid,'synthetic skipped samples')
        need(calls==['natural-closure','term-confirmation','closure-final'] and env['closure_closed'] is True,'Only final successful forced sample establishes clear')
    regression('skipped monitor sample cannot establish closure',skipped_is_not_clear)
    need(len(cases)==80 and all(c['passed'] for c in cases),'All original 80 assertions preserved')
    report['prior80Passed']=True
    def transfer_race(role,mode):
        fs=FS();p=fs.path('/P');e=fs.path('/E');n=copy.deepcopy(N);source_path=n['parked'][0] if role in ('park','unpark') else n['newPaths'][0]
        if role=='park':source=p/source_path;target=e/'park/source'/source_path
        elif role=='unpark':source=e/'park/source'/source_path;target=p/source_path
        elif role=='settings':source=p/n['allowedNewSettings']['path'];target=e/'archive/SceneTemplateSettings.json'
        else:source=p/source_path;target=e/'archive/source'/source_path
        original=b'known original';unknown1=b'concurrent first';unknown2=b'concurrent second';fs.add(str(source),original);calls=[];events=[];errno=[0];injected=[]
        class Native:
            def __call__(self,src,dst,flags):
                a=fs.path(src.decode());b=fs.path(dst.decode());calls.append((str(a),str(b),flags));first=not injected
                try:
                    if first and mode in ('source','source-return'):
                        fs.add(str(source),unknown1);injected.append('first')
                    if mode=='source-return' and flags==2 and len([v for v in calls if v[2]==2])==2:
                        fs.add(str(source),unknown2);injected.append('second')
                    if mode in ('target','target-return') and str(b)==str(target) and flags==4:
                        fs.add(str(target),unknown1);injected.append('target')
                    if mode=='target-return' and str(b)==str(source) and flags==4:
                        fs.add(str(source),unknown2);injected.append('second')
                    if mode in ('retire','retire-return') and str(b).endswith('/retired') and flags==4:
                        fs.add(str(source),unknown1);injected.append('retired')
                    if mode=='retire-return' and str(b)==str(source) and flags==4 and injected:
                        fs.add(str(source),unknown2);injected.append('second')
                    if flags==2:fs.swap(a,b)
                    elif flags==4:fs.commit(a,b,True)
                    else:raise AssertionError('Unexpected native flag')
                    return 0
                except OSError:errno[0]=5;return -1
        native=Native();fake_ctypes=types.SimpleNamespace(CDLL=lambda *a,**kw:types.SimpleNamespace(renamex_np=native),c_char_p=object(),c_uint=object(),c_int=object(),get_errno=lambda:errno[0])
        env={'N':n,'P':p,'E':e,'pathlib':pathlib,'hashlib':hashlib,'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda x:x.exists()),fsync=lambda fd:None,fsencode=lambda x:str(x).encode(),strerror=lambda code:'synthetic errno'),'ctypes':fake_ctypes,'no_links':lambda x:None,'ident':lambda x:byteid(x.read_bytes()),'event':lambda kind,**kw:events.append({'kind':kind,**kw})}
        bind(['check','transfer','rename_exclusive','rename_swap'],env)
        failed=rejected(lambda:env['transfer'](source,target,byteid(original),True))
        need(failed==(mode!='normal'),'Transfer expected conflict verdict '+role+'/'+mode)
        if mode=='normal':need(not source.exists() and target.read_bytes()==original and not env['transfer_conflicts'],'Normal move complete')
        else:
            need(bool(env['transfer_conflicts']),'Unresolved transfer remains recorded, not a successful restore')
            need(unknown1 in fs.files.values(),'First unknown bytes retained exactly')
            if mode in ('source-return','target-return','retire-return'):need(unknown2 in fs.files.values(),'Second unknown bytes retained exactly')
            if mode in ('source','source-return','retire'):need(source.read_bytes()==unknown1,'Captured foreign source returned to its original path')
            if mode in ('target','target-return'):need(target.read_bytes()==unknown1,'Concurrent destination never overwritten')
            if mode in ('target-return','retire-return'):need(source.read_bytes()==unknown2,'Return destination competition never overwritten')
            need(len([x for x in calls if x[2]==2])<=2,'At most capture swap and one return swap')
        report.setdefault('transferRaceEvidence',[]).append({'role':role,'mode':mode,'failed':failed,'sourcePresent':source.exists(),'source':byteid(source.read_bytes()) if source.exists() else None,'targetPresent':target.exists(),'files':{k:byteid(v) for k,v in fs.files.items()},'primitiveCalls':calls,'injections':injected,'conflicts':sorted(env['transfer_conflicts'])})
    for role in ('park','archive'):
        for mode in ('normal','source','source-return','target','target-return','retire','retire-return'):
            regression('FIX05 transfer '+role+' '+mode,lambda role=role,mode=mode:transfer_race(role,mode))
    for role in ('unpark','settings'):
        regression('FIX05 transfer '+role+' source-return',lambda role=role:transfer_race(role,'source-return'))
    def transfer_restore_gate():
        fs=FS();outputs={}
        env={'transfer_conflicts':{'park/'+N['parked'][0]},'time':types.SimpleNamespace(monotonic=lambda:100.),'active':None,'owned':{},'ps':lambda:{},'consumer_guard':lambda rows:None,'synchronized_paths':[],'parked_paths':[],'N':{'allowedNewSettings':{'path':'ProjectSettings/SceneTemplateSettings.json'},'restoreBaseline':{}},'P':fs.path('/P'),'E':fs.path('/E'),'stages':[],'projection_guard':lambda:{'fixture':True},'protection':lambda:None,'write':lambda name,value:outputs.update({name:value})}
        bind(['check','archive_and_restore'],env)
        result=env['archive_and_restore']()
        need(result['complete'] is False and result['errors'],'Unresolved transfer cannot report restoration complete')
    regression('FIX05 unresolved transfer cannot report restore complete',transfer_restore_gate)
    def deadline_case(mode):
        import shlex,subprocess
        clock=[100.];rows={};initial={};signals=[];commands=[];events=[];phase=['natural'];consumer_probes=[]
        for index in range(4):
            item=copy.deepcopy(root if index==0 else child);pid=rootid if index==0 else childid+index-1;item.update(pid=pid,ppid=root['ppid'] if index==0 else rootid,rootPid=rootid,termSent=False)
            initial[pid]=item;rows[pid]={k:item[k] for k in keys}
        def elapsed_probe(label,requested,timeout):
            start=clock[0];amount=min(requested,timeout);clock[0]+=amount
            commands.append({'label':label,'phase':phase[0],'start':start,'timeout':timeout,'end':clock[0]})
            if requested>timeout:raise subprocess.TimeoutExpired(label,timeout)
        def run(args,**kw):
            population=args[0]=='/bin/ps' and '-axo' in args;cwd=args[0]=='/usr/sbin/lsof'
            delay=3 if population else 2
            if phase[0]=='term' and ((mode=='ps-exhausts' and population) or (mode=='details-exhausts' and not population)):delay=40
            elapsed_probe('ps' if population else 'cwd' if cwd else 'args',delay,kw['timeout'])
            if population:
                stdout='\n'.join(str(pid)+' '+str(row['ppid'])+' '+row['start']+' '+row['stat']+' '+row['exe'] for pid,row in rows.items())
            else:
                pid=int(args[args.index('-p')+1]);stdout='n'+initial[pid]['cwd']+'\n' if cwd else ' '.join(initial[pid]['argv'])
            return types.SimpleNamespace(returncode=0,stdout=stdout,stderr='')
        def adb(given):
            timeout=env.get('probe_timeout',lambda:5.)()
            elapsed_probe('adb-consumer',40 if phase[0]=='term' and mode=='consumer-exhausts' else 4,timeout)
            consumer_probes.append(clock[0]);return -1
        def sdk(given):
            timeout=env.get('probe_timeout',lambda:5.)()
            elapsed_probe('sdk-consumer',4,timeout);consumer_probes.append(clock[0]);return set()
        def event(kind,**kw):
            events.append({'kind':kind,'at':clock[0],**copy.deepcopy(kw)})
            if kind=='term_window':phase[0]='term'
        def term(pid,sig):
            need(sig==15 and pid in env['owned'],'Only known TERM');signals.append({'pid':pid,'at':clock[0]});rows.pop(pid,None)
        env={'json':json,'pathlib':pathlib,'shlex':shlex,'re':__import__('re'),'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda delta:clock.__setitem__(0,clock[0]+delta)),'utc':lambda:'fixture-'+str(clock[0]),'os':types.SimpleNamespace(kill=term),'signal':types.SimpleNamespace(SIGTERM=15),'subprocess':types.SimpleNamespace(run=run),'owned':initial,'active':None,'A':{'stages':[{'id':root['stage'],'argv':root['argv']}],'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}},'P':pathlib.Path(root['cwd']),'event':event,'adb_exception':adb,'sdk_adb_exception':sdk,'resources':lambda:None,'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.}
        bind(['check','cmd','ps','details','alive','register','discover','consumer_guard','recorded_chain','recover_root','snapshot_consumers','monitor','closure','archive_and_restore'],env)
        if mode=='unverified':env['pending_details'][999999]={'row':{'ppid':rootid,'start':root['start'],'exe':'/unknown'},'firstError':'original unknown identity'};rows[999999]={'ppid':rootid,'start':'Thu Oct 8 02:01:05 2026','stat':'S','exe':'/unknown'}
        failed=rejected(lambda:env['closure'](root['stage'],rootid,'original synthetic compiler failure'))
        need(failed and env['closure_closed'] is False,'Budget exhaustion remains unclosed')
        need(clock[0]==190.,'Natural and authorization/confirmation stop at original absolute 60+30')
        windows=[x for x in events if x['kind']=='term_window'];need(len(windows)==1 and windows[0]['at']==160. and windows[0]['deadline']==190.,'Absolute TERM window begins before authorization')
        term_commands=[c for c in commands if c['phase']=='term'];need(term_commands,'Actual timed commands exercised')
        need(all(c['start']<190. and 0<c['timeout']<=min(5.,190.-c['start']) and c['end']<=190. for c in term_commands),'No expired probe and every subprocess timeout capped')
        need(all(s['at']<190. for s in signals) and len({s['pid'] for s in signals})==len(signals),'No expired/duplicate TERM')
        if mode=='many-owned':need(len(signals)==1 and len(rows)==3,'Only first fully verified identity fits in shared budget')
        else:need(not signals,'Failed or unknown authorization grants zero signals')
        before=len(commands);need(rejected(env['archive_and_restore']) and len(commands)==before,'Unclosed restore gate precedes new probes')
        need(env['probe_deadline'] is None,'Scoped deadline released after closure')
        report.setdefault('deadlineEvidence',[]).append({'mode':mode,'simulatedSeconds':clock[0]-100.,'termWindow':windows[0],'signals':signals,'commands':commands,'firstError':env['monitor_errors'][0],'remaining':len(rows),'closed':env['closure_closed']})
    for mode in ('many-owned','ps-exhausts','details-exhausts','consumer-exhausts','unverified'):
        regression('FIX05 absolute TERM deadline '+mode,lambda mode=mode:deadline_case(mode))
    need(len(cases)==102 and all(c['passed'] for c in cases),'All FIX05 regression expectations retained')
    report['prior102Passed']=True
    case('combined fixed schema and 17/4/2 source delta',lambda:need([len(N[k]) for k in ('overwritten','newPaths','parked')]==[17,4,2] and len(N['files'])==1037,'Current P01 schema'))
    xmlenv=bind(['check','validate_xml'],{'ET':ET,'Counter':Counter,'Q':Q})
    def xml_text(mode='valid'):
        top=ET.Element('test-run',result='Passed',total='89',passed='89',failed='0',skipped='0',inconclusive='0')
        names=[name for f in Q['fixtures'] for name in f['testFullnames']]
        if mode=='duplicate':names[-1]=names[0]
        if mode=='missing':names.pop()
        if mode=='extra':names.append('Foreign.Case')
        for i,name in enumerate(names):ET.SubElement(top,'test-case',fullname=name,result='Skipped' if mode=='skipped' and i==0 else 'Passed')
        if mode=='root-count':top.set('total','90')
        return ET.tostring(top,encoding='unicode')
    case('exact current 89-name XML',lambda:need(xmlenv['validate_xml'](xml_text())['passed']==89,'89 Passed'))
    for mode in ('duplicate','missing','extra','skipped','root-count'):
        case('XML rejects '+mode,lambda mode=mode:need(rejected(lambda:xmlenv['validate_xml'](xml_text(mode))),'Invalid XML counter/status'))
    def env_modes():
        env=bind(['stage_environment'],{'os':types.SimpleNamespace(environ={'FIGHTMATCH_ACTIVATION_TEST_ROOT':'foreign','BEE_CACHE_DIRECTORY':'old','UNCHANGED':'yes'}),'A':{'environmentOverrides':{'BEE_CACHE_DIRECTORY':'new','UPM_CACHE_ROOT':'K','TMPDIR':'TMP'}},'D':D})
        i=env['stage_environment']('I');t=env['stage_environment']('T')
        need('FIGHTMATCH_ACTIVATION_TEST_ROOT' not in i and t['FIGHTMATCH_ACTIVATION_TEST_ROOT']==D['paths']['activationTests'] and i['BEE_CACHE_DIRECTORY']=='new' and i['UNCHANGED']=='yes','Stage environment isolation')
        need('-quit' in D['commands']['I'] and '-quit' not in D['commands']['T'] and D['commands']['T'].count('-testFilter')==1,'Exact T no quit/filter')
    case('I/T exact argv and external environments',env_modes)
    def sequence(status):
        calls=[]
        def run(sid):calls.append(sid);return {'status':status if sid=='I' else 'TEST_PASS'}
        env=bind(['run_sequence'],{'run_stage':run});result=env['run_sequence']()
        need(calls==(['I','T'] if status=='COMPILE_PASS' else ['I']),'Only passing I reaches T')
    for status in ('COMPILE_PASS','INCOMPLETE','FAILED','NOT_RUN_BLOCKED'):
        case('I to T gate '+status,lambda status=status:sequence(status))
    def stage_reset(mode):
        original={rootid:copy.deepcopy(root)};seen=[];env={'owned':original,'pending_details':{},'active':None,'closure_closed':mode!='unclosed','monitor_errors':[],'monitor_cycles':[{'I':True}],'stage_history':[],'stages':[{'stage':'I','status':'COMPILE_PASS'}],'stage_dlls':{'I':{'assemblies':{'A':{'proven':mode!='unproven'}}}},'N':{'requiredAssemblies':['A']},'launched_root':{'pid':rootid},'ps':lambda:{rootid:{k:root[k] for k in keys}} if mode=='live' else {},'consumer_guard':lambda rows:seen.append(copy.deepcopy(env['owned'])),'copy':copy}
        bind(['check','alive','compilation_passed','isolate_stage'],env)
        failed=rejected(lambda:env['isolate_stage']('T'))
        need(failed==(mode!='normal'),'Closed proven I required')
        if mode=='normal':need(not env['owned'] and seen==[{}] and env['stage_history'][0]['owned']==original and env['closure_closed'] is None,'Historical I retained; T receives no PID exemption')
    for mode in ('normal','unclosed','unproven','live'):case('stage isolation '+mode,lambda mode=mode:stage_reset(mode))
    def compiler_fixture(label):
        p=directory(ROOT/label/'P');editor=directory(ROOT/label/'Editor/Unity.app/Contents/MacOS')/'Unity';content=editor.parent.parent
        tool1=fixture_file(content/'NetCoreRuntime/dotnet',b'dotnet');tool2=fixture_file(content/'DotNetSdkRoslyn/csc.dll',b'csc')
        fixture_file(editor,b'editor');fixture_file(p/'Assets/A.cs',b'class A {}');fixture_file(p/'Assets/A.asmdef',b'{"name":"A"}')
        fixture_file(p/'Packages/manifest.json',b'{}');fixture_file(p/'ProjectSettings/ProjectVersion.txt',b'fixture')
        sdk='Library/PackageCache/com.fixture@1';fixture_file(p/sdk/'Runtime/S.cs',b'class S {}');ref=fixture_file(p/sdk/'Runtime/R.dll',b'reference')
        rsp='Library/Bee/artifacts/g/A.rsp';fixture_file(p/rsp,('-target:library\n-r:'+str(ref)+'\nAssets/A.cs\nPackages/com.fixture/Runtime/S.cs\n').encode());fixture_file(p/(rsp+'2'),b'/pathmap:fixture=.')
        outputs=['Library/Bee/artifacts/g/A.dll','Library/Bee/artifacts/g/A.pdb','Library/Bee/artifacts/g/A.ref.dll']
        for name in outputs:fixture_file(p/name,b'output')
        fixture_file(p/'Library/ScriptAssemblies/A.dll',b'output')
        node={'DisplayName':'Csc A','Annotation':'Csc '+outputs[0],'Action':shlex.join([str(tool1),'exec',str(tool2),'@'+rsp,'@'+rsp+'2']),'Inputs':['Assets/A.cs',sdk+'/Runtime/S.cs',str(ref),rsp,rsp+'2',str(tool1),str(tool2)],'Outputs':outputs,'ToBuildDependencies':[],'ToUseDependencies':[]}
        fixture_file(p/'Library/Bee/g.dag.json',json.dumps({'Nodes':[node]}).encode())
        files={str(path.relative_to(p)):identity(path) for path in fixture_files if p in path.parents and path.parts[len(p.parts)] in ('Assets','Packages','ProjectSettings')}
        n={'files':files,'requiredAssemblies':['A'],'assemblySources':{'A':['Assets/A.cs',sdk+'/Runtime/S.cs']},'editor':{'path':str(editor)},'allowedNewSettings':{'path':'ProjectSettings/SceneTemplateSettings.json'}}
        d={'cache':{'installedPackages':{'com.fixture@1':{}},'sdkFiles':{'com.fixture@1':{'Runtime/S.cs':identity(p/sdk/'Runtime/S.cs')}}},'compilePlan':{'assemblies':{'A':{'asmdef':{'path':'Assets/A.asmdef',**identity(p/'Assets/A.asmdef')}}}}}
        env={'N':n,'D':d,'P':p,'B':{'compilerTools':{str(tool1):identity(tool1),str(tool2):identity(tool2)},'referencePreimages':{str(ref):identity(ref)}},'json':json,'pathlib':pathlib,'os':os,'stat':stat,'hashlib':hashlib,'shlex':shlex,'re':re,'stage_bindings':{},'stage_dlls':{}}
        bind(['check','ident','no_links','inventory','same','basic','source_tree','canonical','validate_inputs','frozen_text','compiler_graph','compiler_path','compiler_binding','cached_reuse_proven','compilation_passed','compile_evidence'],env)
        return env,p,ref
    def provenance(mode):
        env,p,ref=compiler_fixture(mode)
        if mode=='sdk-drift':fixture_file(p/'Library/PackageCache/com.fixture@1/Runtime/S.cs',b'changed source')
        if mode=='dll-postprocess':fixture_file(p/'Library/ScriptAssemblies/A.dll',b'unproved postprocess')
        if mode=='missing-rsp2':
            path=p/'Library/Bee/artifacts/g/A.rsp2';path.unlink();del fixture_files[path]
        if mode=='reference-rewritten':
            fixture_file(ref,b'reference changed')
        log='' if mode=='no-csc' else '[1/1] Csc Library/Bee/artifacts/g/A.dll'
        proof=env['compile_evidence'](log,'I');passed=env['compilation_passed'](proof)
        need(passed==(mode=='valid'),'Full current source/tool/reference/output chain required '+mode)
        if passed:
            need(len(proof['assemblies']['A']['response']['files'])==2,'Both rsp and rsp2 retained')
            env['stage_bindings']['T']={'A':env['compiler_binding']('A')}
            reuse=env['compile_evidence']('','T');need(env['compilation_passed'](reuse) and reuse['assemblies']['A']['cacheReuseClaimed'],'Only I complete six-field chain reused by T')
        report.setdefault('combinedCompilerEvidence',[]).append({'mode':mode,'passed':passed,'reason':proof['assemblies']['A'].get('reason'),'sourceInputs':proof['assemblies']['A'].get('sourceInputs')})
    for mode in ('valid','sdk-drift','dll-postprocess','missing-rsp2','reference-rewritten','no-csc'):
        case('compiler current provenance '+mode,lambda mode=mode:provenance(mode))
    def package_alias():
        env,p,ref=compiler_fixture('aliases')
        need(str(env['compiler_path']('Packages/com.fixture/Runtime/S.cs',True))==str(p/'Library/PackageCache/com.fixture@1/Runtime/S.cs'),'Unique SDK alias')
        need(rejected(lambda:env['compiler_path']('Packages/com.fixture/Samples~/S.cs',True)),'Samples excluded')
        env['D']['cache']['installedPackages']['com.fixture@2']={}
        need(rejected(lambda:env['compiler_path']('Packages/com.fixture/Runtime/S.cs',True)),'Ambiguous package aliases rejected')
    case('SDK source mapping is unique and excludes Samples',package_alias)
    def compiler_ledger():
        fs=FS();p=fs.path('/P');e=fs.path('/E');paths=list(D['compilePlan']['proposedParkExactLeaves']);old={path:('before '+path).encode() for path in paths}
        for path,data in old.items():fs.add(str(p/path),data)
        errno=[0]
        class Native:
            def __call__(self,src,dst,flags):
                a=fs.path(src.decode());b=fs.path(dst.decode());fs.native_calls.append(flags)
                try:
                    if flags==2:fs.swap(a,b)
                    elif flags==4:fs.commit(a,b,True)
                    else:raise AssertionError('Native flag')
                    return 0
                except OSError:errno[0]=5;return -1
        native=Native();env={'N':copy.deepcopy(N),'D':{'compilePlan':{'proposedParkExactLeaves':{path:byteid(data) for path,data in old.items()}},'limits':{'totalMechanicalSeconds':900},'evidenceSlots':{'compilerArchiveMaxBytes':33554432}},'P':p,'E':e,'clock_start':0.,'time':types.SimpleNamespace(monotonic=lambda:100.),'consumer_guard':lambda rows:None,'ps':lambda:{},'hashlib':hashlib,'no_links':lambda path:None,'ident':lambda path:byteid(path.read_bytes()),'event':lambda *a,**kw:None,'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda path:path.exists()),fsync=lambda fd:None,fsencode=lambda path:str(path).encode(),strerror=lambda code:'fixture'),'ctypes':types.SimpleNamespace(CDLL=lambda *a,**kw:types.SimpleNamespace(renamex_np=native),c_char_p=object(),c_uint=object(),c_int=object(),get_errno=lambda:errno[0])}
        bind(['check','transfer','rename_swap','rename_exclusive','park_compiler','restore_compiler'],env)
        env['park_compiler']();need(len(env['compiler_parked'])==43 and all(not (p/path).exists() for path in paths),'Only all exact43 parked')
        for path in paths[::2]:fs.add(str(p/path),('after '+path).encode())
        result=env['restore_compiler']()
        need(result['complete'] and len(result['restored'])==43 and len(result['archived'])==22 and all((p/path).read_bytes()==old[path] for path in paths),'Exact compiler preimages restored; replacements archived; no-generated paths returned')
        need(all('TundraBuildState.state' not in a and 'bee_backend.info' not in a for a in env['compiler_parked']),'Preserved in-place state never parked')
        report['compilerLedger']={'parked':43,'restored':43,'archived':22,'primitiveFlags':sorted(set(x for x in fs.native_calls)),'actualCacheWrites':0}
    case('actual 43-path compiler park and recovery ledger',compiler_ledger)
    def as_links():
        root=directory(ROOT/'as'/'state-tests');external=fixture_file(root/'AS07/product/external.bin',bytes([17,0,255]));link=root/'AS07/product/resource-state/v1/active.json'
        env={'ASROOT':root,'os':os,'pathlib':pathlib,'stat':stat,'D':D}
        bind(['check','no_links','as_guard'],env)
        for target in (external,root/'AS07/product/absent-target'):
            fixture_link(link,target);need(env['as_guard']()['noLinkFollow'],'Allowed AS links observed without following')
        fixture_link(link,root/'foreign');need(rejected(env['as_guard']),'Foreign AS target rejected')
        need(rejected(lambda:env['as_guard'](True)),'Residual AS tree never proves final empty')
    case('AS metadata guard permits only exact links and retains residue',as_links)
    original_cases=json.loads((v00/'replay-results.json').read_text())['rounds'][0]['cases']
    need(len(cases)==127 and [(c['name'],c['passed']) for c in cases]==[(c['name'],c['passed']) for c in original_cases],'All original127 names and expectations retained')
    report['prior127Passed']=True
    def fix01_work_reserve():
        clock=[630.];env=bind(['check'],{'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'work_deadline':630.,'execution_deadline':780.})
        need(rejected(env['probe_timeout']),'No ordinary probe may enter the150-second reserve')
        clock[0]=629.;need(env['probe_timeout']()==1.,'Last ordinary probe bounded by work deadline')
    case('FIX01 ordinary work cannot probe into 150-second cleanup reserve',fix01_work_reserve)
    def fix01_initial_clock():
        clock=[100.];body=F['main'].body
        assignments=[n for n in body if isinstance(n,ast.Assign) and isinstance(n.targets[0],ast.Name) and n.targets[0].id in ('clock_start','execution_deadline','work_deadline')]
        need([n.targets[0].id for n in assignments]==['clock_start','execution_deadline','work_deadline'],'One origin; no reset before preflight')
        need(body[1] is assignments[0],'Clock starts before activation/input reads')
        env={'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'D':{'limits':{'totalMechanicalSeconds':900,'restoreSeconds':60,'finalizationSeconds':0}},'PREF':{'mechanicalPreparationSeconds':120},'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}}}
        exec(compile(ast.Module(body=[copy.deepcopy(assignments[0])],type_ignores=[]),RUNNER_FILENAME,'exec'),env);clock[0]+=20
        exec(compile(ast.Module(body=copy.deepcopy(assignments[1:]),type_ignores=[]),RUNNER_FILENAME,'exec'),env)
        need((env['clock_start'],env['execution_deadline'],env['work_deadline'])==(100.,880.,730.),'Preparation and20-second activation reads reduce remaining work; never grant new900')
        report['FIX01Clock']={'origin':100,'activationReadsEnd':120,'preparation':120,'workDeadline':730,'hardDeadline':880,'remainingWorkAfterReads':610,'cleanupReserve':150}
    case('FIX01 preparation and activation reads share original total clock',fix01_initial_clock)
    def fix01_sync_expiry():
        fs=FS();clock=[629.];moves=[];files={'a.cs':byteid(b'a'),'b.cs':byteid(b'b')}
        def transfer(source,target,identity,move=False):moves.append(str(source));clock[0]=630.
        env={'json':json,'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'work_deadline':630.,'execution_deadline':780.,'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda p:False)),'P':fs.path('/P'),'R':fs.path('/R'),'E':fs.path('/E'),'N':{'overwritten':list(files),'restoreBaseline':files,'shared':files},'synced':False,'ps':lambda:{},'consumer_guard':lambda rows:None,'source_tree':lambda p:files,'projection_guard':lambda:None,'transfer':transfer}
        bind(['check','basic','same','validate_inputs','synchronize'],env)
        need(rejected(env['synchronize']) and moves==['/P/a.cs'] and not env['synced'],'Sync stops before second transfer at work deadline')
        need(clock[0]==630. and env['execution_deadline']==780.,'Sync does not reset clock or consume reserve')
    case('FIX01 synchronization stops at work deadline before next transfer',fix01_sync_expiry)
    def fix01_stage(sid,mode,final_seconds=0,scan_work=False,check_environment=False):
        fs=FS();p=fs.path('/P');e=fs.path('/E');clock=[(270. if sid=='I' else 450.)-20.];rows={};calls=[];signals=[];events=[];writes={};transfers=[];popens=[]
        if mode=='insufficient':clock[0]+=1
        argv=[root['exe'],'-batchmode','-projectPath',str(p)];source='Assets/park.cs';compiler='Library/compiled.dll';oldsource=b'original source';oldcompiler=b'original compiler'
        fs.add(str(e/'park/source'/source),oldsource);fs.add(str(e/'park/compiler'/compiler),oldcompiler)
        class Proc:
            pid=rootid
            returncode=None
            def poll(self):
                if mode=='evidence-exhaustion' or (mode!='unclosed' and signals and clock[0]>=719.5):self.returncode=0;rows.clear()
                return self.returncode
        proc=Proc()
        def ps():
            timeout=env['probe_timeout']();calls.append({'at':clock[0],'timeout':timeout,'deadline':env['probe_deadline'] if env['probe_deadline'] is not None else env['work_deadline']})
            return copy.deepcopy(rows)
        def popen(args,**kw):
            need(args==argv and kw['cwd'] is p,'Exact fake stage binding')
            if check_environment:
                expected={'UNCHANGED':'parent',**env['A']['environmentOverrides']}
                if sid=='T':expected.update(env['D']['commands']['environmentTOnly'])
                need(kw['env']==expected and kw['env']['DOTNET_EnableDiagnostics']=='0','Actual Popen exact diagnostic0 environment overrides parent1')
                report.setdefault('V02PopenEnvironments',[]).append({'stage':sid,'environment':dict(kw['env'])})
            popens.append(clock[0]);fs.add(str(e/sid/'editor.log'),b'fixture compiler output');rows[rootid]={k:root[k] for k in keys};return proc
        def recover(given,stage,pid):env['owned'][pid]={**copy.deepcopy(root),'argv':argv,'cwd':str(p),'stage':stage,'rootPid':pid,'termSent':False}
        def details(pid):env['probe_timeout']();return {'argv':argv,'cwd':str(p),'cwdProbeExit':0}
        def monitor(phase,given,stage,pid,**kw):
            env['probe_timeout']()
            if phase=='running':
                if scan_work:
                    need(env['active'] is proc and proc.poll() is None,'Unity simulated live during chunk scan');clock[0]=629.;fix02_hash(clock,630.,780.+final_seconds)
                else:clock[0]=min(630.,clock[0]+30.)
            return {'failures':[]}
        def preflight():clock[0]=630. if mode=='preflight-exhaustion' else clock[0]+20.
        def transfer(a,b,expected,move=False):
            env['probe_timeout']();need(byteid(a.read_bytes())==expected and not b.exists(),'Exact synthetic restore preimage/no-clobber');b.write_bytes(a.read_bytes())
            if move:fs.files.pop(str(a))
            transfers.append({'source':str(a),'target':str(b),'at':clock[0]});clock[0]+=20.
        def evidence(log,stage):clock[0]=630.;return {'assemblies':{}}
        env={'json':json,'pathlib':pathlib,'copy':copy,'time':types.SimpleNamespace(monotonic=lambda:clock[0],time=lambda:1791413008.,sleep=lambda d:clock.__setitem__(0,clock[0]+d)),'utc':lambda:'fixture-'+str(clock[0]),'P':p,'E':e,'work_deadline':630.,'execution_deadline':780.+final_seconds,'probe_deadline':None,'clock_start':70. if final_seconds else 0.,'A':{'stages':[{'id':k,'argv':argv,'timeoutSeconds':360 if k=='I' else 180} for k in ('I','T')],'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30},'editor':{'path':root['exe']},'environmentOverrides':{}},'D':{'limits':{'restoreSeconds':60,'finalizationSeconds':final_seconds},'compilePlan':{'proposedParkExactLeaves':{compiler:byteid(oldcompiler)}},'evidenceSlots':{'compilerArchiveMaxBytes':33554432},'commands':{'environmentTOnly':{}}},'N':{'requiredAssemblies':[],'overwritten':[],'newPaths':[],'restoreBaseline':{source:byteid(oldsource)},'allowedNewSettings':{'path':'ProjectSettings/SceneTemplateSettings.json'}},'owned':{},'pending_details':{},'active':None,'root_launch_epoch':None,'launch_attempts':1 if sid=='T' else 0,'launch_counts':{'I':1} if sid=='T' else {},'stages':[{'stage':'I','status':'COMPILE_PASS','runCount':1}] if sid=='T' else [],'stage_dlls':{'I':{'assemblies':{}}},'closure_closed':True if sid=='T' else None,'synced':True,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.,'launched_root':None,'synchronized_paths':[],'parked_paths':[source],'compiler_parked':[compiler],'compiler_after':{},'compiler_restored':[],'atomic_conflicts':set(),'subprocess':types.SimpleNamespace(Popen=popen,STDOUT=-2),'signal':types.SimpleNamespace(SIGTERM=15),'os':types.SimpleNamespace(environ={},kill=lambda pid,sig:signals.append({'pid':pid,'signal':sig,'at':clock[0]})),'ps':ps,'details':details,'recover_root':recover,'monitor':monitor,'recorded_chain':lambda pid,rpid:need(pid==rpid==rootid,'Owned root only'),'snapshot_consumers':lambda *a:env['probe_timeout'](),'consumer_guard':lambda rows:env['probe_timeout'](),'event':lambda kind,**kw:events.append({'kind':kind,'at':clock[0],**copy.deepcopy(kw)}),'write':lambda name,value:writes.update({name:copy.deepcopy(value)}),'preflight':preflight,'log_guard':lambda log:None,'resources':lambda:None,'projection_guard':lambda:None,'protection':lambda:None,'compile_evidence':evidence,'ident':lambda path:byteid(path.read_bytes()),'transfer':transfer}
        if check_environment:
            env['os'].environ={'DOTNET_EnableDiagnostics':'1','UNCHANGED':'parent','UPM_CACHE_ROOT':'old','TMPDIR':'old','BEE_CACHE_DIRECTORY':'old','FIGHTMATCH_ACTIVATION_TEST_ROOT':'old'}
            env['A']['environmentOverrides']={'UPM_CACHE_ROOT':'fixed-K','TMPDIR':'fixed-TMP2','BEE_CACHE_DIRECTORY':'fixed-BC2','DOTNET_EnableDiagnostics':'0'}
            env['D']['commands']['environmentTOnly']={'FIGHTMATCH_ACTIVATION_TEST_ROOT':'fixed-AS2'}
        bind(['check','alive','claim_stage','compilation_passed','run_stage','run_sequence','closure','restore_all','restore_compiler','archive_and_restore'],env)
        result=env['run_sequence']() if mode=='evidence-exhaustion' else env['run_stage'](sid)
        failure=result['failure'];closed=env['closure_closed'];before_restore=clock[0]
        if mode in ('insufficient','preflight-exhaustion'):
            need(not popens and result['status']=='NOT_RUN_BLOCKED' and result['runCount']==0 and failure=='Insufficient work window for '+sid+' with cleanup reserve','No stage starts without its complete configured work allocation')
        elif mode=='evidence-exhaustion':
            need(len(popens)==1 and result['status']=='FAILED' and env['launch_counts']=={'I':1} and failure=='Closure probe deadline exhausted','Expired actual compile evidence retains failure and cannot launch T')
        else:
            expected='Closure probe deadline exhausted' if scan_work else sid+' stage deadline exceeded'
            need(len(popens)==1 and result['status']=='FAILED' and failure.startswith(expected),'Original work timeout remains failure')
            need([x for x in events if x['kind']=='closure_begin'][0]['reason']==expected,'Exact original failure passed to closure')
            need(signals==[{'pid':rootid,'signal':15,'at':690.}],'One owned TERM inside reserved absolute window')
            need(all(c['at']<c['deadline'] and c['timeout']<=min(5.,c['deadline']-c['at']) for c in calls),'Every actual probe bounded; none begins at expired deadline')
            if mode=='unclosed':
                need(clock[0]==720. and closed is False and env['active'] is proc,'Unclosed root retained at cleanup deadline')
                count=len(calls);need(rejected(env['restore_all']) and len(calls)==count and not transfers,'No restore/probes after unclosed cleanup deadline')
            else:need(failure==expected and closed is True and clock[0]==719.5 and env['active'] is None,'Successful cleanup never rewrites or duplicates exact timeout')
        if mode!='unclosed':
            recovery=env['restore_all']()
            need(recovery['complete'] and len(transfers)==2 and (p/source).read_bytes()==oldsource and (p/compiler).read_bytes()==oldcompiler,'Actual compiler and source restoration run in reserved scope')
            need(env['restore_deadline']==min(before_restore+60.,780.) and clock[0]<env['restore_deadline'],'Single shared restore60 stays within hard900 including preparation')
            need(result['failure']==failure,'Restoration retains exact original stage failure')
            restore_end=clock[0];clock[0]=780.;count=len(calls);moved=len(transfers)
            need(rejected(env['restore_all']) and len(calls)==count and len(transfers)==moved,'Hard deadline cannot be reset by another restore call')
        else:restore_end=None
        report.setdefault('FIX01DeadlineEvidence',[]).append({'stage':sid,'mode':mode,'popenTimes':popens,'failure':failure,'closureClosed':closed,'cleanupFinishedAt':before_restore,'restorationFinishedAt':restore_end,'hardDeadline':780+final_seconds,'preparationSeconds':160 if final_seconds else 120,'workDeadline':630,'finalizationReserve':final_seconds,'scanWhileLive':scan_work,'signals':signals,'probeCount':len(calls),'probes':calls,'restoredTransfers':transfers,'nativeRuns':0})
    for sid in ('I','T'):
        for mode in ('work-exhaustion','insufficient','preflight-exhaustion'):
            case('FIX01 '+sid+' '+mode+' preserves cleanup and restore allocation',lambda sid=sid,mode=mode:fix01_stage(sid,mode))
    case('FIX01 expired I evidence fails and suppresses T',lambda:fix01_stage('I','evidence-exhaustion'))
    case('FIX01 no probe or TERM after absolute cleanup deadline; unclosed root retained',lambda:fix01_stage('I','unclosed'))
    v01cases=json.loads((v01/'replay-results.json').read_text())['rounds'][-1]['cases']
    need(len(cases)==138 and [(c['name'],c['passed']) for c in cases]==[(c['name'],c['passed']) for c in v01cases],'All prior138 names and expectations retained')
    report['prior138Passed']=True
    def fix02_hash(clock,work,hard,scope=None,read_cost=1.,record=None):
        reads=[];stamp=types.SimpleNamespace(st_mode=stat.S_IFREG,st_nlink=1,st_dev=1,st_ino=1,st_size=3,st_mtime_ns=1)
        class Stream:
            def __enter__(self):return self
            def __exit__(self,*a):pass
            def fileno(self):return 1
            def read(self,n):clock[0]+=read_cost;reads.append(n);return b'a' if len(reads)<=3 else b''
        class Leaf:
            def lstat(self):return stamp
            def open(self,mode):need(mode=='rb','Read only');return Stream()
        env=bind(['check','ident'],{'pathlib':types.SimpleNamespace(Path=lambda p:p),'stat':stat,'os':types.SimpleNamespace(fstat=lambda fd:stamp),'hashlib':hashlib,'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'work_deadline':work,'execution_deadline':hard,'probe_deadline':scope})
        try:return env['ident'](Leaf())
        finally:
            if record is not None:record.update(reads=reads,seconds=clock[0])
    def hash_case(mode):
        clock=[0.];observed={};scope=10. if mode=='cleanup-scope' else None
        if mode=='already-expired':clock[0]=2.
        try:value=fix02_hash(clock,2.,182.,scope,4. if mode=='kernel-stall' else 1.,observed);expired=False
        except RuntimeError as error:expired=True;need(str(error)=='Closure probe deadline exhausted','Exact hash expiry')
        if mode=='cleanup-scope':need(not expired and value==byteid(b'aaa') and len(observed['reads'])==4,'Scoped cleanup can hash beyond ordinary work deadline')
        else:need(expired and len(observed['reads'])==({'already-expired':0,'chunks':2,'kernel-stall':1}[mode]),'No next chunk after detected expiry')
        need(all(n==1048576 for n in observed['reads']),'Every read bounded to1MiB')
        report.setdefault('FIX02HashEvidence',[]).append({'mode':mode,'expired':expired,**observed})
    for mode in ('already-expired','chunks','cleanup-scope','kernel-stall'):case('FIX02 actual hash '+mode,lambda mode=mode:hash_case(mode))
    def scan_case(name,iteration=False):
        clock=[0.];enumerated=[];metadata=[]
        class Node(pathlib.PurePosixPath):
            def is_symlink(self):return False
            def exists(self):return True
            def is_dir(self):return True
            def is_file(self):clock[0]+=1.;metadata.append(str(self));return True
            def lstat(self):clock[0]+=1.;metadata.append(str(self));return types.SimpleNamespace(st_mode=stat.S_IFREG,st_size=1,st_nlink=1)
        class Entries:
            def __enter__(self):return self
            def __exit__(self,*a):pass
            def __iter__(self):return self
            def __next__(self):
                if len(enumerated)==8:raise StopIteration
                name='f'+str(len(enumerated));enumerated.append(name)
                if iteration:clock[0]+=.5
                return types.SimpleNamespace(name=name,is_dir=lambda follow_symlinks:False)
        def identity_stub(path):clock[0]+=1.;metadata.append(str(path));return byteid(b'x')
        env=bind(['check',name],{'pathlib':types.SimpleNamespace(Path=Node),'os':types.SimpleNamespace(scandir=lambda root:Entries()),'stat':stat,'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'work_deadline':2.,'execution_deadline':182.,'no_links':lambda path:None,'ident':identity_stub,'E':Node('/scan'),'O':Node('/scan')})
        fn=env[name];need(rejected(lambda:fn() if name in ('old_evidence','evidence_paths') else fn(Node('/scan'))),'Actual scanner stops inside iteration/metadata sequence')
        need(clock[0]==2. and (len(enumerated)==4 if iteration else len(metadata)==2),'Expiry detected before next entry/metadata read')
        report.setdefault('FIX02ScanEvidence',[]).append({'function':name,'directoryIteration':iteration,'seconds':clock[0],'enumerated':len(enumerated),'metadata':len(metadata)})
    for name in ('size','inventory','tree_entries','old_evidence','evidence_paths'):case('FIX02 '+name+' bounds metadata iteration',lambda name=name:scan_case(name))
    case('FIX02 directory enumeration checks before os.scandir finishes a large directory',lambda:scan_case('size',True))
    case('FIX02 live Unity scan expiry retains closure90 restore60 and finalization30',lambda:fix01_stage('I','work-exhaustion',30,True))
    def fix02_clock():
        nodes=[n for n in F['main'].body if isinstance(n,ast.Assign) and isinstance(n.targets[0],ast.Name) and n.targets[0].id in ('execution_deadline','work_deadline')]
        env={'clock_start':70.,'D':D,'PREF':{'mechanicalPreparationSeconds':160},'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}}}
        exec(compile(ast.Module(body=copy.deepcopy(nodes),type_ignores=[]),RUNNER_FILENAME,'exec'),env)
        need((env['execution_deadline'],env['work_deadline'])==(810.,630.) and env['execution_deadline']-70+160==900,'160 carried preparation and180 reserved cleanup/finalization within900')
        report['FIX02Clock']={'origin':70,'preparation':160,'workDeadline':630,'closureDeadline':720,'restoreDeadline':780,'executionDeadline':810,'total':900}
    case('FIX02 revised main clock reserves30 for finalization inside900',fix02_clock)
    def finalization_case(mode):
        fs=FS();clock=[0.];opened=[None];events=[];stdout=[]
        class TimedPath(Path):
            def __truediv__(self,v):return TimedPath(self.fs,self.p+'/'+str(v))
            def lstat(self):return types.SimpleNamespace(st_mode=stat.S_IFREG,st_nlink=1,st_dev=9,st_ino=1,st_size=len(self.fs.files[self.p]),st_mtime_ns=1)
            def open(self,kind):
                stream=super().open(kind);opened[0]=self;rawread=stream.read;rawwrite=stream.write;rawclose=stream.close
                def read(n=-1):
                    value=rawread(n);clock[0]+=31. if (mode=='evidence-hash' and self.p!='/E/receipt.json') or (mode=='receipt-hash' and self.p=='/E/receipt.json') else .1;events.append(['read',self.p,clock[0]]);return value
                def write(value):result=rawwrite(value);clock[0]+=.001;return result
                def close():
                    if not stream.closed:
                        rawclose()
                        if 'r' not in kind:clock[0]+=31. if mode=='receipt-write' and self.p=='/E/receipt.json' else .05;events.append(['write-close',self.p,clock[0]])
                stream.read=read;stream.write=write;stream.close=close;return stream
        e=TimedPath(fs,'/E');fs.add('/E/I/result.json',b'{"status":"TEST_PASS"}')
        class SlowEncoder(json.JSONEncoder):
            def iterencode(self,value,*a,**kw):
                for part in super().iterencode(value,*a,**kw):clock[0]+=31.;yield part
        def output(text,flush):stdout.append(json.loads(text));clock[0]+=31. if mode=='stdout' else .1
        env=bind(['check','write','ident','finalize'],{'pathlib':types.SimpleNamespace(Path=lambda p:p),'stat':stat,'os':types.SimpleNamespace(fstat=lambda fd:opened[0].lstat()),'hashlib':hashlib,'json':types.SimpleNamespace(JSONEncoder=SlowEncoder,dumps=json.dumps) if mode=='serialization' else json,'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'clock_start':0.,'work_deadline':-180.,'execution_deadline':30.,'probe_deadline':30.,'utc':lambda:'fixture','E':e,'allowed':{'compile.json','after.json','receipt.json'},'PREF':{'mechanicalPreparationSeconds':160},'evidence_paths':lambda:[TimedPath(fs,p) for p in sorted(fs.files)],'evidence_name':lambda p:p.p[3:],'evidence_identity':lambda p:env['ident'](p),'print':output})
        receipt={'task':'RES-COMBINED-V01','status':'FAILED' if mode=='original-failure' else 'TEST_PASS','failure':'original compiler timeout' if mode=='original-failure' else None}
        code=env['finalize'](receipt,[('compile.json',{'ok':True}),('after.json',{})]);out=stdout[-1]
        need(code==(0 if mode=='healthy' else 1),'Only complete within-deadline finalization may return0')
        need(out['status']!='TEST_PASS' and all(json.loads(b).get('status')!='TEST_PASS' for p,b in fs.files.items() if p=='/E/receipt.json' and b.strip().endswith(b'}')),'No final receipt/stdout independently claims TEST_PASS')
        if mode=='healthy':
            saved=json.loads(fs.files['/E/receipt.json']);need(0<saved['mechanicalExecutionSecondsThroughEvidence']<out['mechanicalExecutionSecondsThroughReceiptHash']<clock[0]<30.,'Evidence hash, receipt write/hash, stdout all consume observed time')
            need(out['receipt']==byteid(fs.files['/E/receipt.json']) and out['status']=='AWAITING_PROCESS_EXIT','Exact receipt hash and external exit required')
        elif mode=='original-failure':need(out['failure']=='original compiler timeout','Original failure retained exactly')
        elif mode=='stdout':need(out['status']=='AWAITING_PROCESS_EXIT' and clock[0]>30. and code==1,'Post-flush deadline forces nonzero even after candidate completion record')
        else:need(out['status']=='FINALIZATION_FAILED' and 'finalization:' in out['failure'] and clock[0]>=30.,'Finalization overrun stays failed and evidence is retained')
        report.setdefault('FIX02FinalizationEvidence',[]).append({'mode':mode,'exitCode':code,'secondsThroughPrintReturn':clock[0],'completion':out,'writeReadEvents':events,'retainedPaths':sorted(fs.files)})
    for mode in ('healthy','serialization','evidence-hash','receipt-write','receipt-hash','stdout','original-failure'):case('FIX02 actual finalization '+mode,lambda mode=mode:finalization_case(mode))
    def stable_tree_digest():
        value={'unicode':'中文','nested':{'b':[1,True,None],'a':'x'*70000}};env=bind(['check','json_digest'],{'json':json,'hashlib':hashlib})
        need(env['json_digest'](value)==hashlib.sha256(json.dumps(value,sort_keys=True,separators=(',',':')).encode()).hexdigest(),'Bounded JSON hashing preserves original canonical bytes')
    case('FIX02 chunked tree fingerprint preserves exact canonical hash',stable_tree_digest)
    prior_cases=json.loads((v02_base/'replay-results.json').read_text())['rounds'][-1]['cases']
    need(len(cases)==158 and [(c['name'],c['passed']) for c in cases]==[(c['name'],c['passed']) for c in prior_cases],'All original158 names and expectations retained')
    report['prior158Passed']=True
    prior_seals={'runner.py':{'bytes':81937,'sha256':'1a30b4c6e1a5807664d047635c4fc20b131fe9d15191b128656dc366612aa7ba'},'replay-check.py':{'bytes':98829,'sha256':'6b917019c5ad7354808dc38bbeaa128d5461e18e1dabe55187af3a633ac88175'},'replay-results.json':{'bytes':346894,'sha256':'b038021836165db8ca356e2b74c9b69875268199ba09dbc1402896eb253feddd'},'inputs.json':{'bytes':1241588,'sha256':'30e2254fba0f1ee5f0ac82214d49690b6aab3401d151dee765ac9ddbbb0865fb'},'preparation.json':{'bytes':353328,'sha256':'5d90f8ca8e91b081490faa8d963826ad828f6980ffe1c0c80d5cbb7cd0a6d336'}}
    def v02_scope():
        for name,value in prior_seals.items():need(identity(v02_base/name)==value,'Read-only V01 '+name)
        previous={n.name:n for n in ast.parse((v02_base/'runner.py').read_text()).body if isinstance(n,ast.FunctionDef)}
        changed={n for n in previous if ast.dump(previous[n])!=ast.dump(F[n])}
        need(changed=={'compile_evidence','contract_guard','main','preflight','stage_environment'} and set(F)-set(previous)=={'csc_events'},'Only current bindings, environment and Csc classification changed')
        for name in ('tree_entries','monitor','closure','restore_all','restore_compiler','archive_and_restore','finalize','probe_timeout','ident','checked','read_chunks','scan_tree'):
            need(ast.dump(previous[name])==ast.dump(F[name]),'Exact retained supervisor/IPC/deadline guard '+name)
        report['V02ChangedFunctions']=sorted(changed);report['V02NewFunctions']=['csc_events'];report['V01Seals']=prior_seals
    case('V02 exact five-function delta and unchanged IPC ownership deadlines',v02_scope)
    def v02_inputs():
        before=json.loads((v02_base/'inputs.json').read_text());expected=copy.deepcopy(before);oldE=before['paths']['executionEvidence'];newE=str(E);newBC=str(E.parent/'bee-cache');newAS=str(R/'TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V02/state-tests')
        expected['task']='RES-COMBINED-V02';expected['owner']={'role':'source implementer','thread':'01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c','host':'local','turn':'01a118ca-e3be-7183-a22a-69906300c877','requestedModel':'gpt-6-astra','requestedEffort':'xhigh','returnTo':'01a0e401-511d-79f2-b47f-3ab0ade1681b/local'}
        for key,value in [('executionEvidence',newE),('newBeeCache',newBC),('activationTests',newAS)]:expected['paths'][key]=value;expected['newRootPreconditions'][key]['path']=value
        expected['evidenceSlots']['root']=newE
        for sid in ('I','T'):
            for i,value in enumerate(expected['commands'][sid]):
                if value.startswith(oldE+'/'):expected['commands'][sid][i]=newE+value[len(oldE):]
        expected['commands']['environmentBoth'].update(TMPDIR='fresh canonical /private/tmp/fm-rcv2.XXXXXXXX, actual path sealed at activation',BEE_CACHE_DIRECTORY=newBC,DOTNET_EnableDiagnostics='0');expected['commands']['environmentTOnly']['FIGHTMATCH_ACTIVATION_TEST_ROOT']=newAS
        for row in expected['transferPlan']:
            for key in ('backup','archive','park','archiveReplacement'):
                if row.get(key) is not None:need(row[key].startswith(oldE+'/'),'Only current destination prefix');row[key]=newE+row[key][len(oldE):]
        need(expected==D,'All input fields accounted for; no source/reference rewrite')
        report['V02InputIdentity']=identity(E/'inputs.json');report['V02InputFixedSections']=['fixedReferences','shared','projectionBefore','projectionProposed','cache','compilePlan','testCases','limits','preparation','editor','resourceSourceGate']
        need(before['fixedReferences']==D['fixedReferences'] and len(D['shared']['files'])==1036 and len(D['projectionBefore']['files'])==1035 and len(N['files'])==1037,'Original identities retained')
    case('V02 input changes are exactly current destinations task owner and environment',v02_inputs)
    for sid in ('I','T'):case('V02 actual '+sid+' Popen receives diagnostics0 overriding parent1',lambda sid=sid:fix01_stage(sid,'work-exhaustion',check_environment=True))
    def environment_gates():
        expected={'UPM_CACHE_ROOT':'/K','TMPDIR':'/TMP2','BEE_CACHE_DIRECTORY':'/BC2','DOTNET_EnableDiagnostics':'0'};observed=[]
        for name,label in [('preflight','Exact shared environment'),('main','Environment gate')]:
            statement=next(n for n in ast.walk(F[name]) if isinstance(n,ast.Expr) and isinstance(n.value,ast.Call) and isinstance(n.value.func,ast.Name) and n.value.func.id=='check' and len(n.value.args)==2 and isinstance(n.value.args[1],ast.Constant) and n.value.args[1].value==label)
            code=compile(ast.Module(body=[copy.deepcopy(statement)],type_ignores=[]),RUNNER_FILENAME,'exec');env=bind(['check'],{'K':'/K','TMP':'/TMP2','BC':'/BC2','A':{'environmentOverrides':dict(expected)}});exec(code,env)
            for variant in ('missing','one','integer','extra'):
                bad=dict(expected)
                if variant=='missing':bad.pop('DOTNET_EnableDiagnostics')
                elif variant=='one':bad['DOTNET_EnableDiagnostics']='1'
                elif variant=='integer':bad['DOTNET_EnableDiagnostics']=0
                else:bad['FOREIGN']='x'
                env['A']['environmentOverrides']=bad;need(rejected(lambda:exec(code,env)),'Exact activation environment rejects '+variant)
            observed.append(name)
        report['V02EnvironmentGateStatements']=observed
    case('V02 preflight and main require exact string0 environment mapping',environment_gates)
    def illegal_ipc(mode,expected):
        fifo_case(mode);errors=report['fifoEvidence'][-1]['errors'];need(any(expected in item['error'] for item in errors),'Original special-entry/FIFO-executable failure retained')
    case('V02 diagnostic socket remains forbidden',lambda:illegal_ipc('diagnostic-socket','Special tree entry'))
    case('V02 ILPP FIFO remains forbidden',lambda:illegal_ipc('ilpp-fifo','Unknown FIFO executable'))
    native_log_path=v02_base/'I/editor.log';need(identity(native_log_path)=={'bytes':75322,'sha256':'ff3d89d412f814e0b12f325d00553e6f25c4cc2e5908ef17387e4e0ec900dc70'},'Exact failed V01 log')
    native_log=native_log_path.read_text();native_lines=native_log.splitlines();pairs=[(i,native_lines[i-1],line) for i,line in enumerate(native_lines) if 'Csc Library/Bee/' in line and line.endswith('[CacheWrite ]')]
    need(len(pairs)==4,'Four observed SDK telemetry pairs')
    def original_log_classification():
        env=bind(['check','csc_events'],{'re':re});events=env['csc_events'](native_log);actions=[v for v in events if v['kind']=='ACTION'];writes=[v for v in events if v['kind']=='CACHE_WRITE']
        need(len(actions)==12 and len(writes)==4 and Counter(pathlib.PurePosixPath(v['outputPath']).stem for v in actions)==Counter(N['requiredAssemblies']),'Original twelve actions and four telemetry rows distinguished')
        report['V02OriginalLogClassification']={'identity':identity(native_log_path),'actions':[{'line':v['line'],'outputPath':v['outputPath']} for v in actions],'cacheWrites':[{'line':v['line'],'actionLine':v['actionLine'],'outputPath':v['outputPath']} for v in writes],'nativeCompileProofClaimed':False}
    case('V02 actual failed log contains twelve actions and four associated CacheWrite rows',original_log_classification)
    def csc_model(action,telemetry,mode='valid'):
        path=re.search(r'Csc (Library/Bee/\S+\.dll)',action).group(1);name=pathlib.PurePosixPath(path).stem;stamp=byteid(b'fixture DLL');binding_path=path.replace('/artifacts/','/wrong-node/') if mode=='node-mismatch' else path
        outputs={value:{'identity':stamp} for value in [binding_path,binding_path[:-4]+'.pdb',binding_path[:-4]+'.ref.dll']};binding={'complete':True,'response':{'outputs':outputs},'sourceInputs':{},'references':{},'defines':[],'compilerOutput':stamp,'dll':stamp,'outputPath':binding_path,'outputMtimeNs':10,'nodeIndex':0}
        graph={'Nodes':[{'ToBuildDependencies':[],'ToUseDependencies':[]}],'_sealedIdentity':stamp};p=pathlib.Path('/synthetic-P');graphpath=p/'Library/Bee/fixture.dag.json'
        env=bind(['check','basic','same','validate_inputs','canonical','cached_reuse_proven','compilation_passed','compile_evidence'],{'json':json,'re':re,'hashlib':hashlib,'pathlib':pathlib,'P':p,'N':{'requiredAssemblies':[name],'files':{},'allowedNewSettings':{'path':'ProjectSettings/SceneTemplateSettings.json'}},'source_tree':lambda *a:{},'compiler_graph':lambda:(graphpath,graph),'compiler_binding':lambda *a:copy.deepcopy(binding),'compiler_path':lambda value:p/value,'ident':lambda value:stamp,'B':{'referencePreimages':{}},'stage_bindings':{},'stage_dlls':{}})
        log=action+'\n'+telemetry
        if mode=='double-action':log=log+'\n'+log
        elif mode=='cache-read':log=action+'\n'+telemetry.replace('CacheWrite','CacheRead')
        elif mode=='unknown':log=action+' [Unknown ]'
        elif mode=='orphan-cache':log=telemetry
        elif mode=='duplicate-telemetry':log=log+'\n'+telemetry
        elif mode=='others-mismatch':log=log.replace('(+2 others)','(+3 others)')
        failure=None
        try:proof=env['compile_evidence'](log,'I');passed=env['compilation_passed'](proof)
        except RuntimeError as error:failure=str(error);proof=None;passed=False
        need(passed==(mode=='valid'),'Actual compile_evidence classification gate '+mode)
        if mode=='valid':need(proof['assemblies'][name]['actualCsc'] and len(proof['events'])==2 and proof['events'][1]['actionLine']==proof['events'][0]['line'],'One action with associated telemetry, no deduplication')
        if mode=='double-action':need(proof is not None and len([v for v in proof['events'] if v['kind']=='ACTION'])==2 and not proof['assemblies'][name]['actualCsc'],'Both real actions remain visible and fail')
        report.setdefault('V02CscCases',[]).append({'assembly':name,'mode':mode,'passed':passed,'failure':failure,'actualFunctions':['csc_events','compile_evidence','compilation_passed'],'bindingSource':'Synthetic isolated classification binding; existing full provenance tests retained.'})
    for _,action,telemetry in pairs:
        name=pathlib.PurePosixPath(re.search(r'Csc (Library/Bee/\S+\.dll)',action).group(1)).stem
        case('V02 real '+name+' action plus CacheWrite passes classification',lambda action=action,telemetry=telemetry:csc_model(action,telemetry))
    for mode in ('double-action','cache-read','unknown','orphan-cache','duplicate-telemetry','node-mismatch','others-mismatch'):
        case('V02 Csc rejects '+mode,lambda mode=mode:csc_model(pairs[0][1],pairs[0][2],mode))
    frozen_unchanged();need(all(c['passed'] for c in cases),'Retained regression failed');need(not forbidden,'No forbidden operations');report['status']='SOURCE_REPLAY_PASS'
except BaseException as error:
    failure=str(error);report['failure']=failure;report['traceback']=traceback.format_exc()
finally:
    sys.setprofile(None)
    try:cleanup()
    except BaseException as error:conflicts.append('cleanup: '+str(error))
    report['fixtureFiles']={str(path.relative_to(E)):value for path,value in fixture_files.items()};report['fixtureLinks']={str(path.relative_to(E)):value for path,value in fixture_links.items()};report['preservedConflicts']=conflicts
    if conflicts:report['status']='SOURCE_REPLAY_FAILED';report['failure']='Unknown or changed fixture contents preserved'
    report['seconds']=time.monotonic()-START;report['actualFunctionsCalled']=sorted(traces);report['forbiddenAttempts']=forbidden;report['runner']=identity(E/'runner.py');report['checker']=identity(E/'replay-check.py')
    if sum(x['seconds'] for x in history['rounds'])+report['seconds']>30:report['status']='SOURCE_REPLAY_FAILED';report['failure']='V02 Cumulative30 exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(x['seconds'] for x in history['rounds'])
    with (E/'replay-results.json').open('w') as stream:json.dump(history,stream,indent=2);stream.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':sum(c['passed'] for c in cases),'failedCases':[c['name'] for c in cases if not c['passed']],'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'preservedConflicts':conflicts,'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
