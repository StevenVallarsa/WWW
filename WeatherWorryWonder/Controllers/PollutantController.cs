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
        public static decimal CalculateAQI(decimal oneHrPollutantPPM, decimal eightHrPollutantPPM)
        {
            int index = EightorOneHour(oneHrPollutantPPM, eightHrPollutantPPM);
            decimal oneHr = Math.Round(oneHrPollutantPPM, 3);
            decimal eightHr = Math.Round(eightHrPollutantPPM, 3);
            decimal Ihi = 0;
            decimal Ilo = 0;
            decimal BPhi = 0;
            decimal BPlow = 0;
            decimal Cp = 0;
            if (oneHrPollutantPPM <= (decimal)0.125)
            {
                Ihi = (decimal) pollutants[7].High[index];
                Ilo = (decimal) pollutants[7].Low[index];
                BPhi = (decimal) pollutants[0].High[index];
                BPlow = (decimal) pollutants[0].Low[index];
                Cp = eightHr;

                //calculate using 8 hr Ozone
            }
            else
            {
                Ihi = (decimal)pollutants[7].High[index];
                Ilo = (decimal)pollutants[7].Low[index];
                BPhi = (decimal)pollutants[1].High[index];
                BPlow = (decimal)pollutants[1].Low[index];
                Cp = oneHr;
            }

            decimal AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;

            return AQIForPollutant;
        }

    }
}