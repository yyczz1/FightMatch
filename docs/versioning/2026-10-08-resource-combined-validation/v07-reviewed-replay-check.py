#!/usr/bin/env python3
"""Combined contract: actual runner functions, retained FIX05 assertions, bounded owned offline fixtures."""
import subprocess,ast,copy,hashlib,io,json,os,pathlib,sys,time,traceback,types,shlex,re,stat
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
    except (RuntimeError,OSError,subprocess.TimeoutExpired): return True
    return False
SELECTED=['V04 Bee htc then cth', 'V04 Bee cth then htc', 'V04 Bee rejects post-inode', 'V04 Bee rejects post-dir', 'V04 Bee rejects reuse-during', 'V04 fresh sealed state accepted', 'V04 execution state drift rejected', 'V05 full blocking-probe allowance and real timeout', 'V05 near natural boundary actual chain preserves prior Bee error', 'V05 FIX01 natural observation success', 'V05 FIX01 natural observation capacity', 'V05 FIX01 natural observation io', 'FIX02 independent PID discovery closure during-term', 'FIX02 independent PID discovery closure stubborn']
skipped=[]
def case(name,fn):
    if not name.startswith('FIX02 ') or any(c['name']==name for r in history['rounds'] for c in r['cases'] if c['passed']): skipped.append(name); return
    need(time.monotonic()-START<15 and time.monotonic()-START+sum(x['seconds'] for x in history['rounds'])<30,'V04 per-round30 cumulative60'); fn(); cases.append({'name':name,'passed':True})
def bind(names,env):
    defaults={'pending_details':{},'process_snapshot':{},'snapshot_root':None,'closure_closed':None,'transfer_conflicts':set(),'probe_deadline':None,'execution_deadline':None,'work_deadline':None,'restore_deadline':None,'launch_counts':{},'stage_bindings':{},'stage_history':[],'current_stage':'I','D':{},'compiler_parked':[],'compiler_after':{},'compiler_restored':[],'copy':copy,'natural_boundary':None,'bee_ipc':{},'bee_observation_bytes':0,'json':json,'hashlib':hashlib,'utc':lambda:'offline-fixture'}
    for key,value in defaults.items():env.setdefault(key,value)
    if hasattr(env.get('P'),'fs'):env.setdefault('children',lambda path:path.iterdir())
    names=list(names)+[n for n in ('BeeObservationIncomplete','NaturalGraceExpired','ChildArgsProbeError','child_args','blocking_probe_timeout','bee_fd_binding','bee_capture_output','bee_observation_event','probe_timeout','transfer_slots','move_verified','compiler_paths','stage_environment','isolate_stage','checked','read_chunks','bounded_read','bounded_text','scan_tree','children','json_chunks','json_digest','csc_events') if n in F and n not in names and n not in env]
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

history=json.loads((E/'replay-results.json').read_text())
need(history['task']=='RES-COMBINED-V07','Current V04 history')
need(len(history['rounds'])<2 and not any(x['status']=='SOURCE_REPLAY_PASS' for x in history['rounds']),'Maximum two rounds; first green stops')
report={'round':len(history['rounds'])+1,'status':'SOURCE_REPLAY_FAILED','cases':cases,'nativeRuns':0,'realPsCalls':0,'realSignals':0,'realSockets':0,'actualProjectionWrites':0,'fixtureIOOnly':True}
def stat_value(mode,ino,uid=501,nlink=1):
    return types.SimpleNamespace(st_mode=mode,st_ino=ino,st_dev=7,st_uid=uid,st_gid=20,st_nlink=nlink,st_size=0)
