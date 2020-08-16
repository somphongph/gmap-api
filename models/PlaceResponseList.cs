using Newtonsoft.Json;

namespace netcore_google_map.Models
{
    public class PlaceResponseList
    {        
        [JsonProperty("place_id")]
        public string id { get; set; }

        public string name { get; set; }
        public Geometry geometry { get; set; }
    }    
}