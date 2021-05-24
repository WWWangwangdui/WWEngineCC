using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraTreeList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.Utils.Drawing;
using System.Drawing;
using DevExpress.Images;
using WWEngineCC.Properties;
using DevExpress.XtraEditors;
using System.Xml.Serialization;

namespace WWEngineCC
{
    static class WWasCtrl
    {
        private static Dictionary<string, WWassetBase> Name_Asset;
        private static Dictionary<int, WWassetBase> ID_Asset;
        private static Dictionary<string, Image> Path_Image;
        private static TreeList tree;
        private static int nxtid;
        private static GalleryControl gallery;
        private static DataTable dt;
        public static bool propertylock = false;
        private static string curpath;
        public static string WWCurPath
        {
            get => curpath;
            set => curpath = value;
        }
        public static void WWinit(TreeList _tree, GalleryControl _gallery)
        {
            tree = _tree;
            gallery = _gallery;dt = new DataTable();
            gallery.Gallery.ItemImageLayout = ImageLayoutMode.ZoomInside;
            gallery.Gallery.ImageSize = new Size(120, 90);
            gallery.Gallery.ShowItemText = true;
            GalleryItemGroup group1 = new GalleryItemGroup();
            group1.Caption = "";
            gallery.Gallery.Groups.Add(group1);
            dt.Columns.Add("ID");
            dt.Columns.Add("ParentID");
            dt.Columns.Add("Text");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["ID"] };
            tree.DataSource = dt;
            ID_Asset = new Dictionary<int, WWassetBase>();
            Name_Asset = new Dictionary<string, WWassetBase>();
            Path_Image = new Dictionary<string, Image>();
        }

        public static void WWremoverAsset(string name)
        {
            WWassetBase asset = WWgetAsset(name);
            if (asset == null) return;
            Name_Asset.Remove(name);
            ID_Asset.Remove(asset.AssetID);
            if (File.Exists(asset.GlobleAssetPath))
            {
                try
                {
                    File.Delete(asset.GlobleAssetPath);
                }
                catch
                {

                }
            }
            foreach (GalleryItem item in gallery.Gallery.Groups[0].Items)
            {
                if(item.Description==asset.AssetName)
                {
                    gallery.Gallery.Groups[0].Items.Remove(item);
                    return;
                }
            }
        }

        public static void __add__key__image__(string path, Image ima)
        {
            if (!Path_Image.ContainsKey(path))
                Path_Image.Add(path, ima);
        }
        public static Image __get__key__image(string path)
        {
            if (Path_Image.ContainsKey(path)) return Path_Image[path];
            return null;
        }

        public static WWassetBase WWgetAsset(string name)
        {
            if (Name_Asset.ContainsKey(name)) return Name_Asset[name];
            return null;
        }

        public static void WWaddAsset(WWassetBase asset)
        {
            asset.AssetID = WWuseId();
            ID_Asset.Add(asset.AssetID, asset);
        }

        public static string WWgetCorrectName(string folder, string name, string ext)
        {
            int cnt = 1;
            string tmp = folder + name + "." + ext;
            while (File.Exists(tmp))
            {
                tmp = folder + name + cnt.ToString() + "." + ext;
            }
            return name + cnt.ToString();
        }

        public static string WWgetCorrectPath(string folder, string name, string ext)
        {
            int cnt = 1;
            string tmp = folder + name + "." + ext;
            while (File.Exists(tmp))
            {
                tmp = folder + name + cnt.ToString() + "." + ext;
            }
            return tmp;
        }

        public static string WWgetCorrectName(string name,string ext)
        {
            int cnt = 1;
            string tmp = WWDirector.WWProject.AssetsPath + name + "." + ext;
            while (File.Exists(tmp))
            {
                tmp = WWDirector.WWProject.AssetsPath + name + cnt.ToString() + "." + ext;
            }
            return name + cnt.ToString();
        }

