"""LOC-B-PREF only. A trusted caller must verify this file before invoking a separately signed S2/S4 activation."""
import argparse, collections, datetime, hashlib, json, os, pathlib, re, signal, subprocess, sys, time
import xml.etree.ElementTree as ET
sys.dont_write_bytecode = True
ROOT = pathlib.Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
E = ROOT / 'TestArtifacts/FightMatch/LOC-IMPL-B-PREF/source-activation-001'
OWNER = '01a0e404-d89d-7ab2-bece-3cd1df3fbc52'
PREFIX = 'FightMatch.Host.Tests.LocalePreferenceStoreTests.'
SOURCES = ['Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs', 'Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs', 'Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs']
MAX_LOG, MAX_TOTAL = 8 * 1024**2, 192 * 1024**2
def stamp(): return datetime.datetime.now(datetime.timezone.utc).isoformat()
def digest(data): return dict(bytes=len(data), sha256=hashlib.sha256(data).hexdigest())
def read(path):
    path = pathlib.Path(path)
    assert all(not p.is_symlink() for p in [path] + list(path.parents)), 'symlink'
    assert path.is_file(), str(path)
    return path.read_bytes()
def identity(path): return dict(path=str(path.relative_to(ROOT)), **digest(read(path)))
def save(path, value):
    with path.open('x', encoding='utf-8') as f: json.dump(value, f, ensure_ascii=False, indent=2); f.write('\n')
def snapshot():
    result = []
    for name in ['Assets', 'Packages', 'ProjectSettings', 'Config', 'Generated', 'Tools']:
        for p in sorted((ROOT / name).rglob('*')):
            assert not p.is_symlink(), str(p)
            if p.is_file() and not p.name.startswith('._'): result.append(identity(p))
    for name in ['HEAD', 'index', 'refs/heads/master', 'packed-refs']: result.append(identity(ROOT / '.git' / name))
    return result
def ps():
    r = subprocess.run(['/bin/ps', '-axo', 'pid=,ppid=,comm='], capture_output=True, text=True, check=True, timeout=10)
    return {int(p[0]): dict(pid=int(p[0]), ppid=int(p[1]), comm=p[2]) for line in r.stdout.splitlines() if len(p := line.strip().split(None, 2)) == 3}
def process_identity(pid):
    fields = {}
    for key, args in [('birth', ['lstart=']), ('command', ['command=']), ('comm', ['comm='])]:
        r = subprocess.run(['/bin/ps', '-ww', '-p', str(pid), '-o'] + args, capture_output=True, text=True, timeout=10)
        if r.returncode: return None
        fields[key] = r.stdout.strip()
    return fields
def unity_family(rows, contents):
    names = {'Unity', 'Unity Hub', 'UnityShaderCompiler', 'UnityAutoQuitter', 'UnityCrashHandler', 'bee_backend', 'UnityPackageManager', 'Unity.Licensing.Client'}
    return [p for p, r in rows.items() if pathlib.Path(r['comm']).name in names or r['comm'].startswith(contents + '/') or 'unity-mcp' in r['comm'].lower() or (pathlib.Path(r['comm']).name == 'dotnet' and contents in (process_identity(p) or {}).get('command', ''))]
