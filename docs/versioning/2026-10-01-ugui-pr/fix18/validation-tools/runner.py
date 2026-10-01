# FIX18 single-run derivative of FIX17; see preflight.json for complete source diff.
import ast
import collections
import datetime
import difflib
import hashlib
import json
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
OUT = pathlib.Path(__file__).resolve().parent.parent
SOURCE_ONLY = ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-18-source'
R17 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf/q4-correction-17'
R15 = ROOT / 'TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-15'
PREDECESSOR = R17 / 'validation-tools/runner.py'
pre = json.loads((R17 / 'preflight.json').read_text())
FONT = ROOT / 'Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset'
TIMEOUT = 600
MAX_LOG = 8 * 1024 * 1024
MAX_EVIDENCE = 192 * 1024 * 1024
MIN_FREE = 2 * 1024 * 1024 * 1024
OWNER = dict(threadId='01a0e404-d89d-7ab2-bece-3cd1df3fbc52', hostId='local', turnId='01a0f66a-4d77-7c91-823f-96495ac85a9e')

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
    return [x for x in rows if x['comm'].endswith('/Unity') or x['comm'] == 'Unity']

def no_unity():
    rows = processes()
    assert not [x for x in rows if pathlib.Path(x['comm']).name in ('Unity','Unity Hub','UnityShaderCompiler','UnityAutoQuitter','UnityCrashHandler','bee_backend') or 'unity-mcp' in x['comm'].lower() or 'Unity Hub.app' in x['comm']], 'Existing Unity family process'

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
    paths = sorted(set([x['root'] for x in pre['oldRoots']] + [str(R17), str(R15), str(SOURCE_ONLY)]))
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
    # -ww prevents long exact190 selectors from being truncated by ps.
    command = subprocess.run(['/bin/ps', '-ww', '-p', str(pid), '-o', 'command='],
                             capture_output=True, text=True)
    result['command'] = command.stdout.strip()
    result['queryExitCodes'][1] = command.returncode
    return result

def guard_root(process, birth, argv):
    current = current_identity(process.pid)
    checks = dict(pid=current['pid'] == birth['pid'],
                  birth=bool(current['birth']) and current['birth'] == birth['birth'],
                  command=current['command'] == birth['command'] == ' '.join(argv),
                  executable=current['comm'] == birth['comm'] == EDITOR,
                  projectPath=argv[argv.index('-projectPath') + 1] == str(ROOT),
                  queriesSucceeded=current['queryExitCodes'] == birth['queryExitCodes'] == [0, 0, 0],
                  ownedHandleStillRunning=process.poll() is None)
    return dict(current=current, checks=checks, allowed=all(checks.values()))

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
    assert OUT == ROOT / 'TestArtifacts/FightMatch/UGUI-01/q4-correction-18'
    assert_non_link(OUT)
    assert set(p.name for p in OUT.iterdir()) == {'activation.json', 'validation-tools'}, 'Formal root is not fresh'
    activation = json.loads((OUT / 'activation.json').read_text())
    assert activation['runner'] == identity(pathlib.Path(__file__))
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
    assert frozen_now(sources) == sources and frozen_now(products) == products and frozen_now(res) == res
    assert pre['resourcesBefore'] == res
    core = json.loads((R17 / 'core-190-graphics/expected-fullnames.json').read_text())
    exact2 = pre['expectedTests2']; host = pre['expectedHost30']; all222 = pre['expectedTests222']
    assert core == pre['expectedCore190']
    groups = dict(exact2=exact2, host30=host, core190=core)
    assert [len(v) for v in groups.values()] == [2, 30, 190] and len(all222) == 222
    assert all(len(v) == len(set(v)) for v in list(groups.values()) + [all222])
    pairs = [(a,b) for i,a in enumerate(groups) for b in list(groups)[i+1:]]
    assert all(not(set(groups[a]) & set(groups[b])) for a,b in pairs)
    assert collections.Counter(exact2 + host + core) == collections.Counter(all222)
    retained = xml_result(R15 / 'exact-2-graphics/tests.xml', exact2)
    host_result = xml_result(R17 / 'host-30-graphics/tests.xml', host)
    assert retained['passed'] and host_result['passed'], 'Retained XML gate failed'
    raw_filter = (R17 / 'core-190-graphics/test-filter.txt').read_bytes()
    literal = '^(?:' + '|'.join(re.escape(n) for n in core) + ')$'
    assert raw_filter == literal.encode('utf-8') and not any(';' in n for n in core)
    assert all(re.fullmatch(literal, n) for n in core)
    assert not any(re.fullmatch(literal, n) for n in exact2 + host)
    assert not any(re.fullmatch(literal, n + '_not_a_test') for n in core)
    no_unity()
    rows = processes()
    prior_run = json.loads((R17 / 'core-190-graphics/run.json').read_text())
    previous_birth = prior_run['rootBirthIdentity']
    prior_alive = [r for r in rows if r['pid'] == previous_birth['pid']]
    prior_identity = current_identity(previous_birth['pid']) if prior_alive else None
    previous_commands=json.loads((R17/'core-190-graphics/owned-command-lines.json').read_text())
    prior_comm={e['row']['pid']:e['row']['comm'] for e in previous_commands}
    prior_owned_matches=[r for r in rows if r['pid'] in prior_comm and r['comm']==prior_comm[r['pid']]]
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
                selector=dict(identity=identity(R17/'core-190-graphics/test-filter.txt'), escaping='re.escape; anchored literal alternation; bytes reused verbatim', positiveCount=len(core), excludedCount=len(host+exact2))))
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
    return before,old_trees,core,exact2,host,all222,retained,host_result

