//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.Remoting.Lifetime;
//using System.Security.Policy;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Serialization;
//using DevExpress.XtraBars;
//namespace WWEngineCC
//{

//    public static class WWPluginCC
//    {
//        private static ProxyObject proxy = null;
//        private static AppDomain Domain = null;
//        private static AppDomainSetup setup = null;
//        private static BarListItem scriptlist;
//        public static List<Type> types = new List<Type>();
//        public static Dictionary<string, Type> scripts = new Dictionary<string, Type>();
//        public static void WWinit()
//        {
//            types.Add(typeof(WWTransform));
//            types.Add(typeof(Line));
//            types.Add(typeof(Ellipse));
//            types.Add(typeof(Rectangle));
//            types.Add(typeof(Bitmap));
//            types.Add(typeof(Animation));
//            types.Add(typeof(Text));
//            types.Add(typeof(AsBitmap));
//            types.Add(typeof(AsAnimation));
//            types.Add(typeof(WWScript));
//            types.Add(typeof(WWparam));
//        }
//        public static void RegistMod(BarListItem item, BarListItem script)
//        {
//            scriptlist = script;
//            item.Strings.Add(typeof(WWTransform).FullName);
//            item.Strings.Add(typeof(Line).FullName);
//            item.Strings.Add(typeof(Ellipse).FullName);
//            item.Strings.Add(typeof(Rectangle).FullName);
//            item.Strings.Add(typeof(Bitmap).FullName);
//            item.Strings.Add(typeof(Text).FullName);
//            item.Strings.Add(typeof(Animation).FullName);
//        }
//        public static WWmoduleBase WWgetMod(string str)
//        {
//            Type tp = Type.GetType(str);
//            if (tp == null) return null;
//            return Activator.CreateInstance(tp) as WWmoduleBase;
//        }
//        public static void CreateAppDomainSetup()
//        {
//            setup = new AppDomainSetup();
//            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
//            setup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
//            setup.ShadowCopyFiles = "true";
//            setup.ShadowCopyDirectories = WWDirector.WWProject.BinPath;
//            setup.ApplicationName = "Dynamic";
//        }



//        public static Type WWgetScriptType(string name)
//        {
//            return proxy.getType(name);
//        }

//        public static void WWgetScript(string name, int id)
//        {
//            proxy.Invoke(name, id);
//        }

//        public static void WWloadPlugins()
//        {
//            if (WWDirector.WWProject == null) return;
//            string path = WWDirector.WWProject.BinPath + "WWScripts.dll";
//            if (!File.Exists(path)) return;
//            if (!Directory.Exists(WWDirector.WWProject.BinPath + "temp")) Directory.CreateDirectory(WWDirector.WWProject.BinPath + "temp");
//            if (File.Exists(WWDirector.WWProject.BinPath + "temp\\WWScripts.dll")) File.Delete(WWDirector.WWProject.BinPath + "temp\\WWScripts.dll");
//            File.Copy(path, WWDirector.WWProject.BinPath + "temp\\WWScripts.dll");
//            path = WWDirector.WWProject.BinPath + "temp\\WWScripts.dll";

//            WWunloadPlugins();

//            CreateAppDomainSetup();
//            Evidence evidence = new Evidence(AppDomain.CurrentDomain.Evidence); 
//            Domain = AppDomain.CreateDomain("plugins", evidence, setup);
//            proxy = (ProxyObject)Domain.CreateInstanceAndUnwrap(Assembly.GetAssembly(typeof(ProxyObject)).FullName,typeof(ProxyObject).ToString());
//            proxy.path = path;
//            foreach(string item in proxy.LoadAssembly(scripts, types))
//            {
//                scriptlist.Strings.Add(item);
//            }

//            //Assembly asm = Assembly.LoadFile(path);
//            //Type[] t = asm.GetExportedTypes();
//            //foreach (var item in t)
//            //{
//            //    if(item.GetInterface("IWWScript")!=null)
//            //    {
//            //        scripts.Add(item.Name, item);
//            //        scriptlist.Strings.Add(item.Name);
//            //        types.Add(item);
//            //    }
//            //}
//            WWDirector.WWreLoadScene();
//        }
//        public static void WWunloadPlugins()
//        {
//            WWDirector.WWcloseScene();
//            scriptlist.Strings.Clear();
//            foreach (var item in scripts.Values)
//            {
//                types.Remove(item);
//            }
//            scripts.Clear();
//            GC.Collect();
//            if (Domain != null)
//            {
//                AppDomain.Unload(Domain);
//                Domain = null;
//            }
//        }
//        public static void WWsaveScript(string name, string filepath, int id)
//        {
//            proxy.savescript(name, filepath, id);
//        }
//        public static void WWloadScript(string name, string filepath, int id)
//        {
//            proxy.loadscript(name, filepath, id);
//        }
//        public static bool WWdoMethod(string name, int id, params object[] objs)
//        {
//            if (proxy == null) return false;
//            return proxy.WWdoMethod(name, id, objs);
//        }
//    }

