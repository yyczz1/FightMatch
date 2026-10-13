#!/opt/homebrew/bin/python3
"""One bounded FIX09 delta run. Memory files are STATIC/SIMULATED, never native restore."""
import time
START=time.monotonic()
import signal
def timeout(*args):raise TimeoutError('FIX09 static invocation exceeded 10 seconds')
signal.signal(signal.SIGALRM,timeout)
signal.setitimer(signal.ITIMER_REAL,10-(time.monotonic()-START))
import ast,copy,json,os,runpy,sys
from pathlib import Path
ROOT=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
E=ROOT/'TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix09'
DRIVER=E/'source/m17_driver.py'
CHECKS=E/'authority/offline-checks.json'
writing=False
def guard(event,args):
    if event in ('subprocess.Popen','os.system','os.fork','os.posix_spawn') or event.startswith('socket.'):
        raise RuntimeError('SOURCE-only static execution')
    if event=='open' and isinstance(args[2],int) and args[2]&(os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        if not writing or Path(args[0])!=CHECKS:raise RuntimeError('static write outside checks leaf')
sys.addaudithook(guard)
d=runpy.run_path(str(DRIVER),run_name='fix09_static_subject')
oldraw=d['read'](d['OLD17F8']/'source/m17_driver.py')
old=runpy.run_path(str(d['OLD17F8']/'source/m17_driver.py'),run_name='fix08_readonly_subject')
cache={}
def reader(p):
    p=Path(p)
    if p not in cache:cache[p]=d['read'](p)
    return cache[p]
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
    except Exception as e:error=type(e).__name__+': '+str(e)
    cases.append({'name':name,'passed':error is None,'rawError':error,'kind':'STATIC/SIMULATED'})
phase='M-runner-generation'
fm,ff,fi,_=d['restore_fixture'](phase)
root=d['phase_root'](phase)
pkg=fm['packages'][0]
archivepath=root/'env/packages'/pkg['id'].lower()/pkg['version']/(pkg['key']+'.nupkg')
sidepath=Path(str(archivepath)+'.sha512')
metapath=archivepath.parent/'.nupkg.metadata'
memory={}
for project,pair in ff.items():
    parent=root/'source'/Path(project).parent
    memory[parent/'packages.lock.json'],memory[parent/'obj/project.assets.json']=pair
memory[archivepath],memory[sidepath],memory[metapath]=fi['yamldotnet/16.3.0']
content=d['strict'](memory[metapath])['contentHash']
def fsread(data,p):
    if p not in data:raise FileNotFoundError(str(p))
    return data[p]
def validate(data=memory,model=fm,ph=phase):
    files,installed=d['read_restore_inputs'](model,ph,'runner',lambda p:fsread(data,p),lambda p:p in data)
    return d['validate_restore'](model,ph,'runner',files,installed)
case('three real filename semantics; contentHash differs from archive SHA512',lambda:(
    equal(len(fi['yamldotnet/16.3.0']),3),
    equal(memory[sidepath].decode(),pkg['sha512']),
    equal(content!=pkg['sha512'],True),validate()))
def old_red():
    # Compile the exact F8 production filesystem read statements, with only a
    # fake Path at the filesystem boundary. No validator or tuple is replaced.
    tree=ast.parse(oldraw);fn=next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='execute_material')
    loop=next(n for n in fn.body if isinstance(n,ast.For) and isinstance(n.target,ast.Tuple) and
        [x.id for x in n.target.elts]==['phase','graph'])
    start=next(i for i,n in enumerate(loop.body) if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='files' for t in n.targets))
    end=next(i for i,n in enumerate(loop.body) if isinstance(n,ast.Assign) and any(isinstance(t,ast.Subscript) and isinstance(t.value,ast.Name) and t.value.id=='restored' for t in n.targets))
    class MemoryPath(type(Path())):
        def is_file(self):return Path(self) in memory
    ns=dict(old,Path=MemoryPath,root=MemoryPath(root),m=fm,graph='runner',reader=lambda p:fsread(memory,Path(p)))
    exec(compile(ast.Module(body=loop.body[start:end],type_ignores=[]),'<F8 exact production read>','exec'),ns)
    equal(len(ns['installed']['yamldotnet/16.3.0']),2)
    reject(lambda:old['validate_restore'](fm,phase,'runner',ns['files'],ns['installed']),'lock NuGet contentHash mismatch')
case('M11 A != B shape: F8 actual reader RED; FIX09 reader GREEN',lambda:(old_red(),validate()))
for p,label in ((archivepath,'archive'),(sidepath,'archive sidecar'),(metapath,'NuGet metadata')):
    def missing(p=p):
        data=dict(memory);del data[p];reject(lambda:validate(data))
    def corrupt(p=p):
        data=dict(memory);data[p]=b'corrupt';reject(lambda:validate(data))
    case('missing '+label,missing);case('damaged '+label,corrupt)
