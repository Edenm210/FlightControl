using FlightControlWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Data
{
    public interface IDatabaseContext
    {
        Task<List<FlightPlan>> GetAllFlightPlans();
        Task<FlightPlan> GetFlightPlan(string id);
        Task<string> AddFlightPlan(FlightPlan fp);
        Task<bool> DeleteFlightPlan(string id);
        Task<List<Server>> GetServers();
        Task<bool> AddServer(Server server);
        Task<bool> DeleteServer(string id);
        List<FlightWithServer> LoadAllFlightsWithServers();
        Task AddFlightsFromServers(List<FlightWithServer> fws);
        Task DeleteFlightsFromServer(string serverURL);
        Task<Server> GetServer(string id);
        Task<string> GetServerOfFlight(string id);
    }
}
