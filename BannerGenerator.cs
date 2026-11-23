using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// Original, code-drawn artwork. No external images, APIs, fonts or tracking.
public static class BannerGenerator {
    const int W=1200, H=400, Frames=48;
    static readonly Dictionary<int,byte> PixelCache=new Dictionary<int,byte>();
    sealed class ColorBox {
        public List<KeyValuePair<int,int>> Colors;
        public int Weight, Channel, Range;
        public ColorBox(List<KeyValuePair<int,int>> colors) {
            Colors=colors; Weight=colors.Sum(p=>p.Value);
            for(int channel=0;channel<3;channel++) {
                int shift=channel*8, min=255,max=0;
                foreach(var pair in colors) {int v=(pair.Key>>shift)&255;min=Math.Min(min,v);max=Math.Max(max,v);}
                if(max-min>Range) {Range=max-min;Channel=channel;}
            }
        }
        public Color Average() {
            long r=0,g=0,b=0;
            foreach(var p in Colors) {r+=((p.Key>>16)&255)*(long)p.Value;g+=((p.Key>>8)&255)*(long)p.Value;b+=(p.Key&255)*(long)p.Value;}
            return Color.FromArgb((int)(r/Weight),(int)(g/Weight),(int)(b/Weight));
        }
    }
    static byte[] Pixels(Bitmap image) {
        var data=image.LockBits(new Rectangle(0,0,image.Width,image.Height),ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
        var pixels=new byte[data.Stride*image.Height];Marshal.Copy(data.Scan0,pixels,0,pixels.Length);image.UnlockBits(data);return pixels;
    }
    static Color[] Palette(Bitmap image) {
        var histogram=new Dictionary<int,int>();var pixels=Pixels(image);
        for(int i=0;i<pixels.Length;i+=3) {int rgb=(pixels[i+2]<<16)|(pixels[i+1]<<8)|pixels[i];int count;histogram.TryGetValue(rgb,out count);histogram[rgb]=count+1;}
        var boxes=new List<ColorBox>{new ColorBox(histogram.ToList())};
        while(boxes.Count<256) {
            ColorBox box=boxes.Where(b=>b.Colors.Count>1).OrderByDescending(b=>b.Range*Math.Sqrt(b.Weight)).FirstOrDefault();
            if(box==null) break;
            int shift=box.Channel*8;
            box.Colors.Sort((a,b)=>((a.Key>>shift)&255).CompareTo((b.Key>>shift)&255));
            int sum=0,split=0;for(;split<box.Colors.Count-1;split++) {sum+=box.Colors[split].Value;if(sum>=box.Weight/2) {split++;break;}}
            split=Math.Max(1,Math.Min(split,box.Colors.Count-1));
            boxes.Remove(box);boxes.Add(new ColorBox(box.Colors.GetRange(0,split)));boxes.Add(new ColorBox(box.Colors.GetRange(split,box.Colors.Count-split)));
        }
        return boxes.Select(b=>b.Average()).ToArray();
    }
    static Bitmap Indexed(Bitmap image,Color[] palette) {
        var target=new Bitmap(image.Width,image.Height,PixelFormat.Format8bppIndexed);
        var targetPalette=target.Palette;for(int i=0;i<palette.Length;i++) targetPalette.Entries[i]=palette[i];target.Palette=targetPalette;
        var source=Pixels(image);int sourceStride=((image.Width*3+3)/4)*4;
        var data=target.LockBits(new Rectangle(0,0,image.Width,image.Height),ImageLockMode.WriteOnly,PixelFormat.Format8bppIndexed);
        var dest=new byte[data.Stride*image.Height];
        for(int y=0;y<image.Height;y++) for(int x=0;x<image.Width;x++) {
            int offset=y*sourceStride+x*3,b=source[offset],g=source[offset+1],r=source[offset+2];int rgb=(r<<16)|(g<<8)|b;byte index;
            if(!PixelCache.TryGetValue(rgb,out index)) {
                int best=int.MaxValue;
                for(int i=0;i<palette.Length;i++) {int dr=r-palette[i].R,dg=g-palette[i].G,db=b-palette[i].B;int distance=dr*dr+dg*dg+db*db;if(distance<best) {best=distance;index=(byte)i;}}
                PixelCache[rgb]=index;
            }
            dest[y*data.Stride+x]=index;
        }
        Marshal.Copy(dest,0,data.Scan0,dest.Length);target.UnlockBits(data);return target;
    }
    static Color C(string hex) { return ColorTranslator.FromHtml(hex); }
    static Brush B(string hex) { return new SolidBrush(C(hex)); }
    static Font F(float size, bool bold=false) { return new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel); }
    static void Text(Graphics g,string text,float x,float y,float size,string color,bool bold=false) {
        using(var f=F(size,bold)) using(var b=B(color)) g.DrawString(text,f,b,x,y,StringFormat.GenericTypographic);
    }
    static PointF Orbit(double angle) {
        double x=189*Math.Cos(angle),y=91*Math.Sin(angle),r=-0.27;
        return new PointF(944+(float)(x*Math.Cos(r)-y*Math.Sin(r)),200+(float)(x*Math.Sin(r)+y*Math.Cos(r)));
    }
    static void Pine(Graphics g,float x,float y,float h,string color) {
        using(var b=B(color)) {
            g.FillRectangle(b,x-1,y-h*.2f,2,h*.25f);
            g.FillPolygon(b,new[]{new PointF(x,y-h),new PointF(x-h*.31f,y-h*.22f),new PointF(x+h*.31f,y-h*.22f)});
            g.FillPolygon(b,new[]{new PointF(x,y-h*.77f),new PointF(x-h*.4f,y),new PointF(x+h*.4f,y)});
        }
    }
    static Bitmap Render(int frame) {
        var bmp=new Bitmap(W,H,PixelFormat.Format24bppRgb);
        using(var g=Graphics.FromImage(bmp)) {
            g.SmoothingMode=SmoothingMode.AntiAlias;
            g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using(var bg=new LinearGradientBrush(new Point(0,0),new Point(W,H),C("#0c1722"),C("#19352f"))) g.FillRectangle(bg,0,0,W,H);
            using(var p=new Pen(Color.FromArgb(28,117,155,146))) {
                for(int x=32;x<W;x+=48) g.DrawLine(p,x,0,x,H);
                for(int y=28;y<H;y+=48) g.DrawLine(p,0,y,W,y);
            }
            var random=new Random(22);
            for(int i=0;i<91;i++) {
                int x=random.Next(30,W-25),y=random.Next(24,H-25);
                if(x<710 && y>58 && y<305) continue;
                double a=(i*.73+(x>=740 && y>=35 && y<325 ? frame*Math.PI*2/Frames : 0));
                int alpha=(int)(75+70*(.5+.5*Math.Sin(a)));
                float radius=i%8==0?1.6f:.8f;
                using(var b=new SolidBrush(Color.FromArgb(alpha,211,227,217))) g.FillEllipse(b,x-radius,y-radius,radius*2,radius*2);
            }
            using(var p=new Pen(C("#34514e"))) g.DrawRectangle(p,19,19,W-39,H-39);
            using(var p=new Pen(C("#93aea1"),2)) {
                g.DrawLine(p,19,39,19,19);g.DrawLine(p,19,19,39,19);
                g.DrawLine(p,W-40,H-20,W-20,H-20);g.DrawLine(p,W-20,H-20,W-20,H-40);
            }
            Text(g,"H A L L E Y S 2 2   /   E X P L O R A T I O N   L O G",54,51,15,"#9eb9ad");
            Text(g,"Hernán.",50,109,91,"#f1eadb",true);
            using(var b=B("#e6a56d")) g.FillRectangle(b,56,220,56,3);
            Text(g,"Ideas con rumbo.",54,242,30,"#ecdfc9");
            Text(g,"Código en órbita.",54,280,30,"#9fbdb1");
            Text(g,"DAM  /  DESARROLLO  /  PRODUCTO",56,349,14,"#a1b6ae");
            // Orbital paths, a warm sun and a small forest world.
            using(var p=new Pen(C("#3b5a56"),1)) {
                var points=new List<PointF>();
                for(int i=0;i<=180;i++) points.Add(Orbit(i*Math.PI*2/180));
                g.DrawLines(p,points.ToArray());
            }
            for(int i=7;i>=1;i--) using(var b=new SolidBrush(Color.FromArgb(3+i,234,159,95))) g.FillEllipse(b,1007-i*5,82-i*5,64+i*10,64+i*10);
            using(var b=B("#e9ab73")) g.FillEllipse(b,1007,82,64,64);
            using(var b=B("#0e2025")) g.FillEllipse(b,848,122,174,174);
            using(var p=new Pen(C("#6b9282"),2)) g.DrawEllipse(p,848,122,174,174);
            var region=g.Save();
            using(var circle=new GraphicsPath()) {circle.AddEllipse(849,123,172,172);g.SetClip(circle);}
            using(var b=B("#274d43")) g.FillPolygon(b,new[]{new Point(846,237),new Point(879,201),new Point(902,219),new Point(935,178),new Point(969,212),new Point(1000,200),new Point(1030,249),new Point(1030,310),new Point(846,310)});
            using(var b=B("#41685a")) g.FillPolygon(b,new[]{new Point(846,254),new Point(890,225),new Point(927,248),new Point(976,216),new Point(1024,249),new Point(1024,310),new Point(846,310)});
            for(int i=0;i<9;i++) Pine(g,861+i*19,264+(float)Math.Sin(i)*8,24+i%3*7,"#102e2c");
            using(var b=B("#142e2e")) g.FillEllipse(b,831,266,215,100);
            g.Restore(region);
            using(var p=new Pen(Color.FromArgb(110,165,195,172),1)) g.DrawArc(p,857,131,155,155,200,102);
            // Campfire: a nod to exploration games, with no copied game assets.
            using(var p=new Pen(C("#c59168"),2)) {g.DrawLine(p,948,276,960,282);g.DrawLine(p,948,282,960,276);}
            using(var b=B("#e9ab73")) g.FillPolygon(b,new[]{new Point(954,263),new Point(949,276),new Point(959,276)});
            // Comet moves around the ellipse; an entire loop is seamless.
            double theta=frame*Math.PI*2/Frames-1.4;
            for(int i=23;i>=0;i--) {
                PointF pt=Orbit(theta-i*.017);
                int alpha=230-i*9;
                float rad=1.5f+(23-i)*.045f;
                using(var b=new SolidBrush(Color.FromArgb(alpha,235,179,124))) g.FillEllipse(b,pt.X-rad,pt.Y-rad,rad*2,rad*2);
            }
            PointF comet=Orbit(theta);
            using(var b=new SolidBrush(Color.FromArgb(25,246,209,154))) g.FillEllipse(b,comet.X-9,comet.Y-9,18,18);
            using(var b=B("#fff0cf")) g.FillEllipse(b,comet.X-3,comet.Y-3,6,6);
            Text(g,"S I E M P R E   P O R   D E S C U B R I R",799,349,12,"#a1b6ae");
        }
        return bmp;
    }
    // Each independently encoded GIF frame keeps its own palette, so merging
    // never changes colours or depends on an external encoder.
    static void AppendFrame(BinaryWriter output,byte[] gif,int left,int top) {
        int packed=gif[10], pos=13;
        byte[] palette=new byte[0];
        if((packed&128)!=0) {int n=3*(1<<((packed&7)+1)); palette=new byte[n];Array.Copy(gif,pos,palette,0,n);pos+=n;}
        while(pos<gif.Length) {
            int marker=gif[pos++];
            if(marker==0x21) {pos++;int n;while((n=gif[pos++])>0) pos+=n;continue;}
            if(marker==0x3b) break;
            if(marker!=0x2c) throw new InvalidDataException("Unexpected GIF block");
            var descriptor=new byte[9];Array.Copy(gif,pos,descriptor,0,9);pos+=9;
            descriptor[0]=(byte)(left&255); descriptor[1]=(byte)(left>>8);
            descriptor[2]=(byte)(top&255); descriptor[3]=(byte)(top>>8);
            int ip=descriptor[8];
            if((ip&128)!=0) {int n=3*(1<<((ip&7)+1));palette=new byte[n];Array.Copy(gif,pos,palette,0,n);pos+=n;}
            int bits=0;for(int colors=palette.Length/3;colors>2;colors>>=1) bits++;
            descriptor[8]=(byte)(128|(ip&64)|bits);
            output.Write(new byte[]{0x21,0xf9,4,4,14,0,0,0}); // 140 ms, keep the previous canvas.
            output.Write((byte)0x2c);output.Write(descriptor);output.Write(palette);
            output.Write(gif[pos++]);
            int length;do {length=gif[pos++];output.Write((byte)length);output.Write(gif,pos,length);pos+=length;} while(length!=0);
            return;
        }
        throw new InvalidDataException("No GIF image found");
    }
    public static void Build(string assets) {
        Directory.CreateDirectory(assets);
        PixelCache.Clear();Color[] palette;
        using(var first=Render(0)) palette=Palette(first);
        using(var stream=File.Create(Path.Combine(assets,"banner.gif"))) using(var writer=new BinaryWriter(stream)) {
            writer.Write(Encoding.ASCII.GetBytes("GIF89a"));writer.Write((ushort)W);writer.Write((ushort)H);writer.Write(new byte[]{0x70,0,0});
            writer.Write(new byte[]{0x21,0xff,11});writer.Write(Encoding.ASCII.GetBytes("NETSCAPE2.0"));writer.Write(new byte[]{3,1,0,0,0});
            for(int frame=0;frame<Frames;frame++) using(var image=Render(frame)) {
                if(frame==0) image.Save(Path.Combine(assets,"banner-static.png"),ImageFormat.Png);
                using(var buffer=new MemoryStream()) {
                    if(frame==0) using(var indexed=Indexed(image,palette)) {indexed.Save(buffer,ImageFormat.Gif);AppendFrame(writer,buffer.ToArray(),0,0);}
                    else using(var crop=image.Clone(new Rectangle(740,35,440,290),PixelFormat.Format24bppRgb)) {
                        using(var indexed=Indexed(crop,palette)) indexed.Save(buffer,ImageFormat.Gif);AppendFrame(writer,buffer.ToArray(),740,35);
                    }
                }
            }
            writer.Write((byte)0x3b);
        }
        Console.WriteLine("Built 1200x400 banner: 48 frames, 6.72 s loop.");
    }
}

