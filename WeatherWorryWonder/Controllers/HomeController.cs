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
            List<Sensor> threeSensors = new List<Sensor>();

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
                if (twoClosestAQIs.Count == 2)
                {
                    break;
                }
                Sensor closestSensor = closestSensors[i];
                if (i == 0)
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

                //if O3AQI is less than 5, it's bad data
                if (AQIForO3 > 5)
                {
                    threeSensors.Add(closestSensor);
                    twoClosestAQIs.Add(AQIForO3);

                    int recommendationIndex = PollutantController.EPABreakpointTable(eightHrPollutantPPM);
                    string recommendation = OzoneRecommendations.OzoneLevels[recommendationIndex];
                    rv.Recommendations = recommendation;

                    if (twoClosestAQIs.Count < 2)
                    {
                        List<double> historicData = PollutantController.HistoricData(closestSensor);
                        rv.HistoricData = historicData;
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

                        if (closestSensor.Name.Contains("graq"))
                        {
                            int pm25BreakpointIndex = PollutantController.BreakpointIndex(PollutantController.PollutantAverages[2], 3);
                            rv.PM25AQI = Math.Round(PollutantController.CalculateEPA(PollutantController.PollutantAverages[2], pm25BreakpointIndex, 3));
                        }
                        else
                        {
                            int pm25BreakpointIndex = PollutantController.BreakpointIndex(PollutantController.PollutantAverages[4],3); 
                            rv.PM25AQI = Math.Round(PollutantController.CalculateEPA(PollutantController.PollutantAverages[4], pm25BreakpointIndex,3));

                            int no2BreakpointIndex = PollutantController.BreakpointIndex(PollutantController.PollutantAverages[3], 6); 
                            rv.NO2AQI = Math.Round(PollutantController.CalculateEPA(PollutantController.PollutantAverages[3], no2BreakpointIndex, 6)); 

                            int so2BreakpointIndex = PollutantController.BreakpointIndex(PollutantController.PollutantAverages[5], 5); 
                            rv.SO2AQI = Math.Round(PollutantController.CalculateEPA(PollutantController.PollutantAverages[5], so2BreakpointIndex, 5));

                            int coBreakpointIndex = PollutantController.BreakpointIndex(PollutantController.PollutantAverages[2], 4);
                            rv.CO = Math.Round(PollutantController.CalculateEPA(PollutantController.PollutantAverages[2], coBreakpointIndex, 4));
                           
                        }
                    }
                }
            }

            rv.TwoClosestSensors = threeSensors;
            rv.O3AQI = Math.Round(twoClosestAQIs[0]);
            rv.Second03AQI = Math.Round(twoClosestAQIs[1]);
            //rv.Third03AQI = twoClosestAQIs[2];
            rv.PredictedAQITomorrow = Math.Round(ForecastedAQI[1]);
            rv.PredictedAQI3Day = Math.Round(ForecastedAQI[2]);
            rv.PredictedAQI5Day = Math.Round(ForecastedAQI[3]);
            rv.Weather = weather;
            rv.Factorylat = PollutantController.pollutantlat;
            rv.Factorylong = PollutantController.pollutantlong;

            double EPAAQI = PollutantController.EPAAQIData();
            int breakpointIndex = PollutantController.EPABreakpointTable(EPAAQI);
            double AQIForEPA = PollutantController.CalculateEPA(EPAAQI, breakpointIndex);

            rv.AQIColor1 = ColorWarning(twoClosestAQIs[0]);
            rv.AQIColor2 = ColorWarning(twoClosestAQIs[1]);

            rv.EpaAQI = Math.Round(AQIForEPA);
            ViewBag.AQI = AQIForO3;
            Session["ResultView"] = rv;

            return View(rv);
        }

        public static string ColorWarning(double reading)
        {
            if (reading >= 0 && reading < 51)
            {
                return "limegreen";
            }
            else if (reading >= 51 && reading < 101)
            {
                return "yellow";
            }
            else if (reading >= 101 && reading < 151)
            {
                return "orange";
            }
            else if (reading >= 151 && reading < 201)
            {
                return "red";
            }
            else if (reading >= 201 && reading < 301)
            {
                return "purple";
            }
            else
            {
                return "maroon";
            }

        }
        public static string ColorWarningEO(double reading)
        {
            if (reading >= 0 && reading < 0.18)
            {
                return "limegreen";
            }
            else if (reading >= 0.18 && reading <= 1)
            {
                return "yellow";
            }
            else
            {
                return "red";
            }
        }

        public ActionResult OtherAQI()
        {
            return View();
        }

        public ActionResult ProcessAddress()
        {
            return View();
        }

        public ActionResult OzoneRecommendationTips()
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
    
