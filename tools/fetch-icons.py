"""Local preview assets only. Never included in plugin packaging or Git."""
import concurrent.futures,json,pathlib,struct,urllib.request
from bs4 import BeautifulSoup
from PIL import Image
root=pathlib.Path(__file__).resolve().parents[1]
cache=root/'.artifacts/icons';cache.mkdir(exist_ok=True,parents=True)
spells=json.loads((root/'docs/actions.json').read_text(encoding='utf-8'))
spells.append({'id':7447,'name':'Extra Foudre','icon':468,'level':26})
soup=BeautifulSoup(urllib.request.urlopen('https://fr.finalfantasyxiv.com/jobguide/blackmage/',timeout=30).read(),'html.parser')
def obtain(s):
    f=cache/f'{s["id"]}.png'
    if not f.exists():
        n=soup.find('strong',string=s['name'])
        if not n:raise RuntimeError('Icon missing: '+s['name'])
        u=n.find_parent('tr').find('img')['src']
        f.write_bytes(urllib.request.urlopen(u,timeout=20).read())
    im=Image.open(f).convert('RGBA')
    (cache/f'{s["id"]}.rgba').write_bytes(struct.pack('<ii',im.width,im.height)+im.tobytes())
list(concurrent.futures.ThreadPoolExecutor(6).map(obtain,spells))
print(f'{len(spells)} official icons ready for local preview.')
