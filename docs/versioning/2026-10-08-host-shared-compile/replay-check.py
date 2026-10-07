#!/usr/bin/env python3
"""FIX04: unchanged 58 prior cases plus same-snapshot FIFO and closure-first failure regressions."""
import ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types
START=time.monotonic(); E=pathlib.Path(__file__).parent; OLD=E.parent/'I01'; BASE=E.parent/'I01-FIX02-source'; MID=E.parent/'I01-FIX01-source'; forbidden=[]; cases=[]; traces=set(); RUNNER_FILENAME=str(E/'runner.py'); FIX03=E.parent/'I01-FIX03-source'; RED='--red' in sys.argv
def audit(name,args):
    if name in {'subprocess.Popen','os.system','os.kill','os.killpg','socket.connect','socket.bind','os.remove','os.rename','os.mkdir','os.rmdir','ctypes.dlopen','ctypes.dlsym'}: forbidden.append(name); raise RuntimeError('Offline boundary '+name)
    if name=='open' and args[2] & (os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        if pathlib.Path(args[0]).resolve()!=E.resolve()/'replay-results.json': forbidden.append('write '+str(args[0])); raise RuntimeError('Offline write boundary')
sys.addaudithook(audit)
def need(ok,why):
    if not ok: raise AssertionError(why)
def identity(path):
    b=path.read_bytes(); return {'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}
def byteid(b): return {'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}
def rejected(fn):
    try: fn()
    except (RuntimeError,OSError): return True
    return False
def case(name,fn):
    need(time.monotonic()-START<30,'Replay30'); fn(); cases.append({'name':name,'passed':True})
def bind(names,env):
    for key,value in [('pending_details',{}),('process_snapshot',{}),('closure_closed',None)]:env.setdefault(key,value)
    exec(compile(ast.fix_missing_locations(ast.Module(body=[copy.deepcopy(F[n]) for n in names],type_ignores=[])),str(E/'runner.py'),'exec'),env); return env
def profile(frame,event,arg):
    if event=='call' and frame.f_code.co_filename==RUNNER_FILENAME: traces.add(frame.f_code.co_name)
history=json.loads((E/'replay-results.json').read_text()) if (E/'replay-results.json').exists() else {'rounds':[]}
need(len(history['rounds'])<2 and not any(x['status']=='SOURCE_REPLAY_PASS' for x in history['rounds']),'At most two rounds; stop at first pass')
report={'round':len(history['rounds'])+1,'status':'SOURCE_REPLAY_FAILED','cases':cases,'nativeRuns':0,'realPsCalls':0,'realSignals':0,'actualProjectionWrites':0,'fixtureIOOnly':True}; failure=None
try:
    additionalFrozen=json.loads("{\"I01-FIX03-source\":{\"correction.patch\":{\"bytes\":29516,\"sha256\":\"4142b8ffff648b1582d2b5ffef0fa4f10b952829ed1850bc9b53b2edca8ad334\"},\"preparation.json\":{\"bytes\":31200,\"sha256\":\"d68ff9db8668910a9e46c4fc9bd12906a07f390e9dfad2a3627e4cea33bb23e7\"},\"replay-check.py\":{\"bytes\":33486,\"sha256\":\"82ca9b61fe0f9562a7c3f9b657ea3e585239904be74a2e5c7d94cca4394e1531\"},\"replay-results.json\":{\"bytes\":21589,\"sha256\":\"c5e9f9a47af7c8e689af632411c74a60d599cb7d485cb214b3cd304238408374\"},\"runner.py\":{\"bytes\":47602,\"sha256\":\"b11eb06bc058362aaf833e5070876dc12db0e49491612f7570b87d8e26071317\"}},\"I01-run\":{\"I/editor.log\":{\"bytes\":100793,\"sha256\":\"48d397ee7646bd29f89edf150e5a78fa2ce1a94f3e3ed3fee1fe7da92162b131\"},\"I/launcher.log\":{\"bytes\":0,\"sha256\":\"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\"},\"I/result.json\":{\"bytes\":1654,\"sha256\":\"04c79ceed4bfb0307862ef3de108ea30ffa54d4662cff4c9db832bec5d0ed983\"},\"activation.json\":{\"bytes\":24833,\"sha256\":\"3a89063e90bee9140517860d5589c55b3b8b20c4a86889edf2ad4f804f3e3f5e\"},\"after.json\":{\"bytes\":477,\"sha256\":\"318c2bb752455444d31a1f13d9305ac2d7b8b0e7fd507d978793f4e477a18056\"},\"atomic/sync/Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs\":{\"bytes\":83965,\"sha256\":\"2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac\"},\"atomic/sync/Assets/Scripts/FightMatch/Host/FightMatchHostView.cs\":{\"bytes\":14379,\"sha256\":\"d7a29970b0fb8b594e7114a883b82ffef92c47181218df1bec1f420dfda72136\"},\"atomic/sync/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs\":{\"bytes\":19136,\"sha256\":\"f527b4ceecffff0718ed1214f9d26eaca5d65f6e2c82ba973d6b8774c2e66b7f\"},\"atomic/sync/Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs\":{\"bytes\":13944,\"sha256\":\"38c7cfbb999897db56e90c37b6c7cacdef95d0aaabae87b8c671e1f262ea6216\"},\"atomic/sync/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs\":{\"bytes\":34540,\"sha256\":\"659d879b2154e3e7b9347638cfeaf5f3f33b4daea8908b14711712170b5ed3ed\"},\"atomic/sync/Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs\":{\"bytes\":25884,\"sha256\":\"3baa9fc9f2f4a565a4933130e7f5c46e6c16edf4e7a3a0d13c8dac9761201e7c\"},\"atomic/sync/Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs\":{\"bytes\":18882,\"sha256\":\"b3f28fd28df9687e8a92b95a81b2bd8d65fe974e43b962436b2a75370bfb8cac\"},\"atomic/sync/Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs\":{\"bytes\":11337,\"sha256\":\"dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs\":{\"bytes\":16074,\"sha256\":\"d798f81816a6e9d1583b0ab7f54895731bb69676dbc2aae97355d4d8767f909a\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs\":{\"bytes\":28115,\"sha256\":\"16b3311b564777eb81fba4467a88afc100aaa8abaf80fe03ae0ac39989bb7337\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs\":{\"bytes\":15357,\"sha256\":\"684f9b81ff42f27e5a871a77e6f7f9278dbccf743289a54d7380b387a44fee64\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs\":{\"bytes\":27609,\"sha256\":\"3f4e56ead7feb7e3c85b9f28cf315240010cbbbb20b71fa5baf7ca270a56fcca\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs\":{\"bytes\":17926,\"sha256\":\"00a3244998a419e140d23d25e0e2d8f7e2a4c72415caffc75fec9a53289902b8\"},\"atomic/sync/Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs\":{\"bytes\":24326,\"sha256\":\"0cfa14f00f541ad6d1be26745fffaf14703e7ac3af27d038b4a410b6a6d04fbb\"},\"atomic/sync/Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs\":{\"bytes\":27431,\"sha256\":\"c4fc7de1e9786d3ae4ecb71038f74c0d978c00bf4498855cccaeefc2e4c5ce0e\"},\"atomic/sync/Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab\":{\"bytes\":1060568,\"sha256\":\"55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b\"},\"before.json\":{\"bytes\":916349,\"sha256\":\"a1e4d5ea7f753c0fb8648c42ed417289a9481dc68881009947a77a8f8b8b01a2\"},\"compile.json\":{\"bytes\":8433,\"sha256\":\"ae808d63fe2213c883c0fe57209dec50910bef50c0ccd52b7cb788315d1a89be\"},\"inputs.json\":{\"bytes\":806564,\"sha256\":\"d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3\"},\"park/source/Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef\":{\"bytes\":377,\"sha256\":\"09347e70d10f8cb3e26751c004ba005cadc32d1f3b6ecadf78bb6c9cf1fb2e45\"},\"park/source/Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef.meta\":{\"bytes\":166,\"sha256\":\"71e1d59633f8843474b4b49e08037d59ab5fce7cb98b6c87fbae1a3befbf0b14\"},\"park/source/Assets/Scripts/FightMatch/AssetAccess/FightMatchAssetContracts.cs\":{\"bytes\":11990,\"sha256\":\"4ababded06a870408ebb74e157eeb4ea3233509fb7e6b48d6b904f1d6ac04330\"},\"park/source/Assets/Scripts/FightMatch/AssetAccess/FightMatchAssetContracts.cs.meta\":{\"bytes\":243,\"sha256\":\"cc18e3364c4f4b7e2eeb066924d754603135c61f23c2235fea36eeee3b99d20a\"},\"park/source/Assets/Scripts/FightMatch/AssetAccess.meta\":{\"bytes\":172,\"sha256\":\"ddfbcd983fe098bfcd8e030ead0151a21546e79ed7870588e1bda12717477a32\"},\"park/source/Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef\":{\"bytes\":515,\"sha256\":\"1d605c9ea6e3727c7fe4f595078a12469def24ec7055489f1a467951dde244a0\"},\"park/source/Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef.meta\":{\"bytes\":166,\"sha256\":\"72aca75813de7109895ab4a65f46a5dd60ba1f235850ea566d53df2ad03b5ee3\"},\"park/source/Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetContractsTests.cs\":{\"bytes\":16708,\"sha256\":\"6c65da27c7847d50e5a3c0beb1da30d58db025646be21b2eec493c816c3ba5fc\"},\"park/source/Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetContractsTests.cs.meta\":{\"bytes\":243,\"sha256\":\"3c2a4529ca7e380f23312c718b89725d6f9228034c2e1e438b7cdb4a88891d4e\"},\"park/source/Assets/Tests/EditMode/FightMatchAsset.meta\":{\"bytes\":172,\"sha256\":\"449fc8beb147b177d6c8d018a543b446392c8aae9b2c447982f30255e38bde28\"},\"park/source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs\":{\"bytes\":15710,\"sha256\":\"ed176ec6e87742359cfcd7767236619496180b25ad9783563582235ca1f8014d\"},\"park/source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs.meta\":{\"bytes\":243,\"sha256\":\"b6ecf5844647902cd3499b55c8948e0f2925bbdef9b379bfe13b2aa23e888535\"},\"preparation.json\":{\"bytes\":31200,\"sha256\":\"d68ff9db8668910a9e46c4fc9bd12906a07f390e9dfad2a3627e4cea33bb23e7\"},\"process-after.json\":{\"bytes\":135198,\"sha256\":\"9d6b467176d9f19494f04e0633c4c2e1a7a0d6d159c4b821ef992094c640aa36\"},\"process-events.jsonl\":{\"bytes\":145484,\"sha256\":\"427f55a8fbf200762134b19672c4c1a9af4b17e3c293f2b573593de67285e547\"},\"receipt.json\":{\"bytes\":18698,\"sha256\":\"58856c36ec5cdc1d638323d99f3563a2ddba162ad8b5cb11b905bea9278cdece\"},\"replay-check.py\":{\"bytes\":33486,\"sha256\":\"82ca9b61fe0f9562a7c3f9b657ea3e585239904be74a2e5c7d94cca4394e1531\"},\"replay-results.json\":{\"bytes\":21589,\"sha256\":\"c5e9f9a47af7c8e689af632411c74a60d599cb7d485cb214b3cd304238408374\"},\"restore/source/Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs\":{\"bytes\":83965,\"sha256\":\"2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac\"},\"restore/source/Assets/Scripts/FightMatch/Host/FightMatchHostView.cs\":{\"bytes\":14379,\"sha256\":\"d7a29970b0fb8b594e7114a883b82ffef92c47181218df1bec1f420dfda72136\"},\"restore/source/Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs\":{\"bytes\":19136,\"sha256\":\"f527b4ceecffff0718ed1214f9d26eaca5d65f6e2c82ba973d6b8774c2e66b7f\"},\"restore/source/Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs\":{\"bytes\":13944,\"sha256\":\"38c7cfbb999897db56e90c37b6c7cacdef95d0aaabae87b8c671e1f262ea6216\"},\"restore/source/Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs\":{\"bytes\":34540,\"sha256\":\"659d879b2154e3e7b9347638cfeaf5f3f33b4daea8908b14711712170b5ed3ed\"},\"restore/source/Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs\":{\"bytes\":25884,\"sha256\":\"3baa9fc9f2f4a565a4933130e7f5c46e6c16edf4e7a3a0d13c8dac9761201e7c\"},\"restore/source/Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs\":{\"bytes\":18882,\"sha256\":\"b3f28fd28df9687e8a92b95a81b2bd8d65fe974e43b962436b2a75370bfb8cac\"},\"restore/source/Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs\":{\"bytes\":11337,\"sha256\":\"dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9\"},\"restore/source/Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs\":{\"bytes\":16074,\"sha256\":\"d798f81816a6e9d1583b0ab7f54895731bb69676dbc2aae97355d4d8767f909a\"},\"restore/source/Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs\":{\"bytes\":28115,\"sha256\":\"16b3311b564777eb81fba4467a88afc100aaa8abaf80fe03ae0ac39989bb7337\"},\"restore/source/Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs\":{\"bytes\":15357,\"sha256\":\"684f9b81ff42f27e5a871a77e6f7f9278dbccf743289a54d7380b387a44fee64\"},\"restore/source/Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs\":{\"bytes\":27609,\"sha256\":\"3f4e56ead7feb7e3c85b9f28cf315240010cbbbb20b71fa5baf7ca270a56fcca\"},\"restore/source/Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs\":{\"bytes\":17926,\"sha256\":\"00a3244998a419e140d23d25e0e2d8f7e2a4c72415caffc75fec9a53289902b8\"},\"restore/source/Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs\":{\"bytes\":24326,\"sha256\":\"0cfa14f00f541ad6d1be26745fffaf14703e7ac3af27d038b4a410b6a6d04fbb\"},\"restore/source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs\":{\"bytes\":27431,\"sha256\":\"c4fc7de1e9786d3ae4ecb71038f74c0d978c00bf4498855cccaeefc2e4c5ce0e\"},\"restore/source/Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab\":{\"bytes\":1060568,\"sha256\":\"55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b\"},\"runner.py\":{\"bytes\":47602,\"sha256\":\"b11eb06bc058362aaf833e5070876dc12db0e49491612f7570b87d8e26071317\"}}}")
    def frozen_unchanged():
        for name,leaves in additionalFrozen.items():
            base=E.parent/name
            need({str(p.relative_to(base)) for p in base.rglob('*') if p.is_file()}==set(leaves) and all(identity(base/p)==v for p,v in leaves.items()),'Sealed tree unchanged '+name)
    frozen_unchanged()
    oldIdentities={"runner.py":{"bytes":41638,"sha256":"e39ccb11949a432020f37592d69ffa83f1eb2173ad9088aada7fb6dcb1cd4c59"},"replay-check.py":{"bytes":13372,"sha256":"794498fff48a3482aba3cd0b62fa76ab5652b56c35366ef81927f95ad6c95f50"},"replay-results.json":{"bytes":4141,"sha256":"13067d506570c1bfa3d95be8640f0f6d8f732e096737d85949584f30a618707a"},"inputs.json":{"bytes":806564,"sha256":"d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3"},"preparation.json":{"bytes":8345,"sha256":"0c9c2d4d3ffb8d8920f02c2ce6e7a4614d69de4c3422463ea81771f8b892a6ff"}}
    need({p.name for p in OLD.iterdir()}==set(oldIdentities) and all(identity(OLD/name)==value for name,value in oldIdentities.items()),'Frozen I01 five leaves')
    baseIdentities={"runner.py":{"bytes":45994,"sha256":"6fcc05eb0191b3394338750e69a6a9eb96e5fb6a22e62840c029ecbdf8fc9570"},"replay-check.py":{"bytes":26565,"sha256":"4c105c2b3132dedb0547bc3bec929dde4a319c8c28d1c3fa5bcb909fb1f45fc7"},"replay-results.json":{"bytes":14764,"sha256":"3095f8299730bba024d744976cf6fd270a1b9df9dc5514dc5935148c23edfc8a"},"correction.patch":{"bytes":28351,"sha256":"1a0603f12c022a156146727b1e4712c586f68b508c468eec198ae50964238272"},"preparation.json":{"bytes":25280,"sha256":"769eac92e89f32772ec024e17db5bdffc9b458057dc511b4757445232f8a8881"}}
    middleIdentities={"runner.py":{"bytes":44197,"sha256":"2f5f11ec8011ef8de13774fd5813af1ef697f56634087c7c40f25f197bddf812"},"replay-check.py":{"bytes":19747,"sha256":"efd02cef050f208438bb07af07402bedd24b7ee3b72f4ac5ab20b9f9f2f3fc98"},"replay-results.json":{"bytes":14193,"sha256":"769d5eb06039e66afb503c4b53b6b0ddefe42052367274aec58f01c30817ec3c"},"correction.patch":{"bytes":28398,"sha256":"fbc8fcd83509adcad25bffd10bed55c64be28fb56208303049109f045d110d82"},"preparation.json":{"bytes":13374,"sha256":"8fab17eaa833a4f7e2713eb2b2dd32f57721457f4390959f545de695fcf532c6"}}
    need({p.name for p in MID.iterdir()}==set(middleIdentities) and all(identity(MID/name)==v for name,v in middleIdentities.items()),'Frozen FIX01 five leaves')
    need({p.name for p in BASE.iterdir()}==set(baseIdentities) and all(identity(BASE/name)==value for name,value in baseIdentities.items()),'Frozen FIX02 five leaves')
    text=(E/'runner.py').read_text(); tree=ast.parse(text); compile(tree,str(E/'runner.py'),'exec'); compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec'); F={n.name:n for n in tree.body if isinstance(n,ast.FunctionDef)}; N=json.loads((OLD/'inputs.json').read_text())
    need(sum(bool(x.strip()) for x in text.splitlines())<=580,'580 lines')
    for bad in ('observe_probe','observation_passed'): need(bad not in F,'Old observation removed')
    need(not any(isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id=='ps' for k in ('monitor','snapshot_consumers','discover','register','recover_root') for n in ast.walk(F[k])),'Same snapshot classification')
    calls=[n for n in ast.walk(F['run_stage']) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='Popen']; need(len(calls)==1,'One actual Popen site')
    need(isinstance(F['run_stage'].body[1],ast.Expr) and F['run_stage'].body[1].value.func.id=='claim_stage','Actual run_stage begins with single-use claim')
    need('-executeMethod' not in text and '-runTests' not in text and '-fm029QaBuildOnly' not in text,'Pure compile argv')
    oldtree=ast.parse((FIX03/'runner.py').read_text()); oldfn={n.name:n for n in oldtree.body if isinstance(n,ast.FunctionDef)}
    changed={n for n in oldfn if ast.dump(oldfn[n])!=ast.dump(F[n])}
    expected={'discover','consumer_guard','snapshot_consumers','monitor','closure','tree_entries','archive_and_restore','main'}
    need(changed==(set() if RED else expected) and set(F)==set(oldfn),'Only three approved corrections; no new framework')
    if RED:need(identity(E/'runner.py')==identity(FIX03/'runner.py'),'Red executes exact FIX03 runner')
    report['phase']='RED_BASELINE' if RED else 'GREEN_CORRECTION'
    for n in ('synchronize','archive_and_restore'):need(not any(isinstance(x,ast.Call) and isinstance(x.func,ast.Attribute) and x.func.attr in ('open','write_bytes') for x in ast.walk(F[n])),'No direct P write in '+n)
    report['changedFunctions']=sorted(changed);report['newFunctions']=sorted(set(F)-set(oldfn));report['allOtherFunctionsAstEqual']=True
    report['static']={'syntax':True,'runnerNonblankLines':sum(bool(x.strip()) for x in text.splitlines()),'singlePopenSite':True,'sameSnapshot':True,'pureCompile':True}
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
        env['A']['stopping']={'naturalGraceSeconds':0,'termGraceSeconds':0}
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
                    if not stream.closed:
                        value=stream.getvalue();path.fs.add(path.p,value if binary else value.encode())
                    super().close()
            return Buffer()
    def restore_case(drift=False,interrupt=False,fault=None):
        fs=FS();n=copy.deepcopy(N);base={};target={}
        for p in n['overwritten']+n['parked']:base[p]=('before '+p).encode()
        stable='Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs';base[stable]=b'unchanged'
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
        if not sync_failure:need(sources(env['P'])==n['files'],'All 16+4 synchronized and 12 parked')
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
        if not drift and not interrupt and not fault:need(len(restored['archived'])==20 and len(restored['returnedParked'])==12,'All 32 paths covered')
        if fault:report.setdefault('faultEvidence',[]).append({'fault':fault,'partialFiles':list(fs.partial_written),'atomicTemporaryRetained':atomic in fs.files,'restoredComplete':restored['complete'],'restoredEarlierItem':fs.files['/P/'+n['overwritten'][0]]==base[n['overwritten'][0]],'errors':restored['errors'],'raceAtPrimitive':racing,'swapCallsForAffectedSlot':fs.swap_counts.get(atomic,0),'firstConcurrentRetained':('/E/atomic/displaced/'+phase+'/'+selected) in fs.files,'secondConcurrentRetained':('/E/atomic/conflict/'+phase+'/'+selected) in fs.files,'nativeFlagsUsed':sorted(set(fs.native_calls))})
    case('32 paths synchronize and restore through actual functions',lambda:restore_case())
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
        proc=Proc()
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
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:100.,time=lambda:1791402560.,sleep=lambda n:None),'utc':lambda:'fixture-root-time','os':types.SimpleNamespace(environ={},getpid=lambda:originalRoot['ppid'],kill=term,path=types.SimpleNamespace(lexists=lambda p:p.exists())),'signal':types.SimpleNamespace(SIGTERM=15),'subprocess':types.SimpleNamespace(Popen=popen,STDOUT=-2),'P':p,'E':e,'A':{'stages':[{'id':'I','argv':argv}],'editor':{'path':root['exe']},'environmentOverrides':{},'stopping':{'naturalGraceSeconds':0,'termGraceSeconds':0}},'owned':{},'active':None,'launched_root':None,'root_launch_epoch':None,'launch_attempts':0,'stages':[],'synced':True,'details':fixture_details,'ps':fixture_ps,'event':lambda kind,**kw:emitted.append({'kind':kind,**kw}),'write':lambda name,value:written.update({name:copy.deepcopy(value)}),'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'preflight':lambda:None,'resources':lambda:None,'projection_guard':lambda:None,'protection':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.0}
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
        report.setdefault('launchedRootRecovery',[]).append({'mode':mode,'simulatedPopenCount':len(popens),'rootDetailsSnapshotIndexes':rootDetails,'suppliedSnapshots':len(snapshots),'rootPublished':rootid in env['owned'],'simulatedTerms':signals,'popenStillLive':proc.poll() is None,'restoreRootGateBlocked':pending,'originalFailurePreserved':True,'realPopenPsSignals':0,'naturalAndTermWindows':'Synthetic fast-forward (0/0); runner native 60/30 bounds unchanged'})
    for mode in ('recover','natural-exit','natural-during-recovery','pid-reuse-live','pid-reuse-exited','foreign-parent','foreign-exe','wrong-argv','wrong-cwd','persistent-details'):
        case('launched root '+mode+' through actual run_stage/closure',lambda mode=mode:launched_root_case(mode))
    need(len(cases)==58 and all(c['passed'] for c in cases),'All original 58 assertions preserved')
    report['prior58Passed']=True
    def regression(name,fn):
        start=time.monotonic()
        try:fn();cases.append({'name':name,'passed':True,'newFIX04':True})
        except Exception as error:cases.append({'name':name,'passed':False,'newFIX04':True,'error':str(error),'traceback':traceback.format_exc()})
        need(time.monotonic()-START<30,'Replay30')
    def fifo_case(mode):
        import re,stat
        license_path=str(pathlib.Path(N['editor']['path']).parent.parent/'Frameworks/UnityLicensingClient.app/Contents/MacOS/Unity.Licensing.Client')
        rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)};initial={rootid:copy.deepcopy(root)}
        exe='/fixed/dotnet' if mode=='dotnet' else license_path
        rows[childid]['exe']=exe;detail={rootid:{k:root[k] for k in ('argv','cwd','cwdProbeExit')},childid:{'argv':[exe,'--namedPipe','fixture'],'cwd':root['cwd'],'cwdProbeExit':0}}
        if mode=='foreign-same-name':rows[childid]['ppid']=999999
        if mode=='foreign-exe':rows[childid]['exe']='/foreign/Unity.Licensing.Client'
        if mode in ('reuse','wrong-ancestry'):
            initial[childid]={**copy.deepcopy(child),**detail[childid],**rows[childid]}
            if mode=='reuse':rows[childid]['start']='reused'
            else:initial[childid]['rootPid']=rootid+999
        reads=[];population=[];entries={};sequence=[]
        class FixturePath(pathlib.PurePosixPath):
            def lstat(self):return types.SimpleNamespace(st_mode=(stat.S_IFSOCK if mode=='non-fifo' else stat.S_IFIFO)|0o600,st_uid=202 if mode=='wrong-uid' else 101,st_gid=20,st_dev=9,st_ino=10,st_size=0)
        temporary=FixturePath('/synthetic-tmp');scan=FixturePath('/wrong-root') if mode=='wrong-root' else temporary
        filename='clr-debug-pipe-'+str(childid)+'-1791404087-in'
        if mode=='wrong-name':filename='foreign-pipe'
        def no_read(p):reads.append(str(p));raise AssertionError('FIFO content read')
        def no_ps():population.append(True);raise AssertionError('Second population query')
        env={'json':json,'re':re,'stat':stat,'pathlib':types.SimpleNamespace(Path=FixturePath),'time':types.SimpleNamespace(monotonic=lambda:100.),'utc':lambda:'fixture-fifo','os':types.SimpleNamespace(walk=lambda root,followlinks:[(str(scan),[],[filename])],getuid=lambda:101),'TMP':temporary,'owned':initial,'active':None,'A':{'editor':N['editor'],'stages':[{'id':root['stage'],'argv':root['argv']}]},'P':FixturePath(root['cwd']),'details':lambda pid:copy.deepcopy(detail[pid]),'ps':no_ps,'event':lambda kind,**kw:sequence.append(kind),'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'projection_guard':lambda:None,'no_links':lambda p:None,'ident':no_read,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.}
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
        env['A']['stopping']={'naturalGraceSeconds':0,'termGraceSeconds':0}
        calls=[];actual_monitor=env['monitor']
        def skip_until_final(phase,*args,**kw):
            calls.append(phase)
            if phase!='closure-final':return None
            return actual_monitor(phase,*args,**kw)
        env['monitor']=skip_until_final;bind(['closure'],env);env['closure'](root['stage'],rootid,'synthetic skipped samples')
        need(calls==['natural-closure','term-confirmation','closure-final'] and env['closure_closed'] is True,'Only final successful forced sample establishes clear')
    regression('skipped monitor sample cannot establish closure',skipped_is_not_clear)
    frozen_unchanged();report['fixed03AndRun59Unchanged']=True
    need(all(c['passed'] for c in cases),'FIX04 regression failures: '+', '.join(c['name'] for c in cases if not c['passed']))
    need(all(identity(BASE/name)==value for name,value in baseIdentities.items()),'FIX02 unchanged after replay');report['baseFix02FiveFilesUnchanged']=True
    need(all(identity(MID/name)==value for name,value in middleIdentities.items()),'FIX01 unchanged after replay');report['baseFix01FiveFilesUnchanged']=True
    need(all(identity(OLD/name)==value for name,value in oldIdentities.items()),'Old I01 unchanged after replay');report['oldFiveFilesUnchanged']=True
    need(not forbidden,'No forbidden operations');report['status']='SOURCE_REPLAY_PASS'
except BaseException as error:failure=str(error);report['failure']=failure;report['traceback']=traceback.format_exc()
finally:
    sys.setprofile(None);report['seconds']=time.monotonic()-START;report['actualFunctionsCalled']=sorted(set(traces));report['forbiddenAttempts']=forbidden;report['runner']=identity(E/'runner.py');report['checker']=identity(E/'replay-check.py')
    if sum(x['seconds'] for x in history['rounds'])+report['seconds']>30:report['status']='SOURCE_REPLAY_FAILED';report['failure']='Cumulative30 exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(x['seconds'] for x in history['rounds'])
    with (E/'replay-results.json').open('w') as f:json.dump(history,f,indent=2);f.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':sum(c['passed'] for c in cases),'failedCases':[c['name'] for c in cases if not c['passed']],'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
