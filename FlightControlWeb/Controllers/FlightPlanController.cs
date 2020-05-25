using System;
using System.Collections.Generic;
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

        public FlightPlanController(Data.DatabaseContext db, IDataManagementModel m)
        {
            this.model = m;
            model.AddDatabase(db);
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "GetFlightPlan")]
        public async Task<ActionResult<FlightPlan>> Get(string id)
        {
            // Return null if not found, else return the flight plan.
            var fp = await model.GetFlightPlan(id);
           // Console.WriteLine("Passengers: {0}, Company name: {1}, longi: {2}, lati: {3}, date: {4}", fp.Passengers, fp.CompanyName,
           //     fp.InitialLocation.Longitude, fp.InitialLocation.Latitude, fp.InitialLocation.DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (fp == null)
            {
                return NotFound();
            }
            return Ok(fp);
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
                return BadRequest();
            }
            return Created(response, value);
        }
    }
}
