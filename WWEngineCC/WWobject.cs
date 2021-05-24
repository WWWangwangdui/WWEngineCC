using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraTreeList.Nodes;
using System.ComponentModel;
using System.Xml.Serialization;

namespace WWEngineCC
{
    public class WWobject
    {
        private bool act = true;
        private int id;
        private string name = "";
        private WWTransform transform = null;
        public WWTransform Transform
        {
            get
            {
                if(transform==null)
                {
                    foreach (var item in modules)
                    {
                        if(item.name=="WWTransform")
                        {
                            transform = item as WWTransform;
                            break;
                        }
                    }
                }
                return transform;
            }
        }
        private WWobject parent = null;
        [Category("基本属性")]
        [DisplayName("是否活动")]
        public bool activity
        {
            get
            {
                return act;
            }
            set
            {
                act = value;
            }
        }

        [Category("基本属性")]
        [DisplayName("父游戏对象")]
        public WWobject Parent
        {
            get
            {
                return parent;
            }
        }
        string cstname = null;
        [Category("基本属性")]
        [DisplayName("唯一标识名")]
        public string ConstName
        {
            get
            {
                return cstname;
            }
            set
            {
                if (value != null)
                {
                    if (WWDirector.WWaddGlobleParam(value, this))
                    {
                        if (cstname != null)
                            WWDirector.WWdelGlobleParam(cstname);
                        cstname = value;
                    }
                }
                else
                {
                    if (cstname != null)
                    {
                        WWDirector.WWdelGlobleParam(cstname);
                    }
                    cstname = null;
                }
            }
        }

        public void WWregistConstName()
        {
            cstname = WWDirector.WWaddGlobleParam(this);
        }
        public WWobject()
        {
            id = WWDirector.objRegist(this);
        }
        public void WWregist()
        {
            id = WWDirector.objRegist(this);
            foreach (var item in sonobjects)
            {
                item.WWregist();
            }
            foreach (var item in modules)
            {
                item.WWregist();
            }
        }
        public void WWsave()
        {
            foreach (var item in sonobjects)
            {
                item.WWsave();
            }
            foreach (var item in modules)
            {
                item.WWsave();
            }
        }
        public void WWload()
        {
            foreach (var item in sonobjects)
            {
                item.WWload();
            }
            foreach (var item in modules)
            {
                item.WWload();
            }
        }
        [Browsable(false)]
        public string DataId
        {
            get
            {
                return "o" + id.ToString();
            }
        }
        [Category("基本属性")]
        [DisplayName("游戏对象名")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                DataRow row = WWDirector.treeData.Rows.Find("o"+id.ToString());
                if (row == null) return;
                row.SetField<string>("Text", name);

            }
        }
        [Category("基本属性")]
        [DisplayName("游戏对象编号")]
        public int ID
        {
            get
            {
                return id;
            }
        }
        public void WWinit(WWobject obj)
        {
            parent = obj;
            foreach (WWmoduleBase item in modules.ToArray())
            {
                item.WWinit(this);
            }
            foreach (WWobject item in sonobjects.ToArray())
            {
                item.WWinit(this);
            }
        }

        internal void WWsleepUpdate()
        {
            foreach (WWmoduleBase item in modules.ToArray())
            {
                if (item.activity)
                    item.WWsleepUpdate();
            }
            foreach (WWobject item in sonobjects.ToArray())
            {
                if (item.activity)
                    item.WWsleepUpdate();
            }
        }

        public WWmoduleBase WWaddMod(string name)
        {
            WWmoduleBase NEW = WWPluginCC.WWgetMod(name);
            if (NEW != null)
            {
                NEW.WWinit(this);
                modules.Add(NEW);
                NEW.WWfillTree(DataId);
            }
            if(name=="WWTransform")
            {
                this.transform = NEW as WWTransform;
            }
            return NEW;
        }

        public void WWupdate()
        {
            if (!activity) return;
            foreach (WWmoduleBase item in modules.ToArray()) 
            {
                if (item.activity)
                    item.WWupdate();
            }
            foreach (WWobject item in sonobjects.ToArray())
            { if (item.activity)
                item.WWupdate();
            }
        }

        public WWobject WWaddObj(string name = "")
        {
            WWobject NEW = new WWobject();
            NEW.Name = name;
            NEW.parent = this;
            sonobjects.Add(NEW);
            NEW.WWaddMod("WWTransform");
            return NEW;
        }

        public WWmoduleBase WWgetModule(string name)
        {
            string[] buffer = name.Split('\\');
            if (buffer.Length == 1)
            {
                foreach (var item in modules)
                {
                    if (item.name == buffer[0]) return item;
                }
            }
            else if (buffer.Length > 1) 
            {
                foreach (var item in sonobjects)
                {
                    if (item.Name == buffer[0])
                        return item.WWgetModule(string.Join("\\", buffer, 1, buffer.Length - 1));
                }
            }
            return null;
        }

        public bool WWdelMod(WWmoduleBase mod)
        {
            if(modules.Contains(mod))
            {
                mod.WWkilled();
                modules.Remove(mod);
                WWDirector.WWremoveMod(mod);
                return true;
            }
            return false;
        }

        public bool WWdelObj(WWobject obj)
        {
            if(sonobjects.Contains(obj))
            {
                obj.WWkilled();
                sonobjects.Remove(obj);
                WWDirector.WWremoveObj(obj);
                return true;
            }
            return false;
        }
        public void WWkilled()
        {
            foreach (var item in sonobjects.ToArray())
            {
                item.WWkilled();
            }
            sonobjects.Clear();
            foreach (var item in modules.ToArray())
            {
                item.WWkilled();
            }
            modules.Clear();
            WWDirector.WWremoveObj(this);
        }
        private List<WWobject> sonobjects = new List<WWobject>();
        private List<WWmoduleBase> modules = new List<WWmoduleBase>();
        [Category("子成员")]
        [DisplayName("子游戏对象")]
        public List<WWobject> WWsonObjects
        {
            get
            {
                return sonobjects;
            }
        }
        [Category("子成员")]
        [DisplayName("组件")]
        public List<WWmoduleBase>WWmodules
        {
            get
            {
                return modules;
            }
        }
        public void WWfillTree(string Parent)
        {
            string curid = "o" + id.ToString();
            WWDirector.WWaddRow(curid, Parent, Name, WWassetsType.Object, id.ToString());
            foreach (var item in sonobjects)
            {
                item.WWfillTree(curid);
            }
            foreach (var item in modules)
            {
                item.WWfillTree(curid);
            }
        }
        public void WWaddScript(string name)
        {
            WWScript NEW = new WWScript();
            if (NEW != null)
            {
                NEW.ScriptName = name;
                NEW.Filepath = WWDirector.getRandBaseStr() + ".WWScript";
                NEW.WWinit(this);
                modules.Add(NEW);
                NEW.WWload();
                NEW.WWfillTree(DataId);
            }
        }
    }
}
