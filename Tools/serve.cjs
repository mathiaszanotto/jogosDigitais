const http=require('node:http'),fs=require('node:fs'),path=require('node:path');
const root=path.resolve(process.argv[2]||path.join(__dirname,'../Builds/WebGL'));
const port=Number(process.env.PORT||8080);
const mime={'.html':'text/html; charset=utf-8','.js':'text/javascript','.wasm':'application/wasm','.data':'application/octet-stream','.png':'image/png','.css':'text/css'};
if(!fs.existsSync(path.join(root,'index.html')))throw new Error('Pasta sem index.html: '+root);
http.createServer((req,res)=>{
 let pathname;try{pathname=decodeURIComponent(new URL(req.url,'http://localhost').pathname);}catch{res.writeHead(400);return res.end();}
 const file=path.resolve(root,'.'+(pathname==='/'?'/index.html':pathname));
 if(!file.startsWith(root+path.sep)){res.writeHead(403);return res.end();}
 fs.stat(file,(err,stat)=>{if(err||!stat.isFile()){res.writeHead(404);return res.end();}res.setHeader('Content-Type',mime[path.extname(file)]||'application/octet-stream');fs.createReadStream(file).pipe(res);});
}).listen(port,'127.0.0.1',()=>console.log('The Game: http://127.0.0.1:'+port+' (Ctrl+C para encerrar)'));
