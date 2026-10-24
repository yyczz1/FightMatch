#!/opt/homebrew/bin/python3
"""FIX11: one source-only regression invocation; no native children or old suites."""
import time
START=time.monotonic()
import signal
def timeout(*args):raise TimeoutError('FIX11 static invocation exceeded 10 seconds')
signal.signal(signal.SIGALRM,timeout)
signal.setitimer(signal.ITIMER_REAL,10-(time.monotonic()-START))
import ast,copy,errno,hashlib,json,os,runpy,shutil,stat,sys,types
from pathlib import Path
ROOT=Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
BASE=ROOT/'TestArtifacts/FightMatch/LOC-LIC-ALT-M'
E=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix11'
OLD=BASE/'loc-lic-alt-yamldotnet-16.3.0-m17-fix10'
DRIVER=E/'source/m17_driver.py';CHECKS=E/'authority/offline-checks.json'
writing=False
def guard(event,args):
    if event in ('subprocess.Popen','os.system','os.fork','os.posix_spawn') or event.startswith('socket.'):
        raise RuntimeError('SOURCE-only static invocation')
    if event=='open' and isinstance(args[2],int) and args[2]&(os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):
        if not writing or Path(args[0])!=CHECKS:raise RuntimeError('static write outside checks leaf')
sys.addaudithook(guard)
d=runpy.run_path(str(DRIVER),run_name='fix11_subject')
new=DRIVER.read_bytes();old=OLD.joinpath('source/m17_driver.py').read_bytes()
tree=ast.parse(new);oldtree=ast.parse(old)
cases=[]
def equal(a,b):
    if a!=b:raise AssertionError('values differ: '+repr(a)+' != '+repr(b))
def reject(fn):
    try:fn()
    except (d['GateError'],OSError):return
    raise AssertionError('expected rejection')
def case(name,fn):
    error=None
    try:fn()
    except TimeoutError:raise
    except Exception as e:error=type(e).__name__+': '+str(e)
    cases.append({'name':name,'passed':error is None,'rawError':error,'kind':'STATIC/SIMULATED'})
# Pre-agreed production seams; only filesystem metadata and free-space are simulated.
class MemoryPath(type(Path())):
    fs=None
    def lstat(self):
        f=self.fs;k=str(self);f.calls[k]=f.calls.get(k,0)+1
        effect=f.effects.pop(k,None)
        if effect=='before':f.nodes.pop(k,None)
        if effect=='parent':
            f.nodes.pop(str(self.parent),None);f.nodes.pop(k,None)
        if isinstance(effect,Exception):raise effect
        if k not in f.nodes:raise FileNotFoundError(errno.ENOENT,'fixture absent',k)
        value=types.SimpleNamespace(**f.nodes[k])
        if effect=='after':f.nodes.pop(k)
        return value
    def is_symlink(self):
        value=self.fs.nodes.get(str(self));return value is not None and stat.S_ISLNK(value['st_mode'])
    def iterdir(self):
        if str(self) not in self.fs.nodes:raise FileNotFoundError(errno.ENOENT,'fixture missing directory',str(self))
        if not stat.S_ISDIR(self.fs.nodes[str(self)]['st_mode']):raise NotADirectoryError(str(self))
        return iter([MemoryPath(p) for p in list(self.fs.nodes) if MemoryPath(p).parent==self and MemoryPath(p)!=self])
