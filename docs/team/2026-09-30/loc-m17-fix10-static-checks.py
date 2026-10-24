#!/opt/homebrew/bin/python3
"""FIX10 single delta run: actual packageFolders field + STATIC/SIMULATED restore fixtures."""
import time
START=time.monotonic()
import signal
def timeout(*args):raise TimeoutError('FIX10 static invocation exceeded 10 seconds')
signal.signal(signal.SIGALRM,timeout)
signal.setitimer(signal.ITIMER_REAL,10-(time.monotonic()-START))
import ast,copy,json,os,runpy,sys
from pathlib import Path
ROOT=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
BASE=ROOT/'TestArtifacts/FightMatch/LOC-LIC-ALT-M'
E=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix10'
OLD=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix09'
DRIVER=E/'source/m17_driver.py';CHECKS=E/'authority/offline-checks.json'
writing=False
def guard(event,args):
    if event in ('subprocess.Popen','os.system','os.fork','os.posix_spawn') or event.startswith('socket.'):
        raise RuntimeError('SOURCE-only static execution')
    if event=='open' and isinstance(args[2],int) and args[2]&(os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        if not writing or Path(args[0])!=CHECKS:raise RuntimeError('static write outside checks leaf')
sys.addaudithook(guard)
d=runpy.run_path(str(DRIVER),run_name='fix10_subject')
old=runpy.run_path(str(OLD/'source/m17_driver.py'),run_name='fix09_readonly_subject')
cache={}
def reader(p):
    p=Path(p)
    if p not in cache:cache[p]=d['read'](p)
    return cache[p]
asset=old['W']/'phases/M-runner-generation/source/src/Luban.Bson/obj/project.assets.json'
raw=d['check_bytes'](reader(asset),(17921,'fbf1498a9719bffdb0e6ed9fc05288bee25b1a49fa4fd4e5744eb5b14a2ec477'),'actual M12 Bson assets')
actual=d['strict'](raw);actual_id=d['identity'](asset,raw)
oldP=str(old['W']/'phases/M-runner-generation/env/packages')
if actual['packageFolders']!={oldP:{}}:raise AssertionError('actual field differs from fixed diagnosis')
if actual['project']['restore']['packagesPath']!=oldP:raise AssertionError('actual packagesPath')
if set(actual['project']['restore']['sources'])!={str(old['W']/'feed')}:raise AssertionError('actual feed')
if 'fallbackFolders' in actual['project']['restore']:raise AssertionError('actual unexpected fallback field')
cases=[]
def equal(a,b):
    if a!=b:raise AssertionError('values differ')
def reject(fn,fragment=None):
    try:fn()
    except (d['GateError'],old['GateError'],OSError,KeyError,ValueError,TypeError) as e:
        if fragment is not None and fragment not in str(e):raise AssertionError(str(e))
        return
    raise AssertionError('expected rejection')
def case(name,fn):
    error=None
    try:fn()
    except TimeoutError:raise
    except Exception as e:error=type(e).__name__+': '+str(e)
    cases.append({'name':name,'passed':error is None,'rawError':error,'kind':'STATIC/SIMULATED'})
phase='M-runner-generation';P=str(d['phase_root'](phase)/'env/packages')
m,files,installed,_=d['restore_fixture'](phase)
def with_folders(ff,folders,module=d):
    out={}
    for project,(lock,b) in ff.items():
        a=module['strict'](b);a['packageFolders']=copy.deepcopy(folders);out[project]=(lock,module['jb'](a))
    return out
def validate(folders=None,ph=phase):
    mm,ff,ii,_=d['restore_fixture'](ph)
    if folders is not None:ff=with_folders(ff,folders)
    return d['validate_restore'](mm,ph,'runner',ff,ii)
def old_red():
    mm,ff,ii,_=old['restore_fixture'](phase)
    ff=with_folders(ff,actual['packageFolders'],old)
    reject(lambda:old['validate_restore'](mm,phase,'runner',ff,ii),'unexpected fallback package folder')
case('exact F9 validator rejects fixed M12 no-slash field',old_red)
case('F10 accepts actual no-slash field with explicit W9-to-W10 fixture projection',
    lambda:validate({P:copy.deepcopy(actual['packageFolders'][oldP])}))
case('fixture defaults to actual no-slash form',lambda:(
    equal([d['strict'](b)['packageFolders'] for _,b in files.values()],[{P:{}}]*len(files)),validate()))
case('one trailing slash remains accepted',lambda:validate({P+'/':{}}))
for ph in ('M-runner-generation','M-tests-verification','B-pass-1'):
    for slash in ('','/'):
        case(ph+' singleton '+repr(slash),lambda ph=ph,slash=slash:validate({str(d['phase_root'](ph)/'env/packages')+slash:{}},ph))
bad={
    'empty':{},'two aliases':{P:{},P+'/':{}},'extra directory':{P:{},P+'-other':{}},
    'sibling':{P+'-other':{}},'parent':{str(Path(P).parent):{}},'child':{P+'/child':{}},
    'cross phase':{str(d['phase_root']('M-tests-verification')/'env/packages'):{}},
    'relative':{'env/packages':{}},'dot':{P+'/.':{}},'dotdot':{P+'/../packages':{}},
    'embedded dot':{P.replace('/env/','/env/./'):{}},
    'embedded duplicate separator':{P.replace('/env/','/env//'):{}},
    'double trailing slash':{P+'//':{}},'triple trailing slash':{P+'///':{}},
    'case change':{P.upper():{}},'URI':{'file://'+P:{}},'backslashes':{P.replace('/','\\'):{}},
    'old phase root':{oldP:{}},'empty key':{'':{}},
    'non-object list':[P],'non-object string':P,'non-object null':None,'non-object number':1,
}
for label,folders in bad.items():
    # None is an invalid field here, not validate()'s default.
    case('reject '+label,lambda folders=folders:reject(
        lambda:d['validate_restore'](m,phase,'runner',with_folders(files,folders),installed)))
project=m['graphs']['runner']['root']
def assets_change(change):
    ff=dict(files);a=d['strict'](ff[project][1]);change(a);ff[project]=(ff[project][0],d['jb'](a))
    reject(lambda:d['validate_restore'](m,phase,'runner',ff,installed))
case('packagesPath exactness unchanged',lambda:assets_change(lambda a:a['project']['restore'].update(packagesPath=P+'/')))
case('source exactness unchanged',lambda:assets_change(lambda a:a['project']['restore'].update(sources={str(d['W']/'feed')+'/':{}})))
case('locked-mode gate unchanged',lambda:assets_change(lambda a:a['project']['restore']['restoreLockProperties'].update(restoreLockedMode=True)))
case('assets contentHash gate unchanged',lambda:assets_change(lambda a:a['libraries']['YamlDotNet/16.3.0'].update(sha512=m['packages'][0]['sha512'])))
def metadata_change(key,value):
    ii=dict(installed);archive,side,meta=ii['yamldotnet/16.3.0'];meta=d['strict'](meta);meta[key]=value
    ii['yamldotnet/16.3.0']=(archive,side,d['jb'](meta))
    reject(lambda:d['validate_restore'](m,phase,'runner',files,ii))
case('metadata contentHash remains separate from archive SHA512',lambda:metadata_change('contentHash',m['packages'][0]['sha512']))
case('metadata source remains fixed',lambda:metadata_change('source',str(old['W']/'feed')))
def sidecar_damage():
    ii=dict(installed);a,b,c=ii['yamldotnet/16.3.0'];ii['yamldotnet/16.3.0']=(a,b'wrong',c)
    reject(lambda:d['validate_restore'](m,phase,'runner',files,ii))
case('archive sidecar integrity retained',sidecar_damage)
oldraw=reader(OLD/'source/m17_driver.py');new=reader(DRIVER)
delta=d['_fix09_delta'](oldraw,new)
def exact_production_delta():
    a=ast.get_source_segment(oldraw.decode(),next(n for n in ast.parse(oldraw).body if isinstance(n,ast.FunctionDef) and n.name=='validate_restore'))
    b=ast.get_source_segment(new.decode(),next(n for n in ast.parse(new).body if isinstance(n,ast.FunctionDef) and n.name=='validate_restore'))
    before="require(set(assets['packageFolders'])=={str(root/'env/packages')+'/'},'unexpected fallback package folder')"
    after="require(type(assets['packageFolders']) is dict and set(assets['packageFolders']) in ({str(root/'env/packages')},{str(root/'env/packages')+'/'}),'unexpected fallback package folder')"
    equal(a.count(before),1);equal(a.replace(before,after),b)
case('only exact singleton gate changed in production validator',exact_production_delta)
prior=d['strict'](reader(OLD/'authority/prepare.json'))['model']
replay={'kind':'REPLAY / existing M12 output; not a new restore','status':'NOT_RUN',
    'sourceRecords':[],'nativeRestoresStarted':0,'productionPinChanged':False,
    'namespaceOverride':{'W':str(old['W'])},'partialEnumeration':False}
def replay_first_stage():
    replay['status']='RUNNING';seen={}
    ns={'__name__':'fix10_isolated_readonly_replay','__file__':str(DRIVER)}
    exec(compile(new,str(DRIVER),'exec'),ns)
    ns['W']=old['W']
    def tracked(p):
        p=Path(p);b=reader(p);seen[str(p)]=d['identity'](p,b);return b
    try:
        ff,ii=ns['read_restore_inputs'](prior,phase,'runner',tracked)
        replay.update(projectFilesRead=len(ff),installedPackageRecordsRead=len(ii))
        result=ns['validate_restore'](prior,phase,'runner',ff,ii)
        equal(ns['TRUSTED_CENTRAL_ACTIVATION'],None);equal(d['TRUSTED_CENTRAL_ACTIVATION'],None)
        replay.update(status='PASSED',validatedProjects=len(result['projects']),validatedPackages=result['packages'])
    except Exception as error:
        replay.update(status='FAILED',rawError=type(error).__name__+': '+str(error));raise
    finally:
        replay['sourceRecords']=[seen[p] for p in sorted(seen)]
case('complete production reader and validator replay all existing M12 runner graph bytes',replay_first_stage)
protected=d['_fix05_protection'](reader)
model=d['model'](reader,partial=d['_review01_partial'](reader))
def rebase(v):
    if isinstance(v,str):
        for a,b in ((OLD,E),(old['M'],d['M']),(old['W'],d['W'])):v=v.replace(str(a),str(b))
        return v
    if isinstance(v,list):return [rebase(x) for x in v]
    if isinstance(v,dict):return {k:rebase(x) for k,x in v.items()}
    return v
for key in ('commands','packages','archive','graphs','licenses','sources','writes'):
    case('closed model unchanged '+key,lambda key=key:equal(model[key],rebase(prior[key])))
case('all resource budgets unchanged',lambda:equal(d['execution_budget'](model),old['execution_budget'](prior)))
def preservation():
    oldchecks=d['strict'](reader(OLD/'authority/offline-checks.json'))
    wanted={r['path']:(r['bytes'],r['sha256']) for r in oldchecks['protectedBefore']}
    current={r['path']:(r['bytes'],r['sha256']) for r in protected}
    equal({p:current[p] for p in wanted},wanted)
    equal(current[str(asset)],(actual_id['bytes'],actual_id['sha256']))
    equal(len(new)<=286720,True);equal(d['TRUSTED_CENTRAL_ACTIVATION'],None)
    equal(d['download_contract'](model),[])
    equal(str(d['REVIEW02_SIDECAR']),str(ROOT/'docs/team/2026-09-30/loc-m17-fix10-command-receipt.json'))
    d['_fix09_budget_receipt']();d['_fix05_roots_absent']()
case('all F9 protected records retained; input pinned; closed SOURCE gates',preservation)
elapsed=time.monotonic()-START;passed=sum(c['passed'] for c in cases)
status='DIRECTED_CHECKS_PASSED' if passed==len(cases) else 'DIRECTED_CHECKS_FAILED'
report={'status':status,'attempts':1,'elapsedSeconds':elapsed,'caseCount':len(cases),'passed':passed,'cases':cases,
    'sourceIdentity':d['identity'](DRIVER,new),'sourceDelta':delta,'modelSha256':d['sha'](d['jb'](model)),
    'coreModelSha256':d['_fix05_core_digest'](model),'protectedBefore':protected,'actualInput':actual_id,
    'actualField':actual['packageFolders'],'fixtureProjection':'Only fixed packageFolders field rebased W9 to W10; all other fixture bytes synthetic.',
    'firstStageReplay':replay,
    'osEvidence':model['osSourceEvidence'],'osChildStarts':0,'reusedOSChildStarts':3,'newParentsOSValidated':False,
    'SDKScan':False,'partialNewEnumerations':0,'GETs':0,'restores':0,'materialExecution':False,
    'historicalSuitesRerun':False,'scope':'STATIC/SIMULATED; native0'}
writing=True
with CHECKS.open('xb') as stream:stream.write(d['jb'](report))
writing=False
full=time.monotonic()-START
clock={'event':'FINAL_COMMAND_CLOCK','scope':'FIX10 static delta once','status':status,'attempts':1,'maxSeconds':10,
    'fullInvocationSeconds':full,'caseCount':len(cases),'passed':passed,'exitCode':0 if passed==len(cases) else 20,
    'sourceIdentity':report['sourceIdentity'],'actualInput':actual_id,'osEvidence':report['osEvidence'],'osChildStarts':0,
    'failures':[c for c in cases if not c['passed']]}
print(json.dumps(clock),flush=True)
sys.exit(clock['exitCode'])
