"""Deterministic extraction only; artwork originates from image_gen."""
from pathlib import Path
import importlib.util, json, shutil
import numpy as np
from collections import deque
from PIL import Image

ROOT = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('sprite_processor', '/Users/elliotyip/.codex/skills/generate2dsprite/scripts/generate2dsprite.py')
processor = importlib.util.module_from_spec(spec)
spec.loader.exec_module(processor)
sources = {'stand': 'exec-e3615f4e-80b6-4fc4-9fb6-221297956945.png', 'attack': 'exec-19350289-0fa4-4c11-ba59-464dea5db140.png'}
names = ['warrior-a','warrior-b','warrior-c','infantry-a','infantry-b','infantry-c']
records=[]
for pose, source in sources.items():
    raw=ROOT/'raw'/f'{pose}.png'; raw.parent.mkdir(exist_ok=True)
    shutil.copy2(Path('/Users/elliotyip/.codex/generated_images/01a0f026-e238-7522-be19-a3441f0fa5fb')/source,raw)
    clean=processor.remove_bg_magenta(Image.open(raw).convert('RGBA'))
    arr=np.array(clean); arr[:,:,3]=np.where(arr[:,:,3]>127,255,0); clean=Image.fromarray(arr)
    components=processor.connected_components(clean,min_area=1000)
    assert len(components)==6, components
    components=sorted(components,key=lambda c: (int(c['bbox'][1]>600),c['bbox'][0]))
    for name,c in zip(names,components):
        box=c['bbox']; crop=clean.crop(box)
        data=np.array(crop); alive=data[:,:,3]>0; seen=np.zeros_like(alive); groups=[]
        for cy,cx in zip(*np.where(alive)):
            if seen[cy,cx]: continue
            q=deque([(int(cx),int(cy))]); seen[cy,cx]=True; group=[]
            while q:
                x,y=q.popleft(); group.append((x,y))
                for nx,ny in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                    if 0<=nx<crop.width and 0<=ny<crop.height and alive[ny,nx] and not seen[ny,nx]:
                        seen[ny,nx]=True; q.append((nx,ny))
            groups.append(group)
        keep=max(groups,key=len); data[:,:,3]=0
        for x,y in keep: data[y,x,3]=255
        crop=Image.fromarray(data)
        outdir=ROOT/'processed'/name/pose; outdir.mkdir(parents=True,exist_ok=True)
        # One magnification for all candidates and both poses. No bbox-fit scaling.
        size=tuple(round(v*0.15) for v in crop.size)
        small=crop.resize(size,Image.Resampling.NEAREST)
        alpha=np.array(small)[:,:,3]; yy,xx=np.where(alpha>0)
        feet=xx[yy>=yy.max()-7]; rootx=int(round((int(feet.min())+int(feet.max()))/2))
        px,py=48-rootx,82-int(yy.max())
        assert px>=2 and px+size[0]<=94 and py>=2
        canvas=Image.new('RGBA',(96,96)); canvas.paste(small,(px,py))
        canvas.save(outdir/'input.png')
        records.append({'name':name,'pose':pose,'source':str(raw.relative_to(ROOT)),'source_bbox':box,'source_edge_touch':c['touches_edge'],'scale':0.15,'paste':[px,py],'bbox':canvas.getbbox(),'input':str((outdir/'input.png').relative_to(ROOT))})
(ROOT/'extraction.json').write_text(json.dumps(records,indent=2))
print(json.dumps(records,indent=2))