class MemoryFS:
    def __init__(self):
        self.nodes={};self.effects={};self.calls={};MemoryPath.fs=self
        self.directory('/work');self.directory('/material')
    def directory(self,path):
        p=MemoryPath(path)
        for q in (p,*p.parents):self.nodes.setdefault(str(q),{'st_mode':stat.S_IFDIR,'st_size':0,'st_nlink':1})
        return p
    def leaf(self,path,size=7,mode=stat.S_IFREG,nlink=1):
        p=MemoryPath(path);self.directory(p.parent)
        self.nodes[str(p)]={'st_mode':mode,'st_size':size,'st_nlink':nlink};return p
    def subject(self):
        ns=dict(d,Path=MemoryPath,M=MemoryPath('/material'),W=MemoryPath('/work'),
            ROOTS={'work':MemoryPath('/work'),'material':MemoryPath('/material')})
        names={'MaterialWriter','inspect_command_outputs','_fix11_output_snapshot',
            '_fix06_guid_rule','_fix06_no_links','_fix06_guid_snapshot','_fix06_inspect_guid'}
        exec(compile(ast.Module([n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef)) and n.name in names],[]),
            str(DRIVER),'exec'),ns)
        return ns
def rule(rel='phases/M-tests-generation/env/packages/fixed/1.0/{safe-leaf}',kind='bounded-pattern',count=2,size=20):
    return ['work',rel,kind,'M','M-tests-generation-restore',count,size,'fixture existing mutable output']
def fixture(effect=None,path=None):
    f=MemoryFS();r=rule();parent='/work/'+r[1].removesuffix('{safe-leaf}')
    f.directory(parent);p=f.leaf(path or parent+'qci5yr0f.zv2')
    if effect:f.effects[str(p)]=effect
    return f,f.subject(),r,p
def command_scan(effect,active=True,expect_reject=False):
    f,ns,r,p=fixture(effect)
    action=lambda:ns['inspect_command_outputs']([r],active=active)
    reject(action) if expect_reject else action()
    if not expect_reject:equal(f.calls[str(p)],1)
case('command quota permits current bounded leaf disappearing before metadata',
    lambda:command_scan('before'))
case('command quota counts observed leaf without rereading after disappearance',
    lambda:command_scan('after'))
case('command quota strict scan rejects enumerated disappearance',
    lambda:command_scan('before',False,True))

def writer_for(ns):
    m={'packages':[{'class':'existing'} for _ in range(31)],'archive':{'class':'existing'},'commands':[]}
    return ns['MaterialWriter'](m,types.SimpleNamespace(left=lambda cleanup=False:1))
def with_space(fn,free=3*1024**3):
    original=shutil.disk_usage
    shutil.disk_usage=lambda p:types.SimpleNamespace(free=free)
    try:return fn()
    finally:shutil.disk_usage=original
def aggregate(effect,active=True,expect_reject=False,path=None,size=7,free=3*1024**3):
    f,ns,r,p=fixture(effect,path);f.nodes.get(str(p),{})['st_size']=size
    action=lambda:with_space(lambda:writer_for(ns).budget(active=[r] if active else ()),free)
    reject(action) if expect_reject else action()
    if not expect_reject:equal(f.calls[str(p)],1)
for effect in ('before','after'):
    case('aggregate handles current bounded leaf '+effect+' metadata',lambda effect=effect:aggregate(effect))
case('aggregate default strict scan rejects disappearance',lambda:aggregate('before',False,True))
for path in ('/work/feed/fixed.nupkg','/work/phases/M-tests-verification/env/packages/fixed/1.0/temp','/material/fixed-input'):
    case('aggregate refuses disappearance outside current mutable rule: '+path,
        lambda path=path:aggregate('before',True,True,path))
case('aggregate observed size remains charged after leaf disappears',
    lambda:aggregate('after',True,True,size=4*1024**3+1))
case('aggregate free-space floor retained',lambda:aggregate(None,True,True,free=2*1024**3-1))
def quota(kind):
    f,ns,r,p=fixture('after' if kind=='bytes-after' else None)
    if kind=='count':f.leaf(str(p)+'2');f.leaf(str(p)+'3')
    elif kind=='bytes-after':f.nodes[str(p)]['st_size']=21
    reject(lambda:ns['inspect_command_outputs']([r],active=True))
