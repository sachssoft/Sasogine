from pathlib import Path
import re, json, math, copy, xml.etree.ElementTree as ET
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parent
BASE=ROOT/'SachssoftDark'
PALETTES={
'Light': ['F3F6FB','FFFFFF','EDF2FA','FFFFFF','C7D2E0','E4ECF8','D6E4F7','2563EB','DBEAFE','DBEAFE','17243A','52677F','8997A5','C7354B','9A6700','17835C'],
'Modern':['171A21','22252E','2D313D','1B1E26','424856','3C4351','252A36','9C8CFF','4D426D','3B3454','F1F2F7','ABB1C2','73798A','FF7C93','E7BD75','71CDB7'],
'Futuristic':['050D17','0B1827','10263B','06111E','24516F','123E56','082337','42E9FF','0F6073','124559','D7F7FF','79A8B9','4E6A7F','FF6487','FFD068','61FACB'],
'Medieval':['CDB68B','E4D0A6','C6AB78','F2E4C5','735332','D7BD88','BA975F','805719','C9A868','D5B878','32291E','705C42','9A896D','A12F2D','8A620C','3E6B43']}
KEYS=['canvas','surface','raised','input','border','hover','pressed','accent','accentDark','selected','text','muted','disabled','error','warning','success']

from SkinPrimitives import build_assets

registry=(ROOT.parent/'UIRegistry.cs').read_text(encoding='utf-8-sig')
defaultStyles=re.findall(r'styles\.Register\(\s*"([^"]+)"\s*,\s*new Style\s*\{(.*?)\}\s*\);',registry,re.S)
def setval(style,key,value):
    node=next((n for n in style.findall('Value') if n.get('property')==key),None)
    if node is None:node=ET.SubElement(style,'Value',property=key)
    node.set('value',value)
def colorRole(prop,rgb):
    if prop=='DisabledTextColor':return 'disabled'
    if prop=='FocusColor':return 'accent'
    if prop=='BorderColor':return 'accent' if rgb==(65,199,223) else 'border'
    if prop=='TextColor':return 'accent' if rgb in [(65,199,223),(100,180,255)] else 'muted'
    if prop=='HoverBackground':return 'hover'
    if prop=='PressedBackground':return 'accentDark'
    if prop=='DisabledBackground':return 'raised'
    if prop=='Background':return {(17,24,35):'canvas',(20,28,40):'input',(36,46,62):'raised',(28,35,48):'surface',(25,35,49):'surface'}.get(rgb,'raised')
    return None

