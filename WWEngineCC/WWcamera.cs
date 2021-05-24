using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWEngineCC
{
    public class WWcamera
    {
        private PointF pt;
        public PointF LeftTop
        {
            get => pt;
            set => pt = value;
        }
        public float X
        {
            get => pt.X;
        }
        public float Y
        {
            get => pt.Y;
        }
        private bool followMouse = false;
        public bool WWfollowMouse
        {
            get => followMouse;
            set => followMouse = value;
        }
        private bool mousedown = false;
        public void WWupdate()
        {
            if (followMouse)
            {
                if (mousedown)
                {
                    if (!WWkeyIO.WWgetDown(System.Windows.Forms.Keys.LButton))
                    {
                        mousedown = false;
                    }
                    else
                    {
                        pt.X += WWkeyIO.WWmouseMove.X;
                        pt.Y += WWkeyIO.WWmouseMove.Y;
                    }
                }
                else
                {
                    if (WWkeyIO.WWgetDown(System.Windows.Forms.Keys.LButton))
                    {
                        mousedown = true;
                    }
                }
            }
        }
    }
}
