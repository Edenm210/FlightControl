using FlightControlWeb.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTestFlightPlan
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SqilteManagementModel model = new SqilteManagementModel();
            model.AddDatabase(stubDB);
            List<FlightPlan> fp = model.GetFlights();
        }
    }
}
