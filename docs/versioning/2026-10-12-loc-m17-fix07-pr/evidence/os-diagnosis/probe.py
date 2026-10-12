"""One approved, bounded Seatbelt fixture experiment; never invokes the SDK."""
import ctypes
import datetime
import hashlib
import json
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parent
BASE = ROOT.parent
SOURCE = BASE / 'loc-lic-alt-yamldotnet-16.3.0-m17-fix06'
EXPECTED_PROFILE = '5f769139403d229b1ba22378f979019f9a7ac9425c305c73b5e5b7bca3abb916'
EXPECTED_DRIVER = '715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1'
GUID = r'[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\.tmp'
LEAF = '371f9ba5-1be1-4a9f-996e-d51404ed0ba5.tmp'
TARGET = ROOT / 'obj' / LEAF
ACTUAL = '01a12e4f-69be-72e2-b3ef-ca33e7c4303b'

def digest(data):
    return hashlib.sha256(data).hexdigest()

def encoded(value):
    return (json.dumps(value, ensure_ascii=False, sort_keys=True, indent=2) + '\n').encode()

def put(name, data):
    path = ROOT / name
    assert path.parent == ROOT and not path.exists()
    with path.open('xb') as stream:
        stream.write(data)

def limits():
    leaves = [p for p in ROOT.rglob('*') if p.is_file()]
    assert not any(p.is_symlink() for p in ROOT.rglob('*'))
    assert len(leaves) <= 12 and sum(p.stat().st_size for p in leaves) <= 128 * 1024
    return {'files': len(leaves), 'bytes': sum(p.stat().st_size for p in leaves)}

def reconstruct(model):
    # Exact text algorithm from the inspected FIX06 child_sandbox and GUID helper.
    roots = {k: Path(v) for k, v in model['roots'].items()}
    command = next(c for c in model['commands'] if c['id'] == 'M-runner-generation-restore')
    owners = {command['id'], command['id'].removesuffix('-restore')}
    rules = [r for r in model['writes'] if r[3] == command['stage'] and r[4] in owners]
    lines = ['(version 1)', '(allow default)', '(deny file-write*)', '(deny process-fork)']
    if command['network'] == 'OS denied; local feed only':
        lines.append('(deny network*)')
    directories = set()
    for root, rel, kind, stage, writer, files, size, purpose in rules:
        if kind == 'bounded-guid-tmp':
            parent = roots[root] / rel.removesuffix('/{guid-tmp}')
            expression = '^' + re.escape(str(parent)) + '/' + GUID + '$'
            lines.append('(allow file-write* (regex ' + json.dumps(expression) + '))')
        else:
            p = roots[root] / rel.removesuffix('{safe-leaf}')
            if kind == 'exact':
                lines.append('(allow file-write* (literal ' + json.dumps(str(p)) + '))')
                parent = p.parent
            else:
                assert kind == 'bounded-pattern'
                lines.append('(allow file-write* (regex ' + json.dumps('^' + re.escape(str(p).rstrip('/')) + '/') + '))')
                parent = p
        while parent.is_relative_to(roots[root]):
            directories.add(str(parent))
            parent = parent.parent
    for directory in sorted(directories):
        lines.append('(allow file-write* (require-all (vnode-type DIRECTORY) (literal ' + json.dumps(directory) + ')))')
    return '\n'.join(lines), rules

def fixture_profile(expression=None, raw=False, directory=True):
    lines = ['(version 1)', '(allow default)', '(deny file-write*)', '(deny process-fork)', '(deny network*)']
    literal = '(literal ' + json.dumps(str(TARGET)) + ')'
    if expression is None:
        lines.append('(allow file-write* ' + literal + ')')
    else:
        # The literal intersection restricts even a faulty regex to ONE new leaf.
        value = ('#' if raw else '') + json.dumps(expression)
        lines.append('(allow file-write* (require-all ' + literal + ' (regex ' + value + ')))')
    if directory:
        lines.append('(allow file-write* (require-all (vnode-type DIRECTORY) (literal ' + json.dumps(str(TARGET.parent)) + ')))')
    return '\n'.join(lines)

