#!/usr/bin/env python3
import os,sys,json,hashlib,pathlib,stat,subprocess,time,datetime,signal,re,shlex,collections,shutil,xml.etree.ElementTree as ET
E=pathlib.Path(__file__).parent; M=E; R=E.parents[3]; O=R/'TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03'; P=O/'projection'; K=O/'package-cache'
ACT_SHA=sys.argv[1] if len(sys.argv)==3 else ''; EXECUTION_TURN=sys.argv[2] if len(sys.argv)==3 else ''
# Execution identity is read only from the later, signed activation binding.
def utc(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def check(ok,why):
    if not ok: raise RuntimeError(why)
def ident(p):
    p=pathlib.Path(p); s=p.lstat(); check(stat.S_ISREG(s.st_mode),'Nonregular file '+str(p)); h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(1048576),b''): h.update(b)
    return {'bytes':s.st_size,'sha256':h.hexdigest()}
A=json.loads((E/'activation.json').read_bytes()); check(len(sys.argv)==3 and A['status']=='EXECUTION_BOUND','Bound activation arguments required'); OWNER=A['executionOwner']; check(OWNER=={'thread':'01a0fdbc-bf1e-7780-8f7f-dec13d6d590c','host':'local','turn':EXECUTION_TURN} and EXECUTION_TURN=='01a11762-1ac1-7040-8240-f6f19ccd6268','Execution owner mismatch')
B=json.loads((E/'before.json').read_bytes()); N=json.loads((E/'inputs.json').read_bytes()); PREF=json.loads((E/'preparation.json').read_bytes()); C=json.loads(pathlib.Path(A['cacheManifest']['path']).read_bytes()); TMP=pathlib.Path(A['environmentOverrides']['TMPDIR'])
LIMIT=A['budgets']; allowed=set(A['newEvidenceAllowed']); owned={}; stages=[]; failure=None; active=None; guard_results=[]; synced=False; restored=False; candidate_pending=False; tmp_seen=set(); stage_dlls={}; adb_observations=[]; natural_history=[]; full_inputs=None; isolation_created=False; observation=None; synchronized_paths=[]; clock_start=time.monotonic(); baseline_processes={}; sdk_adb=None; sdk_observations=[]; monitor_errors=[]; monitor_cycles=[]; last_monitor=0.0; root_launch_epoch=None
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
                except RuntimeError:
                    if pid in ps(): raise
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
def monitor(phase,strict=False,force=False):
    global last_monitor
    now=time.monotonic()
    if not force and now-last_monitor<2: return
    cycle={'utc':utc(),'phase':phase,'sincePreviousSeconds':None if not last_monitor else round(now-last_monitor,6),'failures':[]}; last_monitor=now
    for name,fn in [('resources',resources),('projection',projection_guard),('consumers',lambda:consumer_guard(ps()))]:
        try: fn()
        except BaseException as ex:
            item={'utc':utc(),'phase':phase,'scope':name,'error':str(ex)}; monitor_errors.append(item); cycle['failures'].append(item); event('monitor_failure',detail=item)
    cycle['elapsedSeconds']=round(time.monotonic()-now,6); monitor_cycles.append(cycle); event('monitor_cycle',cycle=cycle)
    if strict and cycle['failures']: raise RuntimeError(cycle['failures'][0]['error'])
