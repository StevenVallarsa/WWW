@using WeatherWorryWonder.Models
@model ResultView
@{
    List<double> userLocation = (List<double>)Session["UserLocation"];
    double lat = userLocation[0];
    double lng = userLocation[1];

    double sLat1 = Model.ThreeClosestSensors[0].Lat;
    double sLng1 = Model.ThreeClosestSensors[0].Long;

    double sLat2 = Model.ThreeClosestSensors[1].Lat;
    double sLng2 = Model.ThreeClosestSensors[1].Long;

}

<p></p>
    <div class="container">

        <div class="row">

        <div class="col-sm-6">

            <div style="padding:10px; background-color: @Model.AQIColor1">
                <h3 style="text-align:center; font-size: 3rem">Your Local AQI Is</h3>
                <h1 style="text-align:center; font-size: 6rem"><b>@Model.O3AQI</b></h1>
                <p class="display-6" style="text-align:center"><b>Sensor Location:</b> @Model.SensorName</p>
                <h5 style="text-align:center">@ViewBag.Suggestions</h5>
            </div>

        <div>
            <div class="row">
                <div class="card" style="width: 25%">
                    <div class="card-body" style="background-color:lightblue">
                        <h5 class="card-title">NO<sub>2</sub></h5>
                        <p class="card-text">@Model.NO2AQI</p>
                    </div>
                </div>
                <div class="card" style="width: 25%">
                    <div class="card-body" style="background-color:lightgrey">
                        <h5 class="card-title">PM2.5</h5>
                        <p class="card-text">@Model.PM25AQI</p>
                    </div>
                </div>
                <div class="card" style="width: 25%">
                    <div class="card-body" style="background-color:lightblue">
                        <h5 class="card-title">SO<sub>2</sub></h5>
                        <p class="card-text">@Model.SO2AQI</p>
                    </div>
                </div>
                <div class="card" style="width: 25%">
                    <div class="card-body" style="background-color:lightgray">
                        <h5 class="card-title">CO</h5>
                        <p class="card-text">@Model.CO</p>
                    </div>
                </div>

            </div>

            <div class="futureAQI">
                <h2>Forecast:</h2>
                <p>Tomorrow AQI: @Model.PredictedAQITomorrow</p>
                <p>3-Day AQI: @Model.PredictedAQI3Day</p>
                <p>5-Day AQI: @Model.PredictedAQI5Day</p>
            </div>


            <div class="recommendations">
                <h2>Health Recommendation based on your AQI:</h2>
                <p>
                    @Model.Recommendations
                </p>
            </div>
        </div>

        <div class="col-sm-6">
            <div id="map"></div> 
        </div>

        </div>

            <div class="weather">
                <h2>Current Weather:</h2>
                <p>Temperature: @Math.Round(Model.Weather[0].TemperatureF) F</p>
                <p>Cloud Coverage: @Model.Weather[0].Clouds</p>
                <p>Wind: @Math.Round(Model.Weather[0].WindDirection) degrees at @Model.Weather[0].WindSpeed m/s</p>
                <p>Humidity: @Model.Weather[0].Humidity %</p>
            </div>

            <div class="VOC">
                <p>Ethylene Oxide PPM: @Model.C2H4OPPM</p>
                <p>Pollutant Message: @Model.PollutantWarning</p>
            </div>

        </div>
    </div>


        <script>
function initMap() {
    var userLatLong = { lat: @lat, lng: @lng };
    var sensor1LatLong = { lat: @sLat1, lng: @sLng1 };
    var sensor2LatLong = { lat: @sLat2, lng: @sLng2 };

    var map = new google.maps.Map(
        document.getElementById('map'),
        { zoom: 13, center: userLatLong });
    var iconBase = "http://maps.google.com/mapfiles/kml/pal2/";

    var markerUser = new google.maps.Marker({
        position: userLatLong,
        icon: { url: iconBase + "icon10.png"},
        map: map
    });

    var markerSensor1 = new google.maps.Marker({
        position: sensor1LatLong @*{ lat: @Model.ThreeClosestSensors[0].Lat, @Model.ThreeClosestSensors[0].Long }*@,
        title: "@Model.O3AQI",
        label: { color: 'black', fontWeight: 'bold', fontSize: '20px', text: '@Model.O3AQI' },
        icon: {
            path: google.maps.SymbolPath.CIRCLE,
            scale: 20,
            fillColor: "@Model.AQIColor1",
            fillOpacity: .8,
            strokeWeight: 0
        },
        map: map
    });

    var markerSensor2 = new google.maps.Marker({
        position: sensor2LatLong @*{ lat: @Model.ThreeClosestSensors[1].Lat, @Model.ThreeClosestSensors[1].Long }*@,
        title: "@Model.Second03AQI",
        label: { color: 'black', fontWeight: 'bold', fontSize: '20px', text: '@Model.Second03AQI' },
        icon: {
            path: google.maps.SymbolPath.CIRCLE,
            scale: 20,
            fillColor: "@Model.AQIColor2",
            fillOpacity: 1,
            strokeWeight: 0
        },
        map: map
    });


        @*var markerSensor3 = new google.maps.Marker({
        position: sensor3LatLong,
        //position: sensorLatLong,
        title: "@Model.Third03AQI",
        label: { color: 'black', fontWeight: 'bold', fontSize: '20px', text: '@Model.Third03AQI' },
        icon: {
            path: google.maps.SymbolPath.CIRCLE,
            scale: 20,
            fillColor: "@Model.AQIColor3",
            fillOpacity: 1,
            strokeWeight: 0
        },
        map: map
    });*@
}
        </script>


        <script async defer
                src="https://maps.googleapis.com/maps/api/js?key=@APIKeys.SteveGoogleMapAPI&callback=initMap">
        </script>
