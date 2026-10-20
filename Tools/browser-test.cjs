const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const runtime = path.join(require('node:os').homedir(), '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules');
function dependency(name) { try { return require(name); } catch { return require(path.join(runtime, name)); } }
const { chromium } = dependency('playwright');
const sharp = dependency('sharp');
const project = path.resolve(__dirname, '..');
const root = path.join(project, 'Builds', 'WebGL');
const output = path.join(project, 'Documentation', 'Browser');
const checks = [], errors = [], consoleMessages = [];
const mime = {'.html':'text/html; charset=utf-8','.js':'text/javascript; charset=utf-8','.wasm':'application/wasm','.data':'application/octet-stream','.json':'application/json','.png':'image/png','.css':'text/css'};
let browser, server;
function check(value, description) { if (!value) throw new Error(description); checks.push(description); console.log('BROWSER_CHECK_OK: '+description); }
async function shot(page, name) {
 const file=path.join(output,name+'.png'); await page.screenshot({path:file});
 const raw=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true}); return raw;
}
function gold(raw, area) {
 let count=0;
 for(let y=area.y;y<area.y+area.h;y++)for(let x=area.x;x<area.x+area.w;x++){
  const i=(y*raw.info.width+x)*4, r=raw.data[i],g=raw.data[i+1],b=raw.data[i+2];
  if(r>200&&g>160&&g<220&&b>80&&b<160&&r>g)count++;
 } return count;
}
function vampire(raw) {
 let sx=0,sy=0,n=0;
 for(let y=110;y<520;y++)for(let x=40;x<920;x++){
  const i=(y*raw.info.width+x)*4;
  if(Math.abs(raw.data[i]-240)<7&&Math.abs(raw.data[i+1]-230)<7&&Math.abs(raw.data[i+2]-210)<7){sx+=x;sy+=y;n++;}
 }
 return n>5?{x:sx/n,y:sy/n,pixels:n}:null;
}
function guards(raw) {
 const sides=[{x:0,y:0,n:0},{x:0,y:0,n:0}];
 for(let y=110;y<520;y++)for(let x=40;x<920;x++){const i=(y*raw.info.width+x)*4,r=raw.data[i],g=raw.data[i+1],b=raw.data[i+2];
  if(Math.abs(r-133)<6&&((Math.abs(g-129)<6&&Math.abs(b-139)<6)||(Math.abs(g-71)<6&&Math.abs(b-56)<6))){const side=sides[x<480?0:1];side.x+=x;side.y+=y;side.n++;}
 }return sides.map(s=>s.n>5?{x:s.x/s.n,y:s.y/s.n}:null);
}
(async()=>{
 fs.mkdirSync(output,{recursive:true});
 check(fs.existsSync(path.join(root,'index.html')),'Build WebGL possui index.html');
 server=http.createServer((req,res)=>{
  if(req.url==='/favicon.ico'){res.writeHead(204);return res.end();}
  if(req.url==='/__embed'){
   res.setHeader('content-type','text/html');
   return res.end('<!doctype html><html><body style="margin:0;background:#0b1724"><iframe title="itch.io style embed" src="/index.html" width="960" height="600" style="border:0;display:block" allow="autoplay; fullscreen; gamepad" allowfullscreen sandbox="allow-scripts allow-same-origin allow-pointer-lock"></iframe></body></html>');
  }
  let local;try{local=decodeURIComponent(new URL(req.url,'http://local').pathname);}catch{res.writeHead(400);return res.end();}
  const file=path.resolve(root,'.'+(local==='/'?'/index.html':local));
  if(!file.startsWith(root+path.sep)&&file!==root){res.writeHead(403);return res.end();}
  fs.stat(file,(err,stat)=>{if(err||!stat.isFile()){res.writeHead(404);return res.end();}res.writeHead(200,{'content-type':mime[path.extname(file)]||'application/octet-stream','cache-control':'no-store'});fs.createReadStream(file).pipe(res);});
 });
 await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
 const url='http://127.0.0.1:'+server.address().port;
 const executable=process.env.BROWSER_EXECUTABLE||'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';
 browser=await chromium.launch({executablePath:executable,headless:true,args:['--use-angle=swiftshader','--enable-unsafe-swiftshader']});
 const context=await browser.newContext({viewport:{width:960,height:600},deviceScaleFactor:1});
 // Simulate the browser Gamepad API, preserving Unity's WebGL input backend.
 // This verifies browser mapping; a physical controller remains a separate hardware check.
 await context.addInitScript(()=>{
  window.__audioNodes=[];
  const audioPrototype=window.AudioContext?.prototype;
  if(audioPrototype){const create=audioPrototype.createBufferSource;audioPrototype.createBufferSource=function(...args){const node=create.apply(this,args),record={node,context:this,started:false,stopped:false};window.__audioNodes.push(record);const start=node.start.bind(node),stop=node.stop.bind(node);node.start=(...a)=>{record.started=true;return start(...a);};node.stop=(...a)=>{record.stopped=true;return stop(...a);};return node;};}
  const pad={id:'Xbox 360 Controller (STANDARD GAMEPAD)',index:0,connected:false,mapping:'standard',timestamp:0,axes:[0,0,0,0],buttons:Array.from({length:17},()=>({pressed:false,touched:false,value:0}))};
  window.__virtualPad=pad;
  Object.defineProperty(navigator,'getGamepads',{configurable:true,value:()=>{pad.timestamp=performance.now();return pad.connected?[pad]:[];}});
  window.__connectPad=()=>{pad.connected=true;const event=new Event('gamepadconnected');Object.defineProperty(event,'gamepad',{value:pad});window.dispatchEvent(event);};
 });
 const page=await context.newPage();
 page.on('pageerror',e=>errors.push(e.message));
 page.on('console',msg=>{if(msg.type()==='error')errors.push(msg.text());if(consoleMessages.length<300)consoleMessages.push(msg.type()+': '+msg.text());});
 page.on('requestfailed',req=>{if(req.url().startsWith(url))errors.push(req.url()+': '+req.failure()?.errorText);});
 page.on('dialog',async dialog=>{errors.push(dialog.message());await dialog.dismiss();});
 await page.goto(url,{waitUntil:'domcontentloaded'});
 check(await page.title()==='The Blood Warfare','Título da página corresponde a The Blood Warfare');
 await page.waitForFunction(()=>window.gameReady===true,null,{timeout:300000});
 await page.waitForTimeout(3000);
 check(await page.locator('#loading').isHidden(),'Unity inicializa sem falha no navegador');
 await page.locator('canvas').click({position:{x:900,y:580}});await page.waitForTimeout(250);
 await page.waitForFunction(()=>window.__audioNodes.some(r=>r.started&&!r.stopped&&r.node.loop&&r.node.buffer?.duration>10&&r.context.state==='running'),null,{timeout:30000});
 const menuDuration=await page.evaluate(()=>window.__audioNodes.find(r=>r.started&&!r.stopped&&r.node.loop&&r.node.buffer)?.node.buffer.duration);
 check(menuDuration>10,'ComingHome_8bit está ativa no Web Audio do menu');
 let image=await shot(page,'menu');
 const menuArea={x:65,y:450,w:250,h:30}, resultArea={x:215,y:390,w:220,h:30};
 check(gold(image,menuArea)>3000,'Menu inicial foi renderizado pelo WebGL');
 await page.locator('canvas').click({position:{x:210,y:470}}); await page.waitForTimeout(400);
 image=await shot(page,'jogo-inicial'); let start=vampire(image);const guardsBefore=guards(image);
 let vialPixels=0;for(let y=110;y<520;y++)for(let x=40;x<920;x++){const i=(y*image.info.width+x)*4;if(Math.abs(image.data[i]-224)<6&&Math.abs(image.data[i+1]-69)<6&&Math.abs(image.data[i+2]-88)<6)vialPixels++;}
 check(vialPixels>150,'Frascos de sangue são renderizados no WebGL');
 check(start!==null&&Math.abs(start.x-480)<30,'Clique inicia a missão e renderiza o vampiro');
 let lifePixels=0;for(let y=554;y<588;y++)for(let x=70;x<220;x++){const i=(y*image.info.width+x)*4;if(image.data[i]>180&&image.data[i+1]<140&&image.data[i+2]<170)lifePixels++;}
 check(lifePixels>150,'Indicadores de vida são visíveis no HUD WebGL');
 await page.waitForFunction(()=>window.__audioNodes.some(r=>r.started&&!r.stopped&&r.node.loop&&r.node.buffer?.duration>10&&r.context.state==='running'),null,{timeout:30000});
 check(await page.evaluate(()=>window.__audioNodes.some(r=>r.started&&!r.stopped&&r.node.loop&&r.node.buffer?.duration>10&&r.context.state==='running')),'AceOfSpades_8bit em loop está ativa no Web Audio após interação');
 const gameDuration=await page.evaluate(()=>window.__audioNodes.find(r=>r.started&&!r.stopped&&r.node.loop&&r.node.buffer)?.node.buffer.duration);
 check(gameDuration>10&&Math.abs(gameDuration-menuDuration)>1,'Gameplay troca ComingHome_8bit por uma trilha distinta');
 await page.keyboard.down('d'); await page.waitForTimeout(450); await page.keyboard.up('d'); await page.waitForTimeout(80);
 image=await shot(page,'movimento');let moved=vampire(image);
 check(moved!==null&&moved.x>start.x+35,'Entrada de teclado move o jogador no WebGL');
 await page.waitForTimeout(250);const stoppedImage=await shot(page,'parado');let stopped=vampire(stoppedImage);
 const guardsAfter=guards(stoppedImage);check(guardsBefore.every(Boolean)&&guardsAfter.every(Boolean)&&guardsBefore.some((p,i)=>Math.hypot(guardsAfter[i].x-p.x,guardsAfter[i].y-p.y)>10),'Os dois templários aparecem e patrulham no WebGL');
 check(stopped!==null&&Math.abs(stopped.x-moved.x)<15,'Soltar a tecla interrompe o movimento');
 await page.keyboard.press('Escape'); await page.waitForTimeout(250);
 check(gold(await shot(page,'menu-retorno'),menuArea)>3000,'Escape retorna ao menu no navegador');
 await page.keyboard.press('Enter');await page.waitForTimeout(400);
 check(vampire(await shot(page,'reinicio-teclado'))!==null,'Enter inicia outra partida');
 await page.evaluate(()=>window.__connectPad());await page.waitForTimeout(300);
 const padStart=vampire(await shot(page,'controle-inicio'));
 await page.evaluate(()=>window.__virtualPad.axes[0]=1);await page.waitForTimeout(250);await page.evaluate(()=>window.__virtualPad.axes[0]=0);await page.waitForTimeout(100);
 const padRight=vampire(await shot(page,'controle-analogico'));
 check(padStart&&padRight&&padRight.x>padStart.x+25,'Analógico Xbox simulado pela Gamepad API move o jogador no WebGL');
 async function button(index,value){await page.evaluate(({index,value})=>Object.assign(window.__virtualPad.buttons[index],{pressed:value>0,touched:value>0,value}),{index,value});}
 await button(14,1);await page.waitForTimeout(250);await button(14,0);await page.waitForTimeout(100);
 const padLeft=vampire(await shot(page,'controle-direcional'));
 check(padLeft&&padLeft.x<padRight.x-25,'Direcional Xbox simulado move o jogador no WebGL');
 await button(5,1);await page.evaluate(()=>window.__virtualPad.axes[0]=1);await page.waitForTimeout(250);await page.evaluate(()=>window.__virtualPad.axes[0]=0);await button(5,0);await page.waitForTimeout(100);
 const padBoost=vampire(await shot(page,'controle-impulso'));
 check(padBoost&&padBoost.x-padLeft.x>(padRight.x-padStart.x)*1.2,'RB simulado aumenta o deslocamento no WebGL');
 await button(8,1);await page.waitForTimeout(150);await button(8,0);await page.waitForTimeout(150);
 check(gold(await shot(page,'controle-menu'),menuArea)>3000,'View simulado retorna ao menu no WebGL');
 await button(0,1);await page.waitForTimeout(150);await button(0,0);await page.waitForTimeout(350);
 check(vampire(await shot(page,'controle-confirmar'))!==null,'A simulado confirma o botão inicial pela UI do WebGL');
 await page.evaluate(()=>window.unityInstance.SendMessage('GameManager','AdvanceClock',100));await page.waitForTimeout(300);
 const resultShot=await shot(page,'derrota-tempo');
 check(gold(resultShot,resultArea)>3000,'Esgotar reserva de sangue abre a tela de resultado no WebGL');
 check(vampire(resultShot)===null,'Painel de resultado cobre os sprites e mantém o texto legível');
 await page.locator('canvas').click({position:{x:330,y:405}});await page.waitForTimeout(400);
 image=await shot(page,'reinicio-resultado');
 check(gold(image,resultArea)<1000&&vampire(image)!==null,'Botão do resultado reinicia a missão no navegador');
 await page.goto(url+'/__embed',{waitUntil:'domcontentloaded'});
 const embed=await page.locator('iframe').elementHandle();const frame=await embed.contentFrame();check(!!frame,'Página de teste incorpora o jogo em iframe');
 await frame.waitForFunction(()=>window.gameReady===true,null,{timeout:300000});await page.waitForTimeout(3000);
 check(gold(await shot(page,'iframe-menu'),menuArea)>3000,'Menu renderiza dentro de iframe semelhante ao itch.io');
 await frame.locator('canvas').click({position:{x:210,y:470}});await page.waitForTimeout(400);
 check(vampire(await shot(page,'iframe-jogo'))!==null,'Jogo inicia dentro do iframe');
 check(errors.length===0,'Nenhum erro de JavaScript, WebAssembly ou carregamento local durante a execução');
 await context.close();
})().catch(e=>{errors.push(e.stack||String(e));console.error(e);process.exitCode=1;}).finally(async()=>{
 if(browser)await browser.close();if(server)await new Promise(resolve=>server.close(resolve));
 fs.mkdirSync(output,{recursive:true});
 fs.writeFileSync(path.join(output,'validation.json'),JSON.stringify({title:'The Blood Warfare',success:errors.length===0,checks,errors,browser:'Chromium / Microsoft Edge headless, software WebGL',consoleMessages},null,2));
});







