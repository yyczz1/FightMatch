#!/opt/homebrew/bin/python3
"""Reviewed logic template only. Central may fill only the marked literal data."""
import hashlib
import json
from pathlib import Path
import sys
import types

# CENTRAL_LITERAL_CONFIG_BEGIN
CONFIG = {'driver': {'path': '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix06/source/m17_driver.py', 'bytes': 236347, 'sha256': '715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1'}, 'activation': {'path': '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/docs/team/2026-09-30/central-loc-m17-m08-activation.json', 'bytes': 22834, 'sha256': '2513f98accf91f61344bf1dd26a2ec35a2b6f34a44c6810b4ddef9c3d8e52e48', 'centralThread': '01a0e401-511d-79f2-b47f-3ab0ade1681b', 'centralHost': 'local', 'centralTurn': '01a12e46-6af4-7b82-958d-d18c1146d85d', 'centralItem': '01a12e46-6b4a-7fb2-aea1-235e64f1268f', 'ownerThread': '01a0f40d-b0c5-7bc0-a2b2-be9d1213648a', 'ownerHost': 'local', 'ownerTurn': '01a12e47-da22-7b32-a9be-046bc0919c4b'}, 'argv': ['/opt/homebrew/Cellar/python@3.13/3.13.7/Frameworks/Python.framework/Versions/3.13/Resources/Python.app/Contents/MacOS/Python', '-I', '-B', '/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/docs/team/2026-09-30/central-loc-m17-m08-host.py', 'execute']}
# CENTRAL_LITERAL_CONFIG_END

IDENTITIES = {'centralThread','centralHost','centralTurn','centralItem','ownerThread','ownerHost','ownerTurn'}
def require(ok,message):
    if not ok:raise RuntimeError('BLOCKED: '+message)
def literal_data(value):
    if type(value) is dict:
        return all(type(k) is str and literal_data(v) for k,v in value.items())
    if type(value) is list:return all(literal_data(v) for v in value)
    return value is None or type(value) in (str,int,bool)
def canonical_path(value):
    require(type(value) is str and value and not any(c in value for c in '\0\r\n'),'invalid path type')
    path=Path(value)
    require(path.is_absolute() and str(path)==value and '..' not in path.parts,'noncanonical absolute path')
    require(not any(p.is_symlink() for p in (path,*path.parents)),'file or ancestor symlink')
    return path
def verified_file(record,extra=()):
    require(type(record) is dict and set(record)=={'path','bytes','sha256'}|set(extra),'file identity keys')
    require(type(record['bytes']) is int and record['bytes']>0,'file length type')
    digest=record['sha256']
    require(type(digest) is str and len(digest)==64 and all(c in '0123456789abcdef' for c in digest),'file digest type')
    path=canonical_path(record['path'])
    require(path.is_file() and path.stat().st_size==record['bytes'],'file kind/length')
    data=path.read_bytes()
    require(len(data)==record['bytes'] and hashlib.sha256(data).hexdigest()==digest,'file bytes/hash')
    return path,data
def main():
    require(CONFIG is not None,'central CONFIG is absent; no execution authority')
    require(literal_data(CONFIG) and type(CONFIG) is dict and set(CONFIG)=={'driver','activation','argv'},'literal CONFIG shape')
    argv=CONFIG['argv']
    require(type(argv) is list and argv and all(type(v) is str for v in argv),'argv type')
    require(list(sys.orig_argv)==argv,'actual argv differs')
    activation=CONFIG['activation']
    require(type(activation) is dict and all(type(activation.get(k)) is str and activation[k] for k in IDENTITIES),'seven identity fields')
    verified_file(activation,IDENTITIES)
    driver_path,driver_bytes=verified_file(CONFIG['driver'])
    module=types.ModuleType('_fightmatch_m17_authorized_driver')
    module.__file__=str(driver_path)
    exec(compile(driver_bytes,str(driver_path),'exec'),module.__dict__)
    require('TRUSTED_CENTRAL_ACTIVATION' in module.__dict__ and module.TRUSTED_CENTRAL_ACTIVATION is None,'driver pin not initially null')
    module.TRUSTED_CENTRAL_ACTIVATION=dict(activation)
    result=module.execute(list(sys.orig_argv))
    if type(result) is int:return result
    require(type(result) is dict and result.get('status')=='M_MATERIAL_CANDIDATE_NOT_ACCEPTED','unsupported driver return')
    print(json.dumps(result,ensure_ascii=False))
    return 0

if __name__=='__main__':
    sys.exit(main())