        public static string WWgetCorrectPath(string name, string ext)
        {
            int cnt = 1;
            string tmp = WWDirector.WWProject.AssetsPath + name + "." + ext;
            while (File.Exists(tmp))
            {
                tmp = WWDirector.WWProject.AssetsPath + name + cnt.ToString() + "." + ext;
            }
            return tmp;
        }

        public static bool WWassetRename(string s1, string s2, WWassetBase obj)
        {
            if (s2 == null || Name_Asset.ContainsKey(s2)) return false;
            if (s1 == null)
            {
                Name_Asset.Add(s2, obj);
                return true;
            }
            if(Name_Asset.ContainsKey(s1))
            {
                if (Name_Asset[s1] != obj)
                    return false;
                Name_Asset.Remove(s1);
                Name_Asset.Add(s2, obj);
                return true;
            }
            else
            {
                Name_Asset.Add(s2, obj);
                return true;
            }
        }

        public static void WWfolderReload(string s1,string s2)
        {
            foreach (GalleryItem item in gallery.Gallery.Groups[0].Items)
            {
                if(item.Description==s1)
                {
                    item.Description = s2;
                    item.Caption = s2;
                }
            }
        }

        internal static void WWsaveAssets()
        {
            foreach (var item in Name_Asset.Values)
            {
                item.WWsaveAsset();
            }
        }

        public static int WWasId
        {
            get
            {
                return nxtid;
            }
        }

        public static int WWuseId()
        {
            return nxtid++;
        }

        public static void WWloadAssets()
        {
            ID_Asset.Clear();
            Name_Asset.Clear();
            Path_Image.Clear();
            string path = WWDirector.WWProject.AssetsPath;
            if (!Directory.Exists(path)) return;
            nxtid = 1;
            XmlSerializer xml = new XmlSerializer(typeof(AsBitmap));
            List<string> errors = new List<string>();
            foreach (string item in Directory.GetFiles(path,"*.WWBitmap",SearchOption.AllDirectories))
            {
                try
                {
                    using (FileStream fs = new FileStream(item, FileMode.Open))
                    {
                        AsBitmap NEW = xml.Deserialize(fs) as AsBitmap;
                        ID_Asset.Add(NEW.AssetID, NEW);
                        nxtid = Math.Max(nxtid, NEW.AssetID + 1);
                        fs.Close();
                    }
                }
                catch
                {
                    errors.Add(item);
                }
            }
            xml = new XmlSerializer(typeof(AsAnimation));
            foreach (string item in Directory.GetFiles(path, "*.WWAnimation", SearchOption.AllDirectories))
            {
                try
                {
                    using (FileStream fs = new FileStream(item, FileMode.Open))
                    {
                        AsAnimation NEW = xml.Deserialize(fs) as AsAnimation;
                        ID_Asset.Add(NEW.AssetID, NEW);
                        nxtid = Math.Max(nxtid, NEW.AssetID + 1);
                        fs.Close();
                    }
                }
                catch
                {
                    errors.Add(item);
                }
            }
            if(errors.Count!=0)
            {
                string tmp = "";
                foreach (string item in errors)
                {
                    tmp += item + "\r\n";
                }
                XtraMessageBox.Show("以下资源未正常加载:\r\n" + tmp);
            }
        }

