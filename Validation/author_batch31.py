"""Author only Levels 31–35; evaluate with the unchanged C# BoardManager."""
from pathlib import Path
import json,re,hashlib
ROOT=Path(__file__).resolve().parent
def P(t,x,y,d=0,c=0,ch=0,op=False):return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4):return P(0,x,y,d,1)
def B(x,y,d=4):return P(0,x,y,d,2)
def Y(x,y,d=4):return P(0,x,y,d,3)
def X(x,y):return P(1,x,y)
def D(x,y,d):return P(3,x,y,d)
def T(x,y):return P(4,x,y)
def S(x,y,ch=1):return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False):return P(7,x,y,ch=ch,op=op)
def E(x,y):return P(5,x,y,c=1)
levels=[]
def L(n,name,w,h,budget,pieces,floor,title,hint):
    occupied={(p['x'],p['y']) for p in pieces}|set(floor)
    pieces += [P(2,x,y) for y in range(h) for x in range(w) if (x,y) not in occupied]
    levels.append(dict(n=n,name=name,width=w,height=h,budget=budget,pieces=pieces,title=title,hint=hint))
L(31,'Level31_OnePadTwoGates',6,4,6,
  [R(4,1,3),B(3,1,1),X(3,2),Y(2,2),D(1,1,1),D(1,2,1),E(1,3),P(0,1,0,1,4)],[(5,2),(4,2),(3,2),(2,1)],
  '31 - Across the Divide','What changes on the other side?')
L(32,'Level32_OppositeStates',5,4,6,
  [R(2,1,3),T(1,1),D(1,3,4),T(2,3),D(2,1,4),D(4,1,2),E(4,0),B(1,2),Y(0,2)],[(2,2),(3,2),(3,1)],
  '32 - Return Changed','Some progress must come back.')
L(33,'Level33_SharedDeliveryGate',5,4,6,
  [R(1,1,1),R(3,3,3),D(1,3,4),D(4,3,2),D(4,1,3),D(3,1,2),D(3,0,4),E(4,0),B(2,3,2),Y(2,1,1)],[(1,2),(2,2),(4,2)],
  '33 - A Moving Pair','Two paths. One rhythm.')
L(34,'Level34_ClearTheCorridor',4,5,5,
  [R(2,2),B(2,0,1),Y(1,0,1),T(3,2),D(2,4,4),T(3,4),D(1,2,2),T(1,1),D(0,1,2),E(0,0)],[(2,1),(2,3),(3,3),(3,1)],
  '34 - The Easy Way','Where does the easy way end?')
L(35,'Level35_TwoChannels',6,4,8,
  [R(1,3,2),R(3,3,2),B(0,2,2),D(0,1,4),S(3,0),D(2,1,4),G(4,1),D(5,1,1),D(5,2,1),D(5,3,3),E(4,3),Y(2,0),D(4,0,1)],[(1,2),(1,1),(3,2),(3,1),(2,2)],
  '35 - Assemble the Release','Build it before you fire it.')
ROOT.joinpath('batch31-candidates.json').write_text(json.dumps(levels,indent=2))

if __name__=='__main__':
    import sys
    if '--baseline' in sys.argv:
        old=[]
        for l in levels:
            text=(ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset").read_text();pieces=[]
            for m in re.finditer(r'  - type: (\d+)\s+color: (\d+)\s+direction: (\d+)\s+position: \{x: (\d+), y: (\d+)\}\s+channel: (\d+)\s+initialOpen: (\d+)',text):
                t,c,d,x,y,ch,op=map(int,m.groups());pieces.append(P(t,x,y,d,c,ch,bool(op)))
            old.append(dict(n=l['n'],name=l['name'],width=int(re.search(r'  width: (\d+)',text)[1]),height=int(re.search(r'  height: (\d+)',text)[1]),budget=int(re.search(r'  moveLimit: (\d+)',text)[1]),pieces=pieces))
        ROOT.joinpath('batch31-old.json').write_text(json.dumps(old,indent=2))
    if '--apply' in sys.argv:
        proofs={p['number']:p for p in json.loads(ROOT.joinpath('batch31-analysis.json').read_text(encoding='utf-8-sig'))}
        for l in levels:
            proof=proofs[l['n']];result=proof['analysis'];assert result['minimum']==l['budget']
            values=[l['width'],l['height'],l['budget'],1];stateful=any(p['type'] in (6,7) for p in l['pieces'])
            for p in l['pieces']:
                values += [p[k] for k in ('type','color','direction','x','y')]
                if stateful:values += [p['channel'],int(p['initialOpen'])]
            assert hashlib.sha256(''.join(str(v)+',' for v in values).encode()).hexdigest()==proof['fingerprint']
            path=ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset";old=path.read_text();head=old[:old.index('  width:')]
            lines=[f"  width: {l['width']}",f"  height: {l['height']}",f"  moveLimit: {l['budget']}",'  targetColor: 1','  placements:']
            for p in l['pieces']:
                lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
            lines += ['  displayTitle: '+l['title'],'  hint: '+json.dumps(l['hint']),'  knownSolution:']
            lines += [f"  - {{x: {p['x']}, y: {p['y']}}}" for p in result['shortest']]
            path.write_text(head+'\n'.join(lines)+'\n')
        catalog=ROOT.parent/'Assets/_Game/Resources/LevelDesignCatalog.asset';entries=catalog.read_bytes().decode().split('  - level: ')
        metadata=[(1,11,2,6,'Parking and vacating the upper workshop releases the lower delivery choke; early target pushes redirect its occupant.'),
                  (0,11,3,4,'Leave the starting junction, clear the temporary parking, and return to the same cell with reversed direction.'),
                  (2,58,3,8,'Two perpendicular targets rendezvous at a crossing; the rear target redirects and shares delivery with the waiting target.'),
                  (4,27,3,8,'The direct rotator entry strands the target; lift it into the outer arc, clear its return landing and enter the same rotator from above.'),
                  (4,31,4,8,'Stage separated targets in the firing row, turn the supporting launcher into alignment, open the gate and release the constructed push chain.')]
        for n,(primary,reason,difficulty,payoff,rationale) in enumerate(metadata,31):
            for key,value in dict(primary=primary,secondary='',reasoningStyle=reason,difficultyBand=difficulty,sculptedTopology=1,hasPayoffMove=1,expectedPayoffDepth=payoff,designRationale=json.dumps(rationale)).items():
                entries[n]=re.sub(r'(?m)^(    '+key+r':)[^\r\n]*',lambda m:m[1]+' '+str(value),entries[n])
        catalog.write_bytes('  - level: '.join(entries).encode())
        cert=ROOT.parent/'Assets/_Game/Scripts/Levels/VerifiedOptimality.cs';text=cert.read_text()
        for proof in proofs.values():
            if proof['fingerprint'] not in text:
                text=text.replace('                _ => null',f'                "{proof["fingerprint"]}" => {proof["analysis"]["minimum"]}, // Level {proof["number"]}; Batch31Tests exhaustive graph\n                _ => null')
        cert.write_text(text)
