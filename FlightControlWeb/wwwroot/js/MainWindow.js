// creating dictionary to store all Flights to present in the map
let pinsMap = new Map();
// creating list to store all Flights to present in the tables
let dict = [];
let map;
let flightShowing = null;
let onDelete = null;

function GetMap() {
    map = new Microsoft.Maps.Map("#myMap", {});

    map.setView({

        center: new Microsoft.Maps.Location(32.006833306, 34.885329792),
        zoom: 3
    });
    document.getElementById("myMap").addEventListener
        ("click", stopShowingFlightPlan);
}

function creatingPushpin(location, imgUrl, scale, callback) {
    let img = new Image();
    img.onload = function () {
        let c = document.createElement("canvas");
        c.width = img.width * scale;
        c.height = img.height * scale;
        let context = c.getContext("2d");

        //Draw scaled image
        context.drawImage(img, 0, 0, c.width, c.height);

        let pin = new Microsoft.Maps.Pushpin(location, {
            //Generate an image
            icon: c.toDataURL(),

            //Anchor the image.
            anchor: new Microsoft.Maps.Point(c.width / 2, c.height)
        });

        if (callback) {
            callback(pin);
        }
    };
    img.src = imgUrl;
}

/*
 * code that updates flight tables
*/

// sync every second
let syncLoop = setInterval(sendFlightsRequest, 3000);

function sendFlightsRequest() {
    let currentDate = new Date();
    /// cutting the last chars of the seconds
    let dateStr = (currentDate.toISOString().slice(0, 19) + "Z");
    // builting the GET request - to get all flights
    let requestStr = "/api/Flights?relative_to=" + "2020-05-21T03:45:00Z" + "&sync_all";

    let xhttp = new XMLHttpRequest();
    // when the state of the request changes
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) { // http request is DONE
            if (this.status == 200 || this.status == 400 || this.status == 500) {
                loadFlights(xhttp.responseText);
            } else {
                showError("Failed getting data from server");
            }
        }
    };
    xhttp.open("GET", requestStr, true);
    xhttp.send();
}

function loadFlights(json) {
    // list of all id got from the response
    let flightIdFromResponse = [];
    let flightObj = JSON.parse(json);

    for (i in flightObj) {
        // getting flight from the response
        let myObj = flightObj[i];

        flightIdFromResponse.push(myObj.flight_id);

        //the flight exists in the dict - only need to update the PIN location,
        //NOT add another row
        if (myObj.flight_id in dict) {
            document.getElementById(myObj.flight_id + "Long")
                .innerHTML = myObj.longitude;
            document.getElementById(myObj.flight_id + "Lat")
                .innerHTML = myObj.latitude;

            let newLocation = new Microsoft.Maps.Location
                (myObj.latitude, myObj.longitude);

            dict[myObj.flight_id].setLocation(newLocation);
        }
        // the flight does not exist in the dictionary - need to add Row and Add
        //to dict
        else {
            //Create a location object of where to place the pushpin on the map
            let location = new Microsoft.Maps.Location
                (myObj.latitude, myObj.longitude);

            creatingPushpin
                (location, "/Images/airplane1.png", 0.35, function (pin) {
                    pin.id = myObj.flight_id;
                    map.entities.push(pin);
                    dict[myObj.flight_id] = pin;
                    Microsoft.Maps.Events.addHandler(pin, "click", function () {
                        showFlightPlan(pin);
                    });
                });

            tableRow = "<tr id=\"" + myObj.flight_id +
                "\" onclick=\"rowClicked('" + myObj.flight_id + "')\"><td>" +
                myObj.flight_id + "</td>" + "<td>" + myObj.company_name +
                "</td>" + "<td id=" + myObj.flight_id + "Long>" +
                myObj.longitude + "</td>" + "<td id=" + myObj.flight_id +
                "Lat>" + myObj.latitude + "</td>" + "<td>" + myObj.passengers +
                "</td>" + "<td>" + myObj.date_time + "</td>";

            updateTableRow(myObj, tableRow);
        }
    }

    // going throught the dict and checking the there are flights that not in
    //the response. If there is - delete from dict, its row and pin
    for (flightId in dict) {
        //Return the value for the current key
        let exists = flightIdFromResponse.includes(flightId);

        if (!exists) {
            flightFinished(flightId);
        }
    }
}

