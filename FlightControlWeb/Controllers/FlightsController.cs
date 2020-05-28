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
    public class FlightsController : ControllerBase
    {
        private IDataManagementModel model;

        public FlightsController(IDatabaseContext db, IDataManagementModel m)
        {
            this.model = m;
        }

        // GET: api/Flights
        [HttpGet]
        public async Task<ActionResult<Flight>> Get([FromQuery] DateTime relative_to)
        {
            var currTime = TimeZoneInfo.ConvertTimeToUtc(relative_to);
            List<Flight> flights;
            string s = Request.QueryString.Value;
            if (s.Contains("sync_all"))
            {
                // Get the flights from the db and all the servers.
                flights = await model.GetAllFlights(currTime);
            }
            else
            {
                //Get the flight only from the db.
                flights = await model.GetFlights(currTime);
            }
            // There are no flights in this current time.
            if (flights == null || flights.Count == 0)
            {
                return NoContent();
            }
            // There were problems with connecting to some of the servers. 
            if (flights[0].FlightId.Equals("server"))
            {
                flights.RemoveAt(0);
                return StatusCode(500, flights);
            }
            // There were validation problems with some of the flights.
            if (flights[0].FlightId.Equals("valid"))
            {
                flights.RemoveAt(0);
                return BadRequest(flights);
            }
            return Ok(flights);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            // Return "not found" if couldn't find the flight.
            bool response = await model.DeleteFlight(id);
            if (response == false)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
