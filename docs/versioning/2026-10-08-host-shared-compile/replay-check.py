#!/usr/bin/env python3
"""FIX01 offline fault injection into actual runner functions; no native process/filesystem writes."""
import ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types
START=time.monotonic(); E=pathlib.Path(__file__).parent; OLD=E.parent/'I01'; forbidden=[]; cases=[]; traces=[]
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
    text=(E/'runner.py').read_text(); tree=ast.parse(text); compile(tree,str(E/'runner.py'),'exec'); compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec'); F={n.name:n for n in tree.body if isinstance(n,ast.FunctionDef)}; N=json.loads((OLD/'inputs.json').read_text())
    need(sum(bool(x.strip()) for x in text.splitlines())<=480,'480 lines')
    for bad in ('observe_probe','observation_passed'): need(bad not in F,'Old observation removed')
    need(not any(isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id=='ps' for k in ('monitor','snapshot_consumers','discover','register') for n in ast.walk(F[k])),'Same snapshot classification')
    calls=[n for n in ast.walk(F['run_stage']) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='Popen']; need(len(calls)==1,'One actual Popen site')
    need(isinstance(F['run_stage'].body[1],ast.Expr) and F['run_stage'].body[1].value.func.id=='claim_stage','Actual run_stage begins with single-use claim')
    need('-executeMethod' not in text and '-runTests' not in text and '-fm029QaBuildOnly' not in text,'Pure compile argv')
    oldtree=ast.parse((OLD/'runner.py').read_text()); oldfn={n.name:n for n in oldtree.body if isinstance(n,ast.FunctionDef)}
    changed={n for n in oldfn if ast.dump(oldfn[n])!=ast.dump(F[n])}
    need(changed=={'discover','transfer','resources','synchronize','archive_and_restore'} and set(F)-set(oldfn)=={'rename_exclusive','atomic_write'},'Only two-P1 implementation scope')
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
        def fixture_details(pid):
            if variant=='details-error' and pid==childid:raise RuntimeError('synthetic details unavailable after supplied snapshot')
            return copy.deepcopy(detail[pid])
        def forbidden_ps():ps_calls.append(True);raise AssertionError('No second process snapshot')
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:100.),'utc':lambda:'fixture','owned':initial,'A':{'stages':[{'id':root['stage'],'argv':root['argv']}]},'P':pathlib.Path(root['cwd']),'details':fixture_details,'ps':forbidden_ps,'event':lambda *a,**kw:None,'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'resources':lambda:None,'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.0}
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','snapshot_consumers','monitor'],env)
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
            need(childid in env['owned'] and env['owned'][childid]['argv'] is None,'No invented complete identity or exit')
        if not variant:need(childid in env['owned'] and env['owned'][childid]['argv']==child['argv'] and env['owned'][childid]['cwd']==child['cwd'],'Child full identity')
    for phase in ('running','natural-closure','term-confirmation','closure-final','final'):
        case(phase+' legitimate new child',lambda phase=phase:monitor_case(phase))
        case(phase+' same-name foreign child rejected',lambda phase=phase:monitor_case(phase,'foreign'))
    case('PID reuse rejected',lambda:monitor_case('running','reuse'))
    for entry in ('discover','snapshot_consumers','monitor'):
        case('details failure '+entry+' rejects without second ps',lambda entry=entry:monitor_case('running','details-error',entry))
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
        def __init__(self):self.files={};self.dirs={'/'};self.fail_write=None;self.fail_partial=None;self.fail_commit=None;self.collide_commit=None;self.partial_written=[];self.committed=[]
        def add(self,p,b):
            self.files[p]=b;q=pathlib.PurePosixPath(p).parent
            while str(q)!='/':self.dirs.add(str(q));q=q.parent
        def path(self,p):return Path(self,str(p))
        def commit(self,source,target,exclusive=False):
            if self.collide_commit==source.p:self.add(target.p,b'concurrent third party')
            if self.fail_commit==source.p:raise OSError('synthetic atomic commit refused')
            if exclusive and target.exists():raise FileExistsError('synthetic exclusive destination exists')
            payload=self.files.pop(source.p);self.add(target.p,payload);self.committed.append((source.p,target.p))
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
        env={'N':n,'P':fs.path('/P'),'R':fs.path('/R'),'E':fs.path('/E'),'B':{},'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda p:p.exists()),replace=lambda source,target:fs.commit(source,target),fsync=lambda fd:None),'time':types.SimpleNamespace(monotonic=lambda:100.),'hashlib':hashlib,'json':json,'synced':False,'restored':False,'synchronized_paths':[],'parked_paths':[],'owned':{},'stages':[],'ps':lambda:{},'consumer_guard':lambda rows:None,'projection_guard':lambda:None,'protection':lambda:None,'resources':lambda:None,'source_tree':sources,'no_links':lambda p:None,'ident':lambda p:byteid(p.read_bytes()),'rename_exclusive':lambda source,target:fs.commit(source,target,True),'event':lambda *a,**kw:None,'write':lambda name,value:outputs.update({name:value})}
        bind(['check','basic','same','validate_inputs','transfer','atomic_write','synchronize','archive_and_restore'],env)
        selected=n['newPaths'][1] if fault and 'new' in fault else n['overwritten'][2]
        phase='restore' if fault and fault.startswith('restore') else 'sync';atomic='/E/atomic/'+phase+'/'+selected
        if interrupt:fs.fail_write='/E/atomic/sync/'+selected
        if fault and 'partial' in fault:fs.fail_partial=atomic
        if fault and 'commit' in fault:fs.fail_commit=atomic
        if fault=='new-exclusive-collision':fs.collide_commit=atomic
        sync_failure=interrupt or bool(fault and phase=='sync')
        failed=rejected(env['synchronize']);need(failed==sync_failure,'Synchronization expected failure');fs.fail_write=None
        if not sync_failure:need(sources(env['P'])==n['files'],'All 16+4 synchronized and 12 parked')
        if fault and phase=='sync':
            if 'partial' in fault:need(fs.partial_written==[atomic] and 0<len(fs.files[atomic])<len(target[selected]),'Partial write actually persisted only in E')
            if 'commit' in fault:need(fs.files[atomic]==target[selected],'Full staged postimage retained after failed commit')
            if fault!='new-exclusive-collision':need(fs.files.get('/P/'+selected)==base.get(selected),'Failed sync did not create/truncate P')
            need(any(dst=='/P/'+n['overwritten'][0] for _,dst in fs.committed),'Earlier item actually committed')
        if drift:fs.files['/P/'+n['overwritten'][0]]=b'concurrent third party'
        restored=env['archive_and_restore']()
        expected={p:byteid(b) for p,b in original.items()}
        restore_failure=bool(fault and phase=='restore')
        collision=fault=='new-exclusive-collision'
        if restore_failure:
            expected[selected]=byteid(target[selected]);need(fs.files['/P/'+selected]==target[selected],'Failed restore leaves full postimage, never partial P')
            if 'partial' in fault:need(fs.partial_written==[atomic] and 0<len(fs.files[atomic])<len(base[selected]),'Partial restore retained in E')
            if 'commit' in fault:need(fs.files[atomic]==base[selected],'Complete restore temporary retained')
        if drift:expected[n['overwritten'][0]]=byteid(b'concurrent third party')
        if collision:expected[selected]=byteid(b'concurrent third party')
        need(sources(env['P'])==expected,'Actual ledger restored with unknown values preserved')
        need(restored['complete']==(not drift and not restore_failure and not collision),'Restore verdict')
        if not drift and not interrupt and not fault:need(len(restored['archived'])==20 and len(restored['returnedParked'])==12,'All 32 paths covered')
        if fault:report.setdefault('faultEvidence',[]).append({'fault':fault,'partialFiles':list(fs.partial_written),'atomicTemporaryRetained':atomic in fs.files,'restoredComplete':restored['complete'],'restoredEarlierItem':fs.files['/P/'+n['overwritten'][0]]==base[n['overwritten'][0]],'errors':restored['errors']})
    case('32 paths synchronize and restore through actual functions',lambda:restore_case())
    case('mid-sync failure restores actual ledger',lambda:restore_case(interrupt=True))
    case('third-party target preserved with failed restoration verdict',lambda:restore_case(drift=True))
    for fault in ('sync-overwrite-partial','sync-new-partial','sync-overwrite-commit','sync-new-commit','restore-partial','restore-commit','new-exclusive-collision'):
        case(fault+' preserves P and previous commits',lambda fault=fault:restore_case(fault=fault))
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
