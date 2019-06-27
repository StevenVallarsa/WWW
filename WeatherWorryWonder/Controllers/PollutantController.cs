using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        public static List<Ost_Data_March_Final> OSTPollutantData = new List<Ost_Data_March_Final>();
        public static List<Ost_Data_March_Final> SelectedOstReadings = new List<Ost_Data_March_Final>();
        public static List<simms_Data_Feb_Apr_Final> SimmsPollutantData = new List<simms_Data_Feb_Apr_Final>();
        public static List<simms_Data_Feb_Apr_Final> SelectedSimmsReadings = new List<simms_Data_Feb_Apr_Final>();
        public static List<double> PollutantAverages = new List<double>();
        public static List<double> PollutantAQIs = new List<double>();
        public static int FirstRow = 0;
        public static double eighthourO3 = 0;
        public static double pollutantlat = 0;
        public static double pollutantlong = 0;


        //take in the sensor that is closest to the user
        public static void PullData (Sensor s)
        {
            //take the current hour            
            string currentHour = DateTime.Now.ToString("HH");
            //take the DeLorean and go back to a date in the past
            string currentTime = $"2019-03-28T{currentHour}";
            //pulls sensor name
            string sensorLocation = s.Name;

            Ost_Data_March_Final ostData = new Ost_Data_March_Final();
            simms_Data_Feb_Apr_Final simmsData = new simms_Data_Feb_Apr_Final();
            
            if (sensorLocation.Contains("graq"))
            {
                var startingPoint = db.Ost_Data_March_Final
                .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == s.Name)
                .FirstOrDefault();

                if (startingPoint != null)
                {
                    FirstRow = startingPoint.id;
                    //pull rows of data
                    OSTPollutantData = db.Ost_Data_March_Final
                        .Where(x => x.id >= FirstRow && x.id <= (FirstRow + 480)).Take(480).ToList();
                }
            }
            else
            {
                var startingPoint = db.simms_Data_Feb_Apr_Final
                .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == s.Name)
                .FirstOrDefault();

                if (startingPoint != null)
                {
                    int firstRow = startingPoint.id;
                    //pull rows of data
                    SimmsPollutantData = db.simms_Data_Feb_Apr_Final
                        .Where(x => x.id >= firstRow && x.id <= firstRow + 480).Take(480).ToList();
                    
                }
            }
            
        }

        public static double PullHistoricData(Sensor s, int pulls, string date)
        {
            //take the current hour            
            string currentHour = DateTime.Now.ToString("HH");
            //take the DeLorean and go back to a date in the past
            string currentTime = date;
            //pulls sensor name
            string sensorLocation = s.Name;

            if (sensorLocation.Contains("graq"))
            {
                var startingPoint = db.Ost_Data_March_Final
                .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == s.Name)
                .FirstOrDefault();

                if (startingPoint != null)
                {
                    int firstRow = startingPoint.id;
                    //pull rows of data
                    List<Ost_Data_March_Final> ostData = db.Ost_Data_March_Final
                        .Where(x => x.id >= firstRow && x.id <= firstRow + pulls).Take(pulls).ToList();
                    double average = ostData.Sum(o => o.o3) / ostData.Count;
                    double o3PPM = UGM3ConvertToPPM(average, 48);
                    int breakPointIndex = EPABreakpointTable(o3PPM);
                    double o3APIAQI = Math.Round(CalculateO3AQI(o3PPM, breakPointIndex, 0));
                    return o3APIAQI;
                }
                return 0;
            }
            else
            {
                var startingPoint = db.simms_Data_Feb_Apr_Final
                .Where(ut => ut.time.Contains(currentTime) && ut.dev_id == s.Name)
                .FirstOrDefault();

                if (startingPoint != null)
                {
                    int firstRow = startingPoint.id;
                    //pull rows of data
                    List<simms_Data_Feb_Apr_Final> simmsData = db.simms_Data_Feb_Apr_Final
                        .Where(x => x.id >= firstRow && x.id <= firstRow + pulls).Take(pulls).ToList();
                    double average = simmsData.Sum(o => o.o3) / simmsData.Count;
                    double o3PPM = UGM3ConvertToPPM(average, 48);
                    int breakPointIndex = EPABreakpointTable(o3PPM);
                    double o3APIAQI = CalculateO3AQI(o3PPM, breakPointIndex, 0);
                    return o3APIAQI;
                }
                return 0;
            }

        }

        public static void PollutantListReadings(int numberofReadings)
        {
            if (OSTPollutantData.Count > 0)
            {
                SelectedOstReadings = OSTPollutantData.GetRange(0, numberofReadings).ToList();
            }
            if (SimmsPollutantData.Count > 0)
            {
                SelectedSimmsReadings = SimmsPollutantData.GetRange(0, numberofReadings).ToList();
            }
        }



        public static void PollutantAverageReadings(Sensor s)
        {

            string sensorLocation = s.Name;
            if (sensorLocation.Contains("graq"))
            {
                //sum all the 03 readings from the list
                double ostO3Sum = (double)SelectedOstReadings.Sum(x => x.o3);
                //average the AQI readings
                double ostO3Average = UGM3ConvertToPPM((ostO3Sum / SelectedOstReadings.Count), 48);
                double ostPM25Sum = (double)SelectedOstReadings.Sum(x => x.pm25);
                double ostPM25Average = ostPM25Sum/ SelectedOstReadings.Count;
                double ost8hrO3Sum = (double)OSTPollutantData.Sum(x => x.o3);
                eighthourO3 = UGM3ConvertToPPM((ost8hrO3Sum / OSTPollutantData.Count), 48); //8 hr ppm
                PollutantAverages.Add(eighthourO3);   //index[0] 8 hr ppm
                PollutantAverages.Add(ostO3Average);   //index[1] 1 hr ppm
                PollutantAverages.Add(ostPM25Average); //index[2] ug/m3
            }
            else
            {
                double sims1hrO3Sum = (double)SelectedSimmsReadings.Sum(x => x.o3);
                double simsO31hrAverage = sims1hrO3Sum / SelectedSimmsReadings.Count;
                double simsCOsum = (double)SelectedSimmsReadings.Sum(x => x.co);
                double simsCOToAverage = simsCOsum / SelectedSimmsReadings.Count;
                double simsNO2sum = (double)SelectedSimmsReadings.Sum(x => x.no2);
                double simsNO2Average = simsNO2sum / SelectedSimmsReadings.Count;
                double simsPM25sum = (double)SelectedSimmsReadings.Sum(x => x.pm25);
                double simsPM25Average = simsPM25sum / SelectedSimmsReadings.Count;
                double simsSO2sum = (double)SelectedSimmsReadings.Sum(x => x.so2);
                double simsSO2Average = simsSO2sum / SelectedSimmsReadings.Count;

                double simms8hrO3Sum = (double)SimmsPollutantData.Sum(x => x.o3);
                eighthourO3 = ConvertPPBtoPPM(simms8hrO3Sum / SimmsPollutantData.Count); //8 hr ppm
                PollutantAverages.Add(eighthourO3);   //index[0] 8 hr ppm
                PollutantAverages.Add(ConvertPPBtoPPM(simsO31hrAverage)); //index[1] 1 hr ppm
                PollutantAverages.Add(simsCOToAverage); //index[2] 8 hr
                PollutantAverages.Add(simsNO2Average);  //index[3] ppb 1 hr
                PollutantAverages.Add(simsPM25Average); //index[4] ug/m3 24 hr
                PollutantAverages.Add(simsSO2Average);  //index[5] ppb 1 hr
            }
        }

        public static List<double> HistoricData(Sensor s)
        {
            //0 index is numbers for one week, 1 is number for one month
            List<double> oneWeekValues = new List<double>();

            double sensorData;

            int Day = 21;
            string currentHour = "20"; //DateTime.Now.ToString("HH");
            for (int i = 0; i < 7; i++)
                {
                    if (Day > 31)
                    {
                        Day = 1;
                    }
                    //grabs the day one month ago and then increments it
                    string sDay = Day.ToString();
                    if (sDay.Length == 1)
                    {
                        sDay = "0" + Day;
                    }

                    string monthAgoTime = $"2019-03-{sDay}T{currentHour}";
                    sensorData = PullHistoricData(s, 20, monthAgoTime);
                    if (sensorData != 0)
                    {
                        oneWeekValues.Add(Math.Round(sensorData));
                    }
                    Day++;
                }
            return oneWeekValues;
        }

        public static List<int> EightorOneHour(double oneHrPollutantPPM, double eightHrPollutantPPM)
        {
            List<int> indexAndOneOrEight = new List<int>();
            //using 7 for testing purposes to throw an error if if/else does not work
            int oneOrEightHour = 7;
            int breakPoingIndex = 0;
            //EPA if ppm > .125 use 1 hr readings
            //using 8 hr reading
            if (oneHrPollutantPPM <= (double)0.125)
            {
                //8 hr reading: 5 and 6 index are null values on table
                for (int i = 0; i < 5; i++)
                {
                    double low = pollutants[0].Low[i];
                    double high = pollutants[0].High[i];

                    //takes a reading a looks for the range of low and high
                    if (eightHrPollutantPPM >= (double)low && eightHrPollutantPPM <= (double)high)
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
                    if (oneHrPollutantPPM >= (double)low && oneHrPollutantPPM <= (double)high)
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
        public static int BreakpointIndex(double Pollutant, int pollutantIndex)
        {
            int breakPointIndex = 0;

            for (int i = 0; i < 6; i++)
            {
                double low = pollutants[pollutantIndex].Low[i];
                double high = pollutants[pollutantIndex].High[i];

                if (Pollutant >= low && Pollutant <= high)
                {
                    breakPointIndex = i;
                    break;
                }
            }

            return breakPointIndex;
        }

        // for AQI equation from EPA
        // isEight: 0 index = 8h reading; 1 index = 1h reading
        public static double CalculateO3AQI(double PollutantPPM, int breakPointIndex, int isEight)
        {
            // must round to 3 digits for O3 EPA standards
            double Cp = (double)Math.Round(PollutantPPM, 3);

            // 7 = AQI standards in Pollutant Model
            double Ihi = (double)pollutants[7].High[breakPointIndex];
            double Ilo = (double)pollutants[7].Low[breakPointIndex];

            // index = breakpoint found from OneOrEight method
            double BPhi = (double)pollutants[isEight].High[breakPointIndex];
            double BPlow = (double)pollutants[isEight].Low[breakPointIndex];

            //calculate using 8 hr Ozone
            double AQIForO3Pollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;

            PollutantAQIs.Add(AQIForO3Pollutant);              //added O3 to PollutantAQI list
            return AQIForO3Pollutant;
        }

        public static double ConvertPPBtoPPM(double PollutantPPB)
        {
            //1 ppm = 1000 ppb
            double PollutantPPM = PollutantPPB / 1000;
            return PollutantPPM;
        }
        public static double ConvertPPMtoPPB(double PollutantPPM)
        {
            //1 ppm = 1000 ppb
            double PollutantPPB = PollutantPPM * 1000;
            return PollutantPPB;
        }
        // converts to UG/M3
        public static double ConvertToUGM3(double PPM)
        {
            // Convert back to PPB then to UG/M3 -- 0.0409 is a conversion constant in order to go to UG/M3-- 48 is molecular weight of O3 
            double UGM3 = (PPM * 1000) * (double)0.0409 * 48;
            return UGM3;
        }

        // converts to PPM
        public static double UGM3ConvertToPPM(double UGM3, double moleWeight)
        {
            double PPM = (UGM3 / ((double)0.0409 * moleWeight)) / 1000;
            return PPM;
        }

        public static double EPAAQIData()
        {
            string s = "2019-03-20";

            DateTime dt = DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            EPAGRDataFinal ePADataReading = db.EPAGRDataFinals
                .Where(ut => ut.Date == dt)
                .First();

            //int EPAO3DailyReading = ePADataReading.DAILY_AQI_VALUE;
            double EPA03Reading = (double)ePADataReading.Daily_Max_8_hour_Ozone_Concentration;
            return (EPA03Reading);
        }
        public static double CalculateEPA(double EPA, int breakpointIndex, int pollutantIndex)
        {
            double pollutant = (double)Math.Round(EPA, 3);
            double Ihi = (double)pollutants[7].High[breakpointIndex];
            double Ilo = (double)pollutants[7].Low[breakpointIndex];
            double BPhi = (double)pollutants[pollutantIndex].High[breakpointIndex];
            double BPlow = (double)pollutants[pollutantIndex].Low[breakpointIndex];
            double Cp = pollutant;
            //calculate using 8 hr Ozone
            double AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }
        public static double CalculateEPA(double EPA, int breakpointIndex)
        {
            double pollutant = (double)Math.Round(EPA, 3);
            double Ihi = (double)pollutants[7].High[breakpointIndex];
            double Ilo = (double)pollutants[7].Low[breakpointIndex];
            double BPhi = (double)pollutants[0].High[breakpointIndex];
            double BPlow = (double)pollutants[0].Low[breakpointIndex];
            double Cp = pollutant;
            //calculate using 8 hr Ozone
            double AQIForPollutant = ((Ihi - Ilo) / (BPhi - BPlow)) * (Cp - BPlow) + Ilo;
            return AQIForPollutant;
        }

        public static int EPABreakpointTable(double eightHrPollutantPPM)
        {
            //using 7 for testing purposes to throw an error if if/else does not work
            int breakpointIndex = 0;

            //8 hr reading: 5 and 6 index are null values on table
            for (int i = 0; i < 5; i++)
            {
                double low = pollutants[0].Low[i];
                double high = pollutants[0].High[i];

                //takes a reading a looks for the range of low and high
                if (eightHrPollutantPPM >= (double)low && eightHrPollutantPPM <= (double)high)
                {
                    //take an index for the range of low and high
                    breakpointIndex = i;
                    break;
                }
            }

            return breakpointIndex;
        }
        //C2H4O = ethylene oxide
        public static double ShortestDistancePollutantSensor(List<double> s)
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
                    pollutantlat = f.Latitude;
                    pollutantlong = f.Longitude;
                }
            }

            double ethyleneOxide = (double)pollutantSensor.EtO_ugm3;

            return ethyleneOxide;
        }
        public static string PollutantWarning(double ethyleneOxideUGM3)
        {
            //0.18 µg / m3  normal background concentration of ethylene oxide
            if (ethyleneOxideUGM3 > (double)0.18)
            {
                //grab current 24 hr average double of NO2
                //grab current 24 hr average double of CO
                //grab wind speed and direction

                //air should contain less than 0.1 ppm ethylene oxide averaged over a 10-hour workday

                double c2H4Oppm = (UGM3ConvertToPPM(ethyleneOxideUGM3, 44f));
                if (c2H4Oppm > (double)0.1)
                {
                    return "High Pollutant Level Alert!";
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