case('command count cap retained',lambda:quota('count'))
case('command observed byte cap retained after disappearance',lambda:quota('bytes-after'))
for effect in ('parent',PermissionError(errno.EACCES,'fixture denied'),OSError(errno.EIO,'fixture IO'),
               FileNotFoundError(errno.EIO,'wrong errno despite exception type')):
    case('command rejects parent/non-ENOENT '+repr(effect),lambda effect=effect:command_scan(effect,True,True))
    case('aggregate rejects parent/non-ENOENT '+repr(effect),lambda effect=effect:aggregate(effect,True,True))
def illegal(which,mode):
    f,ns,r,p=fixture()
    target=p if which=='leaf' else (p.parent if which=='parent' else MemoryPath('/work'))
    f.nodes[str(target)]['st_mode']=mode
    action=(lambda:ns['inspect_command_outputs']([r],active=True)) if which!='root' else (lambda:with_space(lambda:writer_for(ns).budget(active=[r])))
    reject(action)
for which,mode in (('leaf',stat.S_IFLNK),('leaf',stat.S_IFIFO),('parent',stat.S_IFLNK),
                   ('root',stat.S_IFREG),('root',stat.S_IFLNK)):
    case('reject '+which+' illegal mode '+str(mode),lambda which=which,mode=mode:illegal(which,mode))
def optional_outputs():
    f=MemoryFS();ns=f.subject()
    ns['inspect_command_outputs']([rule('not-created/file','exact')])
    ns['inspect_command_outputs']([rule('not-created/{safe-leaf}')])
case('initially absent optional exact and bounded outputs retain semantics',optional_outputs)
def missing_root():
    f,ns,r,p=fixture();f.nodes.pop(str(p.parent))
    reject(lambda:ns['inspect_command_outputs']([r],active=True))
case('active mutable root missing is not a vanished leaf',missing_root)
GOOD='43195cad-a941-4061-8b31-b7e1a14bbb92.tmp'
def guid(effect=None,active=True,kind='good',location='obj'):
    f=MemoryFS();r=rule('phases/M-tests-generation/source/src/Luban.Test/'+('obj/' if location=='obj' else '')+'{guid-tmp}',
        'bounded-guid-tmp',1,16*1024**2)
    p=f.leaf('/work/'+r[1].replace('{guid-tmp}',GOOD))
    if effect:f.effects[str(p)]=effect
    if kind=='hardlink':f.nodes[str(p)]['st_nlink']=2
    if kind=='symlink':f.nodes[str(p)]['st_mode']=stat.S_IFLNK
    if kind=='directory':f.nodes[str(p)]['st_mode']=stat.S_IFDIR
    if kind=='bytes':f.nodes[str(p)]['st_size']=16*1024**2+1
    if kind=='count':f.leaf(str(p).replace(GOOD,'0'+GOOD[1:]))
    ns=f.subject();action=lambda:ns['inspect_command_outputs']([r],active=active)
    bad=kind!='good' or (effect=='before' and not active) or effect=='parent'
    reject(action) if bad else action()
    if not bad:equal(f.calls[str(p)],1)
for location in ('obj','project'):
    for effect in ('before','after'):
        case('GUID '+location+' current leaf '+effect,lambda location=location,effect=effect:guid(effect,location=location))
    case('GUID '+location+' strict disappearance',lambda location=location:guid('before',False,location=location))
    for kind in ('hardlink','symlink','directory','bytes','count'):
        case('GUID '+location+' rejects '+kind,lambda location=location,kind=kind:guid(kind=kind,location=location))
case('GUID missing parent rejected',lambda:guid('parent'))
def guid_invalid_context():
    f=MemoryFS();r=rule('phases/M-tests-generation/source/src/Luban.Test/obj/{guid-tmp}','bounded-guid-tmp',1,16*1024**2)
    f.directory('/work/'+r[1].removesuffix('/{guid-tmp}'));p=f.leaf('/work/'+r[1].replace('{guid-tmp}',GOOD.upper()))
    f.effects[str(p)]='before';ns=f.subject()
    reject(lambda:with_space(lambda:writer_for(ns).budget(active=[r])))
