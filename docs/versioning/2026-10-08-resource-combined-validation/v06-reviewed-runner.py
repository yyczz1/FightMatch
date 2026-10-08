#!/usr/bin/env python3
"""RES-COMBINED-V06: sealed I then T; source preparation never activates native work."""
import os,sys,json,hashlib,pathlib,stat,subprocess,time,datetime,signal,re,shlex,shutil,ctypes,copy
from collections import Counter
import xml.etree.ElementTree as ET
E=pathlib.Path(__file__).parent; M=E; R=E.parents[3]; O=R/'TestArtifacts/FightMatch/RES-01A/P01'; P=O/'projection'; K=O/'package-cache'
A={}; B={}; N={}; PREF={}; C={}; TMP=None; OWNER=None; ACT_SHA=''; EXECUTION_TURN=''
LIMIT={'evidenceBytes':33554432,'perLogBytes':8388608,'newCacheBytes':1073741824,'generatedBytes':4294967296,'testTemporaryBytes':16777216,'testTemporaryLeaves':512,'minimumFreeBytes':2147483648,'totalSeconds':660}
allowed=set('activation.json inputs.json before.json preparation.json runner.py replay-check.py replay-results.json process-events.jsonl process-after.json compile.json after.json restore.json receipt.json I/editor.log I/launcher.log I/result.json'.split())
owned={}; stages=[]; active=None; synced=False; restored=False; synchronized_paths=[]; parked_paths=[]; stage_dlls={}; adb_observations=[]; sdk_observations=[]; sdk_adb=None; baseline_processes={}; monitor_errors=[]; monitor_cycles=[]; last_monitor=0.0; root_launch_epoch=None; clock_start=0.0; launch_attempts=0; full_inputs=None; atomic_conflicts=set(); launched_root=None
pending_details={}; process_snapshot={}; snapshot_root=None; closure_closed=None
transfer_conflicts=set(); probe_deadline=None; natural_boundary=None; bee_ipc={}; bee_observation_bytes=0
D={}; Q={}; BC=E.parent/'bee-cache'; ASROOT=R/'TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V06/state-tests'
compiler_parked=[]; compiler_after={}; compiler_restored=[]; stage_history=[]; launch_counts={}; stage_bindings={}; current_stage='I'; restore_deadline=None; execution_deadline=None; work_deadline=None
def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def check(ok,why):
    if not ok: raise RuntimeError(why)
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
def write(name,data):
    check(name in allowed and name!='runner.py','Evidence path not allowed '+name)
    p=E/name; p.parent.mkdir(parents=True,exist_ok=True)
    probe_timeout()
    with p.open('x') as f:
        for part in json_chunks(data,ensure_ascii=False,indent=2): f.write(part); probe_timeout()
        f.write('\n'); probe_timeout()
    probe_timeout()
def event(kind,**kw):
    with (E/'process-events.jsonl').open('a') as f: f.write(json.dumps({'utc':utc(),'kind':kind,**kw},ensure_ascii=False)+'\n')
class NaturalGraceExpired(Exception):
    """The normal natural-grace boundary, never an I/O or total-budget failure."""
def bee_stamp(s):
    return {k:getattr(s,'st_'+k) for k in ('uid','gid','mode','dev','ino','nlink')}
def bee_tools():
    for role,item in D['beeIpcContract']['tools'].items():
        no_links(pathlib.Path(item['path'])); check(ident(item['path'])=={'bytes':item['bytes'],'sha256':item['sha256']},'Bee fixed tool drift '+role)
def bee_process(pid,stage,rootpid):
    check(alive(process_snapshot,pid),'Bee PID/start/executable changed '+str(pid))
    owner=owned[pid]; check(owner['stage']==stage and owner['rootPid']==rootpid and owner['ppid']==process_snapshot[pid]['ppid'],'Bee stage/parent mismatch')
    recorded_chain(pid,rootpid)
    expected=D['beeIpcContract']['tools']['editor' if pid==rootpid else 'beeBackend']['path']
    check(owner['exe']==expected,'Bee executable mismatch')
    if pid==rootpid: check(owner['argv']==D['commands'][stage],'Bee Editor argv mismatch')
    else: check('--ipc' in owner['argv'],'Bee backend lacks --ipc')
    line=cmd(['/bin/ps','-ww','-p',str(pid),'-o','pid=,ppid=,lstart=,stat=,comm=']).strip().split(None,8)
    check(len(line)==9 and int(line[0])==pid and int(line[1])==owner['ppid'] and ' '.join(line[2:7])==owner['start'] and line[8]==owner['exe'] and not line[7].startswith('Z'),'Bee fresh process identity mismatch')
    detail=details(pid)
    check(detail['argv']==owner['argv'] and detail['cwd']==owner['cwd']==str(P) and detail['cwdProbeExit']==0,'Bee fresh argv/cwd mismatch')
    return {'pid':pid,'start':owner['start'],'exe':owner['exe'],'argv':owner['argv'],'cwd':owner['cwd'],'ppid':owner['ppid'],'stage':stage}
def bee_fd_bound(text,pid,path,matches=None):
    spellings=[(str(path),'standard')]
    target=pathlib.Path(path); endpoint=re.fullmatch(r'ipc_(\d+)_(htc|cth)',target.name)
    if TMP is not None and target.parent.parent==TMP and re.fullmatch(D['beeIpcContract']['directoryPattern'],target.parent.name) and endpoint and int(endpoint[1])==snapshot_root:
        spellings.append((str(TMP)+'//'+target.parent.name+'/'+target.name,'extra-tmp-separator'))
    current=None; fd=None; found=False; saw_pid=False
    for line in text.splitlines():
        if not line: continue
        tag,value=line[0],line[1:]
        if tag=='p': check(value==str(pid) and not saw_pid,'Bee FD PID mismatch'); current=pid; saw_pid=True; fd=None
        elif tag=='f': check(current==pid,'Bee FD missing process'); fd={'number':value,'type':None}
        elif tag=='t': check(fd is not None,'Bee FD missing descriptor'); fd['type']=value
        elif tag=='n':
            check(fd is not None,'Bee FD missing descriptor')
            for spelling,label in spellings:
                if re.fullmatch(re.escape(spelling)+r'(?: type=STREAM(?: \(LISTEN\))?)?',value):
                    check(fd['type']=='unix' and re.fullmatch(r'\d+[a-zA-Z]*',fd['number']),'Bee FD wrong type'); found=True
                    if matches is not None: matches.append({'spelling':label,'rawName':value,'fd':fd['number']})
        else: check(tag=='c' and current==pid,'Bee unexpected FD record')
    check(saw_pid,'Bee FD missing PID'); return found
def bee_ipc_entry(path,observed=None):
    path=pathlib.Path(path); known=None; stage=current_stage
    for sid,state in bee_ipc.items():
        if str(path) in state['endpoints']: known=state['endpoints'][str(path)]; stage=sid; break
    match=re.fullmatch(r'ipc_(\d+)_(htc|cth)',path.name)
    check(TMP is not None and path.parent.parent==TMP and re.fullmatch(D['beeIpcContract']['directoryPattern'],path.parent.name) and match,'Unknown Bee IPC path '+str(path))
    pid=int(match[1]); state=bee_ipc.get(stage)
    if known is not None:
        check(state['rootPid']==pid and (stage==current_stage or state['closed']),'Bee endpoint stage mismatch')
        for identity in [state['rootIdentity']]+known['fdOwners']:
            row=process_snapshot.get(identity['pid'])
            check(row is None or (row['start']==identity['start'] and row['exe']==identity['exe']),'Bee admitted PID reused')
        try: ds=path.parent.lstat()
        except FileNotFoundError: ds=None
        if ds is not None: check(bee_stamp(ds)==state['directoryIdentity'],'Bee directory replaced')
        try: current=path.lstat()
        except FileNotFoundError:
            if not known['absent']: known['absent']=True; event('bee_ipc_absent',stage=stage,path=str(path),identity=known['identity'])
            return {'type':'bee-socket-absent','stage':stage,'identity':known['identity'],'contentsRead':False}
        check(not known['absent'] and ds is not None and bee_stamp(current)==known['identity'],'Bee endpoint replaced or reappeared')
        return {'type':'bee-socket','stage':stage,**known['identity'],'contentsRead':False}
    check(stage in ('I','T') and pid==snapshot_root and alive(process_snapshot,pid),'Bee first binding lacks current Editor')
    check(state is None or (not state['closed'] and state['rootPid']==pid and state['directory']==str(path.parent)),'Bee directory/stage changed')
    no_links(path.parent); ds=path.parent.lstat()
    check(stat.S_ISDIR(ds.st_mode) and ds.st_uid==os.getuid() and not stat.S_IMODE(ds.st_mode)&0o022,'Bee directory type/UID/mode')
    if state is not None: check(bee_stamp(ds)==state['directoryIdentity'] and len(state['endpoints'])<2,'Bee directory replaced or endpoint limit')
    try: before=path.lstat()
    except FileNotFoundError: raise RuntimeError('INCOMPLETE: Bee endpoint vanished before FD binding')
    check(stat.S_ISSOCK(before.st_mode) and before.st_uid==os.getuid() and before.st_nlink==1 and not stat.S_IMODE(before.st_mode)&0o022,'Bee endpoint type/UID/mode/link')
    if observed is not None: check(bee_stamp(observed)==bee_stamp(before),'Bee endpoint changed before binding')
    root_identity,holders,after=bee_fd_binding(path,stage,pid,ds,before)
    if state is None:
        state={'stage':stage,'rootPid':pid,'rootIdentity':root_identity,'directory':str(path.parent),'directoryIdentity':bee_stamp(ds),'endpoints':{},'closed':False}; bee_ipc[stage]=state
    state['endpoints'][str(path)]={'identity':bee_stamp(after),'fdOwners':holders,'absent':False}
    event('bee_ipc_admitted',stage=stage,path=str(path),directory=state['directoryIdentity'],identity=bee_stamp(after),fdOwners=holders)
    return {'type':'bee-socket','stage':stage,**bee_stamp(after),'contentsRead':False}
def bee_ipc_snapshot(root,out):
    if root!=TMP: return
    for state in bee_ipc.values():
        for name in state['endpoints']:
            path=pathlib.Path(name); key=path.relative_to(root).as_posix()
            if key not in out: out[key]=bee_ipc_entry(path)

def probe_timeout():
    now=time.monotonic()
    check(execution_deadline is None or now<execution_deadline,'Total mechanical deadline exhausted')
    deadline=work_deadline if probe_deadline is None else probe_deadline
    if natural_boundary is not None and deadline==natural_boundary and now>=natural_boundary: raise NaturalGraceExpired('Natural grace elapsed')
    remaining=5. if deadline is None else deadline-now
    if execution_deadline is not None: remaining=min(remaining,execution_deadline-now)
    check(remaining>0,'Closure probe deadline exhausted')
    return min(5.,remaining)
