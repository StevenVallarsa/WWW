using Newtonsoft.Json.Linq;

namespace WeatherWorryWonder.Models
{
    public class WeatherDataFromAPI
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public int Humidity { get; set; }
        public string Clouds { get; set; }
        public double WindSpeed { get; set; }
        public double WindDirection { get; set; }


        public WeatherDataFromAPI()
        {
        }

        public WeatherDataFromAPI(JToken weather, int index)
        {

            Temperature = double.Parse(weather["list"][index]["main"]["temp"].ToString());

            Pressure = (double)weather["list"][index]["main"]["pressure"];

            Humidity = (int)weather["list"][index]["main"]["humidity"];

            Clouds = weather["list"][index]["weather"][0]["description"].ToString();

            WindSpeed = (double)weather["list"][index]["wind"]["speed"];

            WindDirection = (double)weather["list"][index]["wind"]["deg"];
        }
        
    }

}