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

        public static List<WeatherDataFromAPI> WeatherData()
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

            return weatherTime;
        }

        //index 1 = next day forecast 24 hrs
        //index 2 = 3 day
        //index 3 = 5 day
        public static decimal WeatherForecastEquation(List<WeatherDataFromAPI> weatherTime, int index, decimal eightHourO3)
        {
            //double O3 = (double)Session["O3"];
            decimal FutureAQI1Day = (decimal)(5.3 * weatherTime[index].WindSpeed) + (decimal)(0.4 * weatherTime[index].TemperatureC) + (decimal)(0.1 * weatherTime[index].Humidity) + ((decimal)0.7 * eightHourO3);

            return FutureAQI1Day;
        }
    }
}