def bee_fixture(mode='valid'):
    tmp='/synthetic-tmp'; directory=tmp+'/abcdefgh.xyz'; names=[directory+'/ipc_100_htc',directory+'/ipc_100_cth']; entries={}
    entries[tmp]=stat_value(stat.S_IFDIR|0o700,1,nlink=2);entries[directory]=stat_value(stat.S_IFDIR|0o700,2,nlink=2)
    for index,name in enumerate(names):entries[name]=stat_value(stat.S_IFSOCK|0o700,10+index)
    class VPath(pathlib.PurePosixPath):
        def lstat(self):
            value=entries.get(str(self))
            if isinstance(value,BaseException):raise value
            if value is None:raise FileNotFoundError(str(self))
            return copy.copy(value)
        def is_symlink(self):
            value=entries.get(str(self));return value is not None and not isinstance(value,BaseException) and stat.S_ISLNK(value.st_mode)
    tools={k:{'path':v,**byteid(k.encode())} for k,v in {'editor':'/fixed/Unity','beeBackend':'/fixed/bee_backend','beeDriver':'/fixed/Bee.BeeDriver2.dll','lsof':'/fixed/lsof'}.items()}
    commands={'I':['/fixed/Unity','-batchmode','-projectPath','/P'],'T':['/fixed/Unity','-runTests','-projectPath','/P']}
    root={'pid':100,'ppid':1,'start':'Mon Oct 5 00:00:00 2026','stat':'S','exe':'/fixed/Unity','argv':commands['I'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False}
    bee={**root,'pid':101,'ppid':100,'exe':'/fixed/bee_backend','argv':['/fixed/bee_backend','--ipc']}
    owned={100:copy.deepcopy(root),101:copy.deepcopy(bee)};rows=copy.deepcopy(owned);events=[];calls=[];expected={v['path']:{'bytes':v['bytes'],'sha256':v['sha256']} for v in tools.values()}
    def identity(path):
        if mode.startswith('tool-') and str(path)==tools[mode[5:]]['path']:return byteid(b'drift')
        return expected[str(path)]
    def command(argv):
        pid=int(argv[argv.index('-p')+1]);p=rows[pid]
        return str(pid)+' '+str(p['ppid'])+' '+('Tue Oct 6 00:00:00 2026' if mode=='fresh-start' else p['start'])+' '+p['stat']+' '+p['exe']
    def detail(pid):
        owner=owned[pid];return {'argv':owner['argv']+(['changed'] if mode=='argv' else []),'cwd':'/wrong' if mode=='cwd' else owner['cwd'],'cwdProbeExit':0}
    def lsof(argv,**kwargs):
        need(argv[0]==tools['lsof']['path'] and argv[1:] in [['-a','-p',str(p),'-U','-Fpcftn'] for p in owned],'Only exact owned PID Unix FD command')
        need(0<kwargs['timeout']<=5,'Bounded FD probe');pid=int(argv[3]);calls.append(pid)
        if mode=='fd-timeout':raise subprocess.TimeoutExpired(argv,kwargs['timeout'])
        if mode=='fd-io':raise OSError('synthetic FD read error')
        proof=names[0] if pid==min(owned) else names[1]
        if mode=='fd-mismatch':proof+='.foreign'
        out='p'+str(pid if mode!='fd-pid' else 999)+'\ncfixture\nf7u\nt'+('REG' if mode=='fd-type' else 'unix')+'\nn'+proof+(' type=STREAM' if mode=='fd-suffix' else '')+'\n'
        if mode=='post-inode':entries[names[0]]=stat_value(stat.S_IFSOCK|0o700,999)
        if mode=='post-dir':entries[directory]=stat_value(stat.S_IFDIR|0o700,999,nlink=2)
        if mode=='vanish-during':entries.pop(names[0],None)
        if mode=='reuse-during':rows[100]['start']='Tue Oct 6 00:00:00 2026'
        if mode=='fd-no-record':out=''
        return types.SimpleNamespace(returncode=1 if mode=='fd-no-record' else 0,stdout=out,stderr='failed' if mode=='fd-error' else '')
    env=bind(['check','alive','recorded_chain','no_links','bee_stamp','bee_tools','bee_process','bee_fd_bound','bee_ipc_entry','bee_ipc_snapshot','tree_entries'],{
        'pathlib':types.SimpleNamespace(Path=VPath),'stat':stat,'re':re,'subprocess':types.SimpleNamespace(run=lsof,TimeoutExpired=subprocess.TimeoutExpired),'os':types.SimpleNamespace(getuid=lambda:501),'P':VPath('/P'),'E':VPath('/synthetic-evidence'),'TMP':VPath(tmp),'A':{'stages':[{'id':sid,'argv':argv} for sid,argv in commands.items()]},
        'D':{'beeIpcContract':{'tools':tools,'directoryPattern':'[A-Za-z0-9]{8}[.][A-Za-z0-9]{3}','fdTimeoutSeconds':5},'commands':commands,'beeObservationContract':copy.deepcopy(D['beeObservationContract']),'evidenceSlots':copy.deepcopy(D['evidenceSlots'])},'owned':owned,'process_snapshot':rows,'snapshot_root':100,'ident':identity,'cmd':command,'details':detail,'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'time':types.SimpleNamespace(monotonic=lambda:0.),
        'scan_tree':lambda root:[(tmp,['abcdefgh.xyz'],[]),(directory,[],[pathlib.PurePosixPath(p).name for p in entries if pathlib.PurePosixPath(p).parent==pathlib.PurePosixPath(directory)])]})
    if mode=='stage':env['current_stage']='T'
    if mode=='pid':env['snapshot_root']=200
    if mode=='start':rows[100]['start']='Tue Oct 6 00:00:00 2026'
    if mode=='exe':rows[100]['exe']='/foreign/Unity'
    if mode=='parent':rows[101]['ppid']=999
    if mode=='uid':entries[names[0]].st_uid=502
    if mode=='mode':entries[names[0]].st_mode=stat.S_IFSOCK|0o777
    if mode=='dir-uid':entries[directory].st_uid=502
    if mode=='dir-mode':entries[directory].st_mode=stat.S_IFDIR|0o777
    if mode=='dir-link':entries[directory].st_mode=stat.S_IFLNK|0o700
    if mode=='fifo':entries[names[0]].st_mode=stat.S_IFIFO|0o700
    if mode=='link':entries[names[0]].st_mode=stat.S_IFLNK|0o700
    if mode=='regular':entries[names[0]].st_mode=stat.S_IFREG|0o700
    if mode=='nlink':entries[names[0]].st_nlink=2
    if mode=='vanish-first':entries.pop(names[0])
    if mode=='before-io':entries[names[0]]=PermissionError('synthetic denied lstat')
    return env,entries,names,events,calls,VPath
def bee_order(reverse=False,suffix=False):
    env,entries,names,events,calls,VPath=bee_fixture('fd-suffix' if suffix else 'valid')
    first,second=names[::-1] if reverse else names
    saved=entries.pop(second)
    env['bee_ipc_entry'](VPath(first))
    need(len(env['bee_ipc']['I']['endpoints'])==1,'One endpoint may appear first')
    entries[second]=saved;env['bee_ipc_entry'](VPath(second))
    env['tree_entries'](env['TMP'])
    need(len(env['bee_ipc']['I']['endpoints'])==2 and len([x for x in events if x['kind']=='bee_ipc_admitted'])==2,'Two exact admissions only')
    before=len(calls);env['tree_entries'](env['TMP']);need(len(calls)==before,'Same inode retains original FD proof')
def bee_bad(mode):
    env,entries,names,events,calls,VPath=bee_fixture(mode)
    need(rejected(lambda:env['bee_ipc_entry'](VPath(names[0]))),'Reject '+mode)
    need(not env['bee_ipc'],'No incomplete admission '+mode)
def bee_after(mode):
    env,entries,names,events,calls,VPath=bee_fixture()
    env['bee_ipc_entry'](VPath(names[0]));state=env['bee_ipc']['I']
    if mode=='absent':
        entries.pop(names[0]);result=env['tree_entries'](env['TMP']);need(result['abcdefgh.xyz/ipc_100_htc']['type']=='bee-socket-absent','Exact ENOENT disappearance recorded')
    elif mode=='directory-absent':
        entries.pop(names[0]);entries.pop(str(VPath(names[0]).parent));need(env['bee_ipc_entry'](VPath(names[0]))['type']=='bee-socket-absent','Both same known path and directory absent')
    elif mode=='residual':
        env['process_snapshot'].clear();state['closed']=True;need(env['bee_ipc_entry'](VPath(names[0]))['type']=='bee-socket','Unchanged closed residual accepted')
    elif mode=='closed-I-new-T':
        state['closed']=True;env['current_stage']='T';env['snapshot_root']=200
        need(env['bee_ipc_entry'](VPath(names[0]))['stage']=='I','Prior admitted residual keeps I identity')
        need(rejected(lambda:env['bee_ipc_entry'](VPath(names[1]))),'I never authorizes new T admission')
    else:
        if mode=='inode':entries[names[0]].st_ino+=1
        elif mode=='dir-inode':entries[str(VPath(names[0]).parent)].st_ino+=1
        elif mode=='uid':entries[names[0]].st_uid+=1
        elif mode=='link':entries[names[0]].st_mode=stat.S_IFLNK|0o700
        elif mode=='reused-pid':env['process_snapshot'][100]['start']='reused'
        elif mode=='io':entries[names[0]]=PermissionError('synthetic later lstat failure')
        elif mode=='reappear':
            saved=entries.pop(names[0]);env['bee_ipc_entry'](VPath(names[0]));entries[names[0]]=saved
        elif mode=='wrong-stage':env['current_stage']='T'
        need(rejected(lambda:env['bee_ipc_entry'](VPath(names[0]))),'Reject admitted '+mode)
def bee_unknown(kind):
    env,entries,names,events,calls,VPath=bee_fixture();bad=str(VPath(names[0]).parent)+'/unknown'
    entries.clear();entries[str(env['TMP'])]=stat_value(stat.S_IFDIR|0o700,1);entries[str(VPath(names[0]).parent)]=stat_value(stat.S_IFDIR|0o700,2)
    entries[bad]=stat_value((stat.S_IFSOCK if kind=='socket' else stat.S_IFIFO)|0o700,90)
    need(rejected(lambda:env['tree_entries'](env['TMP'])),'Original unknown '+kind+' rejection')
def bee_second_directory():
    env,entries,names,events,calls,VPath=bee_fixture();env['bee_ipc_entry'](VPath(names[0]));other='/synthetic-tmp/ijklmnop.xyz/ipc_100_cth'
    entries[str(VPath(other).parent)]=stat_value(stat.S_IFDIR|0o700,20);entries[other]=stat_value(stat.S_IFSOCK|0o700,21)
    need(rejected(lambda:env['bee_ipc_entry'](VPath(other))),'One fresh directory per stage')
def bee_path_rejections():
    for path in ('/synthetic-tmp/ipc_100_htc','/synthetic-tmp/abcdefgh.xyz/deeper/ipc_100_htc','/synthetic-tmp/abcdefgh.xyz/ipc_100_bad','/synthetic-tmp/abcdefgh.xyz/ipc_999_htc','/synthetic-tmp/bad/ipc_100_htc'):
        env,entries,names,events,calls,VPath=bee_fixture()
        need(rejected(lambda:env['bee_ipc_entry'](VPath(path))),'Reject unknown IPC structure '+path)
def bee_fresh_T():
    env,entries,names,events,calls,VPath=bee_fixture();env['bee_ipc_entry'](VPath(names[0]));env['bee_ipc']['I']['closed']=True
    oldI=copy.deepcopy(env['bee_ipc']['I']); owners=env['owned']; root=copy.deepcopy(owners[100]); child=copy.deepcopy(owners[101])
    root.update(pid=200,rootPid=200,stage='T',argv=env['D']['commands']['T'],start='Tue Oct 6 00:00:00 2026');child.update(pid=201,ppid=200,rootPid=200,stage='T',start=root['start'])
    owners.clear();owners.update({200:root,201:child});env['process_snapshot'].clear();env['process_snapshot'].update(copy.deepcopy(owners));env.update(current_stage='T',snapshot_root=200)
    directory='/synthetic-tmp/ijklmnop.xyz';names[:]=[directory+'/ipc_200_htc',directory+'/ipc_200_cth'];entries[directory]=stat_value(stat.S_IFDIR|0o700,50,nlink=2)
    for i,name in enumerate(names):entries[name]=stat_value(stat.S_IFSOCK|0o700,51+i);env['bee_ipc_entry'](VPath(name))
    need(env['bee_ipc']['I']==oldI and env['bee_ipc']['T']['rootPid']==200 and len(env['bee_ipc']['T']['endpoints'])==2,'T gets separate fresh PID/directory/FD state')
def boundary(mode):
    clock=[0.];live=[True];signals=[];events=[];root={'pid':100,'ppid':1,'exe':'/fixed/Unity','start':'start','argv':['/fixed/Unity'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False,'stat':'S'}
    child={**root,'pid':101,'ppid':100,'exe':'/fixed/bee_backend','argv':['/fixed/bee_backend','--ipc']}
    rows=lambda:{101:copy.deepcopy(child)} if live[0] else {}
    first=[True]
    def resources():
        if first[0]:
            first[0]=False;clock[0]=60.001
            if mode in ('timeout','unclosed-prior'):raise subprocess.TimeoutExpired(['synthetic'],5)
            if mode=='io':raise OSError('synthetic I/O error')
            if mode=='total':clock[0]=300.
    def kill(pid,sig):
        signals.append(pid)
        if mode!='unclosed-prior':live[0]=False
    env=bind(['check','alive','recorded_chain','monitor','closure'],{'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda s:clock.__setitem__(0,clock[0]+s)),'utc':lambda:'fixture','os':types.SimpleNamespace(kill=kill),'signal':types.SimpleNamespace(SIGTERM=15),'pathlib':pathlib,'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30},'stages':[{'id':'I','argv':root['argv']}]},'D':{'limits':{'finalizationSeconds':30}},'P':pathlib.Path('/P'),'owned':{100:root,101:child},'active':types.SimpleNamespace(poll=lambda:0),'ps':rows,'details':lambda pid:{'argv':child['argv'],'cwd':'/P','cwdProbeExit':0},'snapshot_consumers':lambda *a:None,'resources':resources,'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.,'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'execution_deadline':300.,'work_deadline':120.})
    failed=None
    try:env['closure']('I',100,'fixture')
    except RuntimeError as error:failed=str(error)
    if mode=='natural':
        need(failed is None and env['closure_closed'] and signals==[101] and not env['monitor_errors'],'Natural boundary transitions to one TERM without false errors')
        need(any(x['kind']=='natural_grace_elapsed' for x in events),'Observable normal boundary')
    elif mode in ('timeout','io'):
        need(failed is None and env['closure_closed'] and len(env['monitor_errors'])==1,'Real error remains despite successful close')
        need(rejected(lambda:env['check'](not env['monitor_errors'],'Monitor failures')),'Existing stage failure gate still rejects')
    elif mode=='unclosed-prior':
        need(failed and not env['closure_closed'] and any('timed out' in x['error'] for x in env['monitor_errors']),'Unclosed TERM fails and earlier TimeoutExpired retained')
    elif mode=='total':
        need(failed and not signals and any('Total mechanical deadline exhausted' in x['error'] for x in env['monitor_errors']),'Absolute total budget outranks natural boundary')
    report.setdefault('boundaryEvidence',[]).append({'mode':mode,'signalsSynthetic':signals,'closed':env['closure_closed'],'failure':failed,'monitorErrors':env['monitor_errors']})

