using FlightControlWeb.Data;
using FlightControlWeb.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTestFlightPlan
{
    [TestClass]
    public class TestFlightsAtGivenTime
    {
        /*
         * test 2 flights, one is in the middle of a segment and one is at the end of a segment
         */
        [TestMethod]
        public void Test1GetFlights()
        {
            // Arrange
            IDatabaseContext stubDB = new StubDB();
            LoadFlights(stubDB);
            SqilteManagementModel model = new SqilteManagementModel(stubDB);
            

            //Act
            var dt = new DateTime(2020, 1, 1, 10, 30, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z
            var actualFlightList = model.GetFlights(dt).Result;
            var expectedFlightList = GetExpectedAnswer1();

            //Assert
            var actualJson = JsonConvert.SerializeObject(actualFlightList);
            var expectedJson = JsonConvert.SerializeObject(expectedFlightList);
            Assert.AreEqual(expectedJson, actualJson);
        }

        /*
         * test 2 flights, one is at the end of the flight and the other has finished
         */
        [TestMethod]
        public void Test2GetFlights()
        {
            // Arrange
            IDatabaseContext stubDB = new StubDB();
            LoadFlights(stubDB);
            SqilteManagementModel model = new SqilteManagementModel(stubDB);

            //Act
            var dt = new DateTime(2020, 1, 1, 11, 00, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z
            var actualFlightList = model.GetFlights(dt).Result;
            var expectedFlightList = GetExpectedAnswer2();

            //Assert
            var actualJson = JsonConvert.SerializeObject(actualFlightList);
            var expectedJson = JsonConvert.SerializeObject(expectedFlightList);
            Assert.AreEqual(expectedJson, actualJson);
        }

        private void LoadFlights(IDatabaseContext stubDB)
        {
            //first flight
            var dt1 = new DateTime(2020, 1, 1, 10,00, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z
            var initialLoc = new InitialFlightLocation();
            initialLoc.Longitude = 20;
            initialLoc.Latitude = 20;
            initialLoc.DateTime = dt1;
            var seg = new FlightSegment();
            seg.Latitude = 24;
            seg.Longitude = 24;
            seg.TimespanSeconds = 3600; //one hour later
            List<FlightSegment> segments = new List<FlightSegment>();
            segments.Add(seg);
            var f1 = new FlightPlan();
            f1.FlightId = "1";
            f1.CompanyName = "1";
            f1.Passengers = 1;
            f1.InitialLocation = initialLoc;
            f1.Segments = segments;
            stubDB.AddFlightPlan(f1);

            //second flight
            dt1 = new DateTime(2020, 1, 1, 10, 15, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z
            initialLoc = new InitialFlightLocation();
            initialLoc.Longitude = 30;
            initialLoc.Latitude = 30;
            initialLoc.DateTime = dt1;
            segments = new List<FlightSegment>();            
            var seg1 = new FlightSegment();
            seg1.Latitude = 31;
            seg1.Longitude = 31;
            seg1.TimespanSeconds = 900; //15 minites later
            segments.Add(seg1);
            var seg2 = new FlightSegment();
            seg2.Latitude = 32;
            seg2.Longitude = 32;
            seg2.TimespanSeconds = 900; //15 minites later
            segments.Add(seg2);
            var f2 = new FlightPlan();
            f2.FlightId = "2";
            f2.CompanyName = "2";
            f2.Passengers = 2;
            f2.InitialLocation = initialLoc;
            f2.Segments = segments;
            stubDB.AddFlightPlan(f2);
        }

        private List<Flight> GetExpectedAnswer1()
        {
            List<Flight> flights = new List<Flight>();
            DateTime dt1 = new DateTime(2020, 1, 1, 10, 00, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z           
            var f1 = new Flight("1", 22, 22, 1, "1", dt1);
            flights.Add(f1);

            DateTime dt2 = new DateTime(2020, 1, 1, 10, 15, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z
            var f2 = new Flight("2", 31, 31, 2, "2", dt2);
            flights.Add(f2);
            return flights;

        }

        private List<Flight> GetExpectedAnswer2()
        {
            List<Flight> flights = new List<Flight>();
            DateTime dt1 = new DateTime(2020, 1, 1, 10, 00, 00, DateTimeKind.Utc); //2020-01-01T10:00:00Z           
            var f1 = new Flight("1", 24, 24, 1, "1", dt1);
            flights.Add(f1);
            return flights;

        }
    }
}
