#!/usr/bin/env python3
"""I01: sealed, single Editor compile; activation is supplied only after source review."""
import os,sys,json,hashlib,pathlib,stat,subprocess,time,datetime,signal,re,shlex,shutil,ctypes
E=pathlib.Path(__file__).parent; M=E; R=E.parents[3]; O=R/'TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03'; P=O/'projection'; K=O/'package-cache'
A={}; B={}; N={}; PREF={}; C={}; TMP=None; OWNER=None; ACT_SHA=''; EXECUTION_TURN=''
LIMIT={'evidenceBytes':33554432,'perLogBytes':8388608,'newCacheBytes':1073741824,'generatedBytes':4294967296,'testTemporaryBytes':16777216,'testTemporaryLeaves':512,'minimumFreeBytes':2147483648,'totalSeconds':660}
allowed=set('activation.json inputs.json before.json preparation.json runner.py replay-check.py replay-results.json process-events.jsonl process-after.json compile.json after.json restore.json receipt.json I/editor.log I/launcher.log I/result.json'.split())
owned={}; stages=[]; active=None; synced=False; restored=False; synchronized_paths=[]; parked_paths=[]; stage_dlls={}; adb_observations=[]; sdk_observations=[]; sdk_adb=None; baseline_processes={}; monitor_errors=[]; monitor_cycles=[]; last_monitor=0.0; root_launch_epoch=None; clock_start=0.0; launch_attempts=0; full_inputs=None
def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def check(ok,why):
    if not ok: raise RuntimeError(why)
def ident(p):
    p=pathlib.Path(p); s=p.lstat(); check(stat.S_ISREG(s.st_mode) and s.st_nlink==1,'Nonregular or hard-linked file '+str(p)); h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(1048576),b''): h.update(b)
    return {'bytes':s.st_size,'sha256':h.hexdigest()}
def write(name,data):
    check(name in allowed and name!='runner.py','Evidence path not allowed '+name)
    p=E/name; p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x') as f: json.dump(data,f,ensure_ascii=False,indent=2); f.write('\n')
def event(kind,**kw):
    with (E/'process-events.jsonl').open('a') as f: f.write(json.dumps({'utc':utc(),'kind':kind,**kw},ensure_ascii=False)+'\n')
def cmd(argv):
    p=subprocess.run(argv,capture_output=True,text=True,timeout=5)
    check(p.returncode==0,'Command failed '+json.dumps({'argv':argv,'exit':p.returncode,'stderr':p.stderr[:600]})); return p.stdout
def ps():
    result={}
    for line in cmd(['/bin/ps','-ww','-axo','pid=,ppid=,lstart=,stat=,comm=']).splitlines():
        v=line.split(None,8)
        if len(v)==9: result[int(v[0])]={'ppid':int(v[1]),'start':' '.join(v[2:7]),'stat':v[7],'exe':v[8]}
    return result
def alive(rows,pid):
    return pid in rows and pid in owned and rows[pid]['start']==owned[pid]['start'] and rows[pid]['exe']==owned[pid]['exe'] and not rows[pid]['stat'].startswith('Z')
def details(pid):
    args=shlex.split(cmd(['/bin/ps','-ww','-p',str(pid),'-o','args=']).strip(),posix=False); redacted=[]; hide=False
    for a in args:
        if hide: redacted.append('<REDACTED>'); hide=False; continue
        sensitive=a.startswith('-') and re.search(r'token|password|secret|serial|credential',a,re.I)
        if sensitive and '=' in a: redacted.append(a.split('=',1)[0]+'=<REDACTED>')
        else: redacted.append(a); hide=bool(sensitive)
    p=subprocess.run(['/usr/sbin/lsof','-a','-p',str(pid),'-d','cwd','-Fn'],capture_output=True,text=True,timeout=5)
    cwd=next((s[1:] for s in p.stdout.splitlines() if s.startswith('n')),None)
    return {'argv':redacted,'cwd':cwd,'cwdProbeExit':p.returncode}
def register(pid,row,stage,rootpid):
    owned[pid]={**row,'argv':None,'cwd':None,'pid':pid,'stage':stage,'rootPid':rootpid,'firstObservedUtc':utc(),'termSent':False}; owned[pid].update(details(pid))
    event('owned_discovered',process=owned[pid])
def discover(rows,stage,rootpid):
    changed=True
    while changed:
        changed=False
        for pid,row in rows.items():
            if pid not in owned and row['ppid'] in owned and alive(rows,row['ppid']):
                try: register(pid,row,stage,rootpid); changed=True
                except Exception as error:
                    raise RuntimeError('Supplied-snapshot child identity failed '+str(pid)+': '+str(error)) from error
