using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models {
    public enum OrderStatus {
        New,
        InProgress,
        Completed,
        Cancelled
    }
}