def checked(values):
    iterator=iter(values)
    while True:
        probe_timeout()
        try: value=next(iterator)
        except StopIteration: probe_timeout(); return
        probe_timeout(); yield value
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
def json_chunks(value,**options):
    for part in checked(json.JSONEncoder(**options).iterencode(value)):
        for offset in checked(range(0,len(part),65536)): yield part[offset:offset+65536]
def json_digest(value):
    digest=hashlib.sha256()
    for part in json_chunks(value,sort_keys=True,separators=(',',':')): digest.update(part.encode()); probe_timeout()
    return digest.hexdigest()
def blocking_probe_timeout():
    remaining=probe_timeout()
    if natural_boundary is not None and probe_deadline==natural_boundary:
        now=time.monotonic()
        check(execution_deadline is None or now<execution_deadline,'Total mechanical deadline exhausted')
        if natural_boundary-now<5.: raise NaturalGraceExpired('normal-natural-boundary: insufficient full five-second probe allowance')
        return 5.
    return remaining
def bee_capture_output(record,stdout,stderr):
    complete=True; limit=D['beeObservationContract']['rawBytesPerStream']
    for key,value in (('rawStdout',stdout),('rawStderr',stderr)):
        value='' if value is None else value; raw=value if isinstance(value,bytes) else value.encode('utf-8')
        record[key]=raw[:limit].decode('utf-8',errors='replace'); record[key+'Bytes']=len(raw)
        if isinstance(value,bytes): record[key+'Hex']=raw[:limit].hex()
        if len(raw)>limit: record[key+'Truncated']=True; complete=False
    return complete
def bee_observation_event(record):
    global bee_observation_bytes
    payload={'utc':utc(),'kind':'bee_fd_observation',**record}; encoded=(json.dumps(payload,ensure_ascii=False)+'\n').encode('utf-8')
    limits=D['beeObservationContract']
    check(len(encoded)<=limits['recordBytes'] and bee_observation_bytes+len(encoded)<=limits['totalBytes'],'INCOMPLETE: Bee observation capacity exceeded')
    try: previous=(E/'process-events.jsonl').lstat().st_size
    except FileNotFoundError: previous=0
    check(previous+len(encoded)<D['evidenceSlots']['maxBytes']-D['evidenceSlots']['reservedFinalReceiptBytes'],'INCOMPLETE: process-events capacity exceeded')
    event('bee_fd_observation',**record); bee_observation_bytes+=len(encoded)
def bee_fd_binding(path,stage,pid,ds,before):
    record={'stage':stage,'monotonic':time.monotonic(),'targetPath':str(path),'rootPid':pid,'beforeLstat':bee_stamp(before),'directoryBefore':bee_stamp(ds),'candidates':[],'probes':[]}
    primary=None; root_identity=None; holders=[]; after=None
    try:
        bee_tools(); root_identity=bee_process(pid,stage,pid); record['rootIdentity']=root_identity
        candidates=[pid]+[p for p in owned if p!=pid and alive(process_snapshot,p) and owned[p]['exe']==D['beeIpcContract']['tools']['beeBackend']['path'] and owned[p]['stage']==stage and owned[p]['rootPid']==pid]
        record['candidates']=[{'pid':p,'ownedIdentity':dict(owned[p])} for p in candidates]
        for candidate in candidates:
            item={'candidatePid':candidate,'monotonic':time.monotonic(),'targetPath':str(path),'stage':stage,'exitCode':None,'parseResult':None,'identityRecheck':None}; record['probes'].append(item)
            probe_error=None
            try:
                item['beforeLstat']=bee_stamp(path.lstat()); identity=bee_process(candidate,stage,pid); item['identityBefore']=identity
                argv=[D['beeIpcContract']['tools']['lsof']['path'],'-a','-p',str(candidate),'-U','-Fpcftn']; item['argv']=argv
                timeout=min(D['beeIpcContract']['fdTimeoutSeconds'],blocking_probe_timeout()); item['timeoutSeconds']=timeout
                result=subprocess.run(argv,capture_output=True,text=True,timeout=timeout)
                item['exitCode']=result.returncode; complete=bee_capture_output(item,result.stdout,result.stderr)
                check(complete,'INCOMPLETE: Bee raw FD output exceeds bound')
                check((result.returncode==0 or (result.returncode==1 and not result.stdout)) and not result.stderr.strip(),'Bee FD probe failed')
                item['matchedSpellings']=[]; bound=result.returncode==0 and bee_fd_bound(result.stdout,candidate,path,item['matchedSpellings']); item['parseResult']=bound
                if bound: holders.append(identity)
                refreshed=bee_process(candidate,stage,pid); item['identityRecheck']=refreshed
                check(refreshed==identity,'Bee process changed during FD probe')
            except BaseException as error:
                probe_error=error; item['error']={'type':type(error).__name__,'message':str(error)}
                if isinstance(error,subprocess.TimeoutExpired) and not bee_capture_output(item,error.output,error.stderr):
                    item['capacityFailure']={'status':'INCOMPLETE','rawBytesPerStream':D['beeObservationContract']['rawBytesPerStream'],'stdoutBytes':item['rawStdoutBytes'],'stderrBytes':item['rawStderrBytes']}
                    raise RuntimeError('INCOMPLETE: Bee timeout partial FD output exceeds bound; TimeoutExpired: '+str(error)) from error
                raise
            finally:
                try: item['afterLstat']=bee_stamp(path.lstat())
                except FileNotFoundError: item['afterLstat']={'ENOENT':True}
                except BaseException as error:
                    item['afterLstat']={'errorType':type(error).__name__,'error':str(error)}
                    if probe_error is None: raise
                item['finishedMonotonic']=time.monotonic()
        check(holders,'INCOMPLETE: Bee endpoint lacks exact owned FD binding')
        try: after=path.lstat()
        except FileNotFoundError: raise RuntimeError('INCOMPLETE: Bee endpoint vanished during FD binding')
        check(bee_stamp(after)==bee_stamp(before) and bee_stamp(path.parent.lstat())==bee_stamp(ds),'Bee endpoint/directory changed during FD binding')
        bee_tools()
    except BaseException as error:
        primary=error; record['error']={'type':type(error).__name__,'message':str(error)}
    finally:
        try: record['afterLstat']=bee_stamp(path.lstat())
        except FileNotFoundError: record['afterLstat']={'ENOENT':True}
        except BaseException as error:
            record['afterLstat']={'errorType':type(error).__name__,'error':str(error)}
            if primary is None: primary=error; record['error']={'type':type(error).__name__,'message':str(error)}
        record['finishedMonotonic']=time.monotonic(); record['fdOwners']=holders
        try: bee_observation_event(record)
        except BaseException as error:
            if primary is None: primary=RuntimeError('INCOMPLETE: Bee observation write failed: '+str(error))
            elif isinstance(primary,NaturalGraceExpired):
                failure=RuntimeError('INCOMPLETE: Bee observation write failed after natural boundary: '+str(error))
                failure.__cause__=primary; failure.__context__=error; primary=failure
            else: primary.add_note('INCOMPLETE: Bee observation write failed: '+str(error))
    if primary is not None: raise primary
    return root_identity,holders,after

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
def register(pid,row,stage,rootpid,detail=None):
    detail=details(pid) if detail is None else detail; check(bool(detail.get('argv')) and detail.get('cwd') is not None and detail.get('cwdProbeExit')==0,'Owned identity incomplete '+str(pid)); check(pid not in owned,'Existing owned entry preserved '+str(pid))
    if pathlib.Path(row['exe']).name=='BeeLocalCacheTool':
        expected=pathlib.Path(N['editor']['path']).parent.parent/'Tools/BuildPipeline/BeeLocalCacheTool'
        check(row['exe']==str(expected),'Unbound BeeLocalCacheTool executable')
        probe=subprocess.run(['/usr/sbin/lsof','-nP','-p',str(pid),'-Fn'],capture_output=True,text=True,timeout=blocking_probe_timeout())
        names=[line[1:] for line in probe.stdout.splitlines() if line.startswith('n')]
        check(probe.returncode==0 and not probe.stderr.strip() and any(name==str(BC) or name.startswith(str(BC)+'/') for name in names),'BeeLocalCacheTool lacks current BC evidence')
        detail={**detail,'beeCacheEvidence':{'root':str(BC),'fds':names}}
    owned[pid]={**row,**detail,'pid':pid,'stage':stage,'rootPid':rootpid,'firstObservedUtc':utc(),'termSent':False}
    event('owned_discovered',process=owned[pid])
def discover(rows,stage,rootpid):
    for pid in list(pending_details):
        if pid not in rows or rows[pid]['stat'].startswith('Z'):
            event('pending_resolved',pid=pid,resolution='absent' if pid not in rows else 'zombie',identity=pending_details.pop(pid))
    changed=True; attempted=set()
    while changed:
        changed=False
        for pid,row in rows.items():
            if pid in owned or pid in attempted or row['stat'].startswith('Z'): continue
            prior=pending_details.get(pid)
            if prior and any(row[k]!=prior['row'][k] for k in ('ppid','start','exe')): continue
            if row['ppid'] not in owned or not alive(rows,row['ppid']): continue
            attempted.add(pid)
            try:
                register(pid,row,stage,rootpid); changed=True
                if pid in pending_details: event('pending_resolved',pid=pid,resolution='complete-identity',identity=pending_details.pop(pid))
            except NaturalGraceExpired: raise
            except Exception as error:
                message='Supplied-snapshot child identity failed '+str(pid)+': '+str(error)
                if prior is None: pending_details[pid]={'row':dict(row),'stage':stage,'rootPid':rootpid,'firstError':message,'firstObservedUtc':utc()}
                pending_details[pid]['lastError']=message
                if isinstance(error,ChildArgsProbeError): pending_details[pid]['argsProbe']=error.probe
                event('pending_identity',pid=pid,identity=pending_details[pid])
                probe=error.probe if isinstance(error,ChildArgsProbeError) else None
                if prior is None and probe is not None and probe['argv']==['/bin/ps','-ww','-p',str(pid),'-o','args='] and probe['returncode']==1 and probe['stdout']=='' and probe['stderr']=='':
                    parent=row['ppid']; recorded_chain(parent,rootpid)
                    check(owned[parent]['stage']==stage and owned[parent]['rootPid']==rootpid,'Transient child parent stage mismatch')
                    chain=[]; ancestor=parent
                    while True:
                        chain.append(dict(owned[ancestor]))
                        if ancestor==rootpid: break
                        ancestor=owned[ancestor]['ppid']
                    confirmation={'pid':pid,'initialRow':dict(row),'parentChain':chain,'argsProbe':probe,'stage':stage,'rootPid':rootpid,'startedMonotonic':time.monotonic()}
                    try:
                        fresh=ps()
                    except BaseException as fresh_error:
                        confirmation['failure']={'type':type(fresh_error).__name__,'message':str(fresh_error)}
                        pending_details[pid]['freshConfirmation']=confirmation; event('transient_child_confirmation_failed',confirmation=confirmation)
                        if isinstance(fresh_error,NaturalGraceExpired): raise RuntimeError('INCOMPLETE: transient child fresh confirmation unavailable after args failure') from fresh_error
                        raise
                    confirmation.update(finishedMonotonic=time.monotonic(),freshSnapshot=fresh,pidAbsent=pid not in fresh)
                    pending_details[pid]['freshConfirmation']=confirmation
                    rows.clear(); rows.update(fresh)
                    if pid not in fresh:
                        event('transient-child-exited',confirmation=confirmation); pending_details.pop(pid); changed=True
                    else:
                        event('transient_child_confirmation_rejected',confirmation=confirmation)
                    break
    check(not pending_details,next((p['firstError'] for p in pending_details.values()),'Unverified child remains'))
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
def recorded_chain(pid,rootpid):
    seen=set(); p=pid
    while p!=rootpid:
        check(p in owned and p not in seen,'Owned ancestry incomplete'); seen.add(p); p=owned[p]['ppid']
    check(rootpid in owned and owned[rootpid]['argv']==next(s['argv'] for s in A['stages'] if s['id']==owned[rootpid]['stage']) and owned[rootpid]['cwd']==str(P),'Owned root launch mismatch')
