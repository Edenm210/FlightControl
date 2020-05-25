using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class Server
    {
        [Key]
        [JsonPropertyName("serverId")]
        public string ServerId { get; set; }
        [JsonPropertyName("serverURL")]
        public string ServerURL { get; set; }
    }
}
