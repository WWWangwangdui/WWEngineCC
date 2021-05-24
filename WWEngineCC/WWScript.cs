using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WWEngineCC
{
    public class WWScript
    : WWmoduleBase
    {
        private string scriptName;
        private string filepath;
        [Category("组件信息")]
        [DisplayName("组件名")]
        public string FilePath
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

        //private WWScriptBase script = null;

        //[XmlIgnore()]
        //[TypeConverter(typeof(ExpandableObjectConverter))]
        //public WWScriptBase Script
        //{
        //    get => script;
        //    set => script = value;
        //}

        public override object __get__this__property__()
        {
            return this;
        }

        public override void WWinit(WWobject obj)
        {
            parent = obj;
            WWPluginCC.WWdoMethod("WWinit", ModuleID, ModuleID);
        }

        public override void WWkilled()
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            WWPluginCC.WWdoMethod("WWkilled", ModuleID);
        }

        public override void WWupdate()
        {
            if (!WWPluginCC.WWdoMethod("WWupdate", ModuleID))
            {
                WWPluginCC.WWgetScript(scriptName, ModuleID);
                WWPluginCC.WWdoMethod("WWinit", ModuleID, ModuleID);
            }
        }

        public override void WWsave()
        {
            WWPluginCC.WWsaveScript(scriptName, filepath, ModuleID);
            WWPluginCC.WWdoMethod("WWsave", ModuleID);
        }

        public override void WWload()
        {
            if (File.Exists(filepath))
            {
                WWPluginCC.WWloadScript(scriptName, filepath, ModuleID);
            }
            else
            {
                WWPluginCC.WWgetScript(scriptName, ModuleID);
                WWPluginCC.WWdoMethod("WWinit", ModuleID, ModuleID);
            }
            WWPluginCC.WWdoMethod("WWload", ModuleID);
        }
    }

    public interface IWWScript
    {
        void WWupdate();
        void WWinit(int pt);
        void WWkilled();
        void WWsave();
        void WWload();
    }
    [Serializable()]
    public abstract class WWScriptBase
     : IWWScript
    {
        public abstract void WWupdate();
        public abstract void WWinit(int pt);
        public abstract void WWkilled();
        public abstract void WWsave();
        public abstract void WWload();
        public WWScriptBase()
        {

        }
    }
}
