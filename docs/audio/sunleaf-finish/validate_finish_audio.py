import hashlib,json,math,struct,wave
from pathlib import Path
root=Path(__file__).resolve().parent
results=[]
for declared in json.loads((root/'manifest.json').read_text())['clips']:
    p=root/'wav'/declared['file']
    with wave.open(str(p),'rb') as f:
        channels=f.getnchannels();rate=f.getframerate();width=f.getsampwidth()
        raw=f.readframes(f.getnframes())
    samples=struct.unpack('<'+'h'*(len(raw)//2),raw)
    peak=max(abs(x) for x in samples)
    checks={'format':rate==44100 and width==2 and channels==declared['channels'],
            'sha256':hashlib.sha256(p.read_bytes()).hexdigest()==declared['sha256'],
            'nonzero':peak>100,'not_clipped':peak<32767,
            'silent_endpoints':all(x==0 for x in samples[:channels]+samples[-channels:]),
            'peak_within_tolerance':abs(20*math.log10(peak/32767)-declared['peak_dbfs'])<.02}
    results.append({'file':p.name,'checks':checks,'pass':all(checks.values())})
report={'pass':all(x['pass'] for x in results),'clips':results,'limitation':'Technical checks only; listening and runtime mix not qualified.'}
(root/'verification_report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
raise SystemExit(0 if report['pass'] else 1)