CHILD = r'''
import ctypes, errno, json, os, signal, sys, time
began=time.monotonic()
payload=json.loads(sys.stdin.read())
signal.setitimer(signal.ITIMER_REAL, 1.5)
lib=ctypes.CDLL('/usr/lib/libsandbox.dylib',use_errno=True)
lib.sandbox_init.argtypes=[ctypes.c_char_p,ctypes.c_uint64,ctypes.POINTER(ctypes.c_char_p)]
lib.sandbox_init.restype=ctypes.c_int
error=ctypes.c_char_p(); ctypes.set_errno(0)
result=lib.sandbox_init(payload['profile'].encode(),0,ctypes.byref(error))
initial_errno=ctypes.get_errno()
record={'case':payload['id'],'sandboxReturn':result,'sandboxErrno':initial_errno,
 'sandboxError':None if not error.value else error.value.decode(errors='replace'),
 'openErrno':None,'writeErrno':None,'cleanupErrno':None,'writtenBytes':0,'pid':os.getpid()}
code=77
if result==0:
 fd=None
 try:
  fd=os.open(payload['target'],os.O_WRONLY|os.O_CREAT|os.O_EXCL,0o600)
 except OSError as exc:
  record['openErrno']=exc.errno; record['openError']=str(exc); code=1
 else:
  try:
   record['writtenBytes']=os.write(fd,b'x'); code=0
  except OSError as exc:
   record['writeErrno']=exc.errno; record['writeError']=str(exc); code=2
  finally:
   os.close(fd)
  try:
   os.unlink(payload['target'])
  except OSError as exc:
   record['cleanupErrno']=exc.errno; record['cleanupError']=str(exc); code=3
record['elapsedSeconds']=time.monotonic()-began
print(json.dumps(record),flush=True)
raise SystemExit(code)
'''

def prepare():
    assert ROOT.name == 'loc-m08-sandbox-diagnosis-01'
    assert {p.name for p in ROOT.iterdir()} == {'probe.py'}
    assert not any(p.is_symlink() for p in (ROOT, *ROOT.parents))
    driver = (SOURCE / 'source/m17_driver.py').read_bytes()
    assert len(driver) == 236347 and digest(driver) == EXPECTED_DRIVER
    prepared_bytes = (SOURCE / 'authority/prepare.json').read_bytes()
    prepared = json.loads(prepared_bytes)
    profile, rules = reconstruct(prepared['model'])
    assert digest(profile.encode()) == EXPECTED_PROFILE, 'actual production profile identity mismatch; no native probes'
    rule = next(r for r in rules if r[1].endswith('/Luban.DataLoader.Builtin/obj/{guid-tmp}'))
    original_parent = Path(prepared['model']['roots']['work']) / rule[1].removesuffix('/{guid-tmp}')
    expression = '^' + re.escape(str(TARGET.parent)) + '/' + GUID + '$'
    expanded = re.sub(r'\[0-9a-f\]\{(\d+)\}', lambda m: '[0-9a-f]' * int(m.group(1)), expression)
    assert expanded != expression and re.fullmatch(expression, str(TARGET)) and re.fullmatch(expanded, str(TARGET))
    cases = [
        {'id':'P1-literal-directory', 'profile':fixture_profile()},
        {'id':'P2-production-counted-string', 'profile':fixture_profile(expression)},
        {'id':'P3-expanded-string', 'profile':fixture_profile(expanded)},
        {'id':'P4-counted-regex-literal', 'profile':fixture_profile(expression,raw=True)},
        {'id':'P5-expanded-regex-literal', 'profile':fixture_profile(expanded,raw=True)},
        {'id':'P6-literal-no-directory', 'profile':fixture_profile(directory=False)},
    ]
    for case in cases:
        case.update(target=str(TARGET), profileSha256=digest(case['profile'].encode()))
    command = ['/opt/homebrew/bin/python3', '-I', '-B', str(ROOT / 'probe.py'), 'run']
    plan = {'actual':ACTUAL,'scope':'one <=20s call; <=6 isolated children, <=2s each; 1-byte fixture writes only',
        'hypotheses':['H1 counted repetition differs in Seatbelt','H2 SBPL string versus regex-literal escaping differs',
                      'H3 DIRECTORY vnode permission affects file creation'],
        'productionProfileSha256':EXPECTED_PROFILE,'productionProfileBytes':len(profile.encode()),
        'driverSha256':EXPECTED_DRIVER,'prepareSha256':digest(prepared_bytes),'ruleCount':len(rules),
        'originalRule':rule,'originalParent':str(original_parent),'fixtureParent':str(TARGET.parent),
        'fixtureDifferences':['minimal rule slice, not full production profile','precreated obj directory',
            'Python os.open/write, not NuGet/.NET','literal AND regex restricts every case to one fixture leaf',
            'inherited tool permissions remain active; none of M/W/SDK becomes writable'],
        'command':command,'childArgv':['<current Python executable>','-I','-B','-c',CHILD],
        'maxChildren':6,'maxChildSeconds':2,'maxCallSeconds':20,'maxWriteBytesPerChild':1,
        'stopConditions':['literal positive control fails','sandbox_init fails','timeout','unexpected residual fixture'],
        'cases':[{'id':c['id'],'profileSha256':c['profileSha256']} for c in cases]}
    put('production-profile.sb', profile.encode())
    put('profiles.json', encoded(cases))
    put('plan.json', encoded(plan))
    print(json.dumps({'prepared':True,'planSha256':digest(encoded(plan)),
                      'profileSha256':EXPECTED_PROFILE,'limits':limits(),'command':command}))