function flightFinished(flightId) {
    if ((flightShowing != null) && (flightId == flightShowing.id)) {
        stopShowingFlightPlan();
    }
    let row = document.getElementById(flightId);
    // removing flight row from the table
    row.parentNode.removeChild(row);
    // removing the flight pin
    map.entities.remove(dict[flightId]);
    // removing flight from dictionary
    delete dict[flightId];
}

function updateTableRow(myFlightObj, tableRow) {
    //External Flight
    if (myFlightObj.is_external == true) {
        document.getElementById("ExternalFlightsBody").innerHTML +=
            (tableRow + "</tr>");
    } else {
        document.getElementById("MyFlightsBody").innerHTML +=
            (tableRow + "<td><input type=\"button\" class=\"close\" " +
                "value=\"x\" onclick=\"deleteFlight(this,'" +
            myFlightObj.flight_id + "')\" onmouseover=\"onDeleteButton('" +
                myFlightObj.flight_id +
                "')\" onmouseout=\"OffDeleteButton()\"></td ></tr > ");
    }
}

//if the button is clicked, do not want to trigger the event of the row being
//clicked
function onDeleteButton(id) {
    onDelete = id;
}
function OffDeleteButton() {
    onDelete = null;
}

/*
 * code that shows the route of a flight when clicked
 */

function rowClicked(id) {
    //if "onDelete" is equal to the id, the delete button was clicked and
    //shouldn't show flight
    if (onDelete != id) {
        if (flightShowing != null) {
            stopShowingFlightPlan();
        }
        showFlightPlan(dict[id]);
    }
}

function graphicChange(pin) {
    //highlight the row of the flight in the table
    let elm = document.getElementById(pin.id);
    elm.className += "bg-info";
    changeIcon("start", pin, 0.6); //make flight icon bigger and change color
}

//change the size of the flight icon
function changeIcon(status, pin, scale) {
    let imgUrl;
    if (status == "start") {
        imgUrl = "/Images/airplaneClicked.png";
    } else {
        imgUrl = "/Images/airplane1.png";
    }
    let img = new Image();
    img.onload = function () {
        let c = document.createElement("canvas");
        c.width = img.width * scale;
        c.height = img.height * scale;
        let context = c.getContext("2d");
        //Draw scaled image
        context.drawImage(img, 0, 0, c.width, c.height);
        pin.setOptions({
            icon: c.toDataURL(),
        });
    }
    img.src = imgUrl;
}

//get the flight plan from the server
function showFlightPlan(pin) {
    let url = "/api/FlightPlan/" + pin.id;
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4) { //http sent and answer recieved
            if (this.status == 200) {
                let flightPlan = JSON.parse(this.responseText);
                flightShowing = pin;
                addToMap(flightPlan);
                addFlightDetails(flightPlan, pin.id);
                graphicChange(pin);
            } else { //error accured
                showError("failed to recived flight plan of the flight "
                    + pin.id);
            }
        }
    };
    xmlhttp.open("GET", url, true);
    xmlhttp.send();
}

//add lines to the map according to the route of the plane
function addToMap(flightPlan) {
    //first point is initial point
    let location2 = new Microsoft.Maps.Location(flightPlan.initial_location.
        latitude, flightPlan.initial_location.longitude);
    for (i = 0; i < flightPlan.segments.length; i++) {
        location1 = location2;
        location2 = new Microsoft.Maps.Location(flightPlan.segments[i].
            latitude, flightPlan.segments[i].longitude);
        let coords = [location1, location2];
        map.entities.push(new Microsoft.Maps.Polyline
            (coords, { strokeColor: "DeepPink", strokeThickness: 4 }));
    }
}

