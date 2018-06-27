using System;
using DevExpress.Xpo;
using WebApplication1.Models;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;

namespace ODataService.Helpers {

    public static class ConnectionHelper
    {
        static Type[] persistentTypes = new Type[] {
            typeof(Customer),
            typeof(OrderDetail),
            typeof(Order),
            typeof(Product)
        };
        public static Type[] GetPersistentTypes()
        {
            Type[] copy = new Type[persistentTypes.Length];
            Array.Copy(persistentTypes, copy, persistentTypes.Length);
            return copy;
        }
        public static string ConnectionString {
            get {
                return System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
        }
        public static UnitOfWork CreateSession() {
            return new UnitOfWork() { IdentityMapBehavior = IdentityMapBehavior.Strong };
        }
        public static IDataLayer CreateDataLayer(AutoCreateOption autoCreationOption, bool threadSafe) {
            var dictionary = new ReflectionDictionary();
            dictionary.NullableBehavior = NullableBehavior.ByUnderlyingType;
            dictionary.GetDataStoreSchema(persistentTypes);
            if(threadSafe) {
                var provider = XpoDefault.GetConnectionProvider(XpoDefault.GetConnectionPoolString(ConnectionString), autoCreationOption);
                return new ThreadSafeDataLayer(dictionary, provider);
            } else {
                var provider = XpoDefault.GetConnectionProvider(ConnectionString, autoCreationOption);
                return new SimpleDataLayer(dictionary, provider);
            }
        }
        public static void EnsureDatabaseCreated() {
            using(IDataLayer dataLayer = CreateDataLayer(AutoCreateOption.DatabaseAndSchema, false)) {
                using(UnitOfWork uow = new UnitOfWork(dataLayer)) {
                    uow.UpdateSchema(persistentTypes);
                }
            }
        }
    }

}
