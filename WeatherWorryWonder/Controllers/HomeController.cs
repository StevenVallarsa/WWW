using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeatherWorryWonder.Controllers;
using WeatherWorryWonder.Models;

namespace WeatherWorryWonder.Controllers
{
    public class HomeController : Controller
    {
        WeatherWorryWonderDBEntities db = new WeatherWorryWonderDBEntities();

        public ActionResult Index()
        {
            //JToken weather = WeatherAPIDAL.Json();
            
            //decimal whatever = PollutantController.CalculateAQI((decimal)0.0, (decimal)0.088);

            //WeatherDataFromAPI wd = new WeatherDataFromAPI(weather);

            //Session["weather"] = wd.Temperature;

            return View();
        }

        public ActionResult EnterAddress()
        {
            return View();
        }

        public ActionResult AQI()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AQI(string streetAddress)
        {

            //index zero = index of model pollutant and index one = whether we use one or eight hour
            List<WeatherDataFromAPI> weather = WeatherController.WeatherData();
            decimal AQIForO3 = 0;
            decimal UGM3 = 0;
            decimal futureAQIForO3 = 0;

            ResultView rv = new ResultView();

            // get user location in lat/long
            List<double> userLocation = GeocodeController.ParseUserLocation(streetAddress);
            Session["UserLocation"] = userLocation;

            //grabs the closest sensor to your address
            List<Sensor> closestSensors = GeocodeController.ShortestToLongest(userLocation);
            //got all our weather info here
            List<decimal> threeClosestAQIs = new List<decimal>(); 

            List<decimal> FutureAQIForO3ThreeAndFiveDays = new List<decimal>();

            List<decimal> PollutantAQIs = new List<decimal>();

            //skips sensor if data is unreliable
            for (int i = 0; i < closestSensors.Count; i++)
            {
                if(threeClosestAQIs.Count == 3)
                {
                    break;
                }
                Sensor closestSensor = closestSensors[i];
                if(i == 0)
                {
                    Session["ClosestSensor"] = closestSensor;
                }
                //Session["ClosestSensor"] = closestSensor;

                //get sensor readings from OST and SIMMS
                decimal eightHrPollutantPPM = PollutantController.PollutantDataReading(closestSensor, 480)[0];
                decimal oneHrPollutantPPM = PollutantController.PollutantDataReading(closestSensor, 60)[0];
                //if((eightHrPollutantPPM != null || oneHrPollutantPPM != null) && (eightHrPollutantPPM < 5 && oneHrPollutantPPM)
                //index zero = index of model pollutant and index one = whether we use one or eight hour
                List<int> indexAndOneorEight = PollutantController.EightorOneHour(oneHrPollutantPPM, eightHrPollutantPPM);

                if (oneHrPollutantPPM > (decimal)0.125)
                {
                    // using 1h reading
                    AQIForO3 = PollutantController.CalculateO3AQI(oneHrPollutantPPM, indexAndOneorEight[0], indexAndOneorEight[1]);

                }
                else
                {
                    // using 8h reading
                    AQIForO3 = PollutantController.CalculateO3AQI(eightHrPollutantPPM, indexAndOneorEight[0], indexAndOneorEight[1]);

                }

                int recommendationIndex = PollutantController.EPABreakpointTable(eightHrPollutantPPM);
                string recommendation = OzoneRecommendations.OzoneLevels[recommendationIndex];
                rv.Recommendations = recommendation;
                //if O3AQI is less than 5, it's bad data
                if (AQIForO3 > 5)
                {
                    threeClosestAQIs.Add(AQIForO3);
                    if(threeClosestAQIs.Count < 2)
                    {
                        rv.SensorName = closestSensor.CrossStreet;
                        UGM3 = PollutantController.ConvertToUGM3(eightHrPollutantPPM);
                        for (int j = 0; j < 4; j++)
                        {
                            // using weather data to forecast tomorrow's AQI (index 1 = 24h)
                            decimal futureWeatherForO3 = WeatherController.WeatherForecastEquation(weather, j, UGM3);
                            // convert from UG/M3 to PPM 
                            decimal futureAQIO3PPM = PollutantController.UGM3ConvertToPPM(futureWeatherForO3);
                            int EPABreakpointIndex = PollutantController.EPABreakpointTable(futureAQIO3PPM);
                            decimal FutureAQIForO3 = PollutantController.CalculateO3AQI(futureAQIO3PPM, EPABreakpointIndex, indexAndOneorEight[0]);
                            // add future AQIs to list
                            FutureAQIForO3ThreeAndFiveDays.Add(FutureAQIForO3);

                        }
                        decimal c2H4O = PollutantController.ShortestDistancePollutantSensor(userLocation);
                        rv.PollutantWarning = PollutantController.PollutantWarning(c2H4O);
                        rv.C2H4OPPM = c2H4O;

                        PollutantAQIs = PollutantController.PollutantDataReading(closestSensor, 1);
                        if(closestSensor.Name.Contains("graq"))
                        {
                            rv.PM25AQI = PollutantAQIs[1];
                        }
                        else
                        {
                            //MorePollutantDataReading.Add(SimsO3Average);   //index[0]
                            //MorePollutantDataReading.Add(SimsCOToAverage);   //index[1]
                            //MorePollutantDataReading.Add(SimsNO2Average);     //index[2]
                            //MorePollutantDataReading.Add(SimsNO2_O3Average);    //index[3]
                            //MorePollutantDataReading.Add(SimsPM25Average);      //index[4]
                            //MorePollutantDataReading.Add(SimsSO2Average);      //index[5]
                            rv.PM25AQI = PollutantAQIs[4];
                            rv.NO2AQI = PollutantAQIs[2];
                            rv.SO2AQI = PollutantAQIs[5];
                            rv.CO = PollutantAQIs[1];
                        }
                    }
                }


            }

            //pull list out and attach it to rv
         
            rv.O3AQI = threeClosestAQIs[0];
            rv.Second03AQI = threeClosestAQIs[1];
            rv.Third03AQI = threeClosestAQIs[2];
            rv.PredictedAQITomorrow = FutureAQIForO3ThreeAndFiveDays[1];
            rv.PredictedAQI3Day = FutureAQIForO3ThreeAndFiveDays[2];
            rv.PredictedAQI5Day = FutureAQIForO3ThreeAndFiveDays[3];
            rv.Weather = weather;

            decimal EPAAQI = PollutantController.EPAAQIData();
            int breakpointIndex = PollutantController.EPABreakpointTable(EPAAQI);
            decimal AQIForEPA = PollutantController.CalculateEPA(EPAAQI, breakpointIndex);

            Recommendations(AQIForO3);
            Recommendations(futureAQIForO3);

            rv.EpaAQI = AQIForEPA;
            ViewBag.AQI = AQIForO3;

            return View(rv);
        }