def closure(stage,rootpid,reason):
    event('closure_begin',stage=stage,reason=reason); deadline=time.monotonic()+A['stopping']['naturalGraceSeconds']
    while True:
        if active is not None: active.poll()
        rows=ps(); discover(rows,stage,rootpid); monitor('natural-closure'); remaining=[p for p in owned if alive(rows,p)]
        if not remaining or time.monotonic()>=deadline: break
        time.sleep(.25)
    for pid in reversed(remaining):
        try:
            rows=ps()
            if not alive(rows,pid): continue
            check(pathlib.Path(owned[pid]['exe']).name.lower()!='adb','No permission to signal any ADB')
            d=details(pid); check(d['argv']==owned[pid]['argv'],'Owned argv changed before TERM '+str(pid)); recorded_chain(pid,rootpid); consumer_guard(rows)
            check(not owned[pid]['termSent'],'Duplicate TERM refused'); event('signal',signal='SIGTERM',pid=pid,identity=owned[pid],rematched=d)
            try: os.kill(pid,signal.SIGTERM); owned[pid]['termSent']=True
            except ProcessLookupError: event('signal_race_already_exited',pid=pid)
        except BaseException as ex:
            item={'utc':utc(),'phase':'term-authorization','scope':'process','error':str(ex)}; monitor_errors.append(item); event('monitor_failure',detail=item)
    deadline=time.monotonic()+A['stopping']['termGraceSeconds']
    while True:
        if active is not None: active.poll()
        rows=ps(); discover(rows,stage,rootpid); monitor('term-confirmation'); remaining=[p for p in owned if alive(rows,p)]
        if not remaining or time.monotonic()>=deadline: break
        time.sleep(.25)
    monitor('closure-final',force=True); event('closure_end',stage=stage,remainingOwned=remaining); check(not remaining,'BLOCKED: owned processes remain '+str(remaining))
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
def projection_guard(require_metas=False):
    actual={}
    for d in ['Assets','Packages','ProjectSettings']: actual.update(inventory(P/d,P))
    expected=N['files'] if synced and not restored else B['restoreBaseline']; extras=set(actual)-set(expected); settings=A['allowedNewSettings']; meta=A['allowedNaturalMeta'][0]
    check(extras<={meta,settings['path']} if synced and not restored else not extras,'Unexpected projection additions '+str(sorted(extras)))
    same({p:v for p,v in actual.items() if p not in extras},expected,'Exact current projection inputs')
    if settings['path'] in extras: check(actual[settings['path']]=={k:settings[k] for k in ('bytes','sha256')},'Default settings drift')
    natural={}; guids={}
    for path in actual:
        if path.endswith('.meta'):
            value=(P/path).read_text(); matches=re.findall(r'(?m)^guid:\s*([0-9a-f]{32})\s*$',value); check(len(matches)==1 and matches[0] not in guids,'Invalid/duplicate GUID '+path); guids[matches[0]]=path
            if path==meta:
                header='fileFormatVersion: 2\nguid: '+matches[0]+'\n'; complete=header+'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n'
                check(value in (header,complete),'Nondefault natural probe meta'); item={**actual[path],'guid':matches[0]}
                check(not natural_history or natural_history[0]['guid']==matches[0],'Natural meta GUID changed')
                if not natural_history or natural_history[-1]!=item: natural_history.append(item); event('natural_meta_observed',identity=item)
                natural[path]=item
    if require_metas: check(meta in natural,'Natural probe meta missing')
    check(all(x.name in {'Assets','Packages','ProjectSettings','Library','Temp','Logs','UserSettings','obj','TestArtifacts'} or (x.is_file() and x.suffix in ('.csproj','.sln')) for x in P.iterdir()),'Unexpected projection root')
    return {'leaves':len(actual),'bytes':sum(v['bytes'] for v in actual.values()),'inputUnchanged':True,'naturalMeta':natural,'defaultSettings':actual.get(settings['path']),'uniqueGUIDs':len(guids),'hostIoOutput':host_io_guard()}
def old_evidence():
    result={}
    for base,dirs,files in os.walk(O,followlinks=False):
        if pathlib.Path(base)==O: dirs[:]=[name for name in dirs if name not in ('projection','package-cache')]
        for name in files:
            path=pathlib.Path(base)/name; result[str(path.relative_to(O))]=ident(path)
    return result