//    class ProxyObject : MarshalByRefObject
//    {
//        Assembly assembly = null;
//        public string path;
//        Dictionary<string, string> names = new Dictionary<string, string>();
//        Dictionary<int, WWScriptBase> scriptss = new Dictionary<int, WWScriptBase>();
//        public bool WWdoMethod(string name, int id, params object[] objs)
//        {
//            if (!scriptss.ContainsKey(id))
//                return false;
//            object obj = scriptss[id];
//            Type tp = obj.GetType();
//            MethodInfo method = tp.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
//            method.Invoke(obj, objs);
//            return true;
//        }
//        public string[] LoadAssembly(Dictionary<string, Type> scripts, List<Type> types)
//        {
//            List<string> strs=new List<string>();
//            assembly = Assembly.LoadFile(path);
//            Type[] t = assembly.GetExportedTypes();
//            foreach (var item in t)
//            {
//                if (item.GetInterface("IWWScript") != null)
//                {
//                    names.Add(item.Name, item.FullName);
//                    scripts.Add(item.Name, item);
//                    strs.Add(item.Name);
//                    types.Add(item);
//                }
//            }
//            return strs.ToArray();
//        }
//        public void Invoke(string ClassName,int id, params Object[] args)
//        {
//            if (!names.ContainsKey(ClassName)) return;
//            string fullClassName = names[ClassName];
//            if (assembly == null)
//                return;
//            Type tp = assembly.GetType(fullClassName);
//            if (tp == null)
//                return;
//            var array = tp.GetConstructors();
//            if (array.Length == 0)
//                array = typeof(WWScriptBase).GetConstructors();
//            if (array.Length == 0)
//                return;
//            object obj = Activator.CreateInstance(tp);
//            array[0].Invoke(obj, args);
//            if (scriptss.ContainsKey(id)) scriptss.Remove(id);
//            scriptss.Add(id, obj as WWScriptBase);
//        }
//        public Type getType(string ClassName)
//        {
//            if (!names.ContainsKey(ClassName)) return null;
//            string fullClassName = names[ClassName];
//            if (assembly == null)
//                return null;
//            Type tp = assembly.GetType(fullClassName);
//            return tp;
//        }

//        internal void savescript(string name, string filepath, int id)
//        {
//            if (!scriptss.ContainsKey(id)) return;
//            using (FileStream fp = File.Create(filepath))
//            {
//                XmlSerializer serializer = new XmlSerializer(getType(name));
//                serializer.Serialize(fp, scriptss[id]);
//                fp.Close();
//            }
//        }