def adb_exception(rows):
    spec=A['adbException']; pid=spec['pid']; row=rows.get(pid); item={'utc':utc(),'pid':pid,'present':row is not None}; adb_observations.append(item)
    log=pathlib.Path(spec['logPath']); no_links(log); ls=log.lstat(); check(stat.S_ISREG(ls.st_mode),'ADB log not regular'); li={'device':ls.st_dev,'inode':ls.st_ino,'uid':ls.st_uid,'gid':ls.st_gid,'mode':oct(stat.S_IMODE(ls.st_mode)),'bytes':ls.st_size,'mtimeNs':ls.st_mtime_ns,'ctimeNs':ls.st_ctime_ns}; item['log']=li
    check({k:li[k] for k in spec['logIdentity']}==spec['logIdentity'],'Original ADB log identity changed')
    if row is None or row['stat'].startswith('Z'): return pid
    item['row']=row; check(row['start']==spec['start'] and row['exe']=='adb','Unknown ADB identity'); item['argv']=details(pid)['argv']; check(item['argv']==spec['argv'],'ADB argv changed')
    probe=subprocess.run(['/usr/sbin/lsof','-nP','-p',str(pid),'-FpcftnDi'],capture_output=True,text=True,timeout=5); check(probe.returncode==0 and not probe.stderr.strip(),'Incomplete ADB FD probe'); item['rawFDs']=probe.stdout; files=[]; ownerpid=None
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
        probe=subprocess.run(['/usr/sbin/lsof','-nP','-p',str(pid),'-FpcftnDi'],capture_output=True,text=True,timeout=5)
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
    excluded_adb={adb_exception(rows)}|sdk_adb_exception(rows)
    names={'unity','unitypackagemanager','dotnet','csc','mcs','msbuild','bee_backend','unityshadercompiler','adb'}
    other=[dict(r,pid=p) for p,r in rows.items() if pathlib.Path(r['exe']).name.lower() in names and not r['stat'].startswith('Z') and p not in excluded_adb and (not alive(rows,p) or pathlib.Path(r['exe']).name.lower()=='adb')]
    check(not other,'Other potential compiler/Unity consumers '+json.dumps(other)); return []
def recorded_chain(pid,rootpid):
    seen=set(); p=pid
    while p!=rootpid:
        check(p in owned and p not in seen,'Owned ancestry incomplete'); seen.add(p); p=owned[p]['ppid']
    check(rootpid in owned and owned[rootpid]['argv']==next(s['argv'] for s in A['stages'] if s['id']==owned[rootpid]['stage']) and owned[rootpid]['cwd']==str(P),'Owned root launch mismatch')
def snapshot_consumers(rows,stage,rootpid):
    discover(rows,stage,rootpid)
    for pid in rows:
        if alive(rows,pid):
            check(bool(owned[pid].get('argv')) and owned[pid].get('cwd') is not None and owned[pid].get('cwdProbeExit')==0,'Owned identity incomplete '+str(pid))
            recorded_chain(pid,rootpid)
    return consumer_guard(rows)
def monitor(phase,rows,stage,rootpid,strict=False,force=False):
    global last_monitor
    now=time.monotonic()
    if not force and now-last_monitor<2: return
    cycle={'utc':utc(),'phase':phase,'sincePreviousSeconds':None if not last_monitor else round(now-last_monitor,6),'failures':[]}; last_monitor=now
    for name,fn in [('resources',resources),('projection',projection_guard),('consumers',lambda:snapshot_consumers(rows,stage,rootpid))]:
        try: fn()
        except BaseException as ex:
            item={'utc':utc(),'phase':phase,'scope':name,'error':str(ex)}; monitor_errors.append(item); cycle['failures'].append(item); event('monitor_failure',detail=item)
    cycle['elapsedSeconds']=round(time.monotonic()-now,6); monitor_cycles.append(cycle); event('monitor_cycle',cycle=cycle)
    if strict and cycle['failures']: raise RuntimeError(cycle['failures'][0]['error'])
def closure(stage,rootpid,reason):
    event('closure_begin',stage=stage,reason=reason); deadline=time.monotonic()+A['stopping']['naturalGraceSeconds']
    while True:
        if active is not None: active.poll()
        rows=ps(); discover(rows,stage,rootpid); monitor('natural-closure',rows,stage,rootpid); remaining=[p for p in owned if alive(rows,p)]
        if not remaining or time.monotonic()>=deadline: break
        time.sleep(.25)
    for pid in reversed(remaining):
        try:
            rows=ps()
            if not alive(rows,pid): continue
            check(pathlib.Path(owned[pid]['exe']).name.lower()!='adb','No permission to signal any ADB')
            d=details(pid); check(d['argv']==owned[pid]['argv'] and d['cwd']==owned[pid]['cwd'] and d['cwdProbeExit']==0,'Owned argv changed before TERM '+str(pid)); recorded_chain(pid,rootpid); snapshot_consumers(rows,stage,rootpid)
            check(not owned[pid]['termSent'],'Duplicate TERM refused'); event('signal',signal='SIGTERM',pid=pid,identity=owned[pid],rematched=d)
            try: os.kill(pid,signal.SIGTERM); owned[pid]['termSent']=True
            except ProcessLookupError: event('signal_race_already_exited',pid=pid)
        except BaseException as ex:
            item={'utc':utc(),'phase':'term-authorization','scope':'process','error':str(ex)}; monitor_errors.append(item); event('monitor_failure',detail=item)
    deadline=time.monotonic()+A['stopping']['termGraceSeconds']
    while True:
        if active is not None: active.poll()
        rows=ps(); discover(rows,stage,rootpid); monitor('term-confirmation',rows,stage,rootpid); remaining=[p for p in owned if alive(rows,p)]
        if not remaining or time.monotonic()>=deadline: break
        time.sleep(.25)
    rows=ps(); monitor('closure-final',rows,stage,rootpid,force=True); remaining=[p for p in owned if alive(rows,p)]; event('closure_end',stage=stage,remainingOwned=remaining); check(not remaining,'BLOCKED: owned processes remain '+str(remaining))
def no_links(p):
    for q in [pathlib.Path(p)]+list(pathlib.Path(p).parents): check(not q.is_symlink(),'Symlink '+str(q))
