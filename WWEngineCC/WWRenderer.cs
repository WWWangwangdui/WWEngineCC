using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;

namespace WWEngineCC
{
    public static class WWRenderer
    {
        public static bool drawing = false;
        private static Color background = Color.Black;
        [DllImport("Renderer.dll", EntryPoint = "WWinit", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWinit();
        [DllImport("Renderer.dll", EntryPoint = "WWaddBitmap", CallingConvention = CallingConvention.Cdecl)]
        static extern private int addBitmap(string path);
        [DllImport("Renderer.dll", EntryPoint = "WWaddSolidBrush", CallingConvention = CallingConvention.Cdecl)]
        static extern public int WWaddSolidBrush(uint col);
        [DllImport("Renderer.dll", EntryPoint = "WWaddFormat", CallingConvention = CallingConvention.Cdecl)]
        static extern public int WWaddFormat(string name, float size);
        [DllImport("Renderer.dll", EntryPoint = "WWsetHwnd", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWsetHwnd(uint hwnd, int _width, int _height);
        [DllImport("Renderer.dll", EntryPoint = "WWsetSize", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWsetSize(int _width, int _height);
        [DllImport("Renderer.dll", EntryPoint = "WWstartDraw", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWstartDraw();
        [DllImport("Renderer.dll", EntryPoint = "WWWWstopDraw", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWstopDraw();
        [DllImport("Renderer.dll", EntryPoint = "WWdrawLine", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWdrawLine(float centerX, float centerY, float scaleX, float scaleY, float rotate, float pt1x, float pt1y, float pt2x, float pt2y, float strokewidth, int srcid);
        [DllImport("Renderer.dll", EntryPoint = "WWdrawCircle", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWdrawCircle(float centerX, float centerY, float scaleX, float scaleY, float rotate, float pt1x, float pt1y, float width, float height, float strokewide, int srcid);
        [DllImport("Renderer.dll", EntryPoint = "WWdrawRect", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWdrawRect(float centerX, float centerY, float scaleX, float scaleY, float rotate, float pt1x, float pt1y, float pt2x, float pt2y, float strokewide, int srcid);
        [DllImport("Renderer.dll", EntryPoint = "WWdrawBitmap", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWdrawBitmap(float centerX, float centerY, float scaleX, float scaleY, float rotate, float pt1x, float pt1y, float pt2x, float pt2y, float ptsrcx, float ptsrcy, float ptsizex, float ptsizey, int srcid, float capcity = 1.0f);
        [DllImport("Renderer.dll", EntryPoint = "WWdrawText", CallingConvention = CallingConvention.Cdecl)]
        static extern public void WWdrawText(float centerX, float centerY, float scaleX, float scaleY, float rotate, float pt1x, float pt1y, string text, int brushid, int fontid);
        [DllImport("Renderer.dll", EntryPoint = "WWbeginDraw", CallingConvention = CallingConvention.Cdecl)]
        static extern private void WWbeginDraw(int offx, int offy);
        [DllImport("Renderer.dll", EntryPoint = "WWendDraw", CallingConvention = CallingConvention.Cdecl)]
        static extern private bool WWendDraw();
        [DllImport("Renderer.dll", EntryPoint = "WWclearScreen", CallingConvention = CallingConvention.Cdecl)]
        static extern private void WWclearScreen(int col);
        private static List<RenderBase> renderlist = new List<RenderBase>();
        private static Dictionary<string, int> bitmaps = new Dictionary<string, int>();
        public static void WWaddRenderUnit(RenderBase render)
        {
            renderlist.Add(render);
        }
        public static int WWaddBitmap(string path)
        {
            if (!bitmaps.ContainsKey(path)) bitmaps.Add(path, addBitmap(path));
            return bitmaps[path];
        }
        public static void Render()
        {
            if (!drawing) return;
            WWbeginDraw(0, 0);
            WWclearScreen(background.ToArgb());
            renderlist.Sort();
            foreach (var item in renderlist)
            {
                item.WWrender();
            }
            WWendDraw();

            renderlist.Clear();
        }
    }

    public abstract class RenderBase
        : WWmoduleBase,IComparable
    {
        private int level = 0;
        private float centerX;
        private float centerY;
        private float scaleX = 1.0f;
        private float scaleY = 1.0f;
        private float rotate;

        [XmlIgnore()]
        public PointF GlobleLocation
        {
            get => parent.Transform.globleLocation;
        }


        [Category("图元基本信息")]
        [DisplayName("图元显示图层")]
        public int Level
        {
            get => level;
            set => level = value;
        }

        [Category("图元变换")]
        [DisplayName("变换中心横坐标")]
        public float CenterX
        {
            get { return centerX; }
            set { centerX = value; }
        }
        [Category("图元变换")]
        [DisplayName("变换中心纵坐标")]
        public float CenterY
        {
            get { return centerY; }
            set { centerY = value; }
        }
        [Category("图元变换")]
        [DisplayName("横坐标缩放")]
        public float ScaleX
        {
            get { return scaleX; }
            set { scaleX = value; }
        }
        [Category("图元变换")]
        [DisplayName("纵坐标缩放")]
        public float ScaleY
        {
            get { return scaleY; }
            set { scaleY = value; }
        }
        [Category("图元变换")]
        [DisplayName("旋转角度")]
        public float Rotate
        {
            get { return rotate; }
            set { rotate = ((value % 360) + 360) % 360; }
        }

        public abstract void WWrender();
        public virtual void Start()
        {

        }
        public virtual void Stop()
        {

        }
        public virtual void Flush()
        {

        }

        public int CompareTo(object obj)
        {
            int res = 0;
            RenderBase rand = obj as RenderBase;
            if (level < rand.level)
            {
                res = -1;
            }
            else if (level > rand.level)
            {
                res = 1;
            }
            else
            {
                if(GlobleLocation.Y<rand.GlobleLocation.Y)
                {
                    res = -1;
                }
                else if(GlobleLocation.Y>rand.GlobleLocation.Y)
                {
                    res = 1;
                }
                else
                {
                    res = GlobleLocation.X - rand.GlobleLocation.X < 0.0 ? -1 : 1;
                }
            }
            return res;
        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }
    }

    public class WWLine
        : RenderBase
    {
        private int srcid = -1;
        private PointF begin;
        [Category("图元属性")]
        [DisplayName("直线起点")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Begin
        {
            get
            {
                return begin;
            }
            set
            {
                begin = value;
            }
        }
        private PointF end;
        [Category("图元属性")]
        [DisplayName("直线终点")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF End
        {
            get
            {
                return end;
            }
            set
            {
                end = value;
            }
        }
        private Color col;
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("颜色")]
        public Color color
        {
            get
            {
                return col;
            }
            set
            {
                col = value;
                srcid = -1;
            }
        }
        [XmlElement("color")]
        public int BackColorAsArgb
        {
            get { return color.ToArgb(); }
            set { color = Color.FromArgb(value); }
        }
        private float strokewidth = 1.0f;
        [Category("图元属性")]
        [DisplayName("直线宽度")]
        public float StrokeWidth
        {
            get
            {
                return strokewidth;
            }
            set
            {
                strokewidth = value;
            }
        }

        public override void WWupdate()
        {
            if (Parent == null) return;
            if (Parent.Transform == null) return;
            WWRenderer.WWaddRenderUnit(this);
        }
        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;
            if (srcid == -1)
                srcid = WWRenderer.WWaddSolidBrush((uint)col.ToArgb());
            WWRenderer.WWdrawLine(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, begin.X+pt.X, pt.Y+begin.Y, pt.X+end.X, pt.Y+end.Y, strokewidth, srcid);
        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }
    }

    public class WWRectangle
        : RenderBase
    {
        private int srcid = -1;
        private PointF begin;
        [Category("图元属性")]
        [DisplayName("矩形左上角坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF LeftTop
        {
            get
            {
                return begin;
            }
            set
            {
                begin = value;
            }
        }
        private PointF end;
        [Category("图元属性")]
        [DisplayName("矩形右下角坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF RightBottom
        {
            get
            {
                return end;
            }
            set
            {
                end = value;
            }
        }
        private Color col;

        private bool fill = false;
        [Category("图元属性")]
        [DisplayName("矩形是否填充")]
        public bool Fill
        {
            get
            {
                return fill;
            }
            set
            {
                fill = value;
            }
        }
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("颜色")]
        public Color color
        {
            get
            {
                return col;
            }
            set
            {
                col = value;
                srcid = -1;
            }
        }
        [XmlElement("color")]
        public int BackColorAsArgb
        {
            get { return color.ToArgb(); }
            set { color = Color.FromArgb(value); }
        }
        private float strokewidth = 1.0f;
        [Category("图元属性")]
        [DisplayName("矩形边界宽度")]
        public float StrokeWidth
        {
            get
            {
                return strokewidth;
            }
            set
            {
                strokewidth = value;
            }
        }
        public override void WWupdate()
        {
            if (Parent.Transform == null) return;
            WWRenderer.WWaddRenderUnit(this);
        }
        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;
            if (srcid == -1)
                srcid = WWRenderer.WWaddSolidBrush((uint)col.ToArgb());
            if (!fill) WWRenderer.WWdrawRect(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, pt.X+begin.X, pt.Y+begin.Y, pt.X+end.X, pt.Y+end.Y, strokewidth, srcid);

        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }
    }
    public class WWEllipse
       : RenderBase
    {
        private int srcid = -1;
        private PointF center;
        [Category("图元属性")]
        [DisplayName("椭圆圆心坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
            }
        }
        private SizeF size;
        [Category("图元属性")]
        [DisplayName("矩形半轴长")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        private Color col;

        private bool fill = false;
        [Category("图元属性")]
        [DisplayName("椭圆是否填充")]
        public bool Fill
        {
            get
            {
                return fill;
            }
            set
            {
                fill = value;
            }
        }
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("颜色")]
        public Color color
        {
            get
            {
                return col;
            }
            set
            {
                col = value;
                srcid = -1;
            }
        }
        [XmlElement("color")]
        public int BackColorAsArgb
        {
            get { return color.ToArgb(); }
            set { color = Color.FromArgb(value); }
        }
        private float strokewidth = 1.0f;
        [Category("图元属性")]
        [DisplayName("椭圆边界宽度")]
        public float StrokeWidth
        {
            get
            {
                return strokewidth;
            }
            set
            {
                strokewidth = value;
            }
        }
        public override void WWupdate()
        {
            if (Parent.Transform == null) return;
            WWRenderer.WWaddRenderUnit(this);
        }
        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;
            if (srcid == -1)
                srcid = WWRenderer.WWaddSolidBrush((uint)col.ToArgb());
            if (!fill) WWRenderer.WWdrawCircle(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, center.X+pt.X, pt.Y+center.Y, size.Width, size.Height, strokewidth, srcid);

        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }

    }
    public class WWText
          : RenderBase
    {
        private int brushid = -1;
        private int fontid = -1;
        public string str;
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("显示文本")]
        public string Str
        {
            get => str;
            set => str = value;
        }
        public PointF leftTopPoint;
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("位置")]
        [TypeConverter(typeof(PointConverter))]
        public PointF LeftTopPoint
        {
            get => leftTopPoint;
            set => leftTopPoint = value;
        }
        private Color col;
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("颜色")]
        public Color color
        {
            get
            {
                return col;
            }
            set
            {
                col = value;
                brushid = -1;
            }
        }
        [XmlElement("color")]
        public int BackColorAsArgb
        {
            get { return color.ToArgb(); }
            set { color = Color.FromArgb(value); }
        }
        public string fontname = "宋体";
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("字体")]
        public string Fontname
        {
            get => fontname;
            set => fontname = value;
        }
        public float fontsize = 12;
        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("字体大小")]
        public float Fontsize
        {
            get => fontsize;
            set => fontsize = value;
        }
        public override void WWupdate()
        {
            if (Parent.Transform == null) return;
            if (str == null) return;
            WWRenderer.WWaddRenderUnit(this);
        }
        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;
            if (brushid == -1)
                brushid = WWRenderer.WWaddSolidBrush((uint)col.ToArgb());
            if (fontid == -1)
                fontid = WWRenderer.WWaddFormat(fontname, fontsize);
            WWRenderer.WWdrawText(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, pt.X+leftTopPoint.X, pt.Y+leftTopPoint.Y, str, brushid, fontid);

        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }

    }
    public class WWBitmap
      : RenderBase
    {
        private int srcid = -1;

        private int bitid = -1;

        [Category("图元属性")]
        [DisplayName("位图资源编号")]
        public int BitId
        {
            get
            {
                return bitid;
            }
            set
            {
                AsBitmap asset = WWasCtrl.WWgetAsset(value) as AsBitmap;
                if (asset == null) return;
                bitid = value;
                off = asset.Off;
                size = asset.Size;
                srcid = -1;
                bitname = asset.AssetName;
            }
        }
        private string bitname;

        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("位图资源名")]
        public string BitName
        {
            get => bitname;
            set
            {
                AsBitmap asset = WWasCtrl.WWgetAsset(value) as AsBitmap;
                if (asset == null) return;
                bitid = asset.AssetID;
                off = asset.Off;
                size = asset.Size;
                srcid = -1;
                bitname = value;
            }
        }


        private PointF leftTopPoint;

        [Category("图元属性")]
        [DisplayName("图元坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF LeftTop
        {
            get => leftTopPoint;
            set => leftTopPoint = value;
        }

        private PointF off;

        [Category("图元属性")]
        [DisplayName("位图偏移")]
        [TypeConverter(typeof(PointFConverter))]
        public virtual PointF OffSet
        {
            get => off;
            set => off = value;
        }

        private SizeF size;


        [Category("图元属性")]
        [DisplayName("位图大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                size.Width = Math.Max(0, size.Width);
                size.Height = Math.Max(0, size.Height);
            }
        }


        public override void WWupdate()
        {
            if (bitid == -1) return;
            if (Parent == null) return;
            if (Parent.Transform == null) return;
            WWRenderer.WWaddRenderUnit(this);
        }
        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;
            if (srcid == -1)
            {
                AsBitmap asset = WWasCtrl.WWgetAsset(bitid) as AsBitmap;
                if (asset != null)
                {
                    srcid = (int)asset.WWgetAsset();
                }
            }
            if (srcid == -1) return;
            WWRenderer.WWdrawBitmap(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, pt.X+LeftTop.X, pt.Y+ leftTopPoint.Y, pt.X + leftTopPoint.X + size.Width, pt.Y + LeftTop.Y + size.Height, OffSet.X, OffSet.Y, size.Width, size.Height, srcid);
        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }

    }

    public class WWAnimation
  : RenderBase
    {
        private int srcid = -1;

        private int bitid = -1;

        [Category("图元属性")]
        [DisplayName("动画资源编号")]
        public int BitId
        {
            get
            {
                return bitid;
            }
            set
            {
                AsAnimation asset = WWasCtrl.WWgetAsset(value) as AsAnimation;
                if (asset == null) return;
                srcid = -1;
                bitid = value;
                SrcSize = asset.Sourcesize;
                bitsize = asset.Size;
                framenum = asset.FrameNum;
                size = asset.FrameSize;
                constoff = asset.ConstOffSet;
                nxtframetime = (float)WWTime.now;
                framepersec = asset.FramePerSec;
                secperframe = 1000.0f / framepersec;
                bitname = asset.AssetName;
                off = constoff;
                curframe = 0;
            }
        }


        string bitname;

        [XmlIgnore()]
        [Category("图元属性")]
        [DisplayName("动画资源名")]
        public string BitName
        {
            get => bitname;
            set
            {
                AsAnimation asset = WWasCtrl.WWgetAsset(value) as AsAnimation;
                if (asset == null) return;
                srcid = -1;
                SrcSize = asset.Sourcesize;
                bitid = asset.AssetID;
                bitsize = asset.Size;
                framenum = asset.FrameNum;
                size = asset.FrameSize;
                constoff = asset.ConstOffSet;
                nxtframetime = (float)WWTime.now;
                framepersec = asset.FramePerSec;
                secperframe = 1000.0f / framepersec;
                bitname = value;
                off = constoff;
                curframe = 0;
            }
        }

        private SizeF bitsize;
        [Category("图元属性")]
        [DisplayName("动画位图大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF BitSize
        {
            get
            {
                return bitsize;
            }
        }

        [Category("图元属性")]
        [DisplayName("源位图大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF SrcSize
        {
            get;
            set;
        }

        private PointF leftTopPoint;

        [Category("图元属性")]
        [DisplayName("图元坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF LeftTop
        {
            get => leftTopPoint;
            set => leftTopPoint = value;
        }

        private PointF off;

        [Category("运行时属性")]
        [DisplayName("当前偏移")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF OffSet
        {
            get => off;
        }

        private PointF constoff;

        [Category("图元属性")]
        [DisplayName("动画起始偏移")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF ConstOffSet
        {
            get => constoff;
        }


        private SizeF size;
        [Category("图元属性")]
        [DisplayName("帧大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF Size
        {
            get
            {
                return size;
            }
        }

        private float nxtframetime = 0.0f;

        [XmlIgnore()]
        [Browsable(false)]
        public float NextFrameTime
        {
            get => nxtframetime;
            set => nxtframetime = value;
        }

        private float framepersec;

        [Category("图元属性")]
        [DisplayName("每秒帧数")]
        public float FramePerSec
        {
            get => framepersec;
        }

        private float secperframe = 0.0f;
        private bool enable = true;

        [Category("运行时属性")]
        [DisplayName("动画是否播放")]
        public bool Enable
        {
            get => enable;
            set => enable = value;
        }

        private int curframe;

        [Category("运行时属性")]
        [DisplayName("当前帧数")]
        public int CurFrame
        {
            get => curframe;
        }


        private int framenum;

        [Category("图元属性")]
        [DisplayName("总帧数")]
        public int FrameNum
        {
            get => framenum;
        }

        private float timescale = 1.0f;

        [Category("图元属性")]
        [DisplayName("播放速率")]
        public float Timescale
        {
            get => timescale;
            set => timescale = value;
        }

        public override void Start()
        {
            enable = true;
            nxtframetime = 0.0f;
        }
        public override void Stop()
        {
            enable = false;
        }
        public override void Flush()
        {
            off = constoff;
            nxtframetime = (float)WWTime.now;
            curframe = 0;
        }

        public override void WWupdate()
        {
            if (bitid == -1) return;
            if (Parent == null) return;
            if (Parent.Transform == null) return;
            if (nxtframetime == 0.0f) 
                nxtframetime = (float)WWTime.now + secperframe;
            if (enable)
            {
                if(WWTime.now>nxtframetime+secperframe)
                {
                    nxtframetime = (float)WWTime.now;
                }
                if (WWTime.now >= nxtframetime)
                {
                    curframe++;
                    if (curframe >= framenum)
                    {
                        curframe = 0;
                        off = constoff;
                    }
                    else
                    {
                        off.X += size.Width;
                        if (off.X + size.Width > SrcSize.Width)
                        {
                            off.X = 0;
                            off.Y += size.Height;
                        }
                    }
                    nxtframetime += secperframe / timescale ;
                }
            }
            if (off.X + size.Width <= SrcSize.Width && off.Y + size.Height <= SrcSize.Height)
                WWRenderer.WWaddRenderUnit(this);
        }

        public override void WWrender()
        {
            if (Parent.Transform == null) return;
            PointF pt = Parent.Transform.globleLocation;

            if (srcid == -1)
            {
                AsBitmap asset = WWasCtrl.WWgetAsset(bitid) as AsBitmap;
                if (asset != null)
                {
                    srcid = (int)asset.WWgetAsset();
                    bitsize = asset.Size;
                }
            }
            if (srcid == -1) return;
            WWRenderer.WWdrawBitmap(pt.X + CenterX, pt.Y + CenterY, ScaleX, ScaleY, Rotate, pt.X + LeftTop.X, pt.Y + leftTopPoint.Y, pt.X + leftTopPoint.X + +size.Width, pt.Y + LeftTop.Y + size.Height, OffSet.X, OffSet.Y, size.Width, size.Height, srcid);
        }
        public override void WWsleepUpdate()
        {
            WWupdate();
        }

    }

}