def protection():
    actual={}
    for d in B['sharedRoots']: actual.update(inventory(R/d,R))
    same(actual,B['shared'],'Current frozen shared inputs')
    for path,value in B['frozen'].items(): check(ident(R/path)==value,'Fixed source/evidence drift '+path)
    old=old_evidence()
    same(old,B['oldEvidence'],'M03 old evidence exact leaf set');
    for path,expected in B['additionalEvidenceTrees'].items(): same(inventory(path),expected,'Fixed prior/source tree '+path)
    same(inventory(A['sourceRoot']),B['sourceEvidence'],'SOURCE ten exact leaves'); same(inventory(K),C['files'],'Frozen cache unchanged')
    for path,value in B['git'].items(): check(ident(R/path)==value,'Git HEAD/index/master changed')
    for path,value in B['oldTemporaryTrees'].items():
        current=tree_entries(path)
        if path==A['adbException']['tmpRoot']: current.pop('adb.501.log',None)
        check(current==value,'Prior temporary tree changed '+path)
    return {'sharedLeaves':len(actual),'oldEvidenceLeaves':len(old),'sourceLeaves':len(B['sourceEvidence']),'gitFrozen':True,'cacheFrozen':True,'allUnchanged':True}
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
def host_io_guard():
    root=P/'TestArtifacts/FightMatch/UGUI-01/host-io'; chain=[P/'TestArtifacts',P/'TestArtifacts/FightMatch',P/'TestArtifacts/FightMatch/UGUI-01',root]
    for i,p in enumerate(chain):
        check(stat.S_ISDIR(p.lstat().st_mode),'Host output parent must be directory '+str(p))
        if i<3: check({x.name for x in p.iterdir()}<=({chain[i+1].name,'native-scene-reopen-001'} if i==2 else {chain[i+1].name}),'Unexpected host output sibling '+str(p))
    same(tree_entries(root),B['hostIoEntries'],'Old51 HostRig exact trees')
    parent=pathlib.Path(A['isolationRoot']).parent; old=B['isolationParentEntries']; current=tree_entries(parent) if parent.exists() else {}; gid=A['hostSaveIsolation']['activationId']
    same({k:v for k,v in current.items() if pathlib.PurePosixPath(k).parts[0]!=gid},old,'Prior isolation trees')
    if isolation_created and not restored:
        d=pathlib.Path(A['isolationRoot']); check({x.name for x in d.iterdir()}=={'activation.json','host-save'},'Isolation root files')
        check(ident(d/'activation.json')==A['isolationActivationIdentity'],'Isolation activation unchanged')
        check({x.name for x in (d/'host-save').iterdir()}=={'owner.lock','persistent'} and ident(d/'host-save/owner.lock')=={'bytes':0,'sha256':hashlib.sha256(b'').hexdigest()},'Isolation lock topology')
        check((d/'host-save/persistent').is_dir() and not (d/'host-save/persistent').is_symlink() and not list((d/'host-save/persistent').iterdir()),'Isolation persistent must stay empty')
    return {'oldGuidDirectories':len(B['hostIoGuids']),'newHostRigDirectories':0,'isolationCreated':isolation_created,'persistentEmpty':True}
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
def resources():
    ev=evidence_paths(); eb=sum(p.lstat().st_size for p in ev); cb=size(K)[0]; tb,tc=size(TMP); gb=sum(size(P/n)[0] for n in ['Library','Temp','Logs','UserSettings','obj'])+sum(p.stat().st_size for p in P.iterdir() if p.is_file() and p.suffix in ('.csproj','.sln')); free=shutil.disk_usage(E).free
    dynamic={'archive/source/'+p for p in N['overwritten']+N['newPaths']+A['allowedNaturalMeta']}|{'restore/source/'+p for p in N['overwritten']}
    check(all(evidence_name(p) in allowed|dynamic or evidence_name(p) in {'archive/isolation/activation.json','archive/isolation/host-save/owner.lock','archive/SceneTemplateSettings.json'} for p in ev),'Evidence closure exceeded')
    check(eb<LIMIT['evidenceBytes']-1048576 and all(p.lstat().st_size<LIMIT['perLogBytes'] for p in ev if p.suffix=='.log'),'Evidence/log budget')
    check(cb<=LIMIT['newCacheBytes'] and gb<=LIMIT['generatedBytes'] and tb<=LIMIT['testTemporaryBytes'] and tc<=LIMIT['testTemporaryLeaves'] and free>=LIMIT['minimumFreeBytes'],'Storage budget')
    check(not any(p.name.startswith('FightMatch-') for p in TMP.iterdir()),'No test temporary roots in observation stage')
    obs=E/'P/observations.json'
    if obs.exists(): check(obs.stat().st_size<=1048576,'Observation budget')
    check(time.monotonic()-clock_start+PREF['mechanicalPreparationSeconds']<=600,'Total execution budget')
    return {'evidenceBytes':eb,'cacheBytes':cb,'generatedBytes':gb,'tmpBytes':tb,'tmpLeaves':tc,'freeBytes':free}