for label,value in (('empty',b''),('non-object',b'[]'),('duplicate key',b'{"version":2,"version":2}'),
    ('invalid UTF8',b'\xff'),('bad JSON',b'{')):
    case('metadata '+label,lambda value=value:reject(lambda:validate(dict(memory,**{})|{metapath:value})))
for key,value,label in (
    ('version',1,'wrong version'),('version',True,'boolean version'),('source',str(d['W7']/'feed'),'stale source'),
    ('source','https://api.nuget.org/v3/index.json','network source'),('contentHash','!','non Base64'),
    ('contentHash','YQ==','not 64 bytes'),('contentHash',42,'non-string hash')):
    def bad(key=key,value=value):
        meta=d['strict'](memory[metapath]);meta[key]=value
        reject(lambda:validate(memory|{metapath:d['jb'](meta)}))
    case('metadata '+label,bad)
for key in ('version','source','contentHash'):
    def absent(key=key):
        meta=d['strict'](memory[metapath]);del meta[key];reject(lambda:validate(memory|{metapath:d['jb'](meta)}))
    case('metadata absent '+key,absent)
def mutate_json(path,change):
    data=dict(memory);v=d['strict'](data[path]);change(v);data[path]=d['jb'](v)
    reject(lambda:validate(data))
project=fm['graphs']['runner']['root'];parent=root/'source'/Path(project).parent
lp=parent/'packages.lock.json';ap=parent/'obj/project.assets.json'
case('lock differs from metadata',lambda:mutate_json(lp,lambda v:v['dependencies']['net8.0']['YamlDotNet'].update(contentHash=pkg['sha512'])))
case('assets differs from metadata',lambda:mutate_json(ap,lambda v:v['libraries']['YamlDotNet/16.3.0'].update(sha512=pkg['sha512'])))
case('closed package ID/version',lambda:mutate_json(lp,lambda v:v['dependencies']['net8.0']['YamlDotNet'].update(resolved='99.0.0')))
case('closed target package set',lambda:mutate_json(ap,lambda v:v['targets']['net8.0'].clear()))
case('closed project edges',lambda:mutate_json(ap,lambda v:v['project']['restore']['frameworks']['net8.0']['projectReferences'].clear()))
case('locked mode required',lambda:mutate_json(ap,lambda v:v['project']['restore']['restoreLockProperties'].update(restoreLockedMode=True)))
case('fixed NuGet source',lambda:mutate_json(ap,lambda v:v['project']['restore'].update(sources={'https://example.invalid':{}})))
case('all lock/assets files required',lambda:reject(lambda:d['validate_restore'](fm,phase,'runner',{},fi)))
case('valid locked verification fixture',lambda:(lambda t:d['validate_restore'](t[0],'M-tests-verification','runner',t[1],t[2]))(d['restore_fixture']('M-tests-verification')))
protected=d['_fix05_protection'](reader)
m=d['model'](reader,partial=d['_review01_partial'](reader))
new=reader(DRIVER)
delta=d['_fix09_delta'](oldraw,new)
prior=d['strict'](reader(d['OLD17F8']/'authority/prepare.json'))['model']
def rebase(v):
    if isinstance(v,str):
        for a,b in ((d['OLD17F8'],d['E']),(old['M'],d['M']),(old['W'],d['W'])):v=v.replace(str(a),str(b))
        return v
    if isinstance(v,list):return [rebase(x) for x in v]
    if isinstance(v,dict):return {k:rebase(x) for k,x in v.items()}
    return v
for key in ('commands','packages','archive','graphs','licenses','sources','writes'):
    case('unchanged closed production model '+key,lambda key=key:equal(m[key],rebase(prior[key])))
def budgets():
    expected=old['execution_budget'](prior);expected['driverMaxBytes']=286720
    equal(d['execution_budget'](m),expected)
    equal(d['TRUSTED_CENTRAL_ACTIVATION'],None);equal(d['download_contract'](m),[])
    for name in ('execution_budget','seal_source','_review02_receipt_content'):
        node=next(n for n in ast.parse(new).body if isinstance(n,ast.FunctionDef) and n.name==name)
        if '280*1024' not in ast.get_source_segment(new.decode(),node):raise AssertionError('missing current cap')
    equal(d['_review02_authority']()['packetSha256'],'4f1b779b3bb1a2627007eca45b2bffed1aa1670ad6c4eaffaede8ffeb7a9ad8a')
    equal(str(d['REVIEW02_SIDECAR']),str(ROOT/'docs/team/2026-09-30/loc-m17-fix09-command-receipt.json'))
