"""Scope-checked snapshot updates for five authorized puzzles only."""
from pathlib import Path
import re,json,hashlib,uuid
root=Path(__file__).resolve().parent.parent;game=root/'Assets/_Game';tests=game/'Tests/Editor'
baseline={p['path']:p['hash'] for p in json.loads((root/'Validation/batch31-baseline.json').read_text(encoding='utf-8-sig'))}
def H(b):return hashlib.sha256(b).hexdigest().upper()
allowed={'_Game/Scripts/Levels/VerifiedOptimality.cs','_Game/Resources/LevelDesignCatalog.asset'}
allowed.update(p[7:] for p in baseline if re.search(r'/Level3[1-5]_.+\.asset$',p))
for name in ['Sprint5FrozenFiles.txt','BoardVisualFrozenFiles.txt','IdentityFrozenFiles.txt','Batch21FrozenFiles.txt','Batch26FrozenFiles.txt','Refinement26FrozenFiles.txt']:
    path=tests/name;lines=[]
    for line in path.read_text().splitlines():
        rel,prior=line.split('|');actual=H((root/'Assets'/rel).read_bytes())
        if rel in allowed:prior=actual
        else:assert actual==prior,(name,rel,'Unrelated file changed')
        lines.append(rel+'|'+prior)
    path.write_text('\n'.join(lines)+'\n')
catalog=(game/'Resources/LevelDesignCatalog.asset').read_bytes().decode().split('  - level: ')
original=(root/'Validation/batch31-original-catalog.txt').read_bytes().decode().split('  - level: ')
for n in range(1,41):
    if not 31<=n<=35:assert catalog[n]==original[n],n
for name in ['Batch21CatalogFrozen.txt','Batch26CatalogFrozen.txt','Refinement26CatalogFrozen.txt']:
    path=tests/name;lines=[]
    for line in path.read_text().splitlines():
        n,prior=line.split('|')
        if 31<=int(n)<=35:prior=H(catalog[int(n)].encode())
        else:assert H(catalog[int(n)].encode())==prior,(name,n)
        lines.append(n+'|'+prior)
    path.write_text('\n'.join(lines)+'\n')
protected={line.split('|')[0] for line in (tests/'Refinement26FrozenFiles.txt').read_text().splitlines()}
protected={p for p in protected if not re.search(r'/Level3[1-5]_',p)}
protected.update(p[7:] for p in baseline if re.search(r'/Level(26|27|30)_.+\.asset$',p))
lines=[]
for rel in sorted(protected):
    old=baseline['Assets/'+rel];assert H((root/'Assets'/rel).read_bytes())==old,rel;lines.append(rel+'|'+old)
(tests/'Batch31FrozenFiles.txt').write_text('\n'.join(lines)+'\n')
(tests/'Batch31CatalogFrozen.txt').write_text('\n'.join(str(n)+'|'+H(original[n].encode()) for n in range(1,41) if not 31<=n<=35)+'\n')
for rel in ['Editor/Batch31Review.cs','Tests/Editor/Batch31Tests.cs','Tests/Editor/Batch31FrozenFiles.txt','Tests/Editor/Batch31CatalogFrozen.txt','Tests/Editor/Fixtures','Tests/Editor/Fixtures/LegacyOppositeRooms.asset']:
    meta=game/(rel+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if (game/rel).is_dir() else ''))
assert H((tests/'Fixtures/LegacyOppositeRooms.asset').read_bytes())==baseline['Assets/_Game/Data/Levels/Level32_OppositeStates.asset']
print('Verified',len(protected),'protected files, 35 catalog entries and the original parity fixture.')
