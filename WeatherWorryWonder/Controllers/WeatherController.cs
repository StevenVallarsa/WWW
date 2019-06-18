using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeatherWorryWonder.Models;

namespace WeatherWorryWonder.Controllers
{
    public class WeatherController : Controller
    {
        // GET: Weather

        public ActionResult Index()

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

            //double O3 = (double)Session["O3"];
            double FutureAQI1Day = (5.3 * weatherTime[1].WindSpeed) + (0.4 * weatherTime[1].TemperatureC) + (0.1 * weatherTime[1].Humidity) + (0.7 * 150);

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

    }
}