"""Offline authoring aid, not a certificate. Unity BoardManager tests are authoritative."""
import json,copy
from pathlib import Path
levels=json.loads(Path(__file__).with_name('chapter2-design.json').read_text())
offset={0:(0,0),1:(0,1),2:(0,-1),3:(-1,0),4:(1,0)}
clockwise={1:4,4:2,2:3,3:1}
def step(level,pieces,pos):
 p=copy.deepcopy(pieces); actions=[]; operations=0
 def occupant(pos):return next((a for a in p if a['type'] in (0,1) and not a.get('gone') and (a['x'],a['y'])==pos),None)
 def move(a,d,depth=0):
  nonlocal operations
  operations+=1
  if operations>128:raise ValueError('loop')
  dx,dy=offset[d]; target=(a['x']+dx,a['y']+dy)
  if d==0 or not all(0<=v<level['size'] for v in target):return False
  tile=next((q for q in p if q['type'] not in (0,1) and (q['x'],q['y'])==target),None)
  if tile and (tile['type']==2 or tile['type']==7 and not tile['initialOpen'] or tile['type']==5 and (a['type']!=0 or a['color']!=tile['color'])):return False
  other=occupant(target)
  if other and not move(other,d,depth+1):return False
  if occupant(target) or tile and tile['type']==7 and not tile['initialOpen']:raise ValueError('rollback')
  if a['direction']!=d:actions.append('turn')
  a.update(x=target[0],y=target[1],direction=d); actions.append('move')
  if tile:
   if tile['type']==5:a['gone']=True;actions.append('deliver')
   elif tile['type']==6 and a['type']==0:
    actions.append('switch')
    for gate in p:
     if gate['type']==7 and gate['channel']==tile['channel']:gate['initialOpen']=not gate['initialOpen'];actions.append('gate')
   elif tile['type'] in (3,4):
    a['direction']=tile['direction'] if tile['type']==3 else clockwise[d];actions.append('turn');move(a,a['direction'],depth+1)
  return True
 a=occupant(tuple(pos))
 if a is None or a['type']!=0:return pieces,[]
 try:move(a,a['direction'])
 except ValueError:return pieces,[]
 return p,actions
def won(p):return not any(a['type']==0 and a['color']==1 and not a.get('gone') for a in p)
if __name__=='__main__':
 for l in levels[9:]:
  p=l['pieces']; depths=[]
  for tap in l['taps']:
   p,a=step(l,p,tap);depths.append(len(a))
  print(l['n'],'WON',won(p),'depths',depths)
  if 'alternative' in l:
   p=l['pieces']
   for tap in l['alternative']:p,a=step(l,p,tap)
   print('alternative',won(p))
