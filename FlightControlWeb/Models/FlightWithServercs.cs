using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightWithServer
    {
        [Key]
        public string FlightId { get; set; }
        public string ServerURL { get; set; }
    }
}
