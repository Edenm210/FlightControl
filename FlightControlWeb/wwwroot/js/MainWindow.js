// creating dictionary to store all Flights to present in the map
let pinsMap = new Map();
// creating list to store all Flights to present in the tables
let dict = []

let map;

function GetMap() {
    map = new Microsoft.Maps.Map('#myMap', {});

    map.setView({

        center: new Microsoft.Maps.Location(32.006833306, 34.885329792),
        zoom: 3
    });
}

function creatingPushpin(location, imgUrl, scale, callback) {

    let img = new Image();
    img.onload = function () {
        let c = document.createElement('canvas');
        c.width = img.width * scale;
        c.height = img.height * scale;

        let context = c.getContext('2d');

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
let syncLoop = setInterval(sendFlightsRequest, 1000);

function sendFlightsRequest() {
    let currentDate = new Date();
    /// cutting the last chars of the seconds 
    let dateStr = (currentDate.toISOString().slice(0, 19) + 'Z');
    // builting the GET request - to get all flights
    let requestStr = "/api/Flights?relative_to=" + dateStr + "&sync_all";

    let xhttp = new XMLHttpRequest();
    // when the state of the request changes
    xhttp.onreadystatechange = function () {
        if (this.readyState == 4) { // http request is DONE
            if (this.status == 200) {
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

    for (let i in flightObj) {
        // getting flight from the response
        let myObj = flightObj[i];

        flightIdFromResponse.push(myObj.flight_id);

        // the flight exists in the dict - only need to update the PIN location, NOT add another row
        if (myObj.flight_id in dict) {
            document.getElementById(myObj.flight_id + "Long").innerHTML = myObj.longitude;
            document.getElementById(myObj.flight_id + "Lat").innerHTML = myObj.latitude;

            let newLocation = new Microsoft.Maps.Location(myObj.latitude, myObj.longitude);

            dict[myObj.flight_id].setLocation(newLocation);
        }
        // the flight does not exist in the dictionary - need to add Row and Add to dict
        else {
            tableRow = "<tr id=\"" + myObj.flight_id + "\"><td>" + myObj.flight_id + "</td>" +
                "<td>" + myObj.company_name + "</td>" +
                "<td id=" + myObj.flight_id + "Long>" + myObj.longitude + "</td>" +
                "<td id=" + myObj.flight_id + "Lat>" + myObj.latitude + "</td>" +
                "<td>" + myObj.passengers + "</td>" +
                "<td>" + myObj.date_time + "</td>";

            //Create a location object of where to place the pushpin on the map.
            let location = new Microsoft.Maps.Location(myObj.latitude, myObj.longitude);

            creatingPushpin(location, '/Images/airplane1.png', 0.35, function (pin) {
                map.entities.push(pin);
                dict[myObj.flight_id] = pin;
            });

            //External Flight
            if (myObj.is_external == true) {
                document.getElementById("ExternalFlightsBody").innerHTML += (tableRow + "</tr>");
            } else {
                document.getElementById("MyFlightsBody").innerHTML += (tableRow + "<td><input type=\"button\" class=\"close\" value=\"x\" onclick=\"deleteFlight(this,'" + myObj.flight_id + "')\"></td></tr>");
            }
        }
    }


    // going throught the dict and checking the there are flights that not in the response. if there is - delete from dict, its row and pin
    for (let flightId in dict) {
        //Return the value for the current key
        let exists = flightIdFromResponse.includes(flightId);

        if (!exists) {
            var row = document.getElementById(flightId);
            // removing flight row from the table
            row.parentNode.removeChild(row);
            // removing the flight pin
            map.entities.remove(dict[flightId]);
            // removing flight from dictionary
            delete dict[flightId];
        }

    }
}


function deleteFlight(flight, flightId) {
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
 * start of code that uploads a new flight to the server
 */
const fileSelect = document.getElementById("fileSelect"),
    fileElem = document.getElementById("fileElem");
fileElem.addEventListener("change", handleFiles, false);

/*when the button is pushed, it calls the event that the fileElem button was pushed,
so we can upload a new file*/
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
