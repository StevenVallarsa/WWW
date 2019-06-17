﻿using System;
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

        public Sensor()
        {

        }

        public Sensor(string name, double latitude, double longitude)
        {
            Name = name;
            Lat = latitude;
            Long = longitude;
        }

        public static List<Sensor> Sensors = new List<Sensor>()
        {
            new Sensor("graqm0106,ost", 42.9420703, -85.6847243),
            new Sensor("graqm0107,ost", 42.9547237, -85.6824347),
            new Sensor("graqm0111,ost", 42.9274400, -85.6604877),
            new Sensor("graqm0101,ost", 42.984136, -85.671280),
            new Sensor("graqm0115,ost", 42.9372291, -85.6669082),
            new Sensor("graqm0105,ost", 42.9472356, -85.6822996),
            new Sensor("graqm0108,ost", 42.9201462, -85.6476561),
            new Sensor("graqm0117,ost", 42.9467373, -85.6843539),
            new Sensor("0004a30b0024358c,simms", 42.9273222, -85.6466512),
            new Sensor("0004a30b00232915,simms", 42.904438, -85.5814071),
            new Sensor("0004a30b0023339e,simms", 42.9414937, -85.658029),
            new Sensor("0004a30b0023acbc,simms", 42.984136, -85.671280)
        };

    }
}