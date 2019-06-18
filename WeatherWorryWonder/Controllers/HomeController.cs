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

            //decimal whatever = PollutantController.CalculateAQI((decimal)0.0, (decimal)0.088);

            //WeatherDataFromAPI wd = new WeatherDataFromAPI(weather);

            //Session["weather"] = wd.Temperature;

            return View();
        }

        public ActionResult EnterAddress()
        {
            return View();
        }

        public ActionResult AQIView(string streetAddress)
        {
            Sensor closestSensor = GeocodeController.ClosestSensor(streetAddress);
            Session["ClosestSensor"] = closestSensor;
            decimal eightHrPollutantPPM = PollutantController.PollutantDataReading(closestSensor, 480);
            decimal oneHrPollutantPPM = PollutantController.PollutantDataReading(closestSensor, 60);
            decimal AQIForO3 = PollutantController.EightorOneHour(oneHrPollutantPPM, eightHrPollutantPPM);

            ResultView rv = new ResultView();

            rv.O3AQI = AQIForO3;

            return View(rv);
        }

        public ActionResult WeatherView()

        {

            JToken weather = WeatherAPIDAL.Json();

            // Forecast readings are every 3h: 8=1 day, 24=3days, 39=5days minus 3h
            List<int> indexes = new List<int>() { 0, 8, 24, 39 };

            List<WeatherDataFromAPI> weatherTime = new List<WeatherDataFromAPI>();

            foreach (int index in indexes)
            {
                WeatherDataFromAPI wd = new WeatherDataFromAPI(weather, index);
                wd.TemperatureC = wd.TemperatureK - 273.15;
                wd.TemperatureF = (wd.TemperatureC) * (9 / 5) + 32;

                weatherTime.Add(wd);
            }

            double FutureAQI1Day = (5.3 * weatherTime[1].WindSpeed) + (0.4 * weatherTime[1].TemperatureC) + (0.1 * weatherTime[1].Humidity) + (0.7 * 1.6);
            Session["AQI"] = FutureAQI1Day;

            Session["weather"] = weatherTime;
            return View(weatherTime);
        }

        public ActionResult Equation()
        {
            List<WeatherDataFromAPI> forecast = Session["weather"] as List<WeatherDataFromAPI>;
            //double O3 = (double)Session["O3"];


            double FutureAQI1Day = (5.3 * forecast[1].WindSpeed) + (0.4 * forecast[1].TemperatureC) + (0.1 * forecast[1].Humidity) + (0.7 * 1.6);
            Session["AQI"] = FutureAQI1Day;
            return View(FutureAQI1Day);
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

    }
}