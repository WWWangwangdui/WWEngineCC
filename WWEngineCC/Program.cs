using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WWEngineCC
{
    static class Program
    {
        static Form1 mainForm;
        public static bool playing = false;
        private static Thread timer = null;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            WWTime.init();
            timer = new Thread(new ThreadStart(WWTime.TimeCircle));
            timer.Start();
            WWPluginCC.WWinit();
            mainForm = new Form1();
            Application.Run(mainForm);
        }


        public static void update()
        {
            WWTime.WWupdate();
            WWkeyIO.WWupdate();
            if (playing)
            {
                try
                {
                    WWDirector.WWupdate();
                }
                catch (Exception ex)
                {
                    playing = false;
                    mainForm.ribbonPage1.Visible = true;
                    WWDirector.WWloadScene(mainForm.scenename);
                    MessageBox.Show(ex.Message);
                }
                WWRenderer.Render();
            }
            else
            {
                WWDirector.WWsleepUpdate();
                WWRenderer.Render();
            }
            if (mainForm != null) mainForm.WWupdate();
        }

        public static void onQuit()
        {
            timer.Abort();
        }
    }
}
