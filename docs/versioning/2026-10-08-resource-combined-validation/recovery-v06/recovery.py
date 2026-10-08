#!/usr/bin/env python3
"""One-shot V06 recovery only. Never imports/runs the old runner main."""
import os,sys,json,hashlib,pathlib,stat,subprocess,time,datetime,re,shlex,ctypes,copy,shutil
SOURCE=pathlib.Path(__file__).resolve().parent
PLAN={};R=P=E=K=TMP=None;A={};owned={};pending_details={};adb_observations=[];sdk_observations=[];sdk_adb=None;root_launch_epoch=None;baseline_processes={}
transfer_conflicts=set();atomic_conflicts=set();ROUTES={};deadline=total_deadline=0.;last_guard=0.;in_guard=False;created=False;records=[];moved=[];phase='unstarted';self_identity=None
def probe_timeout():
    remaining=min(deadline,total_deadline)-time.monotonic()
    check(remaining>0,'INCOMPLETE: recovery deadline exhausted')
    return min(5.,remaining)
def blocking_probe_timeout(): return probe_timeout()
def evidence_value(value):
    if isinstance(value,list):return [evidence_value(v) for v in value]
    if isinstance(value,dict):
        result={k:evidence_value(v) for k,v in value.items() if k!='rawFDs'}
        if 'rawFDs' in value:
            raw=value['rawFDs'].encode();result['rawFDIdentity']={'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest(),'unrelatedPathsSaved':False}
        return result
    return value
def output_room(extra):
    total=0
    for base,dirs,files in scan_tree(E):
        for name in checked(dirs+files):
            p=pathlib.Path(base)/name;no_links(p)
            if p.is_file():total+=p.stat().st_size
    check(total+extra<=PLAN['budget']['evidenceBytes'],'Q evidence exceeds100MiB')
def event(kind,**kw):
    if not created:return
    path=E/'events.jsonl';no_links(path)
    payload=(json.dumps(evidence_value({'utc':utc(),'kind':kind,**kw}),ensure_ascii=False)+'\n').encode();output_room(len(payload))
    with path.open('ab') as stream:stream.write(payload);stream.flush()
def write_new(name,value):
    path=E/name;no_links(path);check(not os.path.lexists(path),'Recovery evidence already exists '+name)
    payload=(json.dumps(evidence_value(value),indent=2)+'\n').encode();output_room(len(payload))
    with path.open('xb') as stream:stream.write(payload);stream.flush()
def transfer_slots(): return ROUTES
def rename_exclusive(source,target):
    consumer_tick();native_rename_exclusive(source,target)
def rename_swap(source,target):
    consumer_tick();native_rename_swap(source,target)
def check(ok,why):
    if not ok: raise RuntimeError(why)

def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()

def read_chunks(stream):
    while True:
        probe_timeout(); data=stream.read(1048576); probe_timeout()
        if not data: return
        yield data

def bounded_read(path):
    with path.open('rb') as stream: chunks=list(read_chunks(stream))
    probe_timeout(); data=b''.join(chunks); probe_timeout(); return data

def bounded_text(path,errors='strict'):
    text=bounded_read(path).decode('utf-8',errors=errors); probe_timeout(); return text

def checked(values):
    iterator=iter(values)
    while True:
        probe_timeout()
        try: value=next(iterator)
        except StopIteration: probe_timeout(); return
        probe_timeout(); yield value

def scan_tree(root):
    pending=[pathlib.Path(root)]
    while pending:
        probe_timeout(); base=pending.pop(); check(not base.is_symlink(),'Symlink traversal '+str(base)); dirs=[]; files=[]
        exists=base.exists(); probe_timeout()
        if not exists: continue
        with os.scandir(base) as entries:
            for entry in checked(entries):
                (dirs if entry.is_dir(follow_symlinks=False) else files).append(entry.name); probe_timeout()
        yield str(base),dirs,files
        for name in checked(reversed(dirs)): pending.append(base/name)

def children(root):
    check(root.is_dir(),'Missing directory '+str(root))
    for base,dirs,files in scan_tree(root):
        for name in checked(dirs+files): yield pathlib.Path(base)/name
        return

def ident(p):
    probe_timeout(); p=pathlib.Path(p); before=p.lstat(); probe_timeout()
    check(stat.S_ISREG(before.st_mode) and before.st_nlink==1,'Nonregular or hard-linked file '+str(p))
    fields=('st_dev','st_ino','st_size','st_mtime_ns'); stamp=lambda s:tuple(getattr(s,k) for k in fields); h=hashlib.sha256()
    with p.open('rb') as f:
        check(stamp(os.fstat(f.fileno()))==stamp(before),'Opened identity changed '+str(p))
        for data in read_chunks(f): h.update(data); probe_timeout()
        check(stamp(os.fstat(f.fileno()))==stamp(before),'Read identity changed '+str(p))
    check(stamp(p.lstat())==stamp(before),'Path identity changed '+str(p))
    probe_timeout(); return {'bytes':before.st_size,'sha256':h.hexdigest()}

def no_links(p):
    for q in [pathlib.Path(p)]+list(pathlib.Path(p).parents): check(not q.is_symlink(),'Symlink '+str(q))

def inventory(root,relative=None):
    root=pathlib.Path(root); no_links(root); check(root.is_dir(),'Missing tree '+str(root)); out={}
    for base,dirs,files in scan_tree(root):
        for n in checked(dirs): check(stat.S_ISDIR((pathlib.Path(base)/n).lstat().st_mode),'Non-directory '+str(pathlib.Path(base)/n))
        for n in checked(files):
            p=pathlib.Path(base)/n; out[p.relative_to(relative or root).as_posix()]=ident(p)
    return out

def source_tree(root,blobs=False):
    files={}
    for name in ('Assets','Packages','ProjectSettings'): files.update(inventory(root/name,root))
    if blobs:
        for path,value in checked(files.items()):
            digest=hashlib.sha1(b'blob '+str(value['bytes']).encode()+b'\0'); count=0
            with (root/path).open('rb') as stream:
                for data in read_chunks(stream): count+=len(data); digest.update(data); probe_timeout()
            check(count==value['bytes'],'Blob size changed '+path); value['gitBlob']=digest.hexdigest(); probe_timeout()
    return files

def canonical(files):
    digest=hashlib.sha256()
    for p,v in checked(sorted(files.items())): digest.update(p.encode()+b'\0'+v['gitBlob'].encode()+b'\0'+str(v['bytes']).encode()+b'\n'); probe_timeout()
    return digest.hexdigest()

def cmd(argv):
    p=subprocess.run(argv,capture_output=True,text=True,timeout=blocking_probe_timeout())
    check(p.returncode==0,'Command failed '+json.dumps({'argv':argv,'exit':p.returncode,'stderr':p.stderr[:600]})); return p.stdout

def ps():
    result={}
    for line in cmd(['/bin/ps','-ww','-axo','pid=,ppid=,lstart=,stat=,comm=']).splitlines():
        v=line.split(None,8)
        if len(v)==9: result[int(v[0])]={'ppid':int(v[1]),'start':' '.join(v[2:7]),'stat':v[7],'exe':v[8]}
    return result

def alive(rows,pid):
    return pid in rows and pid in owned and rows[pid]['start']==owned[pid]['start'] and rows[pid]['exe']==owned[pid]['exe'] and not rows[pid]['stat'].startswith('Z')

class ChildArgsProbeError(RuntimeError):
    def __init__(self,probe):
        self.probe=probe
        super().__init__('Single-PID args probe failed '+json.dumps(probe,ensure_ascii=False))

def child_args(pid):
    argv=['/bin/ps','-ww','-p',str(pid),'-o','args=']; started=time.monotonic()
    timeout=blocking_probe_timeout()
    result=subprocess.run(argv,capture_output=True,text=True,timeout=timeout)
    probe={'argv':argv,'returncode':result.returncode,'stdout':result.stdout,'stderr':result.stderr,'startedMonotonic':started,'finishedMonotonic':time.monotonic(),'timeoutSeconds':timeout}
    if result.returncode!=0: raise ChildArgsProbeError(probe)
    return result.stdout

def details(pid):
    args=shlex.split(child_args(pid).strip(),posix=False); redacted=[]; hide=False
    for a in args:
        if hide: redacted.append('<REDACTED>'); hide=False; continue
        sensitive=a.startswith('-') and re.search(r'token|password|secret|serial|credential',a,re.I)
        if sensitive and '=' in a: redacted.append(a.split('=',1)[0]+'=<REDACTED>')
        else: redacted.append(a); hide=bool(sensitive)
    p=subprocess.run(['/usr/sbin/lsof','-a','-p',str(pid),'-d','cwd','-Fn'],capture_output=True,text=True,timeout=blocking_probe_timeout())
    cwd=next((s[1:] for s in p.stdout.splitlines() if s.startswith('n')),None)
    return {'argv':redacted,'cwd':cwd,'cwdProbeExit':p.returncode}

def adb_exception(rows):
    spec=A['adbException']; pid=spec['pid']; row=rows.get(pid); item={'utc':utc(),'pid':pid,'present':row is not None}; adb_observations.append(item)
    log=pathlib.Path(spec['logPath']); no_links(log); ls=log.lstat(); check(stat.S_ISREG(ls.st_mode),'ADB log not regular'); li={'device':ls.st_dev,'inode':ls.st_ino,'uid':ls.st_uid,'gid':ls.st_gid,'mode':oct(stat.S_IMODE(ls.st_mode)),'bytes':ls.st_size,'mtimeNs':ls.st_mtime_ns,'ctimeNs':ls.st_ctime_ns}; item['log']=li
    check({k:li[k] for k in spec['logIdentity']}==spec['logIdentity'],'Original ADB log identity changed')
    if row is None or row['stat'].startswith('Z'): return pid
    item['row']=row; check(row['start']==spec['start'] and row['exe']=='adb','Unknown ADB identity'); item['argv']=details(pid)['argv']; check(item['argv']==spec['argv'],'ADB argv changed')
    probe=subprocess.run(['/usr/sbin/lsof','-nP','-p',str(pid),'-FpcftnDi'],capture_output=True,text=True,timeout=blocking_probe_timeout()); check(probe.returncode==0 and not probe.stderr.strip(),'Incomplete ADB FD probe'); item['rawFDs']=probe.stdout; files=[]; ownerpid=None
    for line in probe.stdout.splitlines():
        if line.startswith('p'): ownerpid=int(line[1:])
        elif line.startswith('f'): files.append({'fd':line[1:]})
        elif files and line[0:1] in 'tniD': files[-1][line[0]]=line[1:]
    check(ownerpid==pid,'ADB FD owner mismatch'); pipes=[spec['priorCompilerPipe']]+[a.split(':',1)[1] for v in owned.values() for a in (v.get('argv') or []) if a.startswith('-pipename:')]
    related=[f for f in files if any(x in f.get('n','') for x in [str(P),str(K),str(TMP),spec['tmpRoot'],*pipes]) or (f.get('n','').startswith(str(R)) and pathlib.Path(f.get('n','')).suffix.lower() in {'.cs','.dll','.asmdef','.asmref','.rsp','.csproj','.sln','.unity','.prefab'})]; item['relatedFDs']=related
    check(len(related)==2 and {f['fd'] for f in related}=={'1','2'} and all(f.get('n')==str(log) and f.get('t')=='REG' and f.get('i')==str(li['inode']) and int(f.get('D','0'),16)==li['device'] for f in related),'ADB association outside exact old-log exception')
    check(not any(any(x in f.get('n','') for x in [str(P),str(K),str(TMP),*pipes]) for f in files),'ADB linked to current compilation'); event('adb_log_exception_checked',observation=item); return pid

def sdk_adb_exception(rows):
    global sdk_adb
    spec=A['sdkAdb']; result=set()
    candidates=[(pid,row) for pid,row in rows.items() if pathlib.Path(row['exe']).name.lower()=='adb' and not row['stat'].startswith('Z') and pid!=A['adbException']['pid']]
    for pid,row in candidates:
        check(root_launch_epoch is not None and pid not in baseline_processes,'ADB not a new process after current Unity launch')
        check(sdk_adb is None or (pid==sdk_adb['pid'] and row['start']==sdk_adb['start']),'More than one current SDK ADB server')
        start=time.mktime(time.strptime(row['start'],'%a %b %d %H:%M:%S %Y')); check(int(root_launch_epoch)<=start<=time.time()+1,'SDK ADB start timeline')
        detail=details(pid); argv=detail['argv']; check(len(argv)==7 and argv[0] in ['adb',spec['path']] and argv[1:6]==['-L','tcp:5037','fork-server','server','--reply-fd'] and re.fullmatch(r'\d+',argv[6]),'SDK ADB exact server argv')
        uid=int(cmd(['/bin/ps','-p',str(pid),'-o','uid=']).strip()); check(uid==os.getuid(),'SDK ADB UID')
        check(ident(spec['path'])=={k:spec[k] for k in ['bytes','sha256']},'Frozen SDK executable bytes')
        check(pathlib.Path(spec['path']).resolve()==pathlib.Path(spec['path']),'Canonical SDK executable')
        probe=subprocess.run(['/usr/sbin/lsof','-nP','-p',str(pid),'-FpcftnDi'],capture_output=True,text=True,timeout=blocking_probe_timeout())
        check(probe.returncode==0 and not probe.stderr.strip(),'Incomplete SDK ADB FD evidence'); files=[]; ownerpid=None
        for line in probe.stdout.splitlines():
            if line.startswith('p'): ownerpid=int(line[1:])
            elif line.startswith('f'): files.append({'fd':line[1:]})
            elif files and line[0:1] in 'tniD': files[-1][line[0]]=line[1:]
        check(ownerpid==pid,'SDK ADB FD owner')
        mapped=[f for f in files if f['fd']=='txt' and f.get('n')==spec['path'] and f.get('t')=='REG']
        check(len(mapped)==1 and mapped[0].get('i')==str(spec['inode']) and int(mapped[0].get('D','0'),16)==spec['device'],'SDK ADB mapped executable inode/device')
        logfile=TMP/('adb.'+str(uid)+'.log'); no_links(logfile); ls=logfile.lstat(); check(stat.S_ISREG(ls.st_mode) and ls.st_uid==uid,'Current ADB regular owned log')
        std=[f for f in files if f['fd'] in ['1','2']]
        check(len(std)==2 and all(f.get('n')==str(logfile) and f.get('t')=='REG' and f.get('i')==str(ls.st_ino) and int(f.get('D','0'),16)==ls.st_dev for f in std),'SDK ADB exact stdout/stderr log')
        with logfile.open() as f: header=''.join(f.readline() for _ in range(5))
        check(header.startswith('--- adb starting (pid '+str(pid)+') ---\n') and 'Installed as '+spec['path'] in header and time.strftime('%m-%d %H:%M:%S',time.localtime(start)) in header,'SDK ADB startup log/PID/time/path')
        pipes=[A['adbException']['priorCompilerPipe']]+[x.split(':',1)[1] for v in owned.values() for x in (v.get('argv') or []) if x.startswith('-pipename:')]
        related=[f for f in files if any(x in f.get('n','') for x in [str(P),str(K),str(TMP),A['adbException']['tmpRoot'],*pipes]) or (f.get('n','').startswith(str(R)) and pathlib.Path(f.get('n','')).suffix.lower() in {'.cs','.dll','.asmdef','.asmref','.rsp','.csproj','.sln','.unity','.prefab'})]
        check(all((f['fd']=='cwd' and f.get('n')==str(P) and f.get('t')=='DIR') or f in std for f in related),'SDK ADB forbidden related source/resource/compiler FD')
        fresh=ps().get(pid); check(fresh is not None and fresh['start']==row['start'] and fresh['exe']==row['exe'] and details(pid)['argv']==argv,'SDK ADB fresh identity')
        binding={'pid':pid,'start':row['start'],'argv':argv,'uid':uid,'mappedExecutable':mapped[0],'logDevice':ls.st_dev,'logInode':ls.st_ino}
        check(sdk_adb is None or sdk_adb==binding,'SDK ADB identity/FD binding changed'); sdk_adb=binding
        item={'utc':utc(),'identity':binding,'relatedFDs':related,'cwd':detail['cwd'],'startupHeader':header,'rootLaunchEpoch':root_launch_epoch,'processStartEpoch':start,'metadataOnlyOutsideCurrentLog':True}
        sdk_observations.append(item); event('sdk_adb_verified',observation=item); result.add(pid)
    return result

def consumer_guard(rows):
    check(not pending_details,'Unverified child identities remain '+str(list(pending_details)))
    probe_timeout(); excluded_adb={adb_exception(rows)}
    probe_timeout(); excluded_adb|=sdk_adb_exception(rows)
    names={'unity','unitypackagemanager','dotnet','csc','mcs','msbuild','bee_backend','unityshadercompiler','unity.licensing.client','beelocalcachetool','adb'}
    other=[dict(r,pid=p) for p,r in rows.items() if pathlib.Path(r['exe']).name.lower() in names and not r['stat'].startswith('Z') and p not in excluded_adb and (not alive(rows,p) or pathlib.Path(r['exe']).name.lower()=='adb')]
    check(not other,'Other potential compiler/Unity consumers '+json.dumps(other)); return []

def native_rename_exclusive(source,target):
    # Darwin SDK sys/stdio.h: RENAME_EXCL = 0x00000004; atomically refuse an existing destination.
    rename=ctypes.CDLL(None,use_errno=True).renamex_np; rename.argtypes=[ctypes.c_char_p,ctypes.c_char_p,ctypes.c_uint]; rename.restype=ctypes.c_int
    if rename(os.fsencode(source),os.fsencode(target),4)!=0:
        code=ctypes.get_errno(); raise OSError(code,os.strerror(code),str(target))

def native_rename_swap(source,target):
    rename=ctypes.CDLL(None,use_errno=True).renamex_np; rename.argtypes=[ctypes.c_char_p,ctypes.c_char_p,ctypes.c_uint]; rename.restype=ctypes.c_int
    if rename(os.fsencode(source),os.fsencode(target),2)!=0:
        code=ctypes.get_errno(); raise OSError(code,os.strerror(code),str(target))

def move_verified(source,target,expected):
    tag=transfer_slots().get((str(source),str(target)))
    check(tag is not None and tag not in transfer_conflicts,'Move outside fixed routes or unresolved transfer '+str(tag))
    capture=E/'atomic/transfer'/tag/'capture'; retired=E/'atomic/transfer'/tag/'retired'
    no_links(capture); no_links(retired)
    check(not os.path.lexists(capture) and not os.path.lexists(retired),'Transfer slots already exist')
    capture.parent.mkdir(parents=True,exist_ok=True)
    check(capture.parent.stat().st_dev==source.parent.stat().st_dev==target.parent.stat().st_dev,'Transfer same-volume requirement')
    marker=('I01 fixed move reservation '+tag+'\n').encode(); marker_id={'bytes':len(marker),'sha256':hashlib.sha256(marker).hexdigest()}
    transfer_conflicts.add(tag)
    with capture.open('xb') as stream: stream.write(marker); stream.flush(); os.fsync(stream.fileno())
    check(ident(capture)==marker_id,'Transfer reservation incomplete')
    def give_back(origin,destination,swap=False):
        try:
            if swap: rename_swap(origin,destination)
            else: rename_exclusive(origin,destination)
            event('transfer_returned',route=tag,source=str(origin),target=str(destination),swap=swap)
        except BaseException as error:
            event('transfer_return_blocked',route=tag,source=str(origin),target=str(destination),error=str(error),retainedSlots=[str(capture),str(retired)])
    rename_swap(capture,source)
    try:
        check(ident(capture)==expected,'Concurrent move source captured '+str(source))
        check(ident(source)==marker_id,'Move reservation replaced '+str(source))
    except BaseException:
        give_back(capture,source,True); raise
    try: rename_exclusive(source,retired)
    except BaseException:
        give_back(capture,source,True); raise
    try: check(ident(retired)==marker_id,'Concurrent source captured during retirement '+str(source))
    except BaseException:
        give_back(retired,source); raise
    try: rename_exclusive(capture,target)
    except BaseException:
        give_back(capture,source); raise
    check(ident(target)==expected and not os.path.lexists(source),'Move postimage or recreated source conflict '+str(source))
    transfer_conflicts.remove(tag); event('transfer_committed',route=tag,source=str(source),target=str(target),retiredReservation=str(retired))

def transfer(source,target,expected,move=False):
    no_links(source); check(ident(source)==expected,'Transfer source drift '+str(source)); no_links(target.parent); target.parent.mkdir(parents=True,exist_ok=True); check(not os.path.lexists(target),'Archive target already exists')
    if move: move_verified(source,target,expected)
    else:
        with target.open('xb') as f: f.write(bounded_read(source))
    check(ident(target)==expected,'Transfer byte verification'); event('transfer',source=str(source),target=str(target),identity=expected,moved=move)

def swap_verified(temporary,target,expected,prior,path,phase):
    rename_swap(temporary,target); atomic_conflicts.add((phase,path)); displaced=None; issues=[]
    try:
        displaced=ident(temporary)
        if displaced==prior: atomic_conflicts.remove((phase,path)); return
        transfer(temporary,E/'atomic/displaced'/phase/path,displaced)
    except BaseException as error: issues.append('displaced retention: '+str(error))
    try:
        rename_swap(temporary,target); returned=ident(temporary)
        if returned!=expected:
            transfer(temporary,E/'atomic/conflict'/phase/path,returned,True); issues.append('New drift during bounded conflict return; preserved in conflict slot')
    except BaseException as error: issues.append('bounded conflict return: '+str(error))
    event('atomic_conflict',phase=phase,path=path,expectedPrior=prior,displaced=displaced,returnAttempts=1,issues=issues)
    raise RuntimeError('Atomic conflict preserved; no further restore of '+phase+'/'+path+'; '+str(issues))

def exact(path):
    path=pathlib.Path(path);no_links(path)
    if not os.path.lexists(path):return None
    s=path.lstat();check(stat.S_ISREG(s.st_mode),'Non-regular recovery leaf '+str(path))
    return ident(path)
def related(name):
    return any(name==root or name.startswith(root+'/') for root in (str(P),PLAN['E6'],str(E)))
def fd_snapshot():
    argv=['/usr/sbin/lsof','-nP','-Fpcftn'];result=subprocess.run(argv,capture_output=True,text=True,timeout=probe_timeout())
    raw=result.stdout;event('recovery_fd_probe',argv=argv,exitCode=result.returncode,stdoutIdentity={'bytes':len(raw.encode()),'sha256':hashlib.sha256(raw.encode()).hexdigest()},stderrIdentity={'bytes':len(result.stderr.encode()),'sha256':hashlib.sha256(result.stderr.encode()).hexdigest()},unrelatedPathsSaved=False)
    check(result.returncode==0 and not result.stderr and len(raw.encode())<=PLAN['budget']['fdOutputBytes'],'INCOMPLETE: global FD evidence incomplete')
    keep=[];pid=None;fd=None;process_count=0
    for line in checked(raw.splitlines()):
        if not line:continue
        tag,value=line[0],line[1:]
        if tag=='p':pid=int(value);fd=None;process_count+=1
        elif tag=='f':check(pid is not None,'FD missing PID');fd={'pid':pid,'fd':value}
        elif tag=='t':check(fd is not None,'FD missing descriptor');fd['type']=value
        elif tag=='n':
            check(fd is not None,'FD missing descriptor')
            if related(value):
                item={**fd,'path':value};keep.append(item)
                # No preflight or evidence file is held open while probing.
                check(pid==os.getpid() and fd['fd']=='cwd' and value==str(R),'Consumer holds recovery target/backup/directory '+json.dumps(item))
        else:check(tag=='c' and pid is not None,'Incomplete global FD record')
    check(process_count>0,'Empty global FD snapshot')
    return {'argv':argv,'exitCode':0,'stderr':'','rawBytes':len(raw.encode()),'rawSha256':hashlib.sha256(raw.encode()).hexdigest(),'relatedRecords':keep,'unrelatedPathsSaved':False,'self':self_identity}
def consumers():
    global in_guard,last_guard,self_identity
    check(not in_guard,'Recursive recovery guard');in_guard=True
    try:
        rows=ps();current=rows.get(os.getpid());check(current is not None,'Recovery process identity absent')
        if self_identity is None:self_identity={'pid':os.getpid(),'start':current['start'],'exe':current['exe']}
        check(current['start']==self_identity['start'] and current['exe']==self_identity['exe'],'Recovery process identity changed')
        old=json.loads(bounded_text(pathlib.Path(PLAN['E6'])/'process-after.json'))
        for identity in old['owned']:
            check(identity['pid'] not in rows,'Old owned PID remains or was reused '+str(identity['pid']))
        consumer_guard(rows);fd=fd_snapshot();last_guard=time.monotonic()
        proc={'utc':utc(),'monotonic':last_guard,'self':self_identity,'rows':rows,'recoveryConsumersClear':True,'adbChecks':adb_observations[-1:],'sdkChecks':sdk_observations[-1:]}
        event('recovery_consumers_clear',processSnapshotSha256=hashlib.sha256(json.dumps(rows,sort_keys=True).encode()).hexdigest(),fd=fd)
        return proc,fd
    finally:in_guard=False
def consumer_tick():
    probe_timeout()
    if not in_guard and time.monotonic()-last_guard>=2:consumers()
def evidence_budget():
    total=0
    for base,dirs,files in scan_tree(E):
        for name in checked(dirs+files):
            path=pathlib.Path(base)/name;no_links(path)
            if path.is_file():total+=path.stat().st_size
    check(total<=PLAN['budget']['evidenceBytes'],'Q evidence exceeds100MiB')
    check(shutil.disk_usage(E).free>=PLAN['budget']['minimumFreeBytes'],'External volume free below4GiB')
def frozen():
    for name,value in checked(PLAN['fixedEvidence'].items()):check(exact(name)==value,'Old evidence changed '+name)
    for item in checked(PLAN['operations']):
        if item['backup']:check(exact(item['backup'])==item['before'],'Original backup changed '+item['path'])
def atomic_inventory():
    root=pathlib.Path(PLAN['E6'])/'atomic';actual={}
    for base,dirs,files in scan_tree(root):
        for name in checked(dirs+files):
            path=pathlib.Path(base)/name;no_links(path)
            if not path.is_dir():actual[str(path)]=exact(path)
    check(actual==PLAN['atomicSlots'],'Unresolved or changed original atomic slots')
def preflight():
    frozen();atomic_inventory();proc,fd=consumers();write_new('process-before.json',proc);write_new('fd-before.json',fd)
    observed={};compiler_bytes=0
    for item in checked(PLAN['operations']):
        current=exact(P/item['path']);observed[item['path']]=current
        if item['kind']=='overwrite':check(current in (item['before'],item['synced']),'Third-party overwritten value '+item['path'])
        elif item['kind']=='new':check(current is None or current==item['synced'],'Third-party new source '+item['path'])
        elif item['kind']=='park':check(current is None or current==item['before'],'Third-party parked source '+item['path'])
        elif current is not None and current!=item['before']:compiler_bytes+=current['bytes']
    check(compiler_bytes<=PLAN['budget']['compilerArchiveBytes'],'Compiler replacement archive exceeds32MiB')
    settings=PLAN['allowedSettings'];value=exact(P/settings['path'])
    check(value is None or value=={k:settings[k] for k in ('bytes','sha256')},'Unapproved natural settings content')
    observed[settings['path']]=value
    estimated=sum(i['before']['bytes'] for i in PLAN['operations'] if i['before'])+sum(2*v['bytes'] for v in observed.values() if v)+1048576
    check(estimated<=PLAN['budget']['evidenceBytes'],'Insufficient fixed evidence allowance')
    event('preflight_complete',observed=observed,compilerArchiveBytes=compiler_bytes,estimatedEvidenceBytes=estimated)
    return observed
def copy_stage(item):
    path=E/'staging'/item['kind']/item['path'];consumer_tick();transfer(pathlib.Path(item['backup']),path,item['before'])
    return path
def install(stage,target,identity):
    consumer_tick();check(exact(stage)==identity and exact(target) is None,'Exclusive install drift')
    target.parent.mkdir(parents=True,exist_ok=True);no_links(target.parent)
    check(stage.parent.stat().st_dev==target.parent.stat().st_dev,'Install volume mismatch')
    rename_exclusive(stage,target);check(exact(target)==identity,'Installed preimage mismatch')
def recover_one(item,prior):
    path=item['path'];target=P/path;kind=item['kind'];consumer_tick()
    check(exact(target)==prior,'Target changed since full preflight '+path)
    if kind!='new' and prior==item['before']:
        records.append({'path':path,'status':'ALREADY_RESTORED'});return
    if kind=='new' and prior is None:
        records.append({'path':path,'status':'ALREADY_ABSENT'});return
    archive=E/'archive'/('compiler' if kind=='compiler' else 'source')/path
    if kind=='overwrite':
        transfer(target,archive,prior);stage=copy_stage(item);consumer_tick()
        check(exact(target)==prior and exact(stage)==item['before'],'Swap preimages changed')
        swap_verified(stage,target,item['before'],prior,path,'recovery')
        check(exact(target)==item['before'] and exact(stage)==prior and exact(archive)==prior,'Swap retained-byte proof')
        moved.extend([{'path':str(stage),'identity':prior},{'path':str(archive),'identity':prior}])
    elif kind=='new':
        archive.parent.mkdir(parents=True,exist_ok=True);transfer(target,archive,prior,True);moved.append({'path':str(archive),'identity':prior})
    else:
        stage=copy_stage(item)
        if prior is not None:
            archive.parent.mkdir(parents=True,exist_ok=True);transfer(target,archive,prior,True);moved.append({'path':str(archive),'identity':prior})
        install(stage,target,item['before'])
    records.append({'path':path,'status':'RESTORED','kind':kind});event('recovered_item',item=records[-1])
    evidence_budget()
def finalize_checks():
    actual=source_tree(P,True);expected=PLAN['projectionBefore']['files']
    check(actual==expected,'P source projection not exact1035 preimage')
    check(len(actual)==1035 and canonical(actual)==PLAN['projectionBefore']['summary']['canonicalSha256'],'P canonical1035 mismatch')
    for item in checked(PLAN['operations']):
        if item['kind']=='compiler':check(exact(P/item['path'])==item['before'],'Compiler preimage mismatch')
    for item in checked(moved):check(exact(item['path'])==item['identity'],'Moved bytes missing or changed')
    check(not atomic_conflicts and not transfer_conflicts,'Unresolved recovery atomic conflict')
    frozen();atomic_inventory();evidence_budget();proc,fd=consumers();write_new('process-after.json',proc);write_new('fd-after.json',fd)
    return {'projectionFiles':1035,'canonicalSha256':canonical(actual),'compilerLeaves':43,'originalPreimagesPreserved':62,'recoveryConsumersClear':True}
def main():
    global PLAN,R,P,E,K,TMP,A,deadline,total_deadline,created,phase,sdk_adb,root_launch_epoch,baseline_processes,ROUTES
    started=time.monotonic();total_deadline=started+120.;deadline=started+30.;result={'task':'RES-COMBINED-V06-RECOVERY','status':'INCOMPLETE','originalV06':'FAILED','nativeRuns':0,'signalCalls':0,'records':records,'movedBytes':moved}
    try:
        check(len(sys.argv)==6,'Usage: recovery.py PLAN_SHA SOURCE_SHA C_ACTUAL REVIEW_HEAD REVIEW_URL')
        plan_sha,source_sha,turn,head,url=sys.argv[1:];check(re.fullmatch(r'[0-9a-f-]{36}',turn) and re.fullmatch(r'[0-9a-f]{40}',head) and url.startswith('https://github.com/yyczz1/FightMatch/'),'Fresh C identity and exact GitHub review required')
        check(ident(SOURCE/'plan.json')['sha256']==plan_sha and ident(pathlib.Path(__file__))['sha256']==source_sha,'Source/plan seal mismatch')
        PLAN=json.loads(bounded_text(SOURCE/'plan.json'));check(PLAN['scriptIdentity']==ident(pathlib.Path(__file__)),'Plan script binding mismatch')
        check(turn!=PLAN['sourceOwner']['turn'] and pathlib.Path(PLAN['Q'])==SOURCE.parent/'recovery-01','Execution identity/Q contract')
        R=pathlib.Path(PLAN['R']);P=pathlib.Path(PLAN['P']);E=pathlib.Path(PLAN['Q']);old=pathlib.Path(PLAN['E6'])
        check(pathlib.Path.cwd()==R,'Recovery cwd must be fixed project root')
        no_links(P);no_links(old);no_links(E.parent);check(not os.path.lexists(E),'Q must be fresh')
        check(P.stat().st_dev==E.parent.stat().st_dev and shutil.disk_usage(E.parent).free>=4294967296,'Recovery volume/free-space gate')
        A=json.loads(bounded_text(old/'activation.json'));K=pathlib.Path(A['cacheRoot']);TMP=pathlib.Path(A['environmentOverrides']['TMPDIR'])
        after=json.loads(bounded_text(old/'process-after.json'));sdk_adb=after['sdkAdbBinding'];baseline_processes={int(k):v for k,v in after['baseline'].items()} if isinstance(after['baseline'],dict) else {v['pid']:v for v in after['baseline']}
        receipt=json.loads(bounded_text(old/'receipt.json'));root_launch_epoch=datetime.datetime.fromisoformat(receipt['stages'][0]['startedUtc']).timestamp()
        E.mkdir(mode=0o700);created=True;write_new('plan.json',PLAN)
        with (E/'recovery.py').open('xb') as stream:stream.write(bounded_read(pathlib.Path(__file__)))
        result['executionOwner']={'thread':PLAN['executorThread'],'host':'local','turn':turn};result['sourceReview']={'head':head,'url':url,'scriptSha256':source_sha,'planSha256':plan_sha}
        for item in PLAN['operations']:
            dest=E/'archive'/('compiler' if item['kind']=='compiler' else 'source')/item['path']
            ROUTES[(str(P/item['path']),str(dest))]='archive/'+item['kind']+'/'+item['path']
            ROUTES[(str(E/'staging'/item['kind']/item['path']),str(E/'atomic/conflict/recovery'/item['path']))]='conflict/'+item['path']
        settings=PLAN['allowedSettings'];ROUTES[(str(P/settings['path']),str(E/'archive/SceneTemplateSettings.json'))]='settings/'+settings['path']
        phase='preflight';observed=preflight();check(time.monotonic()<=started+30 and total_deadline-time.monotonic()>=90,'Insufficient60+30 recovery allowance')
        phase='mutation';deadline=min(time.monotonic()+60,started+90)
        for item in checked(PLAN['operations']):recover_one(item,observed[item['path']])
        if observed[settings['path']] is not None:
            consumer_tick();check(exact(P/settings['path'])==observed[settings['path']],'Settings drift')
            transfer(P/settings['path'],E/'archive/SceneTemplateSettings.json',observed[settings['path']],True);moved.append({'path':str(E/'archive/SceneTemplateSettings.json'),'identity':observed[settings['path']]})
        phase='finalization';deadline=min(time.monotonic()+30,total_deadline);result['verification']=finalize_checks();result['status']='RECOVERED'
    except BaseException as error:
        result['failure']={'phase':phase,'type':type(error).__name__,'message':str(error)}
        if phase!='finalization':deadline=min(time.monotonic()+30,total_deadline);phase='finalization-after-failure'
    result.update(elapsedSecondsThroughChecks=time.monotonic()-started,originalNativeSeconds=117.2632786,transferConflicts=sorted(transfer_conflicts),atomicConflicts=sorted(atomic_conflicts))
    if time.monotonic()>=total_deadline:result['status']='INCOMPLETE';result['budgetExceeded']=True
    try:
        if created:write_new('receipt.json',result)
    except BaseException as error:result['status']='INCOMPLETE';result['receiptFailure']=str(error)
    print(json.dumps({'status':result['status'],'elapsedSecondsThroughReceipt':time.monotonic()-started,'receipt':str(E/'receipt.json') if created else None,'failure':result.get('failure'),'originalV06':'FAILED'}),flush=True)
    return 0 if result['status']=='RECOVERED' and time.monotonic()<total_deadline else 1
if __name__=='__main__':sys.exit(main())
