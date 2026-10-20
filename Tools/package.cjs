const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'..'),out=path.resolve(process.argv[2]||path.join(root,'../Entrega/TheBloodWarfare'));
const table=Array.from({length:256},(_,i)=>{let c=i;for(let k=0;k<8;k++)c=(c&1)?0xedb88320^(c>>>1):c>>>1;return c>>>0;});
function crc32(data){let c=0xffffffff;for(const b of data)c=table[(c^b)&255]^(c>>>8);return(c^0xffffffff)>>>0;}
function walk(dir,prefix=''){
 return fs.readdirSync(dir,{withFileTypes:true}).sort((a,b)=>a.name.localeCompare(b.name)).flatMap(e=>{
  if(e.name==='node_modules'||e.name==='.DS_Store')return[];
  const file=path.join(dir,e.name),name=prefix+e.name;
  return e.isDirectory()?walk(file,name+'/'):[{file,name}];
 });
}
function zip(target,files){
 const chunks=[],central=[];let offset=0;
 for(const {file,name} of files){
  const data=fs.readFileSync(file),compressed=zlib.deflateRawSync(data,{level:6}),filename=Buffer.from(name.replaceAll('\\','/'));
  const crc=crc32(data),d=fs.statSync(file).mtime,year=Math.max(1980,d.getFullYear());
  const time=(d.getHours()<<11)|(d.getMinutes()<<5)|(d.getSeconds()>>1),date=((year-1980)<<9)|((d.getMonth()+1)<<5)|d.getDate();
  const header=Buffer.alloc(30);header.writeUInt32LE(0x04034b50,0);header.writeUInt16LE(20,4);header.writeUInt16LE(0x800,6);header.writeUInt16LE(8,8);header.writeUInt16LE(time,10);header.writeUInt16LE(date,12);header.writeUInt32LE(crc,14);header.writeUInt32LE(compressed.length,18);header.writeUInt32LE(data.length,22);header.writeUInt16LE(filename.length,26);
  const entry=Buffer.alloc(46);entry.writeUInt32LE(0x02014b50,0);entry.writeUInt16LE(20,4);entry.writeUInt16LE(20,6);entry.writeUInt16LE(0x800,8);entry.writeUInt16LE(8,10);entry.writeUInt16LE(time,12);entry.writeUInt16LE(date,14);entry.writeUInt32LE(crc,16);entry.writeUInt32LE(compressed.length,20);entry.writeUInt32LE(data.length,24);entry.writeUInt16LE(filename.length,28);entry.writeUInt32LE(offset,42);
  chunks.push(header,filename,compressed);central.push(entry,filename);offset+=header.length+filename.length+compressed.length;
 }
 const directory=Buffer.concat(central),end=Buffer.alloc(22);end.writeUInt32LE(0x06054b50,0);end.writeUInt16LE(files.length,8);end.writeUInt16LE(files.length,10);end.writeUInt32LE(directory.length,12);end.writeUInt32LE(offset,16);
 fs.writeFileSync(target,Buffer.concat([...chunks,directory,end]));
 return {file:path.basename(target),files:files.length,bytes:fs.statSync(target).size,sha256:crypto.createHash('sha256').update(fs.readFileSync(target)).digest('hex')};
}
for(const report of ['Documentation/resgate-validation.json','Documentation/webgl-build.json','Documentation/Browser/validation.json']){
 const data=JSON.parse(fs.readFileSync(path.join(root,report),'utf8').replace(/^\uFEFF/,'')); if(!data.success||data.title!=='The Blood Warfare')throw new Error('Validação pendente ou de outra versão: '+report);
}
fs.mkdirSync(out,{recursive:true});
const build=walk(path.join(root,'Builds/WebGL'));if(!build.some(f=>f.name==='index.html')||!build.some(f=>f.name.endsWith('.wasm')))throw new Error('Build Unity WebGL incompleto');
const source=['Assets','Packages','ProjectSettings','Tools','Delivery'].flatMap(folder=>walk(path.join(root,folder),folder+'/'));
for(const file of ['README.md','CREDITS.md','.gitignore','.gitattributes'])source.push({file:path.join(root,file),name:file});
for(const folder of ['Previews','PressKit','Browser','AudioOriginals'])source.push(...walk(path.join(root,'Documentation',folder),'Documentation/'+folder+'/'));
for(const file of ['resgate-validation.json','webgl-build.json','audio-validation.json'])source.push({file:path.join(root,'Documentation',file),name:'Documentation/'+file});
const manifests=[zip(path.join(out,'TheBloodWarfare-WebGL.zip'),build),zip(path.join(out,'TheBloodWarfare-Source.zip'),source),zip(path.join(out,'TheBloodWarfare-PressKit.zip'),walk(path.join(root,'Documentation/PressKit')))];
fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify({title:'The Blood Warfare',author:'Mathias Zanotto',unity:'6000.6.6f1',createdAt:new Date().toISOString(),artifacts:manifests},null,2));
fs.copyFileSync(path.join(root,'Delivery/ITCH_IO.md'),path.join(out,'PUBLICAR-NO-ITCH.md'));
fs.copyFileSync(path.join(root,'Delivery/LEIA-ME.md'),path.join(out,'LEIA-ME.md'));
console.log(JSON.stringify(manifests,null,2));





