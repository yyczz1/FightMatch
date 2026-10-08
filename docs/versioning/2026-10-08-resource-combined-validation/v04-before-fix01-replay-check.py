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
    except (RuntimeError,OSError): return True
    return False
def case(name,fn):
    need(time.monotonic()-START<30 and time.monotonic()-START+sum(x['seconds'] for x in history['rounds'])<60,'V04 per-round30 cumulative60'); fn(); cases.append({'name':name,'passed':True})
def bind(names,env):
    defaults={'pending_details':{},'process_snapshot':{},'snapshot_root':None,'closure_closed':None,'transfer_conflicts':set(),'probe_deadline':None,'execution_deadline':None,'work_deadline':None,'restore_deadline':None,'launch_counts':{},'stage_bindings':{},'stage_history':[],'current_stage':'I','D':{},'compiler_parked':[],'compiler_after':{},'compiler_restored':[],'copy':copy,'natural_boundary':None,'bee_ipc':{}}
    for key,value in defaults.items():env.setdefault(key,value)
    if hasattr(env.get('P'),'fs'):env.setdefault('children',lambda path:path.iterdir())
    names=list(names)+[n for n in ('NaturalGraceExpired','probe_timeout','transfer_slots','move_verified','compiler_paths','stage_environment','isolate_stage','checked','read_chunks','bounded_read','bounded_text','scan_tree','children','json_chunks','json_digest','csc_events') if n in F and n not in names and n not in env]
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
need(history['task']=='RES-COMBINED-V04','Current V04 history')
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
        'pathlib':types.SimpleNamespace(Path=VPath),'stat':stat,'re':re,'subprocess':types.SimpleNamespace(run=lsof),'os':types.SimpleNamespace(getuid=lambda:501),'P':VPath('/P'),'TMP':VPath(tmp),'A':{'stages':[{'id':sid,'argv':argv} for sid,argv in commands.items()]},
        'D':{'beeIpcContract':{'tools':tools,'directoryPattern':'[A-Za-z0-9]{8}[.][A-Za-z0-9]{3}','fdTimeoutSeconds':5},'commands':commands},'owned':owned,'process_snapshot':rows,'snapshot_root':100,'ident':identity,'cmd':command,'details':detail,'event':lambda kind,**kw:events.append({'kind':kind,**kw}),'time':types.SimpleNamespace(monotonic=lambda:0.),
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
try:
    D=json.loads((E/'inputs.json').read_text());F={n.name:n for n in ast.parse((E/'runner.py').read_text()).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
    compile(ast.parse((E/'runner.py').read_text()),RUNNER_FILENAME,'exec')
    need(identity(E/'inputs.json')==history['inputs'],'Exact V04 input seal')
    for name,value in history['baseline']['sourceIdentities'].items():need(identity(R/history['baseline']['root']/name)==value,'Immutable V03 '+name)
    need(identity(R/history['reused177']['path'])=={k:history['reused177'][k] for k in ('bytes','sha256')},'Historical177 exact bytes')
    env=bind(['basic','normalize_input'],{});N=env['normalize_input'](D)
    sys.setprofile(profile)
    case('V04 Bee htc then cth',lambda:bee_order())
    case('V04 Bee cth then htc',lambda:bee_order(True))
    case('V04 Bee complete FD path with stream annotation',lambda:bee_order(suffix=True))
    for mode in ('stage','pid','start','fresh-start','exe','parent','argv','cwd','tool-editor','tool-beeBackend','tool-beeDriver','tool-lsof','fd-mismatch','fd-pid','fd-type','fd-no-record','fd-error','uid','mode','dir-uid','dir-mode','dir-link','fifo','link','regular','nlink','vanish-first','vanish-during','before-io','post-inode','post-dir','reuse-during','fd-io'):
        case('V04 Bee rejects '+mode,lambda mode=mode:bee_bad(mode))
    def fd_timeout():
        env,entries,names,events,calls,VPath=bee_fixture('fd-timeout')
        try:env['bee_ipc_entry'](VPath(names[0]))
        except subprocess.TimeoutExpired:return
        raise AssertionError('Real in-flight TimeoutExpired must propagate')
    case('V04 Bee real in-flight TimeoutExpired',fd_timeout)
    for mode in ('absent','directory-absent','residual','closed-I-new-T','inode','dir-inode','uid','link','reused-pid','io','reappear','wrong-stage'):
        case('V04 Bee admitted '+mode,lambda mode=mode:bee_after(mode))
    for kind in ('socket','fifo'):case('V04 unknown '+kind+' stays forbidden',lambda kind=kind:bee_unknown(kind))
    case('V04 one directory per stage',bee_second_directory)
    case('V04 rejects malformed IPC paths and wrong PID',bee_path_rejections)
    case('V04 fresh T uses separate stage FD state',bee_fresh_T)
    for mode in ('natural','timeout','io','total','unclosed-prior'):case('V04 closure '+mode,lambda mode=mode:boundary(mode))
    case('V04 work and total deadlines preserved',probe_budgets)
    case('V04 fresh sealed state accepted',lambda:state_before(False))
    case('V04 execution state drift rejected',lambda:state_before(True))
    case('Retained actual 43-path compiler park and recovery ledger',compiler_ledger)
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
    if report['seconds']>30 or sum(x['seconds'] for x in history['rounds'])+report['seconds']>60:report['status']='SOURCE_REPLAY_FAILED';report['failure']='V04 replay budget exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(x['seconds'] for x in history['rounds'])
    with (E/'replay-results.json').open('w') as stream:json.dump(history,stream,indent=2);stream.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':len(cases),'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'preservedConflicts':conflicts,'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
