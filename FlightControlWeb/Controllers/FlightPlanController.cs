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
    public class FlightPlanController : ControllerBase
    {
        private IDataManagementModel model;

        public FlightPlanController(IDataManagementModel m)
        {
            this.model = m;
            //model.AddDatabase(db);
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public FlightPlan Get(string id)
        {
            return model.GetFlightPlan(id);
        }

        // POST: api/FlightPlan
        [HttpPost]
        public string Post([FromBody] FlightPlan value)
        {
            return model.AddFlightPlan(value);
        }
    }
}
