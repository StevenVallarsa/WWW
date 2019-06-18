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
            JToken weather = WeatherAPIDAL.Json();

            decimal whatever = PollutantController.CalculateAQI((decimal)0.0, (decimal)0.088);

            //WeatherDataFromAPI wd = new WeatherDataFromAPI(weather);

            //Session["weather"] = wd.Temperature;

            return View((object)whatever);
        }

        public ActionResult EnterAddress()
        {
            return View();
        }

        public ActionResult ProcessAddress(string streetAddress)
        {
            JToken address = GoogleMapDAL.GoogleJson(streetAddress);
            //latitude and longitude from google maps geocode API
            double lat = double.Parse(address["results"][0]["geometry"]["location"]["lat"].ToString());
            double lng = double.Parse(address["results"][0]["geometry"]["location"]["lng"].ToString());


            Sensor closestSensor = GeocodeController.ShortestDistanceSensor(lat, lng);

            return View(closestSensor);
        }


        public ActionResult Result(Sensor s)
        {
            //example of what the string date looks like "2019 - 03 - 01T"            
            //take the current hour            
            string strB = DateTime.Now.ToString("HH");

            DateTime datevalue = (Convert.ToDateTime(strB.ToString()));
            string dy = datevalue.Day.ToString();


            //take the DeLorean and go back to a date in the past
            string currentTime = $"2019-03-{dy}T{strB}";

            string sensorLocation = s.Name;

            //take in the sensor that is closest to the user
            //string sensorLocation = "graqm0107";

            bool answer = (sensorLocation.Contains("graq"));
            //if sensor name contains ost
            if (answer == true)
            {
                List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
                ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
                    .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                    .First();

                int x = startingPoint.Id;
                //get 8 hr average AQI
                for (int i = 0; i < 480; i++)
                {
                    ost_data_Jan_June2019 AQIdata = db.ost_data_Jan_June2019.Find(x);
                    OSTData.Add(AQIdata);
                    x++;
                }

                //sum all the O3(ozone) AQI readings from the list
                decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => O3.o3));
                //average the AQI readings by dividing by number of readings
                decimal OST8HrAverage = OSTDataO3sum / OSTData.Count;

                Session["O3"] = OST8HrAverage;
                ViewBag.OST8HrAverage = OST8HrAverage;

                return View();
            }
            //if sensor name contains simms
            else
            {
                List<simms_data_Jan_June2019> simsData = new List<simms_data_Jan_June2019>();
                simms_data_Jan_June2019 startingPoint = db.simms_data_Jan_June2019
                    .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                    .First();

                int x = startingPoint.Id;
                //get 8 hr average AQI
                for (int i = 0; i < 480; i++)
                {
                    simms_data_Jan_June2019 AQIdata = db.simms_data_Jan_June2019.Find(x);
                    simsData.Add(AQIdata);
                    x++;
                }

                //sum all the O3(ozone) AQI readings from the list
                decimal SimsDataO3sum = Convert.ToDecimal(simsData.Sum(O3 => O3.o3));
                //average the AQI readings by dividing by number of readings
                decimal SimsTO8HrAverage = SimsDataO3sum / simsData.Count;

                return View();
            }
        }

        public ActionResult Recommendations(decimal reading)
        {
            if (reading >= 0 || reading <= 50)
            {
                //hex green
                ViewBag.AQIColor = "#42f445";
                ViewBag.Suggestions = "It’s a great day to be active outside.";

            }
            else if (reading >= 51 || reading <= 100)
            {
                //hex yellow
                ViewBag.AQIColor = "#f4f142";
                ViewBag.Suggestions = "Unusually sensitive people: Consider reducing prolonged or heavy outdoor exertion. Watch for symptoms such as coughing or shortness of breath. These are signs to take it a little easier.";
            }
            else if (reading >= 101 || reading <= 150)
            {
                //hex orange
                ViewBag.AQIColor = "#f49241";
                ViewBag.Suggestions = "Sensitive groups: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities. Watch for symptoms such as coughing or shortness of breath. Schedule outdoor activities in the morning when ozone is lower. People with asthma should follow their asthma action plans and keep quick relief medicine handy.";
            }
            else if (reading >= 151 || reading <= 200)
            {
                //hex red
                ViewBag.AQIColor = "#42f445";
                ViewBag.Suggestions = "Sensitive groups: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower. Consider moving activities indoors. People with asthma, keep quick-relief medicine handy. Everyone else: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities.Schedule outdoor activities in the morning when ozone is lower.";
            }
            else if (reading >= 201 || reading <= 300)
            {
                //hex purple
                ViewBag.AQIColor = "#a100f1";
                ViewBag.Suggestions = "Sensitive groups: Avoid all physical activity outdoors. Move activities indoors or reschedule to a time when air quality is better. People with asthma, keep quick-relief medicine handy. Everyone else: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower.Consider moving activities indoors.";
            }
            else if (reading >= 301 || reading <= 500)
            {
                //hex maroon
                ViewBag.AQIColor = "#800000";
                ViewBag.Suggestions = "	Everyone: Avoid all physical activity outdoors";
            }
            return View();

            //}
        }

        //possible double, commented out for now...
        //public ActionResult Result(Sensor s)
        //{
        //    //example of what the string date looks like "2019 - 03 - 01T"            
        //    //take the current hour            
        //    string strB = DateTime.Now.ToString("HH");

        //    DateTime datevalue = (Convert.ToDateTime(strB.ToString()));
        //    string dy = datevalue.Day.ToString();


        //    //take the DeLorean and go back to a date in the past
        //    string currentTime = $"2019-03-{dy}T{strB}";

        //    string sensorLocation = s.Name;

        //    //take in the sensor that is closest to the user
        //    //string sensorLocation = "graqm0107";

        //    bool answer = (sensorLocation.Contains("graq"));
        //    //if sensor name contains ost
        //    if (answer == true)
        //    {
        //        List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
        //        ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
        //            .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
        //            .First();

        //        int x = startingPoint.Id;
        //        //get 8 hr average AQI
        //        for (int i = 0; i < 480; i++)
        //        {
        //            ost_data_Jan_June2019 AQIdata = db.ost_data_Jan_June2019.Find(x);
        //            OSTData.Add(AQIdata);
        //            x++;
        //        }

        //        //sum all the O3(ozone) AQI readings from the list
        //        decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => O3.o3));
        //        //average the AQI readings by dividing by number of readings
        //        decimal OST8HrAverage = OSTDataO3sum / OSTData.Count;

        //        ViewBag.OST8HrAverage = OST8HrAverage;

        //        return View();
        //    }
        //    //if sensor name contains simms
        //    else
        //    {
        //        List<simms_data_Jan_June2019> simsData = new List<simms_data_Jan_June2019>();
        //        simms_data_Jan_June2019 startingPoint = db.simms_data_Jan_June2019
        //            .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
        //            .First();

        //        int x = startingPoint.Id;
        //        //get 8 hr average AQI
        //        for (int i = 0; i < 480; i++)
        //        {

        //            //needs an if statement incase there is no data
        //            simms_data_Jan_June2019 AQIdata = db.simms_data_Jan_June2019.Find(x);
        //            simsData.Add(AQIdata);
        //            x++;
        //        }

        //        //sum all the O3(ozone) AQI readings from the list
        //        decimal SimsDataO3sum = Convert.ToDecimal(simsData.Sum(O3 => O3.o3));
        //        //average the AQI readings by dividing by number of readings
        //        decimal SimsTO8HrAverage = SimsDataO3sum / simsData.Count;

        //        ViewBag.Sims8HrAverage = SimsTO8HrAverage;

        //        return View();
        //    }
        //}

        //public ActionResult Recommendations(decimal reading)
        //{
        //    if (reading >= 0 || reading <= 50)
        //    {
        //        //hex green
        //        ViewBag.AQIColor = "#42f445";
        //        ViewBag.Suggestions = "It’s a great day to be active outside.";

        //    }
        //    else if (reading >= 51 || reading <= 100)
        //    {
        //        //hex yellow
        //        ViewBag.AQIColor = "#f4f142";
        //        ViewBag.Suggestions = "Unusually sensitive people: Consider reducing prolonged or heavy outdoor exertion. Watch for symptoms such as coughing or shortness of breath. These are signs to take it a little easier.";
        //    }
        //    else if (reading >= 101 || reading <= 150)
        //    {
        //        //hex orange
        //        ViewBag.AQIColor = "#f49241";
        //        ViewBag.Suggestions = "Sensitive groups: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities. Watch for symptoms such as coughing or shortness of breath. Schedule outdoor activities in the morning when ozone is lower. People with asthma should follow their asthma action plans and keep quick relief medicine handy.";
        //    }
        //    else if (reading >= 151 || reading <= 200)
        //    {
        //        //hex red
        //        ViewBag.AQIColor = "#42f445";
        //        ViewBag.Suggestions = "Sensitive groups: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower. Consider moving activities indoors. People with asthma, keep quick-relief medicine handy. Everyone else: Reduce prolonged or heavy outdoor exertion. Take more breaks, do less intense activities.Schedule outdoor activities in the morning when ozone is lower.";
        //    }
        //    else if (reading >= 201 || reading <= 300)
        //    {
        //        //hex purple
        //        ViewBag.AQIColor = "#a100f1";
        //        ViewBag.Suggestions = "Sensitive groups: Avoid all physical activity outdoors. Move activities indoors or reschedule to a time when air quality is better. People with asthma, keep quick-relief medicine handy. Everyone else: Avoid prolonged or heavy outdoor exertion. Schedule outdoor activities in the morning when ozone is lower.Consider moving activities indoors.";
        //    }
        //    else if (reading >= 301 || reading <= 500)
        //    {
        //        //hex maroon
        //        ViewBag.AQIColor = "#800000";
        //        ViewBag.Suggestions = "	Everyone: Avoid all physical activity outdoors";
        //    }
        //    return View();

        //    //}
        //}

        public ActionResult Result()
        {
            ost_data_Jan_June2019 data = new ost_data_Jan_June2019();
            //string strA = "2019 - 03 - 01T";
            string strB = DateTime.Now.ToString("HH");
            //string currentTime = String.Concat(strA, strB);
            string currentTime = $"2019-03-15T{strB}";

            string sensorLocation = "graqm0107";

            List<ost_data_Jan_June2019> OSTData = db.ost_data_Jan_June2019.Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation).ToList();


            decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => O3.o3));
            decimal OSTO3hourAverage = OSTDataO3sum / OSTData.Count;

            decimal OSTDataPMsum = Convert.ToDecimal(OSTData.Sum(PM => PM.pm25));
            decimal OSTPMhourAverage = OSTDataPMsum / OSTData.Count;

            ViewBag.O3Houraverage = OSTO3hourAverage;
            ViewBag.PMHouraverage = OSTPMhourAverage;

            return View(OSTData);

        }
    }
}