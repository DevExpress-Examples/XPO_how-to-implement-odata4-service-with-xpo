using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;

namespace WebApplication1.Models {

    [Persistent("OrderDetails")]
    public class OrderDetail : XPLiteObject {
        public OrderDetail(Session session) : base(session) { }
        public OrderDetail() { }

        int fOrderDetailID;
        [Key(true)]
        public int OrderDetailID {
            get { return fOrderDetailID; }
            set { SetPropertyValue<int>(nameof(OrderDetailID), ref fOrderDetailID, value); }
        }

        Order fOrder;
        [Association(@"OrdersReferencesOrderDetails")]
        [Persistent("OrderID")]
        public Order Order {
            get { return fOrder; }
            set { SetPropertyValue<Order>(nameof(Order), ref fOrder, value); }
        }

        Product fProduct;
        [Association(@"ProductsReferencesOrderDetails")]
        [Persistent("ProductID")]
        public Product Product {
            get { return fProduct; }
            set { SetPropertyValue<Product>(nameof(Product), ref fProduct, value); }
        }

        decimal fUnitPrice;
        [ColumnDbDefaultValue("(0)")]
        public decimal UnitPrice {
            get { return fUnitPrice; }
            set { SetPropertyValue<decimal>(nameof(UnitPrice), ref fUnitPrice, value); }
        }

        short fQuantity;
        [ColumnDbDefaultValue("(1)")]
        public short Quantity {
            get { return fQuantity; }
            set { SetPropertyValue<short>(nameof(Quantity), ref fQuantity, value); }
        }               
    }

}
