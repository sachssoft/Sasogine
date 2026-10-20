from PIL import Image, ImageDraw, ImageFilter
import colorsys, math
def build_assets(P,kind):
    S=4
    assets={};meta={}
    def rgba(value):
        if value is None: return (0,0,0,0)
        if isinstance(value, tuple): return value if len(value)==4 else (*value,255)
        value = P.get(value,value).lstrip('#')
        return tuple(int(value[i:i+2],16) for i in (0,2,4)) + (255,)
    
    def aa(w,h,fn):
        im=Image.new('RGBA',(w*S,h*S)); fn(ImageDraw.Draw(im), im)
        return im.resize((w,h),Image.Resampling.LANCZOS)
    
    def box(d,rect,fill,outline=None,r=5,width=1):
        d.rounded_rectangle(tuple(round(v*S) for v in rect),radius=round(r*S),fill=rgba(fill),
            outline=rgba(outline) if outline else None,width=round(width*S))
    
    def line(d,pts,color='text',width=1.6):
        d.line([(round(x*S),round(y*S)) for x,y in pts],fill=rgba(color),width=max(1,round(width*S)),joint='curve')
    
    def ellipse(d,rect,fill=None,outline=None,width=1.5):
        d.ellipse(tuple(round(v*S) for v in rect),fill=rgba(fill) if fill else None,
            outline=rgba(outline) if outline else None,width=round(width*S))
    
    def add(name,im,slice=(0,0,0,0),mode='Fixed'):
        assert name not in assets
        assets[name]=im; meta[name]={'slice':slice,'mode':mode}
        return name
    
    def patch(name,fill,border='border',w=32,h=32,r=5,highlight=False):
        def paint(d,im):
            box(d,(0.5,0.5,w-0.5,h-0.5),fill,border,r)
            if highlight: line(d,[(6,1.8),(w-6,1.8)],(255,255,255,16),1)
        inset=8 if r else 2
        return add(name,aa(w,h,paint),(min(inset,w//3),min(inset,h//3),min(inset,w//3),min(inset,h//3)),'NineSlice')
    
    def glyph(name,kind,color='text',size=20):
        def draw(d,im):
            # Geometric glyphs are original vector constructions, not an external icon font.
            if kind in ('left','right','up','down'):
                pts={'left':[(12,5),(7,10),(12,15)],'right':[(8,5),(13,10),(8,15)],
                     'up':[(5,12),(10,7),(15,12)],'down':[(5,8),(10,13),(15,8)]}[kind]
                line(d,pts,color,1.8)
            elif kind=='check': line(d,[(4,10),(8,14),(16,6)],color,2)
            elif kind=='minus': line(d,[(5,10),(15,10)],color,1.8)
            elif kind=='close': line(d,[(6,6),(14,14)],color,1.6);line(d,[(14,6),(6,14)],color,1.6)
            elif kind=='plus': line(d,[(5,10),(15,10)],color);line(d,[(10,5),(10,15)],color)
            elif kind=='maximize': box(d,(5,5,15,15),None,color,1,1.4)
            elif kind=='restore': box(d,(7,4,16,13),None,color,1,1.4);box(d,(4,7,13,16),'surface',color,1,1.4)
            elif kind=='resize':
                for i in (0,4,8): line(d,[(9+i,17),(17,9+i)],color,1)
            elif kind=='folder':
                box(d,(2,5,18,16),'accentDark',None,2)
                box(d,(3,3,10,8),'accent',None,1)
                box(d,(2,7,18,17),'accent','accent',2,1)
            elif kind=='folder_open':
                box(d,(2,5,17,16),'accentDark',None,2);box(d,(3,3,10,8),'accentDark',None,1)
                d.polygon([(2*S,8*S),(19*S,8*S),(16*S,17*S),(1*S,17*S)],fill=rgba('accent'))
            elif kind=='file':
                d.polygon([(5*S,2*S),(12*S,2*S),(16*S,6*S),(16*S,18*S),(5*S,18*S)],fill=rgba('raised'),outline=rgba(color),width=S)
                line(d,[(12,2),(12,6),(16,6)],color,1)
                line(d,[(8,10),(13,10)],'muted',1);line(d,[(8,13),(13,13)],'muted',1)
            elif kind=='search': ellipse(d,(3.5,3.5,12.5,12.5),None,color);line(d,[(12,12),(17,17)],color,1.7)
            elif kind=='refresh':
                d.arc((4*S,4*S,16*S,16*S),40,325,fill=rgba(color),width=round(1.6*S))
                line(d,[(13,3),(17,5),(16,9)],color,1.6)
            elif kind=='new_folder':
                box(d,(2,6,18,17),'accentDark','accent',2);box(d,(3,3,9,7),'accent',None,1)
                line(d,[(7,11),(13,11)],'text');line(d,[(10,8),(10,14)],'text')
            elif kind=='grid':
                for x in (4,11):
                    for y in (4,11):box(d,(x,y,x+4,y+4),None,color,0.5,1.3)
            elif kind=='list':
                for y in (5,10,15):ellipse(d,(3,y-0.8,4.5,y+0.8),color);line(d,[(7,y),(17,y)],color,1.3)
            elif kind=='drive':
                box(d,(2,6,18,15),None,color,2,1.3);line(d,[(3,11),(17,11)],color,1)
                ellipse(d,(14,12,15.5,13.5),'success')
            elif kind=='image':
                box(d,(3,3,17,17),None,color,2,1.2);ellipse(d,(6,6,8,8),'accent')
                line(d,[(4,14),(8,10),(11,13),(14,9),(17,14)],'accent',1.4)
            elif kind=='more':
                for x in (5,10,15): ellipse(d,(x-1,9,x+1,11),color)
            elif kind=='eyedropper':
                line(d,[(5,15),(14,6)],color,3);line(d,[(12,4),(16,8)],'accent',3);line(d,[(3,17),(6,14)],color,1.2)
            elif kind=='sort_ascending':line(d,[(6,8),(10,4),(14,8)],color);line(d,[(10,4),(10,16)],color)
            elif kind=='sort_descending':line(d,[(6,12),(10,16),(14,12)],color);line(d,[(10,4),(10,16)],color)
            else:raise ValueError(kind)
        return add(name,aa(20,20,draw).resize((size,size),Image.Resampling.LANCZOS))
    
    
    def box(d,rect,fill,outline=None,r=5,width=1):
        x0,y0,x1,y1=rect
        if fill is None and outline is None:
            return
        if kind=='Futuristic':
            cut=min(max(1,r),max(0,(x1-x0)/3),max(0,(y1-y0)/3))
            points=[(x0+cut,y0),(x1,y0),(x1,y1-cut),(x1-cut,y1),(x0,y1),(x0,y0+cut)]
            points=[(round(x*S),round(y*S)) for x,y in points]
            d.polygon(points,fill=rgba(fill) if fill is not None else None)
            if outline:d.line(points+[points[0]],fill=rgba(outline),width=max(1,round(width*S)),joint='curve')
        else:
            radius=r*({'Light':.8,'Modern':1.45,'Medieval':.6}.get(kind,1))
            radius=min(radius,(x1-x0)/2,(y1-y0)/2)
            d.rounded_rectangle(tuple(round(v*S) for v in rect),radius=round(radius*S),fill=rgba(fill) if fill is not None else None,outline=rgba(outline) if outline else None,width=max(1,round(width*S)))
    
    def patch(name,fill,border='border',w=32,h=32,r=5,highlight=False):
        def paint(d,im):
            if fill is not None:
                mask=Image.new('RGBA',im.size);box(ImageDraw.Draw(mask),(.5,.5,w-.5,h-.5),'#ffffff',None,r)
                texture=Image.new('RGBA',im.size);td=ImageDraw.Draw(texture);base=rgba(fill)
                for sy in range(im.height):
                    y=sy/S
                    if kind=='Medieval':
                        wood=fill in ('raised','pressed','accentDark')
                        shift=(math.sin(y*2.1)*4+math.sin(y*5.7)*2) if wood else (math.sin(y*.9)*1.6)
                    else:shift=(1-y/max(1,h))*({'Light':3,'Modern':1,'Futuristic':5}.get(kind,0))
                    color=tuple(max(0,min(255,round(v+shift))) for v in base[:3])+(255,)
                    td.line((0,sy,im.width,sy),fill=color)
                texture.putalpha(mask.getchannel('A'));im.alpha_composite(texture)
            box(d,(.5,.5,w-.5,h-.5),None,border,r)
            if kind=='Medieval' and w>=24 and h>=24 and border:
                box(d,(2,2,w-2,h-2),None,'accentDark',max(1,r-1),.6)
                if name.startswith(('button','window','dialog','panel','popup','titlebar')):
                    for x,y in [(4,4),(w-4,4),(4,h-4),(w-4,h-4)]:
                        ellipse(d,(x-.8,y-.8,x+.8,y+.8),'accent','border',.4)
            if kind=='Futuristic' and border and w>=20 and h>=20:
                line(d,[(4,2),(min(w-4,11),2)],'accent',.6)
                line(d,[(w-2,h-5),(w-2,h-3),(w-5,h-3)],'accentDark',.7)
            if highlight and kind=='Light':line(d,[(5,1.8),(w-5,1.8)],(255,255,255,90),.6)
        inset=8 if r else 2
        return add(name,aa(w,h,paint),(min(inset,w//3),min(inset,h//3),min(inset,w//3),min(inset,h//3)),'NineSlice')
    # Reusable surfaces, overlays and focus rings.
    for name,fill,border in [('panel','surface','border'),('panel_inset','input','border'),('panel_raised','raised','border'),
        ('popup','surface','border'),('window','surface','border'),('dialog','surface','border'),('tooltip','raised','border')]:
        patch(name,fill,border)
    patch('window_inactive','surface','raised')
    patch('titlebar','raised',None,r=5)
    patch('titlebar_inactive','surface',None,r=5)
    patch('menubar','surface','border',r=0)
    patch('focus_ring',None,'accent',r=6)
    patch('invalid_ring',None,'error',r=6)
    add('transparent',Image.new('RGBA',(1,1)),mode='Stretch')
    add('white',Image.new('RGBA',(1,1),'white'),mode='Stretch')
    add('modal_overlay',Image.new('RGBA',(1,1),(0,0,0,160)),mode='Stretch')
    shadow=Image.new('RGBA',(48,48)); mask=Image.new('L',(48,48));ImageDraw.Draw(mask).rounded_rectangle((8,6,39,39),radius=6,fill=150)
    mask=mask.filter(ImageFilter.GaussianBlur(5));shadow.putalpha(mask)
    add('shadow',shadow,(16,16,16,16),'NineSlice')
    for name,fill,border in [('normal','raised','border'),('hover','hover','accentDark'),('pressed','pressed','accent'),
        ('disabled','surface','raised'),('checked','selected','accent'),('checked_hover','accentDark','accent'),
        ('checked_pressed','pressed','accent'),('checked_disabled','raised','border')]:
        patch('button_'+name,fill,border,highlight=name=='normal')
    for name,fill,border in [('normal','input','border'),('hover','input','muted'),('focused','input','accent'),
        ('disabled','canvas','raised'),('readonly','surface','border'),('invalid','input','error')]:patch('textbox_'+name,fill,border)
    add('text_selection',Image.new('RGBA',(2,2),rgba('accentDark')),mode='Stretch')
    add('text_caret',Image.new('RGBA',(1,12),rgba('text')),mode='Stretch')
    
    # Fixed-size checkbox / radio states, including indeterminate.
    for kind in ('checkbox','radio'):
        for selected in ('unchecked','checked') + (('mixed',) if kind=='checkbox' else ()):
            for state in ('normal','hover','pressed','disabled'):
                def paint(d,im,kind=kind,selected=selected,state=state):
                    active=selected!='unchecked'
                    bg=('disabled' if active else 'canvas') if state=='disabled' else ('accentDark' if active else 'input')
                    border='raised' if state=='disabled' else ('accent' if active or state in ('hover','pressed') else 'border')
                    if state=='pressed' and active:bg='pressed'
                    if kind=='checkbox':
                        box(d,(2,2,22,22),bg,border,4,1.2)
                        if selected=='checked':line(d,[(6.5,12),(10.5,16),(17.5,8)],'muted' if state=='disabled' else 'text',2.2)
                        if selected=='mixed':line(d,[(7,12),(17,12)],'muted' if state=='disabled' else 'text',2.2)
                    else:
                        ellipse(d,(2,2,22,22),bg,border,1.2)
                        if active:ellipse(d,(8,8,16,16),'muted' if state=='disabled' else 'accent')
                add(f'{kind}_{selected}_{state}',aa(24,24,paint))
    
    # Independent scrollbar pieces and slider pieces.
    for axis,w,h in [('horizontal',32,12),('vertical',12,32)]:
        patch('scrollbar_'+axis+'_track','canvas',None,w,h,r=3)
        for state,fill,border in [('normal','border',None),('hover','muted',None),('pressed','accent',None),('disabled','raised',None)]:
            patch(f'scrollbar_{axis}_thumb_{state}',fill,border,w,h,r=3)
    for state,fill,border in [('normal','surface','border'),('hover','hover','accentDark'),('pressed','pressed','accent'),('disabled','canvas','raised')]:
        patch('scrollbar_arrow_'+state,fill,border,20,20,r=3)
    patch('scrollviewer_corner','canvas',None,16,16,r=0)
    for axis,w,h in [('horizontal',32,8),('vertical',8,32)]:
        patch('slider_'+axis+'_track','canvas','border',w,h,r=3)
        patch('slider_'+axis+'_fill','accent',None,w,h,r=3)
        patch('slider_'+axis+'_fill_disabled','disabled',None,w,h,r=3)
    for state,fill,border in [('normal','text','muted'),('hover','text','accent'),('pressed','accent','text'),('disabled','disabled','border')]:
        add('slider_thumb_'+state,aa(24,24,lambda d,im,f=fill,b=border:ellipse(d,(3,3,21,21),f,b,1.4)))
    add('slider_thumb_focus',aa(28,28,lambda d,im:ellipse(d,(1,1,27,27),None,'accent',1)))
    add('slider_tick_horizontal',Image.new('RGBA',(1,5),rgba('border')))
    add('slider_tick_vertical',Image.new('RGBA',(5,1),rgba('border')))
    
    for name,fill,border in [('normal',None,None),('hover','raised',None),('pressed','pressed',None),
        ('selected','selected',None),('selected_hover','accentDark',None),('selected_inactive','raised',None),('disabled',None,None)]:
        patch('item_'+name,fill,border,r=3)
    for name,fill,border in [('normal','surface','border'),('hover','raised','muted'),('selected','selected','accent'),('disabled','canvas','raised')]:
        patch('tile_'+name,fill,border,r=5)
    add('separator_horizontal',Image.new('RGBA',(16,1),rgba('border')),mode='Stretch')
    add('separator_vertical',Image.new('RGBA',(1,16),rgba('border')),mode='Stretch')
    for state,fill,border in [('normal',None,None),('hover','closeHover',None),('pressed','closePressed',None),('disabled',None,None)]:
        patch('window_close_'+state,fill,border,24,24,r=4)
    for kind in ['left','right','up','down','check','minus','close','plus','maximize','restore','resize','folder','folder_open',
        'file','search','refresh','new_folder','grid','list','drive','image','more','eyedropper','sort_ascending','sort_descending']:
        glyph('icon_'+kind,kind)
    for kind,col in [('information','accent'),('warning','warning'),('error','error'),('question','muted')]:
        def paint(d,im,kind=kind,col=col):
            if kind=='warning':
                d.polygon([(16*S,3*S),(30*S,28*S),(2*S,28*S)],fill=rgba('surface'),outline=rgba(col),width=round(1.6*S))
                line(d,[(16,11),(16,19)],col,2);ellipse(d,(15,23,17,25),col)
            else:
                ellipse(d,(2,2,30,30),'surface',col,1.5)
                if kind=='information':ellipse(d,(15,8,17,10),col);line(d,[(16,14),(16,24)],col,2)
                elif kind=='error':line(d,[(11,11),(21,21)],col,2);line(d,[(21,11),(11,21)],col,2)
                else:
                    d.arc((11*S,8*S,22*S,18*S),185,460,fill=rgba(col),width=2*S)
                    line(d,[(16,18),(16,20)],col,2);ellipse(d,(15,24,17,26),col)
        add('message_'+kind,aa(32,32,paint))
    
    # Color picker resources compose for any hue; no fixed-hue SV bitmap.
    checker=Image.new('RGBA',(16,16));d=ImageDraw.Draw(checker)
    for y in range(0,16,8):
        for x in range(0,16,8):d.rectangle((x,y,x+7,y+7),fill=rgba('#8290a1' if (x//8+y//8)%2==0 else '#b7c2cf'))
    add('color_checker',checker,mode='Tile')
    for axis in ('horizontal','vertical'):
        im=Image.new('RGBA',(256,16) if axis=='horizontal' else (16,256))
        d=ImageDraw.Draw(im)
        for i in range(256):
            rgb=tuple(round(v*255) for v in colorsys.hsv_to_rgb(i/255,1,1))
            d.line((i,0,i,15) if axis=='horizontal' else (0,i,15,i),fill=(*rgb,255))
        add('color_hue_'+axis,im,mode='Stretch')
    im=Image.new('RGBA',(256,1));im.putdata([(255,255,255,255-i) for i in range(256)])
    add('color_saturation_overlay',im,mode='Stretch')
    im=Image.new('RGBA',(1,256));im.putdata([(0,0,0,i) for i in range(256)])
    add('color_value_overlay',im,mode='Stretch')
    im=Image.new('RGBA',(256,1));im.putdata([(255,255,255,i) for i in range(256)])
    add('color_alpha_ramp',im,mode='Stretch')
    add('color_sv_cursor',aa(16,16,lambda d,im:(ellipse(d,(2,2,14,14),None,'#000000',3),ellipse(d,(2,2,14,14),None,'#ffffff',1.4))))
    add('color_hue_cursor_horizontal',aa(6,22,lambda d,im:box(d,(1,1,5,21),None,'text',1,1.5)))
    add('color_hue_cursor_vertical',aa(22,6,lambda d,im:box(d,(1,1,21,5),None,'text',1,1.5)))
    patch('color_preview_frame',None,'border',r=4)
    patch('progress_track','input','border',w=32,h=12,r=4)
    patch('progress_fill','accent',None,w=32,h=12,r=4)
    
    return assets,meta
