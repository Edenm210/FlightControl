using FlightControlWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IDataManagementModel
    {
        void AddDatabase(IDatabaseContext db);
        Task<List<Flight>> GetFlights(DateTime currTime);
        Task<List<Flight>> GetAllFlights(DateTime currTime);
        Task<string> AddFlightPlan(FlightPlan flightPlan);
        Task<FlightPlan> GetFlightPlan(string id);
        Task<bool> DeleteFlight(string id);
        Task<List<Server>> GetAllServers();
        Task<string> AddServer(Server server);
        Task<bool> DeleteServer(string id);
    }
}