baseSkin=ET.parse(BASE/'sachssoft_dark.skin.xml').getroot()
for kind,colors in PALETTES.items():
    palette=dict(zip(KEYS,['#'+c for c in colors]));folder=ROOT/('Sachssoft'+kind);folder.mkdir(parents=True,exist_ok=True);stem='sachssoft_'+kind.lower()
    palette['closeHover'] = '#F8D7DD' if kind=='Light' else '#E7B8A0' if kind=='Medieval' else '#843D4B'
    palette['closePressed'] = '#E69EA9' if kind=='Light' else '#D49D86' if kind=='Medieval' else '#B94C62'
    bitmaps,meta=build_assets(palette,kind)
    atlas=Image.new('RGBA',(1024,1024));positions={};x=y=2;row=0
    for name,im in sorted(bitmaps.items(),key=lambda pair:(-pair[1].height,-pair[1].width,pair[0])):
        w,h=im.size
        if x+w+6>1024:x=2;y+=row;row=0
        px,py=x+2,y+2;positions[name]=(px,py,w,h);atlas.paste(im,(px,py))
        for crop,dst,dim in [((0,0,w,1),(px,py-2),(w,2)),((0,h-1,w,h),(px,py+h),(w,2)),((0,0,1,h),(px-2,py),(2,h)),((w-1,0,w,h),(px+w,py),(2,h))]:
            atlas.paste(im.crop(crop).resize(dim),dst)
        for dx,dy,cx,cy in [(-2,-2,0,0),(w,-2,w-1,0),(-2,h,0,h-1),(w,h,w-1,h-1)]:atlas.paste(Image.new('RGBA',(2,2),im.getpixel((cx,cy))),(px+dx,py+dy))
        x+=w+6;row=max(row,h+6)
    height=1<<(y+row+1).bit_length();atlas=atlas.crop((0,0,1024,height))
    premul=Image.new('RGBA',atlas.size);premul.putdata([((r*a+127)//255,(g*a+127)//255,(b*a+127)//255,a) for r,g,b,a in (atlas.get_flattened_data() if hasattr(atlas,"get_flattened_data") else atlas.getdata())]);premul.save(folder/(stem+'.png'),optimize=True)
    document=ET.Element('TextureAtlas',imagePath=stem+'.png',width='1024',height=str(height),scale='1',format='RGBA8888',alphaMode='Premultiplied')
    for name,(px,py,w,h) in sorted(positions.items()):ET.SubElement(document,'sprite',n=name,x=str(px),y=str(py),w=str(w),h=str(h),pX='.5',pY='.5',oX='0',oY='0',oW=str(w),oH=str(h),r='n')
    ET.indent(document);ET.ElementTree(document).write(folder/(stem+'.atlas.xml'),encoding='utf-8',xml_declaration=True)
    skin=copy.deepcopy(baseSkin);skin.set('name','Sachssoft'+kind);skin.set('atlas',stem+'.atlas.xml')
    for c in skin.find('Palette'):c.set('value',palette[c.get('name')].upper())
    styles=skin.find('Styles');byId={s.get('key'):s for s in styles}
    for name,body in defaultStyles:
        if name not in byId:
            parent=re.search(r'BasedOn\s*=\s*"([^"]+)"',body)
            node=ET.SubElement(styles,'Style',key=name,basedOn=parent.group(1) if parent else 'Control');byId[name]=node
        for prop,nums in re.findall(r'(\w+)\s*=\s*new Color\(([^)]+)\)',body):
            values=tuple(int(n.strip()) for n in nums.split(','));role=colorRole(prop,values[:3])
            if role:setval(byId[name],prop,role)
    for key,role in [('TextColor','text'),('DisabledTextColor','disabled'),('BorderColor','border'),('FocusColor','accent'),('HoverBackground','hover'),('PressedBackground','accentDark'),('DisabledBackground','raised')]:setval(byId['Control'],key,role)
    for name in ['Grid','TabItem']:setval(byId[name],'Background','canvas')
    for name in ['GroupBox','Calendar','ToolTip']:setval(byId[name],'Background','surface')
    # Make current extended controls consistent with each theme, including hardcoded colors in the old dark XML.
    for name in ['Chart','ValueBar','TextBox','NumericSpinner','ListBox']:setval(byId[name],'Background','input')
    for name in ['GroupBox','Chart','ValueBar']:setval(byId[name],'BorderColor','border')
    for name in ['Control','TextBox','ValueBar']:setval(byId[name],'TextColor','text')
    codeColors={'Light':['2455B8','995028','497740','7958B5'],'Medieval':['79439B','A04427','67733C','265B87']}.get(kind,['6BB1FF','D7A470','73A268','B5CEA8'])
    for key,color in zip(['KeywordColor','StringColor','CommentColor','NumberColor'],codeColors):setval(byId['ScriptTextBox'],key,'#'+color)
    ET.indent(skin);ET.ElementTree(skin).write(folder/(stem+'.skin.xml'),encoding='utf-8',xml_declaration=True)
    report={'skin':'Sachssoft'+kind,'atlasSize':[1024,height],'frames':len(positions),'regions':len(skin.find('Regions')),'styles':len(styles),'alphaMode':'Premultiplied','rotation':False,'trim':False}
    (folder/'Pruefung.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(report)
