using FlightControlWeb.Data;
using FlightControlWeb.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestFlightPlan
{
    class StubDB : IDatabaseContext
    {
        List<FlightPlan> flights = new List<FlightPlan>();

        public Task<string> AddFlightPlan(FlightPlan fp) //add a flightPlan to the list
        {
            flights.Add(fp);
            return null;
        }

        public Task AddFlightsFromServers(List<FlightWithServer> fws)
        {
            throw new NotImplementedException();
        }

        public Task AddServer(Server server)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFlightPlan(string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFlightsFromServer(string serverURL)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteServer(string id)
        {
            throw new NotImplementedException();
        }

        //return the list of flights in a Task
        public Task<List<FlightPlan>> GetAllFlightPlans()
        {
            var tcs = new TaskCompletionSource<List<FlightPlan>>();
            tcs.SetResult(flights);
            return tcs.Task;
        }

        public Task<FlightPlan> GetFlightPlan(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Server> GetServer(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Server>> GetServers()
        {
            throw new NotImplementedException();
        }

        public List<FlightWithServer> LoadAllFlightsWithServers()
        {
            return new List<FlightWithServer>();
        }
    }
}
