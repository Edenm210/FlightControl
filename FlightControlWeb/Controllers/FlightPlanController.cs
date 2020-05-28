using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Data;
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

        public FlightPlanController(IDatabaseContext db, IDataManagementModel m)
        {
            this.model = m;
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public async Task<ActionResult<FlightPlan>> Get(string id)
        {
            // Return server error if there were problems with the server that has that flight.
            var flightplan = await model.GetFlightPlan(id);
            if (flightplan == null)
            {
                return StatusCode(500, "Problem With The Server That Has The Flight Plan");
            }
            // Return couldn't find the flight plan.
            if (flightplan.CompanyName.Equals("?"))
            {
                return NotFound("Could Not Found The Flight Plan");
            }
            // Return bad request because the flight plan is invalid.
            if (flightplan.CompanyName.Equals("!"))
            {
                return BadRequest("Invalid Flight Plan");
            }
            return Ok(flightplan);
        }

        // POST: api/FlightPlan
        [HttpPost]
        public async Task<ActionResult<string>> Post([FromBody] FlightPlan value)
        {
            // Return "bad repuest" if there were problem with creation (like missing fields),
            // else return "created" with the id in header and the object in the body. 
            string response = await model.AddFlightPlan(value);
            if (response.Equals("bad"))
            {
                return BadRequest("Invalid Flight Plan");
            }
            // Convert date time to it's right format.
            DateTime correctFormatDate;
            string date = value.InitialLocation.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            DateTime.TryParseExact(date, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
                                    DateTimeStyles.RoundtripKind, out correctFormatDate);
            value.InitialLocation.DateTime = correctFormatDate;
            return Created(response, value);
        }
    }
}
