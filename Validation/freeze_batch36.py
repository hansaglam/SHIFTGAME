from pathlib import Path
import hashlib,json,re,uuid
r=Path(__file__).resolve().parent.parent;g=r/'Assets/_Game';t=g/'Tests/Editor'
base={p['path']:p['hash'] for p in json.loads((r/'Validation/batch36-baseline.json').read_text())}
def H(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
allowed={'_Game/Scripts/Board/BoardManager.cs','_Game/Scripts/Levels/LevelData.cs','_Game/Scripts/UI/GameHud.cs','_Game/Scripts/Systems/AnalyticsService.cs','_Game/Scripts/Levels/VerifiedOptimality.cs','_Game/Resources/LevelDesignCatalog.asset'}
allowed.update(p[7:] for p in base if re.search(r'/Level(3[6-9]|40)_.+\.asset$',p))
for p in t.glob('*FrozenFiles.txt'):
 if p.name=='Batch36FrozenFiles.txt':continue
 lines=[]
 for line in p.read_text().splitlines():
  rel,prior=line.split('|');actual=H(r/'Assets'/rel)
  if rel in allowed:prior=actual
  else:assert actual==prior,(p.name,rel)
  lines.append(rel+'|'+prior)
 p.write_text('\n'.join(lines)+'\n')
original=(r/'Validation/batch36-baseline/Resources/LevelDesignCatalog.asset').read_bytes().decode().split('  - level: ')
catalog=(g/'Resources/LevelDesignCatalog.asset').read_bytes().decode().split('  - level: ')
for n in range(1,36):assert original[n]==catalog[n],n
for p in t.glob('*CatalogFrozen.txt'):
 if p.name=='Batch36CatalogFrozen.txt':continue
 lines=[]
 for line in p.read_text().splitlines():
  n,prior=line.split('|');actual=hashlib.sha256(catalog[int(n)].encode()).hexdigest().upper()
  if int(n)>=36:prior=actual
  else:assert actual==prior,(p,n)
  lines.append(n+'|'+prior)
 p.write_text('\n'.join(lines)+'\n')
protected={line.split('|')[0] for line in (t/'Batch31FrozenFiles.txt').read_text().splitlines()}
protected={p for p in protected if not re.search(r'/Level(3[6-9]|40)_',p)}
protected.update(p[7:] for p in base if re.search(r'/Level\d\d_.+\.asset$',p) and int(re.search(r'Level(\d\d)_',p)[1])<=35)
lines=[]
for rel in sorted(protected):
 actual=H(r/'Assets'/rel)
 if rel not in allowed:assert actual==base['Assets/'+rel],rel
 lines.append(rel+'|'+actual)
(t/'Batch36FrozenFiles.txt').write_text('\n'.join(lines)+'\n')
(t/'Batch36CatalogFrozen.txt').write_text('\n'.join(str(n)+'|'+hashlib.sha256(original[n].encode()).hexdigest().upper() for n in range(1,36))+'\n')
(t/'Fixtures/LegacyBoardManager.txt').write_bytes((r/'Validation/batch36-baseline/Scripts/Board/BoardManager.cs').read_bytes())
old=(t/'Fixtures/LegacyBoardManager.txt').read_text();new=(g/'Scripts/Board/BoardManager.cs').read_text()
assert new==old.replace('private PieceColor targetColor;','private HashSet<PieceColor> targetColors;').replace('targetColor = level.TargetColor;','targetColors = new HashSet<PieceColor>(level.ResolvedTargetColors);').replace('p.Color == targetColor','targetColors.Contains(p.Color)')
for p in [g/'Editor/Batch36Review.cs',t/'Batch36Tests.cs',t/'Batch36FrozenFiles.txt',t/'Batch36CatalogFrozen.txt',t/'Fixtures/LegacyBoardManager.txt']:
 m=Path(str(p)+'.meta')
 if p.exists() and not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
print(len(protected),'frozen files; all Levels 1–35 and catalog entries unchanged; movement source unchanged.')
