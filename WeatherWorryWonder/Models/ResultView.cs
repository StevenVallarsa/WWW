﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class ResultView
    {
        public string PollutantWarning{ get; set; }
        public decimal CO { get; set; }
        public decimal C2H4OPPM { get; set; }
        public decimal O3AQI { get; set; }
        public decimal Second03AQI { get; set; }
        public decimal Third03AQI { get; set;}
        public decimal PM25AQI { get; set; }
        public decimal NO2AQI { get; set; }
        public decimal SO2AQI { get; set; }
        public decimal PredictedAQITomorrow { get; set; }
        public decimal PredictedAQI3Day { get; set; }
        public decimal PredictedAQI5Day { get; set; }
        public decimal EpaAQI { get; set; }
        public string SensorName { get; set; }
        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public string Address { get; set; }
        public string Recommendations { get; set; }
        public Sensor UserSensor { get; set; }
        public List<WeatherDataFromAPI> Weather { get; set; }
    }
}