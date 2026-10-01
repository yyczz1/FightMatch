# FIX20 derivative of FIX19: exact12 lifecycle probe, frozen candidate, 360s and retained178 union.
import ast
import collections
import datetime
import difflib
import hashlib
import json
import os
import pathlib
import plistlib
import re
import shlex
import shutil
import signal
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

ROOT = pathlib.Path('/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch')
EDITOR = '/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity'
OUT = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-20'
SOURCE_ONLY = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-20-source'
R17 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf/q4-correction-17'
R15 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-15'
R18 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-18'
R19 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-19'
PREDECESSOR = R19 / 'validation-tools/runner.py'
R17_RUNNER = R17 / 'validation-tools/runner.py'
pre = json.loads((R17 / 'preflight.json').read_text())
FONT = ROOT / 'Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset'
TIMEOUT = 360
MAX_LOG = 8 * 1024 * 1024
MAX_EVIDENCE = 192 * 1024 * 1024
MIN_FREE = 2 * 1024 * 1024 * 1024
OWNER = None  # Supplied by the separately authorized execution activation.

def sha(data):
    return hashlib.sha256(data).hexdigest()

def identity(path):
    path = pathlib.Path(path)
    data = path.read_bytes()
    return dict(path=str(path.relative_to(ROOT)), bytes=len(data), sha256=sha(data))

def now():
    return datetime.datetime.now(datetime.timezone.utc).isoformat()

def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n')

def resources():
    return [identity(ROOT / x['path']) for x in pre['resourcesBefore']]

def font_structure():
    text = FONT.read_text()
    fields = {}
    names = ['m_AtlasPopulationMode', 'm_SourceFontFileGUID', 'm_SourceFontFile_EditorRef',
             'm_SourceFontFile', 'm_AtlasWidth', 'm_AtlasHeight', 'm_AtlasPadding', 'm_AtlasRenderMode',
             'm_IsMultiAtlasTexturesEnabled', 'm_ClearDynamicDataOnBuild']
    for key in names:
        match = re.search(r'^  ' + key + r': (.*)$', text, re.M)
        fields[key] = match.group(1) if match else None
    glyph_text = text.split('  m_GlyphTable:\n', 1)[1].split('  m_CharacterTable:\n', 1)[0]
    glyphs = {m.group(1): m.group(0) for m in re.finditer(r'(?ms)^  - m_Index: (\d+)\n.*?(?=^  - m_Index:|\Z)', glyph_text)}
    characters = dict(re.findall(r'm_Unicode: (\d+)\n    m_GlyphIndex: (\d+)', text))
    return dict(font=identity(FONT), fields=fields, characterCount=len(characters), glyphCount=len(glyphs),
                characters=characters, glyphs=glyphs,
                glyphsPerAtlas=dict(collections.Counter(re.findall(r'^    m_AtlasIndex: (\d+)$', glyph_text, re.M))),
                textureCount=len(re.findall(r'^--- !u!28 ', text, re.M)),
                materialCount=len(re.findall(r'^--- !u!21 ', text, re.M)),
                atlasTextureIndex=int(re.search(r'^  m_AtlasTextureIndex: (\d+)$', text, re.M).group(1)),
                externalGuids=sorted(set(re.findall(r'guid: ([0-9a-f]{32})', text))))

def processes():
    result = subprocess.run(['/bin/ps', '-axo', 'pid=,ppid=,comm='], capture_output=True, text=True, check=True)
    rows = []
    for line in result.stdout.splitlines():
        parts = line.strip().split(None, 2)
        if len(parts) == 3:
            rows.append(dict(pid=int(parts[0]), ppid=int(parts[1]), comm=parts[2]))
    return rows

def unity_rows(rows):
    result=[]
    editor_contents=str(pathlib.Path(EDITOR).parents[1])+'/'
    names={'Unity','Unity Hub','UnityShaderCompiler','UnityAutoQuitter','UnityCrashHandler',
           'bee_backend','UnityPackageManager','Unity.Licensing.Client'}
    for row in rows:
        name=pathlib.Path(row['comm']).name
        if name in names or row['comm'].startswith(editor_contents) or 'unity-mcp' in row['comm'].lower() or 'Unity Hub.app' in row['comm']:
            result.append(row)
        elif name=='dotnet' and editor_contents in current_identity(row['pid'])['command']:
            result.append(row)
    return result

def no_unity():
    assert not unity_rows(processes()), 'Existing Unity family process'

def root_identity(pid):
    birth=subprocess.run(['/bin/ps','-p',str(pid),'-o','lstart='],capture_output=True,text=True)
    command=subprocess.run(['/bin/ps','-p',str(pid),'-o','command='],capture_output=True,text=True)
    comm=subprocess.run(['/bin/ps','-p',str(pid),'-o','comm='],capture_output=True,text=True)
    return dict(pid=pid,birth=birth.stdout.strip(),command=command.stdout.strip(),comm=comm.stdout.strip(),queryExitCodes=[birth.returncode,command.returncode,comm.returncode],observedUtc=now())

