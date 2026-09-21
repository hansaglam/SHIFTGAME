"""Five-level authoring only. Never execute the older whole-chapter authoring script.
Candidate metrics are exploratory until the full Unity BoardManager tests reproduce them.
"""
from pathlib import Path
import json,hashlib

ROOT = Path(__file__).resolve().parent
def P(t,x,y,d=0,c=0,ch=0,op=False):return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4):return P(0,x,y,d,1)
def B(x,y,d=4):return P(0,x,y,d,2)
def Y(x,y,d=4):return P(0,x,y,d,3)
def X(x,y):return P(1,x,y)
def D(x,y,d):return P(3,x,y,d)
def S(x,y,ch=1):return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False):return P(7,x,y,ch=ch,op=op)
def E(x,y):return P(5,x,y,c=1)
def rooms(size,pieces,floor):
    occupied={(p['x'],p['y']) for p in pieces}
    return pieces+[P(2,x,y) for y in range(size) for x in range(size) if (x,y) not in occupied|set(floor)]
levels=[]
def L(n,name,size,budget,pieces,floor,title,hint):
    levels.append(dict(n=n,name=name,size=size,budget=budget,pieces=rooms(size,pieces,floor),title=title,hint=hint))
L(21,'Level21_FirstSwitch',4,3,
  [R(1,1),G(2,1),D(3,1,1),E(3,2),B(1,0),S(2,0),Y(3,0,1)],[],
  '21 - Switch Discovery','A circle entering a diamond pad toggles its matching gate.')
L(22,'Level22_OpenForRed',5,4,
  [R(0,2),B(1,2,2),S(1,1),D(2,2,4),G(3,2),E(4,2)],[(1,0)],
  '22 - Prepare Before Advancing','A push changes a circle\'s direction. Make space before advancing.')
L(23,'Level23_ToggleTwice',5,5,
  [R(0,2),G(1,2),D(2,2,4),G(3,2,op=True),E(4,2),B(4,1,3),S(3,1),S(2,1),Y(0,1,1)],[(0,3)],
  '23 - Toggle Trap','Every press changes the state.')
L(24,'Level24_ClosedLane',5,5,
  [R(0,2),R(1,2),B(2,2,2),S(2,1),G(3,2),D(4,2,1),E(4,3),Y(1,1)],[(2,0),(3,1)],
  '24 - Shared Passage','Only one order keeps the passage clear.')
L(25,'Level25_BeforeYouGo',5,6,
  [R(0,1),X(1,1),S(1,1),D(2,1,1),G(2,2),S(2,3,2),D(2,4,4),G(3,4,2),D(4,4,2),E(4,3),B(3,1,2),Y(1,0,1)],[(3,0),(1,2)],
  '25 - Prepare the Release','Prepare before you commit.')
# Rotate the shared passage into a vertical throat; no additional cells or rules.
for p in levels[3]['pieces']:
    p['x'],p['y']=p['y'],4-p['x']
    p['direction']={0:0,1:4,4:2,2:3,3:1}[p['direction']]
# Remove only unused outer wall rows/columns; retain every traversable cell and relation.
for l,(width,height,shift_x) in zip(levels,[(3,3,1),(5,3,0),(5,4,0),(4,5,0),(5,5,0)]):
    l.update(width=width,height=height)
    for p in l['pieces']: p['x']-=shift_x
    l['pieces']=[p for p in l['pieces'] if 0<=p['x']<width and 0<=p['y']<height]
ROOT.joinpath('batch21-candidates.json').write_text(json.dumps(levels,indent=2))

if __name__=='__main__':
    import sys,re
    if '--apply' in sys.argv:
        proofs={p['number']:p for p in json.loads(ROOT.joinpath('batch21-analysis.json').read_text(encoding='utf-8-sig'))}
        for l in levels:
            result=proofs[l['n']]['analysis']
            assert result['minimum']==l['budget'],(l['n'],result)
            values=[l['width'],l['height'],l['budget'],1]
            for p in l['pieces']:
                values += [p[k] for k in ('type','color','direction','x','y','channel')]+[int(p['initialOpen'])]
            fingerprint=hashlib.sha256((''.join(str(v)+',' for v in values)).encode()).hexdigest()
            assert fingerprint==proofs[l['n']]['fingerprint'],'Re-run BoardManager analysis after changing a candidate.'
            path=ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset"
            text=path.read_text();head=text[:text.index('  width:')]
            lines=[f"  width: {l['width']}",f"  height: {l['height']}",f"  moveLimit: {l['budget']}",'  targetColor: 1','  placements:']
            for p in l['pieces']:
                lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
            lines += ['  displayTitle: '+l['title'],'  hint: '+json.dumps(l['hint']),'  knownSolution:']
            route=result['shortest']
            if l['n']==25: route=[route[1],route[0]]+route[2:] # Equally optimal: show setup before gate activation.
            lines += [f"  - {{x: {p['x']}, y: {p['y']}}}" for p in route]
            path.write_text(head+'\n'.join(lines)+'\n')
        # Only these five design entries change; all other entry bytes remain untouched.
        catalog=ROOT.parent/'Assets/_Game/Resources/LevelDesignCatalog.asset'
        text=catalog.read_bytes().decode('utf-8');entries=text.split('  - level: ')
        changes=[
            (3,6,1,4,'Blue opens the route; Yellow would occupy the target-only exit approach.'),
            (2,11,2,0,'Let the crossing occupant leave under its own direction before pushing Red.'),
            (3,6,2,0,'Cross the first gate before the second pad reverses both gate states.'),
            (2,30,2,4,'Blue yields the vertical throat; the rear Red shares pushes with the front Red.'),
            (3,31,3,4,'Clear box parking, enter the first pad, redirect through both gate states, release the corner chain.')]
        for index,(primary,reasoning,difficulty,payoff,rationale) in enumerate(changes,21):
            block=entries[index]
            for key,value in dict(primary=primary,secondary='0000000003000000' if index==24 else ('04000000' if index==25 else ''),
                reasoningStyle=reasoning,difficultyBand=difficulty,sculptedTopology=1,hasPayoffMove=int(payoff>=4),expectedPayoffDepth=payoff,
                designRationale=json.dumps(rationale)).items():
                block=re.sub(r'(?m)^(    '+key+r':)[^\r\n]*',lambda m:m[1]+' '+str(value),block)
            entries[index]=block
        catalog.write_bytes('  - level: '.join(entries).encode('utf-8'))
