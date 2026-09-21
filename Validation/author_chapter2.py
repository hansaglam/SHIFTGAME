"""Explicit Chapter 2 authoring data. No random generation or runtime solver."""
from pathlib import Path
import uuid, json, re
ROOT = Path(__file__).resolve().parents[1] / 'Assets/_Game'
def P(t,x,y,d=0,c=0,ch=0,op=False): return dict(type=t,color=c,direction=d,x=x,y=y,channel=ch,initialOpen=op)
def R(x,y,d=4): return P(0,x,y,d,1)
def Y(x,y,d=4): return P(0,x,y,d,3)
def B(x,y,d=4): return P(0,x,y,d,2)
def S(x,y,ch=1): return P(6,x,y,ch=ch)
def G(x,y,ch=1,op=False): return P(7,x,y,ch=ch,op=op)
def E(x,y): return P(5,x,y,c=1)
def D(x,y,d): return P(3,x,y,d)
def T(x,y): return P(4,x,y)
def X(x,y): return P(1,x,y)
def W(x,y): return P(2,x,y)
levels=[]
def L(n,name,size,pieces,taps,hint): levels.append(dict(n=n,name=f'Level{n:02}_{name}',size=size,pieces=pieces,taps=taps,hint=hint))
L(21,'FirstSwitch',4,[R(0,1),S(1,1),G(2,1),E(3,1)],[(0,1),(1,1),(2,1)],'Enter the diamond pad to toggle its matching gate.')
L(22,'OpenForRed',4,[Y(0,0,1),S(0,1),R(1,2),G(2,2),E(3,2)],[(0,0),(1,2),(2,2)],'Yellow can open the route for Red.')
L(23,'ToggleTwice',4,[R(0,1),S(1,1),G(2,1,op=True),E(3,1),Y(0,3),S(1,3)],[(0,3),(0,1),(1,1),(2,1)],'Two entries toggle twice. Prepare the open gate before Red enters its pad.')
L(24,'ClosedLane',4,[R(0,2),G(1,2),E(3,2),Y(3,0,3),S(2,0)],[(3,0),(0,2),(1,2),(2,2)],'Closed gates block movement. Activate Yellow first.')
L(25,'BeforeYouGo',5,[R(0,1),G(2,1),E(4,1),Y(4,3,3),S(3,3)],[(4,3),(0,1),(1,1),(2,1),(3,1)],'Plan the gate state before spending moves along the lane.')
L(26,'ThreeEntries',5,[R(0,1),S(1,1),S(2,1),G(3,1),E(4,1),Y(0,3),S(1,3)],[(0,3),(0,1),(1,1),(2,1),(3,1)],'Every entry toggles. Three pad entries leave the gate open.')
L(27,'BoxDoesNotPress',4,[Y(0,1),X(1,1),S(2,1),R(1,0,1),G(1,2),E(1,3)],[(0,1),(1,1),(1,0),(1,1),(1,2)],'Boxes do not press pads. Push it aside, then let Yellow enter.')
L(28,'TurnThrough',4,[R(0,1),S(1,1),D(2,1,1),G(2,2),E(2,3)],[(0,1),(1,1),(2,2)],'Open the gate before the arrow sends Red through it.')
L(29,'ClockworkGate',4,[R(1,0,1),S(1,1),T(1,2),G(2,2),E(3,2)],[(1,0),(1,1),(2,2)],'The rotator still turns clockwise. The pad prepares its landing.')
# Floor sets describe topology using ordinary Wall pieces only.
def rooms(size, pieces, floor):
 occupied={(p['x'],p['y']) for p in pieces}
 return pieces+[W(x,y) for y in range(size) for x in range(size) if (x,y) not in occupied|set(floor)]
