"""Scoped candidate authoring for campaign 36–40. Never writes assets without --apply."""
from pathlib import Path
import json,re,hashlib
ROOT=Path(__file__).resolve().parent
def P(t,x,y,d=0,c=0,ch=0,op=False):return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4):return P(0,x,y,d,1)
def B(x,y,d=4):return P(0,x,y,d,2)
def Y(x,y,d=4):return P(0,x,y,d,3)
def C(x,y,d=4):return P(0,x,y,d,4)
def X(x,y):return P(1,x,y)
def D(x,y,d):return P(3,x,y,d)
def T(x,y):return P(4,x,y)
def S(x,y,ch=1):return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False):return P(7,x,y,ch=ch,op=op)
def E(x,y,c=1):return P(5,x,y,c=c)
levels=[]
def L(n,name,w,h,budget,pieces,floor,title,hint,targets=(1,2)):
 occupied={(p['x'],p['y']) for p in pieces}|set(floor)
 pieces += [P(2,x,y) for y in range(h) for x in range(w) if (x,y) not in occupied]
 levels.append(dict(n=n,name=name,width=w,height=h,budget=budget,pieces=pieces,title=title,hint=hint,targets=list(targets)))

L(36,'Level36_CrossBeforeClosing',5,5,6,
 [R(2,1,1),B(1,2),Y(3,1,1),X(3,2),T(2,2),D(4,2,1),E(4,3),D(2,0,3),E(1,0,2)],[(3,3),(3,4)],
 '36 - Two Ways Through','Two colors. One turning point.')
L(37,'Level37_OpenTheSecondPad',6,5,6,
 [B(0,2),R(2,2,1),Y(2,1,1),D(2,4,4),E(3,4,2),D(5,2,1),E(5,3)],[(1,2),(3,2),(4,2),(2,3)],
 '37 - Borrowed Ground','The same space, at different times.')
L(38,'Level38_LiftAndTurn',6,5,7,
 [Y(1,2),Y(1,4,2),R(4,3,3),R(2,2,2),D(3,2,1),T(2,3),E(2,4),T(3,4),D(4,4,4),E(5,4,3),D(1,3,2),D(1,1,4),D(2,1,4),E(3,1,3),S(3,3),G(1,2,op=True)],[],
 '38 - A Favor Returned','Make room for each other.',(1,3))
L(39,'Level39_PrepareToggleRedirect',5,5,8,
 [R(2,3,2),B(0,2),Y(1,3),D(3,2,1),E(3,4),D(4,3,2),E(4,2,2),T(2,0),D(0,0,1),E(0,1)],[(1,2),(2,2),(2,1),(1,0),(3,3)],
 '39 - Better Together','Can one move do two jobs?')
L(40,'Level40_TheStateOfShift',6,5,8,
 [R(1,3,2),B(3,2,2),C(0,2,2),Y(2,0),D(0,1,4),D(2,1,4),G(4,1),D(5,1,1),D(5,2,1),E(5,3,2),S(3,0),D(4,0,1),D(4,2,3),T(3,2),D(3,3,3),D(2,3,1),E(2,4)],[(1,2),(1,1),(3,1),(2,2)],
 '40 - The Color Machine','Build one system. Finish every color.')

if __name__=='__main__':
 ROOT.joinpath('batch36-candidates.json').write_text(json.dumps(levels,indent=2))
 import sys
 if '--baseline' in sys.argv:
  old=[]
  for l in levels:
   s=(ROOT/'batch36-baseline'/f"{l['name']}.asset").read_text(); pieces=[]
   for m in re.finditer(r'  - type: (\d+)\s+color: (\d+)\s+direction: (\d+)\s+position: \{x: (\d+), y: (\d+)\}\s+channel: (\d+)\s+initialOpen: (\d+)',s):
    t,c,d,x,y,ch,op=map(int,m.groups());pieces.append(P(t,x,y,d,c,ch,bool(op)))
   old.append(dict(n=l['n'],name=l['name'],width=int(re.search(r'  width: (\d+)',s)[1]),height=int(re.search(r'  height: (\d+)',s)[1]),budget=int(re.search(r'  moveLimit: (\d+)',s)[1]),pieces=pieces))
  ROOT.joinpath('batch36-old.json').write_text(json.dumps(old,indent=2))
 if '--apply' in sys.argv:
  proofs={p['number']:p for p in json.loads(ROOT.joinpath('batch36-analysis.json').read_text(encoding='utf-8-sig'))}
  for l in levels:
   proof=proofs[l['n']];a=proof['analysis'];assert a['minimum']>0 and a['minimum']<=l['budget']
   values=[l['width'],l['height'],l['budget'],1,-360,len(l['targets'])]+sorted(l['targets'])
   stateful=any(p['type'] in (6,7) for p in l['pieces'])
   for p in l['pieces']:
    values += [p[k] for k in ('type','color','direction','x','y')]
    if stateful:values += [p['channel'],int(p['initialOpen'])]
   assert hashlib.sha256(''.join(str(v)+',' for v in values).encode()).hexdigest()==proof['fingerprint']
   path=ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset";s=path.read_text();head=s[:s.index('  width:')]
   lines=[f"  width: {l['width']}",f"  height: {l['height']}",f"  moveLimit: {l['budget']}",'  targetColor: 1','  targetColors: '+''.join(int(c).to_bytes(4,'little').hex() for c in l['targets']),'  placements:']
   for p in l['pieces']:
    lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
   lines += ['  displayTitle: '+l['title'],'  hint: '+json.dumps(l['hint']),'  knownSolution:']
   lines += [f"  - {{x: {p['x']}, y: {p['y']}}}" for p in a['shortest']]
   path.write_text(head+'\n'.join(lines)+'\n')
  catalog=ROOT.parent/'Assets/_Game/Resources/LevelDesignCatalog.asset';entries=catalog.read_bytes().decode().split('  - level: ')
  metadata=[(2,11,2,4,'Clear the receiving shelf before either color uses the shared rotator; opposite approaches lead to color-specific exits.'),
   (1,11,3,4,'Blue redirects Red out of the shared bay, then must yield upward before it follows Red into an unusable route.'),
   (4,27,3,6,'Yellow lifts Red into its delivery route and temporarily closes access for its partner; the second Red reopens that access while delivering the first.'),
   (5,58,3,6,'Stage Blue underneath Red before the crosswise push; a shared lift replaces separate positioning and delivery moves.'),
   (1,31,5,14,'Assemble a shared row and open its gate for Blue; preserve the control piece to divert the waiting Red onto a separate return cascade.')]
  for n,(primary,reason,difficulty,payoff,rationale) in enumerate(metadata,36):
   for key,value in dict(primary=primary,secondary='',reasoningStyle=reason,difficultyBand=difficulty,sculptedTopology=1,hasPayoffMove=1,expectedPayoffDepth=payoff,designRationale=json.dumps(rationale)).items():
    entries[n]=re.sub(r'(?m)^(    '+key+r':)[^\r\n]*',lambda m:m[1]+' '+str(value),entries[n])
  catalog.write_bytes('  - level: '.join(entries).encode())
  cert=ROOT.parent/'Assets/_Game/Scripts/Levels/VerifiedOptimality.cs';s=cert.read_text()
  for p in proofs.values():
   if p['fingerprint'] not in s:s=s.replace('                _ => null',f'                "{p["fingerprint"]}" => {p["analysis"]["minimum"]}, // Level {p["number"]}; Batch36Tests exhaustive graph\n                _ => null')
  cert.write_text(s)

