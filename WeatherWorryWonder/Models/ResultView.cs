using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class ResultView
    {
        public string PollutantWarning{ get; set; }
        public double CO { get; set; }
        public double C2H4OPPM { get; set; }
        public double O3AQI { get; set; }
        public double Second03AQI { get; set; }
        public double Third03AQI { get; set;}
        public double PM25AQI { get; set; }
        public double NO2AQI { get; set; }
        public double SO2AQI { get; set; }
        public double PredictedAQITomorrow { get; set; }
        public double PredictedAQI3Day { get; set; }
        public double PredictedAQI5Day { get; set; }
        public double EpaAQI { get; set; }
        public string SensorName { get; set; }
        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public string Address { get; set; }
        public string Recommendations { get; set; }
        public Sensor UserSensor { get; set; }
        public string AQIColor1 { get; set; }
        public string AQIColor2 { get; set; }
        public string AQIColor3 { get; set; }
        public double Distance { get; set; }
        public string Suggestion { get; set; }
        public List<WeatherDataFromAPI> Weather { get; set; }
        public List<Sensor> TwoClosestSensors { get; set; }
        public List<double> HistoricData { get; set; }

        public double Factorylat { get; set; }
        public double Factorylong { get; set; }
    }
}