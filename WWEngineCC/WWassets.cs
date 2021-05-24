using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WWEngineCC
{
    public abstract class WWassetBase
    {
        private int ID = -1;
        [Category("资源基本属性")]
        [DisplayName("资源编号")]
        public int AssetID
        {
            get
            {
                return ID;
            }
            set
            {
                if (ID == -1) ID = value;
            }
        }
        private string name;
        [Category("资源基本属性")]
        [DisplayName("资源名")]
        public string AssetName
        {
            get
            {
                return name;
            }
            set
            {
                string tmp = value;
                int cnt = 1;
                while (!WWasCtrl.WWassetRename(name, tmp, this))
                {
                    tmp = value + '(' + (cnt++).ToString() + ')';
                }
                WWasCtrl.WWfolderReload(name, tmp);
                if(File.Exists(GlobleAssetPath))
                {
                    File.Move(GlobleAssetPath, WWDirector.WWProject.AssetsPath + tmp + Path.GetExtension(path));
                }
                path = tmp + Path.GetExtension(path);
                name = tmp;
            }
        }
        protected string path;
        [Category("资源基本属性")]
        [DisplayName("资源地址")]
        [ReadOnly(true)]
        public virtual string AssetPath
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }
        [Category("资源基本属性")]
        [DisplayName("资源绝对地址")]
        public virtual string GlobleAssetPath
        {
            get
            {
                return WWDirector.WWProject.AssetsPath + path;
            }
        }
        public abstract object WWgetAsset();
        public abstract Image WWgetImage();
        public abstract void WWsaveAsset();
    }
    public class AsBitmap
        : WWassetBase
    {

        [Category("资源基本属性")]
        [DisplayName("源位图大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF Sourcesize
        {
            get;
            set;
        }
        protected SizeF size;
        [Category("资源基本属性")]
        [DisplayName("位图大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF Size
        {
            get => size;
            set => size = value;
        }

        private PointF off;

        [Category("资源基本属性")]
        [DisplayName("位图偏移量")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF Off
        {
            get => off;
            set => off = value;
        }

        protected int srcid = -1;
        [Category("运行时属性")]
        [DisplayName("资源号")]
        public int __NotUsed_src_Id__
        {
            get
            {
                return srcid;
            }
        }
        [Browsable(false)]
        public int SrcID
        {
            get
            {
                if (srcid == -1)
                {
                    srcid = WWRenderer.WWaddBitmap(GlobleBitmapPath);
                }
                return srcid;
            }
        }

        protected string Bitpath;

        public string GlobleBitmapPath
        {
            get => WWDirector.WWProject.AssetsPath + Bitpath;
        }

        public string BitmapPath
        {
            get => Bitpath;
            set => Bitpath = value;
        }

        public override object WWgetAsset()
        {
            return SrcID;
        }
        public override Image WWgetImage()
        {
            Image ima = WWasCtrl.__get__key__image(GlobleBitmapPath);
            if (ima == null)
            {
                if (File.Exists(GlobleBitmapPath))
                {
                    ima = Image.FromFile(GlobleBitmapPath);
                    WWasCtrl.__add__key__image__(GlobleBitmapPath, ima);
                }
            }
            if (ima != null)
            {
                if (Size != ima.Size)
                {
                    var templateImage = new System.Drawing.Bitmap(ima);
                    ima = templateImage.Clone(new RectangleF(off, Size), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
            }
            return ima;
        }

        public override void WWsaveAsset()
        {
            XmlSerializer xml = new XmlSerializer(typeof(AsBitmap));
            using(FileStream fs=File.Create(GlobleAssetPath))
            {
                xml.Serialize(fs, this);
                fs.Close();
            }
        }
    }
    public class AsAnimation
        : AsBitmap
    {
        private PointF constoff;
        [Category("动画属性")]
        [DisplayName("动画起始偏移")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF ConstOffSet
        {
            get => constoff;
            set
            {
                constoff = value;
            }
        }


        private SizeF framesize;
        [Category("动画属性")]
        [DisplayName("帧大小")]
        [TypeConverter(typeof(SizeFConverter))]
        public SizeF FrameSize
        {
            get
            {
                return framesize;
            }
            set
            {
                framesize = value;
                framesize.Width = Math.Max(0, framesize.Width);
                framesize.Height = Math.Max(0, framesize.Height);
            }
        }

        private float framepersec = 1;

        [Category("动画属性")]
        [DisplayName("每秒帧数")]
        public float FramePerSec
        {
            get => framepersec;
            set
            {
                framepersec = value;
            }
        }
        private int framenum = 1;

        [Category("动画属性")]
        [DisplayName("总帧数")]
        public int FrameNum
        {
            get => framenum;
            set
            {
                framenum = Math.Min(Math.Max(value, 1), (int)size.Width);
                if (framesize.Equals(SizeF.Empty) || framesize.Equals(size)) 
                {
                    framesize = new SizeF(size.Width / framenum, size.Height);
                }
            }
        }
        public override Image WWgetImage()
        {
            Image ima = WWasCtrl.__get__key__image(GlobleBitmapPath);
            if (ima == null)
            {
                if (File.Exists(GlobleBitmapPath))
                {
                    ima = Image.FromFile(GlobleBitmapPath);
                    WWasCtrl.__add__key__image__(GlobleBitmapPath, ima);
                }
            }
            if (ima != null)
            {
                if (Size != ima.Size)
                {
                    var templateImage = new System.Drawing.Bitmap(ima);
                    ima = templateImage.Clone(new RectangleF(constoff, Size), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
            }
            return ima;
        }
        public override void WWsaveAsset()
        {
            XmlSerializer xml = new XmlSerializer(typeof(AsAnimation));
            using (FileStream fs = File.Create(GlobleAssetPath))
            {
                xml.Serialize(fs, this);
                fs.Close();
            }
        }
    }
}