def discovery_boundary(kind):
    clock=[0.]; live={100:True,101:True}; first_child=[True]; signals=[]; events=[]
    root={'pid':100,'ppid':1,'exe':'/fixed/Unity','start':'root-start','argv':['/fixed/Unity'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False,'stat':'S'}
    child={'pid':101,'ppid':100,'exe':'/fixed/worker','start':'child-start','argv':['/fixed/worker'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False,'stat':'S'}
    records={100:copy.deepcopy(root),101:copy.deepcopy(child)}
    def details(pid):
        item=records[pid]
        if pid==101 and first_child[0]:
            first_child[0]=False
            if kind=='ordinary':raise RuntimeError('synthetic ordinary child identity failure')
            if kind=='parent-first':
                live[100]=False;records[101]['ppid']=1;clock[0]=60.001
            else:
                clock[0]=60.001;env['probe_timeout']()
        if kind=='unknown' and pid==101:raise RuntimeError('synthetic unverified child identity')
        return {'argv':item['argv'],'cwd':item['cwd'],'cwdProbeExit':0}
    def kill(pid,sig):
        need(pid in records and sig==15 and live[pid],'Only live independently verified PID receives TERM')
        need(pid not in signals,'At most one TERM per PID')
        signals.append(pid)
        if not (kind=='stubborn' and pid==101):live[pid]=False
        # Parent exit never makes a child disappear; surviving children reparent.
        for other,item in records.items():
            if live.get(other) and item['ppid']==pid:item['ppid']=1
        if kind=='during-term' and pid==101:
            records[102]={**copy.deepcopy(child),'pid':102,'ppid':100,'start':'late-start','argv':['/fixed/worker','late']};live[102]=True
    env=bind(['check','alive','recorded_chain','register','discover','recover_root','snapshot_consumers','monitor','closure'],{
        'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda seconds:clock.__setitem__(0,clock[0]+seconds)),
        'utc':lambda:'fixture','os':types.SimpleNamespace(kill=kill),'signal':types.SimpleNamespace(SIGTERM=15),'pathlib':pathlib,
        'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30},'stages':[{'id':'I','argv':root['argv']}]},
        'D':{'limits':{'finalizationSeconds':30}},'P':pathlib.Path('/P'),'owned':{100:copy.deepcopy(root)},
        'active':types.SimpleNamespace(poll=lambda:None if live[100] else 0),'ps':lambda:{p:copy.deepcopy(item) for p,item in records.items() if live[p]},
        'details':details,'consumer_guard':lambda rows:None,'resources':lambda:None,'projection_guard':lambda:None,
        'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.,'event':lambda event_kind,**kw:events.append({'kind':event_kind,**kw}),
        'execution_deadline':300.,'work_deadline':120.})
    if kind=='ordinary':
        env['probe_deadline']=60.;env['natural_boundary']=60.
        env['monitor']('natural-closure',env['ps'](),'I',100,force=True)
        need(101 in env['pending_details'] and env['monitor_errors'],'Ordinary identity failure remains pending and fails monitor')
        need('synthetic ordinary child identity failure' in env['pending_details'][101]['firstError'],'Original identity failure retained')
        need(rejected(lambda:env['check'](not env['monitor_errors'],'Monitor failures')),'Stage failure gate remains closed')
    else:
        failure=None
        try:env['closure']('I',100,'synthetic discovery boundary')
        except RuntimeError as error:failure=str(error)
        if kind in ('natural','parent-first','during-term'):
            expected={'natural':[101,100],'parent-first':[101],'during-term':[101,102,100]}[kind]
            need(failure is None and env['closure_closed'] and signals==expected and not any(live.values()),'Every independently live owned member closed '+kind)
            need(not env['pending_details'] and not env['monitor_errors'],'Normal boundary adds no identity or monitor failure')
            need(not any(e['kind']=='pending_identity' for e in events),'Natural control never becomes identity failure')
            need(any(e['kind']=='natural_grace_elapsed' for e in events),'Natural boundary observable')
            if kind=='parent-first':need(records[101]['ppid']==1 and env['owned'][101]['ppid']==100,'Stored verified chain authorizes known reparented survivor')
        elif kind=='stubborn':
            need(failure and not env['closure_closed'] and signals==[101,100] and live[101] and not live[100],'Parent exit cannot hide surviving child; no repeated TERM')
        elif kind=='unknown':
            need(failure and not env['closure_closed'] and not signals and 101 in env['pending_details'] and env['monitor_errors'],'Unknown identity never receives signal; failed discovery closes signal gate')
        need(clock[0]<=90.001001,'Original60+30 window never extended')
    need({'snapshot_consumers','discover','register'}<=traces,'Actual discovery functions invoked')
    report.setdefault('fix02DiscoveryEvidence',[]).append({'kind':kind,'pending':copy.deepcopy(env['pending_details']),'monitorErrors':copy.deepcopy(env['monitor_errors']),'signalsSynthetic':signals,'liveByPid':dict(live),'closed':env['closure_closed'],'clock':clock[0],'events':events})

def probe_budgets():
    env=bind(['check'],{'time':types.SimpleNamespace(monotonic=lambda:61.),'natural_boundary':60.,'probe_deadline':60.,'execution_deadline':61.,'work_deadline':0.})
    need(rejected(env['probe_timeout']),'Total deadline first')
    env.update(execution_deadline=100.,natural_boundary=None,probe_deadline=None,work_deadline=60.)
    need(rejected(env['probe_timeout']),'Work deadline retained')
    env.update(natural_boundary=None,probe_deadline=60.)
    need(rejected(env['probe_timeout']),'Budget-shortened natural window is failure')
def state_before(drift):
    expected=copy.deepcopy(D['compilePlan']['preserveInPlaceIdentities']);actual=copy.deepcopy(expected)
    if drift:actual['Library/Bee/TundraBuildState.state']=byteid(b'drift')
    def identity(path):
        key=str(path).removeprefix('/P/')
        return actual[key] if key in actual else byteid(b'tool')
    env=bind(['check','capture_before'],{'pathlib':pathlib,'json':json,'R':pathlib.Path('/R'),'P':pathlib.Path('/P'),'N':{'editor':D['editor'],'restoreBaseline':{},'requiredAssemblies':[]},'D':{'compilePlan':{'preserveInPlaceIdentities':expected,'previousGraph':{'path':'graph.json'}}},'dlls':lambda:{},'tree_entries':lambda p:{},'old_evidence':lambda:{},'inventory':lambda *a:{},'compiler_paths':lambda:{},'ident':identity,'bounded_text':lambda p:'{"Nodes":[]}','write':lambda *a:None,'time':types.SimpleNamespace(monotonic=lambda:0.)})
    if drift:need(rejected(env['capture_before']),'Exact state preimage drift rejected before synchronization')
    else:env['capture_before']()

def actual_probe_strings():
    evidence=json.loads((R/'TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.json').read_text())
    env=bind(['check','bee_fd_bound'],{'re':re,'pathlib':pathlib,'TMP':None,'D':D,'snapshot_root':None});pid=evidence['pid'];total=0
    for snapshot in evidence['snapshots']:
        raw=snapshot['rawStdout'];paths=sorted({line[1:] for line in raw.splitlines() if line.startswith('n/')})
        need(len(paths)==2,'Two real full endpoint paths per snapshot')
        header='p'+str(pid)+'\ncPython\n';records=[];current=[]
        for line in raw.splitlines():
            if line.startswith('f'):
                if current:records.append(current)
                current=[line]
            elif current:current.append(line)
        if current:records.append(current)
        arrows=header+''.join('\n'.join(x)+'\n' for x in records if any(line.startswith('n->') for line in x))
        for path in paths:
            need(env['bee_fd_bound'](raw,pid,path),'Actual server path matches')
            need(not env['bee_fd_bound'](arrows,pid,path),'Client arrows do not bind target path')
            need(not env['bee_fd_bound'](raw,pid,path+'.wrong'),'Wrong full path not accepted')
            need(rejected(lambda:env['bee_fd_bound'](raw,pid+1,path)),'Wrong PID rejected')
            need(rejected(lambda:env['bee_fd_bound'](raw.replace('tunix','tREG'),pid,path)),'Wrong descriptor type rejected');total+=1
    report['actualProbeFixtures']={'snapshots':3,'exactPathMatches':total,'source':identity(R/'TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.json'),'newRealProbes':0}
def observation_case(mode):
    fixture_mode='fd-mismatch' if mode in ('mismatch','write-with-primary') else 'valid'
    env,entries,names,events,calls,VPath=bee_fixture(fixture_mode)
    if mode=='oversize':
        env['D']['beeObservationContract']['rawBytesPerStream']=8
    if mode=='primary-and-after':
        def failing(argv,**kwargs):
            entries[names[0]]=PermissionError('secondary lstat failed')
            raise subprocess.TimeoutExpired(argv,kwargs['timeout'],output=b'partial raw output',stderr=b'original stderr')
        env['subprocess']=types.SimpleNamespace(run=failing,TimeoutExpired=subprocess.TimeoutExpired)
    if mode in ('write-only','write-with-primary'):
        prior=env['event']
        def failed_write(kind,**kw):
            if kind=='bee_fd_observation':raise OSError('synthetic evidence write failure')
            prior(kind,**kw)
        env['event']=failed_write
    error=None
    try:env['bee_ipc_entry'](VPath(names[0]))
    except BaseException as ex:error=ex
    if mode=='success':need(error is None,'Successful admission')
    elif mode=='primary-and-after':
        need(isinstance(error,subprocess.TimeoutExpired),'Finally lstat does not replace first TimeoutExpired')
    elif mode=='write-with-primary':
        need(type(error) is RuntimeError and str(error)=='INCOMPLETE: Bee endpoint lacks exact owned FD binding','Evidence write cannot replace primary binding failure')
        need(any('observation write failed' in note for note in error.__notes__),'Secondary write failure recorded as INCOMPLETE note')
    else:need(error is not None and ('INCOMPLETE' in str(error)),'Observation failure closes admission gate')
    if mode not in ('write-only','write-with-primary'):
        records=[e for e in events if e['kind']=='bee_fd_observation'];need(len(records)==1,'One bounded raw observation saved on success or failure')
        record=records[0];need(record['beforeLstat'] and record['afterLstat'] and record['targetPath']==names[0] and record['candidates'],'Before/after and candidate selection retained')
        for probe in record['probes']:
            need('afterLstat' in probe and 'rawStdout' in probe and 'rawStderr' in probe and probe['argv'] and probe['timeoutSeconds']<=5,'Raw output, argv, timeout and after image retained')
        if mode=='primary-and-after':
            need(record['probes'][0]['rawStdout']=='partial raw output' and record['probes'][0]['rawStderr']=='original stderr' and record['afterLstat']['errorType']=='PermissionError','Partial raw output and failing after image preserved')
        if mode=='oversize':need(record['probes'][0]['rawStdoutTruncated'],'Truncation explicit and never accepted')
    report.setdefault('observationCases',[]).append({'mode':mode,'error':None if error is None else {'type':type(error).__name__,'message':str(error)},'observations':[e for e in events if e['kind']=='bee_fd_observation']})
def blocking_admission():
    clock=[59.997439];calls=[]
    def run(argv,**kw):
        calls.append({'argv':argv,'timeout':kw['timeout']})
        raise subprocess.TimeoutExpired(argv,kw['timeout'])
    env=bind(['check','cmd','ps'],{'time':types.SimpleNamespace(monotonic=lambda:clock[0]),'subprocess':types.SimpleNamespace(run=run),'json':json,'probe_deadline':60.,'natural_boundary':60.,'execution_deadline':300.})
    try:env['ps']()
    except env['NaturalGraceExpired']:pass
    else:raise AssertionError('Insufficient natural allowance must be normal control')
    need(not calls,'2.561ms remaining never launches ps')
    clock[0]=50.
    try:env['ps']()
    except subprocess.TimeoutExpired:pass
    else:raise AssertionError('Started real TimeoutExpired propagates')
    need(len(calls)==1 and calls[0]['timeout']==5.,'Full fixed probe allowance only')
    clock[0]=60.;env['execution_deadline']=60.
    need(rejected(env['ps']) and len(calls)==1,'Total deadline outranks normal natural boundary')
    report['blockingProbeAdmission']={'insufficientSeconds':0.002561,'callsAtInsufficientAllowance':0,'inFlightTimeoutPreserved':True,'totalDeadlineFirst':True}
def near_boundary_closure():
    clock=[0.];live={100:True,101:True};signals=[];calls=[];first=[True];events=[]
    root={'pid':100,'ppid':1,'exe':'/fixed/Unity','start':'root','argv':['/fixed/Unity'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False,'stat':'S'}
    child={**root,'pid':101,'ppid':100,'exe':'/fixed/worker','argv':['/fixed/worker']};records={100:root,101:child}
    def poll():
        if first[0]:first[0]=False;clock[0]=59.997439
        return None if live[100] else 0
    def run(argv,**kw):
        need(env['natural_boundary'] is None,'No subprocess launch in short natural remainder')
        calls.append({'argv':argv,'timeout':kw['timeout']});return types.SimpleNamespace(returncode=0,stdout='',stderr='')
    def rows():
        env['cmd'](['/bin/ps','synthetic-offline-args'])
        return {p:copy.deepcopy(v) for p,v in records.items() if live[p]}
    def kill(pid,sig):signals.append(pid);live[pid]=False
    env=bind(['check','alive','recorded_chain','register','discover','recover_root','snapshot_consumers','monitor','closure','cmd'],{
        'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda seconds:clock.__setitem__(0,clock[0]+seconds)),'utc':lambda:'fixture','pathlib':pathlib,'json':json,
        'subprocess':types.SimpleNamespace(run=run),'os':types.SimpleNamespace(kill=kill),'signal':types.SimpleNamespace(SIGTERM=15),
        'P':pathlib.Path('/P'),'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30},'stages':[{'id':'I','argv':root['argv']}]},'D':{'limits':{'finalizationSeconds':30}},
        'owned':{100:copy.deepcopy(root)},'active':types.SimpleNamespace(poll=poll),'ps':rows,'details':lambda p:{'argv':records[p]['argv'],'cwd':'/P','cwdProbeExit':0},'consumer_guard':lambda rows:None,
        'resources':lambda:None,'projection_guard':lambda:None,'monitor_errors':[{'error':'prior Bee binding failure'}],'monitor_cycles':[],'last_monitor':0.,'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'execution_deadline':300.,'work_deadline':120.})
    env['closure']('I',100,'preserve prior Bee failure')
    need(env['closure_closed'] and signals==[101,100] and not any(live.values()),'Normal short natural remainder transitions to fresh TERM discovery')
    need(env['monitor_errors']==[{'error':'prior Bee binding failure'}],'Early transition does not erase prior Bee failure')
    need(any(e['kind']=='natural_grace_elapsed' for e in events) and calls,'Boundary and actual TERM probes observed')
    report['nearBoundaryClosure']={'signalsSynthetic':signals,'monitorErrors':env['monitor_errors'],'events':events,'subprocessInvocations':calls,'realProcessCalls':0}


def fix01_natural_observation(mode):
    env,entries,names,events,calls,VPath=bee_fixture()
    clock=[0.];live={100:True,101:True};signals=[];caught=[];first=[True]
    env['time']=types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda s:clock.__setitem__(0,clock[0]+s))
    original_event=env['event']
    def event(kind,**kw):
        if mode=='io' and kind=='bee_fd_observation':raise OSError('synthetic observation disk error')
        original_event(kind,**kw)
    if mode=='capacity':env['D']['beeObservationContract']['recordBytes']=1
    env['event']=event;env['A']['stopping']={'naturalGraceSeconds':60,'termGraceSeconds':30};env['D']['limits']={'finalizationSeconds':30}
    initial=copy.deepcopy(env['owned'])
    def rows():return {p:copy.deepcopy(v) for p,v in initial.items() if live[p]}
    def resources():
        if not first[0]:return
        first[0]=False;clock[0]=59.997439
        try:env['bee_ipc_entry'](VPath(names[0]))
        except BaseException as error:caught.append(error);raise
    def kill(pid,sig):
        need(sig==15 and live[pid] and pid not in signals,'One synthetic TERM per independently live PID')
        signals.append(pid);live[pid]=False
    env.update(ps=rows,active=types.SimpleNamespace(poll=lambda:None if live[100] else 0),consumer_guard=lambda rows:None,resources=resources,projection_guard=lambda:None,monitor_errors=[],monitor_cycles=[],last_monitor=0.,execution_deadline=300.,work_deadline=120.,os=types.SimpleNamespace(getuid=lambda:501,kill=kill),signal=types.SimpleNamespace(SIGTERM=15))
    bind(['register','discover','recover_root','snapshot_consumers','monitor','closure'],env)
    env['closure']('I',100,'synthetic natural plus observation path')
    need(env['closure_closed'] and signals==[101,100] and not any(live.values()),'Fresh TERM and process closure remain intact')
    need(len(caught)==1 and not calls,'No FD subprocess started with only2.561ms allowance')
    if mode=='success':
        need(isinstance(caught[0],env['NaturalGraceExpired']) and not env['monitor_errors'],'Successful observation retains normal boundary control')
        need(any(e['kind']=='bee_fd_observation' for e in events),'Normal boundary observation persisted')
    else:
        error=caught[0]
        need(type(error) is RuntimeError and 'INCOMPLETE' in str(error) and isinstance(error.__cause__,env['NaturalGraceExpired']),'Observation failure replaces control type and retains grace cause')
        need(isinstance(error.__context__,(OSError,RuntimeError)),'Original observation write/capacity failure retained as context')
        need(env['monitor_errors'] and any('INCOMPLETE' in item['error'] for item in env['monitor_errors']),'Actual monitor records observation failure')
        need(rejected(lambda:env['check'](not env['monitor_errors'],'Monitor failures')),'Existing stage gate rejects despite closed processes')
    report.setdefault('fix01NaturalObservation',[]).append({'mode':mode,'raisedType':type(caught[0]).__name__,'causeType':None if caught[0].__cause__ is None else type(caught[0].__cause__).__name__,'contextType':None if caught[0].__context__ is None else type(caught[0].__context__).__name__,'monitorErrors':env['monitor_errors'],'signalsSynthetic':signals,'closed':env['closure_closed'],'fdSubprocessCalls':calls})
def fix01_timeout_capacity(stream):
    env,entries,names,events,calls,VPath=bee_fixture();caught=[];limit=env['D']['beeObservationContract']['rawBytesPerStream']
    partial=b'x'*(limit+1)
    def timeout(argv,**kw):
        raise subprocess.TimeoutExpired(argv,kw['timeout'],output=partial if stream=='stdout' else b'within-bound',stderr=partial if stream=='stderr' else b'within-bound')
    env['subprocess']=types.SimpleNamespace(run=timeout,TimeoutExpired=subprocess.TimeoutExpired)
    def resources():
        try:env['bee_ipc_entry'](VPath(names[0]))
        except BaseException as error:caught.append(error);raise
    env.update(snapshot_consumers=lambda *a:None,resources=resources,projection_guard=lambda:None,monitor_errors=[],monitor_cycles=[],last_monitor=0.)
    bind(['monitor'],env);env['monitor']('running',env['process_snapshot'],'I',100,force=True)
    need(len(caught)==1 and type(caught[0]) is RuntimeError and isinstance(caught[0].__cause__,subprocess.TimeoutExpired),'Capacity INCOMPLETE retains original TimeoutExpired cause')
    need('INCOMPLETE' in str(caught[0]) and 'TimeoutExpired' in str(caught[0]) and env['monitor_errors'],'Capacity and timeout both reach actual monitor failure')
    records=[e for e in events if e['kind']=='bee_fd_observation'];need(len(records)==1,'Failure observation remains persisted')
    item=records[0]['probes'][0];failure=item['capacityFailure']
    need(failure['status']=='INCOMPLETE' and failure[stream+'Bytes']==limit+1 and item['error']['type']=='TimeoutExpired','Record preserves capacity limit, measured size, and timeout')
    need(item['raw'+stream.capitalize()+'Truncated'] and item['afterLstat'] and records[0]['afterLstat'],'Bounded partial bytes and after images retained')
    need(rejected(lambda:env['check'](not env['monitor_errors'],'Monitor failures')),'Timeout capacity failure cannot pass stage gate')
    report.setdefault('fix01TimeoutCapacity',[]).append({'stream':stream,'raisedType':type(caught[0]).__name__,'causeType':type(caught[0].__cause__).__name__,'capacityFailure':failure,'monitorErrors':env['monitor_errors'],'observation':records[0]})


def v06_saved_paths():
    lines=(R/'TestArtifacts/FightMatch/RES-COMBINED-V05/run/process-events.jsonl').read_text().splitlines();checks=[]
    for line_no in (192,203,207,211,218):
        event=json.loads(lines[line_no-1]);need(event['kind']=='bee_fd_observation','Fixed real event kind')
        target=event['targetPath'];root=event['rootPid'];directory=pathlib.Path(target).parent;tmp=directory.parent
        need(event['beforeLstat']==event['afterLstat'],'Real endpoint same before/after identity')
        probe=next(p for p in event['probes'] if p['candidatePid']==root);raw=probe['rawStdout']
        need(probe['exitCode']==0 and probe['parseResult'] is False,'Old V05 failed parse remains historical fact')
        env=bind(['check','bee_fd_bound'],{'re':re,'pathlib':pathlib,'TMP':tmp,'D':D,'snapshot_root':root})
        for suffix in ('htc','cth'):
            endpoint=str(directory/('ipc_'+str(root)+'_'+suffix));matches=[]
            need(env['bee_fd_bound'](raw,root,endpoint,matches),'Real extra-separator complete path binds')
            need(matches and all(x['spelling']=='extra-tmp-separator' for x in matches),'Exact extra-separator spelling recorded')
            canonical=raw.replace(str(tmp)+'//',str(tmp)+'/');matches=[]
            need(env['bee_fd_bound'](canonical,root,endpoint,matches) and all(x['spelling']=='standard' for x in matches),'Standard spelling retained')
            invalid=[raw.replace(str(tmp)+'//',str(tmp)+'///'),raw.replace(str(tmp)+'//',str(tmp)+'/./'),raw.replace(str(tmp)+'//',str(tmp)+'/../'),raw.replace(str(tmp)+'//',str(tmp)+'/wrong/'),raw.replace('/ipc_'+str(root),'//ipc_'+str(root)),raw.replace('ipc_'+str(root),'ipc_'+str(root+1)),raw.replace('_'+suffix,'_wrong')]
            for value in invalid:need(not env['bee_fd_bound'](value,root,endpoint),'Other spelling must not bind')
            need(rejected(lambda:env['bee_fd_bound'](raw,root+1,endpoint)),'Wrong FD PID rejected')
            need(rejected(lambda:env['bee_fd_bound'](raw.replace('tunix','tREG'),root,endpoint)),'Wrong FD type rejected')
        checks.append({'line':line_no,'oldParseResult':probe['parseResult'],'newMatches':'htc/cth exact extra separator','beforeAfterEqual':True})
    report['v06SavedPathEvidence']=checks
def v06_live_spelling():
    env,entries,names,events,calls,VPath=bee_fixture();original=env['subprocess'].run
    def run(argv,**kw):
        result=original(argv,**kw);result.stdout=result.stdout.replace('/synthetic-tmp/','/synthetic-tmp//');return result
    env['subprocess']=types.SimpleNamespace(run=run,TimeoutExpired=subprocess.TimeoutExpired)
    env['bee_ipc_entry'](VPath(names[0]));env['bee_ipc_entry'](VPath(names[1]))
    records=[e for e in events if e['kind']=='bee_fd_observation'];matches=[m for e in records for p in e['probes'] for m in p.get('matchedSpellings',[])]
    need(len(env['bee_ipc']['I']['endpoints'])==2 and len(matches)==2 and all(m['spelling']=='extra-tmp-separator' for m in matches),'Full binding preserves exact alias labels in original raw observation')
    need(all('/synthetic-tmp//' in p['rawStdout'] for e in records for p in e['probes']),'Raw FD strings not rewritten')
    report['v06SpellingObservations']=records
def v06_child(mode):
    now=[10. if mode!='fresh-natural' else 54.];calls=[];events=[];seen=[];signals=[];fresh_count=[0]
    root={'pid':100,'ppid':1,'start':'Thu Oct 8 10:00:00 2026','stat':'S','exe':'/fixed/Unity','argv':['/fixed/Unity'],'cwd':'/P','cwdProbeExit':0,'stage':'I','rootPid':100,'termSent':False}
    child={'pid':101,'ppid':100,'start':'Thu Oct 8 10:51:37 2026','stat':'S','exe':'(adb)' if mode=='adb-label' else '/fixed/worker'}
    initial={100:copy.deepcopy(root),101:copy.deepcopy(child)};fresh={100:copy.deepcopy(root),303:{'pid':303,'ppid':1,'start':'Thu Oct 8 10:10:00 2026','stat':'S','exe':'/unrelated/fresh'}}
    if mode in ('present','reused','zombie'):
        fresh[101]=copy.deepcopy(child)
        if mode=='reused':fresh[101]['start']='Thu Oct 8 10:52:00 2026'
        if mode=='zombie':fresh[101]['stat']='Z'
    def run(argv,**kw):
        calls.append({'argv':argv,'timeout':kw['timeout']})
        if argv==['/bin/ps','-ww','-p','101','-o','args=']:
            if mode=='io':raise PermissionError('synthetic args permission error')
            if mode=='timeout':raise subprocess.TimeoutExpired(argv,kw['timeout'])
            if mode=='fresh-natural':now[0]=58.1
            return types.SimpleNamespace(returncode=2 if mode=='exit2' else (0 if mode in ('complete','cwd-failure') else 1),stdout='/fixed/worker' if mode in ('complete','cwd-failure') else ('not-empty' if mode=='stdout' else ''),stderr='not-empty' if mode=='stderr' else '')
        if argv==['/usr/sbin/lsof','-a','-p','101','-d','cwd','-Fn']:
            return types.SimpleNamespace(returncode=1 if mode=='cwd-failure' else 0,stdout='' if mode=='cwd-failure' else 'n/P\n',stderr='')
        need(argv==['/bin/ps','-ww','-axo','pid=,ppid=,lstart=,stat=,comm='],'Only existing full fresh snapshot command')
        fresh_count[0]+=1
        if mode=='fresh-io':raise OSError('synthetic fresh snapshot IO')
        if mode=='fresh-timeout':raise subprocess.TimeoutExpired(argv,kw['timeout'])
        if mode=='fresh-exit':return types.SimpleNamespace(returncode=1,stdout='',stderr='denied')
        out='\n'.join(str(pid)+' '+str(v['ppid'])+' '+v['start']+' '+v['stat']+' '+v['exe'] for pid,v in fresh.items())
        return types.SimpleNamespace(returncode=0,stdout=out,stderr='')
    env=bind(['check','cmd','ps','alive','details','register','discover','recorded_chain','recover_root','snapshot_consumers','monitor'],{
        'json':json,'shlex':shlex,'re':re,'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:now[0]),'subprocess':types.SimpleNamespace(run=run,TimeoutExpired=subprocess.TimeoutExpired),
        'P':pathlib.Path('/P'),'A':{'stages':[{'id':'I','argv':root['argv']},{'id':'T','argv':root['argv']}]},'N':{'editor':{'path':'/fixed/Unity'}},'owned':{100:copy.deepcopy(root)},'active':None,
        'consumer_guard':lambda rows:seen.append(copy.deepcopy(rows)),'resources':lambda:None,'projection_guard':lambda:None,'utc':lambda:'fixture','event':lambda kind,**kw:events.append({'kind':kind,**kw}),
        'monitor_errors':[],'monitor_cycles':[],'last_monitor':0.,'execution_deadline':300.,'work_deadline':120.,'probe_deadline':60. if mode=='fresh-natural' else None,'natural_boundary':60. if mode=='fresh-natural' else None})
    if mode=='parent-stage':env['owned'][100]['stage']='T'
    if mode=='legacy':
        lines=(R/'TestArtifacts/FightMatch/RES-COMBINED-V05/run/process-events.jsonl').read_text().splitlines();old=json.loads(lines[228]);failure=json.loads(lines[229])
        need(old['kind']=='pending_identity' and failure['kind']=='monitor_failure' and 'argsProbe' not in old['identity'],'Old failure lacks structured double-empty proof')
        def old_details(pid):raise RuntimeError(old['identity']['firstError'])
        env['details']=old_details
    env['monitor']('natural-closure',initial,'I',100,force=True)
    accepted=mode in ('absent','adb-label')
    if accepted:
        need(not env['monitor_errors'] and not env['pending_details'] and 101 not in env['owned'],'Absent child has no fabricated owned identity or failure')
        lifecycle=[e for e in events if e['kind']=='transient-child-exited'];need(len(lifecycle)==1 and fresh_count[0]==1,'Exactly one fresh confirmation lifecycle')
        proof=lifecycle[0]['confirmation'];need(proof['pidAbsent'] and proof['argsProbe']['returncode']==1 and proof['argsProbe']['stdout']==proof['argsProbe']['stderr']=='' and proof['parentChain'],'Structured double-empty and verified chain evidence')
        need(seen and set(seen[-1])=={100,303} and set(initial)=={100,303} and set(env['process_snapshot'])=={100,303},'Subsequent guards use fresh snapshot, not old absence guess')
    elif mode=='complete':
        need(101 in env['owned'] and not env['monitor_errors'] and not fresh_count[0],'Normal complete identity registration preserved')
    else:
        need(env['monitor_errors'] and 101 in env['pending_details'] and 101 not in env['owned'],'Unproved exit remains failure with no owned authority '+mode)
        need(not any(e['kind']=='transient-child-exited' for e in events),'No lifecycle acceptance on failure')
        if mode in ('stdout','stderr','exit2','io','timeout','cwd-failure','legacy','parent-stage'):need(not fresh_count[0],'No special confirmation for other failures')
        if mode in ('present','reused','zombie','fresh-io','fresh-timeout','fresh-exit'):need(fresh_count[0]==1,'No repeated fresh query')
    need(not signals,'No real or synthetic TERM permission granted to unregistered PID')
    report.setdefault('v06ChildEvidence',[]).append({'mode':mode,'freshQueries':fresh_count[0],'calls':calls,'ownedPids':sorted(env['owned']),'pending':copy.deepcopy(env['pending_details']),'monitorErrors':env['monitor_errors'],'guardRows':seen,'events':events,'signals':signals})


def v07_lifetime(mode):
    env,entries,names,events,calls,VPath=bee_fixture()
    if mode=='saved':
        lines=(R/'TestArtifacts/FightMatch/RES-COMBINED-V06/run/process-events.jsonl').read_text().splitlines()
        a,b,c=[json.loads(lines[n-1]) for n in (185,186,187)]
        need(a['beforeLstat']['ino']==34720956 and b['identity']==a['beforeLstat'] and c['beforeLstat']['ino']==34720957 and c['afterLstat']=={'ENOENT':True},'Actual185-187 lifecycle evidence')
        parser=bind(['check','bee_fd_bound'],{'re':re,'pathlib':pathlib,'TMP':pathlib.Path(a['targetPath']).parent.parent,'D':D,'snapshot_root':a['rootPid']})
        raw=a['probes'][0]['rawStdout']
        for name in (a['targetPath'],c['targetPath']):
            need(parser['bee_fd_bound'](raw,a['rootPid'],name),'Saved complete raw FD path')
            need(parser['bee_fd_bound'](raw.replace(str(parser['TMP'])+'//',str(parser['TMP'])+'/'),a['rootPid'],name),'Saved standard spelling')
        env['bee_ipc_entry'](VPath(names[0]));orig=VPath.lstat;counts=[0]
        def lstat(path):
            if str(path)==names[1]:
                counts[0]+=1
                if counts[0]==2:entries.pop(names[1],None)
            return orig(path)
        VPath.lstat=lstat
        result=env['bee_ipc_entry'](VPath(names[1]))
        need(result['type']=='bee-socket-absent' and names[1] not in env['bee_ipc']['I']['endpoints'],'No fabricated binding for real187 before-probe disappearance')
        need(any(x.get('lifecycle')=='observed-ENOENT-not-bound' for x in events),'Raw lifecycle event retained')
        report['savedV06Evidence']={'lines':[185,186,187],'oldFailedUnchanged':True}
        return
    if mode in ('vanish-first','vanish-during'):
        env,entries,names,events,calls,VPath=bee_fixture(mode)
        need(env['bee_ipc_entry'](VPath(names[0]))['type']=='bee-socket-absent' and not env['bee_ipc'],'No fabricated vanished binding');return
    env['bee_ipc_entry'](VPath(names[0]));before=len(calls)
    if mode=='nlink':
        entries[str(VPath(names[0]).parent)].st_nlink+=1
        env['bee_ipc_entry'](VPath(names[0]))
        need(len(calls)==before and any(x['kind']=='bee_directory_difference' and not x['identityChanged'] for x in events),'Directory mutable field recorded without identity replacement');return
    if mode in ('inode','directory-inode','unknown-rebind'):
        entries[names[0]].st_ino+=100
        if mode=='directory-inode':entries[str(VPath(names[0]).parent)].st_ino+=100
        if mode=='unknown-rebind':
            env['subprocess']=types.SimpleNamespace(run=lambda *a,**k:types.SimpleNamespace(returncode=1,stdout='',stderr=''),TimeoutExpired=subprocess.TimeoutExpired)
            need(rejected(lambda:env['bee_ipc_entry'](VPath(names[0]))),'Still-present unbound replacement rejected')
            need(env['bee_ipc']['I']['endpoints'][names[0]]['absent'],'Old proof invalidated');return
        env['bee_ipc_entry'](VPath(names[0]))
        need(len(calls)>before and env['bee_ipc']['I']['endpoints'][names[0]]['identity']['ino']==110,'New inode gets full fresh FD binding');return
    if mode in ('second-same','second-directory'):
        env['bee_ipc_entry'](VPath(names[1]))
        for name in names:entries.pop(name);env['bee_ipc_entry'](VPath(name))
        oldnames=list(names)
        if mode=='second-directory':
            directory='/synthetic-tmp/ijklmnop.xyz';names[:]=[directory+'/ipc_100_htc',directory+'/ipc_100_cth'];entries[directory]=stat_value(stat.S_IFDIR|0o700,30,nlink=2)
        for i,name in enumerate(names):entries[name]=stat_value(stat.S_IFSOCK|0o700,40+i)
        env['owned'][102]=env['owned'].pop(101);env['owned'][102]['pid']=102;env['owned'][102]['start']='Tue Oct 6 00:00:00 2026';env['process_snapshot'].pop(101);env['process_snapshot'][102]=copy.deepcopy(env['owned'][102])
        for name in names:env['bee_ipc_entry'](VPath(name))
        state=env['bee_ipc']['I'];need(len(state['directories'])==(2 if mode=='second-directory' else 1),'Independent per-directory admission')
        need(all(state['endpoints'][name]['identity']['ino']==40+i for i,name in enumerate(names)),'Second invocation current object identities');return
    raise AssertionError(mode)

def v07_enumeration(mode):
    env,entries,names,events,calls,VPath=bee_fixture();VPath.exists=lambda p:str(p) in entries
    def scandir(path):
        if mode=='absent':raise FileNotFoundError(str(path))
        if mode=='permission':raise PermissionError('fixture denied')
        raise OSError('fixture I/O')
    env['os']=types.SimpleNamespace(scandir=scandir,getuid=lambda:501)
    bind(['scan_tree','size'],env)
    try:env['size'](VPath(names[0]).parent)
    except Exception as error:
        need(isinstance(error,env['BeeObservationIncomplete']) if mode=='absent' else isinstance(error,OSError),'Only ENOENT becomes incomplete observation')
        need(bool(events)==(mode=='absent'),'No partial capacity success; other errors stay hard');return
    raise AssertionError('Partial enumeration accepted')

def v07_capacity(kind):
    limits=copy.deepcopy(D['limits']);tmp=pathlib.Path('/synthetic-tmp');e=pathlib.Path('/synthetic-evidence');p=pathlib.Path('/P');bc=pathlib.Path('/BC');k=pathlib.Path('/K')
    env=bind(['check','resources'],{'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:0),'TMP':tmp,'E':e,'P':p,'BC':bc,'K':k,'D':D,'PREF':{'mechanicalPreparationSeconds':160},'clock_start':0,'evidence_paths':lambda:[],'children':lambda p:[],'size':lambda x:((limits['tmpBytes']+1,1) if kind=='bytes' else (0,limits['tmpLeaves']+1)) if x==tmp else (0,0),'shutil':types.SimpleNamespace(disk_usage=lambda p:types.SimpleNamespace(free=10**12)),'tree_entries':lambda p:{},'as_guard':lambda:None})
    need(rejected(env['resources']),'Original TMP capacity bound still enforced '+kind)

def v07_sdk(mode):
    row={'pid':73931,'ppid':1,'start':'fixed','exe':'adb','stat':'S'};rows={73931:copy.deepcopy(row)};calls=[];events=[]
    binding={'pid':73931,'start':'fixed','exe':'adb','argv':['adb'],'uid':501,'mappedExecutable':{},'logDevice':7,'logInode':9}
    probe={'argv':['/bin/ps','-ww','-p','73931','-o','args='],'returncode':2 if mode=='exit2' else 1,'stdout':'x' if mode=='stdout' else '','stderr':'x' if mode=='stderr' else ''}
    env=bind(['check','sdk_adb_exception','consumer_guard','alive'],{'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:10,mktime=lambda x:0,strptime=lambda *a:None,time=lambda:100),'A':{'sdkAdb':{'path':'/sdk/adb'},'adbException':{'pid':999}},'sdk_adb':None if mode=='unbound' else binding,'root_launch_epoch':0,'baseline_processes':{},'owned':{},'adb_exception':lambda rows:999,'sdk_observations':[],'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'re':re})
    def details(pid):raise env['ChildArgsProbeError'](probe)
    def fresh():
        calls.append('fresh')
        if mode=='fresh-error':raise OSError('fresh failed')
        if mode=='fresh-natural':raise env['NaturalGraceExpired']('fresh boundary')
        if mode in ('present','reused','zombie'):
            return {73931:{**row,'start':'reused' if mode=='reused' else row['start'],'stat':'Z' if mode=='zombie' else 'S'}}
        if mode=='new-consumer':return {700:{**row,'pid':700,'exe':'Unity'}}
        return {}
    env.update(details=details,ps=fresh)
    if mode=='wrong-exe':binding['exe']='other'
    if mode=='absent':
        env['consumer_guard'](rows);need(rows=={} and calls==['fresh'] and not env['owned'] and env['sdk_observations'],'Bound SDK exit changes subsequent snapshot without owned permission')
    else:
        need(rejected(lambda:env['consumer_guard'](rows)),'SDK negative rejected '+mode)
        need(len(calls)==(0 if mode in ('unbound','wrong-exe','stdout','stderr','exit2') else 1),'Only one eligible fresh query')
    report.setdefault('sdkCases',[]).append({'mode':mode,'freshQueries':len(calls),'events':events})

def v07_restore(mode):
    events=[];env=bind(['check','alive','restore_all'],{'time':types.SimpleNamespace(monotonic=lambda:10),'D':{'limits':{'restoreSeconds':60,'finalizationSeconds':30}},'execution_deadline':200,'closure_closed':False,'monitor_errors':[{'error':'historical IPC failure'}],'owned':{},'active':types.SimpleNamespace(poll=lambda:None) if mode=='live-root' else None,'ps':lambda:{},'consumer_guard':lambda rows:None,'restore_compiler':lambda:{'complete':True},'archive_and_restore':lambda emit:{'complete':True},'write':lambda *a:events.append(a),'snapshot_consumers':lambda *a:None})
    if mode=='pending':env['pending_details'][42]={'unverified':True}
    if mode=='consumer':env['consumer_guard']=lambda rows:(_ for _ in ()).throw(RuntimeError('current consumer'))
    if mode=='historical':
        need(env['restore_all']()['complete'] and env['monitor_errors'],'Historical errors retained while fresh safe restoration proceeds')
    else:need(rejected(env['restore_all']) and not events,'Current uncertainty blocks restoration '+mode)

def v07_current_closure():
    clock=[0.];events=[]
    env=bind(['check','alive','monitor','closure'],{'time':types.SimpleNamespace(monotonic=lambda:clock[0],sleep=lambda s:clock.__setitem__(0,clock[0]+s)),'owned':{},'active':None,'ps':lambda:{},'snapshot_consumers':lambda *a:None,'resources':lambda:(_ for _ in ()).throw(RuntimeError('historical and current IPC integrity error')),'projection_guard':lambda:None,'monitor_errors':[],'monitor_cycles':[],'last_monitor':0,'A':{'stopping':{'naturalGraceSeconds':60,'termGraceSeconds':30}},'D':{'limits':{'finalizationSeconds':30}},'execution_deadline':300,'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'utc':lambda:'fixture'})
    env['closure']('I',100,'fixture')
    need(env['closure_closed'] and env['monitor_errors'] and all(x['scope']=='resources' for x in env['monitor_errors']),'Fresh empty consumer proof closes; every integrity failure remains for acceptance')


def v07_preimage():
    env=bind(['check','archive_and_restore'],{'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:10),'active':None,'owned':{},'closure_closed':False,'ps':lambda:{},'consumer_guard':lambda rows:None,'synchronized_paths':['source.cs'],'parked_paths':[],'atomic_conflicts':set(),'P':pathlib.Path('/P'),'E':pathlib.Path('/E'),'N':{'newPaths':[],'overwritten':['source.cs'],'restoreBaseline':{'source.cs':byteid(b'before')},'files':{'source.cs':byteid(b'after')},'allowedNewSettings':{'path':'settings.json'}},'stages':[],'ident':lambda p:byteid(b'foreign'),'projection_guard':lambda:None,'protection':lambda:None,'write':lambda *a:None})
    result=env['archive_and_restore'](False);need(not result['complete'] and result['errors'],'Unknown target preimage blocks actual restore loop')

def v07_gates():
    q=json.loads((R/D['testCases']['path']).read_text());env=bind(['check','validate_xml','compilation_passed'],{'ET':ET,'Counter':Counter,'Q':q,'N':N})
    proof={'assemblies':{n:{'proven':True} for n in N['requiredAssemblies']}};need(len(proof['assemblies'])==12 and env['compilation_passed'](proof),'Exactly12 proven assembly gates')
    for name in N['requiredAssemblies']:
        missing=copy.deepcopy(proof);del missing['assemblies'][name];need(not env['compilation_passed'](missing),'Missing assembly refused')
    root=ET.Element('test-run',result='Passed',total='89',passed='89',failed='0',skipped='0',inconclusive='0')
    for fixture in q['fixtures']:
        for name in fixture['testFullnames']:ET.SubElement(root,'test-case',fullname=name,result='Passed')
    need(env['validate_xml'](ET.tostring(root,encoding='unicode'))['passed']==89,'89 exact testcase gate')
    root.remove(root[-1]);need(rejected(lambda:env['validate_xml'](ET.tostring(root,encoding='unicode'))),'Missing actual testcase refused')

def fix01_bound_zombie(mode):
    calls=[];row={'pid':73931,'start':'fixed','exe':'adb','stat':'Z'}
    if mode=='reused':row.update(start='reused',exe='/other/process')
    rows={} if mode=='absent' else {73931:row}
    def forbidden_probe(*a,**kw):calls.append('probe');raise AssertionError('No probe may override supplied zombie evidence')
    env=bind(['check','alive','sdk_adb_exception','consumer_guard','restore_all'],{'pathlib':pathlib,'time':types.SimpleNamespace(monotonic=lambda:10),'A':{'sdkAdb':{'path':'/sdk/adb'},'adbException':{'pid':999}},'sdk_adb':{'pid':73931,'start':'fixed','exe':'adb'},'owned':{},'details':forbidden_probe,'ps':forbidden_probe,'adb_exception':lambda r:999,'D':{'limits':{'restoreSeconds':60,'finalizationSeconds':30}},'execution_deadline':200,'active':None,'closure_closed':True,'event':lambda *a,**k:None})
    if mode=='absent':env['consumer_guard'](rows)
    else:
        need(rejected(lambda:env['consumer_guard'](rows)),'Bound supplied zombie must reject even reused identity')
        env['ps']=lambda:rows
        env['restore_compiler']=env['archive_and_restore']=forbidden_probe
        need(rejected(env['restore_all']),'Actual restoration entry remains blocked by bound zombie')
    need(not calls and not env['owned'],'No new probe, signal, owned identity, or restore effects')

def fix01_sdk_live():
    argv=['adb','-L','tcp:5037','fork-server','server','--reply-fd','7'];pid=73931;rows={pid:{'pid':pid,'ppid':1,'start':'fixed','exe':'adb','stat':'S'}};log='/TMP/adb.501.log';calls=[]
    class VPath(pathlib.PurePosixPath):
        def resolve(self):return self
        def is_symlink(self):return False
        def lstat(self):
            need(str(self)==log,'Only fixed log metadata');return stat_value(stat.S_IFREG|0o600,20)
        def open(self):return io.StringIO('--- adb starting (pid 73931) ---\nInstalled as /sdk/adb\n10-08 00:00:00\n')
    raw='p73931\nftxt\ntREG\nn/sdk/adb\ni10\nD0x7\nf1\ntREG\nn'+log+'\ni20\nD0x7\nf2\ntREG\nn'+log+'\ni20\nD0x7\n'
    def probe(command,**kw):calls.append(command);return types.SimpleNamespace(returncode=0,stdout=raw,stderr='')
    env=bind(['check','alive','no_links','sdk_adb_exception','consumer_guard'],{'pathlib':types.SimpleNamespace(Path=VPath),'time':types.SimpleNamespace(monotonic=lambda:10,mktime=lambda t:0,strptime=lambda *a:None,time=lambda:100,strftime=lambda *a:'10-08 00:00:00',localtime=lambda t:None),'os':types.SimpleNamespace(getuid=lambda:501),'stat':stat,'re':re,'A':{'sdkAdb':{'path':'/sdk/adb','device':7,'inode':10,**byteid(b'sdk')},'adbException':{'pid':999,'tmpRoot':'/OLDTMP','priorCompilerPipe':'oldpipe'}},'sdk_adb':None,'root_launch_epoch':0,'baseline_processes':{},'owned':{},'sdk_observations':[],'details':lambda p:{'argv':argv,'cwd':'/P'},'cmd':lambda a:'501','ident':lambda p:byteid(b'sdk'),'subprocess':types.SimpleNamespace(run=probe),'TMP':VPath('/TMP'),'P':VPath('/P'),'K':VPath('/K'),'R':VPath('/R'),'ps':lambda:copy.deepcopy(rows),'adb_exception':lambda r:999,'event':lambda *a,**k:None})
    env['consumer_guard'](rows);first=copy.deepcopy(env['sdk_adb']);env['consumer_guard'](rows)
    need(first==env['sdk_adb'] and first['pid']==pid and len(calls)==2 and len(env['sdk_observations'])==2,'New and already-bound ordinary live SDK retain full verification')

def fix01_reappearance(mode):
    env,entries,names,events,calls,VPath=bee_fixture('fd-io' if mode=='primary-error' else 'valid');target=names[0];original=VPath.lstat;count=[0]
    missing={2,3} if mode in ('before','same-inode') else ({3} if mode=='primary-error' else {3,4,5})
    last=max(missing)+1
    def lstat(path):
        if str(path)==target:
            count[0]+=1
            if count[0] in missing:raise FileNotFoundError(target)
            if count[0]>=last:
                value=original(path)
                if mode!='same-inode':value.st_ino=999
                return value
        return original(path)
    VPath.lstat=lstat;caught=None
    try:env['bee_ipc_entry'](VPath(target))
    except Exception as error:caught=error
    need(caught is not None and not env['bee_ipc'],'Present unbound endpoint cannot be accepted as absent')
    record=next(x for x in events if x['kind']=='bee_fd_observation')
    need(record['lifecycle']=='reappeared-unbound' and record['reappearance']['identity']==record['afterLstat'],'Final reappearance identity/time persisted')
    need(record['afterLstat']['ino']==(10 if mode=='same-inode' else 999),'Exact final inode retained')
    if mode=='primary-error':need(type(caught) is OSError and 'synthetic FD read error' in str(caught),'Original I/O failure remains primary')
    else:need(type(caught) is RuntimeError and 'INCOMPLETE' in str(caught),'Bounded reappearance observation fails INCOMPLETE')
    report.setdefault('reappearance',[]).append({'mode':mode,'lstatCalls':count[0],'failureType':type(caught).__name__,'observation':record})

def fix02_priority(mode):
    env,entries,names,events,calls,VPath=bee_fixture('vanish-during' if mode=='normal-disappear' else 'valid');target=names[0];orig=VPath.lstat;lookups=[0];captured=[];values=[]
    reappears=mode in ('grace-reappear','hard-reappear','none-reappear')
    grace=mode.startswith('grace-');hard=mode.startswith('hard-');original_error=PermissionError('original hard FD error')
    def lstat(path):
        if str(path)==target:
            lookups[0]+=1;n=lookups[0]
            if mode=='none-reappear' and n in (2,3):raise FileNotFoundError(target)
            if mode in ('grace-reappear','hard-reappear') and n==3:raise FileNotFoundError(target)
            if mode in ('grace-disappear','hard-disappear') and n>=3:raise FileNotFoundError(target)
            if reappears and n>=4:
                v=orig(path);v.st_ino=999;return v
        return orig(path)
    VPath.lstat=lstat
    if grace:env.update(time=types.SimpleNamespace(monotonic=lambda:58.),probe_deadline=60.,natural_boundary=60.,execution_deadline=300.,work_deadline=120.)
    if hard:
        def probe(*a,**kw):raise original_error
        env['subprocess']=types.SimpleNamespace(run=probe,TimeoutExpired=subprocess.TimeoutExpired)
    def resources():
        try:values.append(env['bee_ipc_entry'](VPath(target)))
        except BaseException as error:captured.append(error);raise
    env.update(snapshot_consumers=lambda *a:None,resources=resources,projection_guard=lambda:None,monitor_errors=[],monitor_cycles=[],last_monitor=0.)
    bind(['monitor'],env);outer=None
    try:env['monitor']('natural-closure',env['process_snapshot'],'I',100,strict=True,force=True)
    except BaseException as error:outer=error
    record=next(x for x in events if x['kind']=='bee_fd_observation');cycle=env['monitor_cycles'][-1]
    if reappears:
        need(len(captured)==1 and type(captured[0]) is RuntimeError and 'INCOMPLETE' in str(captured[0]),'Reappearance always wins as independent hard failure')
        cause=captured[0].__cause__
        if grace:need(isinstance(cause,env['NaturalGraceExpired']) and not calls,'Real blocking-probe grace retained as cause; no FD call started')
        elif hard:need(cause is original_error,'Original hard exception retained as exact cause')
        else:need(cause is None,'No-primary reappearance explicitly fails')
        need(type(outer) is RuntimeError and env['monitor_errors'] and not cycle.get('naturalGraceExpired'),'Actual strict monitor records hard error instead of normal grace transition')
        need(record['lifecycle']=='reappeared-unbound' and record['reappearance']['identity']['ino']==999 and record['error']['type']=='RuntimeError','Observation records reappearance and winning failure')
        if cause is not None:need(record['reappearance']['cause']=={'type':type(cause).__name__,'message':str(cause)},'Original cause also retained in evidence')
    elif grace:
        need(isinstance(outer,env['NaturalGraceExpired']) and isinstance(captured[0],env['NaturalGraceExpired']) and not env['monitor_errors'] and cycle.get('naturalGraceExpired'),'No reappearance retains normal natural grace')
        need('reappearance' not in record,'No fabricated reappearance')
    elif hard:
        need(captured==[original_error] and env['monitor_errors'] and 'reappearance' not in record,'Unrelated hard failure unchanged')
    else:
        need(outer is None and not captured and values[0]['type']=='bee-socket-absent' and not env['monitor_errors'],'Ordinary exact disappearance stays a lifecycle observation')
    need(not env['bee_ipc'],'No incomplete binding accepted')
    report.setdefault('priorityCases',[]).append({'mode':mode,'resourceError':None if not captured else type(captured[0]).__name__,'cause':None if not captured or captured[0].__cause__ is None else type(captured[0].__cause__).__name__,'monitorErrors':copy.deepcopy(env['monitor_errors']),'naturalGraceExpired':cycle.get('naturalGraceExpired',False),'observation':record})

try:
    D=json.loads((E/'inputs.json').read_text());F={n.name:n for n in ast.parse((E/'runner.py').read_text()).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
    compile(ast.parse((E/'runner.py').read_text()),RUNNER_FILENAME,'exec')
    need(identity(E/'inputs.json')==history['inputs'],'Fixed V07 input seal')
    for name,value in history['baseline']['sourceIdentities'].items():need(identity(R/history['baseline']['root']/name)==value,'Immutable V06 '+name)
    env=bind(['basic','normalize_input'],{});N=env['normalize_input'](D)
    sys.setprofile(profile)
    for mode in ('grace-reappear','hard-reappear','none-reappear','grace-disappear','grace-stable','hard-disappear','normal-disappear'):
        case('FIX02 actual monitor priority '+mode,lambda mode=mode:fix02_priority(mode))
    need(not forbidden,'No forbidden operations');report['status']='SOURCE_REPLAY_PASS'
except BaseException as error:
    report['failure']=str(error);report['traceback']=traceback.format_exc()
finally:
    sys.setprofile(None)
    try:cleanup()
    except BaseException as error:conflicts.append('cleanup: '+str(error))
    report['preservedConflicts']=conflicts
    if conflicts:report['status']='SOURCE_REPLAY_FAILED';report['failure']='Unknown or changed fixture contents preserved'
    report['seconds']=time.monotonic()-START;report['actualFunctionsCalled']=sorted(traces);report['forbiddenAttempts']=forbidden
    report['runner']=identity(E/'runner.py');report['checker']=identity(E/'replay-check.py');report['inputs']=identity(E/'inputs.json')
    if report['seconds']>15 or sum(x['seconds'] for x in history['rounds'])+report['seconds']>30:report['status']='SOURCE_REPLAY_FAILED';report['failure']='V07 replay budget exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(x['seconds'] for x in history['rounds'])
    with (E/'replay-results.json').open('w') as stream:json.dump(history,stream,indent=2);stream.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':len(cases),'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'preservedConflicts':conflicts,'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
