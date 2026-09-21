"""V2 authoring: only 26, 27, 30. No imports of historical authoring scripts."""
from pathlib import Path
import json,hashlib,re
ROOT=Path(__file__).resolve().parent
def P(t,x,y,d=0,c=0,ch=0,op=False):return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4):return P(0,x,y,d,1)
def B(x,y,d=4):return P(0,x,y,d,2)
def Y(x,y,d=4):return P(0,x,y,d,3)
def X(x,y):return P(1,x,y)
def D(x,y,d):return P(3,x,y,d)
def S(x,y,ch=1):return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False):return P(7,x,y,ch=ch,op=op)
def E(x,y):return P(5,x,y,c=1)
levels=[]
def L(n,name,w,h,budget,pieces,floor,title,hint):
    occupied={(p['x'],p['y']) for p in pieces}|set(floor)
    pieces += [P(2,x,y) for y in range(h) for x in range(w) if (x,y) not in occupied]
    levels.append(dict(n=n,name=name,width=w,height=h,budget=budget,pieces=pieces,title=title,hint=hint))
L(26,'Level26_ThreeEntries',4,4,4,
  [R(0,1),B(1,1,1),Y(1,2,3),D(1,3,4),D(2,3,2),D(2,2,4),D(2,1,1),E(3,2)],[(0,2)],
  '26 - Let It Wait','A clear lane starts one step away.')
L(27,'Level27_BoxDoesNotPress',4,5,5,
  [R(0,2),X(1,2),B(1,1,1),Y(3,2,3),D(2,2,2),D(2,0,4),E(3,0)],[(1,3),(1,4),(2,1)],
  '27 - Borrowed Space','An empty space is not always spare.')
L(30,'Level30_ClearThenOpen',6,5,7,
  [R(0,2),R(1,2),B(2,2,2),X(2,1),S(2,1),Y(1,1),G(3,2,op=True),D(4,2,1),D(4,3,4),D(5,3,1),E(5,4),Y(3,0,1)],[(3,1),(4,1),(5,1),(3,3)],
  '30 - Release the Relay','Create space. Keep the way back clear.')
ROOT.joinpath('refine26-candidates.json').write_text(json.dumps(levels,indent=2))
if __name__=='__main__':
    import sys
    if '--apply' in sys.argv:
        proofs={p['number']:p for p in json.loads(ROOT.joinpath('refine26-analysis.json').read_text(encoding='utf-8-sig'))}
        for l in levels:
            proof=proofs[l['n']];result=proof['analysis'];assert result['minimum']==l['budget']
            stateful=any(p['type'] in (6,7) for p in l['pieces']);values=[l['width'],l['height'],l['budget'],1]
            for p in l['pieces']:
                values += [p[k] for k in ('type','color','direction','x','y')]
                if stateful:values += [p['channel'],int(p['initialOpen'])]
            assert hashlib.sha256(''.join(str(v)+',' for v in values).encode()).hexdigest()==proof['fingerprint']
            path=ROOT.parent/'Assets/_Game/Data/Levels'/f"{l['name']}.asset"
            old=path.read_text();head=old[:old.index('  width:')]
            lines=[f"  width: {l['width']}",f"  height: {l['height']}",f"  moveLimit: {l['budget']}",'  targetColor: 1','  placements:']
            for p in l['pieces']:
                lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
            lines += ['  displayTitle: '+l['title'],'  hint: '+json.dumps(l['hint']),'  knownSolution:']
            lines += [f"  - {{x: {p['x']}, y: {p['y']}}}" for p in result['shortest']]
            path.write_text(head+'\n'.join(lines)+'\n')
        catalog=ROOT.parent/'Assets/_Game/Resources/LevelDesignCatalog.asset';entries=catalog.read_bytes().decode().split('  - level: ')
        metadata={26:dict(primary=2,reasoningStyle=27,expectedPayoffDepth=6,hasPayoffMove=1,designRationale=json.dumps('Vacate the upper landing before freeing the lower lane; both premature pushes feed a wrong color into the exit approach.')),
                  27:dict(primary=2,reasoningStyle=11,expectedPayoffDepth=4,hasPayoffMove=1,designRationale=json.dumps('Choose the north parking pocket over the east recess, then vacate the shared crossing before using the lower return.')),
                  30:dict(designRationale=json.dumps('Preserve the box staging and parking cells while closing and reopening the gate; premature access preparation consumes the parking bay and costs a recoverable extra move.'))}
        for n,fields in metadata.items():
            for key,value in fields.items():entries[n]=re.sub(r'(?m)^(    '+key+r':)[^\r\n]*',lambda m:m[1]+' '+str(value),entries[n])
        catalog.write_bytes('  - level: '.join(entries).encode())
        cert=ROOT.parent/'Assets/_Game/Scripts/Levels/VerifiedOptimality.cs';text=cert.read_text()
        for proof in proofs.values():
            if proof['fingerprint'] not in text:
                text=text.replace('                _ => null',f'                "{proof["fingerprint"]}" => {proof["analysis"]["minimum"]}, // Level {proof["number"]} V2; Refinement26Tests\n                _ => null')
        cert.write_text(text)
