using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WWEngineCC
{
    public partial class SplitImage : DevExpress.XtraEditors.XtraForm
    {
        public System.Drawing.Bitmap bit;
        string basename;
        string path;
        public SplitImage(Image ima, string bs, string pa)
        {
            InitializeComponent();
            bit = new System.Drawing.Bitmap(ima);
            x.Properties.Minimum = 0;
            y.Properties.Minimum = 0;
            x.Properties.Maximum = bit.Width;
            y.Properties.Maximum = bit.Height;
            pictureEdit1.Image = bit;
            width.Properties.Minimum = 1;
            height.Properties.Minimum = 1;
            width.Properties.Maximum = bit.Width;
            height.Properties.Maximum = bit.Height;
            xedit.Text = "0";
            yedit.Text = "0";
            widthedit.Text = "1";
            heightedit.Text = "1";
            basename = bs;
            path = pa;
        }

        private void split()
        {
            if (y.Value == bit.Height || x.Value == bit.Width || height.Value <= 0 || width.Value <= 0) return;
            System.Drawing.Bitmap bitmap = bit.Clone(new RectangleF(x.Value, y.Value, width.Value, height.Value), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            nameedit.Text = basename + xedit.Text + "_" + yedit.Text + "_" + widthedit.Text + "_" + heightedit.Text;
            pictureEdit1.Image = bitmap;
        }

        private void x_EditValueChanged(object sender, EventArgs e)
        {
            xedit.Text = x.Value.ToString();
            width.Properties.Maximum = bit.Width - x.Value;
            widthedit.Text = width.Value.ToString();
            split();
        }

        private void y_EditValueChanged(object sender, EventArgs e)
        {
            yedit.Text = y.Value.ToString();
            height.Properties.Maximum = bit.Height - y.Value;
            heightedit.Text = height.Value.ToString();
            split();
        }

        private void width_EditValueChanged(object sender, EventArgs e)
        {
            widthedit.Text = width.Value.ToString();
            split();
        }

        private void height_EditValueChanged(object sender, EventArgs e)
        {
            heightedit.Text = height.Value.ToString();
            split();
        }

        private void xedit_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Convert.ToInt32(xedit.Text);
            }
            catch(Exception ex)
            {
                e.Cancel = true;
                xedit.ErrorText = ex.Message;
            }
        }

        private void yedit_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Convert.ToInt32(yedit.Text);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                yedit.ErrorText = ex.Message;
            }
        }


        private void xedit_Validated(object sender, EventArgs e)
        {
            int tmp = Convert.ToInt32(xedit.Text);
            if (x.Value != tmp) x.Value = tmp;
        }

        private void yedit_Validated(object sender, EventArgs e)
        {
            int tmp = Convert.ToInt32(yedit.Text);
            if (y.Value != tmp) y.Value = tmp;
        }

        private void widthedit_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Convert.ToInt32(widthedit.Text);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                widthedit.ErrorText = ex.Message;
            }
        }

        private void widthedit_Validated(object sender, EventArgs e)
        {
            int tmp = Convert.ToInt32(widthedit.Text);
            if (width.Value != tmp) width.Value = tmp;
        }

        private void heightedit_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Convert.ToInt32(heightedit.Text);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                heightedit.ErrorText = ex.Message;
            }
        }

        private void heightedit_Validated(object sender, EventArgs e)
        {
            int tmp = Convert.ToInt32(heightedit.Text);
            if (height.Value != tmp) height.Value = tmp;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            AsBitmap NEW = new AsBitmap();
            NEW.AssetName = nameedit.Text;
            NEW.Off = new PointF(x.Value, y.Value);
            NEW.Size = new SizeF(width.Value, height.Value);
            NEW.BitmapPath = path;
            NEW.AssetPath = NEW.AssetName + ".WWBitmap";
            NEW.Sourcesize = bit.Size;
            WWasCtrl.WWaddAsset(NEW);
            NEW.WWsaveAsset();
            XtraMessageBox.Show("拆分成功");
            if (WWasCtrl.WWCurPath == "Assets") WWasCtrl.WWaddToFolder(NEW);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}