def preflight(sid):
    check(ident(E/'activation.json')['sha256']==ACT_SHA and ident(__file__)==A['runner'],'Activation/runner seal')
    check(OWNER==A['executionOwner'] and R==pathlib.Path(A['projectRoot']) and P==pathlib.Path(A['cwd']),'Bound owner/project')
    for path,value in A['frozenReferences'].items(): check(ident(path)==value,'Preparation/input drift '+path)
    no_links(TMP); ts=TMP.stat(); check(TMP.resolve()==TMP and len(os.fsencode(TMP))<=40 and ts.st_uid==os.getuid() and stat.S_IMODE(ts.st_mode)==0o700 and ts.st_ino==PREF['tmp']['inode'] and ts.st_dev==PREF['tmp']['device'] and ts.st_birthtime==PREF['tmp']['birthtime'],'Short owned tmp identity')
    check(ident(A['editor']['path'])=={k:A['editor'][k] for k in ('bytes','sha256')},'Editor identity')
    check(len(N['files'])==1016 and N['base1014CanonicalSha256']=='e4caecf42b39e97d6acc517ae20c0dc1a6d97ffab0bd72b0a675b1cfbddb9050' and A['executionCandidateSha256']==hashlib.sha256((N['base1014CanonicalSha256']+'\n'+A['probeSourceSha256']+'\n').encode()).hexdigest(),'Exact candidate/probe digest')
    check([(s['id'],s['timeoutSeconds'],s['maxRuns']) for s in A['stages']]==[('P',240,1)] and all(x not in A['stages'][0]['argv'] for x in ['-quit','-nographics','-runTests']),'One graphical probe stage limit')
    consumer_guard(ps()); no_links(P)
    check(not os.path.lexists(P/'Temp/UnityLockfile'),'Projection lock present')
    return {'utc':utc(),'projection':projection_guard(False),'protection':protection(),'resources':resources()}
def dlls(): return {p.name:ident(p) for p in (P/'Library/ScriptAssemblies').glob('*.dll')}
def compile_evidence(sid,log):
    current=dlls(); previous=B['dlls']; lines=[{'line':i+1,'text':line} for i,line in enumerate(log.splitlines()) if 'Csc ' in line]
    rows={name:{'before':previous.get(name),'after':value,'actualCsc':any(name in row['text'] for row in lines),'changed':value!=previous.get(name)} for name,value in current.items()}
    proof={'sourceAssemblyBindings':N['sourceAssemblyBindings'],'sourceInputs':N['sources'],'before':previous,'after':current,'events':lines,'assemblies':rows,'inputsUnchanged':True}; stage_dlls[sid]=proof
    return proof
