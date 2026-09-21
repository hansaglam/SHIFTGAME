"""Only three authorized puzzle snapshots and their metadata may change."""
from pathlib import Path
import re,json,hashlib,uuid
root=Path(__file__).resolve().parent.parent;game=root/'Assets/_Game';tests=game/'Tests/Editor'
baseline={p['path']:p['hash'] for p in json.loads((root/'Validation/refine26-baseline.json').read_text(encoding='utf-8-sig'))}
def hashbytes(b):return hashlib.sha256(b).hexdigest().upper()
allowed={'_Game/Scripts/Levels/VerifiedOptimality.cs','_Game/Resources/LevelDesignCatalog.asset'}
allowed.update(p[7:] for p in baseline if re.search(r'/Level(26|27|30)_.+\.asset$',p))
for name in ['Sprint5FrozenFiles.txt','BoardVisualFrozenFiles.txt','IdentityFrozenFiles.txt','Batch21FrozenFiles.txt','Batch26FrozenFiles.txt']:
    path=tests/name;lines=[]
    for line in path.read_text().splitlines():
        rel,prior=line.split('|');actual=hashbytes((root/'Assets'/rel).read_bytes())
        if rel in allowed:prior=actual
        else:assert actual==prior,(name,rel,'Unrelated file differs from frozen baseline')
        lines.append(rel+'|'+prior)
    path.write_text('\n'.join(lines)+'\n')
catalog=(game/'Resources/LevelDesignCatalog.asset').read_bytes().decode().split('  - level: ')
original=(root/'Validation/refine26-original-catalog.txt').read_bytes().decode().split('  - level: ')
for n in range(1,41):
    if n not in (26,27,30):assert catalog[n]==original[n],n
for name in ['Batch21CatalogFrozen.txt','Batch26CatalogFrozen.txt']:
    path=tests/name;lines=[]
    for line in path.read_text().splitlines():
        n,prior=line.split('|')
        if int(n) in (26,27,30):prior=hashbytes(catalog[int(n)].encode())
        else:assert hashbytes(catalog[int(n)].encode())==prior,(name,n)
        lines.append(n+'|'+prior)
    path.write_text('\n'.join(lines)+'\n')
protected={line.split('|')[0] for line in (tests/'Batch26FrozenFiles.txt').read_text().splitlines()}
protected.update(p[7:] for p in baseline if re.search(r'/Level(28|29)_.+\.asset$',p))
lines=[]
for rel in sorted(protected):
    old=baseline['Assets/'+rel];assert hashbytes((root/'Assets'/rel).read_bytes())==old,rel
    lines.append(rel+'|'+old)
(tests/'Refinement26FrozenFiles.txt').write_text('\n'.join(lines)+'\n')
(tests/'Refinement26CatalogFrozen.txt').write_text('\n'.join(str(n)+'|'+hashbytes(original[n].encode()) for n in range(1,41) if n not in (26,27,30))+'\n')
for name in ['Refinement26Tests.cs','Refinement26FrozenFiles.txt','Refinement26CatalogFrozen.txt']:
    meta=tests/(name+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n')
print('Verified',len(protected),'protected files and 37 catalog entries.')
