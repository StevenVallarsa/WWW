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
            //bringing in windspeed, temperature JTokens etc. from the API 
            JToken weather = WeatherAPIDAL.Json();

            // Forecast (with an 'e') readings are every 3h: 8=1 day, 24=3days, 39=5days minus 3h
            List<int> indexes = new List<int>() { 0, 8, 24, 39 };

            List<WeatherDataFromAPI> weatherTime = new List<WeatherDataFromAPI>();

            //converting temperature from Kelvin to Celsius and Fahrenheit
            foreach (int index in indexes)
            {
                WeatherDataFromAPI wd = new WeatherDataFromAPI(weather, index);
                wd.TemperatureC = wd.TemperatureK - 273.15;
                wd.TemperatureF = (wd.TemperatureC) * 9 / 5 + 32;

                weatherTime.Add(wd);
            }

            return weatherTime;
        }

        //index 1 = next day forecast 24 hrs
        //index 2 = 3 day
        //index 3 = 5 day
        public static double WeatherForecastEquation(List<WeatherDataFromAPI> weatherTime, int index, double eightHourO3)
        {
            //double O3 = (double)Session["O3"];

            // double O3 = (double)Session["O3"]
            // eightHourO3 is used because 8h readings are required for the equation
            // input & output of this equation is UG/M3
            double FutureAQI1Day = (double)(5.3 * weatherTime[index].WindSpeed) + (double)(0.4 * weatherTime[index].TemperatureC) + 
                (double)(0.1 * weatherTime[index].Humidity) + ((double)0.7 * eightHourO3);

            return FutureAQI1Day;
        }
    }
}