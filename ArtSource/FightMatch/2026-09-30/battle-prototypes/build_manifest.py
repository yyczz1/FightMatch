"""Inventory all deliverables without self-referential manifest hashing."""
from pathlib import Path
import json,hashlib,struct
from PIL import Image
ROOT=Path(__file__).resolve().parent
NAMES=['warrior-a','warrior-b','warrior-c','infantry-a','infantry-b','infantry-c']
entries=[]
for p in sorted(ROOT.rglob('*')):
    if not p.is_file() or p.name=='manifest.json' or '__pycache__' in p.parts: continue
    rel=p.relative_to(ROOT).as_posix(); data=p.read_bytes(); dims=None
    final=p.stem in NAMES or any(p.name==n+'-'+pose+'.png' for n in NAMES for pose in ['stand','attack'])
    if p.suffix in ['.png','.gif']:
        with Image.open(p) as im: dims=list(im.size)
    elif p.suffix=='.aseprite':
        assert struct.unpack_from('<H',data,4)[0]==0xA5E0
        frames,w,h=struct.unpack_from('<HHH',data,6); dims=[w,h]
        assert [w,h,frames]==[96,96,2]
    if final: source='image_gen -> magenta/component extraction -> nearest neighbor 0.15 -> Aseprite MCP palette and contour cleanup'
    elif rel.startswith('raw/'): source='OpenAI built-in image_gen; exact model version undisclosed'
    elif rel.startswith('processor-audit-'): source='generate2dsprite fixed-grid audit; rejected as delivery art'
    elif rel.startswith('processed/'): source='deterministic extraction from raw image_gen source; Aseprite input only'
    else: source='task documentation, QC evidence or deterministic composition of Aseprite exports'
    entries.append({'file':rel,'bytes':len(data),'dimensions':dims,'sha256':hashlib.sha256(data).hexdigest(),'source':source,'aseprite_cleanup_status':'completed' if final else 'not_applicable_or_intermediate','user_adoption_status':'pending' if final or p.name.startswith('comparison-') else 'not_applicable','production_asset':False})
manifest={'schema_version':1,'stage':'STYLE_GATE_REVIEW_READY','unit_identity_total':8,'battle_form_total':9,'identity_candidate_total':24,'form_candidate_total':27,'delivered_candidates':6,'delivered_poses':12,'canvas':[96,96],'contact_pixel_y':83,'logical_anchor':[48,84],'palette_file':'README.md','user_adoption_status':'pending','unity_imported':False,'manifest_hash_policy':'manifest.json excludes itself to avoid recursive hashes; every other regular task output is listed','files':entries}
(ROOT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print(f'{len(entries)} files inventoried; six Aseprite headers verified')
