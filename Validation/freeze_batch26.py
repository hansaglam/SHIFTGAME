"""Refresh only authorized snapshots; preserve all other original digests."""
from pathlib import Path
import json,hashlib,re,uuid
root=Path(__file__).resolve().parent.parent
game=root/'Assets/_Game'; tests=game/'Tests/Editor'
baseline={p['path']:p['hash'] for p in json.loads((root/'Validation/batch26-baseline.json').read_text(encoding='utf-8-sig'))}
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
allowed={'_Game/Scripts/Levels/VerifiedOptimality.cs','_Game/Resources/LevelDesignCatalog.asset'}
allowed.update(str(p.relative_to(root/'Assets')).replace('\\','/') for p in (game/'Data/Levels').glob('Level*.asset') if 26<=int(p.name[5:7])<=30)
for name in ['Sprint5FrozenFiles.txt','BoardVisualFrozenFiles.txt','IdentityFrozenFiles.txt','Batch21FrozenFiles.txt']:
    path=tests/name;lines=[]
    for line in path.read_text().splitlines():
        rel,prior=line.split('|');actual=digest(root/'Assets'/rel)
        if rel=='_Game/Scenes/Prototype.unity' and actual!=prior:
            # Pre-existing user Inspector selection: preserve it, do not restore an old selected level.
            scene=(root/'Assets'/rel).read_bytes()
            assert actual==baseline['Assets/'+rel]
            assert hashlib.sha256(scene.replace(b'  selectedLevel: 21',b'  selectedLevel: 15')).hexdigest().upper()==prior
            prior=actual
        if rel in allowed: prior=actual
        else: assert actual==prior,(name,rel,'Unrelated content changed')
        lines.append(rel+'|'+prior)
    path.write_text('\n'.join(lines)+'\n')
catalog=(game/'Resources/LevelDesignCatalog.asset').read_bytes().decode().split('  - level: ')
original=(root/'Validation/batch26-original-catalog.txt').read_bytes().decode().split('  - level: ')
for n in range(1,41):
    if not 26<=n<=30: assert catalog[n]==original[n],n
path=tests/'Batch21CatalogFrozen.txt';lines=[]
for line in path.read_text().splitlines():
    n,prior=line.split('|')
    if 26<=int(n)<=30:prior=hashlib.sha256(catalog[int(n)].encode()).hexdigest().upper()
    lines.append(n+'|'+prior)
path.write_text('\n'.join(lines)+'\n')
protected={line.split('|')[0] for line in (tests/'Batch21FrozenFiles.txt').read_text().splitlines()}
protected={p for p in protected if not re.search(r'/Level(26|27|28|29|30)_',p)}
protected.update(p[7:] for p in baseline if re.search(r'/Level(21|22|23|24|25)_.+\.asset$',p))
lines=[]
for rel in sorted(protected):
    old=baseline['Assets/'+rel];assert digest(root/'Assets'/rel)==old,rel
    lines.append(rel+'|'+old)
(tests/'Batch26FrozenFiles.txt').write_text('\n'.join(lines)+'\n')
(tests/'Batch26CatalogFrozen.txt').write_text('\n'.join(str(n)+'|'+hashlib.sha256(original[n].encode()).hexdigest().upper() for n in range(1,41) if not 26<=n<=30)+'\n')
for rel in ['Editor/Batch26Review.cs','Tests/Editor/Batch26Tests.cs','Tests/Editor/Batch26FrozenFiles.txt','Tests/Editor/Batch26CatalogFrozen.txt']:
    meta=game/(rel+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
print('Verified',len(protected),'protected files and 35 unchanged catalog entries.')
