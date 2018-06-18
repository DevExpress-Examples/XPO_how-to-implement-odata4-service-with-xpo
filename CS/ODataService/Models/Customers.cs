using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;

namespace WebApplication1.Models {

    [Persistent("Customers")]
    public class Customer : XPLiteObject
    {
        public Customer(Session session) : base(session) { }
        public Customer() : base(XpoDefault.Session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        string fCustomerID;
        [Key]
        [Size(5)]
        [Nullable(false)]
        public string CustomerID {
            get { return fCustomerID; }
            set { SetPropertyValue<string>(nameof(CustomerID), ref fCustomerID, value); }
        }

        string fCompanyName;
        [Indexed(Name = @"CompanyName")]
        [Size(40)]
        [Nullable(false)]
        public string CompanyName {
            get { return fCompanyName; }
            set { SetPropertyValue<string>(nameof(CompanyName), ref fCompanyName, value); }
        }

        [Association(@"OrdersReferencesCustomers")]
        public XPCollection<Order> Orders { get { return GetCollection<Order>(nameof(Orders)); } }
    }

}
