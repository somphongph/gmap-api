using System.Collections.Generic;
using Newtonsoft.Json;

namespace netcore_google_map.Models
{
    public class Place
    {        
        [JsonProperty("place_id")]
        public string placeId { get; set; }
        public string name { get; set; }
        public Geometry geometry { get; set; }
    }

    public class RootObject  
    {  
        public List<Place> results { get; set; }  
        public string status { get; set; }  
    }  
}