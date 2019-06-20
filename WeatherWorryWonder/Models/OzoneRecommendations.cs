using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherWorryWonder.Models
{
    public class OzoneRecommendations      //Callista
    {
        //EPA's Rating System:

        //OzoneLevels[0] = Good
        //OzoneLevels[1] = Moderate
        //OzoneLevels[2] = Unhealthy for Sensitive Groups
        //OzoneLevels[3] = Unhealthy
        //OzoneLevels[4] = Very Unhealthy
        //OzoneLevels[5] = Hazardous


        public static String[] OzoneLevels = new String[6] { "Air quality is considered satisfactory, and air pollution poses little or no risk.",
                                                    "Air quality is acceptable; however, for some pollutants there may be a moderate health concern for a very small number of people who are unsually sensitive to air pollution.",
                                                    "Members of sensitive groups may experience health effects. The general public is not likely to be affected.",
                                                    "Everyone may begin to experience health effects; members of sensitive groups may experience more serious health effects",
                                                    "Health warnings of emergency conditions. The entire population is more likely to be affected.",
                                                    "Health aleart: everyone may experience more serious health effects." };

        //Reducing Ozone Levels while driving:

        public static String[] OzoneDriving = new String[7] {"Carpool and combine errands to reduce trips.",
                                                    "Bike, walk or ride the bus when possible.",
                                                    "Keep personal vehicles well-tuned and tires inflated properly. You can save up to 20% on the amount of gasoline you use.",
                                                    "Pressure check vehicle gas caps annually and replace when necessary. A faulty gas cap can allow up to 30 gallons of fuel per year to evaporate.",
                                                    "Refuel as late in the day as possible (after 7 pm preferably), especially on ozone alert days.",
                                                    "Stop at the click. Don't top off your tank when you refuel. This keeps harmful fumes from being forced into the air.",
                                                    "Avoid excessive idling of your automobile." };

        //Reducing Ozone levels at home:

        public static String[] OzoneHome = new String[8] {"Reduce the amount of energy you use at home.Much of our energy comes from fossil fuel-burning power plants that produce ozone.",
                                                   "Purchase ENERGY STAR equipment.",
                                                   "Use compact fluorescent light bulbs.Turn off lights and appliances when they are not in use.",
                                                   "Purchase plants such as the snake plant, spider plant, and golden pothos which can help deplete indoor ozone.",
                                                   "Adjust the thermostat to a slightly higher setting in summer and consider installing a programmable thermostat.",
                                                   "Avoid chemicals that contain volatile organic compounds(VOCs) such as spray paint, paint thinners, glue solvents, and pesticides.",
                                                   "Reduce or eliminate fireplace and wood stove use.",
                                                   "Check the Air Quality Index (AQI) for your area and avoid spending too much time outdoors on high-level ozone days."};

        //Reducing Ozone levels at work:


        public static String[] OzoneWork = new String[6] {"Allow and promote teleconferencing instead of driving to meetings. If you must drive, carpool when possible.",
                                                  "Bring your lunch, carpool or walk to lunch, especially on ozone alert days.",
                                                  "Inquire about flexible work schedules that would promote driving less, such as the four-day work week.",
                                                  "Commute in style: bike, walk, carpool or take public transportation to work.Get in some exercise, good conversation or a little reading in the process!",
                                                  "Purchase and use low-volatile organic compound(VOC) paints, solvents, pesticides, etc.",
                                                  "Select printing companies that use soy-based inks or other low-emissions print processes."};

        //Reducing Ozone levels while grilling:

        public static String[] OzoneGrilling = new String[5] {"Do not use lighter fluid. It pollutes on both evaporation and burning. Your food will taste better without it, too!",
                                                       "Use a charcoal chimney instead of lighter fluid to start the coals. They are easy to use and leave no telltale taste in the food.",
                                                       "Choose briquettes that are additive-free and avoid any added chemicals flavors to the food.",
                                                       "Gas grills emit less pollution than charcoal grills.",
                                                       "Postpone grilling until evening on ozone alert days."};

        //Reducing Ozone levels while doing yardwork:

        public static String[] OzoneYardWork = new String[6] {"Mow as late as possible, preferably after 7 pm, when there is less sun and heat.",
                                                     "Replace older gas cans with new no-spill gas cans for refueling equipment. Emissions from gasoline spills are major contributors to ozone.",
                                                     "Practice low-maintenance lawn care, requiring less frequent mowing and less inputs of polluting chemical pesticides.",
                                                     "Consider replacing any gasoline powered equipment with electric, batter or manual powered equipment.",
                                                     "Convert lawn spaces to native plants to reduce the amount of mowing and watering.",
                                                     "Avoid open burning and mulch or compost leaves and yard waste."};
    }
}


