using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private IDataManagementModel model;

        public FlightsController(IDataManagementModel m)
        {
            this.model = m;
            //model.AddDatabase(db);
        }

        // GET: api/Flight
        [HttpGet]
        public List<Flight> Get([FromQuery] DateTime relative_to)
        {
            DateTime currTime = TimeZoneInfo.ConvertTimeToUtc(relative_to);

            string s = Request.QueryString.Value;
            if (s.Contains("sync_all"))
            {
                return model.GetFlights(currTime);
            }
            return model.GetAllFlights(currTime);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            model.DeleteFlight(id);
        }
    }
}
