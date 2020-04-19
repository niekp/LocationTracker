var config = document.querySelector("[data-configuration]");

var platform = new H.service.Platform({
    apikey: config.dataset.hereApiKey,
    app_id: config.dataset.hereAppId,
    app_code: config.dataset.hereAppCode
});

var defaultLayers = platform.createDefaultLayers();

var map = new H.Map(
    document.getElementById('mapContainer'),
    defaultLayers.vector.normal.map,
);
var rides = document.querySelectorAll("[data-container='coordinates'] ride");
var markers = document.querySelectorAll("[data-container='coordinates'] markers coord");
var till = null;

function drawRides() {
    var group = new H.map.Group();
    map.removeObjects(map.getObjects())

    // Rides

    rides.forEach(ride => {
        var startPoint = null;
        routeString = new H.geo.LineString();

        ride.querySelectorAll("coord").forEach(coord => {
            var latitude = parseFloat(coord.dataset.lat);
            var longitude = parseFloat(coord.dataset.long);
            if (till) {
                var time = new Date(coord.dataset.time).getTime();
                if (time > till) {
                    return;
                }
            }
            
            coords = { lat: latitude, lng: longitude };

            routeString.pushLatLngAlt(latitude, longitude);

            if (startPoint == null)
                startPoint = coords;
            endPoint = coords;
        });

        var color = randDarkColor();
        var routeOutline = new H.map.Polyline(routeString, {
            style: {
                lineWidth: 8,
                strokeColor: hexToRgbA(color, 0.7),
                lineTailCap: 'arrow-tail',
                lineHeadCap: 'arrow-head'
            }
        });

        var routeLine = new H.map.Polyline(routeString, {
            style: {
                strokeColor: '#ffffff',
                lineWidth: 4,
                lineDash: [0, 4],
                lineTailCap: 'arrow-tail',
                lineHeadCap: 'arrow-head',
            }
        });

        var startMarker = new H.map.Marker(startPoint);
        var endMarker = new H.map.Marker(endPoint);

        if (rides.length == 1)
            group.addObjects([routeOutline, routeLine, startMarker, endMarker]);
        else
            group.addObjects([routeOutline, routeLine]);
    });

    // Markers

    markers.forEach(marker => {
        var marker = new H.map.Marker({
            lat: parseFloat(marker.dataset.lat),
            lng: parseFloat(marker.dataset.long)
        });

        group.addObject(marker);
    });

    map.addObject(group);

    map.getViewModel().setLookAtData({
        bounds: group.getBoundingBox()
    });
}


var ui = H.ui.UI.createDefault(map, defaultLayers);
var behavior = new H.mapevents.Behavior(new H.mapevents.MapEvents(map));

drawRides();

var slider = document.querySelector("#timeSlider");
if (slider) {
    var coords = rides[0].querySelectorAll("coord");
    var start = new Date(coords[0].dataset.time).getTime();
    var end = new Date(coords[coords.length - 1].dataset.time).getTime();
    slider.min = start;
    slider.max = end;
    slider.value = end;

    $(slider).change(function () {
        var timestamp = $(this).val();
        var datum = new Date(parseInt(timestamp));
        $("#timeSliderValue").html(DispDate(datum));
        $("#splitRide").show();
        till = timestamp;
        drawRides();
    });
}

$("[data-trigger='split-ride']").on('click', function () {
    ride = $(this).data("ride-id");
    if (confirm("Weet je zeker dat je de rit wilt splitten?")) {
        window.location.href = "/Ride/Split/?rideId=" + ride + "&timestamp=" + till;
    }
});


function DispDate(dateInput) {
    var datetime = new Date(dateInput);
    var day = datetime.getDate();
    var month = datetime.getMonth() + 1;
    var year = datetime.getFullYear();

    var hour = datetime.getHours().toString().padLeft(2, "0");
    var minute = datetime.getMinutes().toString().padLeft(2, "0");

    day = day.toString().padLeft(2, "0");
    month = month.toString().padLeft(2, "0");
    year = year.toString().padLeft(4, "0");

    if (hour == 0 && minute == 0) {
        return day + '-' + month + '-' + year;
    } else {
        return day + '-' + month + '-' + year + ' ' + hour + ':' + minute;
    }
}