def run():
    began=time.monotonic()
    def expired(*args):
        raise TimeoutError('overall diagnostic deadline')
    signal.signal(signal.SIGALRM,expired)
    signal.setitimer(signal.ITIMER_REAL,18)
    plan=json.loads((ROOT/'plan.json').read_bytes())
    cases=json.loads((ROOT/'profiles.json').read_bytes())
    assert len(cases)==6 and not TARGET.exists() and not TARGET.is_symlink()
    assert digest((ROOT/'production-profile.sb').read_bytes())==EXPECTED_PROFILE
    put('run-start.json',encoded({'actual':ACTUAL,'argv':sys.orig_argv,
        'startedAtUtc':datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'planSha256':digest((ROOT/'plan.json').read_bytes()),'attempt':1}))
    TARGET.parent.mkdir()
    records=[]; failure=None; process=None
    try:
        for i,case in enumerate(cases):
            assert time.monotonic()-began<16 and not TARGET.exists()
            assert case['target']==str(TARGET) and digest(case['profile'].encode())==case['profileSha256']
            started=time.monotonic()
            argv=[sys.executable,'-I','-B','-c',CHILD]
            process=subprocess.Popen(argv,stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,
                                     start_new_session=True,cwd=ROOT)
            timed_out=False
            try:
                out,err=process.communicate(json.dumps(case).encode(),timeout=max(.01,1.7-(time.monotonic()-started)))
            except subprocess.TimeoutExpired:
                timed_out=True; process.kill(); out,err=process.communicate(timeout=.2)
            elapsed=time.monotonic()-started
            record={'id':case['id'],'pid':process.pid,'argv':argv,'profileSha256':case['profileSha256'],
                'exitCode':process.returncode,'stdout':out.decode(errors='replace'),'stderr':err.decode(errors='replace'),
                'elapsedSeconds':elapsed,'timedOut':timed_out,'childReaped':process.poll() is not None}
            records.append(record)
            print(json.dumps(record),flush=True)
            if timed_out or elapsed>2 or process.returncode==77 or (i==0 and process.returncode!=0) or TARGET.exists():
                failure='STOP: positive control, sandbox installation, time, or residual-file gate'; break
            process=None
    except BaseException as exc:
        failure=type(exc).__name__+': '+str(exc)
    finally:
        if process is not None and process.poll() is None:
            process.kill()
            try: process.communicate(timeout=.2)
            except subprocess.TimeoutExpired: failure='UNREAPED_CHILD'
        signal.setitimer(signal.ITIMER_REAL,0)
    result={'actual':ACTUAL,'records':records,'count':len(records),'failure':failure,
        'elapsedSeconds':time.monotonic()-began,'nativeRestoreRuns':0,'limits':limits(),
        'targetResidual':TARGET.exists(),'status':'PROBE_COMPLETE_NOT_NUGET_VALIDATION' if failure is None else 'PROBE_STOPPED'}
    put('result.json',encoded(result))
    result['finalLimits']=limits()
    print(json.dumps(result),flush=True)
    return 0 if failure is None and result['elapsedSeconds']<=20 else 1

if __name__=='__main__':
    assert len(sys.argv)==2 and sys.argv[1] in ('prepare','run')
    if sys.argv[1]=='prepare': prepare()
    else: raise SystemExit(run())