def inventory(root,relative=None):
    root=pathlib.Path(root); no_links(root); check(root.is_dir(),'Missing tree '+str(root)); out={}
    for base,dirs,files in os.walk(root,followlinks=False):
        for n in dirs: check(stat.S_ISDIR((pathlib.Path(base)/n).lstat().st_mode),'Non-directory '+str(pathlib.Path(base)/n))
        for n in files:
            p=pathlib.Path(base)/n; out[p.relative_to(relative or root).as_posix()]=ident(p)
    return out
def same(actual,expected,label):
    missing=sorted(expected.keys()-actual.keys()); added=sorted(actual.keys()-expected.keys()); changed=sorted(k for k in actual.keys()&expected.keys() if actual[k]!=expected[k])
    check(not(missing or added or changed),json.dumps({'scope':label,'missing':missing[:20],'added':added[:20],'changed':changed[:20],'counts':[len(missing),len(added),len(changed)]}))
def old_evidence():
    result={}
    for base,dirs,files in os.walk(O,followlinks=False):
        if pathlib.Path(base)==O: dirs[:]=[name for name in dirs if name not in ('projection','package-cache')]
        for name in files:
            path=pathlib.Path(base)/name; result[str(path.relative_to(O))]=ident(path)
    return result
def tree_entries(root):
    root=pathlib.Path(root); no_links(root); out={}
    for base,dirs,files in os.walk(root,followlinks=False):
        for name in dirs+files:
            p=pathlib.Path(base)/name; s=p.lstat(); rel=p.relative_to(root).as_posix()
            if stat.S_ISLNK(s.st_mode): out[rel]={'type':'symlink','target':os.readlink(p)}
            elif stat.S_ISDIR(s.st_mode): out[rel]={'type':'directory','mode':stat.S_IMODE(s.st_mode)}
            elif stat.S_ISREG(s.st_mode): out[rel]={'type':'file',**ident(p)}
            elif stat.S_ISFIFO(s.st_mode):
                match=re.fullmatch(r'clr-debug-pipe-(\d+)-(\d+)-(in|out)',p.name); check(root==TMP and match and int(match[1]) in owned and pathlib.Path(owned[int(match[1])]['exe']).name=='dotnet' and s.st_uid==os.getuid(),'Unknown FIFO '+str(p)); out[rel]={'type':'fifo','uid':s.st_uid,'gid':s.st_gid,'mode':stat.S_IMODE(s.st_mode),'device':s.st_dev,'inode':s.st_ino,'bytes':s.st_size,'contentsRead':False}
            else: raise RuntimeError('Special tree entry '+str(p))
    return out
def tree_identity(root):
    entries=tree_entries(root); return {'entries':len(entries),'bytes':sum(v.get('bytes',0) for v in entries.values()),'treeSha256':hashlib.sha256(json.dumps(entries,sort_keys=True,separators=(',',':')).encode()).hexdigest()}
def size(root):
    total=0; count=0
    for base,dirs,files in os.walk(root,followlinks=False):
        for n in files:
            try: s=(pathlib.Path(base)/n).lstat(); total+=s.st_size; count+=1
            except FileNotFoundError: pass
    return total,count
def evidence_paths(): return [p for p in E.rglob('*') if p.is_file() or p.is_symlink()]
def evidence_name(p): return str(p.relative_to(E))
def evidence_identity(p): return {'type':'symlink','target':os.readlink(p)} if p.is_symlink() else ident(p)
def dlls(): return {p.name:ident(p) for p in (P/'Library/ScriptAssemblies').glob('*.dll')}
def transfer(source,target,expected,move=False):
    no_links(source); check(ident(source)==expected,'Transfer source drift '+str(source)); no_links(target.parent); target.parent.mkdir(parents=True,exist_ok=True); check(not os.path.lexists(target),'Archive target already exists')
    if move: rename_exclusive(source,target)
    else:
        with target.open('xb') as f: f.write(source.read_bytes())
    check(ident(target)==expected,'Transfer byte verification'); event('transfer',source=str(source),target=str(target),identity=expected,moved=move)
def rename_exclusive(source,target):
    # Darwin SDK sys/stdio.h: RENAME_EXCL = 0x00000004; atomically refuse an existing destination.
    rename=ctypes.CDLL(None,use_errno=True).renamex_np; rename.argtypes=[ctypes.c_char_p,ctypes.c_char_p,ctypes.c_uint]; rename.restype=ctypes.c_int
    if rename(os.fsencode(source),os.fsencode(target),4)!=0:
        code=ctypes.get_errno(); raise OSError(code,os.strerror(code),str(target))
