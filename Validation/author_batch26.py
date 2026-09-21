"""Scoped candidates for Levels 26–30. Rules are evaluated by the C# BoardManager harness."""
from pathlib import Path
import json,re,hashlib
ROOT=Path(__file__).resolve().parent
def P(t,x,y,d=0,c=0,ch=0,op=False): return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4): return P(0,x,y,d,1)
def B(x,y,d=4): return P(0,x,y,d,2)
def Y(x,y,d=4): return P(0,x,y,d,3)
def X(x,y): return P(1,x,y)
def D(x,y,d): return P(3,x,y,d)
def T(x,y): return P(4,x,y)
def S(x,y,ch=1): return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False): return P(7,x,y,ch=ch,op=op)
def E(x,y): return P(5,x,y,c=1)
levels=[]
def L(n,name,w,h,budget,pieces,floor,title,hint):
    occupied={(p['x'],p['y']) for p in pieces}|set(floor)
    pieces += [P(2,x,y) for y in range(h) for x in range(w) if (x,y) not in occupied]
    levels.append(dict(n=n,name=name,width=w,height=h,budget=budget,pieces=pieces,title=title,hint=hint))
L(26,'Level26_ThreeEntries',5,4,4,
  [R(0,2),B(1,2,2),D(3,2,1),D(3,3,4),E(4,3),Y(0,1,1)],[(1,1),(1,0),(2,2),(0,3)],
  '26 - Let It Wait','A clear lane is worth waiting for.')
L(27,'Level27_BoxDoesNotPress',5,4,5,
  [R(2,3,2),X(2,2),B(1,2),D(2,1,4),E(4,1),Y(3,3,2)],[(3,2),(4,2),(3,1)],
  '27 - Borrowed Space','Moving aside is only half the plan.')
L(28,'Level28_TurnThrough',5,5,5,
  [R(1,3),B(1,0,1),D(1,4,4),T(2,4),D(2,1,4),T(3,1),D(3,0,4),E(4,0),Y(2,0,1)],[(1,1),(1,2),(2,3),(2,2)],
  '28 - Beyond the Turn','Look past the first turn.')
L(29,'Level29_ClockworkGate',6,4,8,
  [R(0,1),R(1,1),R(2,1,1),D(2,3,4),T(3,3),D(3,2,4),T(4,2),D(4,0,4),D(5,0,1),D(5,1,1),E(5,2)],[(2,2),(3,1),(4,1)],
  '29 - One Move, More Work','One route does more work.')
L(30,'Level30_ClearThenOpen',6,5,7,
  [R(0,2),R(1,2),B(2,2,2),X(2,1),S(2,1),Y(1,1),G(3,2,op=True),D(4,2,1),D(4,3,4),D(5,3,1),E(5,4),Y(0,3,2)],[(3,1),(4,1),(0,1)],
  '30 - Release the Relay','Prepare the board, then release it.')
ROOT.joinpath('batch26-candidates.json').write_text(json.dumps(levels,indent=2))

if __name__=='__main__':
    import sys
    if '--baseline' in sys.argv:
        old=[]
        for l in levels:
            text=(ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset").read_text()
            pieces=[]
            for m in re.finditer(r'  - type: (\d+)\s+color: (\d+)\s+direction: (\d+)\s+position: \{x: (\d+), y: (\d+)\}\s+channel: (\d+)\s+initialOpen: (\d+)',text):
                t,c,d,x,y,ch,op=map(int,m.groups());pieces.append(P(t,x,y,d,c,ch,bool(op)))
            old.append(dict(n=l['n'],name=l['name'],width=int(re.search(r'  width: (\d+)',text)[1]),height=int(re.search(r'  height: (\d+)',text)[1]),budget=int(re.search(r'  moveLimit: (\d+)',text)[1]),pieces=pieces))
        ROOT.joinpath('batch26-old.json').write_text(json.dumps(old,indent=2))
    if '--apply' in sys.argv:
        proofs={p['number']:p for p in json.loads(ROOT.joinpath('batch26-analysis.json').read_text(encoding='utf-8-sig'))}
        for l in levels:
            proof=proofs[l['n']];result=proof['analysis']
            assert result['minimum']=={26:4,27:5,28:5,29:5,30:7}[l['n']]
            stateful=any(p['type'] in (6,7) for p in l['pieces'])
            values=[l['width'],l['height'],l['budget'],1]
            for p in l['pieces']:
                values += [p[k] for k in ('type','color','direction','x','y')]
                if stateful: values += [p['channel'],int(p['initialOpen'])]
            assert hashlib.sha256(''.join(str(v)+',' for v in values).encode()).hexdigest()==proof['fingerprint']
            path=ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset"
            text=path.read_text();head=text[:text.index('  width:')]
            lines=[f"  width: {l['width']}",f"  height: {l['height']}",f"  moveLimit: {l['budget']}",'  targetColor: 1','  placements:']
            for p in l['pieces']:
                lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
            lines += ['  displayTitle: '+l['title'],'  hint: '+json.dumps(l['hint']),'  knownSolution:']
            lines += [f"  - {{x: {p['x']}, y: {p['y']}}}" for p in result['shortest']]
            path.write_text(head+'\n'.join(lines)+'\n')
        catalog=ROOT.parent/'Assets/_Game/Resources/LevelDesignCatalog.asset'
        entries=catalog.read_bytes().decode().split('  - level: ')
        metadata=[
            (0,11,1,6,'Clear the waiting lane without redirecting its occupant; preserve the exit approach.'),
            (2,11,2,0,'Park the box sideways, then vacate the borrowed crossing before the target descends.'),
            (4,19,3,8,'Enter the upper turn from below; predict clockwise turns and keep the wrong color out of the final bend.'),
            (5,58,3,4,'A separate loop completes legally; shared pushes redirect and deliver the convoy more efficiently.'),
            (1,63,4,8,'Park the box, clear the switch, restore the gate with the corridor occupant, then release the paired targets.')]
        for index,(primary,reason,difficulty,payoff,rationale) in enumerate(metadata,26):
            block=entries[index]
            for key,value in dict(primary=primary,secondary='',reasoningStyle=reason,difficultyBand=difficulty,sculptedTopology=1,
                                  hasPayoffMove=int(payoff>=4),expectedPayoffDepth=payoff,designRationale=json.dumps(rationale)).items():
                block=re.sub(r'(?m)^(    '+key+r':)[^\r\n]*',lambda m:m[1]+' '+str(value),block)
            entries[index]=block
        catalog.write_bytes('  - level: '.join(entries).encode())
        certificates=ROOT.parent/'Assets/_Game/Scripts/Levels/VerifiedOptimality.cs'
        text=certificates.read_text()
        for proof in proofs.values():
            if proof['fingerprint'] not in text:
                line=f'                "{proof["fingerprint"]}" => {proof["analysis"]["minimum"]}, // Level {proof["number"]}; Batch26Tests exhaustive graph\n'
                text=text.replace('                _ => null',line+'                _ => null')
        certificates.write_text(text)
