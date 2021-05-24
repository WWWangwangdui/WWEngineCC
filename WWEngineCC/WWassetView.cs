using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WWEngineCC.Properties;

namespace WWEngineCC
{
    static class WWassetView
    {
        
        internal static WWassetsType type;
        public static System.Drawing.Bitmap obj = null;
        private static double nxtframetime;
        private static double secperframe;
        private static int framenum;
        private static Size size;
        private static Point off;
        private static int curframe;
        private static PictureEdit edit = null;
        private static Image[] images;
        public static void WWinit(PictureEdit _edit)
        {
            edit = _edit;
        }
        public static void WWupdate()
        {
            if(type==WWassetsType.Animation)
            {
                if (WWTime.now > nxtframetime + secperframe)
                {
                    nxtframetime = (float)WWTime.now;
                }
                if (WWTime.now >= nxtframetime)
                {
                    curframe++;
                    if (curframe >= framenum)
                    {
                        curframe = 0;
                    }
                    nxtframetime += secperframe;
                    showImage(images[curframe]);
                }
            }
            if(type==WWassetsType.Bitmap)
            {
                showImage(obj);
            }
        }
        public static void WWsetImage(Image ima)
        {
            if (edit == null) return;
            obj = new System.Drawing.Bitmap(ima);
            showImage(obj);
            type = WWassetsType.Bitmap;
        }
        public static void WWsetAnimation(Image ima, SizeF _size, int _framenum, int framepersec)
        {
            if (edit == null) return;
            obj = new System.Drawing.Bitmap(ima);
            nxtframetime = WWTime.now;
            secperframe = 1000.0 / framepersec;
            framenum = _framenum;
            size = new Size((int)_size.Width,(int)_size.Height);
            off = new Point();
            curframe = 0;
            type = WWassetsType.Animation;
            images = new Image[framenum];
            try
            {
                for (int i = 0; i < framenum; i++)
                {
                    images[i] = obj.Clone(new System.Drawing.Rectangle(off, size), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    off.X += size.Width;
                    if (off.X + size.Width > obj.Size.Width)
                    {
                        off.X = 0;
                        off.Y += size.Height;
                    }
                }
            }
            catch
            {
                showImage(Resources.错误);
            }
        }
        delegate void DelShow(Image Msg); //代理
                                           //将对控件的操作写到一个函数中
        private static void showImage(Image ima)
        {
            if (!edit.InvokeRequired) //不需要唤醒，就是创建控件的线程
                                          //如果是创建控件的线程，直接正常操作
            {
                edit.Image = ima;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(showImage);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                edit.Invoke(ds, new object[] { ima });
            }
        }
        public static void flush()
        {
            curframe = 0;
            off = new Point();
        }
    }
}
