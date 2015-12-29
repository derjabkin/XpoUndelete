using DevExpress.Xpo;

namespace XpoRecoverDeleted
{
    public class AnotherMasterObject : XPObject
    {
        public AnotherMasterObject(Session session): base (session)
        {
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

        [Association]
        public XPCollection<MasterObject> Masters => GetCollection<MasterObject>(nameof(Masters));
    }
}