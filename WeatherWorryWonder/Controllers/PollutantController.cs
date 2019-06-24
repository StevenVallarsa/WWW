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
        public static WWWEntities db = new WWWEntities();
        public static List<Pollutant> pollutants = Pollutant.GetPollutantTypes();
        public static List<float> MorePollutantDataReading = new List<float>();       //storing sensor readings in this list
        public static List<float> PollutantAQIs = new List<float>();                          //storing all pollutant aqis in this list

        //Depending on the sensor and user time
        //OST an SIMMS are in seperate database tables therefore we need to know which table to pull using an if/else statement
        public static List<float> PollutantDataReading(Sensor s, int mins)
        {
            List<float> MorePollutantDataReading = new List<float>();

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
                List<Ost_Data_Feb_Apr_Final> OSTData = new List<Ost_Data_Feb_Apr_Final>();
                if (isOSTSensor == true)
                {
                    //pulling the data based on user current time and location of sensor
                    Ost_Data_Feb_Apr_Final startingPoint = db.Ost_Data_Feb_Apr_Final
                        .Where(ut => ut.DateTime.Contains(currentTime) && ut.dev_id == sensorLocation)
                        .First();

                    //pulls row of data
                    int x = startingPoint.id;
                    //mins are either 480 or 60
                    for (int i = 0; i < mins; i++)
                    {
                        Ost_Data_Feb_Apr_Final OSTAQIdata = db.Ost_Data_Feb_Apr_Final.Find(x);
                        if((float)OSTAQIdata.o3 != 0 )
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
                    float OSTDataO3sum = (float)OSTData.Sum(O3 => (O3.o3) * 0.509f);

                    //This next line should be removed b/c PM25 needs to have a 24 hour reading
                    float OSTDataPM25sum = (float)OSTData.Sum(PM25 => (PM25.pm25 ) * 148.17f);   //PM25 weighs a lot more than O3 BTW

                    float OSTO3Average = OSTDataO3sum / OSTData.Count;
                    //average the AQI readings by dividing by number of readings
                    float OSTPM25Average = OSTDataPM25sum / OSTData.Count;

                    float ConvertedOSTO3 = ConvertPPBtoPPM(OSTO3Average);
                    float ConvertedOSTPM25 = ConvertPPBtoPPM(OSTPM25Average);

                    MorePollutantDataReading.Add(ConvertedOSTO3);    //index[0]
                    MorePollutantDataReading.Add(ConvertedOSTPM25);   //index[1]

                //sum all the O3(ozone) AQI readings from the list
                // ADDED UG/M3 TO PPB CONVERSION CONSTANT TO O3 DATA BEING DRAWN FROM DB TO MAKE DATA MATCH SIMM SENSORS 

                    //average the AQI readings by dividing by number of readings



                }
                //if sensor name contains simms
                else
                {
                    List<simms_Data_Feb_Apr_Final> simsData = new List<simms_Data_Feb_Apr_Final>();
                    simms_Data_Feb_Apr_Final startingPoint = db.simms_Data_Feb_Apr_Final
                        .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == sensorLocation)
                        .First();

                    int x = startingPoint.id;
                    //get 8 hr average AQI
                    for (int i = 0; i < mins; i++)
                    {
                        simms_Data_Feb_Apr_Final SimmsAQIdata = db.simms_Data_Feb_Apr_Final.Find(x);
                        if ((float)SimmsAQIdata.o3 != 0)
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
                    float SimsDataO3sum = (float)simsData.Sum(O3 => O3.o3);

                    //average the AQI readings by dividing by number of readings
                    float SimsO3Average = SimsDataO3sum / simsData.Count;

                    //sum all the CO AQI readings from the list
                    float SimsDataCOsum = (float)simsData.Sum(CO => CO.co);
                    //average the AQI readings by dividing by number of readings
                    float SimsCOToAverage = SimsDataCOsum / simsData.Count;

                    //sum all the no2 readings from the list
                    float SimsDataNO2sum = (float)simsData.Sum(NO2 => NO2.no2);
                    //average the AQI readings by dividing by number of readings
                    float SimsNO2Average = SimsDataNO2sum / simsData.Count;

                    //sum all the no2 readings from the list
                    float SimsDataPM25sum = (float)simsData.Sum(PM25 => PM25.pm25);
                    //average the AQI readings by dividing by number of readings
                    float SimsPM25Average = SimsDataPM25sum / simsData.Count;

                    //sum all the no2 readings from the list
                    float SimsDataSO2sum = (float)simsData.Sum(SO2 => SO2.so2);
                    //average the AQI readings by dividing by number of readings
                    float SimsSO2Average = SimsDataSO2sum / simsData.Count;

                    MorePollutantDataReading.Add(SimsO3Average);   //index[0]
                    MorePollutantDataReading.Add(SimsCOToAverage);   //index[1]
                    MorePollutantDataReading.Add(SimsNO2Average);     //index[2]
                    MorePollutantDataReading.Add(SimsPM25Average);      //index[3]
                    MorePollutantDataReading.Add(SimsSO2Average);      //index[4]

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
            }
            return MorePollutantDataReading;
        }

        public static float ConvertPPBtoPPM(float PollutantPPB)
        {
                //1 ppm = 1000 ppb
                float PollutantPPM = PollutantPPB / 1000;

                return PollutantPPM;
        }

        public static List<List<float>> HistoricData(Sensor s, int month)
        {
            //0 index is numbers for one week, 1 is number for one month
            List<float> oneWeekValues = new List<float>();
            List<float> oneMonthValues = new List<float>();

            float sensorData;

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
            List<List<float>> oneWeekAndMonthHistoricData = new List<List<float>> {oneWeekValues, oneMonthValues };
            return oneWeekAndMonthHistoricData;
        }

        public static float PullOSTSensorData(Sensor s, int mins, string dateTime)
        {
            List<Ost_Data_Feb_Apr_Final> OSTData = new List<Ost_Data_Feb_Apr_Final>();
            int x = 0;
            //pulling the data based on user current time and location of sensor
            try
            {
                Ost_Data_Feb_Apr_Final startingPoint = db.Ost_Data_Feb_Apr_Final
                    .Where(ut => ut.DateTime.Contains(dateTime) && ut.dev_id == s.Name)
                    .First();
                //pulls row of data
                x = startingPoint.id;
                //mins are either 480 or 60
            }
            catch
            {
                return 0;
            }
            for (int i = 0; i < mins; i++)
            {
                Ost_Data_Feb_Apr_Final OSTAQIdata = db.Ost_Data_Feb_Apr_Final.Find(x);
                if ((float)OSTAQIdata.o3 != 0)
                {
                    OSTData.Add(OSTAQIdata);
                    x++;
                }
                else
                {
                    x++;
                }
            }
            float average = (float)OSTData.Sum(O3 => (O3.o3) / OSTData.Count) * (float)0.509;
            return ConvertPPBtoPPM(average);
        }

        public static float PullSimmsSensorData(Sensor s, int mins, string dateTime)
        {
            List<simms_Data_Feb_Apr_Final> simsData = new List<simms_Data_Feb_Apr_Final>();
            int x = 0;
            try
            {
                simms_Data_Feb_Apr_Final startingPoint = db.simms_Data_Feb_Apr_Final
                    .Where(ut => ut.time.Contains(dateTime) && ut.dev_id == s.Name)
                    .First();

                x = startingPoint.id;
            }
            catch
            {
                return 0;
            }
            //get 8 hr average AQI
            for (int i = 0; i < mins; i++)
            {
                simms_Data_Feb_Apr_Final SimmsAQIdata = db.simms_Data_Feb_Apr_Final.Find(x);
                if ((float)SimmsAQIdata.o3 != 0)
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
            float SimsDataO3sum = (float)simsData.Sum(O3 => O3.o3);
            //average the AQI readings by dividing by number of readings
            float average = SimsDataO3sum / simsData.Count;

            return ConvertPPBtoPPM(average);
        }

        public static List<int> EightorOneHour(float oneHrPollutantPPM, float eightHrPollutantPPM)
        {
            List<int> indexAndOneOrEight = new List<int>();
            //using 7 for testing purposes to throw an error if if/else does not work
            int oneOrEightHour = 7;
            int breakPoingIndex = 0;
            //EPA if ppm > .125 use 1 hr readings
            //using 8 hr reading
            if (oneHrPollutantPPM <= (float)0.125)
            {
                //8 hr reading: 5 and 6 index are null values on table
                for (int i = 0; i < 5; i++)
                {
                    double low = pollutants[0].Low[i];
                    double high = pollutants[0].High[i];

                    //takes a reading a looks for the range of low and high
                    if (eightHrPollutantPPM >= (float)low && eightHrPollutantPPM <= (float)high)
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
                    if (oneHrPollutantPPM >= (float)low && oneHrPollutantPPM <= (float)high)
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
        public static float CalculateO3AQI(float PollutantPPM, int breakPointIndex, int isEight)
        {
            // must round to 3 digits for O3 EPA standards
            float Cp = (float)Math.Round(PollutantPPM, 3);

            // 7 = AQI standards in Pollutant Model
            float Ihi = (float)pollutants[7].High[breakPointIndex];
            float Ilo = (float)pollutants[7].Low[breakPointIndex];

            // index = breakpoint found from OneOrEight method
            float BPhi = (float)pollutants[isEight].High[breakPointIndex];
            float BPlow = (float)pollutants[isEight].Low[breakPointIndex];

            //calculate using 8 hr Ozone
            float AQIForO3Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;

            PollutantAQIs.Add(AQIForO3Pollutant);              //added O3 to PollutantAQI list
            return AQIForO3Pollutant;
        }

        //create new method to cycle through PollutantAQIlist, then find highest AQI out of all the pollutants 

        //----------------------------------------------------------------------------------------------------------------------------------------------------
        // converts to UG/M3
        public static float ConvertToUGM3(float PPM)
        {
            // Convert back to PPB then to UG/M3 -- 0.0409 is a conversion constant in order to go to UG/M3-- 48 is molecular weight of O3 
            float UGM3 = (PPM * 1000) * (float)0.0409 * 48;
            return UGM3;
        }

        // converts to PPM
        public static float UGM3ConvertToPPM(float UGM3)
        {
            float PPM = (UGM3 / ((float)0.0409 * 48)) / 1000;
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

        public static float EPAAQIData()
        {
            string s = "2019-03-20";

            DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            EPAGRDataFinal ePADataReading = db.EPAGRDataFinals
                .Where(ut => ut.Date == dt)
                .First();


            //int EPAO3DailyReading = ePADataReading.DAILY_AQI_VALUE;
            float EPA03Reading = (float)ePADataReading.Daily_Max_8_hour_Ozone_Concentration;
            return (EPA03Reading);
        }

        public static float CalculateEPA(float EPA, int breakpointIndex)
        {

            float pollutant = (float)Math.Round(EPA, 3);
            float Ihi = (float)pollutants[7].High[breakpointIndex];
            float Ilo = (float)pollutants[7].Low[breakpointIndex];
            float BPhi = (float)pollutants[0].High[breakpointIndex];
            float BPlow = (float)pollutants[0].Low[breakpointIndex];
            float Cp = pollutant;
            //calculate using 8 hr Ozone
            float AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }

        public static int EPABreakpointTable(float eightHrPollutantPPM)
        {
            //using 7 for testing purposes to throw an error if if/else does not work
            int breakpointIndex = 0;

            //8 hr reading: 5 and 6 index are null values on table
            for (int i = 0; i < 5; i++)
            {
                double low = pollutants[0].Low[i];
                double high = pollutants[0].High[i];

                //takes a reading a looks for the range of low and high
                if (eightHrPollutantPPM >= (float)low && eightHrPollutantPPM <= (float)high)
                {
                    //take an index for the range of low and high
                    breakpointIndex = i;
                    break;
                }
            }

            return breakpointIndex;
        }
        //C2H4O = ethylene oxide
        public static float ShortestDistancePollutantSensor(List<double> s)
        {
            List<Factory_Pollutant_Final> pollutantSensors = db.Factory_Pollutant_Finals.ToList();

            double largeNum = double.MaxValue;
            Factory_Pollutant_Final pollutantSensor = new Factory_Pollutant_Final();
            foreach (Factory_Pollutant_Final f in pollutantSensors)
            {
                double sensorDistance = GeocodeController.LatLongDistance(s[0],s[1],f.Latitude,f.Longitude);
                if (sensorDistance < largeNum)
                {
                    largeNum = sensorDistance;
                    pollutantSensor = f;
                }
            }

            float ethyleneOxide = (float)pollutantSensor.EtO_ugm3;
            return ethyleneOxide;
        }
        public static string PollutantWarning(float ethyleneOxideUGM3)
        {
            //0.18 µg / m3  normal background concentration of ethylene oxide
            if (ethyleneOxideUGM3 > (float)0.18)
            {
                //grab current 24 hr average float of NO2
                //grab current 24 hr average float of CO
                //grab wind speed and direction

                //air should contain less than 0.1 ppm ethylene oxide averaged over a 10-hour workday

                float c2H4Oppm = (ethyleneOxideUGM3 / (float)(0.0409 * 44.05)) / 1000; //g/mol
                if (c2H4Oppm > (float)0.1)
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