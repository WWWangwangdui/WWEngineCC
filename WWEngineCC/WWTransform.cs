using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace WWEngineCC
{
    public class WWTransform
        :WWmoduleBase
    {
        private PointF pos;
        private float scalex;
        private float scaley;
        private float rotate;
        private PointF gpt;
        [Category("运行时变换")]
        [DisplayName("世界坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF globleLocation
        {
            get
            {
                return gpt;
            }
        }
        [Category("基本变换")]
        [DisplayName("水平缩放")]
        public float scaleX
        {
            get
            {
                return scalex;
            }
            set
            {
                scalex = value;
            }
        }
        [Category("基本变换")]
        [DisplayName("垂直缩放")]
        public float scaleY
        {
            get
            {
                return scaley;
            }
            set
            {
                scaley = value;
            }
        }
        [Category("基本变换")]
        [DisplayName("旋转")]
        public float rotation
        {
            get
            {
                return rotate;
            }
            set
            {
                rotate = value;
            }
        }

        [Category("组件基本属性")]
        [DisplayName("是否活动")]
        public override bool activity
        {
            get
            {
                return act;
            }
        }
        [Category("基本变换")]
        [DisplayName("坐标")]
        [TypeConverter(typeof(PointFConverter))]
        public PointF location
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value; 
            }
        }
        public override void WWupdate()
        {
            gpt = pos;
            if (Parent == null) return;
            WWobject fa = Parent.Parent;
            if (fa != null) 
            {
                WWTransform transform = fa.Transform;
                gpt.X += transform.globleLocation.X;
                gpt.Y += transform.globleLocation.Y;
            }
            else
            {
                gpt.X += WWDirector.WWcamera.X;
                gpt.Y += WWDirector.WWcamera.Y;
            }
        }
        public override void WWsleepUpdate()
        {
            gpt = pos;
            if (Parent == null) return;
            WWobject fa = Parent.Parent;
            if (fa != null)
            {
                WWTransform transform = fa.Transform;
                gpt.X += transform.globleLocation.X;
                gpt.Y += transform.globleLocation.Y;
            }
            else
            {
                gpt.X += WWDirector.WWcamera.X;
                gpt.Y += WWDirector.WWcamera.Y;
            }
        }
    }
}