def run_stage(s):
    global active,root_launch_epoch
    sid=s['id']; check(not (M/sid).exists(),'Stage already exists '+sid); (M/sid).mkdir()
    check(not os.path.lexists(P/'Temp/UnityLockfile'),'Project lock before '+sid); result={'stage':sid,'status':'NOT_RUN','runCount':0,'pid':None,'exitCode':None,'argv':s['argv'],'cwd':str(P),'environmentOverrides':A['environmentOverrides'],'runner':ident(__file__)}; problem=None; rootpid=None
    try:
        check(synced,'Input delta not applied'); preflight(sid); start=time.monotonic(); result['startedUtc']=utc()
        with (M/sid/'launcher.log').open('xb') as log:
            root_launch_epoch=time.time()
            active=subprocess.Popen(s['argv'],cwd=P,env={**os.environ,**A['environmentOverrides']},stdout=log,stderr=subprocess.STDOUT,start_new_session=True)
            rootpid=active.pid; result.update(pid=rootpid,runCount=1,status='RUNNING'); stages.append(result); rows=ps()
            check(rootpid in rows,'Editor missing at launch'); register(rootpid,rows[rootpid],sid,rootpid); check(owned[rootpid]['exe']==A['editor']['path'] and owned[rootpid]['cwd']==str(P),'Launched Editor identity mismatch'); event('stage_started',stage=sid,pid=rootpid,argv=s['argv'],cwd=str(P))
            lastguard=start; lastprogress=start
            while active.poll() is None:
                rows=ps(); discover(rows,sid,rootpid); now=time.monotonic(); text=(M/sid/'editor.log').read_text(errors='replace') if (M/sid/'editor.log').exists() else ''
                net=[l for l in text.splitlines() if re.search(r'(?:Downloading|downloaded|fetching).*https?://|(?:Package Manager|UPM).*(?:unable to|failed to|error).*?(?:resolve|connect|registry|network)|ENOTFOUND|ETIMEDOUT',l,re.I)]
                check(not net,'Cache/network dependency: '+json.dumps(net[-5:])); check(not re.search(r'error CS\d+|Compilation failed|Scripts have compiler errors|Failed to load.*assembly|Could not load.*assembly',text,re.I),'Compiler/domain error during '+sid)
                check(now-start<s['timeoutSeconds'],'Stage timeout '+sid)
                monitor('running',strict=True)
                if now-lastprogress>=30: print(json.dumps({'stage':sid,'pid':rootpid,'elapsedSeconds':round(now-start,1),'state':'RUNNING'}),flush=True); lastprogress=now
                time.sleep(.25)
        result['editorSeconds']=round(time.monotonic()-start,6)
    except BaseException as ex: problem=str(ex)
    finally:
        if rootpid is not None:
            try: closure(sid,rootpid,problem or 'Editor exited')
            except BaseException as ex: problem=(problem+'; ' if problem else '')+str(ex)
            if monitor_errors and problem is None: problem=monitor_errors[0]['error']
            result['exitCode']=active.poll(); result['elapsedSecondsIncludingClosure']=round(time.monotonic()-start,6); result['finishedUtc']=utc()
    if result not in stages: stages.append(result)
    try: result['observations']=observe_probe()
    except BaseException as ex: problem=(problem+'; ' if problem else '')+'Probe observation: '+str(ex)
    try:
        result['compilation']=compile_evidence(sid,(M/sid/'editor.log').read_text(errors='replace') if (M/sid/'editor.log').exists() else '')
    except BaseException as ex: problem=(problem+'; ' if problem else '')+'Compile evidence: '+str(ex)
    try:
        check(problem is None,problem or 'unknown'); check(result['exitCode']==0,'Editor exit '+str(result['exitCode']))
        log=(M/sid/'editor.log').read_text(errors='replace'); net=[l for l in log.splitlines() if re.search(r'(?:Downloading|downloaded|fetching).*https?://|(?:Package Manager|UPM).*(?:unable to|failed to|error).*?(?:resolve|connect|registry|network)|ENOTFOUND|ETIMEDOUT',l,re.I)]; check(not net,'Cache/network dependency: '+json.dumps(net[-5:])); errors=[l for l in log.splitlines() if re.search(r'error CS\d+|Compilation failed|Scripts have compiler errors|Failed to load.*assembly|Could not load.*assembly|Aborting batchmode due to failure',l,re.I)]
        check(not errors,'Compiler/domain failure: '+json.dumps(errors[-10:])); result['projection']=projection_guard(True); result['protection']=protection(); result['resources']=resources()
        result['compilation']=compile_evidence(sid,log)
        c=result['compilation']['assemblies'].get('FightMatch.Host.Editor.dll',{}); check(c.get('actualCsc') and c.get('changed'),'New Host.Editor actual Csc/changed DLL')
        check(result['observations'] is not None,'Probe did not produce observations')
        check(not monitor_errors,'Continuous monitor failures'); check(not observation.get('firstError') and observation.get('cleanupLogErrors')==0 and observation.get('gameCleanupVerified') and observation.get('sceneCleanupVerified') and observation.get('diskUnchanged') and observation.get('prefabUnchanged'),'Probe actual cleanup failed')
        result['status']='PASSED'
    except BaseException as ex: problem=(problem+'; ' if problem else '')+str(ex); result['status']='FAILED' if rootpid else 'NOT_RUN_BLOCKED'
    result['failure']=problem; write(sid+'/result.json',result); event('stage_completed',stage=sid,status=result['status'],exitCode=result['exitCode']); print(json.dumps({'stage':sid,'status':result['status'],'exitCode':result['exitCode'],'failure':problem}),flush=True); active=None
    check(problem is None,problem or 'stage failed')