def atomic_write(path,payload,expected,prior,phase):
    paths=N['overwritten']+N['newPaths'] if phase=='sync' else N['overwritten']
    check(phase in ('sync','restore') and path in paths,'Atomic scope'); check(expected==(N['files'] if phase=='sync' else N['restoreBaseline'])[path],'Atomic expected identity')
    check({'bytes':len(payload),'sha256':hashlib.sha256(payload).hexdigest()}==expected,'Atomic payload mismatch')
    target=P/path; temporary=E/'atomic'/phase/path; no_links(target); no_links(temporary); check(not os.path.lexists(temporary),'Atomic leaf already exists')
    if phase=='sync' and path in N['overwritten']: check(ident(E/'restore/source'/path)==N['restoreBaseline'][path],'Verified backup required before sync')
    check((ident(target) if os.path.lexists(target) else None)==prior,'Atomic preimage drift '+path)
    temporary.parent.mkdir(parents=True,exist_ok=True); target.parent.mkdir(parents=True,exist_ok=True); check(temporary.parent.stat().st_dev==target.parent.stat().st_dev,'Atomic same-volume requirement')
    event('atomic_begin',phase=phase,path=path,temporary=str(temporary),expected=expected,prior=prior)
    try:
        with temporary.open('xb') as stream:
            stream.write(payload); stream.flush(); os.fsync(stream.fileno())
        check(ident(temporary)==expected,'Atomic temporary incomplete '+path); check((ident(target) if os.path.lexists(target) else None)==prior,'Atomic preimage changed before commit '+path)
        if prior is None: rename_exclusive(temporary,target)
        else: os.replace(temporary,target)
        check(ident(target)==expected,'Atomic committed bytes '+path); event('atomic_committed',phase=phase,path=path,identity=expected)
    except BaseException as error:
        event('atomic_failed',phase=phase,path=path,temporary=str(temporary),partialRetained=os.path.lexists(temporary),error=str(error)); raise
def basic(files): return {p:{k:v[k] for k in ('bytes','sha256')} for p,v in files.items()}
def source_tree(root,blobs=False):
    files={}
    for name in ('Assets','Packages','ProjectSettings'): files.update(inventory(root/name,root))
    if blobs:
        for path,value in files.items():
            b=(root/path).read_bytes(); value['gitBlob']=hashlib.sha1(b'blob '+str(len(b)).encode()+b'\0'+b).hexdigest()
    return files
def canonical(files):
    return hashlib.sha256(b''.join(p.encode()+b'\0'+v['gitBlob'].encode()+b'\0'+str(v['bytes']).encode()+b'\n' for p,v in sorted(files.items()))).hexdigest()
def host_io_guard():
    actual=tree_entries(P/'TestArtifacts'); same(actual,B['hostArtifacts'],'All old host and isolation trees'); return {'unchanged':True,'newHostRigOrIsolation':0}
def projection_guard():
    actual=source_tree(P); expected=N['files'] if synced and not restored else N['restoreBaseline']; extra=set(actual)-set(expected); settings=N['allowedNewSettings']
    check(extra<={settings['path']} if synced and not restored else not extra,'Unexpected projection additions')
    same({p:v for p,v in actual.items() if p not in extra},expected,'Exact projection inputs')
    if extra: check(actual[settings['path']]==basic({settings['path']:settings})[settings['path']],'Default settings drift')
    guids={}
    for path in actual:
        if path.endswith('.meta'):
            matches=re.findall(r'(?m)^guid:\s*([0-9a-f]{32})\s*$',(P/path).read_text()); check(len(matches)==1 and matches[0] not in guids,'Invalid/duplicate GUID '+path); guids[matches[0]]=path
    check(all(x.name in {'Assets','Packages','ProjectSettings','Library','Temp','Logs','UserSettings','obj','TestArtifacts'} or (x.is_file() and x.suffix in ('.csproj','.sln')) for x in P.iterdir()),'Unexpected projection root')
    return {'leaves':len(actual),'bytes':sum(x['bytes'] for x in actual.values()),'defaultSettings':actual.get(settings['path']),'uniqueGUIDs':len(guids),'hostArtifacts':host_io_guard()}
def temp_snapshot(path):
    p=pathlib.Path(path); no_links(p)
    if not p.exists(): return {'exists':False}
    s=p.stat(); entries=tree_entries(p)
    if path==A['adbException']['tmpRoot']: entries.pop(pathlib.Path(A['adbException']['logPath']).name,None)
    return {'exists':True,'root':{'device':s.st_dev,'inode':s.st_ino,'uid':s.st_uid,'gid':s.st_gid,'mode':stat.S_IMODE(s.st_mode),'birthtime':s.st_birthtime},'entries':entries}
def protection():
    actual={}
    for d in B['sharedRoots']: actual.update(inventory(R/d,R))
    same(actual,B['shared'],'Shared WIP frozen'); same(old_evidence(),B['oldEvidence'],'Original O evidence')
    for path,value in B['frozenTrees'].items(): same(inventory(path),value,'Frozen evidence '+path)
    for path,value in N['fixedReferences'].items(): check(ident(R/path)==value,'Fixed reference drift '+path)
    for path,value in B['oldTemporaryTrees'].items(): check(temp_snapshot(path)==value,'Old TMP drift '+path)
    same(inventory(K),C['files'],'Frozen cache payload'); return {'sharedUnchanged':True,'oldEvidenceUnchanged':True,'cacheUnchanged':True,'oldTemporaryTreesUnchanged':True}
