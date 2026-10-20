const fs=require('node:fs'),path=require('node:path');
const runtime=path.join(require('node:os').homedir(),'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules');
let sharp;try{sharp=require('sharp');}catch{sharp=require(path.join(runtime,'sharp'));}
const project=path.resolve(__dirname,'..'),output=path.join(project,'Documentation','PressKit');
fs.mkdirSync(output,{recursive:true});
const art=name=>'data:image/png;base64,'+fs.readFileSync(path.join(project,'Assets','Art',name+'.png')).toString('base64');
const blocks=Array.from({length:70},(_,i)=>{const row=Math.floor(i/7),col=i%7;return '<rect x="'+(col*100-(row%2)*50)+'" y="'+(row*50)+'" width="97" height="47" fill="'+(i%3===0?'#281e2c':'#211a27')+'"/>';}).join('');
const svg='<svg xmlns="http://www.w3.org/2000/svg" width="630" height="500" viewBox="0 0 630 500">'+
'<rect width="630" height="500" fill="#130f1c"/>'+blocks+
'<circle cx="495" cy="273" r="107" fill="#5d2339"/><circle cx="485" cy="260" r="87" fill="#7f2c44"/>'+
'<path d="M388 390V250l22-30v-28l20-20v-25l25-27h17v30l-20 24v30l-19 22v164M576 390V250l-22-30v-28l-20-20v-25l-25-27h-17v30l20 24v30l19 22v164" fill="#3f2e41" stroke="#8c6b73" stroke-width="5"/>'+
'<g font-family="Arial,sans-serif"><text x="40" y="55" fill="#e6c273" font-size="13" font-weight="700" letter-spacing="3">8-BIT / CASTELO SOB CERCO</text>'+
'<text x="36" y="123" fill="#f0e6d2" font-size="63" font-weight="800" letter-spacing="-2">THE BLOOD</text>'+
'<text x="36" y="191" fill="#e55b70" font-size="73" font-weight="800" letter-spacing="-2">WARFARE</text>'+
'<text x="42" y="250" font-size="18" fill="#c4aebc">A noite pertence</text><text x="42" y="276" font-size="18" fill="#c4aebc">aos sedentos.</text>'+
'<text x="42" y="323" font-size="16" fill="#e6c273">Recolha o sangue.</text><text x="42" y="348" font-size="16" fill="#e6c273">Escape dos templários.</text>'+
'<image href="'+art('Vampire')+'" x="399" y="207" width="166" height="166" style="image-rendering:pixelated"/>'+
'<image href="'+art('BloodVial')+'" x="304" y="286" width="70" height="70" style="image-rendering:pixelated"/>'+
'<image href="'+art('Templar')+'" x="520" y="345" width="77" height="77" style="image-rendering:pixelated"/>'+
'<rect x="42" y="387" width="328" height="37" fill="#e6c273"/><text x="54" y="411" font-size="14" font-weight="700" fill="#241627">45 SEGUNDOS · 12 FRASCOS · 3 VIDAS</text>'+
'<text x="42" y="464" font-size="13" fill="#b6a0b2" letter-spacing="2">MATHIAS ZANOTTO</text></g></svg>';
(async()=>{fs.writeFileSync(path.join(output,'capa.svg'),svg);await sharp(Buffer.from(svg)).png().toFile(path.join(output,'capa.png'));
for(const name of ['menu','jogo','vitoria','derrota-tempo','derrota-vida'])fs.copyFileSync(path.join(project,'Documentation','Previews',name+'.png'),path.join(output,name+'.png'));
console.log('BLOOD_PRESSKIT_OK');})().catch(e=>{console.error(e);process.exitCode=1;});
