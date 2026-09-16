"""Original deterministic synthesized Sunleaf footsteps and victory resolve.
No samples or third-party source material. Python standard library only.
Run this file to reproduce all WAVs and the SHA256/technical manifest.
"""
import math, random, wave, struct, hashlib, json
from pathlib import Path
R=44100
ROOT=Path(__file__).resolve().parent
OUT=ROOT/'wav'
OUT.mkdir(exist_ok=True)
manifest=[]
def save(name, channels, peak_db):
    peak=max(abs(x) for c in channels for x in c)
    gain=10**(peak_db/20)/max(peak,1e-9)
    data=bytearray()
    for frame in zip(*channels):
        for x in frame: data.extend(struct.pack('<h',round(x*gain*32767)))
    path=OUT/(name+'.wav')
    with wave.open(str(path),'wb') as f:
        f.setnchannels(len(channels)); f.setsampwidth(2); f.setframerate(R); f.writeframes(data)
    manifest.append(dict(file=path.name,seconds=len(channels[0])/R,channels=len(channels),sample_rate=R,bit_depth=16,peak_dbfs=peak_db,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
for surface in ('stone','wood'):
    for v in range(3):
        rng=random.Random(1700+v+(0 if surface=='stone' else 100))
        n=int(R*.23); buf=[]; filtered=0
        for i in range(n):
            t=i/R; noise=rng.uniform(-1,1); filtered=.67*filtered+.33*noise
            onset=min(1,t/.0015); tail=min(1,(n-1-i)/(R*.025))
            base=112+v*9 if surface=='stone' else 175+v*13
            thump=.75*math.sin(2*math.pi*base*t)*math.exp(-t*43)
            grit=(noise-filtered)*math.exp(-t*(90 if surface=='stone' else 135))*.28
            body=math.sin(2*math.pi*base*2.73*t)*math.exp(-t*31)*(.04 if surface=='stone' else .21)
            scuff=filtered*math.exp(-((t-.045)/.026)**2)*.14
            buf.append(onset*tail*(thump+grit+body+scuff))
        save('sunleaf_footstep_'+surface+'_'+str(v+1),[buf],-12)
# A non-looping 8-second D-major cadence: warm plucks over held root/fifth.
n=R*8; left=[0.0]*n; right=[0.0]*n
def note(start,duration,midi,amp,pan):
    freq=440*2**((midi-69)/12)
    for i in range(int(start*R),min(n,int((start+duration)*R))):
        t=i/R-start; remain=duration-t
        envelope=(1-math.exp(-t*45))*math.exp(-t*1.7)*min(1,remain/.35)
        s=amp*envelope*(math.sin(2*math.pi*freq*t)+.22*math.sin(4*math.pi*freq*t)+.06*math.sin(6*math.pi*freq*t))
        left[i]+=s*math.cos((pan+1)*math.pi/4);right[i]+=s*math.sin((pan+1)*math.pi/4)
for start,chord in [(0,[50,57,62]),(1.6,[55,59,62]),(3.2,[57,61,64]),(4.8,[50,57,62,66])]:
    for k,midi in enumerate(chord): note(start+k*.035,3.0,midi,.15,-.5+k*.3)
for start,midi in [(0,74),(.4,78),(.8,81),(1.6,79),(2.0,78),(2.4,74),(3.2,76),(3.6,73),(4,69),(4.8,74)]:
    note(start,2.6,midi,.12,.2)
save('sunleaf_victory_resolve',[left,right],-9)
(ROOT/'manifest.json').write_text(json.dumps({'provenance':'Original deterministic mathematical synthesis; no samples or external downloads. Not professionally mixed or listener-qualified.','clips':manifest},indent=2))
print(json.dumps(manifest,indent=2))
