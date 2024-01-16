using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace destination_generator_webapp.Models
{
    public class PlaceListResponse
    {
        public PlaceListResponse()
        {
        }

        [JsonProperty(PropertyName = "features")] public List<Place> places;

        public class Place
        {
            [JsonProperty(PropertyName = "type")]
            public string type;

            [JsonProperty(PropertyName = "id")]
            public int id;

            [JsonProperty(PropertyName = "geometry")]
            public Geometry geometry;
            public class Geometry
            {
                [JsonProperty(PropertyName = "type")]
                public string type;

                [JsonProperty(PropertyName = "coordinates")]
                public List<double> coordinates;
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

                [JsonProperty(PropertyName = "kinds")]
                public string kinds;

            }

        }
        

    }
}