case('aggregate GUID eligibility does not expand leaf matcher',guid_invalid_context)
# Run the actual production AST statements at both monitored call sites.
run_node=next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name=='run_material_command')
old_run=next(n for n in oldtree.body if isinstance(n,ast.FunctionDef) and n.name=='run_material_command')
loop=next(n for n in ast.walk(run_node) if isinstance(n,ast.While) and ast.unparse(n.test)=='selector.get_map()')
monitor=next(n for n in loop.body if isinstance(n,ast.If))
outer=next(n for n in run_node.body if isinstance(n,ast.Try))
post=[n for n in outer.body if isinstance(n,ast.Expr) and isinstance(n.value,ast.Call) and
    ast.unparse(n.value.func) in ('inspect_command_outputs','writer.budget')]
def callsite(live,post_exit=False):
    f,ns,r,p=fixture('before');ns.update(rules=[r],writer=writer_for(ns),
        process=types.SimpleNamespace(poll=lambda:None if live else 0))
    statements=post if post_exit else monitor.body
    action=lambda:with_space(lambda:exec(compile(ast.Module(statements,[]),str(DRIVER),'exec'),ns))
    reject(action) if post_exit or not live else action()
case('actual monitor forwards current rules while process is live',lambda:callsite(True))
case('actual monitor becomes strict after process exit while streams drain',lambda:callsite(False))
case('actual post-exit call site stays strict',lambda:callsite(False,True))
def callsite_scope():
    equal(len(post),2)
    for n in post:equal(n.value.keywords,[])
    old_loop=next(n for n in ast.walk(old_run) if isinstance(n,ast.While) and ast.unparse(n.test)=='selector.get_map()')
    old_monitor=next(n for n in old_loop.body if isinstance(n,ast.If))
    normalized=copy.deepcopy(run_node)
    target=next(n for n in ast.walk(normalized) if isinstance(n,ast.While) and ast.unparse(n.test)=='selector.get_map()')
    next(n for n in target.body if isinstance(n,ast.If)).body=copy.deepcopy(old_monitor.body)
    equal(ast.dump(normalized,include_attributes=False),ast.dump(old_run,include_attributes=False))
    old_class=next(n for n in oldtree.body if isinstance(n,ast.ClassDef) and n.name=='MaterialWriter')
    new_class=next(n for n in tree.body if isinstance(n,ast.ClassDef) and n.name=='MaterialWriter')
    for name in ('__init__','path','put'):
        a=next(n for n in old_class.body if isinstance(n,ast.FunctionDef) and n.name==name)
        b=next(n for n in new_class.body if isinstance(n,ast.FunctionDef) and n.name==name)
        equal(ast.dump(a,include_attributes=False),ast.dump(b,include_attributes=False))
    for name in ('execute_material','consume_m_stage','authorize_write','verify_activation','execution_budget','validate_restore','consume_source','seal_source'):
        a=next(n for n in oldtree.body if isinstance(n,ast.FunctionDef) and n.name==name)
        b=next(n for n in tree.body if isinstance(n,ast.FunctionDef) and n.name==name)
        equal(ast.dump(a,include_attributes=False),ast.dump(b,include_attributes=False))
case('only active call plumbing changed; stop/timeout/no-retry/put/final protections retained',callsite_scope)

def aggregate_count():
    f=MemoryFS()
    for i in range(25001):f.leaf('/material/f'+str(i),size=0)
    ns=f.subject()
    reject(lambda:with_space(lambda:writer_for(ns).budget()))
case('aggregate material file count cap retained',aggregate_count)
cache={}
def reader(p):
    p=Path(p)
    if p not in cache:cache[p]=d['read'](p)
    return cache[p]