def execute():
    before,old_trees,expected,exact2,host,all222,retained,host_result = preflight()
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
    run = dict(task='FIX18',owner=OWNER,argv=argv,exactCommand=shlex.join(argv),
               startedUtc=now(),timeoutSeconds=TIMEOUT,expectedTestCount=len(expected),unityLaunchCount=0,
               timedOut=False,signalsSent=[],exitCode=None,passed=False)
    create_json(OUT/'run.json',run)
    owned = set()
    owned_identities = {}
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
        for _ in range(20):
            children = {r['pid'] for r in rows if r['ppid'] in owned}
            if children <= owned:
                break
            owned.update(children)
        for row in rows:
            if row['pid'] in owned and row['pid'] not in owned_identities:
                owned_identities[row['pid']] = current_identity(row['pid'])
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
    def wait_root(deadline):
        while process.poll() is None:
            left=deadline-time.monotonic()
            if left<=0: return False
            observe()
            try:
                process.wait(timeout=min(1,left))
            except subprocess.TimeoutExpired:
                pass
        return True
    with (OUT/'editor.stdout.log').open('x') as stdout,(OUT/'editor.stderr.log').open('x') as stderr:
        process=subprocess.Popen(argv,cwd=ROOT,stdout=stdout,stderr=stderr)
        owned.add(process.pid)
        run.update(unityLaunchCount=1,editorPid=process.pid)
        birth=current_identity(process.pid)
        run['rootBirthIdentity']=birth
        process_event('launch',pid=process.pid,argv=argv,birth=birth)
        process_event('deadline',deadlineUtc=(datetime.datetime.fromisoformat(run['startedUtc'])+datetime.timedelta(seconds=TIMEOUT)).isoformat(),timeoutSeconds=TIMEOUT)
        write(OUT/'run.json',run)
        print(json.dumps(dict(status='RUNNING',pid=process.pid,output=str(OUT),timeoutSeconds=TIMEOUT)),flush=True)
        natural=wait_root(start+TIMEOUT)
        if not natural:
            run.update(timedOut=True,timeoutUtc=now(),timeoutElapsedSeconds=time.monotonic()-start)
            process_event('timeout',pid=process.pid)
            snapshot=observe(True)
            create_json(OUT/'timeout-snapshot.json',dict(utc=now(),elapsedSeconds=time.monotonic()-start,
                        source=frozen_now(before['source']),resources=resources(),delta=frozen_now(before['delta']),
                        processTree=snapshot,lastProgress=event_trace(process.pid),rootBirthIdentity=birth))
            natural_deadline=start+TIMEOUT+60
            process_event('natural-observation-start',limitSeconds=60)
            wait_root(natural_deadline)
            if process.poll() is None:
                guarded=guard_root(process,birth,argv)
                run['signalGuard']=guarded
                if guarded['allowed']:
                    event=dict(signal='SIGTERM',number=int(signal.SIGTERM),pid=process.pid,utc=now())
                    process.send_signal(signal.SIGTERM)
                    run['signalsSent'].append(event)
                    process_event('signal',**event)
                    wait_root(time.monotonic()+30)
                else:
                    process_event('signal-blocked',guard=guarded)
        run.update(exitCode=process.poll(),processExitObservedUtc=now() if process.poll() is not None else None,
                   launchToExitObservationSeconds=time.monotonic()-start)
        if process.poll() is not None:
            process_event('process-exit',pid=process.pid,exitCode=process.returncode,afterDeadline=run['timedOut'])
        else:
            process_event('manual-cleanup-required',pid=process.pid)
    final=observe(True)
    cleanup=dict(status='NOT_RUN',signalsSent=[],launchCount=0)
    # FIX17's exact, pipe-specific compiler-server cleanup, with output kept in the permitted quiescence file.
    remaining=final['ownedProcesses']
    if process.poll() is not None and len(remaining)==1 and remaining[0]['ppid']==1 and pathlib.Path(remaining[0]['comm']).name=='dotnet' and not final['unityProcesses']:
        server=remaining[0]
        dotnet=pathlib.Path(EDITOR).parents[1]/'NetCoreRuntime/dotnet'
        compiler=pathlib.Path(EDITOR).parents[1]/'DotNetSdkRoslyn/VBCSCompiler.dll'
        current=current_identity(server['pid'])
        args=shlex.split(current['command'])
        pipe=[a for a in args if a.startswith('-pipename:')]
        lsof=subprocess.run(['/usr/sbin/lsof','-a','-p',str(server['pid']),'-d','txt','-Fn'],capture_output=True,text=True)
        original_code=PREDECESSOR.read_text()
        expected_tools=ast.literal_eval(re.search(r"tools_match = .* == (\[.*\])",original_code).group(1))
        actual_tools=[(p.stat().st_size,sha(p.read_bytes())) for p in [dotnet,compiler]]
        guards=dict(birth=current['birth']==owned_identities[server['pid']]['birth'],
                    exactCommand=len(args)==4 and args[0] in ['dotnet',str(dotnet)] and args[1]=='exec' and args[2]==str(compiler) and len(pipe)==1 and args[3]==pipe[0] and re.fullmatch(r'-pipename:[A-Za-z0-9_-]+',pipe[0]) is not None,
                    executable=lsof.returncode==0 and ('n'+str(dotnet)) in lsof.stdout.splitlines(),
                    toolHashesMatch=actual_tools==expected_tools)
        cleanup.update(serverPid=server['pid'],guards=guards)
        if all(guards.values()) and not run['timedOut'] and time.monotonic()-start<TIMEOUT:
            command=[str(dotnet),'exec',str(compiler),pipe[0],'-shutdown']
            cleanup.update(status='REQUESTED',argv=command,launchCount=1,startedUtc=now())
            process_event('compiler-server-graceful-shutdown',pid=server['pid'],argv=command)
            client=subprocess.Popen(command,cwd=ROOT,stdout=subprocess.PIPE,stderr=subprocess.PIPE)
            owned.add(client.pid)
            deadline=min(start+TIMEOUT,time.monotonic()+30)
            while client.poll() is None and time.monotonic()<deadline:
                observe()
                try: client.wait(timeout=min(1,max(.001,deadline-time.monotonic())))
                except subprocess.TimeoutExpired: pass
            if client.poll() is not None:
                so,se=client.communicate()
                cleanup.update(exitCode=client.returncode,stdout=so.decode(errors='replace'),stderr=se.decode(errors='replace'))
            cleanup['endedUtc']=now()
        else: cleanup['status']='BLOCKED_IDENTITY_OR_BUDGET'
    deadline=(start+TIMEOUT if not run['timedOut'] else time.monotonic()+min(5,max(0,start+TIMEOUT+90-time.monotonic())))
    while (final['ownedProcesses'] or final['unityProcesses']) and time.monotonic()<deadline:
        time.sleep(min(1,max(.001,deadline-time.monotonic())))
        final=observe()
    final=observe(True)
    quiescent=not final['ownedProcesses'] and not final['unityProcesses']
    if cleanup['status']=='REQUESTED':
        cleanup['status']='COMPLETE' if quiescent and cleanup.get('exitCode')==0 else 'BLOCKED_COMPILER_SERVER_CLEANUP'
    create_json(OUT/'quiescence.json',dict(utc=now(),quiescent=quiescent,ownedPids=sorted(owned),ownedIdentities=owned_identities,
                finalObservation=final,rootExitCode=process.poll(),signalsSent=run['signalsSent'],compilerServerCleanup=cleanup,
                totalObservationSeconds=time.monotonic()-start))
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
    checks=dict(exitZero=process.returncode==0,within600=not run['timedOut'] and run['launchToExitObservationSeconds']<=TIMEOUT and run['observationCompletedSeconds']<=TIMEOUT,
                xmlExact190Passed=current_xml['passed'],progressPrefixValid=valid_prefix,
                finishedCounterExact190=finished_counter==collections.Counter(expected),
                finishedAllPassed=all(e['result']=='Passed' for e in trace['finished']),
                realRunFinished=trace['runFinished']==1 and trace.get('runResult')=='Passed',
                noUnresolvedFullnames=not trace['unresolvedFullnames'],quiescent=quiescent,
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
    all_cases=retained['cases']+host_result['cases']+current_xml['cases']
    actual_counter=collections.Counter(t['fullname'] for t in all_cases)
    expected_counter=collections.Counter(all222)
    grouped=dict(passed=run['passed'] and actual_counter==expected_counter and all(t['result']=='Passed' for t in all_cases),
                 actualCount=len(all_cases),expectedCount=len(all222),counterExact=actual_counter==expected_counter,
                 missing=list((expected_counter-actual_counter).elements()),unexpected=list((actual_counter-expected_counter).elements()),
                 results={t['fullname']:t['result'] for t in all_cases},
                 evidence=[retained.get('identity'),host_result.get('identity'),current_xml.get('identity')],
                 reusedDifferentRuns=True,diagnosticDeltaOnlyInCore190=True,releaseOptimizationOnlyInThisRun=True,
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
    if compiler_errors: status='BLOCKED_COMPILE'
    elif not valid_prefix: status='BLOCKED_PROGRESS_EVIDENCE'
    elif run['timedOut']: status='BLOCKED_CORE190_TIMEOUT'
    elif not quiescent: status='BLOCKED_MANUAL_PROCESS_CLEANUP'
    elif run['passed'] and grouped['passed']: status='CORE190_PASS_GROUPED222_EVIDENCE_READY'
    else: status='BLOCKED_CORE190_VALIDATION'
    run['status']=status
    write(OUT/'run.json',run)
    boundary=('progress evidence unavailable or invalid' if not valid_prefix else
              'unfinished test boundary' if trace['unresolvedFullnames'] else
              'runner finalization boundary' if trace['finishedCount']==len(expected) and not (trace['runFinished'] and current_xml['exists'] and process.poll() is not None) else
              'next scheduling or domain-resume boundary' if not run['passed'] else 'completed')
    receipt=dict(status=status,owner=OWNER,returnThread='01a0e401-511d-79f2-b47f-3ab0ade1681b/local',
                 prBinding=json.loads((OUT/'activation.json').read_text())['prBinding'],
                 endedUtc=now(),unityLaunchCount=1,exitCode=run['exitCode'],timedOut=run['timedOut'],
                 launchToExitObservationSeconds=run['launchToExitObservationSeconds'],totalWallSeconds=time.monotonic()-start,
                 boundary=boundary,lastFinished=trace['lastFinished'],unmatchedStarted=trace['unmatchedStarted'],
                 unresolvedFullnames=trace['unresolvedFullnames'],finishedCount=trace['finishedCount'],eventRows=trace['rows'],
                 domainCount=len(trace['domains']),checks=checks,failedChecks=[k for k,v in checks.items() if not v],
                 grouped222Passed=grouped['passed'],allOwnedProcessesExited=quiescent,signalsSent=run['signalsSent'],
                 resourceCounts=dict(characters=after['font']['characterCount'],glyphs=after['font']['glyphCount'],atlases=after['font']['textureCount']),
                 evidence=[identity(p) for p in sorted(OUT.rglob('*')) if p.is_file()],
                 notRun=['extra compile','Host30','exact2','split runs','native reopen','APK','source repair','Git'],
                 review='GitHub review is independent and pending; this receipt is not author acceptance.')
    create_json(OUT/'final-receipt.json',receipt)
    print(json.dumps({k:receipt[k] for k in ['status','exitCode','timedOut','finishedCount','eventRows','domainCount','boundary','lastFinished','unresolvedFullnames','failedChecks','allOwnedProcessesExited']},ensure_ascii=False),flush=True)
    return 0 if status=='CORE190_PASS_GROUPED222_EVIDENCE_READY' else 3

if __name__=='__main__':
    try:
        sys.exit(execute())
    except Exception as exc:
        # Preserve all existing artifacts; never launch again or repair source from this runner.
        error=dict(status='BLOCKED_RUNNER_EXCEPTION',owner=OWNER,utc=now(),
                   errorType=type(exc).__name__,error=str(exc),rerun=False)
        if not (OUT/'final-receipt.json').exists():
            create_json(OUT/'final-receipt.json',error)
        print(json.dumps(error,ensure_ascii=False),flush=True)
        raise
