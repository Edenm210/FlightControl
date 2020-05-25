using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using FlightControlWeb.Data;

namespace FlightControlWeb.Models
{
    public class SqilteManagementModel : IDataManagementModel
    {
        private IDatabaseContext database;
        private HttpClient client;
        Dictionary<string, string> flightsWithServers;
        int flagg;

        public SqilteManagementModel(/*DatabaseContext db*/)
        {
            //database = new DatabaseContext();
            client = new HttpClient();
            flightsWithServers = new Dictionary<string, string>();
            flagg = 0;
            //LoadDictionaryFromDB();
        }

        // Load all tha flight's ids and servers from db.
        private void LoadDictionaryFromDB()
        {
            var flightWithServers = database.LoadAllFlightsWithServers();
            if (flightWithServers != null)
            {
                foreach (FlightWithServer flight in flightWithServers)
                {
                    flightsWithServers.Add(flight.FlightId, flight.ServerURL);
                }
            }
        }

        public void AddDatabase(IDatabaseContext db)
        {
            database = db;
            if (flagg == 0)
            {
                LoadDictionaryFromDB();
                flagg++;
            }
        }

        // Return a list of all the active flights in this server.
        public async Task<List<Flight>> GetFlights(DateTime currTime)
        {
            var flightPlans = await database.GetAllFlightPlans();
            var currFlights = new List<Flight>();
            if (flightPlans == null)
            {
                return null;
            }
            foreach (FlightPlan currFlight in flightPlans)
            {
                // Compare the starting time of the flight with the given time.
                int compTime = DateTime.Compare(currTime, currFlight.InitialLocation.DateTime);
                // Starting time is exactly the given time.
                if (compTime == 0)
                {
                    currFlights.Add(new Flight(currFlight.FlightId,
                        currFlight.InitialLocation.Longitude,
                        currFlight.InitialLocation.Latitude, currFlight.Passengers,
                        currFlight.CompanyName, currFlight.InitialLocation.DateTime));
                }
                // Flight starts before requested time.
                else if (compTime > 0)
                {
                    IsCurrFlight(currTime, currFlight, currFlights);
                }
            }
            return currFlights;
        }

        /*
         * Find the segment of the flight where the plane is at the given time and add an 
         * approriate Flight object to the list.
         * If the flight ends before the given time will add nothing.
         */
        private void IsCurrFlight(DateTime currTime, FlightPlan flightP, List<Flight> currFlights)
        {
            var segments = flightP.Segments;
            var endTime = flightP.InitialLocation.DateTime;
            // Number of segments for this flight.
            int numOfSegments = segments.Count;
            for (int i = 0; i < numOfSegments; i++)
            {
                // Add to filght time the time spent in this segment.
                endTime = endTime.AddSeconds(segments[i].TimespanSeconds);
                int compTime = DateTime.Compare(currTime, endTime);
                // Finished the segment at the given time.
                if (compTime == 0)
                {
                    var flight = new Flight(flightP.FlightId, segments[i].Longitude,
                        segments[i].Latitude, flightP.Passengers, flightP.CompanyName,
                        flightP.InitialLocation.DateTime);
                    currFlights.Add(flight);
                    return;
                }
                // The flight is in the middle of this segment at the time requested.
                else if (compTime < 0)
                {
                    // Get the location of the plane.
                    var location = FindLocation(flightP, i, currTime, endTime);
                    double longitude = Math.Round(location.Longitude, 3);
                    double latitude = Math.Round(location.Latitude, 3);
                    var flight = new Flight(flightP.FlightId, longitude, latitude,
                    flightP.Passengers, flightP.CompanyName, flightP.InitialLocation.DateTime);
                    currFlights.Add(flight);
                    return;
                }
            }
            // If reached this line the flight ended before the given time.
        }

        /*
         * Calculate the location of the plane in the current segment and return an object with 
         * the location.
         */
        private Location FindLocation(FlightPlan flightP, int currSegment, DateTime currTime,
            DateTime endTime)
        {
            Location prevLocation;
            // Flight is in the first segment at the requested time.
            if (currSegment == 0)
            {
                prevLocation = new Location(flightP.InitialLocation.Longitude,
                    flightP.InitialLocation.Latitude);
            }
            else
            {
                prevLocation = new Location(flightP.Segments[currSegment - 1].Longitude,
                    flightP.Segments[currSegment - 1].Latitude);
            }

            var segment = flightP.Segments[currSegment];
            // The difference between the end time of the segment and the time that we need.
            double diffInSeconds = (endTime - currTime).TotalSeconds;
            // Total time spent in the segment.
            double timePassed = segment.TimespanSeconds - diffInSeconds;
            // The relative amount of the segment that the plane passed.
            double proportion = timePassed / segment.TimespanSeconds;
            // Total longitude length of the segment.
            double longiLen = segment.Longitude - prevLocation.Longitude;
            // Longitued length passed.
            double longiPassed = proportion * longiLen;
            // New longitude location.
            double currLongi = prevLocation.Longitude + longiPassed;
            // Total latitude length of the segment.
            double latiLen = segment.Latitude - prevLocation.Latitude;
            // Latitude length passed.
            double latiPassed = proportion * latiLen;
            // New latitude location.
            double currLati = prevLocation.Latitude + latiPassed;
            return new Location(currLongi, currLati);
        }