def resources():
    ev=evidence_paths(); eb=sum(p.lstat().st_size for p in ev); cb=size(K)[0]; tb,tc=size(TMP); gb=sum(size(P/n)[0] for n in ['Library','Temp','Logs','UserSettings','obj'])+sum(p.stat().st_size for p in P.iterdir() if p.is_file() and p.suffix in ('.csproj','.sln')); free=shutil.disk_usage(E).free
    dynamic={'archive/source/'+p for p in N['overwritten']+N['newPaths']}|{'restore/source/'+p for p in N['overwritten']}|{'park/source/'+p for p in N['parked']}|{'archive/SceneTemplateSettings.json'}|{'atomic/sync/'+p for p in N['overwritten']+N['newPaths']}|{'atomic/restore/'+p for p in N['overwritten']}
    check(all(evidence_name(p) in allowed|dynamic and not p.is_symlink() for p in ev),'Evidence closure exceeded')
    check(eb<LIMIT['evidenceBytes']-1048576 and all(p.lstat().st_size<LIMIT['perLogBytes'] for p in ev if p.suffix=='.log'),'Evidence/log budget')
    check(cb<=LIMIT['newCacheBytes'] and gb<=LIMIT['generatedBytes'] and tb<=LIMIT['testTemporaryBytes'] and tc<=LIMIT['testTemporaryLeaves'] and free>=LIMIT['minimumFreeBytes'],'Storage budget')
    tree_entries(TMP); check(not any(p.name.startswith('FightMatch-') for p in TMP.iterdir()),'No new save/test roots')
    check(time.monotonic()-clock_start+PREF['mechanicalPreparationSeconds']<=660,'Total mechanical budget')
    return {'evidenceBytes':eb,'cacheBytes':cb,'generatedBytes':gb,'tmpBytes':tb,'tmpLeaves':tc,'freeBytes':free}
def validate_inputs(actual,expected,label): same(basic(actual),basic(expected),label)
def preflight():
    check(ident(E/'activation.json')['sha256']==ACT_SHA and ident(__file__)==A['runner'],'Activation/runner seal')
    for name in ('inputs','preparation','replay-check','replay-results'):
        path=E/(name+('.py' if name=='replay-check' else '.json')); check(ident(path)==A['seals'][name],'Preparation seal '+name)
    check(A['sourceReview']['runner']==A['runner'] and A['sourceReview']['status']=='ACCEPT' and bool(A['sourceReview']['head']) and bool(A['sourceReview']['resultUrl']),'Exact source GitHub review required')
    check(OWNER=={'thread':'01a0fdbc-bf1e-7780-8f7f-dec13d6d590c','host':'local','turn':EXECUTION_TURN} and EXECUTION_TURN!=N['preparationOwner']['turn'],'Fresh actual owner')
    check(A['projectRoot']==str(R) and A['cwd']==str(P) and A['cacheRoot']==str(K),'Fixed execution roots')
    check(A['stopping']=={'naturalGraceSeconds':60,'termGraceSeconds':30,'sigkill':False,'retries':0,'perOwnedPidTermMax':1},'Fixed closure bounds')
    check(A['environmentOverrides']=={'UPM_CACHE_ROOT':str(K),'TMPDIR':str(TMP)},'Only two environment overrides')
    no_links(TMP); s=TMP.stat(); tmp={'device':s.st_dev,'inode':s.st_ino,'uid':s.st_uid,'gid':s.st_gid,'mode':stat.S_IMODE(s.st_mode),'birthtime':s.st_birthtime}
    check(TMP.resolve()==TMP and re.fullmatch(r'/private/tmp/fm-hi01\.[A-Za-z0-9]{8}',str(TMP)) and len(os.fsencode(TMP))<=40 and s.st_uid==os.getuid() and stat.S_IMODE(s.st_mode)==0o700 and tmp==A['tmpIdentity'],'New short TMP binding')
    check(ident(A['editor']['path'])==basic({'editor':N['editor']})['editor'] and A['editor']==N['editor'],'Fixed Intel Editor')
    validate_inputs(source_tree(R),N['shared'],'Actual adopted shared inputs'); consumer_guard(ps())
    check(not os.path.lexists(P/'Temp/UnityLockfile') and not os.path.lexists(R/'Temp/UnityLockfile'),'R/P lock present')
    projection_guard(); protection(); resources()
def capture_before():
    global B
    B={'sharedRoots':['Assets','Packages','ProjectSettings','Config','Tools','Generated','ArtSource'],'shared':{},'restoreBaseline':N['restoreBaseline'],'dlls':dlls(),'compileBindings':{name:compiler_binding(name) for name in N['requiredAssemblies']},'hostArtifacts':tree_entries(P/'TestArtifacts'),'oldEvidence':old_evidence(),'frozenTrees':{p:inventory(p) for p in N['frozenEvidenceTrees']},'oldTemporaryTrees':{p:temp_snapshot(p) for p in N['oldTemporaryRoots']}}
    for d in B['sharedRoots']: B['shared'].update(inventory(R/d,R))
    write('before.json',B)
def synchronize():
    global synced
    consumer_guard(ps()); check(not os.path.lexists(P/'Temp/UnityLockfile') and not os.path.lexists(R/'Temp/UnityLockfile'),'No R/P lock'); validate_inputs(source_tree(R),N['shared'],'Shared synchronization source'); projection_guard()
    for path in N['overwritten']: transfer(P/path,E/'restore/source'/path,N['restoreBaseline'][path])
    for path in N['newPaths']: check(not os.path.lexists(P/path),'New path must be absent '+path)
    for path in N['parked']:
        parked_paths.append(path); transfer(P/path,E/'park/source'/path,N['restoreBaseline'][path],True)
    for path in N['parkDirectories']:
        target=P/path; check(not list(target.iterdir()),'Only empty scoped directory removal'); target.rmdir()
    for path in N['overwritten']+N['newPaths']:
        source=R/path; target=P/path; no_links(source); no_links(target); check(ident(source)==N['files'][path],'Frozen adopted source '+path)
        payload=source.read_bytes(); check({'bytes':len(payload),'sha256':hashlib.sha256(payload).hexdigest()}==N['files'][path],'Source changed during read')
        if path in N['overwritten']: check(ident(target)==N['restoreBaseline'][path],'Concurrent target drift '+path)
        synchronized_paths.append(path); atomic_write(path,payload,N['files'][path],N['restoreBaseline'][path] if path in N['overwritten'] else None,'sync')
        check(ident(target)==N['files'][path],'Synchronized bytes '+path); event('source_synchronized',path=path,identity=N['files'][path])
    synced=True; projection_guard()