def binding(a, mode):
    for item in a['inputs']:
        assert identity(ROOT / item['path']) == item, 'activated input drift: ' + item['path']
    assert identity(ROOT / a['runner']['path']) == a['runner'] and a['runner']['path'] == str(pathlib.Path(__file__).relative_to(ROOT))
    if mode != 'focused': return None
    assert a['review']['completed'] and a['review']['head'] == a['head'] and a['review']['receipt']['sha256'] == digest(read(ROOT / a['review']['receipt']['path']))['sha256']
    assert not any(k.startswith('GIT_') and k not in ('GIT_PAGER', 'GIT_OPTIONAL_LOCKS', 'GIT_NO_REPLACE_OBJECTS') for k in os.environ)
    assert (ROOT / '.git').is_dir() and not (ROOT / '.git').is_symlink()
    for name in ['commondir', 'objects/info/alternates', 'objects/info/http-alternates', 'refs/replace']:
        assert not (ROOT / '.git' / name).exists() and not (ROOT / '.git' / name).is_symlink()
    packed = ROOT / '.git/packed-refs'
    assert not packed.exists() or b'refs/replace/' not in read(packed)
    def obj(kind, oid):
        assert re.fullmatch('[0-9a-f]{40}', oid)
        raw = subprocess.run(['git', '--no-replace-objects', '-c', 'protocol.allow=never', 'cat-file', kind, oid], cwd=ROOT,
            capture_output=True, check=True, timeout=30, env=dict(os.environ, GIT_NO_LAZY_FETCH='1', GIT_OPTIONAL_LOCKS='0')).stdout
        assert hashlib.sha1(kind.encode() + b' ' + str(len(raw)).encode() + b'\0' + raw).hexdigest() == oid
        return raw
    commit = obj('commit', a['head'])
    assert [x[5:].decode() for x in commit.split(b'\n\n', 1)[0].splitlines() if x.startswith(b'tree ')] == [a['tree']]
    entries, pending = {}, [('', a['tree'])]
    while pending:
        prefix, oid = pending.pop(); raw, at = obj('tree', oid), 0
        while at < len(raw):
            sp = raw.index(b' ', at); nul = raw.index(b'\0', sp); mode = raw[at:sp]; name = raw[sp+1:nul].decode(); child = raw[nul+1:nul+21].hex(); at = nul+21
            assert name not in ('', '.', '..') and '/' not in name and '\\' not in name and len(child) == 40
            path = prefix + name
            if mode == b'40000': pending.append((path + '/', child))
            else:
                assert mode in (b'100644', b'100755') and path not in entries
                entries[path] = child
    current = {r['path'] for r in snapshot() if r['path'].startswith(('Assets/', 'Packages/', 'ProjectSettings/'))}
    assert current == {p for p in entries if p.startswith(('Assets/', 'Packages/', 'ProjectSettings/'))}
    mappings = {p: p for p in current}; mappings.update(a['treeMappings'])
    assert a['runner']['path'] in mappings and str((E / 'source-receipt.json').relative_to(ROOT)) in mappings
    for p, tree_path in mappings.items(): assert read(ROOT / p) == obj('blob', entries[tree_path]), 'raw blob mismatch: ' + p
    return dict(head=a['head'], tree=a['tree'], rawCommit=digest(commit), mappedFiles=len(mappings))
