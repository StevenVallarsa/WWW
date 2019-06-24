using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WeatherWorryWonder.Controllers;
using WeatherWorryWonder.Models;
using System.Web.Mvc;


namespace WeatherWorryWonder.Controllers
{
    public class GeocodeController : Controller
    {
        //Haversine formula for distance between two lat/longs
        public static double LatLongDistance(double lat1, double long1, double lat2, double long2)
        {
            //mean earth Radius in statute(normal) miles
            double earthRadius = 3958.8;
            //turn the difference between the two lats and longs into radians
            double degreeLat = degreesToRadians(lat1 - lat2);
            double degreeLong = degreesToRadians(long1 - long2);
            //turn it all into a haversine, no biggie
            double Haversine =
                Math.Pow(Math.Sin(degreeLat / 2), 2)
                + Math.Cos(degreesToRadians(lat1))
                * Math.Cos(degreesToRadians(lat2))
                * Math.Sin(degreeLong / 2)
                * Math.Sin(degreeLong / 2);
            //final calculation to complete the formula
            double c = 2 * Math.Atan2(Math.Sqrt(Haversine), Math.Sqrt(1 - Haversine));
            //trigonometry is fun for everyone
            double jeezFinally = earthRadius * c;
            
            //Now we have the great circle distance between the two coordinates
            return jeezFinally;

        }

        // separating the "ShortestToLongest" method so as to extract the user location in lattitute and longitude
        // to put in a Session in the HomeController AND for the rest of the "ShortestToLongest" method
        public static List<double> ParseUserLocation(string address)
        {
            //List<Sensor> sensors = Sensor.GetSensors();
            //List<Sensor> shortSensors = new List<Sensor>();
            //Changes the Address to a longitude and latitude coordinate from the google geocode API
            JToken jsonAddress = GoogleMapDAL.GoogleJson(address);

            double addressLat = double.Parse(jsonAddress["results"][0]["geometry"]["location"]["lat"].ToString());
            double addressLng = double.Parse(jsonAddress["results"][0]["geometry"]["location"]["lng"].ToString());

            List<double> userLocation = new List<double>() { addressLat, addressLng };

            return userLocation;
        }


        public static List<Sensor> ShortestToLongest(List<double> userLocation)
        { 
            List<Sensor> sensors = Sensor.GetSensors();
            List<Sensor> shortSensors = new List<Sensor>();

            double addressLat = userLocation[0];
            double addressLng = userLocation[1];

            //rearranges sensors from closest to furthest from the user input address
            for(int i = 0; i < Sensor.Sensors.Count; i++)
            {
                double lat = Sensor.Sensors[i].Lat;
                double Lng = Sensor.Sensors[i].Long;
                Sensor s = ShortestDistanceSensor(addressLat, addressLng, sensors);
                shortSensors.Add(s);
                sensors.Remove(s);
            }
            return shortSensors;

        }

        private static double degreesToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }

        //finds the sensor that is closest to given lat and long and returns the object
        public static Sensor ShortestDistanceSensor(double latitude, double longitude, List<Sensor> sensors)
        {
            //the max value that a double type can hold, used to find the next closest sensor
            double closestSensorD = double.MaxValue;
            Sensor closestSensor = new Sensor();
            List<Sensor> listSensors = Sensor.Sensors;
            //will return sensor named "Error" if foreach doesn't work
            closestSensor.Name = "Error";
            //compares distance to each of the sensors and finds the closests and returns the closest sensor. 
            foreach(Sensor s in sensors)
            {
                double sensorDistance = LatLongDistance(latitude, longitude, s.Lat, s.Long);
                if(sensorDistance < closestSensorD)
                {
                    closestSensorD = sensorDistance;
                    closestSensor = s;
                }
            }

            return closestSensor;
        }

        //public static Sensor ClosestSensor(string address)
        //{
        //    //Creates lat and long from google map API response
        //    JToken jsonAddress = GoogleMapDAL.GoogleJson(address);
        //    //latitude and longitude from google maps geocode API
        //    double lat = double.Parse(jsonAddress["results"][0]["geometry"]["location"]["lat"].ToString());
        //    double lng = double.Parse(jsonAddress["results"][0]["geometry"]["location"]["lng"].ToString());

        //    Sensor closestSensor = ShortestDistanceSensor(lat, lng);
        //    return closestSensor;
        //}

    }
}