using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using FlightControlWeb.Data;
using System.Text.Json;
using System.Globalization;

namespace FlightControlWeb.Models
{
    public class SqilteManagementModel : IDataManagementModel
    {
        private IDatabaseContext database;

        public SqilteManagementModel(IDatabaseContext db)
        {
            database = db;
        }

        // Return a list of all the active flights in this server.
        public async Task<List<Flight>> GetFlights(DateTime currTime)
        {
            DateTime correctFormatDate;
            var flightPlans = await database.GetAllFlightPlans();
            var currFlights = new List<Flight>();
            if (flightPlans == null)
            {
                return null;
            }
            foreach (FlightPlan currFlight in flightPlans)
            {
                // Convert date time to it's right format.
                var date = currFlight.InitialLocation.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
                                        DateTimeStyles.RoundtripKind, out correctFormatDate);
                currFlight.InitialLocation.DateTime = correctFormatDate;
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
            int serverError = 0;
            int validationError = 0;
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
            HttpClient client = new HttpClient();
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
                    var flights = JsonSerializer.Deserialize<List<Flight>>(responseBody);
                    // Add the new flights with their server to database, And update is_external.
                    validationError += await UpdateAndAdd(flights, url);
                    // Add the flights to the list.
                    allFlights.AddRange(flights);
                }
                catch(JsonException)
                {
                    // Change flag so we will know that somthing went wrong.
                    validationError++;
                }
                catch (Exception)
                {
                    // Change flag so we will know that somthing went wrong.
                    serverError++;
                }
            }
            // Somthing went wrong with some of the servers.
            if (serverError != 0)
            {
                allFlights.Insert(0, new Flight { FlightId = "server" });
            }
            // there are invalid flights.
            else if (validationError != 0)
            {
                allFlights.Insert(0, new Flight { FlightId = "valid" });
            }
            return allFlights;
        }

        /* 
         * Add the new flight with their server to data base,
         * And update is_external to true.
         */
        private async Task<int> UpdateAndAdd(List<Flight> flights, string url)
        {
            int validationError = 0;
            var newFlights = new List<FlightWithServer>();
            var flightFromServer = new List<Flight>(flights);
            if (flights == null)
            {
                return 0;
            }
            foreach (Flight flight in flightFromServer)
            {
                // Check if the flight is valid.
                if (!IsValidFlight(flight))
                {
                    // Invalid flight so we remove it, change flag so we will know that
                    // not all the flights were valid and continue to the next.
                    flights.Remove(flight);
                    validationError++;
                    continue;
                }
                // Valid flight- we add (if dont exist) it's id and serverURL to db,
                // and change is_external to true.
                flight.IsExternal = true;
                newFlights.Add(new FlightWithServer
                {
                    FlightId = flight.FlightId,
                    ServerURL = url
                });
            }
            await database.AddFlightsFromServers(newFlights);
            return validationError;
        }

        // Add Flight Plan to database and return the unique id.
        public async Task<string> AddFlightPlan(FlightPlan flightPlan)
        {
            // Check if the object is valid.
            if (!IsValidFlightPlan(flightPlan))
            {
                return "bad";
            }
            // If is valid, return it's id.
            return await database.AddFlightPlan(flightPlan);
        }

        // Return the flight plan with this id.
        public async Task<FlightPlan> GetFlightPlan(string id)
        {
            // Try to Find the flight plan from my database. if couldn't find, null will be sent.
            var flightPlan = await database.GetFlightPlan(id);
            if (flightPlan != null)
            {
                // Convert date time to it's right format.
                DateTime correctFormatDate;
                string date = flightPlan.InitialLocation.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
                                        DateTimeStyles.RoundtripKind, out correctFormatDate);
                flightPlan.InitialLocation.DateTime = correctFormatDate;
                return flightPlan;
            }
            // Check if the flight plan is from other servers, and get the server url.
            string serverUrl = await database.GetServerOfFlight(id);
            if (serverUrl == null)
            {
                // Didn't find the flight plan.
                return new FlightPlan { CompanyName = "?"};
            }
            HttpClient client = new HttpClient();
            try
            {
                // Get the flight plan from the server.
                HttpResponseMessage response = await client
                    .GetAsync(serverUrl + "/api/FlightPlan/" + id);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Convert response to flight plan object.
                flightPlan = JsonSerializer.Deserialize<FlightPlan>(responseBody);
            }
            catch (Exception)
            {
                // Problem with the server.
                return null;
            }
            // Invalid flight plan.
            if (!IsValidFlightPlan(flightPlan))
            {
                return new FlightPlan { CompanyName = "!" };
            }
            return flightPlan;
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
            if (server.ServerURL.EndsWith('/'))
            {
                server.ServerURL = server.ServerURL.Substring(0, server.ServerURL.Length - 1);
            }
            // If the addition was successful return the id of the server.
            if (await database.AddServer(server))
            {
                return server.ServerId;
            }
            // If not, it means that the key wasn't unique - return "key".
            return "key";
        }

        // Delete a server, return false if wasn't found.
        public async Task<bool> DeleteServer(string id)
        {
            Server s = await database.GetServer(id);
            // Delete the flight's ids from that server.
            if (s != null)
            {
                await database.DeleteFlightsFromServer(s.ServerURL);
                return await database.DeleteServer(id);
            }
            return false;
        }

        // Check validation of flight.
        private bool IsValidFlight(Flight flight)
        {
            if (flight == null)
            {
                return false;
            }
            if (flight.FlightId == null ||
                flight.Passengers < 0 ||
                flight.CompanyName == null)
            {
                return false;
            }
            // Check that the longitude and latitude in the right range.
            if (flight.Latitude <= -90 || flight.Latitude >= 90 ||
                flight.Longitude <= -180 || flight.Longitude >= 180)
            {
                return false;
            }
            return true;
        }

        // Check validation of flight plan.
        private bool IsValidFlightPlan(FlightPlan flightPlan)
        {
            if (flightPlan == null)
            {
                return false;
            }
            if (flightPlan.Passengers < 0 ||
                flightPlan.CompanyName == null ||
                flightPlan.InitialLocation == null ||
                flightPlan.Segments == null)
            {
                return false;
            }
            // Check that the longitude and latitude in the right range.
            if ( flightPlan.InitialLocation.Latitude <= -90 ||
                flightPlan.InitialLocation.Latitude >= 90 ||
                flightPlan.InitialLocation.Longitude <= -180 ||
                flightPlan.InitialLocation.Longitude >= 180) 
            {
                return false;
            }
            FlightSegment previous = null;
            int lastTimespWasZero = 0;
            // For each segment check its validation.
            foreach (FlightSegment segment in flightPlan.Segments)
            {
                if (segment.Latitude <= -90 || segment.Latitude >= 90 ||
                    segment.Longitude <= -180 || segment.Longitude >= 180)
                {
                    return false;
                }
                // Can't have negative time.
                if (segment.TimespanSeconds < 0)
                {
                    return false;
                }
                // If the previous timespan was 0, it valid only if the next segment
                // start in the same place.
                if (lastTimespWasZero > 0)
                {
                    if (segment.Latitude != previous.Latitude ||
                        segment.Longitude != previous.Longitude)
                    {
                        return false;
                    }
                    lastTimespWasZero = 0;
                }
                // Raise flag so for the next segment we will check it's start location.
                if (segment.TimespanSeconds == 0)
                {
                    lastTimespWasZero++;
                }
                previous = segment;
            }
            return true;
        }
    }
}