def compiler_binding(name):
    paths=list((P/'Library/Bee/artifacts').rglob(name+'.rsp'))
    if len(paths)!=1: return {'complete':False,'reason':'Response file absent or ambiguous'}
    rsp=paths[0]; no_links(rsp); tokens=shlex.split(rsp.read_text()); sources=sorted(t for t in tokens if t.startswith('Assets/') and t.endswith('.cs')); references=[t.split(':',1)[1] for t in tokens if t.startswith(('-r:','/reference:'))]; refs={}
    for ref in references:
        p=pathlib.Path(ref) if os.path.isabs(ref) else P/ref
        if p.is_file(): refs[ref]=ident(p)
    output=rsp.with_suffix('.dll'); dll=P/'Library/ScriptAssemblies'/(name+'.dll')
    return {'complete':bool(sources) and len(refs)==len(set(references)) and output.is_file() and dll.is_file(),'response':{'path':str(rsp.relative_to(P)),**ident(rsp)},'sourceInputs':{p:ident(P/p) for p in sources if (P/p).is_file()},'references':refs,'defines':[t for t in tokens if t.startswith(('-define:','/define:'))],'compilerOutput':ident(output) if output.is_file() else None,'dll':ident(dll) if dll.is_file() else None}
def cached_reuse_proven(before,current,prior):
    keys=('response','sourceInputs','references','defines','compilerOutput','dll')
    return before.get('complete') is True and current.get('complete') is True and prior.get('proven') is True and prior.get('origin') in ('actualCsc','verifiedCacheChain') and all(k in prior and before.get(k)==current.get(k)==prior[k] for k in keys)
def compilation_passed(proof):
    return set(proof.get('assemblies',{}))==set(N['requiredAssemblies']) and all(v.get('proven') is True for v in proof['assemblies'].values())
def compile_evidence(log):
    global full_inputs
    full_inputs=source_tree(P,True); validate_inputs({p:v for p,v in full_inputs.items() if p!=N['allowedNewSettings']['path']},N['files'],'Compiled source input')
    events=[{'line':i+1,'text':x} for i,x in enumerate(log.splitlines()) if re.search(r'Csc |ReloadAssembly|script compilation|Importing|Asset Pipeline Refresh',x,re.I)]; current=dlls(); assemblies={}
    for name in N['requiredAssemblies']:
        matching=[(row,re.search(r'\bCsc\s+(Library/Bee/\S+/'+re.escape(name)+r'\.dll)(?=\s|$)',row['text'])) for row in events]; matching=[(row,m) for row,m in matching if m]
        row={'before':B['dlls'].get(name+'.dll'),'after':current.get(name+'.dll'),'actualCsc':bool(matching),'cacheReuseClaimed':False,'proven':False}
        if len(matching)==1:
            eventrow,match=matching[0]; output=P/match[1]; rsp=output.with_suffix('.rsp'); no_links(rsp)
            if rsp.is_file() and output.is_file() and row['after'] is not None:
                tokens=shlex.split(rsp.read_text()); sources=sorted(t for t in tokens if t.startswith('Assets/') and t.endswith('.cs')); refs=[t.split(':',1)[1] for t in tokens if t.startswith(('-r:','/reference:'))]; reference_ids={}
                for ref in refs:
                    target=pathlib.Path(ref) if os.path.isabs(ref) else P/ref
                    if target.is_file(): reference_ids[ref]=ident(target)
                row.update(event=eventrow,response={'path':str(rsp.relative_to(P)),**ident(rsp)},sourceInputs={p:N['files'][p] for p in sources if p in N['files']},references=reference_ids,defines=[t for t in tokens if t.startswith(('-define:','/define:'))],compilerOutput=ident(output))
                row['proven']=sources==N['assemblySources'][name] and len(reference_ids)==len(set(refs)) and bool(sources)
        currentBinding=compiler_binding(name); prior=N['priorCompileBindings'].get(name,{})
        row['cacheReuseClaimed']=not row['actualCsc'] and cached_reuse_proven(B['compileBindings'][name],currentBinding,prior)
        row['proven']=row['proven'] or row['cacheReuseClaimed']; row['cacheChain']={'before':B['compileBindings'][name],'after':currentBinding,'priorReference':N['priorCompileReference'],'priorBinding':prior,'missingPriorFields':[k for k in ('response','sourceInputs','references','defines','compilerOutput','dll') if k not in prior]}
        assemblies[name]=row
    proof={'events':events,'assemblies':assemblies,'fullInputCanonicalSha256':canonical(full_inputs),'fullInputLeaves':len(full_inputs),'fullInputBytes':sum(v['bytes'] for v in full_inputs.values()),'newerAndroidBuild':N['files']['Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs'],'layoutTests':N['files']['Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs'],'missingEvidenceMeans':'INCOMPLETE; no forced recompile'}
    stage_dlls['I']=proof; return proof