def recover_root(rows,stage,rootpid):
    if rootpid in owned or active is None or active.pid!=rootpid or active.poll() is not None: return
    row=rows.get(rootpid); check(launched_root is not None and launched_root['pid']==rootpid and launched_root['stage']==stage and row is not None,'Launched root remains unverified')
    check(all(row[k]==launched_root[k] for k in ('start','exe','ppid')) and not row['stat'].startswith('Z') and row['exe']==A['editor']['path'] and row['ppid']==os.getpid(),'Launched root snapshot identity mismatch')
    detail=details(rootpid)
    if active.poll() is not None: return
    check(detail['argv']==next(s['argv'] for s in A['stages'] if s['id']==stage) and detail['cwd']==str(P),'Launched root argv/cwd mismatch')
    register(rootpid,row,stage,rootpid,detail)
def snapshot_consumers(rows,stage,rootpid):
    global process_snapshot,snapshot_root
    process_snapshot=rows; snapshot_root=rootpid
    recover_root(rows,stage,rootpid); discover(rows,stage,rootpid)
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
    for name,fn in [('consumers',lambda:snapshot_consumers(rows,stage,rootpid)),('resources',resources),('projection',projection_guard)]:
        try: probe_timeout(); fn()
        except NaturalGraceExpired:
            cycle['naturalGraceExpired']=True; break
        except BaseException as ex:
            item={'utc':utc(),'phase':phase,'scope':name,'error':str(ex)}; monitor_errors.append(item); cycle['failures'].append(item); event('monitor_failure',detail=item)
    cycle['elapsedSeconds']=round(time.monotonic()-now,6); monitor_cycles.append(cycle); event('monitor_cycle',cycle=cycle)
    if cycle.get('naturalGraceExpired'): raise NaturalGraceExpired('Natural grace elapsed')
    if strict and cycle['failures']: raise RuntimeError(cycle['failures'][0]['error'])
    return cycle
def closure(stage,rootpid,reason):
    global closure_closed,probe_deadline,natural_boundary
    previous_deadline=probe_deadline; previous_natural=natural_boundary; closure_closed=False; event('closure_begin',stage=stage,reason=reason)
    def sample(phase,force=False):
        try:
            probe_timeout(); rows=ps()
            probe_timeout(); cycle=monitor(phase,rows,stage,rootpid,force=force)
            probe_timeout(); remaining=[p for p in owned if alive(rows,p)]
            clear=cycle is not None and not cycle['failures'] and not remaining and not pending_details and (active is None or active.poll() is not None)
            return remaining,clear
        except NaturalGraceExpired: raise
        except BaseException as error:
            item={'utc':utc(),'phase':phase,'scope':'process-snapshot','error':str(error)}
            monitor_errors.append(item); event('monitor_failure',detail=item)
            monitor_cycles.append({'utc':utc(),'phase':phase,'failures':[item],'clear':False})
            return list(owned),False
    try:
        natural_end=time.monotonic()+A['stopping']['naturalGraceSeconds']; deadline=natural_end; deadline=min(deadline,execution_deadline-D.get('limits',{}).get('finalizationSeconds',0)-60-A['stopping']['termGraceSeconds']) if execution_deadline is not None else deadline; probe_deadline=deadline; natural_boundary=natural_end if deadline==natural_end else None; first=True; remaining=list(owned)
        while first or time.monotonic()<deadline:
            if active is not None: active.poll()
            try: remaining,clear=sample('natural-closure',force=first)
            except NaturalGraceExpired:
                remaining=list(owned); event('natural_grace_elapsed',stage=stage); break
            first=False
            if clear or time.monotonic()>=deadline: break
            time.sleep(max(0.,min(.25,deadline-time.monotonic())))
        natural_boundary=None
        deadline=time.monotonic()+A['stopping']['termGraceSeconds']; deadline=min(deadline,execution_deadline-D.get('limits',{}).get('finalizationSeconds',0)-60) if execution_deadline is not None else deadline; probe_deadline=deadline
        event('term_window',stage=stage,deadline=deadline)
        first=True; attempted_terms=set()
        while first or time.monotonic()<deadline:
            if active is not None: active.poll()
            try:
                probe_timeout(); rows=ps()
                probe_timeout(); snapshot_consumers(rows,stage,rootpid)
                candidates=[p for p in reversed(list(owned)) if alive(rows,p) and p not in attempted_terms and not owned[p]['termSent']]
                if candidates:
                    pid=candidates[0]; attempted_terms.add(pid)
                    check(pathlib.Path(owned[pid]['exe']).name.lower()!='adb','No permission to signal any ADB')
                    probe_timeout(); d=details(pid)
                    check(d['argv']==owned[pid]['argv'] and d['cwd']==owned[pid]['cwd'] and d['cwdProbeExit']==0,'Owned argv changed before TERM '+str(pid)); recorded_chain(pid,rootpid)
                    probe_timeout(); snapshot_consumers(rows,stage,rootpid)
                    check(not owned[pid]['termSent'],'Duplicate TERM refused')
                    probe_timeout(); event('signal',signal='SIGTERM',pid=pid,identity=owned[pid],rematched=d)
                    try:
                        probe_timeout(); os.kill(pid,signal.SIGTERM); owned[pid]['termSent']=True
                    except ProcessLookupError: event('signal_race_already_exited',pid=pid)
                    continue
            except BaseException as ex:
                item={'utc':utc(),'phase':'term-authorization','scope':'process','error':str(ex)}; monitor_errors.append(item); event('monitor_failure',detail=item)
            remaining,clear=sample('term-confirmation',force=first); first=False
            if clear or time.monotonic()>=deadline: break
            time.sleep(max(0.,min(.25,deadline-time.monotonic())))
        remaining,closure_closed=sample('closure-final',force=True)
        event('closure_end',stage=stage,remainingOwned=remaining,pendingIdentities=list(pending_details),closed=closure_closed,popenStillLive=active is not None and active.poll() is None)
        check(closure_closed,'BLOCKED: process closure lacks a successful clear sample '+str({'owned':remaining,'pending':list(pending_details),'root':rootpid}))
        if stage in bee_ipc: bee_ipc[stage]['closed']=True
    finally: probe_deadline=previous_deadline; natural_boundary=previous_natural
def no_links(p):
    for q in [pathlib.Path(p)]+list(pathlib.Path(p).parents): check(not q.is_symlink(),'Symlink '+str(q))
def inventory(root,relative=None):
    root=pathlib.Path(root); no_links(root); check(root.is_dir(),'Missing tree '+str(root)); out={}
    for base,dirs,files in scan_tree(root):
        for n in checked(dirs): check(stat.S_ISDIR((pathlib.Path(base)/n).lstat().st_mode),'Non-directory '+str(pathlib.Path(base)/n))
        for n in checked(files):
            p=pathlib.Path(base)/n; out[p.relative_to(relative or root).as_posix()]=ident(p)
    return out
def same(actual,expected,label):
    missing=sorted(expected.keys()-actual.keys()); added=sorted(actual.keys()-expected.keys()); changed=sorted(k for k in actual.keys()&expected.keys() if actual[k]!=expected[k])
    check(not(missing or added or changed),json.dumps({'scope':label,'missing':missing[:20],'added':added[:20],'changed':changed[:20],'counts':[len(missing),len(added),len(changed)]}))
def old_evidence():
    result={}
    for base,dirs,files in scan_tree(O):
        if pathlib.Path(base)==O: dirs[:]=[name for name in dirs if name not in ('projection','package-cache')]
        for name in checked(files):
            path=pathlib.Path(base)/name; result[str(path.relative_to(O))]=ident(path)
    return result
def tree_entries(root):
    root=pathlib.Path(root); no_links(root); out={}
    for base,dirs,files in scan_tree(root):
        for name in checked(dirs+files):
            p=pathlib.Path(base)/name; rel=p.relative_to(root).as_posix()
            if root==TMP and (p.name.startswith('ipc_') or any(str(p) in v['endpoints'] for v in bee_ipc.values())):
                out[rel]=bee_ipc_entry(p); continue
            s=p.lstat()
            if stat.S_ISLNK(s.st_mode): out[rel]={'type':'symlink','target':os.readlink(p)}
            elif stat.S_ISDIR(s.st_mode): out[rel]={'type':'directory','mode':stat.S_IMODE(s.st_mode)}
            elif stat.S_ISREG(s.st_mode): out[rel]={'type':'file',**ident(p)}
            elif stat.S_ISFIFO(s.st_mode):
                match=re.fullmatch(r'clr-debug-pipe-(\d+)-(\d+)-(in|out)',p.name)
                check(root==TMP and match and s.st_uid==os.getuid(),'Unknown FIFO '+str(p))
                pid=int(match[1]); check(alive(process_snapshot,pid),'FIFO owner absent or reused '+str(p))
                owner=owned[pid]; licensing=str(pathlib.Path(A['editor']['path']).parent.parent/'Frameworks/UnityLicensingClient.app/Contents/MacOS/Unity.Licensing.Client')
                check(pathlib.Path(owner['exe']).name=='dotnet' or owner['exe']==licensing,'Unknown FIFO executable '+str(p))
                check(owner['rootPid']==snapshot_root and process_snapshot[pid]['ppid']==owner['ppid'],'FIFO root/parent mismatch '+str(p))
                recorded_chain(pid,snapshot_root)
                out[rel]={'type':'fifo','uid':s.st_uid,'gid':s.st_gid,'mode':stat.S_IMODE(s.st_mode),'device':s.st_dev,'inode':s.st_ino,'bytes':s.st_size,'contentsRead':False}
            else: raise RuntimeError('Special tree entry '+str(p))
    bee_ipc_snapshot(root,out)
    return out