def main():
    parser = argparse.ArgumentParser(); parser.add_argument('mode', choices=['discover', 'focused']); parser.add_argument('--authority-sha', required=True)
    args = parser.parse_args(); mode = args.mode
    activation_path = E / ('activation-discovery.json' if mode == 'discover' else 'activation-focused.json')
    authority_bytes = read(activation_path); assert digest(authority_bytes)['sha256'] == args.authority_sha
    a = json.loads(authority_bytes)
    assert a['status'] == ('AUTHORIZED_DISCOVERY_S2' if mode == 'discover' else 'AUTHORIZED_FOCUSED_S4') and a['mode'] == mode
    assert a['owner']['threadId'] == OWNER and a['owner']['hostId'] == 'local' and a['owner']['turnId'] and a['runId']
    assert datetime.datetime.now(datetime.timezone.utc) < datetime.datetime.fromisoformat(a['expiresUtc'])
    assert a['evidenceRoot'] == str(E.relative_to(ROOT)) and a['starts'] == 1
    assert a['sourceReceipt'] == identity(E / 'source-receipt.json')
    editor = a['editor']['path']; assert digest(read(editor)) == {k: a['editor'][k] for k in ['bytes', 'sha256']}
    assert editor == '/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity'
    contents = str(pathlib.Path(editor).parents[1]); budget = 300 if mode == 'discover' else 600
    assert a['budgetSeconds'] == budget and a['logBytes'] == MAX_LOG and a['evidenceBytes'] == MAX_TOTAL
    import shutil, plistlib
    assert plistlib.loads(read(pathlib.Path(contents) / 'Info.plist'))['CFBundleVersion'] == '2022.3.18f1' and shutil.disk_usage(ROOT).free >= 2 * 1024**3
    before = snapshot(); assert before == a['protectedSnapshot'], 'unapproved execution snapshot'
    required = set(SOURCES) | {r['path'] for r in json.loads(read(E / 'binding.json'))['currentSourceInputs']}
    required |= {r['path'] for r in json.loads(read(E / 'binding.json'))['utfApiInputs']}
    if mode == 'focused': required |= {p + '.meta' for p in SOURCES}
    else: assert all(not (ROOT / (p + '.meta')).exists() and not (ROOT / (p + '.meta')).is_symlink() for p in SOURCES), 'natural meta already present'
    assert required <= {r['path'] for r in a['inputs']}
    binding_before = binding(a, mode)
    assert not unity_family(ps(), contents), 'other Unity process'
    directory = E / ('discovery' if mode == 'discover' else 'focused-run-1')
    assert not directory.exists() and not directory.is_symlink(); directory.mkdir()
    argv = [editor, '-batchmode', '-nographics', '-projectPath', str(ROOT)]
    expected = []
    if mode == 'discover': argv += ['-fightMatchLocalePreferenceDiscover', str(directory)]
    else:
        assert a['discoveryLeaves'] == identity(E / 'discovery/leaves.json')
        leaves = json.loads(read(E / 'discovery/leaves.json'))['leaves']; expected = [x['fullName'] for x in leaves]
        assert expected and len(expected) == len(set(expected)) and all(x['runState'] == 'Runnable' and x['fullName'].startswith(PREFIX) for x in leaves)
        selector = '^(?:' + '|'.join(re.escape(n) for n in expected) + ')$'
        assert a['expectedNames'] == expected and a['filter'] == selector
        argv += ['-runTests', '-testPlatform', 'EditMode', '-testFilter', selector, '-testResults', str(directory / 'tests.xml')]
    argv += ['-logFile', str(directory / 'unity.log')]; assert argv == a['argv']
    save(directory / 'argv.json', dict(owner=a['owner'], runId=a['runId'], argv=argv, editor=a['editor'], authoritySha=args.authority_sha, binding=binding_before))
    assert read(activation_path) == authority_bytes and snapshot() == before
    assert binding(a, mode) == binding_before, 'pre-spawn binding drift'
    assert not unity_family(ps(), contents)
    process = subprocess.Popen(argv, cwd=ROOT, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    start = time.monotonic(); owned = {}; actions = []; failure = None; signalled = set(); clients = []
    def event(kind, **values):
        with (E / 'process-events.jsonl').open('a', encoding='utf-8') as f: f.write(json.dumps(dict(mode=mode, runId=a['runId'], utc=stamp(), event=kind, **values)) + '\n')
    def capture():
        process.poll()
        for client in clients: client.poll()
        rows = ps(); candidates = {process.pid} | set(owned)
        while True:
            children = {p for p, r in rows.items() if r['ppid'] in candidates}
            if children <= candidates: break
            candidates |= children
        for pid in candidates:
            if pid not in rows or pid in owned: continue
            value = process_identity(pid)
            if value:
                exe = pathlib.Path(contents) / 'NetCoreRuntime/dotnet' if value['comm'] == 'dotnet' else pathlib.Path(value['comm'])
                assert str(exe).startswith(contents + '/') and exe.is_file(), 'unrecognized owned executable'
                value.update(executable=str(exe), executableIdentity=digest(read(exe))); owned[pid] = value; event('owned', pid=pid, identity=value)
        alive = [p for p in owned if p in rows and process_identity(p) is not None]
        foreign = [p for p in unity_family(rows, contents) if p not in owned]
        return alive, foreign
    def stable(pid):
        saved = owned[pid]; current = process_identity(pid)
        if current is None: return False
        assert all(current[k] == saved[k] for k in ['birth', 'command', 'comm']), 'PID identity changed'
        assert digest(read(saved['executable'])) == saved['executableIdentity']
        loaded = subprocess.run(['/usr/sbin/lsof', '-a', '-p', str(pid), '-d', 'txt', '-Fn'], capture_output=True, text=True, timeout=10)
        assert loaded.returncode == 0 and 'n' + saved['executable'] in loaded.stdout.splitlines(), 'loaded executable mismatch'
        return True
    event('spawn', pid=process.pid, argv=argv)
    try:
        while True:
            alive, foreign = capture(); elapsed = time.monotonic() - start; code = process.poll()
            if foreign: failure = 'UNOWNED_UNITY'; break
            if any(p.stat().st_size > MAX_LOG for p in directory.rglob('*') if p.is_file()) or sum(p.stat().st_size for p in E.rglob('*') if p.is_file()) > MAX_TOTAL: failure = 'EVIDENCE_LIMIT'; break
            log = directory / 'unity.log'; text = read(log).decode('utf-8', 'replace') if log.exists() else ''
            if re.search(r'error CS\d+|Scripts have compiler errors|Compilation failed|PreferenceDiscoveryFailed', text): failure = 'COMPILE_OR_DISCOVERY_FAILURE'; break
            if elapsed >= budget: failure = 'TIMEOUT'; break
            if code is not None:
                if code != 0: failure = 'ROOT_EXIT_NONZERO'
                break
            time.sleep(1)
    except Exception as error: failure = 'MONITOR_' + type(error).__name__; event('monitor-error', detail=str(error))
    alive, foreign = list(owned), []; cleanup_error = None
    try:
        cleanup_start = time.monotonic(); gentle = set()
        while time.monotonic() - cleanup_start < 60:
            alive, foreign = capture()
            if not alive: break
            for pid in alive:
                if time.monotonic() - cleanup_start >= 60: break
                command = owned[pid]['command']
                import shlex
                words = shlex.split(command); compiler = str(pathlib.Path(contents) / 'DotNetSdkRoslyn/VBCSCompiler.dll')
                if pid not in gentle and len(words) == 4 and words[1:3] == ['exec', compiler] and re.fullmatch('-pipename:[A-Za-z0-9_-]+', words[3]) and stable(pid):
                    assert digest(read(compiler)) == a['compilerIdentity']
                    child = subprocess.Popen([owned[pid]['executable'], 'exec', compiler, words[3], '-shutdown'], cwd=ROOT, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                    clients.append(child); value = process_identity(child.pid)
                    if value: value.update(executable=owned[pid]['executable'], executableIdentity=owned[pid]['executableIdentity']); owned[child.pid] = value
                    gentle.add(pid); actions.append(dict(action='compiler-shutdown', pid=pid, clientPid=child.pid)); event('gentle', pid=pid, clientPid=child.pid)
            time.sleep(1)
        alive, foreign = capture()
        for pid in alive:
            if stable(pid):
                if time.monotonic() - cleanup_start >= 90: raise RuntimeError('owned cleanup deadline')
                assert pid not in signalled; os.kill(pid, signal.SIGTERM); signalled.add(pid); actions.append(dict(action='SIGTERM', pid=pid)); event('signal', pid=pid)
        while time.monotonic() - cleanup_start < 90:
            alive, foreign = capture()
            if not alive: break
            time.sleep(1)
    except Exception as error:
        cleanup_error = type(error).__name__ + ': ' + str(error); event('cleanup-blocked', detail=cleanup_error)
    code = process.poll()
    with (directory / 'exit.txt').open('x') as f: f.write(str(code) + '\n')
    after = snapshot(); new_meta = {p + '.meta' for p in SOURCES} if mode == 'discover' else set()
    old_by_path = {r['path']: r for r in before}; added = [r for r in after if r['path'] not in old_by_path]
    scope_ok = {r['path'] for r in added} == new_meta and [r for r in after if r['path'] in old_by_path] == before
    validation = dict(scopeUnchangedExceptNaturalMeta=scope_ok, exitZero=code == 0, ownedClear=not alive, noForeignUnity=not foreign, cleanupSafe=cleanup_error is None, firstFailure=failure, actions=actions)
    binding_after = None; executed = 0 if mode == 'discover' else None
    try:
        if mode == 'discover':
            leaves = json.loads(read(directory / 'leaves.json'))['leaves'] if (directory / 'leaves.json').exists() else []
            receipt = json.loads(read(directory / 'receipt.json')) if (directory / 'receipt.json').exists() else {}
            validation['discovery'] = bool(leaves) and receipt.get('testsExecuted') == 0 and receipt.get('leafCount') == len(leaves) and len({x['fullName'] for x in leaves}) == len(leaves) and all(x['runState'] == 'Runnable' and x['fullName'].startswith(PREFIX) for x in leaves)
            guids = [re.search(rb'^guid: ([0-9a-f]{32})$', read(ROOT / r['path']), re.M).group(1) for r in added]
            all_guids = [m.group(1) for p in (ROOT / 'Assets').rglob('*.meta') if (m := re.search(rb'^guid: ([0-9a-f]{32})$', read(p), re.M))]
            validation['naturalMetaUnique'] = len(guids) == 3 and all(all_guids.count(g) == 1 for g in guids)
            validation['zeroExecutionMarker'] = 'Running tests for EditMode' not in (directory / 'unity.log').read_text(errors='replace')
        else:
            xml = ET.fromstring(read(directory / 'tests.xml')); cases = list(xml.iter('test-case')); executed = len(cases)
            validation['xml'] = collections.Counter(x.get('fullname') for x in cases) == collections.Counter(expected) and all(x.get('result') == 'Passed' for x in cases) and int(xml.get('total', -1)) == int(xml.get('passed', -1)) == len(expected) and all(int(xml.get(k, 0)) == 0 for k in ['failed', 'skipped', 'inconclusive', 'errors'])
        binding_after = binding(a, mode); validation['authorityUnchanged'] = read(activation_path) == authority_bytes; validation['bindingUnchanged'] = binding_before == binding_after
    except Exception as error:
        validation['validationComplete'] = False; event('validation-error', detail=type(error).__name__ + ': ' + str(error))
    passed = not failure and all(v for k, v in validation.items() if k not in ('firstFailure', 'actions'))
    save(E / 'process-safety.json' if mode == 'discover' else directory / 'summary.json', dict(status='PASS' if passed else 'BLOCKED', owner=a['owner'], runId=a['runId'], mode=mode, testsExecuted=executed, expectedFocusedCount=len(expected) if mode == 'focused' else None, elapsedSeconds=time.monotonic()-start, checks=validation, before=before, after=after, naturalMeta=added, binding=binding_after, ownedIdentities=owned, cleanupError=cleanup_error))
    print(json.dumps(dict(status='PASS' if passed else 'BLOCKED', mode=mode, firstFailure=failure)), flush=True)
    return 0 if passed else 3
if __name__ == '__main__':
    sys.exit(main())
