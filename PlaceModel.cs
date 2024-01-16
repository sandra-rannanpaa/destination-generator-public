using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace destination_generator_webapp.Models
{
    public class Place
    {
        public Place()
        {

        }

        // from placelist repsonse model

        [JsonProperty(PropertyName = "type")]
        public string type;

        [JsonProperty(PropertyName = "id")]
        public int id;

        public string name { get; set; }

        public string kinds { get; set; }
        public double[] coordinatesarray { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }

        [JsonProperty(PropertyName = "geometry")]
        public Geometry geometry;

        public class Geometry
        {
            [JsonProperty(PropertyName = "type")]
            public string type;

            [JsonProperty(PropertyName = "coordinates")]
            public List<float> coordinates;
            
        }

        [JsonProperty(PropertyName = "properties")]
        public Properties properties;

        public class Properties
        {
            [JsonProperty(PropertyName = "xid")]
            public string xid;

            [JsonProperty(PropertyName = "name")]
            public string name;

            [JsonProperty(PropertyName = "rate")]
            public string rate;

        }

      
    }   
}