def tree_identity(root):
    entries=tree_entries(root); return {'entries':len(entries),'bytes':sum(v.get('bytes',0) for v in entries.values()),'treeSha256':json_digest(entries)}
def size(root):
    total=0; count=0
    for base,dirs,files in scan_tree(root):
        for n in checked(files):
            try: s=(pathlib.Path(base)/n).lstat(); total+=s.st_size; count+=1
            except FileNotFoundError: pass
    return total,count
def evidence_paths():
    result=[]
    for base,dirs,files in scan_tree(E):
        for name in checked(dirs+files):
            p=pathlib.Path(base)/name
            if p.is_file() or p.is_symlink(): result.append(p)
    return result
def evidence_name(p): return str(p.relative_to(E))
def evidence_identity(p): return {'type':'symlink','target':os.readlink(p)} if p.is_symlink() else ident(p)
def dlls(): return {p.name:ident(p) for p in children(P/'Library/ScriptAssemblies') if p.name.endswith('.dll')}
def transfer_slots():
    routes={}
    for path in N['parked']:
        routes[(str(P/path),str(E/'park/source'/path))]='park/'+path
        routes[(str(E/'park/source'/path),str(P/path))]='unpark/'+path
    for path in N['overwritten']+N['newPaths']: routes[(str(P/path),str(E/'archive/source'/path))]='archive/'+path
    routes[(str(P/N['allowedNewSettings']['path']),str(E/'archive/SceneTemplateSettings.json'))]='settings/'+N['allowedNewSettings']['path']
    for phase in ('sync','restore'):
        for path in N['overwritten']: routes[(str(E/'atomic'/phase/path),str(E/'atomic/conflict'/phase/path))]='conflict-'+phase+'/'+path
    for path in compiler_paths():
        routes[(str(P/path),str(E/'park/compiler'/path))]='compiler-park/'+path
        routes[(str(E/'park/compiler'/path),str(P/path))]='compiler-unpark/'+path
        routes[(str(P/path),str(E/'archive/compiler'/path))]='compiler-archive/'+path
    return routes
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
def rename_exclusive(source,target):
    # Darwin SDK sys/stdio.h: RENAME_EXCL = 0x00000004; atomically refuse an existing destination.
    rename=ctypes.CDLL(None,use_errno=True).renamex_np; rename.argtypes=[ctypes.c_char_p,ctypes.c_char_p,ctypes.c_uint]; rename.restype=ctypes.c_int
    if rename(os.fsencode(source),os.fsencode(target),4)!=0:
        code=ctypes.get_errno(); raise OSError(code,os.strerror(code),str(target))
def rename_swap(source,target):
    rename=ctypes.CDLL(None,use_errno=True).renamex_np; rename.argtypes=[ctypes.c_char_p,ctypes.c_char_p,ctypes.c_uint]; rename.restype=ctypes.c_int
    if rename(os.fsencode(source),os.fsencode(target),2)!=0:
        code=ctypes.get_errno(); raise OSError(code,os.strerror(code),str(target))
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
        else: swap_verified(temporary,target,expected,prior,path,phase)
        check(ident(target)==expected,'Atomic committed bytes '+path); event('atomic_committed',phase=phase,path=path,identity=expected)
    except BaseException as error:
        event('atomic_failed',phase=phase,path=path,temporary=str(temporary),partialRetained=os.path.lexists(temporary),error=str(error)); raise
def basic(files): return {p:{k:v[k] for k in ('bytes','sha256')} for p,v in files.items()}
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
def host_io_guard():
    actual=tree_entries(P/'TestArtifacts'); same(actual,B['hostArtifacts'],'All old host and isolation trees'); return {'unchanged':True,'newHostRigOrIsolation':0}
def projection_guard():
    actual=source_tree(P); expected=N['files'] if synced and not restored else N['restoreBaseline']; extra=set(actual)-set(expected); settings=N['allowedNewSettings']
    check(extra<={settings['path']} if synced and not restored else not extra,'Unexpected projection additions')
    same({p:v for p,v in actual.items() if p not in extra},expected,'Exact projection inputs')
    if extra: check(actual[settings['path']]==basic({settings['path']:settings})[settings['path']],'Default settings drift')
    guids={}
    for path in checked(actual):
        if path.endswith('.meta'):
            matches=re.findall(r'(?m)^guid:\s*([0-9a-f]{32})\s*$',bounded_text(P/path)); check(len(matches)==1 and matches[0] not in guids,'Invalid/duplicate GUID '+path); guids[matches[0]]=path
    check(all(x.name in {'Assets','Packages','ProjectSettings','Library','Temp','Logs','UserSettings','obj','TestArtifacts'} or (x.is_file() and x.suffix in ('.csproj','.sln')) for x in children(P)),'Unexpected projection root')
    return {'leaves':len(actual),'bytes':sum(x['bytes'] for x in actual.values()),'defaultSettings':actual.get(settings['path']),'uniqueGUIDs':len(guids),'hostArtifacts':host_io_guard()}
def temp_snapshot(path):
    p=pathlib.Path(path); no_links(p)
    if not p.exists(): return {'exists':False}
    s=p.stat(); entries=tree_entries(p)
    if path==A['adbException']['tmpRoot']: entries.pop(pathlib.Path(A['adbException']['logPath']).name,None)
    return {'exists':True,'root':{'device':s.st_dev,'inode':s.st_ino,'uid':s.st_uid,'gid':s.st_gid,'mode':stat.S_IMODE(s.st_mode),'birthtime':s.st_birthtime},'entries':entries}
def protection():
    actual={}
    for directory in B['sharedRoots']: actual.update(inventory(R/directory,R))
    same(actual,B['shared'],'Shared protected inputs'); same(old_evidence(),B['oldEvidence'],'P01 historical evidence')
    for path,expected in checked(D['fixedReferences'].items()): check(ident(R/path)==expected,'Fixed reference drift '+path)
    check(tree_entries(P/'TestArtifacts')==B['hostArtifacts'],'P old TestArtifacts changed')
    cache_guard()
    for path,identity in B['compilerTools'].items(): check(ident(path)==identity,'Compiler tool drift')
    return {'sharedUnchanged':True,'oldEvidenceUnchanged':True,'cacheFrozen':True}
def resources():
    ev=evidence_paths(); eb=sum(p.lstat().st_size for p in checked(ev)); tb,tc=size(TMP); bb,bc=size(BC)
    generated=sum(size(P/n)[0] for n in ['Library','Temp','Logs','UserSettings','obj'])+sum(p.stat().st_size for p in children(P) if p.is_file() and p.suffix in ('.csproj','.sln'))
    check(all(evidence_name(p) in allowed|dynamic_evidence() and not p.is_symlink() for p in checked(ev)),'Evidence closure exceeded')
    check(eb<D['evidenceSlots']['maxBytes']-D['evidenceSlots']['reservedFinalReceiptBytes'],'Evidence budget')
    check(all(p.lstat().st_size<=D['evidenceSlots']['perLogBytes'] for p in checked(ev) if p.suffix=='.log'),'Log budget')
    check(not (E/'T/results.xml').exists() or (E/'T/results.xml').stat().st_size<=D['evidenceSlots']['xmlBytes'],'XML budget')
    limits=D['limits']; check(tb<=limits['tmpBytes'] and tc<=limits['tmpLeaves'],'TMP budget')
    check(bb<=limits['newBeeCacheBytes'] and bc<=limits['newBeeCacheLeaves'],'Bee cache budget')
    check(size(K)[0]<=limits['upmBytes'] and generated<=limits['generatedBytes'],'Cache/generated budget')
    check(shutil.disk_usage(E).free>=limits['freeExternalBytesMin'],'External free-space budget')
    check(time.monotonic()-clock_start+PREF['mechanicalPreparationSeconds']<limits['totalMechanicalSeconds'],'Total900 including preparation exceeded')
    tree_entries(TMP); as_guard()
    check(not any(v['type']=='symlink' for v in tree_entries(BC).values()),'Bee cache link')
    return {'evidenceBytes':eb,'tmpBytes':tb,'tmpLeaves':tc,'beeBytes':bb,'beeLeaves':bc,'generatedBytes':generated}
def validate_inputs(actual,expected,label): same(basic(actual),basic(expected),label)
def preflight():
    check(ident(E/'activation.json')['sha256']==ACT_SHA and ident(__file__)==A['runner'],'Activation/runner seal')
    for name in ('inputs','preparation','replay-check','replay-results'):
        path=E/(name+('.py' if name=='replay-check' else '.json')); check(ident(path)==A['seals'][name],'Preparation seal '+name)
    check(A['sourceReview']['runner']==A['runner'] and A['sourceReview']['status']=='ACCEPT' and bool(A['sourceReview']['head']) and bool(A['sourceReview']['resultUrl']),'Exact source review required')
    check(OWNER=={'thread':'01a0fdbc-bf1e-7780-8f7f-dec13d6d590c','host':'local','turn':EXECUTION_TURN} and EXECUTION_TURN!=PREF['owner']['turn'],'Fresh C actual')
    check(A['projectRoot']==str(R) and A['cwd']==str(P) and A['cacheRoot']==str(K),'Fixed execution roots')
    check(A['stopping']=={'naturalGraceSeconds':60,'termGraceSeconds':30,'sigkill':False,'retries':0,'perOwnedPidTermMax':1},'Closure contract')
    check(A['environmentOverrides']=={'UPM_CACHE_ROOT':str(K),'TMPDIR':str(TMP),'BEE_CACHE_DIRECTORY':str(BC),'DOTNET_EnableDiagnostics':'0'},'Exact shared environment')
    check(A['stages']==[{'id':sid,'timeoutSeconds':D['limits'][sid+'Seconds'],'maxRuns':1,'argv':D['commands'][sid]} for sid in ('I','T')],'Exact ordered I/T')
    bee_tools()
    no_links(TMP); s=TMP.stat()
    check(TMP.resolve()==TMP and re.fullmatch(r'/private/tmp/fm-rcv6\.[A-Za-z0-9]{8}',str(TMP)) and len(os.fsencode(TMP))<=40 and s.st_uid==os.getuid() and stat.S_IMODE(s.st_mode)==0o700,'New short TMP')
    check({k:getattr(s,'st_'+k) for k in ('dev','ino','uid','gid')}==A['tmpIdentity'],'Sealed TMP identity')
    check(ident(N['editor']['path'])==basic({'x':N['editor']})['x'] and A['editor']==N['editor'],'Fixed Intel Editor')
    validate_inputs(source_tree(R),N['shared'],'Current shared input'); consumer_guard(ps())
    check(not os.path.lexists(P/'Temp/UnityLockfile') and not os.path.lexists(R/'Temp/UnityLockfile'),'R/P lock present')
    if current_stage=='I': check(not tree_entries(BC),'BC must be empty before I')
    as_guard(True); projection_guard(); protection(); resources()