def create_json(path, data):
    with path.open('x', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write('\n')

def canonical(data):
    return sha(json.dumps(data, sort_keys=True, separators=(',', ':'), ensure_ascii=False).encode())

def jsonl(path, data):
    with path.open('a', encoding='utf-8') as f:
        f.write(json.dumps(data, ensure_ascii=False, separators=(',', ':')) + '\n')
        f.flush()

def assert_non_link(path):
    for part in [path] + list(path.parents):
        assert part.exists() and not part.is_symlink(), 'Missing path or symlink: ' + str(part)

def frozen_now(items):
    return [identity(ROOT / row['path']) for row in items]

def tree_manifest(directory):
    return [identity(p) for p in sorted(directory.rglob('*')) if p.is_file() and not p.name.startswith('._')]

def old_tree_snapshots():
    paths = sorted(set([x['root'] for x in pre['oldRoots']] + [str(R17), str(R15), str(R18), str(R19), str(ROOT/'TestArtifacts/FightMatch/UGUI-01/q4-correction-19-source'), str(ROOT/'TestArtifacts/FightMatch/UGUI-01/q4-correction-18-source'), str(SOURCE_ONLY)]))
    return [dict(root=p, files=tree_manifest(pathlib.Path(p))) for p in paths]

def compact_trees(trees):
    return [dict(root=t['root'], fileCount=len(t['files']),
                 totalBytes=sum(x['bytes'] for x in t['files']), manifestSha256=canonical(t['files'])) for t in trees]

def protected_files():
    return [identity(p) for d in ['Assets', 'Packages', 'ProjectSettings', 'Config', 'Generated', 'Tools']
            for p in sorted((ROOT / d).rglob('*')) if p.is_file() and not p.name.startswith('._')]

def xml_result(path, expected):
    result = dict(exists=path.exists(), expectedCount=len(expected), actualFullnames=[], cases=[], passed=False)
    if not result['exists']:
        return result
    result['identity'] = identity(path)
    if path.stat().st_size > MAX_LOG:
        result['error'] = 'XML exceeds byte limit'
        return result
    try:
        xml = ET.parse(path).getroot()
        tests = list(xml.iter('test-case'))
        result.update(statistics=xml.attrib, cases=[
            dict(fullname=t.attrib.get('fullname'), result=t.attrib.get('result'),
                 output=t.findtext('output') or '', message=t.findtext('failure/message'),
                 stack=t.findtext('failure/stack-trace')) for t in tests])
        result['actualFullnames'] = [t['fullname'] for t in result['cases']]
        result['counterExact'] = collections.Counter(result['actualFullnames']) == collections.Counter(expected)
        result['zeroNonPass'] = all(t['result'] == 'Passed' for t in result['cases'])
        result['statisticsExact'] = (int(xml.attrib.get('total', '-1')) == len(expected) and
            int(xml.attrib.get('passed', '-1')) == len(expected) and
            all(int(xml.attrib.get(k, '0')) == 0 for k in ['failed', 'skipped', 'inconclusive', 'errors']) and
            xml.attrib.get('result') == 'Passed')
        result['passed'] = result['counterExact'] and result['zeroNonPass'] and result['statisticsExact']
    except Exception as exc:
        result['error'] = type(exc).__name__ + ': ' + str(exc)
    return result

def current_identity(pid):
    result = root_identity(pid)
    # -ww prevents long exact12 selectors from being truncated by ps.
    command = subprocess.run(['/bin/ps', '-ww', '-p', str(pid), '-o', 'command='],
                             capture_output=True, text=True)
    result['command'] = command.stdout.strip()
    result['queryExitCodes'][1] = command.returncode
    return result


def supervise_root(poll, compile_error, observe, wait_step, elapsed, budget):
    """Return the first terminal fact; callers never replace a failure with cleanup results."""
    while True:
        error = compile_error()
        exit_code = poll()
        observed_at = elapsed()
        if observed_at > budget or (observed_at >= budget and exit_code is None):
            return dict(kind='TIMEOUT', elapsedSeconds=observed_at, exitCode=exit_code)
        if error:
            return dict(kind='COMPILE_FAILURE', elapsedSeconds=observed_at, exitCode=exit_code, compilerError=error)
        if exit_code is not None:
            return dict(kind='ROOT_EXIT_ZERO' if exit_code == 0 else 'ROOT_EXIT_NONZERO',
                        elapsedSeconds=observed_at, exitCode=exit_code)
        observe()
        wait_step(min(1, max(0, budget-elapsed())))

def settle_owned(reason, root_pid, snapshot, verify, gentle, terminate, wait_step, elapsed):
    """At most 60 seconds of gentle/natural cleanup, then guarded SIGTERM and 30 seconds."""
    began = elapsed()
    gentle_attempts = set()
    actions = []
    signals = []
    final = {}
    def finish(status):
        return dict(status=status, reason=reason, startedElapsedSeconds=began,
                    durationSeconds=elapsed()-began, gentleLimitSeconds=60, terminationLimitSeconds=30,
                    actions=actions, signalsSent=signals, finalObservation=final)
    while True:
        final = snapshot()
        if final.get('unownedUnityProcesses'):
            return finish('BLOCKED_UNOWNED_PROCESS')
        remaining = final['ownedProcesses']
        if not remaining:
            return finish('COMPLETE')
        if elapsed() >= began + 60:
            break
        # Re-evaluate on each sample: transient children can vanish after root exit.
        if reason != 'TIMEOUT' and len(remaining) == 1 and remaining[0].get('compilerServer'):
            row = remaining[0]
            if row['pid'] not in gentle_attempts:
                proof = verify(row)
                if proof.get('gone'):
                    continue
                if not proof['allowed']:
                    actions.append(dict(action='identity-rejected', pid=row['pid'], proof=proof))
                    return finish('BLOCKED_PROCESS_IDENTITY')
                gentle_attempts.add(row['pid'])
                result=gentle(row, proof)
                actions.append(dict(action='gentle', pid=row['pid'], result=result))
                if result.get('status')=='IDENTITY_NO_LONGER_MATCHES':
                    return finish('BLOCKED_PROCESS_IDENTITY')
        wait_step(min(1, max(0, began + 60-elapsed())))
    candidates = [r for r in final['ownedProcesses'] if reason != 'TIMEOUT' or r['pid'] == root_pid]
    proofs = []
    for row in candidates:
        if elapsed() >= began + 90:
            return finish('BLOCKED_MANUAL_PROCESS_CLEANUP')
        proof = verify(row)
        if proof.get('gone'):
            continue
        if not proof['allowed']:
            actions.append(dict(action='identity-rejected', pid=row['pid'], proof=proof))
            return finish('BLOCKED_PROCESS_IDENTITY')
        proofs.append((row, proof))
    # Verify the whole candidate set before sending any signal; terminate also rechecks immediately.
    for row, proof in proofs:
        if elapsed() >= began + 90:
            return finish('BLOCKED_MANUAL_PROCESS_CLEANUP')
        result = terminate(row, proof)
        actions.append(dict(action='SIGTERM', pid=row['pid'], result=result))
        if result.get('sent'):
            signals.append(result)
        elif not result.get('gone'):
            return finish('BLOCKED_PROCESS_IDENTITY')
    while elapsed() < began + 90:
        final = snapshot()
        if final.get('unownedUnityProcesses'):
            return finish('BLOCKED_UNOWNED_PROCESS')
        if not final['ownedProcesses']:
            return finish('COMPLETE')
        wait_step(min(1, max(0, began + 90-elapsed())))
    final = snapshot()
    return finish('COMPLETE' if not final['ownedProcesses'] and not final.get('unownedUnityProcesses')
                  else 'BLOCKED_MANUAL_PROCESS_CLEANUP')


def owned_tool_identity(info):
    command = shlex.split(info['command'])
    executable = pathlib.Path(info['comm'])
    if info['comm'] == 'dotnet':
        executable = pathlib.Path(EDITOR).parents[1] / 'NetCoreRuntime/dotnet'
    executable.relative_to(pathlib.Path(EDITOR).parents[1])
    assert executable.is_file(), 'Owned executable is missing'
    loaded = subprocess.run(['/usr/sbin/lsof','-a','-p',str(info['pid']),'-d','txt','-Fn'],
                            capture_output=True,text=True,timeout=5)
    assert loaded.returncode == 0 and 'n'+str(executable) in loaded.stdout.splitlines(), 'Executable identity unavailable'
    files = [executable]
    compiler = pathlib.Path(EDITOR).parents[1] / 'DotNetSdkRoslyn/VBCSCompiler.dll'
    is_compiler = len(command) >= 3 and command[1:3] == ['exec',str(compiler)]
    if is_compiler:
        files.append(compiler)
    identities = [dict(path=str(p),bytes=p.stat().st_size,sha256=sha(p.read_bytes())) for p in files]
    if is_compiler:
        expected = ast.literal_eval(re.search(r"tools_match = .* == (\[.*\])",R17_RUNNER.read_text()).group(1))
        assert [(e['bytes'],e['sha256']) for e in identities] == expected, 'Compiler tool identity drift'
    return dict(files=identities,compilerServer=is_compiler and len(command)==4 and
                re.fullmatch(r'-pipename:[A-Za-z0-9_-]+',command[3]) is not None)

def verify_owned(row, saved, ancestry, root_pid):
    current = current_identity(row['pid'])
    if current['queryExitCodes'] == [1,1,1]:
        return dict(allowed=False,gone=True,current=current)
    proof = dict(allowed=False,gone=False,current=current,checks={})
    try:
        tools = owned_tool_identity(current)
        checks = dict(pid=current['pid']==saved['pid'] and current['pid']>0,
                      birth=bool(current['birth']) and current['birth']==saved['birth'],
                      command=current['command']==saved['command'],comm=current['comm']==saved['comm'],
                      queries=current['queryExitCodes']==saved['queryExitCodes']==[0,0,0],
                      tools=tools==saved.get('tools'),
                      ancestry=bool(ancestry) and ancestry['chain'][0]==row['pid'] and
                      ((ancestry['origin']=='Unity' and ancestry['chain'][-1]==root_pid) or
                       (ancestry['origin']=='cleanup-client' and ancestry['chain'][-1]==os.getpid())))
        final_identity=current_identity(row['pid'])
        checks['identityStableThroughToolCheck']=all(final_identity[k]==current[k] for k in ['pid','birth','command','comm','queryExitCodes'])
        proof.update(checks=checks,tools=tools,finalIdentity=final_identity,allowed=all(checks.values()))
    except Exception as exc:
        proof['error']=type(exc).__name__+': '+str(exc)
    return proof

def event_trace(pid, final=False):
    path = OUT / 'test-events.jsonl'
    result = dict(exists=path.exists(), rows=0, errors=[], runStarted=0, runFinished=0,
                  startedCount=0, finishedCount=0, domains=[], finished=[], lastFinished=None,
                  unmatchedStarted=[], unresolvedFullnames=[], repeatedStarts={}, partialTail=False)
    if not path.exists():
        result['errors'].append('missing progress file')
        return result
    if path.stat().st_size > 4 * 1024 * 1024:
        result['errors'].append('progress file exceeds 4 MiB')
        return result
    raw = path.read_bytes()
    lines = raw.splitlines(keepends=True)
    if len(lines) > 4096:
        result['errors'].append('progress file exceeds 4096 rows')
        return result
    starts = collections.defaultdict(list)
    start_counts = collections.Counter()
    last_start = {}
    last_finish = {}
    process_start = None
    domains = set()
    fields = {'seq', 'utc', 'pid', 'processStartUtc', 'domain', 'source', 'eventName',
              'testId', 'fullName', 'result', 'duration'}
    for i, line in enumerate(lines):
        if not line.endswith(b'\n'):
            result['partialTail'] = True
            if final:
                result['errors'].append('partial final row')
            break
        try:
            assert len(line) <= 16 * 1024, 'row exceeds 16 KiB'
            event = json.loads(line.decode('utf-8'))
            assert set(event) == fields, 'schema mismatch'
            assert type(event['seq']) is int and event['seq'] == i + 1, 'sequence gap'
            assert type(event['pid']) is int and event['pid'] == pid, 'foreign PID'
            assert re.fullmatch('[0-9a-f]{32}', event['domain']), 'invalid domain token'
            stamp = datetime.datetime.fromisoformat(event['utc'].replace('Z', '+00:00'))
            assert stamp.utcoffset() == datetime.timedelta(0), 'non-UTC timestamp'
            assert event['testId'] and event['fullName'], 'missing test identity'
            assert isinstance(event['duration'], (int, float)) and 0 <= event['duration'] < float('inf'), 'duration'
            if process_start is None:
                process_start = event['processStartUtc']
                datetime.datetime.fromisoformat(process_start.replace('Z', '+00:00'))
            assert event['processStartUtc'] == process_start, 'process start identity changed'
            kind = event['eventName']
            assert kind in ['RunStarted', 'RunFinished', 'TestStarted', 'TestFinished'], 'unknown event'
            assert event['source'] == ('editor' if kind.startswith('Run') else 'attribute'), 'source mismatch'
            assert not result['runFinished'], 'events after RunFinished'
            if i == 0:
                assert kind == 'RunStarted', 'missing initial RunStarted'
            if kind.endswith('Started'):
                assert event['result'] == '' and event['duration'] == 0, 'invalid start result'
            else:
                assert event['result'], 'missing result'
            domains.add(event['domain'])
            result['rows'] += 1
            if kind == 'RunStarted':
                result['runStarted'] += 1
            elif kind == 'RunFinished':
                result['runFinished'] += 1
                result['runResult'] = event['result']
            elif kind == 'TestStarted':
                result['startedCount'] += 1
                start_counts[event['fullName']] += 1
                starts[event['fullName']].append(event)
                last_start[event['fullName']] = event
            else:
                result['finishedCount'] += 1
                result['finished'].append(event)
                result['lastFinished'] = event
                last_finish[event['fullName']] = event
                assert starts[event['fullName']], 'finish without a preceding start'
                starts[event['fullName']].pop()
        except Exception as exc:
            result['errors'].append(dict(row=i + 1, error=type(exc).__name__ + ': ' + str(exc)))
            break
    result['domains'] = sorted(domains)
    result['processStartUtc'] = process_start
    result['repeatedStarts'] = {k: v for k, v in start_counts.items() if v > 1}
    # Raw repeated starts remain in the JSONL and unmatched list; no event is deduplicated.
    result['unmatchedStarted'] = sorted([e for values in starts.values() for e in values], key=lambda e: e['seq'])
    result['unresolvedFullnames'] = sorted(k for k, e in last_start.items()
                                         if last_finish.get(k, {}).get('seq', 0) < e['seq'])
    if final and not result['rows']:
        result['errors'].append('no progress events')
    return result

def preflight():
    global OWNER
    assert OUT == ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-20'
    assert_non_link(OUT)
    assert set(p.name for p in OUT.iterdir()) == {'activation.json', 'validation-tools'}, 'Formal root is not fresh'
    activation = json.loads((OUT / 'activation.json').read_text())
    assert activation['mode']=='SINGLE_GRAPHICS_EXACT12'
    assert re.fullmatch('[0-9a-f]{40}',activation['prBinding']['head']) and activation['prBinding']['head']!='302a95bb895f87ac3e4d308f56f167f2e055e3d8', 'FIX20 requires its newly bound PR head'
    OWNER=activation['owner']
    assert OWNER['threadId']=='01a0e404-d89d-7ab2-bece-3cd1df3fbc52' and OWNER['hostId']=='local' and OWNER['turnId']
    assert activation['runner'] == identity(pathlib.Path(__file__))
    ready=json.loads((SOURCE_ONLY/'static-result.json').read_text())
    assert identity(pathlib.Path(__file__))['sha256']==ready['runner']['sha256'], 'Prepared FIX20 runner drift'
    assert identity(ROOT / activation['packet']['path']) == activation['packet']
    baseline = json.loads((SOURCE_ONLY / 'delta-manifest.json').read_text())
    static = json.loads((SOURCE_ONLY / 'static-result.json').read_text())
    assert static['status'] == 'SOURCE_READY' and all(static['mechanicalSourceChecks'].values())
    assert frozen_now(static['peerEvidence']) == static['peerEvidence']
    assert frozen_now(baseline['fixedInputs']) == baseline['fixedInputs']
    assert frozen_now(baseline['addedFiles']) == baseline['addedFiles']
    sources = json.loads((R17 / 'static/source-manifest.json').read_text())
    products = json.loads((R17 / 'static/product-only-manifest.json').read_text())
    res = json.loads((R17 / 'static/resources-manifest-before.json').read_text())
    assert [len(sources), len(products), len(res)] == [53, 29, 37]
    original_sources = sources
    sources = baseline['candidateSources']
    changed_paths = {r['path'] for r in baseline['codeAfter']}
    assert {r['path'] for r in sources} == {r['path'] for r in original_sources}
    assert [r for r in sources if r['path'] not in changed_paths] == [r for r in original_sources if r['path'] not in changed_paths]
    assert frozen_now(sources) == sources and frozen_now(products) == products and frozen_now(res) == res
    assert protected_files() == baseline['protectedCandidate'], 'Prepared protected candidate drift'
    assert pre['resourcesBefore'] == res
    core190 = json.loads((R17 / 'core-190-graphics/expected-fullnames.json').read_text())
    exact2 = pre['expectedTests2']; host = pre['expectedHost30']; all222 = pre['expectedTests222']
    assert core190 == pre['expectedCore190']
    old_core = xml_result(R19/'tests.xml', core190)
    assert old_core['counterExact'] and len(old_core['cases']) == 190
    old_xml = ET.parse(R19/'tests.xml').getroot()
    classnames = ['FightMatch.Core.Tests.UguiSceneCompositionTests', 'FightMatch.Core.Tests.LocalizedTextBindingTests']
    selected = [t for t in old_xml.iter('test-case') if t.get('classname') in classnames]
    core = sorted(t.get('fullname') for t in selected)
    assert len(core) == 12 and all(sum(t.get('classname') == c for t in selected) == 6 for c in classnames)
    retained178 = [t for t in old_core['cases'] if t['fullname'] not in core]
    assert len(retained178) == 178 and all(t['result'] == 'Passed' for t in retained178)
    groups = dict(exact2=exact2, host30=host, retained178=sorted(t['fullname'] for t in retained178), exact12=core)
    assert [len(v) for v in groups.values()] == [2, 30, 178, 12] and len(all222) == 222
    prepared = json.loads((SOURCE_ONLY/'expected-fullnames.json').read_text())
    assert prepared == dict(groups=groups, expected222=all222)
    assert all(len(v) == len(set(v)) for v in list(groups.values()) + [all222])
    pairs = [(a,b) for i,a in enumerate(groups) for b in list(groups)[i+1:]]
    assert all(not(set(groups[a]) & set(groups[b])) for a,b in pairs)
    assert collections.Counter(exact2 + host + groups['retained178'] + core) == collections.Counter(all222)
    retained = xml_result(R15 / 'exact-2-graphics/tests.xml', exact2)
    host_result = xml_result(R17 / 'host-30-graphics/tests.xml', host)
    assert retained['passed'] and host_result['passed'], 'Retained XML gate failed'
    raw_filter = (SOURCE_ONLY/'test-filter.txt').read_bytes()
    literal = '^(?:' + '|'.join(re.escape(n) for n in core) + ')$'
    assert raw_filter == literal.encode('utf-8') and not any(';' in n for n in core)
    assert all(re.fullmatch(literal, n) for n in core)
    assert not any(re.fullmatch(literal, n) for n in exact2 + host + groups['retained178'])
    assert not any(re.fullmatch(literal, n + '_not_a_test') for n in core)
    no_unity()
    rows = processes()
    prior_run = json.loads((R19 / 'run.json').read_text())
    previous_birth = prior_run['rootBirthIdentity']
    prior_alive = [r for r in rows if r['pid'] == previous_birth['pid']]
    prior_identity = current_identity(previous_birth['pid']) if prior_alive else None
    previous_identities=json.loads((R19/'quiescence.json').read_text())['ownedIdentities']
    prior_owned_matches=[]
    for row in rows:
        saved=previous_identities.get(str(row['pid']))
        if saved and saved.get('comm')==row['comm']:
            current=current_identity(row['pid'])
            if current['birth']==saved['birth'] and current['command']==saved['command']:
                prior_owned_matches.append(current)
    assert not prior_owned_matches, 'Prior owned child identity remains: '+repr(prior_owned_matches)
    assert prior_identity is None or (prior_identity['birth'], prior_identity['command']) != (previous_birth['birth'], previous_birth['command']), 'Prior Unity owner still alive'
    assert plistlib.load(open(pathlib.Path(EDITOR).parents[1] / 'Info.plist', 'rb'))['CFBundleVersion'] == '2022.3.18f1'
    free = shutil.disk_usage(ROOT).free
    assert free >= MIN_FREE
    font = font_structure()
    assert font['characterCount'] == font['glyphCount'] == 398 and font['textureCount'] == 4
    protected = protected_files()
    old_trees = old_tree_snapshots()
    diagnostic = sorted(sources + res + baseline['addedFiles'], key=lambda r:r['path'])
    assert len({r['path'] for r in diagnostic}) == 92 and canonical(diagnostic) == baseline['diagnosticCandidate']['sha256']
    (OUT / 'static').mkdir()
    create_json(OUT / 'static/inputs.json', dict(fixed=baseline['fixedInputs'],
                sourceOnly=tree_manifest(SOURCE_ONLY), predecessorPreflight=identity(R17/'preflight.json'),
                predecessorPartition=identity(R17/'static/partition-proof.json')))
    create_json(OUT / 'static/delta-manifest.json', dict(sources=sources, products=products, resources=res,
                delta=baseline['addedFiles'], diagnosticCandidate=baseline['diagnosticCandidate']))
    create_json(OUT / 'static/expected-fullnames.json', dict(groups=groups, expected222=all222))
    ownership = {}
    for label,names in [('fixed28', pre['expectedTests28']), ('lifecycle4', pre['expectedTests4']),
                        ('parameterized6', [n for n in pre['expectedTests28'] if '(' in n])]:
        ownership[label] = {n: next(g for g,values in groups.items() if n in values) for n in names}
    create_json(OUT / 'static/partition-proof.json', dict(counts={k:len(v) for k,v in groups.items()},
                hashes={k:canonical(v) for k,v in groups.items()}, expected222Sha256=canonical(all222),
                pairwiseDisjoint=True, counterUnionExact222=True, coverageOwnership=ownership,
                retainedExact2=retained, retainedHost30=host_result,
                retainedCore178=dict(identity=identity(R19/'tests.xml'),fullnames=groups['retained178'],passed=True),
                selector=dict(identity=identity(SOURCE_ONLY/'test-filter.txt'), escaping='re.escape; anchored literal alternation; mechanically selected by two classnames', positiveCount=len(core), excludedCount=len(host+exact2+retained178))))
    create_json(OUT/'static/static-result.json', static)
    with (OUT/'test-filter.txt').open('xb') as f:
        f.write(raw_filter)
    before = dict(utc=now(), source=sources, products=products, resources=res, delta=baseline['addedFiles'],
                  font=font, protectedClosedSet=protected, oldEvidence=compact_trees(old_trees),
                  freeBytes=free, gitFiles=[identity(ROOT/'.git/HEAD'),identity(ROOT/'.git/refs/heads/master')],
                  sceneTemplateSettingsAbsent=not (ROOT/'ProjectSettings/SceneTemplateSettings.json').exists())
    create_json(OUT/'before.json', before)
    old_code = PREDECESSOR.read_text()
    current_code = pathlib.Path(__file__).read_text()
    diff = ''.join(difflib.unified_diff(old_code.splitlines(keepends=True), current_code.splitlines(keepends=True),
                                      fromfile=str(PREDECESSOR.relative_to(ROOT)), tofile=str(pathlib.Path(__file__).relative_to(ROOT))))
    proof = dict(status='PREFLIGHT_PASS', utc=now(), owner=OWNER, prBinding=activation['prBinding'],
                 originalCounts=dict(source=len(sources), product=len(products), resources=len(res)),
                 addedCount=len(baseline['addedFiles']), processPreflight=dict(psSucceeded=True,noUnityFamily=True,previousRootIdentity=prior_identity),
                 resourceLimits=dict(testSeconds=TIMEOUT,naturalWaitSeconds=60,sigtermWaitSeconds=30,
                                     logBytesEach=MAX_LOG,evidenceBytes=MAX_EVIDENCE,freeBytesFloor=MIN_FREE),
                 runnerDerivation=dict(predecessor=identity(PREDECESSOR),current=identity(pathlib.Path(__file__)),
                                       diffSha256=sha(diff.encode()),diff=diff),
                 sourceOnlyStaticResult=identity(SOURCE_ONLY/'static-result.json'), freeBytes=free,
                 priorOwnedProcessesAbsent=True)
    create_json(OUT/'preflight.json', proof)
    print(json.dumps(dict(status=proof['status'],groups={k:len(v) for k,v in groups.items()},freeGiB=round(free/1024**3,2))),flush=True)
    return before,old_trees,core,exact2,host,all222,retained,host_result,retained178

def execute():
    before,old_trees,expected,exact2,host,all222,retained,host_result,retained178 = preflight()
    test_filter = (OUT/'test-filter.txt').read_bytes().decode('utf-8')
    argv = [EDITOR,'-batchmode','-releaseCodeOptimization','-projectPath',str(ROOT),
            '-runTests','-testPlatform','EditMode','-testFilter',test_filter,
            '-testResults',str(OUT/'tests.xml'),'-fightMatchTestProgress',str(OUT/'test-events.jsonl'),
            '-logFile',str(OUT/'unity.log')]
    assert not set(['-quit','-nographics','-disableManagedDebugger','-buildTarget','-automated']) & set(argv)
    no_unity()
    assert frozen_now(before['source']+before['resources']+before['delta']) == before['source']+before['resources']+before['delta']
    for name in ['test-events.jsonl','process-events.jsonl','progress.jsonl']:
        with (OUT/name).open('xb'):
            pass
    run = dict(task='FIX20',owner=OWNER,argv=argv,exactCommand=shlex.join(argv),
               startedUtc=now(),timeoutSeconds=TIMEOUT,expectedTestCount=len(expected),unityLaunchCount=0,
               timedOut=False,signalsSent=[],exitCode=None,passed=False)
    create_json(OUT/'run.json',run)
    owned = set()
    owned_identities = {}
    owned_ancestry = {}
    cleanup_clients = {}
    cleanup_client_exits = set()
    signalled_pids = set()
    samples = []
    progress_times = []
    last_progress = -10
    log_offset = 0
    last_line = None
    xml_first_seen = None
    budget_violations = []
    start = time.monotonic()
    def process_event(kind, **data):
        payload=dict(event=kind,utc=now(),elapsedSeconds=time.monotonic()-start)
        payload.update(data)
        jsonl(OUT/'process-events.jsonl',payload)
    def observe(force=False):
        nonlocal last_progress,log_offset,last_line,xml_first_seen
        rows = processes()
        if process.poll() is not None and not run.get('processExitObservedUtc'):
            run.update(exitCode=process.returncode,processExitObservedUtc=now(),
                       launchToExitObservationSeconds=time.monotonic()-start)
            process_event('process-exit',pid=process.pid,exitCode=process.returncode,afterDeadline=run['timedOut'])
        for pid,client in cleanup_clients.items():
            if client.poll() is not None and pid not in cleanup_client_exits:
                cleanup_client_exits.add(pid)
                process_event('compiler-shutdown-client-exit',pid=pid,exitCode=client.returncode)
        def remember_owned(pid):
            if pid not in owned_identities:
                info=current_identity(pid)
                try:
                    info['tools']=owned_tool_identity(info)
                except Exception as exc:
                    info['toolIdentityError']=type(exc).__name__+': '+str(exc)
                owned_identities[pid]=info
        for row in rows:
            if row['pid'] in owned:
                remember_owned(row['pid'])
        for _ in range(20):
            children=[r for r in rows if r['ppid'] in owned and r['pid'] not in owned]
            adopted=[]
            for row in children:
                saved=owned_identities.get(row['ppid'],{})
                parent_now=current_identity(row['ppid'])
                # Never extend an ancestry chain through a reused or unverifiable parent PID.
                if not (saved.get('birth') and parent_now['birth']==saved['birth'] and
                        parent_now['command']==saved['command'] and
                        parent_now['queryExitCodes']==saved['queryExitCodes']==[0,0,0]):
                    continue
                owned.add(row['pid'])
                parent=owned_ancestry[row['ppid']]
                owned_ancestry[row['pid']]=dict(origin=parent['origin'],chain=[row['pid']]+parent['chain'])
                remember_owned(row['pid'])
                adopted.append(row['pid'])
            if not adopted:
                break
        snapshot = dict(utc=now(),ownedProcesses=[r for r in rows if r['pid'] in owned],unityProcesses=unity_rows(rows))
        elapsed = time.monotonic()-start
        if force or elapsed-last_progress >= 5:
            last_progress = elapsed
            progress_times.append(elapsed)
            log_path=OUT/'unity.log'; xml_path=OUT/'tests.xml'
            log_info=dict(exists=log_path.exists())
            if log_path.exists():
                size=log_path.stat().st_size
                if size<=MAX_LOG:
                    with log_path.open('rb') as f:
                        f.seek(log_offset); added=f.read(MAX_LOG+1)
                    nonempty=[s for s in added.decode('utf-8',errors='replace').splitlines() if s.strip()]
                    if nonempty:
                        last_line=dict(byteRangeStart=log_offset,byteRangeEnd=size,text=nonempty[-1])
                    log_offset=size
                log_info.update(bytes=size,mtimeUtc=datetime.datetime.fromtimestamp(log_path.stat().st_mtime,datetime.timezone.utc).isoformat(),lastObservedNonemptyLine=last_line)
            xml_info=dict(exists=xml_path.exists())
            if xml_path.exists():
                if xml_first_seen is None: xml_first_seen=now()
                xml_info.update(bytes=xml_path.stat().st_size,mtimeUtc=datetime.datetime.fromtimestamp(xml_path.stat().st_mtime,datetime.timezone.utc).isoformat())
            trace=event_trace(process.pid)
            short_trace={k:trace[k] for k in ['rows','errors','runStarted','runFinished','startedCount','finishedCount','domains','lastFinished','unresolvedFullnames','partialTail']}
            record=dict(utc=now(),elapsedSeconds=elapsed,unityPid=process.pid,argv=argv,
                        **{k:snapshot[k] for k in ['ownedProcesses','unityProcesses']},log=log_info,xml=xml_info,progress=short_trace)
            jsonl(OUT/'progress.jsonl',record)
            samples.append(snapshot)
            for name in ['unity.log','editor.stdout.log','editor.stderr.log','tests.xml']:
                p=OUT/name
                if p.exists() and p.stat().st_size>MAX_LOG:
                    violation=dict(path=name,bytes=p.stat().st_size)
                    if violation not in budget_violations: budget_violations.append(violation)
            evidence_bytes=sum(p.stat().st_size for p in OUT.rglob('*') if p.is_file())
            free=shutil.disk_usage(ROOT).free
            if evidence_bytes>MAX_EVIDENCE or free<MIN_FREE:
                violation=dict(evidenceBytes=evidence_bytes,freeBytes=free)
                if violation not in budget_violations: budget_violations.append(violation)
        return snapshot

    def compile_failure():
        for name in ['unity.log','editor.stdout.log','editor.stderr.log']:
            path=OUT/name
            if path.exists():
                with path.open('rb') as f:
                    text=f.read(MAX_LOG+1).decode('utf-8',errors='replace')
                for line in text.splitlines():
                    if re.search(r'error CS\d+|Compilation failed|Scripts have compiler errors',line):
                        return dict(file=name,line=line)
        return None
    def wait_step(seconds):
        if seconds<=0:
            return
        if process.poll() is None:
            try: process.wait(timeout=seconds)
            except subprocess.TimeoutExpired: pass
        else:
            time.sleep(seconds)
    def remaining_snapshot():
        state=observe()
        state['unownedUnityProcesses']=[r for r in state['unityProcesses'] if r['pid'] not in owned]
        state['ownedProcesses']=[dict(r,compilerServer=owned_identities.get(r['pid'],{}).get('tools',{}).get('compilerServer',False))
                                 for r in state['ownedProcesses']]
        return state
    def verify(row):
        return verify_owned(row,owned_identities.get(row['pid'],{}),owned_ancestry.get(row['pid']),process.pid)
    def gentle(row, proof):
        immediate=verify(row)
        if not immediate.get('allowed'):
            return dict(status='IDENTITY_NO_LONGER_MATCHES',proof=immediate)
        args=shlex.split(immediate['current']['command'])
        command=[str(pathlib.Path(EDITOR).parents[1]/'NetCoreRuntime/dotnet'),
                 'exec',str(pathlib.Path(EDITOR).parents[1]/'DotNetSdkRoslyn/VBCSCompiler.dll'),args[3],'-shutdown']
        client=subprocess.Popen(command,cwd=ROOT,stdout=stdout,stderr=stderr)
        owned.add(client.pid)
        owned_ancestry[client.pid]=dict(origin='cleanup-client',chain=[client.pid,os.getpid()])
        cleanup_clients[client.pid]=client
        process_event('compiler-server-graceful-shutdown',pid=row['pid'],clientPid=client.pid,argv=command,identityProof=immediate)
        return dict(status='REQUEST_SENT',pid=row['pid'],clientPid=client.pid,argv=command,signalsSent=False)
    def terminate(row, proof):
        immediate=verify(row)
        if not immediate.get('allowed'):
            return dict(sent=False,gone=immediate.get('gone',False),pid=row['pid'],proof=immediate)
        assert row['pid'] not in signalled_pids, 'Repeated signal prohibited'
        signalled_pids.add(row['pid'])
        try:
            os.kill(row['pid'],signal.SIGTERM)
        except ProcessLookupError:
            return dict(sent=False,gone=True,pid=row['pid'])
        event=dict(sent=True,signal='SIGTERM',number=int(signal.SIGTERM),pid=row['pid'],utc=now(),identityProof=immediate)
        run['signalsSent'].append(event)
        process_event('signal',**event)
        return event
    with (OUT/'editor.stdout.log').open('x') as stdout,(OUT/'editor.stderr.log').open('x') as stderr:
        process=subprocess.Popen(argv,cwd=ROOT,stdout=stdout,stderr=stderr)
        owned.add(process.pid)
        owned_ancestry[process.pid]=dict(origin='Unity',chain=[process.pid])
        run.update(unityLaunchCount=1,editorPid=process.pid,firstFailure=None)
        birth=current_identity(process.pid)
        run['rootBirthIdentity']=birth
        assert birth['command']==' '.join(argv) and birth['comm']==EDITOR and birth['queryExitCodes']==[0,0,0], 'Launched root identity mismatch'
        process_event('launch',pid=process.pid,argv=argv,birth=birth)
        process_event('deadline',deadlineUtc=(datetime.datetime.fromisoformat(run['startedUtc'])+datetime.timedelta(seconds=TIMEOUT)).isoformat(),timeoutSeconds=TIMEOUT)
        write(OUT/'run.json',run)
        print(json.dumps(dict(status='RUNNING',pid=process.pid,output=str(OUT),timeoutSeconds=TIMEOUT)),flush=True)
        outcome=supervise_root(process.poll,compile_failure,observe,wait_step,lambda:time.monotonic()-start,TIMEOUT)
        run['monitorOutcome']=outcome
        if outcome['kind']!='ROOT_EXIT_ZERO':
            run['firstFailure']=dict(outcome,observedUtc=now())
            process_event('first-failure',failure=run['firstFailure'])
            # Persist the first reason before cleanup, even if a later cleanup operation fails.
            write(OUT/'run.json',run)
        if outcome['kind']=='TIMEOUT':
            run.update(timedOut=True,timeoutUtc=now(),timeoutElapsedSeconds=outcome['elapsedSeconds'])
            process_event('timeout',pid=process.pid)
            snapshot=observe(True)
            run['timeoutSnapshot']=dict(utc=now(),elapsedSeconds=time.monotonic()-start,
                        source=frozen_now(before['source']),resources=resources(),delta=frozen_now(before['delta']),
                        processTree=snapshot,lastProgress=event_trace(process.pid),rootBirthIdentity=birth)
            write(OUT/'run.json',run)
        process_event('cleanup-start',reason=outcome['kind'],gentleSeconds=60,terminationSeconds=30)
        cleanup=settle_owned(outcome['kind'],process.pid,remaining_snapshot,verify,gentle,terminate,
                             wait_step,lambda:time.monotonic()-start)
        process_event('cleanup-end',status=cleanup['status'],durationSeconds=cleanup['durationSeconds'])
        run['cleanup']=cleanup
    final=observe(True)
    quiescent=not final['ownedProcesses'] and not final['unityProcesses'] and cleanup['status']=='COMPLETE'
    run['exitCode']=process.poll()
    if process.poll() is None:
        process_event('manual-cleanup-required',pid=process.pid)
    create_json(OUT/'quiescence.json',dict(utc=now(),quiescent=quiescent,ownedPids=sorted(owned),
                ownedIdentities=owned_identities,ownedAncestry=owned_ancestry,
                finalObservation=final,rootExitCode=process.poll(),signalsSent=run['signalsSent'],cleanup=cleanup,
                firstFailure=run['firstFailure'],totalObservationSeconds=time.monotonic()-start))
    run['observationCompletedSeconds']=time.monotonic()-start
    trace=event_trace(process.pid,True)
    logs={}
    for name in ['unity.log','editor.stdout.log','editor.stderr.log']:
        p=OUT/name
        logs[name]=p.read_bytes()[:MAX_LOG+1].decode('utf-8',errors='replace') if p.exists() else ''
    log=logs['unity.log']
    combined_logs='\n'.join(logs.values())
    callback_errors=[s for s in combined_logs.splitlines() if 'BLOCKED_PROGRESS_EVIDENCE' in s]
    compiler_errors=[s for s in combined_logs.splitlines() if re.search(r'error CS\d+|Compilation failed|Unhandled Exception',s)]
    runtime_errors=[s for s in combined_logs.splitlines() if not s.startswith('##utp:') and re.search(
        r'MissingReferenceException|SerializationException|Failed to deserialize|NullReferenceException|ObjectDisposedException|AssertionException|Assertion failed|Cannot change the sibling position|Cannot set the parent|while activating or deactivating|Error[^\n]*serializ|serialized file[^\n]*(?:corrupt|invalid)|referenced script[^\n]*(?:missing|unknown)|Missing \(Mono Script\)',s,re.I)]
    current_xml=xml_result(OUT/'tests.xml',expected)
    after=dict(utc=now(),source=frozen_now(before['source']),products=frozen_now(before['products']),
               resources=resources(),delta=frozen_now(before['delta']),font=font_structure(),
               protectedClosedSet=protected_files(),oldEvidence=compact_trees(old_tree_snapshots()),
               freeBytes=shutil.disk_usage(ROOT).free,
               gitFiles=[identity(ROOT/'.git/HEAD'),identity(ROOT/'.git/refs/heads/master')],
               sceneTemplateSettingsAbsent=not (ROOT/'ProjectSettings/SceneTemplateSettings.json').exists())
    create_json(OUT/'after.json',after)
    valid_prefix=not trace['errors'] and not callback_errors and trace['rows']>0
    finished_counter=collections.Counter(e['fullName'] for e in trace['finished'])
    checks=dict(exitZero=process.returncode==0,within360=not run['timedOut'] and run.get('launchToExitObservationSeconds',float('inf'))<=TIMEOUT,
                xmlExact12Passed=current_xml['passed'],progressPrefixValid=valid_prefix,
                finishedCounterExact12=finished_counter==collections.Counter(expected),
                finishedAllPassed=all(e['result']=='Passed' for e in trace['finished']),
                startedCounterExact12=trace['startedCount']==len(expected) and not trace['repeatedStarts'] and not trace['unmatchedStarted'] and finished_counter==collections.Counter(expected),
                realRunStarted=trace['runStarted']==1,
                realRunFinished=trace['runFinished']==1 and trace.get('runResult')=='Passed',
                noUnresolvedFullnames=not trace['unresolvedFullnames'],quiescent=quiescent,cleanupComplete=cleanup['status']=='COMPLETE',
                source53Unchanged=after['source']==before['source'],product29Unchanged=after['products']==before['products'],
                resources37Unchanged=after['resources']==before['resources'],diagnosticDeltaUnchanged=after['delta']==before['delta'],
                font398Glyph398Atlas4=after['font']==before['font'] and after['font']['characterCount']==after['font']['glyphCount']==398 and after['font']['textureCount']==4,
                protectedClosedSetUnchanged=after['protectedClosedSet']==before['protectedClosedSet'],
                oldEvidenceUnchanged=after['oldEvidence']==before['oldEvidence'],gitFilesUnchanged=after['gitFiles']==before['gitFiles'],
                noCompilerErrors=not compiler_errors,noRuntimeErrors=not runtime_errors,
                graphicsMetal=bool(re.search(r'(?m)^Initializing Metal device caps: Apple ',log)) and not bool(re.search(r'Forcing GfxDevice: Null|NullGfxDevice:|Renderer: Null Device|Renderer:[^\n]*(?:software|swiftshader|llvmpipe)',log,re.I)),
                budgets=not budget_violations and after['freeBytes']>=MIN_FREE,
                sceneTemplateSettingsAbsent=after['sceneTemplateSettingsAbsent'])
    progress_intervals=[b-a for a,b in zip(progress_times,progress_times[1:])]
    run.update(checks=checks,passed=all(checks.values()),testEvents=trace,xml=current_xml,
               callbackErrors=callback_errors,compilerErrors=compiler_errors,runtimeErrors=runtime_errors,
               resourceBudgetViolations=budget_violations,xmlFirstObservedUtc=xml_first_seen,
               maximumProgressIntervalSeconds=max(progress_intervals,default=0),
               graphicsEvidence=[s for s in log.splitlines() if re.search(r'GfxDevice|^Using device |^Initializing Metal device caps:|^\s*Renderer:',s)],
               runFinishedLogMarkers=[s for s in log.splitlines() if 'Saving results to:' in s or 'RunFinished' in s])
    all_cases=retained['cases']+host_result['cases']+retained178+current_xml['cases']
    actual_counter=collections.Counter(t['fullname'] for t in all_cases)
    expected_counter=collections.Counter(all222)
    grouped=dict(passed=run['passed'] and actual_counter==expected_counter and all(t['result']=='Passed' for t in all_cases),
                 actualCount=len(all_cases),expectedCount=len(all222),counterExact=actual_counter==expected_counter,
                 missing=list((expected_counter-actual_counter).elements()),unexpected=list((actual_counter-expected_counter).elements()),
                 results={t['fullname']:t['result'] for t in all_cases},
                 evidence=[retained.get('identity'),host_result.get('identity'),identity(R19/'tests.xml'),current_xml.get('identity')],
                 reusedDifferentRuns=True,retainedCore190PassedSubsetCount=len(retained178),replacedOldClassTestsCount=12,
                 releaseOptimizationInFIX19AndThisRun=True,originalFIX19SixFailuresPreserved=True,
                 originalExact222TimeoutRemainsFailure=True,nativeReopenRun=False)
    by_name={t['fullname']:t for t in all_cases}
    grouped['coverage']={label:{n:by_name.get(n,{}).get('result','NOT_OBSERVED') for n in names}
                         for label,names in [('fixed28',pre['expectedTests28']),('lifecycle4',pre['expectedTests4']),
                                            ('parameterized6',[n for n in pre['expectedTests28'] if '(' in n])]}
    b16=next((t for t in all_cases if '.B16B04_Cancel' in t['fullname']),None)
    wire=next((t for t in all_cases if '.UGUI_COPY_WIRE01_' in t['fullname']),None)
    grouped['b16Matrix']=bool(b16 and b16['result']=='Passed' and all('uGUI cancellation: '+cause in b16['output'] for cause in ['cancel','captureout','detach','close','geometry','transform','selection','outside']))
    grouped['wireTerminalRetry']=bool(wire and wire['result']=='Passed' and 'FIX13 battle-dispose + pending: original operation committed once; terminal view remains unbound; BattleViewDisposed visible.' in wire['output'])
    grouped['passed']=grouped['passed'] and grouped['b16Matrix'] and grouped['wireTerminalRetry']
    create_json(OUT/'grouped-exact222-coverage.json',grouped)
    geometry_failures = []
    business_failures = []
    teardown_failures = []
    for case in current_xml['cases']:
        if case['result'] == 'Passed':
            continue
        message = case.get('message') or ''; stack = case.get('stack') or ''
        detail = dict(fullname=case['fullname'],message=message,stack=stack,output=case.get('output') or '')
        if 'TearDown' in message or 'ExitControlledPlayModeAfterFailure' in stack:
            teardown_failures.append(detail)
        if ('Board geometry is unavailable' in message or
            any(marker in stack for marker in ['AssertBoardReady', 'UguiHostRig+<Ready>', 'UguiHostRig.Ready']) or
            ('.BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical' in case['fullname'] and
             'Expected: greater than 0' in message)):
            geometry_failures.append(detail)
        else:
            business_failures.append(detail)
    run['failureBranches'] = dict(activationOrGeometry=geometry_failures,otherTestFailures=business_failures,teardown=teardown_failures)
    first_kind=(run.get('firstFailure') or {}).get('kind')
    if first_kind=='COMPILE_FAILURE': status='BLOCKED_COMPILE'
    elif geometry_failures: status='BLOCKED_PRODUCT_ACTIVATION_OR_GEOMETRY'
    elif first_kind=='ROOT_EXIT_NONZERO': status='BLOCKED_ROOT_NONZERO'
    elif first_kind=='TIMEOUT': status='BLOCKED_EXACT12_TIMEOUT'
    elif not quiescent: status='BLOCKED_MANUAL_PROCESS_CLEANUP'
    elif not valid_prefix: status='BLOCKED_PROGRESS_EVIDENCE'
    elif run['passed'] and grouped['passed']: status='EXACT12_PASS_GROUPED222_EVIDENCE_READY'
    else: status='BLOCKED_EXACT12_VALIDATION'
    run['status']=status
    write(OUT/'run.json',run)
    boundary=('compilation before test execution' if first_kind=='COMPILE_FAILURE' else
              'root exited nonzero' if first_kind=='ROOT_EXIT_NONZERO' else
              'progress evidence unavailable or invalid' if not valid_prefix else
              'unfinished test boundary' if trace['unresolvedFullnames'] else
              'runner finalization boundary' if trace['finishedCount']==len(expected) and not (trace['runFinished'] and current_xml['exists'] and process.poll() is not None) else
              'next scheduling or domain-resume boundary' if not run['passed'] else 'completed')
    receipt=dict(status=status,owner=OWNER,returnThread='01a0e401-511d-79f2-b47f-3ab0ade1681b/local',
                 prBinding=json.loads((OUT/'activation.json').read_text())['prBinding'],
                 endedUtc=now(),unityLaunchCount=1,exitCode=run['exitCode'],timedOut=run['timedOut'],
                 firstFailure=run.get('firstFailure'),cleanup=cleanup,failureBranches=run['failureBranches'],
                 launchToExitObservationSeconds=run.get('launchToExitObservationSeconds'),totalWallSeconds=time.monotonic()-start,
                 boundary=boundary,lastFinished=trace['lastFinished'],unmatchedStarted=trace['unmatchedStarted'],
                 unresolvedFullnames=trace['unresolvedFullnames'],finishedCount=trace['finishedCount'],eventRows=trace['rows'],
                 domainCount=len(trace['domains']),checks=checks,failedChecks=[k for k,v in checks.items() if not v],
                 grouped222Passed=grouped['passed'],allOwnedProcessesExited=quiescent,signalsSent=run['signalsSent'],
                 resourceCounts=dict(characters=after['font']['characterCount'],glyphs=after['font']['glyphCount'],atlases=after['font']['textureCount']),
                 evidence=[identity(p) for p in sorted(OUT.rglob('*')) if p.is_file()],
                 notRun=['extra compile','Core190','Host30','exact2','split runs','native reopen','APK','source repair','Git'],
                 review='GitHub review is independent and pending; this receipt is not author acceptance.')
    create_json(OUT/'final-receipt.json',receipt)
    print(json.dumps({k:receipt[k] for k in ['status','exitCode','timedOut','finishedCount','eventRows','domainCount','boundary','lastFinished','unresolvedFullnames','failedChecks','allOwnedProcessesExited']},ensure_ascii=False),flush=True)
    return 0 if status=='EXACT12_PASS_GROUPED222_EVIDENCE_READY' else 3

if __name__=='__main__':
    try:
        sys.exit(execute())
    except Exception as exc:
        # Preserve all existing artifacts; never launch again or repair source from this runner.
        saved_run=json.loads((OUT/'run.json').read_text()) if (OUT/'run.json').exists() else {}
        first=saved_run.get('firstFailure')
        primary={'COMPILE_FAILURE':'BLOCKED_COMPILE','ROOT_EXIT_NONZERO':'BLOCKED_ROOT_NONZERO','TIMEOUT':'BLOCKED_EXACT12_TIMEOUT'}.get((first or {}).get('kind'),'BLOCKED_RUNNER_EXCEPTION')
        error=dict(status=primary,owner=OWNER,utc=now(),firstFailure=first,
                   cleanupStatus='BLOCKED_RUNNER_EXCEPTION',errorType=type(exc).__name__,error=str(exc),rerun=False)
        if not (OUT/'final-receipt.json').exists():
            create_json(OUT/'final-receipt.json',error)
        print(json.dumps(error,ensure_ascii=False),flush=True)
        raise
