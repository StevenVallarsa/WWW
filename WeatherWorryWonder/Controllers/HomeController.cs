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
        WWWEntities db = new WWWEntities();

        public ActionResult Index()
        {
            //JToken weather = WeatherAPIDAL.Json();
            
            //double whatever = PollutantController.CalculateAQI((double)0.0, (double)0.088);

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
            double AQIForO3 = 0;
            double UGM3 = 0;
            double futureAQIForO3 = 0;

            ResultView rv = new ResultView();

            // get user location in lat/long
            List<double> userLocation = GeocodeController.ParseUserLocation(streetAddress);
            Session["UserLocation"] = userLocation;

            //grabs the closest sensor to your address
            List<Sensor> closestSensors = GeocodeController.ShortestToLongest(userLocation);
            //got all our weather info here
            List<double> twoClosestAQIs = new List<double>();
            List<double> ForecastedAQI = new List<double>();
            List<double> PollutantAQIs = new List<double>();

            //skips sensor if data is unreliable
            for (int i = 0; i < closestSensors.Count; i++)
            {
                if(twoClosestAQIs.Count == 2)
                {
                    break;
                }
                Sensor closestSensor = closestSensors[i];
                if(i == 0)
                {
                    Session["ClosestSensor"] = closestSensor;
                }

                //get sensor readings from OST and SIMMS
                PollutantController.PullData(closestSensor);
                PollutantController.PollutantListReadings(60);
                PollutantController.PollutantAverageReadings(closestSensor);

                //List<Pollutant> pollutants
                //List<Ost_Data_Feb_Apr_Final> OSTPollutantData
                //List<Ost_Data_Feb_Apr_Final> SelectedOstReadings
                //List<simms_Data_Feb_Apr_Final> SimmsPollutantData
                //List<simms_Data_Feb_Apr_Final> SelectedSimmsReadings
                //List<double> PollutantAverages
                //List<double> PollutantAQIs

                double eightHrPollutantPPM = PollutantController.eighthourO3;
                double oneHrPollutantPPM = PollutantController.PollutantAverages[0];

                //index zero = index of model pollutant and index one = whether we use one or eight hour
                List<int> indexAndOneorEight = PollutantController.EightorOneHour(oneHrPollutantPPM, eightHrPollutantPPM);

                if (eightHrPollutantPPM > 0.125)
                {
                    // using 1h reading
                    AQIForO3 = PollutantController.CalculateO3AQI(oneHrPollutantPPM, indexAndOneorEight[0], indexAndOneorEight[1]);
                }
                else
                {
                    // using 8h reading
                    AQIForO3 = PollutantController.CalculateO3AQI(eightHrPollutantPPM, indexAndOneorEight[0], indexAndOneorEight[0]);
                }

                twoClosestAQIs.Add(AQIForO3);

                int recommendationIndex = PollutantController.EPABreakpointTable(eightHrPollutantPPM);
                string recommendation = OzoneRecommendations.OzoneLevels[recommendationIndex];
                rv.Recommendations = recommendation;

                    if(twoClosestAQIs.Count < 2)
                    {
                        rv.SensorName = closestSensor.CrossStreet;
                        UGM3 = PollutantController.ConvertToUGM3(PollutantController.eighthourO3);
                        for (int j = 0; j < 4; j++)
                        {
                            // using weather data to forecast tomorrow's AQI (index 1 = 24h)
                            double futureWeatherForO3 = WeatherController.WeatherForecastEquation(weather, j, UGM3);
                            // convert from UG/M3 to PPM 
                            double futureAQIO3PPM = PollutantController.UGM3ConvertToPPM(futureWeatherForO3, 48);
                            int EPABreakpointIndex = PollutantController.EPABreakpointTable(futureAQIO3PPM);
                            double FutureAQIForO3 = PollutantController.CalculateO3AQI(futureAQIO3PPM, EPABreakpointIndex, indexAndOneorEight[0]);
                            // add future AQIs to list
                            ForecastedAQI.Add(FutureAQIForO3);
                        }

                        double c2H4O = PollutantController.ShortestDistancePollutantSensor(userLocation);
                        rv.PollutantWarning = PollutantController.PollutantWarning(c2H4O);
                        rv.C2H4OPPM = c2H4O;

                        if(closestSensor.Name.Contains("graq"))
                        {
                            rv.PM25AQI = PollutantController.PollutantAverages[2]; ; //PM25
                        }
                        else
                        {
                            rv.PM25AQI = PollutantController.PollutantAverages[4]; //PM25
                            rv.NO2AQI = PollutantController.PollutantAverages[3]; //NO2
                            rv.SO2AQI = PollutantController.PollutantAverages[5]; //SO2
                            rv.CO = PollutantController.PollutantAverages[2]; //CO
                        }
                    }
            }

            rv.O3AQI = twoClosestAQIs[0];
            rv.Second03AQI = twoClosestAQIs[1];
            //rv.Third03AQI = twoClosestAQIs[2];
            rv.PredictedAQITomorrow = ForecastedAQI[1];
            rv.PredictedAQI3Day = ForecastedAQI[2];
            rv.PredictedAQI5Day = ForecastedAQI[3];
            rv.Weather = weather;

            double EPAAQI = PollutantController.EPAAQIData();
            int breakpointIndex = PollutantController.EPABreakpointTable(EPAAQI);
            double AQIForEPA = PollutantController.CalculateEPA(EPAAQI, breakpointIndex);

            Recommendations(AQIForO3);
            Recommendations(futureAQIForO3);

            rv.EpaAQI = AQIForEPA;
            ViewBag.AQI = AQIForO3;

            return View(rv);
        }



        // being called from AQI ActionResult and stored in ViewBags to call along to view
        public void Recommendations(double reading)
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
    
