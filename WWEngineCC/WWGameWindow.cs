using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WWEngineCC
{
    public partial class WWGameWindow : UserControl
    {
        private static bool Lock = false; 
        public WWGameWindow()
        {
            InitializeComponent();
            WWkeyIO.Target = this;
        }

        private void WWGameWindow_Load(object sender, EventArgs e)
        {

        }

        private void WWGameWindow_Paint(object sender, PaintEventArgs e)
        {
            if(!Lock)
            {
                Lock = true;
                WWRenderer.WWsetHwnd((uint)this.Handle, this.Size.Width, this.Size.Height);
                WWRenderer.WWstartDraw();
                WWRenderer.drawing = true;
            }
        }

        private void WWGameWindow_Resize(object sender, EventArgs e)
        {
            WWRenderer.WWsetSize(this.Width, this.Height);
        }

    }
}