def transfer(source,target,expected,move=False):
    no_links(source); check(ident(source)==expected,'Transfer source drift '+str(source)); no_links(target.parent); target.parent.mkdir(parents=True,exist_ok=True); check(not os.path.lexists(target),'Archive target already exists')
    if move: source.rename(target)
    else:
        with target.open('xb') as f: f.write(source.read_bytes())
    check(ident(target)==expected,'Transfer byte verification'); event('transfer',source=str(source),target=str(target),identity=expected,moved=move)
def synchronize():
    global synced,candidate_pending,isolation_created
    consumer_guard(ps()); check(not os.path.lexists(P/'Temp/UnityLockfile'),'No projection lock'); projection_guard()
    for path in N['overwritten']: check(ident(E/'restore/source'/path)==B['restoreBaseline'][path] and ident(P/path)==B['restoreBaseline'][path],'Eight exact preimages')
    for path in N['newPaths']: check(not os.path.lexists(P/path),'New path must be absent '+path)
    for path in N['overwritten']+N['newPaths']:
        source=pathlib.Path(N['sources'][path]); check(ident(source)==N['files'][path],'Frozen source '+path); target=P/path; target.parent.mkdir(parents=True,exist_ok=True)
        with target.open('wb' if path in N['overwritten'] else 'xb') as f: f.write(source.read_bytes())
        check(ident(target)==N['files'][path],'Synced bytes '+path); synchronized_paths.append(path); candidate_pending=True; event('source_synchronized',path=path,identity=N['files'][path])
    synced=True; candidate_pending=True
    d=pathlib.Path(A['isolationRoot']); no_links(d); check(not os.path.lexists(d),'Fresh isolation ID')
    (d/'host-save/persistent').mkdir(parents=True); (d/'activation.json').write_bytes(A['isolationActivationJson'].encode())
    with (d/'host-save/owner.lock').open('xb'): pass
    isolation_created=True; projection_guard()
