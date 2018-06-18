using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Default;
using NUnit.Framework;

namespace Tests {
    public class ActionTests : ODataTestsBase {

        [Test]
        public void FunctionTest() {
            Container container = GetODataContainer();
            decimal sales2017 = container.TotalSalesByYear(2017).GetValue();
            decimal sales2018 = container.TotalSalesByYear(2018).GetValue();

            Assert.AreEqual(0, sales2017);
            Assert.AreEqual(3501.55m, sales2018);
        }
    }
}
