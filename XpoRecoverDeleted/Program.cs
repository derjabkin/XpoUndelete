using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System.Linq;
using System;
using DevExpress.Xpo.Metadata;
using DevExpress.Xpo.Helpers;
using System.Diagnostics;

namespace XpoRecoverDeleted
{
    class Program
    {
        static SimpleDataLayer dataLayer;
        static void Main(string[] args)
        {
            var store = XpoDefault.GetConnectionProvider("Data Source=.;Integrated Security=SSPI;Initial Catalog=XpoUndelete", DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            dataLayer = new SimpleDataLayer(store);
            XPAssociationList.DoNotSetAssociatedMemberToNullWhenRemovingDeletedObjectFromAssociationList = true;

            ClearDB();
            Debug.Assert(GetTotalRowsCount<MasterObject>() == 0 && GetTotalRowsCount<DetailObject>() == 0);

            CreateTestObject();
            Debug.Assert(GetTotalRowsCount<MasterObject>() == 1 && GetTotalRowsCount<DetailObject>() == 3);

            DeleteTestObject();
            Debug.Assert(GetTotalRowsCount<MasterObject>() == 0 && GetTotalRowsCount<DetailObject>() == 0);

            UnDeleteTestObject();
            Debug.Assert(GetTotalRowsCount<MasterObject>() == 1 && GetTotalRowsCount<DetailObject>() == 3);

        }

        private static int GetTotalRowsCount<T>()
        {
            using (var uow = new UnitOfWork(dataLayer))
            {
                string tableName = uow.GetClassInfo<T>().Table.Name;
                return (int)uow.ExecuteScalar($"select count(*) from [{tableName}] where GCRecord is null");
            }
        }

        private static void ClearDB()
        {
            using (var uow = new UnitOfWork(dataLayer))
            {
                uow.ExecuteScalar("Delete from DetailObject;Delete from MasterObject");
            }
        }
        private static void UnDeleteTestObject()
        {
            using (UnitOfWork uow = new UnitOfWork(dataLayer))
            {
                var collection = new XPCollection<MasterObject>(uow, GetDeletedCriteria());
                collection.SelectDeleted = true;
                foreach (XPBaseObject obj in collection)
                    UndeleteObject(obj);
                uow.CommitChanges();
            }
        }

        private static void DeleteTestObject()
        {
            using (UnitOfWork uow = new UnitOfWork(dataLayer))
            {
                var obj = (
                    from m in uow.Query<MasterObject>()
                    where m.Name == "Test"
                    select m).FirstOrDefault();
                uow.Delete(obj);
                uow.CommitChanges();
            }
        }

        private static void CreateTestObject()
        {
            using (UnitOfWork uow = new UnitOfWork(dataLayer))
            {
                MasterObject masterObject = new MasterObject(uow);
                masterObject.Name = "Test";
                masterObject.Details.Add(new DetailObject(uow) { Name = "Detail 1" });
                masterObject.Details.Add(new DetailObject(uow) { Name = "Detail 2" });
                masterObject.Details.Add(new DetailObject(uow) { Name = "Detail 3" });

                uow.CommitChanges();
            }
        }

        private static CriteriaOperator GetDeletedCriteria()
        {
            return CriteriaOperator.Parse("GCRecord is not null");
        }

        private static void UndeleteObject(XPBaseObject obj)
        {
            obj.SetMemberValue("GCRecord", null);
            foreach(XPMemberInfo memberInfo in obj.ClassInfo.AssociationListProperties)
            {
                XPBaseCollection collection = memberInfo.GetValue(obj) as XPBaseCollection;

                collection.Criteria = GetDeletedCriteria();
                if (collection != null)
                    collection.SelectDeleted = true;

                foreach (var item in collection.OfType<XPBaseObject>())
                    UndeleteObject(item);
            }
        }
    }
}