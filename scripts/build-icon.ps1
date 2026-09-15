param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Runtime,System.Private.Windows.GdiPlus,System.Private.Windows.Core -TypeDefinition @'
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
public static class IconBuilder {
 public static void Build(string dir) {
  Directory.CreateDirectory(dir);
  int[] sizes={16,20,24,32,40,48,64,128,256};
  var blobs=new byte[sizes.Length][];
  for(int i=0;i<=sizes.Length;i++) {
   int size=i==sizes.Length?1024:sizes[i];
   using(var bitmap=new Bitmap(size,size,PixelFormat.Format32bppArgb)) {
    using(var g=Graphics.FromImage(bitmap)) {
     g.Clear(Color.Transparent); g.SmoothingMode=SmoothingMode.AntiAlias;
     g.ScaleTransform(size/512f,size/512f);
     using(var shape=new GraphicsPath()) {
      shape.AddArc(6,6,176,176,180,90);shape.AddArc(330,6,176,176,270,90);
      shape.AddArc(330,330,176,176,0,90);shape.AddArc(6,330,176,176,90,90);shape.CloseFigure();
      using(var fill=new SolidBrush(Color.FromArgb(40,40,40)))g.FillPath(fill,shape);
      using(var pen=new Pen(Color.FromArgb(210,210,210),Math.Max(7,512f/size))) {
       pen.StartCap=pen.EndCap=LineCap.Round;pen.LineJoin=LineJoin.Round;
       g.DrawPath(pen,shape);
       using(var arrow=new GraphicsPath()) {
        arrow.AddLine(112,360,221,360);arrow.AddBezier(221,360,229,360,230,357,230,349);
        arrow.AddLine(230,349,230,239);arrow.AddBezier(230,239,230,217,242,203,265,203);
        arrow.AddLine(265,203,398,203);g.DrawPath(pen,arrow);
       }
       g.DrawLines(pen,new PointF[]{new PointF(345,150),new PointF(398,203),new PointF(345,256)});
      }
     }
    }
    if(i==sizes.Length)bitmap.Save(Path.Combine(dir,"App.png"),ImageFormat.Png);
    else using(var stream=new MemoryStream()){bitmap.Save(stream,ImageFormat.Png);blobs[i]=stream.ToArray();}
   }
  }
  using(var writer=new BinaryWriter(File.Create(Path.Combine(dir,"App.ico")))) {
   writer.Write((short)0);writer.Write((short)1);writer.Write((short)sizes.Length);
   int offset=6+16*sizes.Length;
   for(int i=0;i<sizes.Length;i++){writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((short)0);writer.Write((short)1);writer.Write((short)32);writer.Write(blobs[i].Length);writer.Write(offset);offset+=blobs[i].Length;}
   foreach(var blob in blobs)writer.Write(blob);
  }
 }
}
'@
[IconBuilder]::Build($OutputDirectory)