L(30,'ClearThenOpen',5,rooms(5,[R(0,2),B(2,2,2),S(2,1),D(3,2,1),G(3,3),D(3,4,4),E(4,4)],[(1,2),(2,0)]),[(2,2),(0,2),(1,2),(2,2),(3,3)],'Blue prepares the lower alcove. Red finishes around the high corner.')
L(31,'OnePadTwoGates',5,rooms(5,[R(0,1),S(1,1),G(2,1),G(3,1),E(4,1)],[(0,0),(1,0),(4,2),(4,3)]),[(0,1),(1,1),(2,1),(3,1)],'One diamond pad toggles both matching gates.')
L(32,'OppositeStates',5,rooms(5,[R(0,3),G(1,3,op=True),S(2,3),G(3,3),E(4,3),R(0,1),G(1,1),S(2,1),G(3,1,op=True),E(4,1)],[(0,4),(1,4),(3,4),(4,4),(0,0),(1,0),(3,0),(4,0)]),[(0,3),(1,3),(2,3),(3,3),(0,1),(1,1),(2,1),(3,1)],'Finish the upper room before the lower pad reverses all four gates.')
L(33,'SharedDeliveryGate',5,rooms(5,[R(0,1),R(1,1),G(2,1),G(3,1),E(4,1),Y(0,3),S(1,3)],[(2,3),(0,4),(1,4),(4,0)]),[(0,3),(0,1),(1,1),(2,1),(3,1)],'Open both gates, then push both Reds down the shared lane.')
L(34,'ClearTheCorridor',5,rooms(5,[R(0,2),B(2,2,2),S(2,1),G(3,2),D(4,2,1),E(4,3)],[(1,2),(2,0),(2,3),(2,4),(0,1),(0,3)]),[(2,2),(0,2),(1,2),(2,2),(3,2)],'Blue yields the junction and opens the east passage. Red takes the corner.')
L(35,'TwoChannels',4,rooms(4,[R(0,2),G(1,2,1),G(2,2,2),E(3,2),Y(0,0),S(1,0,1),B(3,3,3),S(2,3,2)],[(2,0),(0,1)]),[(0,0),(3,3),(0,2),(1,2),(2,2)],'Two isolated staging pads prepare the narrow delivery lane.')
L(36,'CrossBeforeClosing',5,rooms(5,[Y(0,0),S(1,0,1),S(2,0,1),B(0,3),G(1,3,1),S(2,3,1),S(3,3,2),R(1,1),G(2,1,2),E(4,1)],[(3,1),(4,3),(0,4),(1,4)]),[(0,0),(0,3),(1,3),(2,3),(1,1),(2,1),(3,1)],"Open Blue's approach, cross before reversing it, then reach the second channel.")
L(37,'OpenTheSecondPad',5,rooms(5,[Y(0,0),S(1,0,1),B(0,3),G(1,3,1),S(2,3,2),R(1,1),G(2,1,2),E(4,1)],[(2,0),(3,1),(3,3),(0,4),(1,4),(2,4)]),[(0,0),(0,3),(1,3),(1,1),(2,1),(3,1)],'One diamond opens the way to the two-diamond pad.')
L(38,'LiftAndTurn',6,rooms(6,[R(0,1),X(1,1),D(2,1,1),S(2,2),T(2,3),G(3,3),D(4,3,1),T(4,4),E(5,4),B(2,4)],[(3,4),(0,0),(1,0),(3,1),(4,5),(5,5)]),[(2,4),(0,1),(1,1),(2,2),(3,3)],'Clear the box parking space. Prepare the gate, then release the final cascade.')
L(39,'PrepareToggleRedirect',5,rooms(5,[R(0,2),R(1,2),G(2,2),D(3,2,1),D(3,3,4),E(4,3),Y(0,0),S(1,0)],[(2,0),(0,1),(0,3),(1,3),(4,4)]),[(0,0),(0,2),(1,2),(2,2)],'Both Reds share the throat. Can one help deliver the other?')
levels[-1]['budget']=6
levels[-1]['alternative']=[(0,0),(1,2),(2,2),(0,2),(1,2),(2,2)]
L(40,'TheStateOfShift',6,rooms(6,[R(0,1),X(1,1),S(1,1,1),D(2,1,1),G(2,2,1),S(2,3,2),T(2,4),G(3,4,2),D(4,4,1),D(4,5,4),D(5,5,2),E(5,4),B(3,1,2),Y(1,0,1)],[(3,0),(1,2),(0,0),(0,2),(3,3),(4,3),(5,3)]),[(3,1),(0,1),(1,1),(2,2),(2,3),(3,4)],'Clear Blue, park the box east, then join the two rooms. The tempting upward push changes the plan.')

for l in levels:
 path=ROOT/'Data/Levels'/f"{l['name']}.asset"
 lines=['%YAML 1.1','%TAG !u! tag:unity3d.com,2011:','--- !u!114 &11400000','MonoBehaviour:',
 '  m_ObjectHideFlags: 0','  m_CorrespondingSourceObject: {fileID: 0}','  m_PrefabInstance: {fileID: 0}',
 '  m_PrefabAsset: {fileID: 0}','  m_GameObject: {fileID: 0}','  m_Enabled: 1','  m_EditorHideFlags: 0',
 '  m_Script: {fileID: 11500000, guid: 5c1f84ab736b460fa687851315dce861, type: 3}',f"  m_Name: {l['name']}",'  m_EditorClassIdentifier:',
 f"  width: {l['size']}",f"  height: {l['size']}",f"  moveLimit: {l.get('budget',len(l['taps']))}",'  targetColor: 1','  placements:']
 for p in l['pieces']:
  lines += [f"  - type: {p['type']}",f"    color: {p['color']}",f"    direction: {p['direction']}",f"    position: {{x: {p['x']}, y: {p['y']}}}",f"    channel: {p['channel']}",f"    initialOpen: {int(p['initialOpen'])}"]
 lines += [f"  displayTitle: {l['n']:02} - " + re.sub(r"(?<!^)(?=[A-Z])", " ", l['name'].split('_')[1]),'  hint: '+json.dumps(l['hint']),'  knownSolution:']
 lines += [f'  - {{x: {x}, y: {y}}}' for x,y in l['taps']]
 path.write_text('\n'.join(lines)+'\n',encoding='utf-8')
 meta=Path(str(path)+'.meta')
 if not meta.exists(): meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n',encoding='utf-8')
Path(__file__).with_name('chapter2-design.json').write_text(json.dumps(levels,indent=2),encoding='utf-8')
