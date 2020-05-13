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
    public class ServersController : ControllerBase
    {
        private IDataManagementModel model;

        public ServersController(IDataManagementModel m)
        {
            this.model = m;
            //model.AddDatabase(db);
        }

        // GET: api/Servers
        [HttpGet]
        public List<Server> GetAllServers()
        {
            return model.GetAllServers();
        }

        // POST: api/Servers
        [HttpPost]
        public void Post([FromBody] Server value)
        {
            model.AddServer(value);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            model.DeleteServer(id);
        }
    }
}
