using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;

namespace WebApplication1.Models {

    [Persistent("Contracts")]
    public class Contract : BaseDocument {

        public Contract(Session session) : base(session) { }
        public Contract() {
        }
        public override void AfterConstruction() { base.AfterConstruction(); }

        string fNumber;
        public string Number {
            get { return fNumber; }
            set { SetPropertyValue<string>(nameof(Number), ref fNumber, value); }
        }

        Customer fCustomerID;
        [Size(5)]
        [Association(@"ContractsReferencesCustomers")]
        [Persistent("CustomerID")]
        public Customer Customer {
            get { return fCustomerID; }
            set { SetPropertyValue<Customer>(nameof(Customer), ref fCustomerID, value); }
        }
    }

}
