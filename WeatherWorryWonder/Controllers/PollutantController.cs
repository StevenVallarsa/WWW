﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WeatherWorryWonder.Models;

namespace WeatherWorryWonder.Controllers
{
    public class PollutantController : Controller 
    {
        public static WeatherWorryWonderDBEntities db = new WeatherWorryWonderDBEntities();
        public static List<Pollutant> pollutants = Pollutant.GetPollutantTypes();

        //Depending on the sensor and user time
        //OST an SIMMS are in seperate database tables therefore we need to know which table to pull using an if/else statement
        public static decimal PollutantDataReading(Sensor s, int mins)
        {
            //example of what the string date looks like "2019 - 03 - 01T"            
            //take the current hour            
            string strB = DateTime.Now.ToString("HH");

            //DateTime datevalue = (Convert.ToDateTime(strB.ToString()));
            //string dy = datevalue.Day.ToString();


            //take the DeLorean and go back to a date in the past
            string currentTime = $"2019-03-15T{strB}";
            //pulls sensor name
            string sensorLocation = s.Name;

            //take in the sensor that is closest to the user
            //string sensorLocation = "graqm0107";

            //if contains graq = ost sensor
            bool answer = (sensorLocation.Contains("graq"));
            try
            {
                List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
                if (answer == true)
                {
                    //pulling the data based on user current time and location of sensor
                    ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
                        .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                        .First();

                    //pulls row of data
                    int x = startingPoint.Id;
                    //mins are either 480 or 60
                    for (int i = 0; i < mins; i++)
                    {
                        ost_data_Jan_June2019 OSTAQIdata = db.ost_data_Jan_June2019.Find(x);
                        if(OSTAQIdata != null)
                        {
                            OSTData.Add(OSTAQIdata);
                            x++;
                        }
                        else
                        {
                            x++;
                            continue;
                        }
                    }

                //sum all the O3(ozone) AQI readings from the list
                // ADDED UG/M3 TO PPB CONVERSION CONSTANT TO O3 DATA BEING DRAWN FROM DB TO MAKE DATA MATCH SIMM SENSORS 
                decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => (O3.o3 * (decimal)0.509)));
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
                        simms_data_Jan_June2019 SimmsAQIdata = db.simms_data_Jan_June2019.Find(x);
                        if (SimmsAQIdata != null)
                        {
                            simsData.Add(SimmsAQIdata);
                            x++;
                        }
                        else
                        {
                            x++;
                            continue;
                        }
                    }

                    //sum all the O3(ozone) AQI readings from the list
                    decimal SimsDataO3sum = Convert.ToDecimal(simsData.Sum(O3 => O3.o3));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsTOAverage = SimsDataO3sum / simsData.Count;

                    return ConvertPPBtoPPM(SimsTOAverage);
                }
            }
            //will return 0 which will then be caught by the HomeController loop as unreliable data, moving to the next sensor
            catch(Exception)
            {
                return 0;
            }
        }
        
        public static decimal ConvertPPBtoPPM(decimal PollutantPPB)
        {
            //1 ppm = 1000 ppb
            decimal PollutantPPM = PollutantPPB / 1000;

            return PollutantPPM;
        }


        public static List<int> EightorOneHour(decimal oneHrPollutantPPM, decimal eightHrPollutantPPM)
        {
            List<int> indexAndOneOrEight = new List<int>();
            //using 7 for testing purposes to throw an error if if/else does not work
            int oneOrEightHour = 7;
            int num = 0;
            //EPA if ppm > .125 use 1 hr readings
            //using 8 hr reading
            if (oneHrPollutantPPM <= (decimal)0.125)
            {
                //8 hr reading i = 2 because 0 and 1 index are null values
                for (int i = 2; i < 7; i++)
                {
                    double low = pollutants[0].Low[i];
                    double high = pollutants[0].High[i];

                    //takes a reading a looks for the range of low and high
                    if (eightHrPollutantPPM >= (decimal)low && eightHrPollutantPPM <= (decimal)high)
                    {
                        //should we use 8 hr reading
                        oneOrEightHour = 0;
                        //take an index for the range of low and high
                        num = i;
                        break;
                    }
                }

            }
            //using 1 hr reading
            else
            {
                //1 hr readings i = 5 and 6 are null values
                for (int i = 0; i < 5; i++)
                {
                    double low = pollutants[1].Low[i];
                    double high = pollutants[1].High[i];
                    if (oneHrPollutantPPM >= (decimal)low && oneHrPollutantPPM <= (decimal)high)
                    {
                        oneOrEightHour = 1;
                        num = i;
                        break;
                    }
                }
            }

            //a list of 2 values; first one is breakpoint index and second one is which hr reading (8 or 1) to use 
            indexAndOneOrEight.Add(num);
            indexAndOneOrEight.Add(oneOrEightHour);
            return indexAndOneOrEight;
        }

        // for AQI equation from EPA
        // isEight: 0 index = 8h reading; 1 index = 1h reading
        public static decimal CalculateAQI(decimal PollutantPPM, int index, int isEight)
        {
            // must round to 3 digits for O3 EPA standards
            decimal Cp = Math.Round(PollutantPPM, 3);

            // 7 = AQI standards in Pollutant Model
            decimal Ihi = (decimal)pollutants[7].High[index];
            decimal Ilo = (decimal)pollutants[7].Low[index];

            // index = breakpoint found from OneOrEight method
            decimal BPhi = (decimal)pollutants[isEight].High[index];
            decimal BPlow = (decimal)pollutants[isEight].Low[index];

            //calculate using 8 hr Ozone
            decimal AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }

        // converts to UG/M3
        public static decimal ConvertToUGM3(decimal PPM)
        {
            // Convert back to PPB then to UG/M3 -- 0.0409 is a conversion constant in order to go to UG/M3-- 48 is molecular weight of O3 
            decimal UGM3 = (PPM * 1000) * (decimal)0.0409 * 48;
            return UGM3;
        }

        // converts to PPM
        public static decimal UGM3ConvertToPPM(decimal UGM3)
        {
            decimal PPM = (UGM3 / ((decimal)0.0409 * 48)) / 1000;
            return PPM;
        }

        
        //public static int EPAAQIReading()
        //{
        //    string s = "2019-03-10";

        //    DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        //    EPA_GR_Data ePADataReading = db.EPA_GR_Data
        //        .Where(ut => ut.Date == dt)
        //        .First();


        //    int EPAO3DailyReading = ePADataReading.DAILY_AQI_VALUE;
        //    return (EPAO3DailyReading);
        //}

        public static decimal EPAAQIData()
        {
            string s = "2019-03-10";

            DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            EPA_GR_Data ePADataReading = db.EPA_GR_Data
                .Where(ut => ut.Date == dt)
                .First();


            //int EPAO3DailyReading = ePADataReading.DAILY_AQI_VALUE;
            decimal EPA03Reading = ePADataReading.Daily_Max_8_hour_Ozone_Concentration;
            return (EPA03Reading);
        }

        public static decimal CalculateEPA(decimal EPA)
        {

            decimal pollutant = Math.Round(EPA, 3);
            decimal Ihi = (decimal)pollutants[7].High[0];
            decimal Ilo = (decimal)pollutants[7].Low[0];
            decimal BPhi = (decimal)pollutants[0].High[0];
            decimal BPlow = (decimal)pollutants[0].Low[0];
            decimal Cp = pollutant;
            //calculate using 8 hr Ozone
            decimal AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }
    }
}