def capture_before():
    global B
    editor=pathlib.Path(N['editor']['path']).parent.parent
    tools=[editor/'NetCoreRuntime/dotnet',editor/'DotNetSdkRoslyn/csc.dll']
    B={'sharedRoots':['Assets','Packages','ProjectSettings','Config','Tools','Generated','ArtSource'],'shared':{},'restoreBaseline':N['restoreBaseline'],'dlls':dlls(),'compileBindings':{},'hostArtifacts':tree_entries(P/'TestArtifacts'),'oldEvidence':old_evidence(),'compilerTools':{str(p):ident(p) for p in tools}}
    for directory in B['sharedRoots']: B['shared'].update(inventory(R/directory,R))
    for path,value in compiler_paths().items(): check(ident(P/path)==value,'Compiler preimage drift '+path)
    for path,value in D['compilePlan']['preserveInPlaceIdentities'].items(): check(ident(P/path)==value,'Compiler state preimage drift '+path)
    graph=json.loads(bounded_text(P/D['compilePlan']['previousGraph']['path'])); B['referencePreimages']={}
    for node in checked(graph['Nodes']):
        if node.get('DisplayName') in {'Csc '+name for name in N['requiredAssemblies']}:
            for value in checked(node.get('Inputs',[])):
                if value.endswith('.dll'):
                    path=compiler_path(value); B['referencePreimages'][str(path)]=ident(path)
    write('before.json',B)
def synchronize():
    global synced
    consumer_guard(ps()); check(not os.path.lexists(P/'Temp/UnityLockfile') and not os.path.lexists(R/'Temp/UnityLockfile'),'No R/P lock'); validate_inputs(source_tree(R),N['shared'],'Shared synchronization source'); projection_guard()
    for path in N['overwritten']: probe_timeout(); transfer(P/path,E/'restore/source'/path,N['restoreBaseline'][path])
    for path in N['newPaths']: check(not os.path.lexists(P/path),'New path must be absent '+path)
    for path in N['parked']:
        probe_timeout(); parked_paths.append(path); transfer(P/path,E/'park/source'/path,N['restoreBaseline'][path],True)
    for path in N['parkDirectories']:
        probe_timeout(); target=P/path; check(not list(children(target)),'Only empty scoped directory removal'); target.rmdir()
    for path in N['overwritten']+N['newPaths']:
        probe_timeout(); source=R/path; target=P/path; no_links(source); no_links(target); check(ident(source)==N['files'][path],'Frozen adopted source '+path)
        payload=bounded_read(source); check({'bytes':len(payload),'sha256':hashlib.sha256(payload).hexdigest()}==N['files'][path],'Source changed during read')
        if path in N['overwritten']: check(ident(target)==N['restoreBaseline'][path],'Concurrent target drift '+path)
        synchronized_paths.append(path); atomic_write(path,payload,N['files'][path],N['restoreBaseline'][path] if path in N['overwritten'] else None,'sync')
        check(ident(target)==N['files'][path],'Synchronized bytes '+path); event('source_synchronized',path=path,identity=N['files'][path])
    probe_timeout(); synced=True; projection_guard(); probe_timeout()
def frozen_text(path):
    data=bounded_read(path); identity=ident(path)
    digest=hashlib.sha256()
    for offset in checked(range(0,len(data),1048576)): digest.update(data[offset:offset+1048576]); probe_timeout()
    check(identity=={'bytes':len(data),'sha256':digest.hexdigest()},'Text changed while reading '+str(path))
    return data.decode('utf-8-sig'),identity
def compiler_graph():
    candidates=[]
    for path in children(P/'Library/Bee'):
        if not path.name.endswith('.dag.json'): continue
        text,identity=frozen_text(path); graph=json.loads(text); graph['_sealedIdentity']=identity
        if all(any(node.get('DisplayName')=='Csc '+name for node in checked(graph.get('Nodes',[]))) for name in N['requiredAssemblies']): candidates.append((path,graph))
    check(len(candidates)==1,'INCOMPLETE: current compiler graph absent or ambiguous')
    return candidates[0]
def compiler_path(value,source=False):
    path=pathlib.Path(value)
    if path.is_absolute():
        if path.is_relative_to(P): value=path.relative_to(P).as_posix()
        else:
            check(not source and path.is_relative_to(pathlib.Path(N['editor']['path']).parents[3]),'Compiler path outside bound roots')
            return path
    check('..' not in pathlib.PurePosixPath(value).parts and 'Samples~' not in pathlib.PurePosixPath(value).parts,'Unscoped compiler path')
    if value.startswith('Packages/'):
        pieces=value.split('/'); matches=[name for name in D['cache']['installedPackages'] if name.split('@')[0]==pieces[1]]
        check(len(matches)==1,'Package source alias ambiguous'); value='Library/PackageCache/'+matches[0]+'/'+('/'.join(pieces[2:]))
    if source:
        if value.startswith('Assets/'): check(value in N['files'],'Unfrozen project source')
        else:
            parts=value.split('/'); check(parts[:2]==['Library','PackageCache'] and parts[2] in D['cache']['sdkFiles'],'Unfrozen SDK source')
            check('/'.join(parts[3:]) in D['cache']['sdkFiles'][parts[2]],'SDK source outside frozen package')
    return P/value
def compiler_binding(name,graph_pair=None):
    try:
        graph_path,graph=graph_pair or compiler_graph(); matches=[(i,n) for i,n in checked(enumerate(graph['Nodes'])) if n.get('DisplayName')=='Csc '+name]
        check(len(matches)==1,'Ambiguous Csc node'); index,node=matches[0]; command=shlex.split(node['Action'])
        editor=pathlib.Path(N['editor']['path']).parent.parent; tools=[str(editor/'NetCoreRuntime/dotnet'),str(editor/'DotNetSdkRoslyn/csc.dll')]
        check(command[:3]==[tools[0],'exec',tools[1]],'Unknown compiler action')
        responses={}; tokens=[]; seen=set()
        def expand(values):
            for token in checked(values):
                if token.startswith('@'):
                    path=compiler_path(token[1:]); key=str(path.relative_to(P))
                    check(key not in seen and len(seen)<16,'Recursive/duplicate response file'); seen.add(key)
                    text,identity=frozen_text(path); responses[key]={'identity':identity,'text':text}; expand(shlex.split(text))
                else: tokens.append(token)
        expand(command[3:]); sources={}; refs={}
        for token in checked(tokens):
            if token.endswith('.cs') and not token.startswith(('-','/')) or token.startswith(str(P)) and token.endswith('.cs'):
                path=compiler_path(token,True); key=path.relative_to(P).as_posix(); sources[key]=ident(path)
            if token.startswith(('-r:','/r:','-reference:','/reference:')):
                raw=token.split(':',1)[1]; path=compiler_path(raw)
                refs[raw]={'path':str(path),'identity':ident(path),'mtimeNs':path.stat().st_mtime_ns}
        expected=N['assemblySources'][name]; check(sorted(sources)==sorted(expected),'Exact current source set '+name)
        for key,value in sources.items():
            if key.startswith('Assets/'): want=N['files'][key]
            else:
                parts=key.split('/'); want=D['cache']['sdkFiles'][parts[2]]['/'.join(parts[3:])]
            check(value==want,'Compiler source drift '+key)
        definition=D['compilePlan']['assemblies'][name]['asmdef']; check(ident(P/definition['path'])==basic({'x':definition})['x'],'Assembly definition drift')
        outputs={path:{'identity':ident(compiler_path(path)),'mtimeNs':compiler_path(path).stat().st_mtime_ns} for path in node['Outputs']}
        dll=ident(P/'Library/ScriptAssemblies'/(name+'.dll')); output=next(path for path in outputs if path.endswith('/'+name+'.dll'))
        tool_ids={path:ident(path) for path in tools}; check(tool_ids==B['compilerTools'],'Compiler tool provenance')
        check(outputs[output]['identity']==dll,'INCOMPLETE: imported DLL requires an IL postprocess chain')
        graph_inputs={value:ident(compiler_path(value)) for value in node['Inputs']}
        response={'graphInputs':graph_inputs,'files':responses,'tokens':tokens,'action':command,'tools':tool_ids,'asmdef':definition,'node':node,'outputs':outputs,'graphPath':str(graph_path.relative_to(P))}
        return {'complete':bool(sources) and bool(responses) and bool(refs),'response':response,'sourceInputs':sources,'references':refs,'defines':[t for t in tokens if t.startswith(('-define:','/define:'))],'compilerOutput':outputs[output]['identity'],'dll':dll,'outputPath':output,'outputMtimeNs':outputs[output]['mtimeNs'],'nodeIndex':index}
    except (RuntimeError,OSError,ValueError,KeyError,StopIteration) as error: return {'complete':False,'reason':str(error)}
def cached_reuse_proven(before,current,prior):
    keys=('response','sourceInputs','references','defines','compilerOutput','dll')
    return before.get('complete') is True and current.get('complete') is True and prior.get('proven') is True and prior.get('origin') in ('actualCsc','verifiedCacheChain') and all(k in prior and before.get(k)==current.get(k)==prior[k] for k in keys)
def compilation_passed(proof):
    return set(proof.get('assemblies',{}))==set(N['requiredAssemblies']) and all(v.get('proven') is True for v in proof['assemblies'].values())
def csc_events(log):
    events=[]
    output=r'(Library/Bee/[^\s]+\.dll)(?: \(\+(\d+) others\))?'
    for index,line in checked(enumerate(log.splitlines())):
        if not re.search(r'\bCsc\s+Library/Bee/',line): continue
        action=re.fullmatch(r'\[(\d+)/(\d+)(?:[ \t]+\d+(?:\.\d+)?(?:ms|s))?\][ \t]+Csc '+output,line)
        telemetry=re.fullmatch(r'\[[ \t]+\d+(?:\.\d+)?(?:ms|s)\][ \t]+Csc '+output+r' \[CacheWrite \]',line)
        row={'line':index+1,'text':line}
        if action:
            current,total,path,others=action.groups(); check(0<int(current)<=int(total),'INCOMPLETE: invalid Csc action counter')
            row.update(kind='ACTION',outputPath=path,otherOutputs=None if others is None else int(others))
        elif telemetry:
            path,others=telemetry.groups(); others=None if others is None else int(others); previous=events[-1] if events else {}
            check(previous.get('kind')=='ACTION' and previous['line']==index and previous['outputPath']==path and previous['otherOutputs']==others,'INCOMPLETE: unassociated CacheWrite telemetry')
            row.update(kind='CACHE_WRITE',outputPath=path,otherOutputs=others,actionLine=previous['line'])
        else: raise RuntimeError('INCOMPLETE: unrecognized Csc/CacheRead format at line '+str(index+1))
        events.append(row)
    return events