delta=d['_fix09_delta'](old,new)
protected=d['_fix05_protection'](reader)
model=d['model'](reader,partial=d['_review01_partial'](reader))
prior=d['strict'](reader(OLD/'authority/prepare.json'))['model']
def rebase(v):
    if isinstance(v,str):
        for a,b in ((OLD,E),(d['M'].with_name('5.1.0-fm-yamldotnet-16.3.0-m17-fix10'),d['M']),
                    (d['W'].with_name('m17-fix10'),d['W'])):v=v.replace(str(a),str(b))
        return v
    if isinstance(v,list):return [rebase(x) for x in v]
    if isinstance(v,dict):return {k:rebase(x) for k,x in v.items()}
    return v
for key in ('commands','packages','archive','graphs','licenses','sources','writes'):
    case('fixed model unchanged except candidate roots: '+key,lambda key=key:equal(model[key],rebase(prior[key])))
case('all resource caps unchanged',lambda:equal(d['execution_budget'](model),d['execution_budget'](prior)))
def preservation():
    oldchecks=d['strict'](reader(OLD/'authority/offline-checks.json'))
    wanted={r['path']:(r['bytes'],r['sha256']) for r in oldchecks['protectedBefore']}
    current={r['path']:(r['bytes'],r['sha256']) for r in protected}
    equal({p:current[p] for p in wanted},wanted)
    equal(len(new)<=286720,True);equal(d['TRUSTED_CENTRAL_ACTIVATION'],None)
    equal(d['download_contract'](model),[])
    d['_fix09_budget_receipt']();d['_fix05_roots_absent']()
    first=Path(protected[0]['path'])
    def absent(p):
        if Path(p)==first:raise FileNotFoundError(errno.ENOENT,'protected input deliberately absent')
        return reader(p)
    reject(lambda:d['_fix05_protection'](absent))
case('old protection retained; fixed input absence and closed SOURCE gates enforced',preservation)
def patch_rebuild():
    import difflib
    wanted=''.join(difflib.unified_diff(old.decode().splitlines(True),new.decode().splitlines(True),
        fromfile='M17-FIX10/source/m17_driver.py',tofile='M17-FIX11/source/m17_driver.py')).encode()
    equal((E/'source/source.patch').read_bytes(),wanted)
    names={n.name for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))}
    equal('fix08_checks' in names,False)
case('exact approved subtraction and reproducible patch',patch_rebuild)
elapsed=time.monotonic()-START;passed=sum(c['passed'] for c in cases)
status='DIRECTED_CHECKS_PASSED' if passed==len(cases) else 'DIRECTED_CHECKS_FAILED'
report={'status':status,'attempts':1,'elapsedSeconds':elapsed,'caseCount':len(cases),'passed':passed,'cases':cases,
    'sourceIdentity':d['identity'](DRIVER,new),'sourceDelta':delta,'modelSha256':d['sha'](d['jb'](model)),
    'coreModelSha256':d['_fix05_core_digest'](model),'protectedBefore':protected,
    'redEvidenceReused':True,'redRerun':False,'fixtureKind':'in-memory filesystem metadata; production functions and call-site AST',
    'osEvidence':model['osSourceEvidence'],'osChildStarts':0,'reusedOSChildStarts':3,'newParentsOSValidated':False,
    'SDKScan':False,'partialNewEnumerations':0,'GETs':0,'restores':0,'materialExecution':False,
    'historicalSuitesRerun':False,'scope':'STATIC/SIMULATED; native0'}
writing=True
with CHECKS.open('xb') as stream:stream.write(d['jb'](report))
writing=False
full=time.monotonic()-START
clock={'event':'FINAL_COMMAND_CLOCK','scope':'FIX11 static delta once','status':status,'attempts':1,'maxSeconds':10,
    'fullInvocationSeconds':full,'caseCount':len(cases),'passed':passed,'exitCode':0 if passed==len(cases) else 20,
    'sourceIdentity':report['sourceIdentity'],'osEvidence':report['osEvidence'],'osChildStarts':0,
    'failures':[c for c in cases if not c['passed']]}
print(json.dumps(clock),flush=True)
sys.exit(clock['exitCode'])
