using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.XtraEditors;
using DevExpress.Utils;

namespace WWEngineCC
{
    public partial class Form1
    {
        void initcontrols()
        {
            SuperToolTip superToolTip = new SuperToolTip();
            ToolTipItem item1 = new ToolTipItem();
            item1.Text = "请先打开场景进行编辑";
            superToolTip.Items.Add(item1);
            barButtonItem8.SuperTip = superToolTip;
            var info = new ToolTipControlInfo();
            info.SuperTip = barButtonItem8.SuperTip;
            info.Object = barButtonItem8;
            barButtonItem8.Tag = info;
            WWassetView.WWinit(pictureEdit1);
            WWDirector.fileSystemWatcher1 = fileSystemWatcher1;
        }

        public void WWupdate()
        {
            FPS.Caption = "FPS: " + WWTime.FPS.ToString("0.000");
            if(WWDirector.WWScene!=null)
            {
                ltpos.Caption = "游戏窗口左上角世界坐标" + WWDirector.WWcamera.LeftTop.ToString();
                mousepos.Caption = "鼠标世界坐标" + new PointF(WWkeyIO.WWmousePoint.X - WWDirector.WWcamera.X, WWkeyIO.WWmousePoint.Y - WWDirector.WWcamera.Y);
            }
            WWassetView.WWupdate();
        }

        private bool filechanged = false;
        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            filechanged = true;
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            filechanged = true;
        }

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            filechanged = true;
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            filechanged = true;
        }
    }
}
