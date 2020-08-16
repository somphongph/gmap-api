using System.Collections.Generic;

namespace netcore_google_map.Models
{
    public class RootResultList
    {  
        public List<PlaceResponseList> results { get; set; }  
        public string status { get; set; }  
    }  
}