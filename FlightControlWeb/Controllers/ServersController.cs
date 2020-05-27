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
    public class ServersController : ControllerBase
    {
        private IDataManagementModel model;

        public ServersController(/*Data.DatabaseContext db, */IDataManagementModel m)
        {
            this.model = m;
            //model.AddDatabase(db);
        }

        // GET: api/Servers
        [HttpGet]
        public async Task<ActionResult<List<Server>>> GetAllServers()
        {
            // Return "no content" if there were no servers.
            var servers = await model.GetAllServers();
            if (servers == null)
            {
                return NoContent();
            }
            return Ok(servers);
        }

        // POST: api/Servers
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Server value)
        {
            // Return "bad request" if there were problem with creation (like missing fields),
            // else return "created" with server id in header. 
            string response = await model.AddServer(value);
            if (response.Equals("bad"))
            {
                return BadRequest("Invalid Json");
            }
            if (response.Equals("key"))
            {
                return BadRequest("Not Unique Id");
            }
            return Created(response, value);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            // Return "not found" if couldn't find the server.
            bool response = await model.DeleteServer(id);
            if (response == false)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
