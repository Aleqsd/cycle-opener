"""Preview-only cache; textures are never committed or shipped with the plugin."""
import concurrent.futures,json,pathlib,re,struct,urllib.request
from PIL import Image
root=pathlib.Path(__file__).resolve().parents[1]
cache=root/'.artifacts/icons';cache.mkdir(exist_ok=True,parents=True)
actions={int(m[0]):int(m[1]) for m in re.findall(r'\[\d+\]=new\((\d+),"[^"]+",\d+,(\d+),', (root/'src/Spells.cs').read_text(encoding='utf8'))}
for a in json.loads((root/'docs/imported-actions.json').read_text(encoding='utf8'))['actions']:actions[a['id']]=a['icon']
for job in [19,20,21,22,23,24,25,27,28,30,31,32,33,34,35,37,38,39,40,41,42]:actions[62100+job]=62100+job
def obtain(item):
    id,icon=item;f=cache/f'{id}.png'
    if not f.exists():
        path=f'ui/icon/{icon//1000*1000:06d}/{icon:06d}.tex'
        req=urllib.request.Request(f'https://v2.xivapi.com/api/asset?path={path}&format=png',headers={'User-Agent':'CycleOpener-local-preview'})
        f.write_bytes(urllib.request.urlopen(req,timeout=40).read())
    im=Image.open(f).convert('RGBA')
    (cache/f'{id}.rgba').write_bytes(struct.pack('<ii',im.width,im.height)+im.tobytes())
with concurrent.futures.ThreadPoolExecutor(6) as pool:list(pool.map(obtain,actions.items()))
print(f'{len(actions)} action/job icons cached for off-game previews only.')
