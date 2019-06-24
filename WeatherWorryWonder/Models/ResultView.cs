using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class ResultView
    {
        public string PollutantWarning{ get; set; }
        public float CO { get; set; }
        public float C2H4OPPM { get; set; }
        public float O3AQI { get; set; }
        public float Second03AQI { get; set; }
        public float Third03AQI { get; set;}
        public float PM25AQI { get; set; }
        public float NO2AQI { get; set; }
        public float SO2AQI { get; set; }
        public float PredictedAQITomorrow { get; set; }
        public float PredictedAQI3Day { get; set; }
        public float PredictedAQI5Day { get; set; }
        public float EpaAQI { get; set; }
        public string SensorName { get; set; }
        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public string Address { get; set; }
        public string Recommendations { get; set; }
        public Sensor UserSensor { get; set; }
        public List<WeatherDataFromAPI> Weather { get; set; }
    }
}