        // being called from AQI ActionResult and stored in ViewBags to call along to view
        public void Recommendations(decimal reading)
        {
            if (reading >= 0 && reading < 51)
            {
                //hex green
                ViewBag.AQIColor = "#42f445";
                ViewBag.Suggestions = "It’s a great day to be active outside.";

            }
            else if (reading >= 51 && reading < 101)
            {
                //hex yellow
                ViewBag.AQIColor = "#f4f142";
                ViewBag.Suggestions = "Unusually sensitive people: Consider reducing prolonged or heavy outdoor exertion. Watch for symptoms such as coughing or shortness of breath. These are signs to take it a little easier.";
            }
            else if (reading >= 101 && reading < 151)
            {
                //hex orange
                ViewBag.AQIColor = "#f49241";
                ViewBag.Suggestions = "Sensitive groups: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities. Watch for symptoms such as coughing or shortness of breath. Schedule outdoor activities in the morning when ozone is lower. People with asthma should follow their asthma action plans and keep quick relief medicine handy.";
            }
            else if (reading >= 151 && reading < 201)
            {
                //hex red
                ViewBag.AQIColor = "#42f445";
                ViewBag.Suggestions = "Sensitive groups: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower. Consider moving activities indoors. People with asthma, keep quick-relief medicine handy. Everyone else: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities.Schedule outdoor activities in the morning when ozone is lower.";
            }
            else if (reading >= 201 && reading < 301)
            {
                //hex purple
                ViewBag.AQIColor = "#a100f1";
                ViewBag.Suggestions = "Sensitive groups: Avoid all physical activity outdoors. Move activities indoors or reschedule to a time when air quality is better. People with asthma, keep quick-relief medicine handy. Everyone else: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower.Consider moving activities indoors.";
            }
            else
            {
                //hex maroon
                ViewBag.AQIColor = "#800000";
                ViewBag.Suggestions = "	Everyone: Avoid all physical activity outdoors";
            }

        }

        // used for 
        public ActionResult ProcessAddress()
        {
            return View();
        }


        public ActionResult OzoneReductionTips(string option)   //Callista
        {

            if (option == "Homeprojects")
            {
                return View("Homeprojects");    //redirect to Homeprojects page
            }
            if (option == "Driving")
            {
                return View("Driving");   //redirect to Driving page
            }
            if (option == "Grilling")
            {
                return View("Grilling");   //redirect to Grilling page
            }
            if (option == "Office")
            {
                return View("Office");   //redirect to Office page
            }
            if (option == "Yardwork")
            {
                return View("Yardwork");   //redirect to Yardwork page
            }


            return View("Index");
        }   
    }
}
    