def compile_evidence(log,sid='I'):
    global full_inputs
    full_inputs=source_tree(P,True); extras=set(full_inputs)-set(N['files'])
    check(extras<={N['allowedNewSettings']['path']},'Imported additions'); validate_inputs({p:v for p,v in full_inputs.items() if p not in extras},N['files'],'Full compiled inputs')
    if extras: check(ident(P/N['allowedNewSettings']['path'])==basic({'x':N['allowedNewSettings']})['x'],'Natural settings identity')
    pair=compiler_graph(); graph=pair[1]; bindings={name:compiler_binding(name,pair) for name in N['requiredAssemblies']}; assemblies={}
    events=csc_events(log)
    for name,binding in checked(bindings.items()):
        related=[row for row in events if re.search(r'/'+re.escape(name)+r'\.dll$',row['outputPath'])]; matches=[row for row in related if row['kind']=='ACTION']
        actual=len(matches)==1 and all(row['outputPath']==binding.get('outputPath') and (row['otherOutputs'] is None or row['otherOutputs']==len(binding.get('response',{}).get('outputs',{}))-1) for row in related)
        cached=sid=='T' and not related and cached_reuse_proven(stage_bindings.get('T',{}).get(name,{}),binding,stage_dlls['I']['assemblies'].get(name,{}))
        assemblies[name]={**binding,'actualCsc':actual,'event':matches[0] if actual else None,'cacheReuseClaimed':cached,'origin':'verifiedCacheChain' if cached else 'actualCsc','proven':cached,'referenceProof':[]}
    def ancestors(index):
        found=set(); todo=list(graph['Nodes'][index].get('ToBuildDependencies',[]))+list(graph['Nodes'][index].get('ToUseDependencies',[]))
        while todo:
            probe_timeout(); current=todo.pop()
            if current in found: continue
            check(isinstance(current,int) and 0<=current<len(graph['Nodes']),'Graph dependency index')
            found.add(current); node=graph['Nodes'][current]; todo.extend(node.get('ToBuildDependencies',[])+node.get('ToUseDependencies',[]))
        return found
    output_owners={}
    for name,binding in checked(bindings.items()):
        if binding.get('complete'):
            for path in binding['response']['outputs']: output_owners[str(compiler_path(path))]=name
            output_owners[str(P/'Library/ScriptAssemblies'/(name+'.dll'))]=name
    for name,row in sorted(assemblies.items(),key=lambda pair:(pair[1]['event'] or {'line':-1})['line']):
        if row['proven'] or not row.get('complete') or not row['actualCsc']: continue
        valid=True; deps=ancestors(row['nodeIndex'])
        for raw,ref in checked(row['references'].items()):
            producer=output_owners.get(ref['path']); proof={'reference':raw,'identity':ref,'producer':producer}
            if producer:
                parent=assemblies[producer]; expected=bindings[producer]['response']['outputs'].get(str(pathlib.Path(ref['path']).relative_to(P)),{'identity':parent['dll']})['identity']
                ok=parent['proven'] and parent['nodeIndex'] in deps and expected==ref['identity'] and (parent['cacheReuseClaimed'] or parent['event']['line']<row['event']['line'])
            else: ok=B['referencePreimages'].get(ref['path'])==ref['identity']
            ok=ok and ref['mtimeNs']<=row['outputMtimeNs']; proof['proven']=bool(ok); row['referenceProof'].append(proof); valid=valid and ok
        row['proven']=bool(valid)
    check(ident(pair[0])==graph['_sealedIdentity'],'Graph changed during evidence capture')
    proof={'stage':sid,'assemblies':assemblies,'events':events,'graph':graph,'graphIdentity':ident(pair[0]),'fullImportedInputs':full_inputs,'fullInputCanonicalSha256':canonical(full_inputs),'fullInputLeaves':len(full_inputs),'missingEvidenceMeans':'INCOMPLETE; never retry or infer an old cache chain'}
    stage_dlls[sid]=proof; return proof
def claim_stage(sid='I'):
    global launch_attempts
    check(sid in ('I','T') and not launch_counts.get(sid),'Stage may be claimed once')
    check(sid=='I' and not launch_counts or sid=='T' and launch_counts=={'I':1},'Exact I then T sequence')
    launch_counts[sid]=1; launch_attempts+=1
def run_stage(sid='I'):
    global active,root_launch_epoch,launched_root,probe_deadline
    claim_stage(sid); isolate_stage(sid); s=next(v for v in A['stages'] if v['id']==sid); check(not (E/sid).exists(),'I already exists'); (E/sid).mkdir(); start=time.monotonic(); rootpid=None; problem=None; prior=probe_deadline
    result={'stage':sid,'status':'NOT_RUN_BLOCKED','runCount':0,'exitCode':None,'argv':s['argv'],'cwd':str(P),'environmentOverrides':A['environmentOverrides'],'startedUtc':utc()}; stages.append(result)
    try:
        probe_timeout(); check(synced,'Sync required'); preflight(); stage_bindings[sid]={name:compiler_binding(name) for name in N['requiredAssemblies']} if sid=='T' else {}; start=time.monotonic()
        check(work_deadline is None or start+s['timeoutSeconds']<=work_deadline,'Insufficient work window for '+sid+' with cleanup reserve')
        probe_deadline=start+s['timeoutSeconds']; probe_deadline=min(probe_deadline,work_deadline) if work_deadline is not None else probe_deadline
        with (E/sid/'launcher.log').open('xb') as stream:
            probe_timeout()
            root_launch_epoch=time.time() if root_launch_epoch is None else root_launch_epoch; active=subprocess.Popen(s['argv'],cwd=P,env=stage_environment(sid),stdout=stream,stderr=subprocess.STDOUT,start_new_session=True); rootpid=active.pid; result.update(pid=rootpid,runCount=1,status='RUNNING'); rows=ps()
            check(rootpid in rows,'Editor missing at launch'); launched_root=dict(rows[rootpid],pid=rootpid,stage=sid); recover_root(rows,sid,rootpid); check(rootpid in owned,'Initial root not registered'); check(owned[rootpid]['exe']==A['editor']['path'] and owned[rootpid]['cwd']==str(P),'Root identity'); event('stage_started',stage=sid,pid=rootpid,argv=s['argv'])
            while active.poll() is None:
                rows=ps(); monitor('running',rows,sid,rootpid,strict=True); check(time.monotonic()-start<s['timeoutSeconds'],sid+' stage deadline exceeded')
                text=bounded_text(E/sid/'editor.log',errors='replace') if (E/sid/'editor.log').exists() else ''; log_guard(text); time.sleep(.25)
        result['editorSeconds']=time.monotonic()-start
    except BaseException as ex: problem=str(ex)
    finally:
        probe_deadline=prior
        if rootpid is not None:
            try: closure(sid,rootpid,problem or 'Editor exited')
            except BaseException as ex: problem=(problem+'; ' if problem else '')+str(ex)
            result['exitCode']=active.poll()
        result['elapsedSecondsIncludingClosure']=time.monotonic()-start; result['finishedUtc']=utc()
    try:
        check(problem is None,problem); check(result['exitCode'] is not None,'INCOMPLETE: unknown OS exit'); check(result['runCount']==1 and result['exitCode']==0 and result.get('editorSeconds',s['timeoutSeconds']+1)<=s['timeoutSeconds'],'Editor exit/time '+str(result['exitCode'])); check(not monitor_errors,'Monitor failures')
        probe_timeout(); log=bounded_text(E/sid/'editor.log',errors='replace'); log_guard(log); projection_guard(); protection(); resources(); probe_timeout(); proof=compile_evidence(log,sid); probe_timeout()
        result['status']='COMPILE_PASS' if compilation_passed(proof) else 'INCOMPLETE'
        if sid=='T' and result['status']=='COMPILE_PASS': result['tests']=test_evidence(); probe_timeout(); result['status']='TEST_PASS'
    except BaseException as ex:
        if str(ex)!=problem: problem=(problem+'; ' if problem else '')+str(ex)
        result['status']=('INCOMPLETE' if 'INCOMPLETE:' in str(ex) else 'CONTRACT_MISMATCH' if 'CONTRACT_MISMATCH:' in str(ex) else 'FAILED') if rootpid else 'NOT_RUN_BLOCKED'
    result['failure']=problem; write(sid+'/result.json',result)
    if active is not None and active.poll() is not None: active=None
    return result
def log_guard(text):
    bad=[x for x in checked(text.splitlines()) if re.search(r'TypeLoadException|ReflectionTypeLoadException|Failed to reload.*assembl|Domain reload.*(?:failed|error)|error CS\d+|Compilation failed|Scripts have compiler errors|Failed to load.*assembly|Could not load.*assembly|Aborting batchmode due to failure|(?:Downloading|downloaded|fetching).*https?://|(?:Package Manager|UPM).*(?:unable to|failed to|error).*?(?:resolve|connect|registry|network)|ENOTFOUND|ETIMEDOUT',x,re.I)]
    check(not bad,'Compiler/network failure '+json.dumps(bad[-10:]))
def archive_and_restore(emit=True):
    global restored
    check(closure_closed is not False,'Unclosed process sampling; restore forbidden')
    start=time.monotonic(); check(active is None or active.poll() is not None,'Launched Popen root still live; restore forbidden'); probe_timeout(); rows=ps(); snapshot_consumers(rows,'I',next(iter(owned),None)) if owned else consumer_guard(rows); check(not any(alive(rows,p) for p in owned),'Owned remains; restore forbidden')
    report={'archived':[],'restored':[],'returnedParked':[],'errors':[],'complete':False}; last=start
    def tick():
        nonlocal last
        probe_timeout(); now=time.monotonic(); check(now-start<=60 and (restore_deadline is None or now<restore_deadline),'Restore60 exceeded')
        if now-last>=2:
            rows=ps(); consumer_guard(rows); check(not any(alive(rows,p) for p in owned),'Owned during restore'); resources(); last=now
    for path in synchronized_paths:
        try:
            tick(); target=P/path; check(('sync',path) not in atomic_conflicts,'Preserve prior atomic conflict '+path)
            if path in N['newPaths'] and not target.exists(): continue
            if path in N['overwritten'] and ident(target)==N['restoreBaseline'][path]: report['restored'].append(path); continue
            check(ident(target)==N['files'][path],'Concurrent/partial value preserved '+path); transfer(target,E/'archive/source'/path,N['files'][path],path in N['newPaths']); report['archived'].append(path)
            if path in N['overwritten']:
                check(ident(E/'restore/source'/path)==N['restoreBaseline'][path],'Backup drift '+path); atomic_write(path,bounded_read(E/'restore/source'/path),N['restoreBaseline'][path],N['files'][path],'restore'); check(ident(target)==N['restoreBaseline'][path],'Restored bytes '+path)
            report['restored'].append(path)
        except BaseException as ex: report['errors'].append(str(ex))
    for path in parked_paths:
        try:
            tick(); source=E/'park/source'/path
            if not source.exists(): check(ident(P/path)==N['restoreBaseline'][path],'Incomplete park preserved'); continue
            transfer(source,P/path,N['restoreBaseline'][path],True); report['returnedParked'].append(path)
        except BaseException as ex: report['errors'].append(str(ex))
    try:
        tick(); settings=N['allowedNewSettings']; target=P/settings['path']
        if any(s['runCount'] for s in stages) and target.exists() and settings['path'] not in N['restoreBaseline']: transfer(target,E/'archive/SceneTemplateSettings.json',basic({'x':settings})['x'],True)
        tick(); check(not transfer_conflicts,'Unresolved transfers '+str(sorted(transfer_conflicts))); restored=True; report['projection']=projection_guard(); protection(); tick(); report['complete']=not report['errors']
    except BaseException as ex: report['errors'].append(str(ex))
    report['transferConflicts']=sorted(transfer_conflicts); report['seconds']=time.monotonic()-start
    if emit: write('restore.json',report)
    return report
