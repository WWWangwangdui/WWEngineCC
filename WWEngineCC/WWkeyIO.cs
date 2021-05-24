using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WWEngineCC
{
    public static class WWkeyIO
    {
        public static Point P1;
        public static Point Mousemove;
        //按键按下状态数组
        public static bool[] WWarrDwon = new bool[260];
        //双击数组
        public static bool[] WWDCarr = new bool[260];
        //单击数组
        public static int[] WWSCarr = new int[260];
        //记录按下时间
        private static double[] WWTimeDown = new double[260];
        //记录抬起时间
        private static double[] WWTimeUp = new double[260];

        private static Point currentpoint;

        private static UserControl target;
        
        public static UserControl Target
        {
            get => target;
            set
            {
                target = value;
                target.KeyDown += WWdown;
                target.KeyUp += WWup;
                target.MouseDown += WWmouseDown;
                target.MouseUp += WWmouseUp;
                target.MouseMove += WWonMouseMove;
            }
        }

        private static void WWonMouseMove(object sender, MouseEventArgs e)
        {
            currentpoint = e.Location;
        }


        /// <summary>
        /// 设置按下数组，按下的那一位设为true
        /// </summary>
        /// <param name="sender"></param>>
        /// <param name="e">键盘事件</param>>
        public static void WWdown(object sender, KeyEventArgs e)
        {
            WWarrDwon[e.KeyValue] = true;
            WWTimeDown[e.KeyValue] = WWTime.now;
            if (WWTimeDown[e.KeyValue] - WWTimeUp[e.KeyValue] < 200)
            {
                WWDCarr[e.KeyValue] = true;
            }
        }
        /// <summary>
        /// 把抬起的键盘数组设为false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void WWup(object sender, KeyEventArgs e)
        {
            WWDCarr[e.KeyValue] = false;
            WWarrDwon[e.KeyValue] = false;
            WWTimeUp[e.KeyValue] = WWTime.now;
            if (WWTimeUp[e.KeyValue] - WWTimeDown[e.KeyValue] < 200)
            {
                WWSCarr[e.KeyValue]++;
                WWtimerUnit unit = new WWtimerUnit(WWonTimer, e.KeyValue.ToString(), 50.0);
                WWTime.WWaddTimer(unit);
            }
        }
        /// <summary>
        /// 检测按下的鼠标按键 把相应的设为true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">鼠标事件</param>
        public static void WWmouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WWarrDwon[1] = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                WWarrDwon[2] = true;
            }
            else
            {
                WWarrDwon[4] = true;
            }
        }
        /// <summary>
        /// 检测抬起的鼠标相应的WWarrDown设为false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void WWmouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WWarrDwon[1] = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                WWarrDwon[2] = false;
            }
            else
            {
                WWarrDwon[4] = false;
            }
        }

        /// <summary>
        /// 检测某个按键是否被按下
        /// </summary>
        /// <param name="k">按键键码参数</param>
        /// <returns>被按下或者没按下</returns>
        public static bool WWgetDown(Keys k)
        {
            return WWarrDwon[(int)k];
        }

        public static bool WWgetUp(Keys k)
        {
            return !WWarrDwon[(int)k];
        }
        public static bool WWgetClick(Keys k)
        {
            return WWSCarr[(int)k] > 0;
        }
        public static bool WWgetDClick(Keys k)
        {
            return WWDCarr[(int)k];
        }

        /// <summary>
        /// 更新当前鼠标位置和离上一次鼠标位置移动了多少
        /// </summary>
        /// <param Form ="f1">窗口句柄</param>
        public static void WWupdate()
        {
            Point pt = currentpoint;
            Mousemove.X = pt.X - P1.X;
            Mousemove.Y = pt.Y - P1.Y;
            P1 = pt;
        }
        /// <summary>
        /// 获得鼠标在上一帧和这一帧移动了多少
        /// </summary>
        /// <returns>Mousemove</returns>
        public static Point WWmouseMove => Mousemove;
        /// <summary>
        /// 得到鼠标当前位置
        /// </summary>
        /// <returns>鼠标当前位置</returns>
        public static Point WWmousePoint => P1;
        /// <summary>
        /// 把单击数组和双击数组重置
        /// </summary>
        /// <param name="k">按下的键位</param>
        /// <param name="clickTime">按下次数</param>
        public static void WWonTimer(string str)
        {
            int k = Convert.ToInt32(str);
            WWSCarr[k]--;
        }
    }
}