case('only current driver budget changed; null activation; mandatory sidecar',budgets)
case('new roots remain absent',d['_fix05_roots_absent'])
case('graph write restrictions retained',lambda:d['_fix06_validate_rules'](m))
case('backup preserved before canonical replacement',d['_fix09_budget_receipt'])
case('source scope and 280KiB cap',lambda:equal(delta['driverMaxBytes'],286720))
# Existing sidecar field validator is exercised through its complete synthetic
# public receipt object, not by replacing gate functions.
osrows=m['osSourceEvidence'];osresult=d['strict'](reader(d['FIX07_OS']/'probe-result.json'))
def sidefixture():
    f=d['_review02_fixture']();s=f['sidecar'];mm=f['model'];mm['osSourceEvidence']=osrows
    checks=d['strict'](f['files'][E/'authority/offline-checks.json'])
    checks.update(osEvidence=osrows,osChildStarts=0)
    dc=d['strict'](checks['executionReceipt']['directedTool']['output']['text'].encode());dc.update(osEvidence=osrows,osChildStarts=0)
    checks['executionReceipt']['directedTool']['output']['text']=json.dumps(dc)+'\n'
    f['files'][E/'authority/offline-checks.json']=d['jb'](checks)
    seal=d['strict'](f['frozen']);seal['modelSha256']=d['sha'](d['jb'](mm))
    seal['filesExcludingThisSeal']=[d['identity'](p,b) for p,b in sorted(f['files'].items())]
    f['frozen']=d['jb'](seal);fid=d['identity'](E/'authority/source-freeze.json',f['frozen'])
    leaves=seal['filesExcludingThisSeal']+[fid];total=sum(x['bytes'] for x in leaves)
    result,clock=[json.loads(x) for x in s['tool']['output']['text'].splitlines()]
    result.update(modelSha256=seal['modelSha256'],freeze=fid,leaves=leaves,totalBytes=total)
    s['tool']['output']['text']=json.dumps(result)+'\n'+json.dumps(clock)+'\n'
    s.update(sourceFreeze=fid,sourceFreezeUtf8=f['frozen'].decode(),sourceModelSha256=seal['modelSha256'],eightLeaves=leaves,totalSourceBytes=total)
    s['directedVerification']['evidence']=d['identity'](E/'authority/offline-checks.json',f['files'][E/'authority/offline-checks.json'])
    s['osVerification']={'evidence':osrows,'records':osresult['records'],'toolId':'exec-4edf9488-4e3e-4c64-8627-a36ae7e16de9',
        'newChildStartsThisDelivery':0,'reused':True,'newParentsOSValidated':False}
    return f
def sidecheck(change=None):
    f=sidefixture()
    if change:change(f['sidecar'])
    return d['_review02_receipt_content'](f['sidecar'],f['frozen'],f['model'],lambda p:f['files'][p] if p in f['files'] else reader(p))
case('complete mandatory sidecar succeeds only as synthetic evidence',sidecheck)
for label,change in (
    ('missing tool',lambda s:s.pop('tool')),
    ('failure exit',lambda s:s['tool'].update(exitCode=1)),
    ('truncated output',lambda s:s['tool']['output'].update(truncated=True)),
    ('wrong source',lambda s:s.update(sourceModelSha256='0'*64)),
    ('wrong authority',lambda s:s['authority'].update(packetSha256='0'*64)),
    ('self acceptance',lambda s:s.update(integrationVerdict='ACCEPT')),
    ('material authorization',lambda s:s.update(materialExecutionAuthorized=True)),
    ('wrong OS tool',lambda s:s['osVerification'].update(toolId='exec-00000000-0000-0000-0000-000000000000'))):
    case('sidecar rejects '+label,lambda change=change:reject(lambda:sidecheck(change)))
elapsed=time.monotonic()-START
passed=sum(c['passed'] for c in cases)
status='DIRECTED_CHECKS_PASSED' if passed==len(cases) else 'DIRECTED_CHECKS_FAILED'
report={'status':status,'attempts':1,'elapsedSeconds':elapsed,'caseCount':len(cases),'passed':passed,'cases':cases,
    'sourceIdentity':d['identity'](DRIVER,new),'sourceDelta':delta,'modelSha256':d['sha'](d['jb'](m)),
    'coreModelSha256':d['_fix05_core_digest'](m),'protectedBefore':protected,'osEvidence':osrows,'osChildStarts':0,
    'reusedOSChildStarts':3,'newParentsOSValidated':False,'SDKScan':False,'partialNewEnumerations':0,'GETs':0,
    'restores':0,'materialExecution':False,'historicalSuitesRerun':False,'scope':'STATIC/SIMULATED; native0'}
writing=True
with CHECKS.open('xb') as stream:stream.write(d['jb'](report))
writing=False
full=time.monotonic()-START
clock={'event':'FINAL_COMMAND_CLOCK','scope':'FIX09 static delta once','status':status,'attempts':1,
    'maxSeconds':10,'fullInvocationSeconds':full,'caseCount':len(cases),'passed':passed,'exitCode':0 if passed==len(cases) else 20,
    'sourceIdentity':report['sourceIdentity'],'osEvidence':osrows,'osChildStarts':0,
    'failures':[c for c in cases if not c['passed']]}
print(json.dumps(clock),flush=True)
sys.exit(clock['exitCode'])

