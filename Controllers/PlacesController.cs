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
using System.Text;

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
        public async Task<ActionResult<List<Place>>> List([FromQuery]string keyword)
        {
            List<Place> place = null;

            // Get Redis cache
            var inCache = _distributedCache.GetString(keyword);
            if (!string.IsNullOrEmpty(inCache)) {
                var getData = await _distributedCache.GetAsync(keyword);
                var bytesAsString = Encoding.UTF8.GetString(getData);
                place = JsonConvert.DeserializeObject<List<Place>>(bytesAsString);

                return Ok(place);
            }   
           
            // Get Google map api
            var googleMapApiUrl = _configuration["GoogleMapApi:Url"];
            var googleMapApiKey = _configuration["GoogleMapApi:Key"];            

            string url = googleMapApiUrl + "/place/textsearch/json?query=restaurants+in+" + keyword + "&key=" + googleMapApiKey;
            var client = new HttpClient();
            var result = await client.GetStringAsync(String.Format(url));        

            var jsonObject = JsonConvert.DeserializeObject<RootPlace>(result);
            place = jsonObject.results;
            
            // Set Redis cache
            var expirationMinutes = Convert.ToDouble(_configuration["Redis:ExpirationMinutes"]);
            string serializeObject = JsonConvert.SerializeObject(place);
            byte[] data = Encoding.UTF8.GetBytes(serializeObject);
            await _distributedCache.SetAsync(keyword, data, new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expirationMinutes)
            });

            return Ok(place);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlaceDetail>> Show(string id)
        {
            PlaceDetail place = null;

            // Get Redis cache
            // var inCache = _distributedCache.GetString(id);
            // if (!string.IsNullOrEmpty(inCache)) {
            //     var getData = await _distributedCache.GetAsync(id);
            //     var bytesAsString = Encoding.UTF8.GetString(getData);
            //     place = JsonConvert.DeserializeObject<Place>(bytesAsString);

            //     return Ok(place);
            // }   
           
            // Get Google map api
            var googleMapApiUrl = _configuration["GoogleMapApi:Url"];
            var googleMapApiKey = _configuration["GoogleMapApi:Key"];            

            string url = googleMapApiUrl + "/place/details/json?placeid=" + id + "&key=" + googleMapApiKey;
            var client = new HttpClient();
            var result = await client.GetStringAsync(String.Format(url));        

            var jsonObject = JsonConvert.DeserializeObject<RootPlaceDetail>(result);
            place = jsonObject.result;
            
            // Set Redis cache
            // var expirationMinutes = Convert.ToDouble(_configuration["Redis:ExpirationMinutes"]);
            // string serializeObject = JsonConvert.SerializeObject(place);
            // byte[] data = Encoding.UTF8.GetBytes(serializeObject);
            // await _distributedCache.SetAsync(id, data, new DistributedCacheEntryOptions()
            // {
            //     AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expirationMinutes)
            // });

            return Ok(place);
        }
    }
}
