using DevExpress.XtraEditors;
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

namespace WWEngineCC
{
    public partial class NewProj : DevExpress.XtraEditors.XtraForm
    {
        public string path;
        public string name;
        public bool data;
        public bool save;
        public NewProj()
        {
            InitializeComponent();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            XtraFolderBrowserDialog dlg = new XtraFolderBrowserDialog();
            dlg.DialogStyle = DevExpress.Utils.CommonDialogs.FolderBrowserDialogStyle.Wide;
            if (dlg.ShowDialog() == DialogResult.Cancel) return;
            textEdit2.Text = dlg.SelectedPath + '\\';
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if(textEdit1.Text==null)
            {
                errorProvider1.SetError(textEdit1, "项目名不应为空");
                return;
            }
            if(!Directory.Exists(textEdit2.Text))
            {
                errorProvider2.SetError(textEdit2, "项目路径不存在");
                return;
            }
            if (File.Exists(textEdit2.Text + "\\" + textEdit1.Text + "\\" + textEdit1.Text + ".WWproj")) 
            {
                errorProvider2.SetError(textEdit2, "已存在同名项目");
                return;
            }
            path = textEdit2.Text;
            name = textEdit1.Text;
            data = toggleSwitch2.IsOn;
            save = toggleSwitch1.IsOn;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}