using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeatherWorryWonder.Models;

namespace WeatherWorryWonder.Controllers
{
    public class PollutantController : Controller
    {
        public static WeatherWorryWonderDBEntities db = new WeatherWorryWonderDBEntities();

        //Depending on the sensor and user time, pulling an 8 hr average reading
        public static decimal PollutantDataReading(Sensor s, int mins)
        {
            //example of what the string date looks like "2019 - 03 - 01T"            
            //take the current hour            
            string strB = DateTime.Now.ToString("HH");

            DateTime datevalue = (Convert.ToDateTime(strB.ToString()));
            string dy = datevalue.Day.ToString();


            //take the DeLorean and go back to a date in the past
            string currentTime = $"2019-03-{dy}T{strB}";
   
            string sensorLocation = s.Name;

            //take in the sensor that is closest to the user
            //string sensorLocation = "graqm0107";

            bool answer = (sensorLocation.Contains("graq"));
            //if sensor name contains ost
            if (answer == true)
            {
                List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
                ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
                    .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                    .First();

                int x = startingPoint.Id;
                //get 8 hr average AQI
                for (int i = 0; i < mins; i++)
                {
                    ost_data_Jan_June2019 AQIdata = db.ost_data_Jan_June2019.Find(x);
                    OSTData.Add(AQIdata);
                    x++;
                }

                //sum all the O3(ozone) AQI readings from the list
                decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => O3.o3));
                //average the AQI readings by dividing by number of readings
                decimal OSTAverage = OSTDataO3sum / OSTData.Count;

                return ConvertPPBtoPPM(OSTAverage);

            }
            //if sensor name contains simms
            else
            {
                List<simms_data_Jan_June2019> simsData = new List<simms_data_Jan_June2019>();
                simms_data_Jan_June2019 startingPoint = db.simms_data_Jan_June2019
                    .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                    .First();

                int x = startingPoint.Id;
                //get 8 hr average AQI
                for (int i = 0; i < mins; i++)
                {
                    simms_data_Jan_June2019 AQIdata = db.simms_data_Jan_June2019.Find(x);
                    simsData.Add(AQIdata);
                    x++;
                }

                //sum all the O3(ozone) AQI readings from the list
                decimal SimsDataO3sum = Convert.ToDecimal(simsData.Sum(O3 => O3.o3));
                //average the AQI readings by dividing by number of readings
                decimal SimsTOAverage = SimsDataO3sum / simsData.Count;

                return ConvertPPBtoPPM(SimsTOAverage);
            }
        }
        
        public static decimal ConvertPPBtoPPM(decimal PollutantPPB)
        {
            //1 ppm = 1000 ppb
            decimal PollutantPPM = PollutantPPB / 1000;

            return PollutantPPM;
        }

        public static List<Pollutant> pollutants = Pollutant.GetPollutantTypes();
        public static int EightorOneHour(decimal oneHrPollutantPPM, decimal eightHrPollutantPPM)
        {

            int num = 0;
            if (oneHrPollutantPPM <= (decimal)0.125)
            {
                for (int i = 0; i < 5; i++)
                {
                    double low = pollutants[0].Low[i];
                    double high = pollutants[0].High[i];
                    if (eightHrPollutantPPM >= (decimal)low && eightHrPollutantPPM <= (decimal)high)
                    {
                        num = i;
                    }
                }

            }
            else
            {
                for (int i = 2; i < 7; i++)
                {
                    double low = pollutants[1].Low[i];
                    double high = pollutants[1].High[i];
                    if (oneHrPollutantPPM >= (decimal)low && oneHrPollutantPPM <= (decimal)high)
                    {
                        num = i;
                    }
                }
            }
            return num;
        }
        public static decimal CalculateAQI(decimal eightHrPollutantPPM, int index, bool isEight)
        {
            int pollutantOneOrEight = 7;
            if (isEight)
            {
                pollutantOneOrEight = 0;
            }
            else
            {
                pollutantOneOrEight = 1;
            }
                decimal eightHr = Math.Round(eightHrPollutantPPM, 3);
                decimal Ihi = (decimal)pollutants[7].High[index];
                decimal Ilo = (decimal)pollutants[7].Low[index];
                decimal BPhi = (decimal)pollutants[pollutantOneOrEight].High[index];
                decimal BPlow = (decimal)pollutants[pollutantOneOrEight].Low[index];
                decimal Cp = eightHr;
            //calculate using 8 hr Ozone
            decimal AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }
    }
}