def observe_probe():
    global observation,full_inputs
    path=E/'P/observations.json'
    if not path.exists(): return None
    check(ident(path)['bytes']<=1048576,'Probe JSON budget'); obs=json.loads(path.read_bytes()); observation=obs
    check(obs['ownerTurn']==EXECUTION_TURN and obs['nonce']==A['nonce'] and obs['activationSha256']==ACT_SHA and obs['executionCandidateSha256']==A['executionCandidateSha256'],'Probe identity')
    check(obs['sourceSha256']==A['probeSourceSha256'] and obs['assemblySha256']==ident(P/'Library/ScriptAssemblies/FightMatch.Host.Editor.dll')['sha256'],'Probe source/DLL binding')
    check(all(obs[k] in ['PASS','FAIL','UNOBSERVED'] for k in ['A','B','C','D']) and obs['normalInteraction']=='LOC_BLOCKED','Four independent verdicts')
    check(obs['opens']<=1 and obs['playEntries']<=1 and obs['playExits']<=1 and obs['leaseProbes']<=1 and obs['raycastCalls']<=2 and all(obs[k]==0 for k in ['saves','reopens','exports']) and len(obs['samples'])<=20,'Probe execution counts')
    full_inputs={}
    for sub in ['Assets','Packages','ProjectSettings']:
        for path,value in inventory(P/sub,P).items():
            data=(P/path).read_bytes(); full_inputs[path]=dict(value,gitBlob=hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest())
    digest=hashlib.sha256(b''.join(path.encode()+b'\0'+value['gitBlob'].encode()+b'\0'+str(value['bytes']).encode()+b'\n' for path,value in sorted(full_inputs.items()))).hexdigest()
    check(digest==obs['fullInputCanonicalSha256'],'Probe full imported candidate identity')
    return obs
def archive_and_restore():
    global restored
    consumer_guard(ps()); check(not any(alive(ps(),pid) for pid in owned),'Owned process remains before restore')
    report={'candidate':{},'restored':[],'removedNew':[],'complete':False,'isolation':None}
    if candidate_pending:
        projection_guard(False); paths=N['overwritten']+N['newPaths']+[x for x in A['allowedNaturalMeta'] if x not in N['newPaths'] and (P/x).exists()]
        for path in paths:
            value=ident(P/path); check(path not in N['files'] or value==N['files'][path],'Candidate input drift')
            transfer(P/path,E/'archive/source'/path,value,move=path not in N['overwritten']); report['candidate'][path]=value
            if path not in N['overwritten']: report['removedNew'].append(path)
        for path in N['overwritten']:
            check(ident(P/path)==N['files'][path] and ident(E/'restore/source'/path)==B['restoreBaseline'][path],'Restore exact before/after identity '+path)
            (P/path).write_bytes((E/'restore/source'/path).read_bytes()); check(ident(P/path)==B['restoreBaseline'][path],'Restore byte verification'); report['restored'].append(path)
        settings=A['allowedNewSettings']
        if settings['path'] not in B['restoreBaseline'] and (P/settings['path']).exists(): transfer(P/settings['path'],E/'archive/SceneTemplateSettings.json',{k:settings[k] for k in ('bytes','sha256')},True)
        if isolation_created:
            source=pathlib.Path(A['isolationRoot']); entries=tree_entries(source); target=E/'archive/isolation'; check(not target.exists(),'Isolation archive absent'); source.rename(target); check(tree_entries(target)==entries,'Isolation archive bytes'); report['isolation']=entries
    restored=True; report['projection']=projection_guard(); protection(); report['complete']=True; report['finalProjectionLeaves']=len(B['restoreBaseline']); report['isolationParentRetained']=pathlib.Path(A['isolationRoot']).parent.exists()
    write('restore.json',report); return report
startedUtc=utc(); remaining=None; restore_report=None
try:
    check(sum(bool(s.strip()) for s in pathlib.Path(__file__).read_text().splitlines())<=450,'Runner line budget'); check(not (E/'process-events.jsonl').exists(),'Execution already used')
    check(not list(TMP.iterdir()),'New TMP initially empty'); baseline_processes=ps(); event('process_baseline',rows=baseline_processes,sdkAdb=A['sdkAdb']); event('preflight_begin',owner=OWNER); preflight('P'); synchronize()
    for stage in A['stages']: run_stage(stage)