//add flight details to the details table and show the table
function addFlightDetails(flightPlan, flightId) {
    document.getElementById("flightDetails").style.display = "";
    document.getElementById("FlightDetailsTable").style.display = "";
    let table = document.getElementById("FlightDetailsTable");
    let row = table.insertRow(-1);
    let cell1 = row.insertCell(0);
    cell1.innerHTML = flightId;
    let cell2 = row.insertCell(1);
    cell2.innerHTML = flightPlan.company_name;
    let cell3 = row.insertCell(2);
    cell3.innerHTML = flightPlan.passengers;
    let cell4 = row.insertCell(3);
    cell4.innerHTML = flightPlan.initial_location.longitude;
    let cell5 = row.insertCell(4);
    cell5.innerHTML = flightPlan.initial_location.latitude;
    let cell6 = row.insertCell(5);
    let departue = new Date(flightPlan.initial_location.date_time);
    cell6.innerHTML = departue.toUTCString();
    //calulate arrival time
    let arrivalTime = new Date(flightPlan.initial_location.date_time);
    let timePassed = 0;
    let i = 0;
    for (i; i < flightPlan.segments.length; i++) {
        timePassed += flightPlan.segments[i].timespan_seconds;
    }
    timePassed *= 1000; //miliseconds passed
    arrivalTime.setTime(arrivalTime.getTime() + timePassed);
    let destination = flightPlan.segments[i - 1];
    let cell7 = row.insertCell(6);
    cell7.innerHTML = destination.longitude;
    let cell8 = row.insertCell(7);
    cell8.innerHTML = destination.latitude;
    let cell9 = row.insertCell(8);
    cell9.innerHTML = arrivalTime.toUTCString();
}
//remove all additions to page involving the flight shown
function stopShowingFlightPlan() {
    if (flightShowing != null) {
        //remove lines from the map
        for (let i = map.entities.getLength() - 1; i >= 0; i--) {
            let polyline = map.entities.get(i);
            if (polyline instanceof Microsoft.Maps.Polyline) {
                map.entities.removeAt(i);
            }
        }
        //remove flightPlan table
        flightTable = document.getElementById("FlightDetailsTable");
        if (flightTable.rows.length == 3) {
            document.getElementById("FlightDetailsTable").deleteRow(-1);
        }

        document.getElementById("FlightDetailsTable").style.display = "none";
        document.getElementById("flightDetails").style.display = "none";
        //stop highlighting the row in the table
        let element = document.getElementById(flightShowing.id);
        element.classList.remove("bg-info");
        //make the plane icon smaller and change back color
        changeIcon("stop", flightShowing, 0.35);
        flightShowing = null;
    }
}

/*
 * code that deletes a flight
 */
function deleteFlight(flight, flightId) {
    if ((flightShowing != null) && (flightId == flightShowing.id)) {
        stopShowingFlightPlan();
    }
    //removing the pin from map
    map.entities.remove(dict[flightId]);
    delete dict[flightId];
    let i = flight.parentNode.parentNode;
    i.parentNode.removeChild(i);

    // deleting the flight from DB
    deleteFlightFromDB(flightId);
}


function deleteFlightFromDB(flightId) {
    let requestStr = "/api/Flights/" + flightId;

    let xhttp = new XMLHttpRequest();
    // when the state of the request changes
    xhttp.onreadystatechange = function () {
        // The request has been completed successfully
        if (this.readyState == 4 && this.status != 200) {
            showError("Failed to delete flight");
        }
    };
    xhttp.open("DELETE", requestStr, true);
    xhttp.send();
}


/*
 * code that uploads a new flight to the server
 */
const fileSelect = document.getElementById("fileSelect"),
    fileElem = document.getElementById("fileElem");
fileElem.addEventListener("change", handleFiles, false);

/*when the button is pushed, it calls the event that the fileElem button was
 * pushed, so we can upload a new file*/
fileSelect.addEventListener("click", function (e) {
    if (fileElem) {
        fileElem.click();
    }
}, false);

//send the file uploaded to the server
function handleFiles() {
    let file = fileElem.files[0]; //the flight to upload
    let url = "/api/FlightPlan";
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4) { //http sent and answer recieved
            if (this.status == 400) { //error accured
                showError("error in json file");
            } else if (this.status != 200) { //different error accured
                showError("error loading json file");
            }
        }
    };

    let reader = new FileReader();
    reader.readAsText(file); //read the file
    reader.onload = function () { //send the content to the server
        xmlhttp.open("POST", url, true);
        xmlhttp.setRequestHeader("content-type", "application/json");
        xmlhttp.send(reader.result);
    };
    reader.onerror = function () {
        showError("error while reading file");
    };
}
// end of code that uploads a new flight to the server

/*
 * code that shows errors to the user
*/
let timer = null;

function showError(error) {
    let errorTag = document.getElementById("errors");
    errorTag.style.display = "block";
    errorTag.innerHTML = error;
    if (timer != null) {
        clearTimeout(timer);
    }
    timer = setTimeout(removeError, 4000);
}

function removeError() {
    let errorTag = document.getElementById("errors");
    errorTag.style.display = "none";
    errorTag.innerHTML = "";
}
// end of code that shows errors to the user