        private static void WWaddRow(string ID, string ParentId, string Text)
        {
            DataRow row = dt.NewRow();
            row.SetField<string>("ID", ID);
            if (ParentId == null) row.SetField("ParentID", DBNull.Value);
            else row.SetField("ParentID", ParentId);
            row.SetField("Text", Text);
            dt.Rows.Add(row);
        }
        public static void WWfillTree(string path)
        {
            if (path.Last() == '\\') path = path.Remove(path.Length - 1);
            dt.Rows.Clear();
            WWfill(null, path);
            foreach (TreeListNode item in tree.Nodes)
            {
                item.Expand();
            }
        }
        private static void WWfill(string parent, string path)
        {
            WWaddRow(path, parent, path.Split('\\').Last());
            foreach (var item in Directory.GetDirectories(path))
            {
                WWfill(path, item);
            }
        }
        public static void WWopenFolder(string path)
        {
            gallery.Gallery.Groups[0].Items.Clear();
            Image folder = Resources.Folder;
            curpath = path.Split('\\').Last();
            foreach (var item in Directory.GetDirectories(path))
            {
                GalleryItem NEW = new GalleryItem(folder, item.Split('\\').Last(), item);
                gallery.Gallery.Groups[0].Items.Add(NEW);
            }
            foreach (string item in Directory.GetFiles(path, "*.WWBitmap")) 
            {
                string name = Path.GetFileNameWithoutExtension(item);
                if (Name_Asset.ContainsKey(name))
                {
                    WWassetBase tmp = Name_Asset[name];
                    GalleryItem NEW = new GalleryItem(tmp.WWgetImage(), tmp.AssetName, tmp.AssetName);
                    NEW.Tag = tmp;
                    gallery.Gallery.Groups[0].Items.Add(NEW);
                }
            }
            foreach (string item in Directory.GetFiles(path, "*.WWAnimation"))
            {
                string name = Path.GetFileNameWithoutExtension(item);
                if (Name_Asset.ContainsKey(name))
                {
                    WWassetBase tmp = Name_Asset[name];
                    GalleryItem NEW = new GalleryItem(tmp.WWgetImage(), tmp.AssetName, tmp.AssetName);
                    NEW.Tag = tmp;
                    gallery.Gallery.Groups[0].Items.Add(NEW);
                }
            }
        }

        public static string WWimportBitmap(string path)
        {
            string name = path.Split('\\').Last().Split('.').First();
            name = WWgetCorrectName(name, "png");
            File.Copy(path, WWDirector.WWProject.AssetsPath + name + ".png");
            AsBitmap NEW = new AsBitmap();
            NEW.AssetName = name;
            NEW.AssetPath = name + ".WWBitmap";
            NEW.BitmapPath = name + ".png";
            NEW.Size = Image.FromFile(NEW.GlobleBitmapPath).Size;
            NEW.Sourcesize = NEW.Size;
            WWaddAsset(NEW);
            if (WWCurPath == "Assets")
                WWaddToFolder(NEW);
            NEW.WWsaveAsset();
            return null;
        }

        public static string WWimportAnimation(string path)
        {
            string name = path.Split('\\').Last().Split('.').First();
            name = WWgetCorrectName(name, "png");
            File.Copy(path, WWDirector.WWProject.AssetsPath + name + ".png");
            AsAnimation NEW = new AsAnimation();
            NEW.AssetName = name;
            NEW.AssetPath = name + ".WWAnimation";
            NEW.BitmapPath = name + ".png";
            NEW.Size = Image.FromFile(NEW.GlobleBitmapPath).Size;
            NEW.Sourcesize = NEW.Size;
            WWaddAsset(NEW);
            if (WWCurPath == "Assets")
                WWaddToFolder(NEW);
            NEW.WWsaveAsset();
            return null;
        }

        public static void WWaddToFolder(WWassetBase tmp)
        {
            GalleryItem NEW = new GalleryItem(tmp.WWgetImage(), tmp.AssetName, tmp.AssetName);
            NEW.Tag = tmp;
            gallery.Gallery.Groups[0].Items.Add(NEW);
            gallery.Gallery.FocusedItem = NEW;
        }

        internal static void WWcloseAssets()
        {
            nxtid = 1;
            ID_Asset.Clear();
            dt.Rows.Clear();
            gallery.Gallery.Groups[0].Items.Clear();
        }

        public static WWassetBase WWgetAsset(int id)
        {
            if (ID_Asset.ContainsKey(id)) return ID_Asset[id];
            return null;
        }
    }
}