except BaseException as ex: failure=str(ex)
finally:
    after={'projectionLockPresentAfter':os.path.lexists(P/'Temp/UnityLockfile'),'utc':utc(),'owner':OWNER,'checks':{},'differences':[],'globalGitFrozen':True}
    for name,fn in [('projection',lambda:projection_guard(False)),('protection',protection),('resources',resources)]:
        try: after['checks'][name]=fn()
        except BaseException as ex: after['differences'].append({'scope':name,'error':str(ex)}); failure=(failure+'; ' if failure else '')+str(ex)
    try:
        rows=ps(); remaining=[dict(owned[pid],current=rows[pid]) for pid in owned if alive(rows,pid)]; check(not remaining,'Owned closure'); restore_report=archive_and_restore()
    except BaseException as ex: failure=(failure+'; ' if failure else '')+'Archive/restore: '+str(ex)
    write('compile.json',{'stages':stage_dlls,'DLLsAfter':dlls(),'sourceAssemblyBindings':N['sourceAssemblyBindings'],'fullImportedInputs':full_inputs,'naturalMetaHistory':natural_history,'noProductAcceptance':True})
    after['restore']=restore_report
    for key,fn in [('temporaryTree',lambda:tree_entries(TMP)),('finalResources',resources)]:
        try: after[key]=fn()
        except BaseException as ex: after['differences'].append({'scope':key,'error':str(ex)}); failure=(failure+'; ' if failure else '')+str(ex)
    write('after.json',after)
    write('process-after.json',{'sdkAdbObservations':sdk_observations,'sdkAdbBinding':sdk_adb,'processBaseline':baseline_processes,'monitorCycles':monitor_cycles,'monitorErrors':monitor_errors,'adbObservations':adb_observations,'owner':OWNER,'owned':list(owned.values()),'remainingOwned':remaining,'UnityStarts':sum(s['runCount'] for s in stages),'signals':[pid for pid in owned if owned[pid]['termSent']],'sigkill':False})
    status='FAILED' if failure is not None or observation is None else observation['status']
    receipt={'projectionLockPresentAfter':os.path.lexists(P/'Temp/UnityLockfile'),'task':A['task'],'status':status,'owner':OWNER,'issuer':A['issuer'],'centralActivation':A['centralActivation'],'startedUtc':startedUtc,'finishedUtc':utc(),'activation':ident(E/'activation.json'),'runner':ident(__file__),'stages':stages,'failure':failure,'restore':restore_report,'toolCleanup':{k:observation.get(k) for k in ['gameCleanupVerified','sceneCleanupVerified','cleanupLogErrors','diskUnchanged','prefabUnchanged','firstError','secondaryErrors']} if observation else None,'monitorErrors':monitor_errors,'fourObservations':{k:observation[k] for k in ['A','B','C','D','normalInteraction']} if observation else None,'candidate':A['candidate'],'evidence':{evidence_name(p):evidence_identity(p) for p in evidence_paths() if p.name!='receipt.json'},'remainingOwned':remaining,'soleCompletionReceiver':A['soleCompletionReceiver'],'realPlayerSaveReadOrWrite':False,'authorIntegrationVerdict':None,'unrun':A['unrun'],'downloadsRequested':0,'networkBytesMeasured':False,'writingStoppedAfterReceipt':True}
    write('receipt.json',receipt); print(json.dumps({'status':receipt['status'],'failure':failure,'stages':[{'stage':s['stage'],'status':s['status'],'runCount':s['runCount'],'exitCode':s['exitCode']} for s in stages],'receipt':ident(E/'receipt.json'),'remainingOwned':remaining}),flush=True)
sys.exit(0 if failure is None else 1)
