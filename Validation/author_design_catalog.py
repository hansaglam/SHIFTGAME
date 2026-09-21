"""Reviewed 40-level design audit; separate metadata never enters puzzle simulation."""
from pathlib import Path
import re,json,uuid
root=Path(__file__).resolve().parents[1]
game=root/'Assets/_Game'
# primary, difficulty, reasoning flags, secondary archetypes, rationale
# Archetypes Corridor0 Rooms1 Crossroads2 StateMachine3 Cascade4 Optimization5.
# Reasoning Setup1 Sequence2 State4 Space8 Cascade16 Optimization32.
rows=[
(0,0,2,[],'First tap and directional intent. Keep tutorial unchanged.'),
(0,0,2,[],'One-cell movement and exit distance. Keep unchanged.'),
(2,0,9,[0],'Yellow clears the box before Red. Keep unchanged.'),
(4,0,16,[],'First multi-piece push. Keep unchanged.'),
(0,0,8,[],'Blocked attempts are free. Keep unchanged.'),
(0,1,2,[4],'Direction tile and corner prediction. Keep unchanged.'),
(2,1,9,[0],'Blue must leave the red exit lane. Keep unchanged.'),
(4,1,2,[],'Clockwise transformation teaching. Keep unchanged.'),
(4,1,18,[2],'Predict redirected push landing. Keep unchanged.'),
(4,2,19,[2],'Prepare space before releasing a chain. Keep unchanged.'),
(0,2,9,[2],'Staging before delivery. Keep unchanged.'),
(2,2,10,[0],'Corner access order. Keep unchanged.'),
(4,2,19,[],'One reaction prepares the next. Keep unchanged.'),
(0,2,18,[4],'Trace two corners. Keep unchanged.'),
(4,3,9,[2],'Long setup before the tempting chain. Keep unchanged.'),
(4,3,18,[],'Rotator then direction prediction. Keep unchanged.'),
(2,3,10,[0],'Shared corner timing. Keep unchanged.'),
(1,3,11,[4],'Parking space then trigger then delivery. Keep unchanged.'),
(5,3,34,[0],'Cooperative target delivery. Keep unchanged.'),
(4,5,27,[2],'Chapter 1 combined-mechanic finale. Keep unchanged.'),
(3,0,4,[0],'Introduce one pad and one gate. Preserve teaching.'),
(3,1,5,[1],'Other colors operate the target gate. Preserve teaching.'),
(3,2,6,[],'Two toggles and parity. Preserve teaching.'),
(0,1,5,[3],'Closed gate is a visible barrier. Preserve teaching.'),
(0,2,7,[3],'Budget preparation along the lane. Preserve teaching.'),
(3,2,6,[],'Odd/even entry count. Preserve teaching.'),
(3,2,5,[2],'Boxes do not activate switches. Preserve teaching.'),
(4,2,22,[3],'Switch before direction. Preserve teaching.'),
(4,2,22,[3],'Clockwise gate landing. Preserve teaching.'),
(0,3,31,[1,4],'L-shaped throat with a lower staging alcove. Blue yields and opens the upper gate; final corner delivers in depth 4.'),
(0,3,6,[3],'Thin lane and offset exit alcove; one pad opens two sequential gates. Spatial constraint plus state preparation.'),
(3,4,6,[1],'Two rooms exchange opposite gate states twice. Finish the upper target before the lower pad closes its exit gate.'),
(0,3,7,[1],'Detached switch staging room prepares the shared two-target corridor. Cooperating Reds reduce moves.'),
(2,3,15,[0],'Blue and Red compete for the central choke. Blue exits south to a pad; Red crosses east to the raised finish.'),
(1,4,13,[3],'Separate control pockets operate two consecutive channel barriers in the target lane. Topology, setup and state.'),
(3,4,7,[1],'Open the upper approach, cross, reverse A, then activate B. Second yellow pad is a premature-close decoy.'),
(1,4,7,[3],'Upper staging chamber leads to channel B, lower delivery lane waits for it. Topology, setup, state and ordered phases.'),
(4,4,31,[1],'Box parking preparation, gate activation and raised serpentine finish. The final depth-6 cascade follows several setup moves.'),
(5,4,42,[2,0],'Two Reds share a single throat. Together: 4 taps; individually: 6. Both paths deliver, efficient pushing earns mastery.'),
(1,5,31,[3,4],'Lower box staging room joins upper finish through gate A. Blue yields shared space; Yellow offers a tempting upward box push. Two channels and a depth-8 final return loop.')]
payoffs={30:4,31:4,32:6,33:4,34:4,38:6,39:6,40:8}
creative={30:2,32:4,34:1,38:2,39:5,40:3}
script=game/'Scripts/Levels/LevelDesignCatalog.cs'
meta=Path(str(script)+'.meta')
if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
guid=re.search(r'guid: (\w+)',meta.read_text())[1]
lines=['%YAML 1.1','%TAG !u! tag:unity3d.com,2011:','--- !u!114 &11400000','MonoBehaviour:',
'  m_ObjectHideFlags: 0','  m_CorrespondingSourceObject: {fileID: 0}','  m_PrefabInstance: {fileID: 0}',
'  m_PrefabAsset: {fileID: 0}','  m_GameObject: {fileID: 0}','  m_Enabled: 1','  m_EditorHideFlags: 0',
f'  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}','  m_Name: LevelDesignCatalog','  m_EditorClassIdentifier:','  entries:']
for n,(primary,difficulty,reason,secondary,rationale) in enumerate(rows,1):
 asset=next((game/'Data/Levels').glob(f'Level{n:02}_*.asset'))
 ref=re.search(r'guid: (\w+)',Path(str(asset)+'.meta').read_text())[1]
 lines += [f'  - level: {{fileID: 11400000, guid: {ref}, type: 2}}',f'    primary: {primary}']
 # Unity encodes enum lists as a packed little-endian integer sequence.
 lines += ['    secondary: '+''.join(v.to_bytes(4,'little').hex() for v in secondary),f'    difficultyBand: {difficulty}',f'    reasoningStyle: {reason}',
 f'    sculptedTopology: {int(n>=30)}',f'    hasPayoffMove: {int(n in payoffs)}',f'    expectedPayoffDepth: {payoffs.get(n,0)}',
 f'    creativeCandidate: {int(n in creative)}',f'    creativeHook: {creative.get(n,0)}','    designRationale: '+json.dumps(rationale)]
out=game/'Resources/LevelDesignCatalog.asset';out.parent.mkdir(exist_ok=True)
out.write_text('\n'.join(lines)+'\n',encoding='utf-8')
for path in list(game.rglob('*.cs'))+[out]+[d for d in game.rglob('*') if d.is_dir()]:
 meta=Path(str(path)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if path.is_dir() else ''))
