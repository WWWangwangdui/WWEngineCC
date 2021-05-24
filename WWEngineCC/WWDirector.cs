using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using System.Data;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.Utils;
using WWEngineCC.Properties;

namespace WWEngineCC
{
    static public class WWDirector
    {
        public static FileSystemWatcher fileSystemWatcher1;
        private static TreeListNode curScene = null;
        public static TreeList tree;
        public static PropertyGridControl proper;
        private static WWscene scene = null;
        private static WWproj proj = null;
        private static Dictionary<int, WWobject> keyobjects = new Dictionary<int, WWobject>();
        private static Dictionary<int, WWmoduleBase> keymodules = new Dictionary<int, WWmoduleBase>();
        public static DataTable treeData = new DataTable();
        public static ToolTipController tipController = null;
        public static Dictionary<string, object> globleParams = new Dictionary<string, object>();
        public static WWproj WWProject
        {
            get
            {
                return proj;
            }
        }
        public static WWscene WWScene
        {
            get
            {
                return scene;
            }
        }
        public static string sceneTreeId
        {
            get
            {
                if (scene == null) return null;
                return scene.TreeNodeId;
            }
        }
        public static TreeListNode SceneNode
        {
            get
            {
                return curScene;
            }
        }
        public static void WWinit(TreeList _tree, PropertyGridControl _proper, ToolTipController _tip)
        {
            tree = _tree;
            proper = _proper;
            tipController = _tip;
            treeData.Columns.Add("ID");
            treeData.Columns.Add("ParentID");
            treeData.Columns.Add("Text");
            treeData.Columns.Add("ImageIndex");
            treeData.Columns.Add("Data");
            treeData.PrimaryKey = new DataColumn[] { treeData.Columns[0] };
            tree.DataSource = treeData;
        }
        public static void WWaddRow(string ID, string ParentID, string Text, WWassetsType type, string Data = "")
        {
            if (!treeData.Rows.Contains(ParentID)) return;
            DataRow row = treeData.NewRow();
            row.SetField(0, ID);
            row.SetField(1, ParentID);
            row.SetField(2, Text);
            row.SetField(3, (int)type);
            row.SetField(4, Data);
            try
            {
                treeData.Rows.Add(row);
            }
            catch
            {

            }
        }
        public static int objRegist(WWobject obj)
        {
            if (scene == null) return -1;
            int id = scene.nxtObjId;
            scene.addObjId();
            keyobjects.Add(id, obj);
            return id;
        }
        public static int modRegist(WWmoduleBase mod)
        {
            if (scene == null) return -1;
            int id = scene.nxtModId;
            scene.addModId();
            keymodules.Add(id, mod);
            return id;
        }
        public static WWobject WWaddObj(string name = "")
        {
            if (scene == null) return null;
            return scene.WWaddObj(name);
        }
        public static bool WWquerySaveProj()
        {
            DialogResult res = XtraMessageBox.Show("是否保存项目？", "保存项目", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
            if (res == DialogResult.Yes)
            {
                WWsaveProj();
                return true;
            }
            else if (res == DialogResult.No)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void WWsaveProj()
        {
            if (scene != null)
            {
                WWscene.WWsaveScene(scene);
            }
            if (proj != null)
            {
                WWproj.WWsaveProj(proj);
                WWasCtrl.WWsaveAssets();
            }
        }

        public static void WWsaveScene()
        {
            if (scene != null)
            {
                WWscene.WWsaveScene(scene);
            }
        }
        public static bool WWquerySaveScene()
        {
            if (scene != null)
            {
                DialogResult res = XtraMessageBox.Show("是否保存场景？", "保存场景", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (res == DialogResult.Yes)
                {
                    WWsaveScene();
                    return true;
                }
                else if (res == DialogResult.No)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static string WWnewProj(string path, string name, bool data, bool save)
        {
            try
            {
                WWproj NEW = new WWproj();
                NEW.name = name;
                NEW.path = path + name + "\\";
                if (!Directory.Exists(NEW.path)) Directory.CreateDirectory(NEW.path);
                Directory.CreateDirectory(NEW.path + "Scenes");
                Directory.CreateDirectory(NEW.path + "Assets");
                Directory.CreateDirectory(NEW.path + "Bin");
                if (data) Directory.CreateDirectory(NEW.path + "Data");
                if (save) Directory.CreateDirectory(NEW.path + "Save");
                File.Create(NEW.ProjPath).Close();
                WWproj.WWsaveProj(NEW);
                WWloadProj(NEW.ProjPath);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static void WWloadProj(string path)
        {
            try
            {
                if (proj != null)
                {
                    WWquerySaveProj();
                    WWcloseProj();
                }
                Program.playing = false;
                proj = WWproj.WWloadProj(path);
                if (Directory.Exists(WWDirector.WWProject.BinPath + "temp"))
                {
                    Directory.Delete(WWProject.BinPath + "temp", true);
                }
                WWPluginCC.WWloadPlugins();
                proj.WWfillTree(tree);
                tree.ExpandAll();
                WWasCtrl.WWfillTree(proj.path);
                WWasCtrl.WWloadAssets();
                if (!WWasCtrl.propertylock)
                    proper.SelectedObject = proj;
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory+"lastpath.ini"))
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini").Close();
                using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini"))
                {
                    writer.WriteLine(path);
                    writer.Close();
                }
                fileSystemWatcher1.Path = WWDirector.WWProject.BinPath;
                fileSystemWatcher1.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {

                XtraMessageBox.Show(ex.Message);
            }
        }
        public static void WWcloseProj()
        {
            if (proj == null) return;
            Program.playing = false;
            proj = null;
            WWcloseScene();
            WWasCtrl.WWcloseAssets();
            WWPluginCC.WWunloadPlugins();
            keyobjects.Clear();
            keymodules.Clear();
        }
        static string randerbasestr = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" + "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";//复杂字符
        static Random randstrrandom = new Random();
        static string randerbasebasestr = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string getRandStr()
        {
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                SB.Append(randerbasestr.Substring(randstrrandom.Next(0, randerbasestr.Length), 1));
            }
            return SB.ToString();
        }
        public static string getRandBaseStr()
        {
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                SB.Append(randerbasebasestr.Substring(randstrrandom.Next(0, randerbasebasestr.Length), 1));
            }
            return SB.ToString();
        }
        public static string WWaddGlobleParam(object obj)
        {
            string name = getRandStr();
            while(!WWaddGlobleParam(name,obj))
            {
                name = getRandStr();
            }
            return name;
        }

        public static bool WWdelGlobleParam(string name)
        {
            if(globleParams.ContainsKey(name))
            {
                globleParams.Remove(name);
                return true;
            }
            return false;
        }

        public static bool WWaddGlobleParam(string name, object obj)
        {
            if (name == null) return false;
            if (globleParams.ContainsKey(name)) return false;
            globleParams.Add(name, obj);
            return true;
        }
        public static WWcamera WWcamera
        {
            get
            {
                if (scene == null) return null;
                return scene.WWcamera;
            }
        }

        public static string Curname { get; private set; }

        public static object WWgetGlobleParam(string name)
        {
            if (globleParams.ContainsKey(name)) return globleParams[name];
            return null;
        }

        public static void WWcloseScene()
        {
            proper.SelectedObject = null;
            scene = null;
            keyobjects.Clear();
            keymodules.Clear();
            globleParams.Clear();
            tree.BeginUpdate();
            treeData.Rows.Clear();
            if (proj != null)
            {
                proj.WWfillTree(tree);
                foreach (TreeListNode item in tree.Nodes)
                {
                    item.Expand();
                }
            }
            tree.EndUpdate();
        }

        public static void WWupdate()
        {
            if (scene == null) return;
            scene.WWupdate();
        }

        public static void WWsleepUpdate()
        {
            if (scene == null) return;
            scene.WWsleepUpdate();
        }

        public static void WWnewScene()
        {
            if (proj == null)
            {
                XtraMessageBox.Show("请打开项目再创建场景");
                return;
            }
            if (!WWquerySaveScene()) return;
            WWcloseScene();
            string name = XtraInputBox.Show("场景名", "新建场景", proj.Recommended_name);
            if (name == string.Empty) return;
            while (!proj.WWcheckName(name))
            {
                name = XtraInputBox.Show("场景名", "新建场景", proj.Recommended_name);
                if (name == string.Empty) return;
            }
            proj.WWaddScene(name);
            DataRow row = (tree.DataSource as DataTable).NewRow();
            row.SetField(0, "s" + name);
            row.SetField(1, "root");
            row.SetField(2, name);
            row.SetField(3, (int)WWassetsType.Scene);
            row.SetField(4, proj.SceneCount - 1);
            (tree.DataSource as DataTable).Rows.Add(row);
        }

        delegate void LoadSceneInvokeS(string Msg);

        public static void WWloadScene(string name)
        {
            try
            {
                if (proper.InvokeRequired)
                {
                    LoadSceneInvokeS ds = new LoadSceneInvokeS(WWloadScene);
                    proper.Invoke(ds, new object[] { name });
                    return;
                }
                WWcloseScene();
                WWscene NEW = WWscene.WWloadScene(proj.ScenesPath + name + ".WWscene");
                scene = NEW;
                foreach (var item in scene.Objects)
                {
                    item.WWregist();
                    item.WWload();
                }
                tree.BeginUpdate();
                NEW.WWinit();
                NEW.WWfillTree();
                curScene = WWgetNode(NEW.name);
                if (!curScene.Expanded) curScene.Expand();
                tree.FocusedNode = curScene;
                if (!WWasCtrl.propertylock)
                    proper.SelectedObject = scene;
                tree.EndUpdate();
                Curname = NEW.name;
                using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini"))
                {
                    writer.WriteLine(proj.ProjPath);
                    writer.WriteLine(scene.name);
                    writer.Close();
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        private static TreeListNode WWgetNode(string name)
        {
            if (tree.Nodes.Count == 0) return null;
            foreach (TreeListNode item in tree.Nodes[0].Nodes)
            {
                if(item.GetDisplayText("Text")==name)
                {
                    return item;
                }
            }
            return null;
        }

        public static void WWreLoadScene()
        {
            if (scene == null) return;
            WWloadScene(scene.path);
        }

        delegate void LoadSceneInvoke(int Msg); //代理

        public static void WWloadScene(int index)
        {
            try
            {
                if (proper.InvokeRequired)
                {
                    LoadSceneInvoke ds = new LoadSceneInvoke(WWloadScene);
                    proper.Invoke(ds, new object[] { index });
                    return;
                }
                WWcloseScene();
                string path = proj.ScenesPath + proj.Scenename[index] + ".WWscene";
                WWscene NEW = WWscene.WWloadScene(path);
                scene = NEW;
                foreach (var item in scene.Objects)
                {
                    item.WWregist();
                    item.WWload();
                }
                NEW.WWinit();
                NEW.WWfillTree();
                curScene = WWgetNode(NEW.name);
                if (!curScene.Expanded) curScene.Expand();
                tree.FocusedNode = curScene;
                if (!WWasCtrl.propertylock)
                    proper.SelectedObject = scene;
                Curname = NEW.name;
                using (StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "lastpath.ini"))
                {
                    writer.WriteLine(proj.ProjPath);
                    writer.WriteLine(scene.name);
                    writer.Close();
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }

        public static WWobject WWgetObj(int id)
        {
            if (keyobjects.Keys.Contains(id))
                return keyobjects[id];
            return null;
        }

        public static WWmoduleBase WWgetMod(int id)
        {
            if (keymodules.ContainsKey(id))
                return keymodules[id];
            return null;
        }

        public static void WWreName(int objId, string name)
        {
            WWobject obj = WWgetObj(objId);
            if (obj != null)
            {
                obj.Name = name;
            }
        }
        public static bool WWdelObj(int objId)
        {
            if (scene == null) return false;
            WWobject obj = null;
            if (keyobjects.ContainsKey(objId)) 
                obj = keyobjects[objId];
            if (obj == null) return false;
            if(obj.Parent!=null)
            {
                return obj.Parent.WWdelObj(obj);
            }
            WWremoveObj(obj);
            return scene.WWdelObj(obj);
        }

        public static void WWremoveMod(WWmoduleBase mod)
        {
            if (keymodules.ContainsKey(mod.ModuleID))
                keymodules.Remove(mod.ModuleID);
        }

        public static void WWremoveObj(WWobject obj)
        {
            if (keyobjects.ContainsKey(obj.ID))
                keyobjects.Remove(obj.ID);
        }

        internal static bool WWdelMod(int id)
        {
            if (scene == null) return false;
            WWmoduleBase mod = null;
            if (keymodules.ContainsKey(id)) mod = keymodules[id];
            if (mod == null) return false;
            if (mod.Parent != null)
            {
                return mod.Parent.WWdelMod(mod);
            }
            return false;
        }

    }
}
