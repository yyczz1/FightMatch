#!/usr/bin/env python3
"""Offline calls into actual I01 functions. Process and filesystem boundaries use fixtures."""
import ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types
START=time.monotonic(); E=pathlib.Path(__file__).parent; forbidden=[]; cases=[]; traces=[]
def audit(name,args):
    if name in {'subprocess.Popen','os.system','os.kill','os.killpg','socket.connect','socket.bind','os.remove','os.rename','os.mkdir','os.rmdir'}: forbidden.append(name); raise RuntimeError('Offline boundary '+name)
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
    except RuntimeError: return True
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
    text=(E/'runner.py').read_text(); tree=ast.parse(text); compile(tree,str(E/'runner.py'),'exec'); compile(ast.parse(pathlib.Path(__file__).read_text()),__file__,'exec'); F={n.name:n for n in tree.body if isinstance(n,ast.FunctionDef)}; N=json.loads((E/'inputs.json').read_text())
    need(sum(bool(x.strip()) for x in text.splitlines())<=480,'480 lines')
    for bad in ('observe_probe','observation_passed'): need(bad not in F,'Old observation removed')
    need(not any(isinstance(n,ast.Call) and isinstance(n.func,ast.Name) and n.func.id=='ps' for k in ('monitor','snapshot_consumers') for n in ast.walk(F[k])),'Same snapshot classification')
    calls=[n for n in ast.walk(F['run_stage']) if isinstance(n,ast.Call) and isinstance(n.func,ast.Attribute) and n.func.attr=='Popen']; need(len(calls)==1,'One actual Popen site')
    need(isinstance(F['run_stage'].body[1],ast.Expr) and F['run_stage'].body[1].value.func.id=='claim_stage','Actual run_stage begins with single-use claim')
    need('-executeMethod' not in text and '-runTests' not in text and '-fm029QaBuildOnly' not in text,'Pure compile argv')
    report['static']={'syntax':True,'runnerNonblankLines':sum(bool(x.strip()) for x in text.splitlines()),'singlePopenSite':True,'sameSnapshot':True,'pureCompile':True}
    sys.setprofile(profile)
    root=N['processFixture']['root']; child=N['processFixture']['child']; rootid=root['pid']; childid=child['pid']; keys=('ppid','start','stat','exe')
    def monitor_case(phase,variant=None):
        rows={x['pid']:{k:x[k] for k in keys} for x in (root,child)}; initial={rootid:copy.deepcopy(root)}
        if variant=='foreign':rows[childid]['ppid']=999999
        if variant=='reuse':initial[childid]=copy.deepcopy(child);rows[childid]['start']='synthetic reused PID'
        detail={x['pid']:{k:x[k] for k in ('argv','cwd','cwdProbeExit')} for x in (root,child)}; snapshots=[]
        env={'json':json,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:100.),'utc':lambda:'fixture','owned':initial,'A':{'stages':[{'id':root['stage'],'argv':root['argv']}]},'P':pathlib.Path(root['cwd']),'details':lambda pid:copy.deepcopy(detail[pid]),'ps':lambda:(_ for _ in ()).throw(AssertionError('No nested ps')),'event':lambda *a,**kw:None,'adb_exception':lambda rows:-1,'sdk_adb_exception':lambda rows:set(),'resources':lambda:None,'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.0}
        bind(['check','alive','register','discover','consumer_guard','recorded_chain','snapshot_consumers','monitor'],env)
        original=env['snapshot_consumers']
        def capture(given,stage,rpid):snapshots.append(given is rows);return original(given,stage,rpid)
        env['snapshot_consumers']=capture
        env['monitor'](phase,rows,root['stage'],rootid,force=True)
        need(bool(env['monitor_errors'])==bool(variant),'Expected ownership verdict '+str(variant));need(snapshots==[True],'Actual supplied snapshot object')
        if not variant:need(childid in env['owned'] and env['owned'][childid]['argv']==child['argv'] and env['owned'][childid]['cwd']==child['cwd'],'Child full identity')
    for phase in ('running','natural-closure','term-confirmation','closure-final','final'):
        case(phase+' legitimate new child',lambda phase=phase:monitor_case(phase))
        case(phase+' same-name foreign child rejected',lambda phase=phase:monitor_case(phase,'foreign'))
    case('PID reuse rejected',lambda:monitor_case('running','reuse'))
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
        def __init__(self):self.files={};self.dirs={'/'};self.fail_write=None
        def add(self,p,b):
            self.files[p]=b;q=pathlib.PurePosixPath(p).parent
            while str(q)!='/':self.dirs.add(str(q));q=q.parent
        def path(self,p):return Path(self,str(p))
    class Path:
        def __init__(self,fs,p):self.fs=fs;self.p=str(pathlib.PurePosixPath(p))
        def __str__(self):return self.p
        def __fspath__(self):return self.p
        def __truediv__(self,v):return Path(self.fs,self.p+'/'+str(v))
        @property
        def parent(self):return Path(self.fs,str(pathlib.PurePosixPath(self.p).parent))
        def exists(self):return self.p in self.fs.files or self.p in self.fs.dirs
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
            if self.fs.fail_write==self.p and 'w' in mode:raise RuntimeError('synthetic interrupted write before truncation')
            need(not ('x' in mode and self.exists()),'exclusive target')
            path=self;binary='b' in mode
            class Buffer(io.BytesIO if binary else io.StringIO):
                def close(stream):
                    if not stream.closed:
                        value=stream.getvalue();path.fs.add(path.p,value if binary else value.encode())
                    super().close()
            return Buffer()
    def restore_case(drift=False,interrupt=False):
        fs=FS();n=copy.deepcopy(N);base={};target={}
        for p in n['overwritten']+n['parked']:base[p]=('before '+p).encode()
        stable='Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs';base[stable]=b'unchanged'
        for p in n['overwritten']+n['newPaths']:target[p]=('after '+p).encode()
        target[stable]=b'unchanged';original=copy.deepcopy(base)
        for p,b in base.items():fs.add('/P/'+p,b)
        for p,b in target.items():fs.add('/R/'+p,b)
        n['restoreBaseline']={p:byteid(b) for p,b in base.items()};n['files']={p:byteid(b) for p,b in target.items()};n['shared']=n['files'];outputs={}
        def sources(root):return {p[len(str(root))+1:]:byteid(b) for p,b in fs.files.items() if p.startswith(str(root)+'/')}
        env={'N':n,'P':fs.path('/P'),'R':fs.path('/R'),'E':fs.path('/E'),'B':{},'os':types.SimpleNamespace(path=types.SimpleNamespace(lexists=lambda p:p.exists())),'time':types.SimpleNamespace(monotonic=lambda:100.),'hashlib':hashlib,'json':json,'synced':False,'restored':False,'synchronized_paths':[],'parked_paths':[],'owned':{},'stages':[],'ps':lambda:{},'consumer_guard':lambda rows:None,'projection_guard':lambda:None,'protection':lambda:None,'resources':lambda:None,'source_tree':sources,'no_links':lambda p:None,'ident':lambda p:byteid(p.read_bytes()),'event':lambda *a,**kw:None,'write':lambda name,value:outputs.update({name:value})}
        bind(['check','basic','same','validate_inputs','transfer','synchronize','archive_and_restore'],env)
        if interrupt:fs.fail_write='/P/'+n['overwritten'][2]
        failed=rejected(env['synchronize']);need(failed==interrupt,'Synchronization expected failure');fs.fail_write=None
        if not interrupt:need(sources(env['P'])==n['files'],'All 16+4 synchronized and 12 parked')
        if drift:fs.files['/P/'+n['overwritten'][0]]=b'concurrent third party'
        restored=env['archive_and_restore']()
        expected={p:byteid(b) for p,b in original.items()}
        if drift:expected[n['overwritten'][0]]=byteid(b'concurrent third party')
        need(sources(env['P'])==expected,'All corresponding original bytes restored; foreign value preserved')
        need(restored['complete']==(not drift),'Restore verdict')
        if not drift and not interrupt:need(len(restored['archived'])==20 and len(restored['returnedParked'])==12,'All 32 paths covered')
    case('32 paths synchronize and restore through actual functions',lambda:restore_case())
    case('mid-sync failure restores actual ledger',lambda:restore_case(interrupt=True))
    case('third-party target preserved with failed restoration verdict',lambda:restore_case(drift=True))
    need(not forbidden,'No forbidden operations');report['status']='SOURCE_REPLAY_PASS'
except BaseException as error:failure=str(error);report['failure']=failure;report['traceback']=traceback.format_exc()
finally:
    sys.setprofile(None);report['seconds']=time.monotonic()-START;report['actualFunctionsCalled']=sorted(set(traces));report['forbiddenAttempts']=forbidden;report['runner']=identity(E/'runner.py');report['checker']=identity(E/'replay-check.py')
    if sum(x['seconds'] for x in history['rounds'])+report['seconds']>30:report['status']='SOURCE_REPLAY_FAILED';report['failure']='Cumulative30 exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(x['seconds'] for x in history['rounds'])
    with (E/'replay-results.json').open('w') as f:json.dump(history,f,indent=2);f.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':len(cases),'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
