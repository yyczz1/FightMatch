#!/opt/homebrew/bin/python3
"""Reviewed logic template only. Central may fill only the marked literal data."""
import hashlib
import json
from pathlib import Path
import sys
import types

# CENTRAL_LITERAL_CONFIG_BEGIN
CONFIG = None
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
