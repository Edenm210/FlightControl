using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class SqilteManagementModel : IDataManagementModel
    {
        public static Dictionary<string, FlightPlan> FlightPlans;
        public static Dictionary<string, Server> Servers;

        public SqilteManagementModel()
        {
            FlightPlans = new Dictionary<string, FlightPlan>();
            Servers = new Dictionary<string, Server>();
        }

        // Return a list of all the active flights in this server.
        public List<Flight> GetFlights(DateTime currTime)
        {
            List<Flight> currFlights = new List<Flight>();
            foreach (KeyValuePair<string, FlightPlan> flightP in FlightPlans)
            {
                FlightPlan currFlight = flightP.Value;
                //compare the starting time of the flight with the given time
                int compTime = DateTime.Compare(currTime, currFlight.Initial_Location.Date_Time);
                if (compTime == 0) //starting time is exactly the given time
                {
                    currFlights.Add(new Flight(flightP.Key, currFlight.Initial_Location.Longitude,
                        currFlight.Initial_Location.Latitude, currFlight.Passengers,
                        currFlight.Company_Name, currFlight.Initial_Location.Date_Time));
                }
                else if (compTime > 0) //flight starts before requested time
                {
                    IsCurrFlight(currTime, currFlight, flightP.Key, currFlights);
                }
            }
            return currFlights;
        }

        /*
         * find the segment of the flight where the plane is at the given time and add an 
         * approriate Flight object to the list.
         * if the flight ends before the given time will add nothing
         */
        private void IsCurrFlight(DateTime currTime, FlightPlan flightP, string id,
            List<Flight> currFlights)
        {
            List<FlightSegment> segments = flightP.Segments;
            DateTime endTime = flightP.Initial_Location.Date_Time;
            int numOfSegments = segments.Count; //number of segments for this flight
            for (int i = 0; i < numOfSegments; i++)
            {
                //add to filght time the time spent in this segment
                endTime = endTime.AddSeconds(segments[i].Timespan_Seconds);
                int compTime = DateTime.Compare(currTime, endTime);
                if (compTime == 0) //finished the segment at the given time
                {

                    Flight flight = new Flight(id, segments[i].Longitude, segments[i].Latitude,
                        flightP.Passengers, flightP.Company_Name, 
                        flightP.Initial_Location.Date_Time);
                    currFlights.Add(flight);
                    return;
                }
                //the flight is in the middle of this segment at the time requested
                else if (compTime < 0)
                {
                    //get the location of the plane
                    Location location = FindLocation(flightP, i, currTime, endTime);
                    double longitude = Math.Round(location.Longitude, 3);
                    double latitude = Math.Round(location.Latitude, 3);
                    Flight flight = new Flight(id, longitude, latitude,
                        flightP.Passengers, flightP.Company_Name, 
                        flightP.Initial_Location.Date_Time);
                    currFlights.Add(flight);
                    return;
                }
            }
            //if reached this line the flight ended before the given time
        }

        /*
         * calculate the location of the plane in the current segment and return an object with 
         * the location
         */
        private Location FindLocation(FlightPlan flightP, int currSegment, DateTime currTime,
            DateTime endTime)
        {
            Location prevLocation;
            if (currSegment == 0) //flight is in the first segment at the requested time
            {
                prevLocation = new Location(flightP.Initial_Location.Longitude,
                    flightP.Initial_Location.Latitude);
            }
            else
            {
                prevLocation = new Location(flightP.Segments[currSegment - 1].Longitude,
                    flightP.Segments[currSegment - 1].Latitude);
            }

            FlightSegment segment = flightP.Segments[currSegment];
            //the difference between the end time of the segment and the time that we need
            double diffInSeconds = (endTime - currTime).TotalSeconds;
            //total time spent in the segment
            double timePassed = segment.Timespan_Seconds - diffInSeconds;
            //the relative amount of the segment that the plane passed
            double proportion = timePassed / segment.Timespan_Seconds;
            double longiLen = segment.Longitude - prevLocation.Longitude; //total longitude length of the segment
            double longiPassed = proportion * longiLen; //longitued length passed
            double currLongi = prevLocation.Longitude + longiPassed; //new longitude location
            double latiLen = segment.Latitude - prevLocation.Latitude; //total latitude length of the segment
            double latiPassed = proportion * latiLen; //latitude length passed
            double currLati = prevLocation.Latitude + latiPassed; //new latitude location
            return new Location(currLongi, currLati);
        }

        // Return a list of all the active flights in all the servers. 
        public List<Flight> GetAllFlights(DateTime currTime)
        {
            return GetFlights(currTime);
        }

        // Add Flight Plan to database and return the unique id.
        public string AddFlightPlan(FlightPlan fp)
        {
            string id = GetUniqueId(fp);
            FlightPlans.Add(id, fp);
            return id;
        }

        // Return the flight plan with this id.
        public FlightPlan GetFlightPlan(string id)
        {
            try
            {
                FlightPlan fp = FlightPlans[id];
                return fp;
            } 
            catch(KeyNotFoundException)
            {
                return null;
            }
        }

        // Delete the flight plan with this id.
        public void DeleteFlight(string id)
        {
            FlightPlans.Remove(id);
        }

        // Return a list of all the servers that we use.
        public List<Server> GetAllServers()
        {
            Server s = new Server { ServerId = "14", ServerURL = "www" };
            List<Server> allservers =  new List<Server>();
            foreach (KeyValuePair<string, Server> serv in Servers)
            {
                allservers.Add(serv.Value);
            }
            return allservers; 
        }

        // Add a server.
        public void AddServer(Server server)
        {
            Servers.Add(server.ServerId, server);
        }

        // Delete a server.
        public void DeleteServer(string id)
        {
            Servers.Remove(id);
        }

        private string GetUniqueId(FlightPlan fp)
        {
            return fp.Passengers.ToString() + fp.Company_Name + fp.Initial_Location.Latitude.ToString();
        }
    }
}
