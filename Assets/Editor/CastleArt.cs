using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Original 32x32 pixel art with a restricted palette, point filtering and no antialiasing.</summary>
public static class CastleArt
{
    private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }
    public static Sprite Sprite(string name)
    {
        const int n = 32;
        var pixels = new Color[n * n];
        void Dot(int x, int y, string color) { if (x >= 0 && x < n && y >= 0 && y < n) pixels[y * n + x] = Hex(color); }
        void Box(int x, int y, int w, int h, string color) { for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) Dot(i, j, color); }
        void Disc(int cx, int cy, int radius, string color) { for (int y = 0; y < n; y++) for (int x = 0; x < n; x++) if ((x-cx)*(x-cx)+(y-cy)*(y-cy) <= radius*radius) Dot(x,y,color); }
        switch (name)
        {
            case "Vampire":
                // Cape, boots, waistcoat, pointed collar, pale face, widow's peak and fangs.
                for (int y = 5; y <= 23; y++) { int edge = 5 + (y-5)/4; Box(edge,y,33-2*edge,1,"110e1b"); Box(edge+2,y,29-2*edge,1,"67182f"); }
                Box(9,2,6,6,"100e18"); Box(19,2,6,6,"100e18");
                Box(10,6,14,3,"2c1a2b"); Box(12,8,10,12,"251b2c");
                Box(15,10,4,9,"d3b99b"); Box(16,9,2,7,"a62a42"); Box(12,8,10,2,"b98a54");
                Box(8,11,3,5,"f0e6d2"); Box(23,11,3,5,"f0e6d2");
                for (int k=0;k<5;k++){Box(7+k,21-k,1,k+1,"b52c48");Box(25-k,21-k,1,k+1,"b52c48");}
                Box(11,22,13,8,"100e18"); Box(12,21,11,7,"f0e6d2");
                Box(11,26,3,5,"100e18"); Box(21,26,3,5,"100e18"); Box(14,28,7,3,"100e18");
                Box(16,26,3,3,"100e18"); Dot(17,25,"100e18");
                Box(13,24,3,1,"c92646"); Box(19,24,3,1,"c92646");
                Dot(14,24,"f4ba89"); Dot(20,24,"f4ba89"); Dot(17,23,"c6a791");
                Box(14,21,7,1,"7f263b"); Dot(15,20,"fff6df"); Dot(20,20,"fff6df");
                break;
            case "Templar":
                Box(10,2,5,6,"14131d"); Box(19,2,5,6,"14131d");
                Box(10,6,14,15,"181722"); Box(12,7,10,13,"c0bbaf"); Box(20,7,2,13,"8c8790");
                Box(16,10,2,9,"b92c43"); Box(13,14,8,2,"b92c43");
                Box(11,20,13,10,"17151f"); Box(12,21,11,8,"85818b"); Box(13,26,9,2,"b9b3b2");
                Box(13,24,9,2,"27212d"); Box(17,21,2,7,"b9b3b2"); Box(17,24,2,2,"302733");
                Box(4,8,8,13,"17151f"); Box(5,9,6,11,"d3c6ad"); Box(6,7,4,3,"17151f");
                Box(7,11,2,8,"b92c43"); Box(5,15,6,2,"b92c43");
                Box(25,12,3,17,"17151f"); Box(26,14,1,14,"d1cbd0"); Dot(26,29,"d1cbd0");
                Box(24,11,5,2,"c7a46b"); Box(26,7,1,4,"745143");
                break;
            case "BloodVial":
                Box(12,25,9,5,"1a1420"); Box(13,26,7,3,"bc8e59"); Box(14,26,1,3,"e5c18c");
                Box(13,21,7,5,"a99aa7"); Box(15,22,3,4,"292331");
                for(int y=4;y<=22;y++){int inset=y>18?(y-18)/2:y<7?7-y:0;Box(8+inset,y,17-inset*2,1,"17131f");Box(10+inset,y,13-inset*2,1,"bba6ad");}
                Box(11,6,11,12,"8d1836"); Box(12,7,9,10,"c12848"); Box(11,17,11,2,"e04558");
                Box(11,9,2,7,"f17b7b"); Box(13,12,7,4,"d4bb97"); Box(16,12,2,3,"a01f3d"); Dot(16,15,"a01f3d");
                Box(11,20,3,2,"e4cfb7"); Dot(13,23,"e4cfb7");
                break;
            case "CastleFloor":
                Box(0,0,32,32,"211d29");
                for(int y=0;y<32;y++)for(int x=0;x<32;x++){
                    int row=y/8, offset=(row%2)*8, cell=(x+offset)/16;
                    Dot(x,y,(row+cell)%3==0?"292430":(row+cell)%3==1?"25212c":"302833");
                    if(y%8==0||(x+offset)%16==0)Dot(x,y,"15131e");
                    else if(y%8==1)Dot(x,y,"3d333e");
                }
                Box(4,5,4,1,"39303b"); Box(21,12,5,1,"1a1723"); Dot(12,26,"49404a"); Dot(26,29,"49404a");
                break;
            case "CastleWall":
                Box(0,0,32,32,"28202b");
                for(int row=0;row<4;row++)for(int col=-1;col<3;col++){
                    int x=col*16+(row%2)*8,y=row*8;
                    Box(x+1,y+1,14,6,(row+col)%2==0?"65545b":"51464f"); Box(x+1,y+6,14,1,"968077"); Box(x+2,y+2,1,3,"75616a");
                }
                Box(1,2,7,2,"3e4140"); Box(21,18,5,1,"332c38"); Dot(23,17,"332c38");
                break;
            case "Sarcophagus":
                Box(3,1,26,30,"14111d"); Box(5,3,22,27,"78616a"); Box(7,4,18,25,"413443");
                Box(9,6,14,21,"594653"); Box(11,8,10,17,"342a39"); Box(15,12,2,11,"a3846b"); Box(12,18,8,2,"a3846b");
                Box(6,27,20,2,"af9282"); Box(7,3,18,1,"2b2232");
                Dot(9,25,"d0b48a"); Dot(23,25,"d0b48a"); Dot(9,6,"d0b48a"); Dot(23,6,"d0b48a");
                break;
            case "BloodSigil":
                Disc(16,16,15,"2a1b2d"); Disc(16,16,13,"8b2943"); Disc(16,16,11,"302032"); Disc(16,16,7,"4b243a");
                Box(15,7,2,18,"b04455"); Box(7,15,18,2,"b04455"); Box(11,11,10,10,"302032");
                for(int y=10;y<23;y++){int w=y<16?y-9:23-y;Box(16-w/2,y,w,1,"a52343");}
                Dot(16,28,"dab987");Dot(4,16,"dab987");Dot(28,16,"dab987");Dot(16,4,"dab987");
                break;
            case "Torch":
                Box(13,3,7,17,"13111b"); Box(15,5,3,14,"87624e"); Box(11,13,12,3,"a98a61"); Box(12,11,10,2,"51404a");
                Disc(16,23,7,"651c32"); Box(12,19,9,9,"d64a40");Box(14,18,5,12,"f4a957");Box(16,21,3,8,"ffe1a0");Dot(15,30,"ffcb72");
                break;
            case "Banner":
                Box(5,27,23,3,"b29468"); Box(7,4,19,23,"251422"); Box(9,5,15,22,"831f3d"); Box(9,24,15,2,"bc4557");
                for(int x=9;x<24;x++)Box(x,3+Math.Abs(16-x)/3,1,4,"831f3d");
                Box(13,11,7,9,"d0ac76");Box(10,16,13,2,"d0ac76");Box(15,9,3,13,"d0ac76");Box(15,16,3,2,"831f3d");
                break;
            case "CastleArch":
                Box(2,0,6,23,"4a3a49");Box(24,0,6,23,"4a3a49");
                for(int y=19;y<31;y++){int left=2+(y-19)/2;Box(left,y,4,1,"78616c");Box(28-left,y,4,1,"78616c");}
                for(int y=2;y<23;y+=6){Box(2,y,6,1,"b29a87");Box(24,y,6,1,"b29a87");}
                Box(0,0,10,3,"8e7378");Box(22,0,10,3,"8e7378");Box(4,22,5,2,"b29a87");Box(23,22,5,2,"b29a87");
                break;
            case "Bat":
                for(int x=2;x<30;x++){int y=15+Math.Abs(16-x)/3;Box(x,y,1,3,"0c0b13");if(x%4<2)Box(x,y-2,1,3,"0c0b13");}
                Box(14,10,5,9,"0c0b13");Box(14,18,1,3,"0c0b13");Box(18,18,1,3,"0c0b13");Dot(15,16,"e65b6c");Dot(18,16,"e65b6c");
                break;
            case "BloodMoon":
                Disc(16,16,14,"491d33");Disc(16,17,12,"a33c4d");Disc(13,20,7,"c95a60");Disc(20,12,5,"782a42");Box(5,21,21,1,"b44b59");
                break;
            default: throw new ArgumentException("Unknown castle sprite: " + name);
        }
        var texture = new Texture2D(n,n,TextureFormat.RGBA32,false); texture.SetPixels(pixels);texture.Apply();
        string path="Assets/Art/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spritePixelsPerUnit=32;
        importer.spriteImportMode=SpriteImportMode.Single;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Point;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static void Audio()
    {
        WriteWav("Coleta",.22,t=>Math.Sin(2*Math.PI*(660*t+1100*t*t))*Math.Exp(-15*t)*Math.Sin(Math.PI*t/.22));
        WriteWav("Dano",.28,t=>(Math.Sin(2*Math.PI*(180*t-150*t*t))+.25*Math.Sin(2700*t)*Math.Sin(4011*t))*Math.Exp(-10*t));
        WriteWav("Vitoria",1,t=>{double f=t<.25?440:t<.5?523.25:659.25;return Math.Sin(2*Math.PI*f*t)*Math.Exp(-4*(t%.25))*(1-t);});
        WriteWav("Derrota",.8,t=>Math.Sin(2*Math.PI*(250*t-100*t*t))*Math.Exp(-3*t));
    }
    private static void WriteWav(string name,double duration,Func<double,double> waveform)
    {
        const int rate=22050;int count=(int)(duration*rate);string path="Assets/Audio/"+name+".wav";
        using(var w=new BinaryWriter(File.Create(path))){
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(count*2);
            for(int i=0;i<count;i++)w.Write((short)(Math.Max(-1,Math.Min(1,waveform((double)i/rate)))*20000));
        }
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
    }
}

