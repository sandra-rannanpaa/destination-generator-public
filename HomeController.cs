using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using destination_generator_webapp.Helper;
using destination_generator_webapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace destination_generator_webapp.Controllers
{
    public class HomeController : Controller
    {
        HelperAPI _helperapi = new HelperAPI();
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult ChooseEmojis()
        {
            return View();
        }

        // the actual code and generation process
        [HttpPost]
        public async Task<IActionResult> GenerateDestinationAsync(string[] emojichoice)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
           
            
            List<Place> places1 = new List<Place>();
            PlaceListResponse placesresponse = new PlaceListResponse();
            HttpClient client = _helperapi.InitialOTM();
            string kind = emojichoice[0];
            string limit = "70";

            if(emojichoice.Count() == 1)
            {
                limit = "2";
            }
            // currently we have a min max lat lon
            HttpResponseMessage res1 = await client.GetAsync("/0.1/en/places/bbox?lon_min=-179&lon_max=179&lat_min=-89&lat_max=89&kinds=" + kind + "&limit=" + limit + "&apikey=PLACESAPIKEY");

            if (res1.IsSuccessStatusCode)
            {
                var result = res1.Content.ReadAsStringAsync().Result;
                placesresponse = JsonConvert.DeserializeObject<PlaceListResponse>(result);
                PlacesMethods pm = new PlacesMethods();
                places1 = pm.ConvertToListPlaces(placesresponse);


            }
            else
            {
                ViewBag.error = res1;
            }

            Place[] pair = new Place[2];

            Place[] shortestPair = new Place[2];

            if (emojichoice.Count() > 1)
            {
                kind = emojichoice[1];
               

                List<Place> places2 = new List<Place>();
                PlaceListResponse placesresponse2 = new PlaceListResponse();

                HttpResponseMessage res2 = await client.GetAsync("/0.1/en/places/bbox?lon_min=-179&lon_max=179&lat_min=-89&lat_max=89&kinds=" + kind + "&limit=" + limit + "&apikey=PLACESAPIKEY");

                if (res2.IsSuccessStatusCode)
                {
                    var result2 = res2.Content.ReadAsStringAsync().Result;
                    placesresponse2 = JsonConvert.DeserializeObject<PlaceListResponse>(result2);
                    PlacesMethods pm2 = new PlacesMethods();
                    places2 = pm2.ConvertToListPlaces(placesresponse2);

                }
                else
                {
                    ViewBag.error = res2;
                }
               
                // never using a pair with more than 2.0 coordinates difference
                double shortestDistance = 2;

                // preferably using a pair with this distance or lower
                double preferredDistance = 0.3;
                double currentDistance;
                bool stopLooping = false;

                foreach (Place plc1 in places1)
                {
                    foreach (Place plc2 in places2)
                    {
                        currentDistance = Math.Sqrt(Math.Abs(Math.Pow(plc1.lat - plc2.lat, 2)) + Math.Abs(Math.Pow(plc1.lng - plc2.lng, 2)));
                        if (currentDistance < preferredDistance)
                        {
                            // save pair
                            pair[0] = plc1;
                            pair[1] = plc2;
                            ViewBag.pair = pair[0].name + " and " + pair[1].name;
                            ViewBag.currentDistance = currentDistance;
                            stopLooping = true;
                           
                            break;
                        }
                        // else save the shortest one you could find
                        else if (currentDistance < shortestDistance)
                        {
                            // save as the shortest pair for now...
                            shortestPair[0] = plc1;
                            shortestPair[1] = plc2;
                            shortestDistance = currentDistance;
                            ViewBag.shortestDistance = shortestDistance;


                        }
                    }
                    if (stopLooping)
                    {
                        break;
                    }

                }
            }
            else
            {
                // if the user only chooses one emoji the pair will consist of the same place, this is just one of the places in the list
                pair[0] = places1.ElementAt(0);
                pair[1] = places1.ElementAt(0);
                ViewBag.pair = pair[0].name;
            }
           
            Destination dest = new Destination();

            if ((pair[0] != null && pair[1] != null) || (shortestPair[0] != null && shortestPair[1] != null))
            {
                // search the final destination based on these two coordinates from the two places
                // search first place using coordinates and google maps search places api
                MapsPlaceSearchResponse searchresponse = new MapsPlaceSearchResponse();
                HttpClient clientGoogle = _helperapi.InitialGoogle();

                if(pair[0] == null && pair[1] == null)
                {
                    pair[0] = shortestPair[0];
                    pair[1] = shortestPair[1];
                }
                // update
                int radius = 15000;
                // calculate middleground between both places
                Place middlePlace = new Place();
                middlePlace.lat = (pair[0].lat + pair[1].lat)/2;
                middlePlace.lng = (pair[0].lng + pair[1].lng)/2;

                HttpResponseMessage searchres = await clientGoogle.GetAsync("place/nearbysearch/json?location=" + middlePlace.lng + "," + middlePlace.lat + "&radius=" + radius + "&type=locality&key=GOOGLEAPIKEY");

                if (searchres.IsSuccessStatusCode)
                {
                    var searchresult = searchres.Content.ReadAsStringAsync().Result;
                    searchresponse = JsonConvert.DeserializeObject<MapsPlaceSearchResponse>(searchresult);
                    MapsPlaceSearchResponse.Result mpsrResult = new MapsPlaceSearchResponse.Result();

                    mpsrResult = searchresponse.results.ElementAt(0);
                   

                    string placeId = mpsrResult.place_id;
                    HttpResponseMessage placedetailres = await clientGoogle.GetAsync("place/details/json?place_id=" + placeId + "&fields=name,rating,formatted_address&key=GOOGLEAPIKEY");
                    string address;
                    if (placedetailres.IsSuccessStatusCode)
                    {
                        MapsPlaceDetailsResponse placedetailresponse = new MapsPlaceDetailsResponse();
                        
                        var placeresult = placedetailres.Content.ReadAsStringAsync().Result;
                        placedetailresponse = JsonConvert.DeserializeObject<MapsPlaceDetailsResponse>(placeresult);
                        MapsPlaceDetailsResponse.Result placedetailresult = new MapsPlaceDetailsResponse.Result();
                        address = placedetailresult.formatted_address;
                        address = placedetailresponse.result.formatted_address;
                        string vicinity = mpsrResult.vicinity;
                        ViewBag.vicinity = vicinity;
                        
                        // create destination                    
                        dest.de_lat = mpsrResult.geometry.location.lat;
                        dest.de_lng = mpsrResult.geometry.location.lng;
                        dest.de_name = address;
                    }
                    else
                    {
                        ViewBag.error = placedetailres;
                    }

                    

                }
                else
                {
                    ViewBag.error = searchres;
                }
                ViewBag.status = "OK";

            }
            else
            {
                ViewBag.status = "Sorry, we could not find a destination with these emojis, please try another combination";
            }
            
            return View(dest);
        }


      
        [HttpGet]
        public IActionResult GenerateDestination()
        {
            
            Destination noDest = new Destination();
            return View(noDest);
        }

    }
}