//        internal void loadscript(string name, string filepath, int id)
//        {
//            if (scriptss.ContainsKey(id)) scriptss.Remove(id);
//            using (FileStream fp = File.Open(filepath,FileMode.Open))
//            {
//                XmlSerializer serializer = new XmlSerializer(getType(name));
//                scriptss.Add(id, serializer.Deserialize(fp) as WWScriptBase);
//                fp.Close();
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DevExpress.XtraBars;
namespace WWEngineCC
{
    public class WWScript
        : WWmoduleBase
    {
        private string scriptName;
        private string filepath;
        [Category("组件信息")]
        [DisplayName("组件地址")]
        public string GlobalPath
        {
            get => WWDirector.WWProject.DataPath + filepath;
        }

        public string Filepath
        {
            get => filepath;
            set => filepath = value;
        }

        [Category("组件信息")]
        [DisplayName("组件名")]
        public string ScriptName
        {
            get => scriptName;
            set => scriptName = value;
        }

        public override string name => scriptName;

        private WWScriptBase script = null;

        [XmlIgnore()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public WWScriptBase Script
        {
            get => script;
            set => script = value;
        }

        public override object __get__this__property__()
        {
            return this;
        }

        public override void WWinit(WWobject obj)
        {
            parent = obj;
            if (script != null)
            {
                (script).WWinit(this);
            }
        }

        public override void WWsleepUpdate()
        {
            if (script != null)
            {
                script.WWsleepUpdate();
            }
        }

        public override void WWkilled()
        {
            if (script != null)
            {
                (script).WWkilled();
            }
            script = null;
        }

        public override void WWupdate()
        {
            if (script != null)
            {
                script.WWupdate();
            }
            else
            {
                script = WWPluginCC.WWgetScript(scriptName) as WWScriptBase;
                script.WWinit(this);
            }
        }

        public override void WWsave()
        {
            if (script == null) return;
            XmlSerializer serializer = new XmlSerializer(WWPluginCC.WWgetScriptType(scriptName));
            using (FileStream fp = File.Create(GlobalPath))
            {
                serializer.Serialize(fp, script);
                fp.Close();
            }
            if (script != null)
            {
                ((IWWScript)script).WWsave();
            }
        }

        public override void WWload()
        {
            if (File.Exists(GlobalPath))
            {
                XmlSerializer serializer = new XmlSerializer(WWPluginCC.WWgetScriptType(scriptName));
                using (FileStream fp = new FileStream(GlobalPath, FileMode.Open))
                {
                    script = (WWScriptBase)serializer.Deserialize(fp);
                    script.WWinit(this);
                    fp.Close();
                }
            }
            else
            {
                script = (WWScriptBase)WWPluginCC.WWgetScript(scriptName);
                script.WWinit(this);
            }
            if (script != null)
            {
                ((WWScriptBase)script).WWload();
            }
        }
    }

    public interface IWWScript
    {
        void WWupdate();
        void WWinit(WWScript pt);
        void WWkilled();
        void WWsave();
        void WWload();
        void WWsleepUpdate();
    }

    public abstract class WWScriptBase
        : IWWScript
    {
        public abstract void WWupdate();
        public virtual void WWinit(WWScript pt) { }
        public virtual void WWkilled() { }
        public virtual void WWsave() { }
        public virtual void WWload() { }
        public virtual void WWsleepUpdate() { }
    }

    public static class WWPluginCC
    {
        private static BarListItem scriptlist;
        public static List<Type> types = new List<Type>();
        public static Dictionary<string, Type> scripts = new Dictionary<string, Type>();
        public static Dictionary<string, string> Names = new Dictionary<string, string>();
        public static void WWinit()
        {
            types.Add(typeof(WWTransform));
            types.Add(typeof(WWLine));
            types.Add(typeof(WWEllipse));
            types.Add(typeof(WWRectangle));
            types.Add(typeof(WWBitmap));
            types.Add(typeof(WWAnimation));
            types.Add(typeof(WWText));
            types.Add(typeof(AsBitmap));
            types.Add(typeof(AsAnimation));
            types.Add(typeof(WWScript));
        }
        public static void RegistMod(BarListItem item, BarListItem script)
        {
            scriptlist = script;
            item.Strings.Add(typeof(WWTransform).Name);
            Names.Add(typeof(WWTransform).Name, typeof(WWTransform).FullName);
            item.Strings.Add(typeof(WWLine).Name);
            Names.Add(typeof(WWLine).Name, typeof(WWLine).FullName);
            item.Strings.Add(typeof(WWEllipse).Name);
            Names.Add(typeof(WWEllipse).Name, typeof(WWEllipse).FullName);
            item.Strings.Add(typeof(WWRectangle).Name);
            Names.Add(typeof(WWRectangle).Name, typeof(WWRectangle).FullName);
            item.Strings.Add(typeof(WWBitmap).Name);
            Names.Add(typeof(WWBitmap).Name, typeof(WWBitmap).FullName);
            item.Strings.Add(typeof(WWText).Name);
            Names.Add(typeof(WWText).Name, typeof(WWText).FullName);
            item.Strings.Add(typeof(WWAnimation).Name);
            Names.Add(typeof(WWAnimation).Name, typeof(WWAnimation).FullName);
        }
        public static WWmoduleBase WWgetMod(string str)
        {
            if (Names.ContainsKey(str)) str = Names[str];
            else return null;
            Type tp = Type.GetType(str);
            if (tp == null) return null;
            return Activator.CreateInstance(tp) as WWmoduleBase;
        }

        public static Type WWgetScriptType(string name)
        {
            if (scripts.ContainsKey(name)) return scripts[name];
            return null;
        }

        public static object WWgetScript(string name)
        {
            if (scripts.ContainsKey(name)) return Activator.CreateInstance(scripts[name]);
            return null;
        }

        public static void WWloadPlugins()
        {
            //if (setup == null) CreateAppDomainSetup();
            //Domain = AppDomain.CreateDomain("plugins", null, setup);
            //Domain.DoCallBack(delegate
            //{
            //    LifetimeServices.LeaseTime = TimeSpan.Zero;
            //});
            WWunloadPlugins();
            if (WWDirector.WWProject == null) return;
            string _path = WWDirector.WWProject.BinPath + "WWScripts.dll";
            string _pdb = WWDirector.WWProject.BinPath + "WWScripts.pdb";
            if (!File.Exists(_path)) return;
            if (!File.Exists(_pdb)) return;
            if (!Directory.Exists(WWDirector.WWProject.BinPath + "temp")) Directory.CreateDirectory(WWDirector.WWProject.BinPath + "temp");
            string _name = WWDirector.getRandBaseStr();
            string basepath = WWDirector.WWProject.BinPath + "temp\\" + _name;
            while(Directory.Exists(basepath))
            {
                _name = WWDirector.getRandBaseStr();
                basepath = WWDirector.WWProject.BinPath + "temp\\" + _name;
            }
            Directory.CreateDirectory(basepath);
            string path = basepath + "\\WWScripts.dll";
            string pdbpath = basepath + "\\WWScripts.pdb";

            File.Copy(_path, path);
            File.Copy(_pdb, pdbpath);
            //File.Delete(_pdb);
            Assembly asm = Assembly.LoadFile(path);
            Type[] t = asm.GetExportedTypes();
            foreach (var item in t)
            {
                if (item.GetInterface("IWWScript") != null)
                {
                    scripts.Add(item.Name, item);
                    scriptlist.Strings.Add(item.Name);
                    types.Add(item);
                }
            }
        }
        public static void WWunloadPlugins()
        {
            scriptlist.Strings.Clear();
            foreach (var item in scripts.Values)
            {
                types.Remove(item);
            }
            scripts.Clear();
        }

    }
}
