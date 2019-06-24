using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class GoogleMapDAL
    {
        public static string APICall(string address)
        {
            string key = APIKeys.GoogleMapsAPI;
            //adds address to url in usable format
            string URL = $"https://maps.googleapis.com/maps/api/geocode/json?address={address}&key={key}";

            HttpWebRequest request = WebRequest.CreateHttp(URL);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader rd =  new StreamReader(response.GetResponseStream());

            string APIText = rd.ReadToEnd();

            return APIText;
        }

        //takes in address in Google's URL format, still gotta code that
        public static JToken GoogleJson(string address)
        {
            string APIText = APICall(GoogleAddress(address));

            JToken j = JToken.Parse(APIText);

            return j;
        }

        //turns street address into format to put into URL
        public static string GoogleAddress(string streetAddress)
        {
            string[] addressArr = streetAddress.Split(' ');
            string googleAddress = "";

            for(int i = 0; i < addressArr.Length; i++)
            {
                googleAddress += addressArr[i] + "+";
            }
            googleAddress += ",+Grand+Rapids,+MI";

            return googleAddress;
        }
            //1600+Amphitheatre+Parkway,+Mountain+View,+CA
    }
}