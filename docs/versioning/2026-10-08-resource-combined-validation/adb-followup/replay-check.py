#!/usr/bin/env python3
"""Bounded offline SDK lifecycle replay; all process, FD and log data are synthetic."""
import ast,copy,datetime,hashlib,io,json,os,pathlib,re,shlex,stat,subprocess,sys,time,traceback,types
START=time.monotonic();E=pathlib.Path(__file__).parent;R=E.parents[3];SOURCE=str(E/'runner.py');forbidden=[];traces=set()
def audit(name,args):
    if name in {'subprocess.Popen','os.system','os.kill','os.killpg','socket.connect','socket.bind','os.rename','os.mkdir','os.remove','os.rmdir','os.symlink','ctypes.dlopen','ctypes.dlsym'}:
        forbidden.append(name);raise RuntimeError('Offline boundary '+name)
    if name=='open' and isinstance(args[0],(str,bytes,os.PathLike)) and args[2]&(os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        if pathlib.Path(args[0])!=E/'replay-results.json':forbidden.append('write '+str(args[0]));raise RuntimeError('Offline write boundary')
sys.addaudithook(audit)
def need(ok,why):
    if not ok:raise AssertionError(why)
def identity(path):
    raw=path.read_bytes();return {'bytes':len(raw),'sha256':hashlib.sha256(raw).hexdigest()}
def profile(frame,event,arg):
    if event=='call' and frame.f_code.co_filename==SOURCE:traces.add(frame.f_code.co_name)
fixture=json.loads((E/'fixture.json').read_text());F=fixture['synthetic'];history=json.loads((E/'replay-results.json').read_text())
need(len(history['rounds'])<2 and not any(r['status']=='SOURCE_REPLAY_PASS' for r in history['rounds']),'At most two rounds; first green stops')
functions={n.name:n for n in ast.parse((E/'runner.py').read_text()).body if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
selected=['check','NaturalGraceExpired','ChildArgsProbeError','IdentityProbeIncomplete','identity_args','identity_sample','identity_event','child_args','details','sdk_adb_exception','probe_timeout','blocking_probe_timeout','no_links','cmd','ps','alive','consumer_guard','isolate_stage']
module=compile(ast.fix_missing_locations(ast.Module(body=[functions[n] for n in selected],type_ignores=[])),SOURCE,'exec')
report={'round':len(history['rounds'])+1,'status':'SOURCE_REPLAY_FAILED','cases':[],'nativeRuns':0,'realProbes':0,'realSignals':0,'realSockets':0,'productWrites':0,'rawHistoricalTIdentityKnown':False}
def epoch(row):return time.mktime(time.strptime(row['start'],'%a %b %d %H:%M:%S %Y'))
def rows(*items):return {r['pid']:copy.deepcopy(r) for r in (F['observer'],)+items}
def fixture_env():
    ctx={'mode':'normal','snapshot':rows(F['I']),'queue':[],'calls':[],'events':[],'logPid':101,'argsCalls':{},'fdCalls':0}
    log=F['logPath'];sdk=F['sdkPath'];data=F['sdkBytes'].encode();sdkid={'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}
    class VPath(pathlib.PurePosixPath):
        def resolve(self):return self
        def is_symlink(self):return False
        def lstat(self):
            need(str(self)==log,'Only fixed synthetic log metadata')
            return types.SimpleNamespace(st_mode=stat.S_IFREG|0o600,st_uid=F['uid'],st_dev=F['logDevice'],st_ino=F['logInode'])
        def open(self):
            need(str(self)==log,'Only synthetic startup log')
            pid=101 if ctx['mode']=='old-log' else ctx['logPid'];row=F['I'] if pid==101 else F['T'];stamp=time.strftime('%m-%d %H:%M:%S',time.localtime(epoch(row)))
            return io.StringIO('--- adb starting (pid '+str(pid)+') ---\nInstalled as '+sdk+'\n'+stamp+'\n')
    def run(argv,**kw):
        mode=ctx['mode'];ctx['calls'].append({'argv':list(argv),'timeout':kw['timeout']})
        if argv==['/bin/ps','-ww','-axo','pid=,ppid=,lstart=,stat=,comm=']:
            if mode in ('snapshot-io','doubleempty-query-failure'):raise PermissionError('synthetic fresh query denied')
            if mode=='snapshot-timeout':raise subprocess.TimeoutExpired(argv,kw['timeout'])
            snapshot=ctx['queue'].pop(0) if ctx['queue'] else ctx['snapshot'];out=''.join(str(pid)+' '+str(row['ppid'])+' '+row['start']+' '+row['stat']+' '+row['exe']+'\n' for pid,row in snapshot.items())
            if mode=='snapshot-malformed':out+='bad row\n'
            if mode=='snapshot-truncated':out=out.rstrip('\n')
            return types.SimpleNamespace(returncode=2 if mode=='snapshot-exit' else 0,stdout=out,stderr='denied' if mode=='snapshot-stderr' else '')
        pid=int(argv[argv.index('-p')+1]);is_old=pid==101;row=F['I'] if is_old else F['T']
        if argv==['/bin/ps','-p',str(pid),'-o','uid=']:
            return types.SimpleNamespace(returncode=0,stdout=str(999 if mode=='wrong-uid' else F['uid'])+'\n',stderr='')
        if argv==['/bin/ps','-ww','-p',str(pid),'-o','args=']:
            ctx['argsCalls'][pid]=ctx['argsCalls'].get(pid,0)+1
            if mode=='args-io':raise PermissionError('synthetic args denied')
            if mode=='args-timeout':raise subprocess.TimeoutExpired(argv,kw['timeout'])
            if mode.startswith('doubleempty') and is_old:return types.SimpleNamespace(returncode=1,stdout='',stderr='')
            code=2 if mode=='args-exit2' else 0;out='adb -L tcp:5037 fork-server server --reply-fd 7\n'
            if mode=='args-stderr':return types.SimpleNamespace(returncode=1,stdout='',stderr='denied')
            if mode=='args-empty0':out=''
            if mode=='invalid-argv':out='adb devices\n'
            if mode=='late-argv' and ctx['argsCalls'][pid]>1:out='adb -L tcp:5037 fork-server server --reply-fd 8\n'
            return types.SimpleNamespace(returncode=code,stdout=out,stderr='')
        if argv==['/usr/sbin/lsof','-a','-p',str(pid),'-d','cwd','-Fn']:
            empty=mode.startswith('doubleempty') and is_old
            return types.SimpleNamespace(returncode=1 if empty else 0,stdout='' if empty else 'p'+str(pid)+'\nfcwd\nn/P\n',stderr='')
        need(argv==['/usr/sbin/lsof','-nP','-p',str(pid),'-FpcftnDi'],'Only fixed SDK FD query')
        ctx['fdCalls']+=1
        if mode=='fd-query-error':return types.SimpleNamespace(returncode=1,stdout='',stderr='denied')
        raw='p'+str(pid)+'\n'
        if mode!='missing-mapped':raw+='ftxt\ntREG\nn'+sdk+'\ni10\nD0x7\n'
        raw+='f1\ntREG\nn'+log+'\ni20\nD0x7\n'
        if mode!='missing-log-fd':raw+='f2\ntREG\nn'+log+'\ni20\nD0x7\n'
        if mode=='source-fd':raw+='f3\ntREG\nn/P/Assets/Source.cs\ni30\nD0x7\n'
        if mode=='fd-owner':raw=raw.replace('p'+str(pid)+'\n','p999\n',1)
        return types.SimpleNamespace(returncode=0,stdout=raw,stderr='')
    env={'pathlib':types.SimpleNamespace(Path=VPath),'time':types.SimpleNamespace(monotonic=lambda:10.,mktime=time.mktime,strptime=time.strptime,time=lambda:epoch(F['T'])+20,strftime=time.strftime,localtime=time.localtime),'os':types.SimpleNamespace(getuid=lambda:F['uid']),'stat':stat,'re':re,'shlex':shlex,'json':json,'hashlib':hashlib,'copy':copy,'subprocess':types.SimpleNamespace(run=run,TimeoutExpired=subprocess.TimeoutExpired),'utc':lambda:'synthetic-snapshot-time','D':{'evidenceSlots':{'perLogBytes':8388608},'beeObservationContract':{'rawBytesPerStream':8192}},'A':{'sdkAdb':{'path':sdk,'device':7,'inode':10,**sdkid},'adbException':{'pid':999,'tmpRoot':'/OLDTMP','priorCompilerPipe':'oldpipe'}},'sdk_adb':None,'root_launch_epoch':epoch(F['I'])-2,'baseline_processes':{},'owned':{},'pending_details':{},'sdk_observations':[],'TMP':VPath('/TMP'),'P':VPath('/P'),'K':VPath('/K'),'R':VPath('/R'),'adb_exception':lambda snapshot:999,'event':lambda kind,**kw:ctx['events'].append(copy.deepcopy({'kind':kind,**kw})),'ident':lambda path:({'bytes':0,'sha256':'bad'} if ctx['mode']=='wrong-sdk-hash' else sdkid),'current_stage':'I','snapshot_root':100,'probe_deadline':None,'natural_boundary':None,'execution_deadline':None,'work_deadline':None,'stages':[{'stage':'I','status':'COMPILE_PASS'}],'closure_closed':True,'active':None,'monitor_errors':[],'monitor_cycles':[],'stage_dlls':{'I':{}},'compilation_passed':lambda proof:True,'stage_history':[],'launched_root':None}
    exec(module,env);return env,ctx

def case(mode):
    env,ctx=fixture_env();initial=rows(F['I'])
    need(env['sdk_adb_exception'](initial)=={101},'Synthetic I binding verified through original checks')
    old=copy.deepcopy(env['sdk_adb']);need(old['pid']==101 and not env['owned'],'SDK binding never grants owned identity')
    ctx['calls']=[];ctx['argsCalls']={};ctx['events']=[];ctx['fdCalls']=0
    if mode=='stage-only':
        env['isolate_stage']('T');need(env['sdk_adb']==old and env['current_stage']=='T','Actual isolate_stage preserves live SDK binding')
        need(not any(e['kind']=='sdk_adb_retired' for e in ctx['events']),'Stage switch alone does not retire');record(mode,env,ctx,None);return
    env['current_stage']='T';env['snapshot_root']=200;ctx['logPid']=202
    supplied=rows(F['T']);ctx['snapshot']=rows(F['T']);env['root_launch_epoch']=epoch(F['T'])-2
    if mode=='retire-only':supplied=rows();ctx['snapshot']=rows()
    if mode=='old-live-new':supplied=rows(F['I'],F['T']);ctx['snapshot']=copy.deepcopy(supplied)
    if mode=='two-new':supplied=rows(F['T'],F['extra']);ctx['snapshot']=copy.deepcopy(supplied)
    if mode in ('same-pid-start','same-pid-exe','bound-zombie'):
        changed=copy.deepcopy(F['I'])
        if mode=='same-pid-start':changed['start']=F['T']['start']
        if mode=='same-pid-exe':changed['exe']='/foreign/process'
        if mode=='bound-zombie':changed['stat']='Z'
        supplied=rows(changed);ctx['snapshot']=copy.deepcopy(supplied)
    if mode in ('confirmation-live','confirmation-zombie','confirmation-reused'):
        changed=copy.deepcopy(F['I'])
        if mode=='confirmation-zombie':changed['stat']='Z'
        if mode=='confirmation-reused':changed['start']=F['T']['start']
        ctx['snapshot']=rows(changed,F['T'])
    if mode.startswith('doubleempty') or mode.startswith('args-'):
        supplied=rows(F['I']);ctx['logPid']=101;env['root_launch_epoch']=epoch(F['I'])-2
        ctx['mode']=mode
        if mode=='doubleempty-absent':ctx['snapshot']=rows()
        elif mode=='doubleempty-rebind':ctx['snapshot']=rows(F['T']);ctx['logPid']=202
        elif mode=='doubleempty-zombie':ctx['snapshot']=rows({**F['I'],'stat':'Z'})
        else:ctx['snapshot']=rows(F['I'])
    elif mode.startswith('snapshot-'):ctx['mode']=mode
    elif mode in ('missing-mapped','missing-log-fd','old-log','wrong-uid','wrong-sdk-hash','source-fd','invalid-argv','late-argv','fd-owner','fd-query-error'):ctx['mode']=mode
    if mode=='baseline-candidate':env['baseline_processes']={202:F['T']}
    if mode=='start-before-launch':env['root_launch_epoch']=epoch(F['T'])+5
    if mode in ('late-reuse','late-zombie','late-two'):
        later=rows({**F['T'],'start':F['extra']['start']}) if mode=='late-reuse' else (rows({**F['T'],'stat':'Z'}) if mode=='late-zombie' else rows(F['T'],F['extra']))
        ctx['queue']=[rows(F['T']),later]
    caught=None;result=None
    try:result=env['sdk_adb_exception'](supplied)
    except Exception as error:caught=error
    success=mode in ('rebind','retire-only','doubleempty-absent','doubleempty-rebind')
    retired=[e for e in ctx['events'] if e['kind']=='sdk_adb_retired']
    if success:
        need(caught is None,'Expected lifecycle success '+mode+': '+str(caught));need(len(retired)==1 and retired[0]['observation']['identity']==old,'Exactly one evidenced retirement retains old binding')
        if mode in ('retire-only','doubleempty-absent'):need(env['sdk_adb'] is None and result==set(),'No current binding after proved exit')
        else:
            need(env['sdk_adb']['pid']==202 and result=={202} and ctx['fdCalls']==1,'New synthetic candidate independently fully verified')
            verified=next(e['observation'] for e in ctx['events'] if e['kind']=='sdk_adb_verified')
            need('p202\n' in verified['rawFDs'] and 'pid 202' in verified['startupHeader'] and verified['identity']!=old,'New FD/log evidence replaces no historical data')
        need(set(supplied)==set(ctx['snapshot']),'Confirmed snapshot propagates to caller')
    else:
        need(caught is not None,'Negative case must fail '+mode)
        need(env['sdk_adb'] is None or env['sdk_adb']==old,'Rejected candidate never creates a new binding')
        if mode in ('old-live-new','same-pid-start','same-pid-exe','bound-zombie','confirmation-live','confirmation-zombie','confirmation-reused','doubleempty-live','doubleempty-zombie','doubleempty-query-failure') or mode.startswith('snapshot-') or mode.startswith('args-'):
            need(env['sdk_adb']==old and not retired,'Failed absence/query/present identity cannot retire binding')
        if mode in ('old-live-new','two-new'):need(not ctx['fdCalls'],'Candidate cardinality rejected before grants')
    need(not env['owned'],'No SDK signal authority')
    first=next(e for e in ctx['events'] if e['kind']=='sdk_adb_candidates')['observation']
    need(first['stage']=='T' and first['rootPid']==200 and first['oldBinding']==old and 'boundPidAbsent' in first and 'snapshotMonotonic' in first,'Decision diagnostic saved before acceptance/rejection')
    record(mode,env,ctx,caught)
def record(mode,env,ctx,error):
    report['cases'].append({'name':mode,'passed':True,'synthetic':True,'errorType':None if error is None else type(error).__name__,'error':None if error is None else str(error),'bindingAfter':copy.deepcopy(env['sdk_adb']),'ownedPids':sorted(env['owned']),'calls':ctx['calls'],'events':ctx['events']})

try:
    compile((E/'runner.py').read_text(),SOURCE,'exec')
    need(identity(E/'runner.py')==history['frozenSource']['runner'] and identity(E/'replay-check.py')==history['frozenSource']['checker'] and identity(E/'fixture.json')==history['frozenSource']['fixture'],'Frozen replay inputs')
    for name,seal in fixture['baselineReferences'].items():need(identity(R/fixture['baselineRoot']/name)==seal,'Original evidence unchanged '+name)
    need(len(fixture['historical']['mismatchFailures'])==14 and not fixture['historical']['TActualCandidateIdentityKnown'] and fixture['historical']['nativeStatus']=='FAILED','Historical failure remains with unknown T candidate')
    passed={c['name'] for r in history['rounds'] for c in r['cases'] if c['passed']}
    sys.setprofile(profile)
    for name in history['selectedCases']:
        if name in passed:continue
        need(time.monotonic()-START<30 and time.monotonic()-START+sum(r['seconds'] for r in history['rounds'])<60,'Replay time budget')
        report['activeCase']=name;case(name)
    need(not forbidden,'No forbidden operations');report['status']='SOURCE_REPLAY_PASS'
except BaseException as error:
    report['failure']=str(error);report['traceback']=traceback.format_exc()
finally:
    sys.setprofile(None);report['seconds']=time.monotonic()-START;report['actualFunctionsCalled']=sorted(traces);report['forbiddenAttempts']=forbidden
    report['runner']=identity(E/'runner.py');report['checker']=identity(E/'replay-check.py');report['fixture']=identity(E/'fixture.json')
    if report['seconds']>30 or sum(r['seconds'] for r in history['rounds'])+report['seconds']>60:report['status']='SOURCE_REPLAY_FAILED';report['failure']='Replay budget exceeded'
    history['rounds'].append(report);history['status']=report['status'];history['cumulativeSeconds']=sum(r['seconds'] for r in history['rounds'])
    with (E/'replay-results.json').open('w') as stream:json.dump(history,stream,ensure_ascii=False,indent=2);stream.write('\n')
    print(json.dumps({'status':report['status'],'round':report['round'],'passedCases':len(report['cases']),'seconds':report['seconds'],'cumulativeSeconds':history['cumulativeSeconds'],'failure':report.get('failure'),'activeCase':report.get('activeCase'),'forbiddenAttempts':forbidden}))
sys.exit(0 if report['status']=='SOURCE_REPLAY_PASS' else 1)