def claim_stage():
    global launch_attempts
    check(launch_attempts==0,'Only one I launch attempt'); launch_attempts+=1
def run_stage():
    global active,root_launch_epoch
    claim_stage(); s=A['stages'][0]; sid='I'; check(not (E/sid).exists(),'I already exists'); (E/sid).mkdir(); start=time.monotonic(); rootpid=None; problem=None
    result={'stage':sid,'status':'NOT_RUN_BLOCKED','runCount':0,'exitCode':None,'argv':s['argv'],'cwd':str(P),'environmentOverrides':A['environmentOverrides'],'startedUtc':utc()}; stages.append(result)
    try:
        check(synced,'Sync required'); preflight(); start=time.monotonic()
        with (E/sid/'launcher.log').open('xb') as stream:
            root_launch_epoch=time.time(); active=subprocess.Popen(s['argv'],cwd=P,env={**os.environ,**A['environmentOverrides']},stdout=stream,stderr=subprocess.STDOUT,start_new_session=True); rootpid=active.pid; result.update(pid=rootpid,runCount=1,status='RUNNING'); rows=ps()
            check(rootpid in rows,'Editor missing at launch'); register(rootpid,rows[rootpid],sid,rootpid); check(owned[rootpid]['exe']==A['editor']['path'] and owned[rootpid]['cwd']==str(P),'Root identity'); event('stage_started',stage=sid,pid=rootpid,argv=s['argv'])
            while active.poll() is None:
                rows=ps(); monitor('running',rows,sid,rootpid,strict=True); check(time.monotonic()-start<360,'I360 exceeded')
                text=(E/sid/'editor.log').read_text(errors='replace') if (E/sid/'editor.log').exists() else ''; log_guard(text); time.sleep(.25)
        result['editorSeconds']=time.monotonic()-start
    except BaseException as ex: problem=str(ex)
    finally:
        if rootpid is not None:
            try: closure(sid,rootpid,problem or 'Editor exited')
            except BaseException as ex: problem=(problem+'; ' if problem else '')+str(ex)
            result['exitCode']=active.poll()
        result['elapsedSecondsIncludingClosure']=time.monotonic()-start; result['finishedUtc']=utc()
    try:
        check(problem is None,problem); check(result['runCount']==1 and result['exitCode']==0 and result.get('editorSeconds',361)<=360,'Editor exit/time '+str(result['exitCode'])); check(not monitor_errors,'Monitor failures')
        log=(E/sid/'editor.log').read_text(errors='replace'); log_guard(log); projection_guard(); protection(); resources(); proof=compile_evidence(log)
        result['status']='COMPILE_PASS' if compilation_passed(proof) else 'INCOMPLETE'
    except BaseException as ex: problem=(problem+'; ' if problem else '')+str(ex); result['status']='FAILED' if rootpid else 'NOT_RUN_BLOCKED'
    result['failure']=problem; write('I/result.json',result); active=None; return result
def log_guard(text):
    bad=[x for x in text.splitlines() if re.search(r'error CS\d+|Compilation failed|Scripts have compiler errors|Failed to load.*assembly|Could not load.*assembly|Aborting batchmode due to failure|(?:Downloading|downloaded|fetching).*https?://|(?:Package Manager|UPM).*(?:unable to|failed to|error).*?(?:resolve|connect|registry|network)|ENOTFOUND|ETIMEDOUT',x,re.I)]
    check(not bad,'Compiler/network failure '+json.dumps(bad[-10:]))
def archive_and_restore():
    global restored
    start=time.monotonic(); rows=ps(); snapshot_consumers(rows,'I',next(iter(owned),None)) if owned else consumer_guard(rows); check(not any(alive(rows,p) for p in owned),'Owned remains; restore forbidden')
    report={'archived':[],'restored':[],'returnedParked':[],'errors':[],'complete':False}; last=start
    def tick():
        nonlocal last
        now=time.monotonic(); check(now-start<=90,'Restore90 exceeded')
        if now-last>=2:
            rows=ps(); consumer_guard(rows); check(not any(alive(rows,p) for p in owned),'Owned during restore'); resources(); last=now
    for path in synchronized_paths:
        try:
            tick(); target=P/path
            if path in N['newPaths'] and not target.exists(): continue
            if path in N['overwritten'] and ident(target)==N['restoreBaseline'][path]: report['restored'].append(path); continue
            check(ident(target)==N['files'][path],'Concurrent/partial value preserved '+path); transfer(target,E/'archive/source'/path,N['files'][path],path in N['newPaths']); report['archived'].append(path)
            if path in N['overwritten']:
                check(ident(E/'restore/source'/path)==N['restoreBaseline'][path],'Backup drift '+path); atomic_write(path,(E/'restore/source'/path).read_bytes(),N['restoreBaseline'][path],N['files'][path],'restore'); check(ident(target)==N['restoreBaseline'][path],'Restored bytes '+path)
            report['restored'].append(path)
        except BaseException as ex: report['errors'].append(str(ex))
    for path in parked_paths:
        try:
            tick(); source=E/'park/source'/path
            if not source.exists(): check(ident(P/path)==N['restoreBaseline'][path],'Incomplete park preserved'); continue
            transfer(source,P/path,N['restoreBaseline'][path],True); report['returnedParked'].append(path)
        except BaseException as ex: report['errors'].append(str(ex))
    try:
        settings=N['allowedNewSettings']; target=P/settings['path']
        if any(s['runCount'] for s in stages) and target.exists() and settings['path'] not in N['restoreBaseline']: transfer(target,E/'archive/SceneTemplateSettings.json',basic({'x':settings})['x'],True)
        tick(); restored=True; report['projection']=projection_guard(); protection(); report['complete']=not report['errors']
    except BaseException as ex: report['errors'].append(str(ex))
    report['seconds']=time.monotonic()-start; write('restore.json',report); return report
