using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class Sensor
    {
        public double Lat { get; set; }
        public double Long { get; set; }
        public string Name { get; set; }
        public string CrossStreet { get; set; }
        public string Distance { get; set; }

        public Sensor()
        {

        }

        public Sensor(string name, string crossStreet, double latitude, double longitude)
        {
            CrossStreet = crossStreet;
            Name = name;
            Lat = latitude;
            Long = longitude;
        }

        public static List<Sensor> Sensors = new List<Sensor>()
        {
            new Sensor("graqm0107", "Market Ave and Godfrey", 42.9547237, -85.6824347),
            new Sensor("graqm0101", "Leonard St and Monroe Ave OST",  42.984136, -85.671280),
            new Sensor("graqm0105", "B St and Godfrey", 42.9472356, -85.6822996),
            new Sensor("graqm0108", "Alger and Eastern", 42.9201462, -85.6476561),
            new Sensor("graqm0117", "Oxford and Godfrey", 42.9467373, -85.6843539),
            new Sensor("0004a30b00232915", "32nd and Broadmore", 42.904438, -85.5814071),
            new Sensor("0004a30b0023339e", "Hall and Madison", 42.9414937, -85.658029),
            new Sensor("0004a30b0023acbc", "Leonard St and Monroe Ave Simms", 42.984136, -85.671280)
        };

        //might be useful later
        public static List<Sensor> GetSensors()
        {
            List<Sensor> sensors = new List<Sensor>();
            sensors.Add(new Sensor("graqm0107", "Market Ave and Godfrey", 42.9547237, -85.6824347));
            sensors.Add(new Sensor("graqm0101", "Leonard St and Monroe Ave OST", 42.984136, -85.671280));
            sensors.Add(new Sensor("graqm0105", "B St and Godfrey", 42.9472356, -85.6822996));
            sensors.Add(new Sensor("graqm0108", "Alger and Eastern", 42.9201462, -85.6476561));
            sensors.Add(new Sensor("graqm0117", "Oxford and Godfrey", 42.9467373, -85.6843539));
            sensors.Add(new Sensor("0004a30b00232915", "32nd and Broadmore", 42.904438, -85.5814071));
            sensors.Add(new Sensor("0004a30b0023339e", "Hall and Madison", 42.9414937, -85.658029));
            sensors.Add(new Sensor("0004a30b0023acbc", "Leonard St and Monroe Ave Simms", 42.984136, -85.671280));
            return sensors;
        }

    }
}