        // Return a list of all the active flights in all the servers. 
        public async Task<List<Flight>> GetAllFlights(DateTime currTime)
        {
            string time = currTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var servers = await database.GetServers();
            var allFlights = new List<Flight>();
            int flag = 0;
            // Add the flights from our db, according to current time.
            var flightsFromDb = await GetFlights(currTime);
            if (flightsFromDb != null)
            {
                allFlights.AddRange(flightsFromDb);
            }
            if (servers == null)
            {
                return allFlights;
            }
            // Get the flights from the servers.
            foreach (Server server in servers)
            {
                string url = server.ServerURL;
                try
                {
                    // Connecting to servers.
                    HttpResponseMessage response = await client
                        .GetAsync(url + "/api/Flights?relative_to=" + time);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Convert response to flight plan object.
                    List<Flight> flights = JsonConvert
                        .DeserializeObject<List<Flight>>(responseBody);
                    // Add to dictionary and db new flights, and change is_external.
                    flag += await UpdateChanges(flights, url);
                    // Add the flights to the list.
                    allFlights.AddRange(flights);
                }
                catch (Exception e)
                {
                    // Change flag so we will know that somthing went wrong.
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    flag++;
                }
            }
            // Somthing went wrong with some of the flights or servers.
            if (flag != 0)
            {
                allFlights.Insert(0, new Flight { FlightId = "bad" });
            }
            return allFlights;
        }

        /* 
         * Add the new flight with their server to dictionary and data base,
         * And change is_external to true.
         */
        private async Task<int> UpdateChanges(List<Flight> flights, string url)
        {
            int flag = 0;
            var newFlights = new List<FlightWithServer>();
            var flightFromServer = new List<Flight>(flights);
            if (flights == null)
            {
                return 0;
            }
            foreach (Flight flight in flightFromServer)
            {
                // Check if the json has all the needed fields.
                if (flight.FlightId == null ||
                    flight.Passengers == 0 ||
                    flight.CompanyName == null)
                {
                    // Invalid flight so we remove it, change flag so we will know that
                    // not all the flights were valid and continue to the next.
                    flights.Remove(flight);
                    flag++;
                    continue;
                }
                // Valid flight- we add (if dont exist) it's id and serverURL to dictionart and db,
                // and change is_external to true.
                flight.IsExternal = true;
                if (!flightsWithServers.ContainsKey(flight.FlightId))
                {
                    flightsWithServers.Add(flight.FlightId, url);
                    newFlights.Add(new FlightWithServer
                    {
                        FlightId = flight.FlightId,
                        ServerURL = url
                    });
                }
            }
            await database.AddFlightsFromServers(newFlights);
            return flag;
        }

        // Add Flight Plan to database and return the unique id.
        public async Task<string> AddFlightPlan(FlightPlan flightPlan)
        {
            // Check if the object has all the needed fields.
            if (flightPlan == null)
            {
                return "bad";
            }
            if (flightPlan.Passengers == 0)
            {
                return "bad";
            }
            if (flightPlan.CompanyName == null)
            {
                return "bad";
            }
            if (flightPlan.Segments == null)
            {
                return "bad";
            }
            if (flightPlan.InitialLocation == null)
            {
                return "bad";
            }
            // if the initial location is empty but not null it's fields will have default values.
            return await database.AddFlightPlan(flightPlan);
        }

        // Return the flight plan with this id.
        public async Task<FlightPlan> GetFlightPlan(string id)
        {
            // Check if the flight plan is from other servers.
            if (flightsWithServers.ContainsKey(id))
            {
                // Get the server url from the dictionary.
                string url = flightsWithServers[id];
                try
                {
                    // Get the flight plan from the server.
                    HttpResponseMessage response = await client
                        .GetAsync(url + "/api/FlightPlan/" + id);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    // Convert response to flight plan object.
                    return JsonConvert.DeserializeObject<FlightPlan>(responseBody);
                }
                catch (Exception e)
                {
                    // Problem with find the flight plan. 
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                    return null;
                }
            }
            // Find the flight plan from my database. if couldn't find, null will be sent.
            else
            {
                return await database.GetFlightPlan(id);
            }
        }

        // Delete the flight plan with this id.
        public async Task<bool> DeleteFlight(string id)
        {
            // If couldn't find the flight false will be sent.
            return await database.DeleteFlightPlan(id);
        }

        // Return a list of all the servers that we use, if there aren't any return null.
        public async Task<List<Server>> GetAllServers()
        {
            return await database.GetServers();
        }

        // Add a server.
        public async Task<string> AddServer(Server server)
        {
            // Check if the object has all the needed fields.
            if (server.ServerId == null)
            {
                return "bad";
            }
            if (server.ServerURL == null)
            {
                return "bad";
            }
            await database.AddServer(server);
            return server.ServerId;
        }

        // Delete a server.
        public async Task<bool> DeleteServer(string id)
        {
            // Return true if found the server and deleted it, else false.
            Server s = await database.GetServer(id);
            // Delete the flight's ids from that server.
            if (s != null)
            {
                await DeleteServerFlights(s.ServerURL);
                return await database.DeleteServer(id);
            }
            return false;
        }

        // Update the dictionary and db when server has been deleted.
        private async Task DeleteServerFlights(string url)
        {
            // Delete the flights that are from that server in dictionary.
            foreach (KeyValuePair<string, string> flight in flightsWithServers)
            {
                if (flightsWithServers.ContainsValue(url))
                {
                    flightsWithServers.Remove(flight.Key);
                }
            }
            // Do the same for the db.
            await database.DeleteFlightsFromServer(url);
        }
    }
}
