"""QC and contact-sheet composition of Aseprite exports; does not draw sprite art."""
from pathlib import Path
import json, hashlib
import numpy as np
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parent
NAMES=['warrior-a','warrior-b','warrior-c','infantry-a','infantry-b','infantry-c']
PALETTE=['20343d','39535b','287e82','55b3a0','f4e6bc','c5ba97','ac773f','e2b85b','b65551','ed886b','c28b60','f2c98c','715444','75969c','b6d0ca']
allowed={tuple(bytes.fromhex(c)) for c in PALETTE}
checks=[]
sheet=Image.new('RGB',(6*304,680),'#eee6d2'); d=ImageDraw.Draw(sheet)
font=ImageFont.truetype('/System/Library/Fonts/Menlo.ttc',17)
small=ImageFont.truetype('/System/Library/Fonts/Menlo.ttc',13)
one=Image.new('RGB',(6*104,224),'#eee6d2')
for i,name in enumerate(NAMES):
    d.text((i*304+12,12),name.upper(),font=font,fill='#20343d')
    for j,pose in enumerate(['stand','attack']):
        img=Image.open(ROOT/f'{name}-{pose}.png').convert('RGBA'); arr=np.array(img)
        a=arr[:,:,3]; y,x=np.where(a>0); colors={tuple(v) for v in arr[a>0,:3]}
        bbox=img.getbbox(); foot=x[y>=y.max()-7]
        src=np.array(Image.open(ROOT/'processed'/name/pose/'input.png').convert('RGBA'))[:,:,3]>0
        expected=src.copy()
        expected[1:,:]|=src[:-1,:]; expected[:-1,:]|=src[1:,:]
        expected[:,1:]|=src[:,:-1]; expected[:,:-1]|=src[:,1:]
        result={'file':f'{name}-{pose}.png','size':list(img.size),'alpha_values':np.unique(a).tolist(),'opaque_colors':len(colors),'palette_valid':colors<=allowed,'bbox':bbox,'edge_touch':bool(a[0,:].any() or a[-1,:].any() or a[:,0].any() or a[:,-1].any()),'foot_y':int(y.max()),'foot_span_center':float((int(foot.min())+int(foot.max()))/2)}
        result['pose_mask_matches_source_plus_outline']=bool(np.array_equal(a>0,expected))
        assert img.size==(96,96) and result['alpha_values']==[0,255] and result['palette_valid'] and not result['edge_touch'], result
        assert result['pose_mask_matches_source_plus_outline'] and result['foot_y']==83,result
        checks.append(result)
        img.resize((576,576),Image.Resampling.NEAREST).save(ROOT/f'{name}-{pose}-6x.png')
        y0=40+j*316
        # Contact backgrounds are deliberately neutral, with baseline guides outside art.
        d.rectangle((i*304+8,y0,i*304+295,y0+287),fill='#d9ddd4')
        sheet.paste(img.resize((288,288),Image.Resampling.NEAREST),(i*304+8,y0),img.resize((288,288),Image.Resampling.NEAREST))
        d.text((i*304+12,y0+290),pose+' / 3x nearest',font=small,fill='#39535b')
        one.paste(img,(i*104+4,j*112),img)
sheet.save(ROOT/'comparison-six.png'); one.save(ROOT/'comparison-1x.png')
(ROOT/'pixel-qc.json').write_text(json.dumps(checks,indent=2))
print(json.dumps(checks,indent=2))