def main():
    global A,B,N,PREF,C,TMP,OWNER,ACT_SHA,EXECUTION_TURN,clock_start,baseline_processes
    check(len(sys.argv)==3,'Activation SHA and fresh owner turn required'); ACT_SHA,EXECUTION_TURN=sys.argv[1:]; check(ident(E/'activation.json')['sha256']==ACT_SHA,'Activation SHA')
    A=json.loads((E/'activation.json').read_bytes()); check(A['status']=='EXECUTION_BOUND' and A['task']=='HOST-INTEGRATE-001/I01','I01 bound activation'); OWNER=A['executionOwner']; N=json.loads((E/'inputs.json').read_bytes()); PREF=json.loads((E/'preparation.json').read_bytes())
    check(PREF['status']=='SOURCE_REPLAY_PASS' and PREF['mechanicalPreparationSeconds']<=120,'Preparation gate'); C=json.loads((R/N['cacheManifest']['path']).read_bytes()); check(ident(R/N['cacheManifest']['path'])==basic({'x':N['cacheManifest']})['x'],'Cache manifest seal')
    A['adbException']=N['adbException']; A['sdkAdb']=N['sdkAdb']; TMP=pathlib.Path(A['environmentOverrides']['TMPDIR']); argv=[N['editor']['path'],'-batchmode','-nographics','-quit','-buildTarget','StandaloneOSX','-projectPath',str(P),'-logFile',str(E/'I/editor.log')]
    check(A['argv']==argv,'Exact compile-only argv'); A['stages']=[{'id':'I','timeoutSeconds':360,'maxRuns':1,'argv':argv}]; clock_start=time.monotonic(); started=utc(); failure=None; restore=None; after={}; status='NOT_RUN_BLOCKED'; remaining=None
    try:
        check(sum(bool(x.strip()) for x in pathlib.Path(__file__).read_text().splitlines())<=480,'480 lines'); check(not (E/'process-events.jsonl').exists() and not (E/'before.json').exists(),'Activation already used')
        no_links(TMP); check(not list(TMP.iterdir()),'TMP initially empty'); baseline_processes=ps(); consumer_guard(baseline_processes); event('process_baseline',rows=baseline_processes); capture_before(); preflight(); synchronize(); result=run_stage(); status=result['status']; failure=result['failure']
    except BaseException as ex: failure=str(ex)
    finally:
        for name,fn in [('projection',projection_guard),('protection',protection),('resources',resources)]:
            try: after[name]=fn()
            except BaseException as ex: after[name]={'error':str(ex)}; failure=(failure+'; ' if failure else '')+name+': '+str(ex)
        try:
            check(bool(B),'No initialized baseline; no restore attempted'); restore=archive_and_restore(); check(restore['complete'],'Restore incomplete')
        except BaseException as ex: failure=(failure+'; ' if failure else '')+'restore: '+str(ex)
        try:
            rows=ps(); monitor('final',rows,'I',next(iter(owned),None),force=True); remaining=[dict(owned[p],current=rows[p]) for p in owned if alive(rows,p)]; check(not remaining and not monitor_errors,'Final process/monitor gate'); after['temporaryTree']=tree_entries(TMP)
        except BaseException as ex: failure=(failure+'; ' if failure else '')+str(ex)
        if failure: status='FAILED' if any(s['runCount'] for s in stages) else 'NOT_RUN_BLOCKED'
        write('compile.json',{'stages':stage_dlls,'DLLsAfter':dlls(),'fullImportedInputs':full_inputs,'noRuntimeOrAndroidAcceptance':True})
        write('after.json',after); write('process-after.json',{'processBaseline':baseline_processes,'owned':list(owned.values()),'remainingOwned':remaining,'monitorCycles':monitor_cycles,'monitorErrors':monitor_errors,'adbObservations':adb_observations,'sdkAdbObservations':sdk_observations,'sdkAdbBinding':sdk_adb,'signals':[p for p in owned if owned[p]['termSent']],'sigkill':False})
        receipt={'task':A['task'],'status':status,'owner':OWNER,'startedUtc':started,'finishedUtc':utc(),'mechanicalExecutionSeconds':time.monotonic()-clock_start,'preparationSeconds':PREF['mechanicalPreparationSeconds'],'approvalWait':A['approvalWait'],'stages':stages,'failure':failure,'restore':restore,'remainingOwned':remaining,'newSdkAdbRetained':sdk_adb,'launchAttempts':launch_attempts,'unityStarts':sum(s['runCount'] for s in stages),'evidence':{evidence_name(p):evidence_identity(p) for p in evidence_paths()},'unrun':['Play','tests','scene save/reopen/export','downloads','real saves','Git mutations'],'soleCompletionReceiver':'01a0e401-511d-79f2-b47f-3ab0ade1681b/local'}
        write('receipt.json',receipt); print(json.dumps({'status':status,'failure':failure,'receipt':ident(E/'receipt.json')}),flush=True)
    return 0 if status=='COMPILE_PASS' else 1
if __name__=='__main__': sys.exit(main())