def normalize_input(raw):
    proposed=raw['projectionProposed']; files=basic(raw['shared']['files'])
    extra=proposed['retainedNaturalMeta']; files[extra['path']]=basic({'x':extra})['x']
    return {'files':files,'shared':basic(raw['shared']['files']),'restoreBaseline':basic(raw['projectionBefore']['files']),
            'overwritten':proposed['overwritten'],'newPaths':proposed['newPaths'],'parked':proposed['parked'],'parkDirectories':proposed['parkDirectories'],
            'allowedNewSettings':raw['allowedNewSettings'],'requiredAssemblies':raw['compilePlan']['requiredAssemblies'],
            'assemblySources':{n:v['expectedSources'] for n,v in raw['compilePlan']['assemblies'].items()},'priorCompileBindings':{},'editor':raw['editor']}
def contract_guard(raw,qa):
    check(raw['task']=='RES-COMBINED-V06' and raw['schemaVersion']==1,'Current combined schema required')
    for key,count in [('shared',1036),('projectionBefore',1035)]:
        section=raw[key]; check(len(section['files'])==count and canonical(section['files'])==section['summary']['canonicalSha256'],'Fixed '+key)
    proposed=dict(raw['shared']['files']); meta=raw['projectionProposed']['retainedNaturalMeta']; proposed[meta['path']]=meta
    check(len(proposed)==1037 and canonical(proposed)==raw['projectionProposed']['summary']['canonicalSha256'],'Proposed canonical')
    normalized=normalize_input(raw); before=dict(normalized['restoreBaseline'])
    for path in normalized['parked']: before.pop(path)
    for path in normalized['overwritten']+normalized['newPaths']: before[path]=normalized['files'][path]
    check(before==normalized['files'] and [len(normalized[k]) for k in ('overwritten','newPaths','parked')]==[17,4,2],'Exact source delta')
    check(len(raw['compilePlan']['proposedParkExactLeaves'])==43 and raw['compilePlan']['oldPriorCompileBindings']=={},'Exact compiler invalidation')
    names=[name for fixture in qa['fixtures'] for name in fixture['testFullnames']]
    check(len(names)==89 and len(set(names))==89,'Fixed 89-name Counter')
    check(raw['commands']['T'][raw['commands']['T'].index('-testFilter')+1]==qa['selection']['testFilterArgument'],'Fixed filter')
    check('-quit' in raw['commands']['I'] and '-runTests' not in raw['commands']['I'] and '-quit' not in raw['commands']['T'],'Exact I/T mode')
    check(raw['limits']['mechanicalPreparationSeconds']==raw['preparation']['budgetSeconds']==160 and raw['limits']['finalizationSeconds']==30 and raw['limits']['totalMechanicalSeconds']==900,'Fixed preparation/finalization/total budgets')
    check(raw['commands']['environmentBoth']['DOTNET_EnableDiagnostics']=='0','Diagnostics disabled for current I/T')
    return normalized
def compiler_paths():
    return D.get('compilePlan',{}).get('proposedParkExactLeaves',{})
def dynamic_evidence():
    paths=N['overwritten']+N['newPaths']
    result={'archive/source/'+p for p in paths}|{'restore/source/'+p for p in N['overwritten']}|{'park/source/'+p for p in N['parked']}
    result|={'atomic/sync/'+p for p in paths}|{'atomic/restore/'+p for p in N['overwritten']}
    result|={'atomic/'+kind+'/'+phase+'/'+p for kind in ('displaced','conflict') for phase in ('sync','restore') for p in N['overwritten']}
    result|={kind+'/compiler/'+p for kind in ('park','archive') for p in compiler_paths()}
    return result|{'atomic/transfer/'+tag+'/'+leaf for tag in transfer_slots().values() for leaf in ('capture','retired')}
def cache_guard():
    def exact_tree(root,expected):
        entries=tree_entries(root); check(not any(v['type']=='symlink' for v in entries.values()),'Cache link '+str(root))
        digest=json_digest(entries)
        check(digest==expected['treeSha256'] and sum(v.get('bytes',0) for v in entries.values())==expected['bytes'],'Frozen cache tree '+str(root))
    exact_tree(K,D['cache']['upmRoot'])
    package_root=P/'Library/PackageCache'; packages=D['cache']['installedPackages']
    check({x.name for x in children(package_root)}==set(packages),'Installed package set')
    for name,expected in packages.items(): exact_tree(package_root/name,expected)
    check(json.loads(bounded_text(P/'Packages/packages-lock.json'))['dependencies']==D['cache']['lockedNodes'],'53-node package graph')
    for path,value in D['cache']['projectCacheBefore'].items(): check(ident(P/path)==value,'PackageManager cache drift '+path)
    return {'packages':len(packages),'payloadFrozen':True}
def as_guard(final=False):
    no_links(ASROOT); entries={}
    for base,dirs,files in scan_tree(ASROOT):
        for name in checked(dirs+files):
            path=pathlib.Path(base)/name; relative=path.relative_to(ASROOT).as_posix()
            try:
                value=path.lstat()
                if stat.S_ISLNK(value.st_mode): entries[relative]={'type':'symlink','target':os.readlink(path)}
                elif stat.S_ISDIR(value.st_mode): entries[relative]={'type':'directory'}
                else:
                    check(stat.S_ISREG(value.st_mode) and value.st_nlink==1,'AS special/hard-linked leaf')
                    entries[relative]={'type':'file','bytes':value.st_size}
            except FileNotFoundError:
                check(not final,'AS changed during final proof')
    for name,item in checked(entries.items()):
        if item['type']=='symlink':
            check(name=='AS07/product/resource-state/v1/active.json' and item['target'] in [str(ASROOT/'AS07/product/external.bin'),str(ASROOT/'AS07/product/absent-target')],'Foreign AS link')
    check(sum(v.get('bytes',0) for v in entries.values())<=D['limits']['asBytes'] and len(entries)<=D['limits']['asLeaves'],'AS budget')
    if final: check(not entries and not list(children(ASROOT)),'AS lease residue retained')
    return {'entries':entries,'empty':not entries,'noLinkFollow':True}
def isolate_stage(sid):
    global owned,pending_details,process_snapshot,snapshot_root,launched_root,closure_closed,last_monitor,monitor_errors,monitor_cycles,current_stage
    if sid=='T':
        check(stages and stages[-1]['stage']=='I' and stages[-1]['status']=='COMPILE_PASS' and closure_closed is True,'T requires passed closed I')
        check(active is None and not pending_details and not monitor_errors and compilation_passed(stage_dlls.get('I',{})),'I proof and process closure required')
        rows=ps(); check(not any(alive(rows,p) for p in owned),'I identities still live')
        stage_history.append({'stage':'I','owned':copy.deepcopy(owned),'launchedRoot':launched_root,'monitorCycles':monitor_cycles,'monitorErrors':monitor_errors})
        owned={}; pending_details={}; process_snapshot={}; snapshot_root=None; launched_root=None; closure_closed=None; last_monitor=0.; monitor_errors=[]; monitor_cycles=[]
        consumer_guard(rows)
    current_stage=sid
def park_compiler():
    for path,identity in compiler_paths().items():
        probe_timeout(); consumer_guard(ps()); probe_timeout()
        compiler_parked.append(path); transfer(P/path,E/'park/compiler'/path,identity,True)
def restore_compiler():
    errors=[]; archived=[]
    for path in compiler_parked:
        probe_timeout()
        if (P/path).exists(): compiler_after[path]=ident(P/path)
        else: compiler_after[path]=None
    check(sum(v['bytes'] for v in compiler_after.values() if v is not None)<=D['evidenceSlots']['compilerArchiveMaxBytes'],'Compiler archive budget')
    for path in compiler_parked:
        try:
            probe_timeout(); consumer_guard(ps()); original=compiler_paths()[path]; backup=E/'park/compiler'/path
            if not backup.exists(): check(ident(P/path)==original,'Missing parked compiler preimage '+path); continue
            check(ident(backup)==original,'Compiler backup drift '+path)
            if compiler_after[path] is not None:
                check(ident(P/path)==compiler_after[path],'Compiler postimage changed '+path)
                transfer(P/path,E/'archive/compiler'/path,compiler_after[path],True); archived.append(path)
            transfer(backup,P/path,original,True); compiler_restored.append(path)
        except BaseException as error: errors.append(str(error))
    for path,identity in compiler_paths().items():
        try: probe_timeout(); check(ident(P/path)==identity,'Compiler restoration drift '+path)
        except BaseException as error: errors.append(str(error))
    return {'archived':archived,'restored':list(compiler_restored),'afterIdentities':dict(compiler_after),'errors':errors,'complete':not errors and not transfer_conflicts}
def restore_all():
    global restore_deadline,probe_deadline
    check(closure_closed is not False and (active is None or active.poll() is not None),'Unclosed; no restore')
    restore_deadline=time.monotonic()+D['limits']['restoreSeconds']; restore_deadline=min(restore_deadline,execution_deadline-D.get('limits',{}).get('finalizationSeconds',0)) if execution_deadline is not None else restore_deadline; prior=probe_deadline; probe_deadline=restore_deadline
    try:
        probe_timeout(); rows=ps(); snapshot_consumers(rows,current_stage,next(iter(owned),None)) if owned else consumer_guard(rows)
        check(not any(alive(rows,p) for p in owned),'Owned remains; no restore')
        compiler=restore_compiler(); source=archive_and_restore(False)
        probe_timeout(); source['compiler']=compiler; source['complete']=source['complete'] and compiler['complete']
        write('restore.json',source); return source
    finally: probe_deadline=prior
def validate_xml(text):
    root=ET.fromstring(text); nodes=list(root.iter('test-case')); expected=Counter(name for fixture in Q['fixtures'] for name in fixture['testFullnames'])
    check(root.tag=='test-run' and nodes,'INCOMPLETE: missing complete test-run')
    check(Counter(x.get('fullname') for x in nodes)==expected,'CONTRACT_MISMATCH: test fullname Counter')
    check(all(x.get('result')=='Passed' for x in nodes),'FAILED: non-passing test')
    check(root.get('result')=='Passed' and all(int(root.get(k,'-1'))==v for k,v in [('total',89),('passed',89),('failed',0),('skipped',0),('inconclusive',0)]),'FAILED: XML root counts')
    return {'total':len(nodes),'passed':len(nodes),'counter':dict(expected),'allPassed':True}
