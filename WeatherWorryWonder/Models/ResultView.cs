using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class ResultView
    {
        public decimal O3AQI { get; set; }
        public decimal PM25AQI { get; set; }
        public decimal PM10AQI { get; set; }
        public decimal NO2AQI { get; set; }
        public decimal SO2AQI { get; set; }
        public decimal VOCData { get; set; }
        public decimal PredictedAQITomorrow { get; set; }
        public decimal PredictedAQI3Day { get; set; }
        public decimal PredictedAQI5Day { get; set; }
        public decimal EpaAQI { get; set; }
        public string SensorName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string Recommendations { get; set; }
        public List<WeatherDataFromAPI> Weather { get; set; }
    }
}