using System;
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
        public static List<decimal> MorePollutantDataReading = new List<decimal>();       //storing sensor readings in this list
        public static List<decimal> PollutantAQIs = new List<decimal>();                          //storing all pollutant aqis in this list

        //Depending on the sensor and user time
        //OST an SIMMS are in seperate database tables therefore we need to know which table to pull using an if/else statement
        public static List<decimal> PollutantDataReading(Sensor s, int mins)
        {
            List<decimal> MorePollutantDataReading = new List<decimal>();

            //example of what the string date looks like "2019 - 03 - 01T"            
            //take the current hour            
            string currentHour = DateTime.Now.ToString("HH");

            //DateTime datevalue = (Convert.ToDateTime(currentHour.ToString()));
            //string dy = datevalue.Day.ToString();


            //take the DeLorean and go back to a date in the past
            string currentTime = $"2019-03-20T{currentHour}";
            //pulls sensor name
            string sensorLocation = s.Name;

            //take in the sensor that is closest to the user
            //string sensorLocation = "graqm0107";

            //if contains graq = ost sensor
            bool isOSTSensor = (sensorLocation.Contains("graq"));
            try 
            {
                List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
                if (isOSTSensor == true)
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
                        if((decimal)OSTAQIdata.o3 != 0 )
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
                    decimal OSTDataO3sum = Convert.ToDecimal(OSTData.Sum(O3 => (O3.o3) * (decimal)0.509));

                    //This next line should be removed b/c PM25 needs to have a 24 hour reading
                    decimal OSTDataPM25sum = Convert.ToDecimal(OSTData.Sum(PM25 => (PM25.pm25 ) * (decimal)148.17));   //PM25 weighs a lot more than O3 BTW

                    decimal OSTO3Average = OSTDataO3sum / OSTData.Count;
                    //average the AQI readings by dividing by number of readings
                    decimal OSTPM25Average = OSTDataPM25sum / OSTData.Count;

                    decimal ConvertedOSTO3 = ConvertPPBtoPPM(OSTO3Average);
                    decimal ConvertedOSTPM25 = ConvertPPBtoPPM(OSTPM25Average);

                    MorePollutantDataReading.Add(ConvertedOSTO3);    //index[0]
                    MorePollutantDataReading.Add(ConvertedOSTPM25);   //index[1]

                //sum all the O3(ozone) AQI readings from the list
                // ADDED UG/M3 TO PPB CONVERSION CONSTANT TO O3 DATA BEING DRAWN FROM DB TO MAKE DATA MATCH SIMM SENSORS 

                    //average the AQI readings by dividing by number of readings



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
                        if ((decimal)SimmsAQIdata.o3 != 0)
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
                    decimal SimsO3Average = SimsDataO3sum / simsData.Count;

                    //sum all the CO AQI readings from the list
                    decimal SimsDataCOsum = Convert.ToDecimal(simsData.Sum(CO => CO.co));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsCOToAverage = SimsDataCOsum / simsData.Count;

                    //sum all the no2 readings from the list
                    decimal SimsDataNO2sum = Convert.ToDecimal(simsData.Sum(NO2 => NO2.no2));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsNO2Average = SimsDataNO2sum / simsData.Count;

                    //sum all the no2 readings from the list
                    decimal SimsDataNO2_O3sum = Convert.ToDecimal(simsData.Sum(NO2o3 => NO2o3.no2_o3));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsNO2_O3Average = SimsDataNO2_O3sum / simsData.Count;

                    //sum all the no2 readings from the list
                    decimal SimsDataPM25sum = Convert.ToDecimal(simsData.Sum(PM25 => PM25.pm25));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsPM25Average = SimsDataPM25sum / simsData.Count;

                    //sum all the no2 readings from the list
                    decimal SimsDataSO2sum = Convert.ToDecimal(simsData.Sum(SO2 => SO2.so2));
                    //average the AQI readings by dividing by number of readings
                    decimal SimsSO2Average = SimsDataSO2sum / simsData.Count;

                    MorePollutantDataReading.Add(SimsO3Average);   //index[0]
                    MorePollutantDataReading.Add(SimsCOToAverage);   //index[1]
                    MorePollutantDataReading.Add(SimsNO2Average);     //index[2]
                    MorePollutantDataReading.Add(SimsNO2_O3Average);    //index[3]
                    MorePollutantDataReading.Add(SimsPM25Average);      //index[4]
                    MorePollutantDataReading.Add(SimsSO2Average);      //index[5]

                }

            }

            //will return 0 which will then be caught by the HomeController loop as unreliable data, moving to the next sensor
            catch (Exception)

            {
                MorePollutantDataReading.Add(0);
                MorePollutantDataReading.Add(0);
                MorePollutantDataReading.Add(0);
                MorePollutantDataReading.Add(0);
                MorePollutantDataReading.Add(0);
                MorePollutantDataReading.Add(0);
            }
            return MorePollutantDataReading;
        }
        //-------------------------------------------------------------------------------------------------- Callista added 6/22/19

        public static List<decimal> CalculatePM25SensorReading(Sensor s, int mins)  //PM25 needs a 24 hour reading 
        {
            List<decimal> PMDataToConvertToAQI = new List<decimal>();

            string oneDay = DateTime.Now.ToString("DD");
            //DateTime datevalue = (Convert.ToDateTime(oneDay.ToString()));
            //string day = datevalue.Day.ToString();

            ////example of what the string date looks like "2019 - 03 - 01T"            

            //take the DeLorean and go back to a date in the past
            string currentDay = $"2019-03-20T{oneDay}";
            //pulls sensor name
            string sensorLocation = s.Name;

            //take in the sensor that is closest to the user
            s.Name = "graqm0107";

            //if contains graq = ost sensor
            bool answer = (sensorLocation.Contains("graq"));
            try
            {
                List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
                if (answer == true)
                {
                    //pulling the data based on user current time and location of sensor
                    ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
                        .Where(ut => ut.time.Contains(currentDay) && ut.dev_id == sensorLocation) 
                        .First();
                //}
                //pulls row of data
                int x = startingPoint.Id;
                    //mins are either 480 or 60
                    for (int i = 0; i < mins; i++)
                    {
                        ost_data_Jan_June2019 OSTPM25data = db.ost_data_Jan_June2019.Find(currentDay);
                        if (OSTPM25data.pm25 != 0)
                        {
                            OSTData.Add(OSTPM25data);
                            x++;
                        }
                        else
                        {
                            x++;
                            continue;
                        }
                    }

                    decimal OSTDataPM25sum = Convert.ToDecimal(OSTData.Sum(PM25 => (PM25.pm25)) * (decimal)148.17);   //PM25 weighs a lot more than O3 BTW


                    //average the AQI readings by dividing by number of readings
                    decimal OSTPM25Average = OSTDataPM25sum / OSTData.Count;

                    decimal ConvertedOSTPM25 = ConvertPPBtoPPM(OSTPM25Average);

                    PMDataToConvertToAQI.Add(ConvertedOSTPM25);   //index[0]

                }
            }
            //will return 0 which will then be caught by the HomeController loop as unreliable data, moving to the next sensor
            catch (Exception e)

            {
                string Message = "No data.";
                Message = e.Message;
            }
            return PMDataToConvertToAQI;
        }

       

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        public static decimal ConvertPPBtoPPM(decimal PollutantPPB)
        {
                //1 ppm = 1000 ppb
                decimal PollutantPPM = PollutantPPB / 1000;

                return PollutantPPM;
        }

        public static List<List<decimal>> HistoricData(Sensor s, int month)
        {
            //0 index is numbers for one week, 1 is number for one month
            List<decimal> oneWeekValues = new List<decimal>();
            List<decimal> oneMonthValues = new List<decimal>();

            decimal sensorData;

            int Day = 23;
            string currentHour = "20"; //DateTime.Now.ToString("HH");
            string oneMonthAgo = (month - 1).ToString();
            if(oneMonthAgo.Length == 1)
            {
                oneMonthAgo = "0" + oneMonthAgo;
            }
            if(s.Name.Contains("graq"))
            {
                for(int i = 0; i < 30; i++)
                {
                    if(Day > 28)
                    {
                        Day = 1;
                    }
                    //grabs the day one month ago and then increments it
                    string sDay = Day.ToString();
                    if(sDay.Length == 1)
                    {
                        sDay = "0" + Day;
                    }

                    string monthAgoTime = $"2019-{oneMonthAgo}-{sDay}T{currentHour}";
                    sensorData = PullOSTSensorData(s, 20, monthAgoTime);
                    if (sensorData != 0)
                    {
                        oneMonthValues.Add(sensorData);
                    }
                    //when we get up to a week in the past, start adding to the week list as well
                    if(i > 23)
                    {
                        if (sensorData != 0)
                        {
                            oneMonthValues.Add(sensorData);
                        }
                    }
                    Day++;
                }
            }
            else
            {
                for (int i = 0; i < 30; i++)
                {
                    if (Day > 28)
                    {
                        Day = 1;
                    }
                    string sDay = Day.ToString();
                    if (sDay.Length == 1)
                    {
                        sDay = "0" + Day;
                    }
                    //grabs the day one month ago and then increments it
                    string monthAgoTime = $"2019-{oneMonthAgo}-{sDay}T{currentHour}";
                    sensorData = PullSimmsSensorData(s, 20, monthAgoTime);
                    if (sensorData != 0)
                    {
                        oneMonthValues.Add(sensorData);
                    }
                    //when we get up to a week in the past, start adding to the week list as well
                    if (i > 23)
                    {
                        if (sensorData != 0)
                        {
                            oneMonthValues.Add(sensorData);
                        }
                    }
                    Day++;
                }
            }
            List<List<decimal>> oneWeekAndMonthHistoricData = new List<List<decimal>> {oneWeekValues, oneMonthValues };
            return oneWeekAndMonthHistoricData;
        }

        public static decimal PullOSTSensorData(Sensor s, int mins, string dateTime)
        {
            List<ost_data_Jan_June2019> OSTData = new List<ost_data_Jan_June2019>();
            int x = 0;
            //pulling the data based on user current time and location of sensor
            try
            {
                ost_data_Jan_June2019 startingPoint = db.ost_data_Jan_June2019
                    .Where(ut => ut.time.Contains(dateTime) && ut.dev_id == s.Name)
                    .First();
                //pulls row of data
                x = startingPoint.Id;
                //mins are either 480 or 60
            }
            catch
            {
                return 0;
            }
            for (int i = 0; i < mins; i++)
            {
                ost_data_Jan_June2019 OSTAQIdata = db.ost_data_Jan_June2019.Find(x);
                if ((decimal)OSTAQIdata.o3 != 0 || OSTAQIdata.o3 != null)
                {
                    OSTData.Add(OSTAQIdata);
                    x++;
                }
                else
                {
                    x++;
                }
            }
            decimal average = (decimal)OSTData.Sum(O3 => (O3.o3) / OSTData.Count) * (decimal)0.509;
            return ConvertPPBtoPPM(average);
        }

        public static decimal PullSimmsSensorData(Sensor s, int mins, string dateTime)
        {
            List<simms_data_Jan_June2019> simsData = new List<simms_data_Jan_June2019>();
            int x = 0;
            try
            {
                simms_data_Jan_June2019 startingPoint = db.simms_data_Jan_June2019
                    .Where(ut => ut.time.Contains(dateTime) && ut.dev_id == s.Name)
                    .First();

                x = startingPoint.Id;
            }
            catch
            {
                return 0;
            }
            //get 8 hr average AQI
            for (int i = 0; i < mins; i++)
            {
                simms_data_Jan_June2019 SimmsAQIdata = db.simms_data_Jan_June2019.Find(x);
                if ((decimal)SimmsAQIdata.o3 != 0)
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
            decimal SimsDataO3sum = Convert.ToDecimal(simsData.Sum(O3 => O3.o3));
            //average the AQI readings by dividing by number of readings
            decimal average = SimsDataO3sum / simsData.Count;

            return ConvertPPBtoPPM(average);
        }

        public static List<int> EightorOneHour(decimal oneHrPollutantPPM, decimal eightHrPollutantPPM)
        {
            List<int> indexAndOneOrEight = new List<int>();
            //using 7 for testing purposes to throw an error if if/else does not work
            int oneOrEightHour = 7;
            int breakPoingIndex = 0;
            //EPA if ppm > .125 use 1 hr readings
            //using 8 hr reading
            if (oneHrPollutantPPM <= (decimal)0.125)
            {
                //8 hr reading: 5 and 6 index are null values on table
                for (int i = 0; i < 5; i++)
                {
                    double low = pollutants[0].Low[i];
                    double high = pollutants[0].High[i];

                    //takes a reading a looks for the range of low and high
                    if (eightHrPollutantPPM >= (decimal)low && eightHrPollutantPPM <= (decimal)high)
                    {
                        //should we use 8 hr reading
                        oneOrEightHour = 0;
                        //take an index for the range of low and high
                        breakPoingIndex = i;
                        break;
                    }
                }

            }
            //using 1 hr reading
            else
            {
                //1 hr reading: 1 and 2 are null values on table
                for (int i = 2; i < 7; i++)
                {
                    double low = pollutants[1].Low[i];
                    double high = pollutants[1].High[i];
                    if (oneHrPollutantPPM >= (decimal)low && oneHrPollutantPPM <= (decimal)high)
                    {
                        oneOrEightHour = 1;
                        breakPoingIndex = i;
                        break;
                    }
                }
            }

            //a list of 2 values; first one is breakpoint index and second one is which hr reading (8 or 1) to use 
            indexAndOneOrEight.Add(breakPoingIndex);
            indexAndOneOrEight.Add(oneOrEightHour);
            return indexAndOneOrEight;
        }

        // for AQI equation from EPA
        // isEight: 0 index = 8h reading; 1 index = 1h reading
        public static decimal CalculateO3AQI(decimal PollutantPPM, int index, int isEight)
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
            decimal AQIForO3Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;

            PollutantAQIs.Add(AQIForO3Pollutant);              //added O3 to PollutantAQI list
            return AQIForO3Pollutant;
        }
        //---------------------------------------------------------------------------------------------- Callista added 6/23/19

        public static decimal CalculatePM25AQI(decimal PollutantPPM, int index)
        {

            // must round to 3 digits for O3 EPA standards
            decimal Cp = Math.Round(PollutantPPM, 4);

            // 7 = AQI standards in Pollutant Model
            decimal Ihi = (decimal)pollutants[7].High[index];
            decimal Ilo = (decimal)pollutants[7].Low[index];

            // index = breakpoint found from OneOrEight method
            decimal BPhi = (decimal)pollutants[3].High[index];
            decimal BPlow = (decimal)pollutants[3].Low[index];


            decimal AQIForPM25Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            PollutantAQIs.Add(AQIForPM25Pollutant);       //adding PM25 to AQI List

            return AQIForPM25Pollutant;
        }

        public static decimal CalculateCOAQI(decimal PollutantPPM, int index)
        {
            // must round to 3 digits for O3 EPA standards
            decimal Cp = Math.Round(PollutantPPM, 4);

            // 7 = AQI standards in Pollutant Model
            decimal Ihi = (decimal)pollutants[7].High[index];
            decimal Ilo = (decimal)pollutants[7].Low[index];

            // index = breakpoint found from OneOrEight method
            decimal BPhi = (decimal)pollutants[4].High[index];
            decimal BPlow = (decimal)pollutants[4].Low[index];

            decimal AQIForCOPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            PollutantAQIs.Add(AQIForCOPollutant);       //adding PM25 to AQI List

            return AQIForCOPollutant;
        }

        public static decimal CalculateSO2AQI(decimal PollutantPPM, int index)
        {
            // must round to 3 digits for O3 EPA standards
            decimal Cp = Math.Round(PollutantPPM, 4);

            // 7 = AQI standards in Pollutant Model
            decimal Ihi = (decimal)pollutants[7].High[index];
            decimal Ilo = (decimal)pollutants[7].Low[index];

            // index = breakpoint found from OneOrEight method
            decimal BPhi = (decimal)pollutants[5].High[index];
            decimal BPlow = (decimal)pollutants[5].Low[index];

            decimal AQIForSO2Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            PollutantAQIs.Add(AQIForSO2Pollutant);       //adding PM25 to AQI List

            return AQIForSO2Pollutant;
        
        }

        public static decimal CalculateNO2AQI(decimal PollutantPPM, int index)
        {
            // must round to 3 digits for O3 EPA standards
            decimal Cp = Math.Round(PollutantPPM, 4);

            // 7 = AQI standards in Pollutant Model
            decimal Ihi = (decimal)pollutants[7].High[index];
            decimal Ilo = (decimal)pollutants[7].Low[index];

            // index = breakpoint found from OneOrEight method
            decimal BPhi = (decimal)pollutants[6].High[index];
            decimal BPlow = (decimal)pollutants[6].Low[index];

            decimal AQIForNO2Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            PollutantAQIs.Add(AQIForNO2Pollutant);       //adding PM25 to AQI List

            return AQIForNO2Pollutant;
        }

        //create new method to cycle through PollutantAQIlist, then find highest AQI out of all the pollutants 

        //----------------------------------------------------------------------------------------------------------------------------------------------------
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
            string s = "2019-03-20";

            DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            EPA_GR_Data ePADataReading = db.EPA_GR_Data
                .Where(ut => ut.Date == dt)
                .First();


            //int EPAO3DailyReading = ePADataReading.DAILY_AQI_VALUE;
            decimal EPA03Reading = ePADataReading.Daily_Max_8_hour_Ozone_Concentration;
            return (EPA03Reading);
        }

        public static decimal CalculateEPA(decimal EPA, int breakpointIndex)
        {

            decimal pollutant = Math.Round(EPA, 3);
            decimal Ihi = (decimal)pollutants[7].High[breakpointIndex];
            decimal Ilo = (decimal)pollutants[7].Low[breakpointIndex];
            decimal BPhi = (decimal)pollutants[0].High[breakpointIndex];
            decimal BPlow = (decimal)pollutants[0].Low[breakpointIndex];
            decimal Cp = pollutant;
            //calculate using 8 hr Ozone
            decimal AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }

        public static int EPABreakpointTable(decimal eightHrPollutantPPM)
        {
            //using 7 for testing purposes to throw an error if if/else does not work
            int breakpointIndex = 0;

            //8 hr reading: 5 and 6 index are null values on table
            for (int i = 0; i < 5; i++)
            {
                double low = pollutants[0].Low[i];
                double high = pollutants[0].High[i];

                //takes a reading a looks for the range of low and high
                if (eightHrPollutantPPM >= (decimal)low && eightHrPollutantPPM <= (decimal)high)
                {
                    //take an index for the range of low and high
                    breakpointIndex = i;
                    break;
                }
            }

            return breakpointIndex;
        }
        //C2H4O = ethylene oxide
        public static decimal ShortestDistancePollutantSensor(List<double> s)
        {
            List<Factory_Pollution> pollutantSensors = db.Factory_Pollution.ToList();

            double largeNum = double.MaxValue;
            Factory_Pollution pollutantSensor = new Factory_Pollution();
            foreach (Factory_Pollution f in pollutantSensors)
            {
                double sensorDistance = GeocodeController.LatLongDistance(s[0],s[1],f.Latitude,f.Longitude);
                if (sensorDistance < largeNum)
                {
                    largeNum = sensorDistance;
                    pollutantSensor = f;
                }
            }

            decimal ethyleneOxide = pollutantSensor.ETO_ppm.GetValueOrDefault();
            return ethyleneOxide;
        }
        public static string PollutantWarning(decimal ethyleneOxideUGM3)
        {
            //0.18 µg / m3  normal background concentration of ethylene oxide
            if (ethyleneOxideUGM3 > (decimal)0.18)
            {
                //grab current 24 hr average decimal of NO2
                //grab current 24 hr average decimal of CO
                //grab wind speed and direction

                //air should contain less than 0.1 ppm ethylene oxide averaged over a 10-hour workday

                decimal c2H4Oppm = (ethyleneOxideUGM3 / (decimal)(0.0409 * 44.05)) / 1000; //g/mol
                if (c2H4Oppm > (decimal)0.1)
                {
                    return "Warning! High Pollutant Level Alert!";
                }
                else
                {
                    return "Warning! Mild Pollutant Level Alert!";
                }

                
            }
            else
            {
                return "Don't worry! You're safe.";   
            }


        }
    }
}