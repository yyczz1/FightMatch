#!/usr/bin/env python3
"""FIX03 launched-root registration and bounded closure; all process and file effects are fixtures."""
import ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types
START=time.monotonic(); E=pathlib.Path(__file__).parent; OLD=E.parent/'I01'; BASE=E.parent/'I01-FIX02-source'; MID=E.parent/'I01-FIX01-source'; forbidden=[]; cases=[]; traces=[]
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
    exec(compile(ast.fix_missing_locations(ast.Module(body=[copy.deepcopy(F[n]) for n in names],type_ignores=[])),str(E/'runner.py'),'exec'),env); return env
def profile(frame,event,arg):
    if event=='call' and frame.f_code.co_filename==str(E/'runner.py'): traces.append(frame.f_code.co_name)
history=json.loads((E/'replay-results.json').read_text()) if (E/'replay-results.json').exists() else {'rounds':[]}
need(len(history['rounds'])<2 and not any(x['status']=='SOURCE_REPLAY_PASS' for x in history['rounds']),'At most two rounds; stop at first pass')
report={'round':len(history['rounds'])+1,'status':'SOURCE_REPLAY_FAILED','cases':cases,'nativeRuns':0,'realPsCalls':0,'realSignals':0,'actualProjectionWrites':0,'fixtureIOOnly':True}; failure=None
try:
    oldIdentities={"runner.py":{"bytes":41638,"sha256":"e39ccb11949a432020f37592d69ffa83f1eb2173ad9088aada7fb6dcb1cd4c59"},"replay-check.py":{"bytes":13372,"sha256":"794498fff48a3482aba3cd0b62fa76ab5652b56c35366ef81927f95ad6c95f50"},"replay-results.json":{"bytes":4141,"sha256":"13067d506570c1bfa3d95be8640f0f6d8f732e096737d85949584f30a618707a"},"inputs.json":{"bytes":806564,"sha256":"d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3"},"preparation.json":{"bytes":8345,"sha256":"0c9c2d4d3ffb8d8920f02c2ce6e7a4614d69de4c3422463ea81771f8b892a6ff"}}
    need({p.name for p in OLD.iterdir()}==set(oldIdentities) and all(identity(OLD/name)==value for name,value in oldIdentities.items()),'Frozen I01 five leaves')
    baseIdentities={"runner.py":{"bytes":45994,"sha256":"6fcc05eb0191b3394338750e69a6a9eb96e5fb6a22e62840c029ecbdf8fc9570"},"replay-check.py":{"bytes":26565,"sha256":"4c105c2b3132dedb0547bc3bec929dde4a319c8c28d1c3fa5bcb909fb1f45fc7"},"replay-results.json":{"bytes":14764,"sha256":"3095f8299730bba024d744976cf6fd270a1b9df9dc5514dc5935148c23edfc8a"},"correction.patch":{"bytes":28351,"sha256":"1a0603f12c022a156146727b1e4712c586f68b508c468eec198ae50964238272"},"preparation.json":{"bytes":25280,"sha256":"769eac92e89f32772ec024e17db5bdffc9b458057dc511b4757445232f8a8881"}}
    middleIdentities={"runner.py":{"bytes":44197,"sha256":"2f5f11ec8011ef8de13774fd5813af1ef697f56634087c7c40f25f197bddf812"},"replay-check.py":{"bytes":19747,"sha256":"efd02cef050f208438bb07af07402bedd24b7ee3b72f4ac5ab20b9f9f2f3fc98"},"replay-results.json":{"bytes":14193,"sha256":"769d5eb06039e66afb503c4b53b6b0ddefe42052367274aec58f01c30817ec3c"},"correction.patch":{"bytes":28398,"sha256":"fbc8fcd83509adcad25bffd10bed55c64be28fb56208303049109f045d110d82"},"preparation.json":{"bytes":13374,"sha256":"8fab17eaa833a4f7e2713eb2b2dd32f57721457f4390959f545de695fcf532c6"}}
    need({p.name for p in MID.iterdir()}==set(middleIdentities) and all(identity(MID/name)==v for name,v in middleIdentities.items()),'Frozen FIX01 five leaves')
    need({p.name for p in BASE.iterdir()}==set(baseIdentities) and all(identity(BASE/name)==value for name,value in baseIdentities.items()),'Frozen FIX02 five leaves')
    text=(E/'runner.py').read_text(); tree=ast.parse(text); compile(tree,str(E/'runner.py'),'exec'); compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec'); F={n.name:n for n in tree.body if isinstance(n,ast.FunctionDef)}; N=json.loads((OLD/'inputs.json').read_text())
    need(sum(bool(x.strip()) for x in text.splitlines())<=480,'480 lines')
    for bad in ('observe_probe','observation_passed'): need(bad not in F,'Old observation removed')
    need(not any(isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id=='ps' for k in ('monitor','snapshot_consumers','discover','register','recover_root') for n in ast.walk(F[k])),'Same snapshot classification')
    calls=[n for n in ast.walk(F['run_stage']) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='Popen']; need(len(calls)==1,'One actual Popen site')
    need(isinstance(F['run_stage'].body[1],ast.Expr) and F['run_stage'].body[1].value.func.id=='claim_stage','Actual run_stage begins with single-use claim')
    need('-executeMethod' not in text and '-runTests' not in text and '-fm029QaBuildOnly' not in text,'Pure compile argv')
    oldtree=ast.parse((BASE/'runner.py').read_text()); oldfn={n.name:n for n in oldtree.body if isinstance(n,ast.FunctionDef)}
    changed={n for n in oldfn if ast.dump(oldfn[n])!=ast.dump(F[n])}
    need(changed=={'register','snapshot_consumers','closure','run_stage','archive_and_restore','main'} and set(F)-set(oldfn)=={'recover_root'},'Only two-P1 implementation scope')
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
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':len(cases),'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