def test_evidence():
    path=E/'T/results.xml'; check(path.is_file(),'INCOMPLETE: missing XML')
    check(path.stat().st_size<=D['evidenceSlots']['xmlBytes'],'XML budget')
    try: result=validate_xml(bounded_text(path))
    except ET.ParseError as error: raise RuntimeError('INCOMPLETE: corrupt XML') from error
    fixture=next(v for v in Q['fixtures'] if v['id']=='AS'); source=fixture['source']
    check(ident(P/source['path'])==basic({'x':source})['x'],'AS test implementation changed')
    result['xml']=ident(path); result['AS']={'implementation':source,'passedCalls':fixture['testFullnames'],'lease':as_guard(True),'evidenceBasis':'Fixed native AS06/AS07/AS08 test implementation and this exact current-run Passed XML; no simulated symlink or restart claim'}
    write('T/test-proof.json',result); return result
def run_sequence():
    result=run_stage('I')
    return run_stage('T') if result['status']=='COMPILE_PASS' else result
def stage_environment(sid):
    environment={k:v for k,v in os.environ.items() if k not in ('UPM_CACHE_ROOT','TMPDIR','BEE_CACHE_DIRECTORY','FIGHTMATCH_ACTIVATION_TEST_ROOT')}
    environment.update(A['environmentOverrides'])
    if sid=='T': environment.update(D['commands']['environmentTOnly'])
    environment['DOTNET_EnableDiagnostics']='0'
    return environment
def finalize(receipt,outputs):
    validation_status=receipt['status']; failure=receipt.get('failure'); attempted=[]
    completion={'status':'FINALIZATION_FAILED','validationStatus':validation_status,'failure':failure,'receipt':None,'attemptedEvidence':attempted,'successRequires':'Validated TEST_PASS, this receipt hash, OS exit0, and externally observed completion within900 seconds including preparation.'}
    try:
        for name,data in checked(outputs): attempted.append(name); write(name,data)
        evidence={}
        for path in checked(evidence_paths()): evidence[evidence_name(path)]=evidence_identity(path)
        probe_timeout(); receipt.update(evidence=evidence,validationStatus=validation_status,status='AWAITING_PROCESS_EXIT' if validation_status=='TEST_PASS' else validation_status,mechanicalExecutionSecondsThroughEvidence=time.monotonic()-clock_start,finishedUtcThroughEvidence=utc(),measurementBoundary='After all prior evidence writes/hashes; before receipt serialization/write/hash and stdout. The stdout completion record plus external OS exit and elapsed time are mandatory.')
        attempted.append('receipt.json'); write('receipt.json',receipt); completion['receipt']=ident(E/'receipt.json'); probe_timeout()
        completion.update(status='AWAITING_PROCESS_EXIT' if validation_status=='TEST_PASS' and not failure else 'FAILED',mechanicalExecutionSecondsThroughReceiptHash=time.monotonic()-clock_start,preparationSeconds=PREF['mechanicalPreparationSeconds'],measurementBoundary='After receipt write/close/hash; before this stdout serialization/flush. Deadline checked again after flush; external observer must record process exit and total elapsed time.')
    except BaseException as error:
        completion['failure']=(failure+'; ' if failure else '')+'finalization: '+str(error)
        completion['mechanicalExecutionSecondsAtFailure']=time.monotonic()-clock_start
    try:
        print(json.dumps(completion),flush=True)
        probe_timeout()
    except BaseException: return 1
    return 0 if completion['status']=='AWAITING_PROCESS_EXIT' and validation_status=='TEST_PASS' and not completion['failure'] else 1
def main():
    global A,B,D,Q,N,PREF,TMP,OWNER,ACT_SHA,EXECUTION_TURN,clock_start,baseline_processes,allowed,execution_deadline,work_deadline,probe_deadline
    clock_start=time.monotonic()
    check(len(sys.argv)==3,'Activation SHA and fresh C turn required'); ACT_SHA,EXECUTION_TURN=sys.argv[1:]
    check(ident(E/'activation.json')['sha256']==ACT_SHA,'Activation SHA'); A=json.loads(bounded_read(E/'activation.json'))
    check(A['status']=='EXECUTION_BOUND' and A['task']=='RES-COMBINED-V06','Current combined activation required')
    D=json.loads(bounded_read(E/'inputs.json')); PREF=json.loads(bounded_read(E/'preparation.json')); OWNER=A['executionOwner']
    check(ident(E/'inputs.json')=={'bytes':1248829,'sha256':'4fdcfff78f03da819b028e596db29c4ea584c7fde79d9830b929da2d4321405b'},'Current fixed inputs')
    check({k:str(v) for k,v in [('R',R),('P',P),('K',K),('executionEvidence',E),('newBeeCache',BC),('activationTests',ASROOT)]}==D['paths'],'Fixed path bindings')
    check(ident(R/D['testCases']['path'])==basic({'x':D['testCases']})['x'],'Case seal'); Q=json.loads(bounded_read(R/D['testCases']['path'])); N=contract_guard(D,Q)
    check(PREF['status']=='SOURCE_REPLAY_PASS' and PREF['mechanicalPreparationSeconds']<=160,'Preparation gate')
    allowed=set(D['evidenceSlots']['fixed']); TMP=pathlib.Path(A['environmentOverrides']['TMPDIR'])
    check(all(key in A for key in ('adbException','sdkAdb','sourceReview','runner','seals','stopping','stages')),'Fresh activation process exception bindings required')
    check(A['runner']==ident(__file__) and A['sourceReview']['runner']==A['runner'] and A['sourceReview']['status']=='ACCEPT','Reviewed runner required before effects')
    check(OWNER=={'thread':'01a0fdbc-bf1e-7780-8f7f-dec13d6d590c','host':'local','turn':EXECUTION_TURN},'Sole native executor')
    check(A['sourceReview'].get('head') and A['sourceReview'].get('resultUrl'),'Review receipt required before effects')
    for key in ('inputs','preparation','replay-check','replay-results'):
        check(ident(E/(key+('.py' if key=='replay-check' else '.json')))==A['seals'][key],'Activation artifact seal '+key)
    check(A['environmentOverrides']=={'UPM_CACHE_ROOT':str(K),'TMPDIR':str(TMP),'BEE_CACHE_DIRECTORY':str(BC),'DOTNET_EnableDiagnostics':'0'},'Environment gate')
    check(A['stages']==[{'id':sid,'timeoutSeconds':D['limits'][sid+'Seconds'],'maxRuns':1,'argv':D['commands'][sid]} for sid in ('I','T')],'Ordered stage gate')
    execution_deadline=clock_start+D['limits']['totalMechanicalSeconds']-PREF['mechanicalPreparationSeconds']; work_deadline=execution_deadline-A['stopping']['naturalGraceSeconds']-A['stopping']['termGraceSeconds']-D['limits']['restoreSeconds']-D['limits']['finalizationSeconds']; started=utc(); failure=None; restore=None; after={}; status='NOT_RUN_BLOCKED'; remaining=None
    try:
        check(not (E/'process-events.jsonl').exists() and not (E/'before.json').exists(),'Activation already used')
        no_links(TMP); check(not list(children(TMP)),'TMP initially empty')
        baseline_processes=ps(); consumer_guard(baseline_processes)
        for root in (BC,ASROOT):
            no_links(root); check(not os.path.lexists(root),'Fresh external root must be absent'); root.mkdir(parents=True,mode=0o700)
        event('process_baseline',rows=baseline_processes)
        for fn in (capture_before,preflight,synchronize,park_compiler): probe_timeout(); fn(); probe_timeout()
        result=run_sequence(); status=result['status']; failure=result['failure']
    except BaseException as error: failure=str(error)
    finally:
        if B:
            for name,fn in [('projection',projection_guard),('protection',protection),('resources',resources)]:
                try: probe_timeout(); after[name]=fn(); probe_timeout()
                except BaseException as error: after[name]={'error':str(error)}; failure=(failure+'; ' if failure else '')+name+': '+str(error)
            try: restore=restore_all(); check(restore['complete'],'Restore incomplete')
            except BaseException as error: failure=(failure+'; ' if failure else '')+'restore: '+str(error)
            try:
                probe_deadline=min(time.monotonic()+D['limits']['finalizationSeconds'],execution_deadline); probe_timeout(); rows=ps(); monitor('final',rows,current_stage,next(iter(owned),None),force=True); remaining=[dict(owned[p],current=rows[p]) for p in owned if alive(rows,p)]
                check(not remaining and not pending_details and not monitor_errors,'Final monitor gate'); after['temporaryTree']=tree_entries(TMP); after['beeIpc']=copy.deepcopy(bee_ipc); after['beeTree']=tree_identity(BC); after['AS']=as_guard(True); probe_timeout()
            except BaseException as error: failure=(failure+'; ' if failure else '')+str(error)
        else: probe_deadline=min(time.monotonic()+D['limits']['finalizationSeconds'],execution_deadline)
        if failure and not (status in ('INCOMPLETE','CONTRACT_MISMATCH') and restore and restore['complete'] and not monitor_errors): status='FAILED' if any(s['runCount'] for s in stages) else 'NOT_RUN_BLOCKED'
        outputs=[('compile.json',{'stages':stage_dlls,'fullImportedInputs':full_inputs,'compilerAfter':compiler_after,'noRuntimeOrAndroidAcceptance':True}),('after.json',after)]
        outputs.append(('process-after.json',{'stageHistory':stage_history,'currentStage':current_stage,'baseline':baseline_processes,'owned':list(owned.values()),'remainingOwned':remaining,'pendingIdentities':pending_details,'closureClosed':closure_closed,'launchedRoot':launched_root,'popenStillLive':active is not None and active.poll() is None,'monitorCycles':monitor_cycles,'monitorErrors':monitor_errors,'adbObservations':adb_observations,'sdkAdbObservations':sdk_observations,'sdkAdbBinding':sdk_adb,'transferConflicts':sorted(transfer_conflicts),'sigkill':False}))
        receipt={'task':D['task'],'status':status,'owner':OWNER,'startedUtc':started,'preparationSeconds':PREF['mechanicalPreparationSeconds'],'stages':stages,'failure':failure,'restore':restore,'launchCounts':launch_counts,'remainingOwned':remaining,'closureClosed':closure_closed,'unrun':['Play','downloads','real player saves','Git mutations','Android/device acceptance'],'soleCompletionReceiver':'01a0e401-511d-79f2-b47f-3ab0ade1681b/local'}
        completion_code=finalize(receipt,outputs)
    return completion_code
if __name__=='__main__': sys.exit(main())
