using DevExpress.Xpo;

namespace XpoRecoverDeleted
{
    public class MasterObject : XPObject
    {
        public MasterObject(Session session): base (session)
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

        [Association, Aggregated]
        public XPCollection<DetailObject> Details => GetCollection<DetailObject>(nameof(Details));
    }
}