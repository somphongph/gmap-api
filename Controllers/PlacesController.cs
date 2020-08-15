using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using netcore_google_map.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;

// using System.IO.TextWriter;

namespace netcore_google_map.Controllers
{
    [ApiController]
    [Route("/v1/places")]
    [Produces("application/json")]
    public class PlacesController : ControllerBase
    {

        private readonly ILogger<PlacesController> _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;

        public PlacesController(ILogger<PlacesController> logger, IDistributedCache distributedCache, IConfiguration configuration)
        {
            _logger = logger;
            _distributedCache = distributedCache;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<List<Place>>> Get([FromQuery]string keyword)
        {
            List<Place> place = null;
            var inCache = _distributedCache.GetString(keyword);
            if (!string.IsNullOrEmpty(inCache)) {
                return NotFound();
            } else {
                var googleMapApiUrl = _configuration["GoogleMapApi:Url"];
                var googleMapApiKey = _configuration["GoogleMapApi:Key"];

                string url = googleMapApiUrl + "/place/textsearch/json?query=restaurants+in+" + keyword + "&key=" + googleMapApiKey;
                var client = new HttpClient();
                var result = await client.GetStringAsync(String.Format(url));
            

                var jsonObject = JsonConvert.DeserializeObject<RootObject>(result);

                place = jsonObject.results; 
            }

            return Ok(place);
        }
    }
}
