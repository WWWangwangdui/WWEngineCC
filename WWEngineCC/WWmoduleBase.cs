using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;

namespace WWEngineCC
{
    public abstract class WWmoduleBase
    {
        private int id;
        protected WWobject parent;
        [XmlIgnore]
        public WWobject Parent
        {
            get
            {
                return parent;
            }
        }

        public WWmoduleBase()
        {
            id = WWDirector.modRegist(this);
        }
        public void WWregist()
        {
            id = WWDirector.modRegist(this);
        }
        public int ModuleID
        {
            get
            {
                return id;
            }
        }
        protected bool act = true;
        public virtual bool activity
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
        public virtual string name
        {
            get
            {
                return GetType().Name;
            }
        }
        public void save(XmlWriter fs)
        {
            Type tp = GetType();
            XmlSerializer type = new XmlSerializer(typeof(string));
            type.Serialize(fs, tp.Name);

            XmlSerializer serializer = new XmlSerializer(tp);
            serializer.Serialize(fs, this);
        }

        public void WWfillTree(string Parent)
        {
            WWDirector.WWaddRow("m" + id.ToString(), Parent, name, WWassetsType.Module, id.ToString());
        }

        public static void load(FileStream fs, WWmoduleBase module)
        {
            XmlSerializer type = new XmlSerializer(typeof(string));
            string name = type.Deserialize(fs) as string;
            Type tp = Type.GetType(name);
            XmlSerializer serializer = new XmlSerializer(tp);
            module = (WWmoduleBase)serializer.Deserialize(fs);
        }
        public virtual void WWinit(WWobject obj)
        {
            parent = obj;
        }
        public abstract void WWupdate();
        public virtual void WWkilled()
        {
            WWDirector.WWremoveMod(this);
        }
        public virtual void WWsave()
        {

        }
        public virtual void WWload()
        {

        }
        public virtual object __get__this__property__()
        {
            return this;
        }
        public virtual void WWsleepUpdate()
        {

        }
    }
}
