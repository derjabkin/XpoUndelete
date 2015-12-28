using DevExpress.Xpo;

namespace XpoRecoverDeleted
{
    public class DetailObject : XPObject
    {
        public DetailObject(Session session) : base(session)
        {
        }



        private MasterObject master;
        [Association]
        public MasterObject Master
        {
            get { return master; }
            set { SetPropertyValue("Master", ref master, value); }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                SetPropertyValue(nameof(Name), ref name, value);
            }
        }

    }
}