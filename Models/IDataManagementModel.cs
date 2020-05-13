using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IDataManagementModel
    {
        //public void AddDatabase(Data.DatabaseContext db);
        List<Flight> GetFlights(DateTime currTime);
        List<Flight> GetAllFlights(DateTime currTime);
        string AddFlightPlan(FlightPlan fp);
        FlightPlan GetFlightPlan(string id);
        void DeleteFlight(string id);
        List<Server> GetAllServers();
        void AddServer(Server server);
        void DeleteServer(string id);
    }
}
