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
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraVerticalGrid.Rows;
using System.Reflection;
using WWEngineCC.Properties;

namespace WWEngineCC
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private object Propertyitem = null;
        public string scenename;
        public Form1()
        {
            InitializeComponent();
            // Handling the QueryControl event that will populate all automatically generated Documents
            this.MainDoumentManager.QueryControl += MainDoumentManager_QueryControl;
            initcontrols();
            WWPluginCC.RegistMod(AddMod, barListItem2);
            WWRenderer.WWinit();
            WWasCtrl.WWinit(treeListAssets, Gallery);
            WWDirector.WWinit(treeListObject, proper, toolTipController1);
        }

        void MainDoumentManager_QueryControl(object sender, DevExpress.XtraBars.Docking2010.Views.QueryControlEventArgs e)
        {
            if (e.Document == WWGameWindowDocument)
                e.Control = new WWEngineCC.WWGameWindow();
            if (e.Control == null)
                e.Control = new System.Windows.Forms.Control();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string path = "";
            string scene = "";
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini"))
            {
                using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini"))
                {
                    path = reader.ReadLine();
                    scene = reader.ReadLine();
                    reader.Close();
                }
            }
            if(File.Exists(path))
            {
                wait.ShowWaitForm();
                wait.SetWaitFormCaption("正在恢复上次启动项目");
                try
                {
                    WWDirector.WWloadProj(path);
                    if (scene != null && scene != "") 
                        WWDirector.WWloadScene(scene);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("项目存在错误，未正常加载\n" + ex.Message);
                    WWDirector.WWcloseProj();
                }
                wait.CloseWaitForm();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.onQuit();
        }

        private void dockPanel5_Click(object sender, EventArgs e)
        {

        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!WWDirector.WWquerySaveProj()) return;
            NewProj dlg = new NewProj();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            string tmp = WWDirector.WWnewProj(dlg.path, dlg.name, dlg.data, dlg.save);
            if (tmp != "")
            {
                XtraMessageBox.Show(tmp);
            }
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!WWDirector.WWquerySaveProj()) return;
            treeListObject.ClearNodes();
            WWDirector.WWcloseProj();
            fileSystemWatcher1.EnableRaisingEvents = false;
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!WWDirector.WWquerySaveProj()) return;
            Opendlg.Filter = "项目文件|*.WWproj";
            if (Opendlg.ShowDialog() != DialogResult.OK) return;
            WWDirector.WWloadProj(Opendlg.FileName);
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            WWDirector.WWsaveProj();
            toolTipController1.ShowHint("保存成功");
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            WWDirector.WWnewScene();
        }

        private void treeListObject_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Program.playing) return;
            var node = treeListObject.FocusedNode;
            if (node == null) return;
            var row = treeListObject.GetFocusedDataRow();
            if (Convert.ToInt32(row.Field<string>("ImageIndex")) == (int)WWassetsType.Scene)
            {
                WWDirector.WWloadScene(Convert.ToInt32(row.Field<string>("Data")));
            }
        }


        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (row == null) return;
            WWassetsType type = (WWassetsType)Convert.ToInt32(row.Field<string>("ImageIndex"));
            WWobject obj;
            if (type == WWassetsType.Scene)
            {
                obj = WWDirector.WWaddObj("");
                if (obj == null)
                {
                    toolTipController1.ShowHint(barButtonItem8.Tag as ToolTipControlInfo);
                }
                else
                {
                    obj.WWfillTree(WWDirector.sceneTreeId);
                    treeListObject.FocusedNode = treeListObject.FindNodeByKeyID("o" + obj.ID.ToString());
                    barButtonItem9_ItemClick(null, null);
                }
            }
            else if (type == WWassetsType.Object)
            {
                int id = Convert.ToInt32(row.Field<string>("Data"));
                WWobject tmp = WWDirector.WWgetObj(id);
                obj = tmp.WWaddObj();
                if (obj == null)
                {
                    toolTipController1.ShowHint(barButtonItem8.Tag as ToolTipControlInfo);
                }
                else
                {
                    obj.WWfillTree(tmp.DataId);
                    treeListObject.FocusedNode = treeListObject.FindNodeByKeyID(obj.DataId);
                    barButtonItem9_ItemClick(null, null);
                }
            }
        }

        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (Convert.ToInt32(row.Field<string>("ImageIndex")) == (int)WWassetsType.Object)
            {
                treeListObject.OptionsBehavior.Editable = true;
                treeListObject.ShowEditor();
            }
        }

        private void treeListObject_HiddenEditor(object sender, EventArgs e)
        {
            treeListObject.OptionsBehavior.Editable = false;
            DataRow row = treeListObject.GetFocusedDataRow();
            if (Convert.ToInt32(row.Field<string>("ImageIndex")) == (int)WWassetsType.Object)
                WWDirector.WWreName(Convert.ToInt32(row.Field<string>("Data")), row.Field<string>("Text"));
        }

        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (Convert.ToInt32(row.Field<string>("ImageIndex")) == (int)WWassetsType.Object)
            {
                if (WWDirector.WWdelObj(Convert.ToInt32(row.Field<string>("Data"))))
                {
                    TreeListNode node = treeListObject.FocusedNode;
                    node.ParentNode.Nodes.Remove(node);
                }
                else
                {
                    XtraMessageBox.Show("对象删除失败");
                }
            }
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            WWDirector.WWquerySaveScene();
            WWDirector.WWcloseScene();
        }

        private void treeListObject_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (WWasCtrl.propertylock) return;
            DataRow row = treeListObject.GetFocusedDataRow();
            if (row == null) return;
            WWassetsType type = (WWassetsType)Convert.ToInt32(row.Field<string>("ImageIndex"));
            int id = Convert.ToInt32(row.Field<string>("Data"));
            switch (type)
            {
                case WWassetsType.Project: proper.SelectedObject = WWDirector.WWProject; break;
                case WWassetsType.Scene: proper.SelectedObject = WWDirector.WWScene; break;
                case WWassetsType.Object: proper.SelectedObject = WWDirector.WWgetObj(id); break;
                case WWassetsType.Module: proper.SelectedObject = WWDirector.WWgetMod(id).__get__this__property__(); break;
                default:
                    break;
            }
        }

        private void barListItem2_ListItemClick(object sender, DevExpress.XtraBars.ListItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (row == null) return;
            if ((WWassetsType)Convert.ToInt32(row.Field<string>("ImageIndex")) != WWassetsType.Object)
            {
                toolTipController1.ShowHint("游戏组件需要附加在游戏对象上");
                return;
            }
            int id = Convert.ToInt32(row.Field<string>("Data"));
            WWobject obj = WWDirector.WWgetObj(id);
            if (obj == null)
            {
                toolTipController1.ShowHint("游戏对象不存在");
                return;
            }
            obj.WWaddMod(AddMod.Strings[e.Index]);
        }



        private void treeListAssets_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            DataRow row = treeListAssets.GetFocusedDataRow();
            if (row == null) return;
            string tmp = row.Field<string>("ID");
            if (Directory.Exists(tmp))
                WWasCtrl.WWopenFolder(tmp);
        }

        private void barButtonItem13_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (WWDirector.WWProject == null)
            {
                toolTipController1.ShowHint("请打开项目后导入图片");
                return;
            }
            xtraOpenFileDialog1.Filter = "图片文件|*.png;*.jpg;*.bmp;*.jpeg";
            if (xtraOpenFileDialog1.ShowDialog() != DialogResult.OK) return;
            string res = WWasCtrl.WWimportBitmap(xtraOpenFileDialog1.FileName);
            if (res != null)
            {
                XtraMessageBox.Show(res);
            }
            else
            {
                
            }
        }

        private void Gallery_Gallery_ItemClick(object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {

        }

        private void Gallery_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (WWDirector.WWProject == null)
            {
                toolTipController1.ShowHint("请打开项目后导入图片");
                return;
            }
            xtraOpenFileDialog1.Filter = "图片文件|*.png;*.jpg;*.bmp;*.jpeg";
            if (xtraOpenFileDialog1.ShowDialog() != DialogResult.OK) return;
            string res = WWasCtrl.WWimportAnimation(xtraOpenFileDialog1.FileName);
            if (res != null)
            {
                XtraMessageBox.Show(res);
            }
            else
            {

            }
        }

        private GalleryItem hoveritem = null;

        private void Gallery_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var it = hoveritem;
                if (it == null) return;
                var item = WWasCtrl.WWgetAsset(it.Description);
                if (item == null) return;
                if (item.GetType() == typeof(AsBitmap))
                {
                    AsImagePop.ShowPopup(Control.MousePosition);
                }
                else if(item.GetType()==typeof(AsAnimation))
                {
                    AsAniPop.ShowPopup(Control.MousePosition);
                }
            }
        }

        private void barButtonItem17_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var it = hoveritem;
            AsBitmap wWassetBase = WWasCtrl.WWgetAsset(it.Description) as AsBitmap;
            SplitImage dlg = new SplitImage(Image.FromFile((wWassetBase).GlobleBitmapPath), wWassetBase.AssetName, wWassetBase.BitmapPath);
            dlg.ShowDialog();
        }

        private void Gallery_Gallery_GalleryItemHover(object sender, DevExpress.XtraBars.Ribbon.GalleryItemEventArgs e)
        {
            hoveritem = e.Item;
        }

        private void barListItem2_ListItemClick_1(object sender, DevExpress.XtraBars.ListItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (row == null) return;
            if ((WWassetsType)Convert.ToInt32(row.Field<string>("ImageIndex")) != WWassetsType.Object)
            {
                toolTipController1.ShowHint("游戏组件需要附加在游戏对象上");
                return;
            }
            int id = Convert.ToInt32(row.Field<string>("Data"));
            WWobject obj = WWDirector.WWgetObj(id);
            if (obj == null)
            {
                toolTipController1.ShowHint("游戏对象不存在");
                return;
            }
            obj.WWaddScript(barListItem2.Strings[e.Index]);
        }

        private void barButtonItem18_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataRow row = treeListObject.GetFocusedDataRow();
            if (Convert.ToInt32(row.Field<string>("ImageIndex")) == (int)WWassetsType.Module)
            {
                WWmoduleBase mod = WWDirector.WWgetMod(Convert.ToInt32(row.Field<string>("Data")));
                if(mod is WWScript)
                {
                    WWScript script = mod as WWScript;
                    if (File.Exists(script.GlobalPath)) File.Delete(script.GlobalPath);
                }
                if (WWDirector.WWdelMod(Convert.ToInt32(row.Field<string>("Data"))))
                {
                    TreeListNode node = treeListObject.FocusedNode;
                    node.ParentNode.Nodes.Remove(node);
                }
                else
                {
                    XtraMessageBox.Show("对象删除失败");
                }
            }
        }

        private void Gallery_DragDrop(object sender, DragEventArgs e)
        {

        }


        private void proper_MouseMove(object sender, MouseEventArgs e)
        {
            if (Propertyitem != null)
            {
                BaseRow row = proper.CalcHitInfo(e.Location).Row;
                if (row == null) return;
            }
        }

        private void proper_MouseLeave(object sender, EventArgs e)
        {
            Propertyitem = null;
        }

        private void proper_DragEnter(object sender, DragEventArgs e)
        {
        }

        private void proper_DragDrop(object sender, DragEventArgs e)
        {
            Point pt = proper.PointToClient(new Point(e.X, e.Y));
            BaseRow row = proper.CalcHitInfo(pt).Row;
            if (e.Data.GetDataPresent(typeof(AsBitmap)))
            {
                ((WWBitmap)proper.SelectedObject).BitId = ((AsBitmap)e.Data.GetData(typeof(AsBitmap))).AssetID;
                proper.UpdateData();
            }
            else if (e.Data.GetDataPresent(typeof(AsAnimation)))
            {
                ((WWAnimation)proper.SelectedObject).BitId = ((AsAnimation)e.Data.GetData(typeof(AsAnimation))).AssetID;
                proper.UpdateData();
            }
            //else if (e.Data.GetDataPresent(typeof(WWobject)))
            //{
            //    WWobject obj = (WWobject)e.Data.GetData(typeof(WWobject));
            //    if (obj.ConstName == null)
            //    {
            //        obj.WWregistConstName();
            //    }
            //    ((WWparam)row.Properties.Value).CstName = obj.ConstName;
            //}
            //else if (e.Data.GetDataPresent(typeof(WWmoduleBase)))
            //{
            //    WWmoduleBase mod = (WWmoduleBase)e.Data.GetData(typeof(WWmoduleBase));
            //    if (mod.ConstName == null)
            //    {
            //        mod.WWregistConstName();
            //    }
            //    ((WWparam)row.Properties.Value).CstName = mod.ConstName;
            //}
        }

        private void proper_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            Point pt = proper.PointToClient(new Point(e.X, e.Y));
            BaseRow row = proper.CalcHitInfo(pt).Row;
            if (e.Data.GetDataPresent(typeof(AsBitmap)))
            {
                if (proper.SelectedObject.GetType() == typeof(WWBitmap))
                {
                    e.Effect = DragDropEffects.All;
                }
            }
            else if (e.Data.GetDataPresent(typeof(AsAnimation)))
            {
                if (proper.SelectedObject.GetType() == typeof(WWAnimation))
                {
                    e.Effect = DragDropEffects.All;
                }
            }
            else if (e.Data.GetDataPresent(typeof(TreeListNode)))
            {
                DataRow Row = treeListObject.GetFocusedDataRow();
                if (Row == null) return;
                WWassetsType tp = (WWassetsType)Convert.ToInt32(Row.Field<string>("ImageIndex"));
                if (tp == WWassetsType.Object)
                {
                    WWobject obj = WWDirector.WWgetObj(Convert.ToInt32(Row.Field<string>("Data")));
                    DoDragDrop(obj, DragDropEffects.All);
                }
                else if (tp == WWassetsType.Module)
                {
                    WWmoduleBase obj = WWDirector.WWgetMod(Convert.ToInt32(Row.Field<string>("Data")));
                    DoDragDrop(obj, DragDropEffects.All);
                }
            }
            //else if (e.Data.GetDataPresent(typeof(WWobject)))
            //{
            //    if (row != null)
            //    {
            //        if (row.Properties != null && row.Properties.RowType == typeof(WWparam))
            //        {
            //            e.Effect = DragDropEffects.All;
            //        }
            //    }
            //}
            //else if (e.Data.GetDataPresent(typeof(WWmoduleBase)))
            //{
            //    if (row != null && row.Properties != null && row.Properties.RowType == typeof(WWparam))
            //    {
            //        e.Effect = DragDropEffects.All;
            //    }
            //}
        }

        private void Gallery_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {


        }

        private void Gallery_Gallery_DragItemStart(object sender, DevExpress.XtraBars.Ribbon.Gallery.GalleryDragItemStartEventArgs e)
        {
            e.Cancel = true;
            GalleryItem item = hoveritem;
            if (item == null) return;
            WWassetBase asset = WWasCtrl.WWgetAsset(item.Description);
            if (asset == null) return;
            if (asset.GetType() == typeof(AsBitmap) || asset.GetType() == typeof(AsAnimation))
            {
                DoDragDrop(asset, DragDropEffects.All);
            }
        }

        private void Gallery_Gallery_ItemDoubleClick(object sender, GalleryItemClickEventArgs e)
        {
            if (WWasCtrl.propertylock) return;
            if (e.Item.Tag != null)
            {
                object obj = e.Item.Tag;
                proper.SelectedObject = obj;
                if(obj.GetType()==typeof(AsBitmap))
                {
                    WWassetView.WWsetImage(((AsBitmap)obj).WWgetImage());
                }
                else if(obj.GetType()==typeof(AsAnimation))
                {
                    AsAnimation ani = obj as AsAnimation;
                    WWassetView.WWsetAnimation(((AsAnimation)obj).WWgetImage(), ani.FrameSize, ani.FrameNum, (int)ani.FramePerSec);
                }
            }
        }

        private void treeListObject_BeforeDropNode(object sender, DevExpress.XtraTreeList.BeforeDropNodeEventArgs e)
        {

        }

        private void treeListObject_DragObjectStart(object sender, DevExpress.XtraTreeList.DragObjectStartEventArgs e)
        {

        }

        private void treeListObject_DragLeave(object sender, EventArgs e)
        {

        }

        private void propertyunlock_Click(object sender, EventArgs e)
        {
            propertyunlock.Visible = false;
            propertylock.Visible = true;
            WWasCtrl.propertylock = false;
        }

        private void propertylock_Click(object sender, EventArgs e)
        {
            propertyunlock.Visible = true;
            propertylock.Visible = false;
            WWasCtrl.propertylock = true;
        }

        private void barButtonItem19_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (WWDirector.WWProject == null) return;
            if (WWDirector.WWScene != null)
                scenename = WWDirector.WWScene.name;
            if (WWDirector.WWProject.WWDefaultSceneName != null)
            {
                WWDirector.WWloadScene(WWDirector.WWProject.WWDefaultSceneName);
            }
            else
            {
                if (WWDirector.WWScene == null) return;
                WWDirector.WWloadScene(WWDirector.WWScene.name);
            }
            ribbonPage1.Visible = false;
            Program.playing = true;
        }

        private void barButtonItem20_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (WWDirector.WWScene == null) return;
            ribbonPage1.Visible = true;
            Program.playing = false;
            WWDirector.WWloadScene(scenename);
        }

        private void barButtonItem23_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var it = hoveritem;
            AsBitmap wWassetBase = WWasCtrl.WWgetAsset(it.Description) as AsBitmap;
            AsAnimation NEW = new AsAnimation();
            NEW.AssetName = wWassetBase.AssetName + "_ani";
            NEW.AssetPath = NEW.AssetName + ".WWAnimation";
            NEW.ConstOffSet = wWassetBase.Off;
            NEW.Size = wWassetBase.Size;
            NEW.BitmapPath = wWassetBase.BitmapPath;
            NEW.Sourcesize = wWassetBase.Sourcesize;
            WWasCtrl.WWaddAsset(NEW);
            NEW.WWsaveAsset();
            if (WWasCtrl.WWCurPath == "Assets") WWasCtrl.WWaddToFolder(NEW);
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
           
        }

        private void barButtonItem24_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (hoveritem == null) return;
            WWasCtrl.WWremoverAsset(hoveritem.Description);
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            WWDirector.WWsaveScene();
        }
        private bool changed = false;
        private void filechange(object sender, FileSystemEventArgs e)
        {
            changed = true;
        }

        private void ribbonControl1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (changed)
            {
                try
                {
                    wait.ShowWaitForm();
                    wait.SetWaitFormCaption("正在重新加载插件");
                    string path = null;
                    if (WWDirector.WWScene != null)
                    {
                        path = WWDirector.WWScene.name;
                    }
                    WWDirector.WWcloseScene();
                    WWPluginCC.WWloadPlugins();
                    if (path != null)
                        WWDirector.WWloadScene(path);
                    changed = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    wait.CloseWaitForm();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(WWDirector.WWProject!=null)
            {
                WWDirector.WWsaveProj();
            }
        }

        private void proper_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {

        }

        private void proper_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            if (proper.SelectedObject.GetType() == typeof(AsBitmap)) 
            {
                AsBitmap bit = proper.SelectedObject as AsBitmap;
                bit.WWsaveAsset();
                WWassetView.WWsetImage(bit.WWgetImage());
            }
            else if(proper.SelectedObject.GetType() == typeof(AsAnimation))
            {
                AsAnimation ani = proper.SelectedObject as AsAnimation;
                ani.WWsaveAsset();
                WWassetView.WWsetAnimation(ani.WWgetImage(), ani.FrameSize, ani.FrameNum, (int)ani.FramePerSec);
            }
        }
    }
}

