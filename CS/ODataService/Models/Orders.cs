using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;

namespace WebApplication1.Models {

    [Persistent("Orders")]
    public class Order : XPLiteObject {

        public Order(Session session) : base(session) { }
        public Order() {
        }
        public override void AfterConstruction() { base.AfterConstruction(); }

        int fOrderID;
        [Key(true)]
        public int OrderID {
            get { return fOrderID; }
            set { SetPropertyValue<int>(nameof(OrderID), ref fOrderID, value); }
        }

        OrderStatus fOrderStatus;
        public OrderStatus OrderStatus {
            get { return fOrderStatus; }
            set { SetPropertyValue<OrderStatus>(nameof(OrderStatus), ref fOrderStatus, value); }
        }

        Customer fCustomerID;
        [Size(5)]
        [Association(@"OrdersReferencesCustomers")]
        [Persistent("CustomerID")]
        public Customer Customer {
            get { return fCustomerID; }
            set { SetPropertyValue<Customer>(nameof(Customer), ref fCustomerID, value); }
        }

        DateTime? fOrderDate;
        [Indexed(Name = @"OrderDate")]
        public DateTime? OrderDate {
            get { return fOrderDate; }
            set { SetPropertyValue<DateTime?>(nameof(OrderDate), ref fOrderDate, value); }
        }

        [Association(@"OrdersReferencesOrderDetails")]
        [Aggregated]
        public XPCollection<OrderDetail> OrderDetails { get { return GetCollection<OrderDetail>(nameof(OrderDetails)); } }
    }

}
