using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using WeatherWorryWonder.Controllers;

namespace WeatherWorryWonder.Models
{
    public class WeatherAPIDAL
    {

        public static string APIText()
        {
            //My Key
            string key = APIKeys.WeatherAPI;
            //City code for Grand Rapids
            string cityCode = "4994358";

            string URL = $"http://api.openweathermap.org/data/2.5/forecast?id={cityCode}&APPID={key}";

            HttpWebRequest request = WebRequest.CreateHttp(URL);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            StreamReader rd = new StreamReader(response.GetResponseStream());

            string APIText = rd.ReadToEnd();

            return APIText;

        }

        public static JToken Json()
        {
            string APIText = WeatherAPIDAL.APIText();

            JToken json = JToken.Parse(APIText);

            return json;
        }

    }
}