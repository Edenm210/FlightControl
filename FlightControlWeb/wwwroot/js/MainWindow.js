
function GetMap() {
    var map = new Microsoft.Maps.Map('#myMap', {});

    map.setView({

        center: new Microsoft.Maps.Location(32.006833306, 34.885329792),
        zoom: 3
    });
  
    var center = map.getCenter();

    //Create 3 red pushpins.
    for (var i = -2; i <= 3; i++) {
        //Create a location object of where to place the pushpin on the map.
        var location = new Microsoft.Maps.Location(center.latitude+(i*10), center.longitude + (i) * 25);

        //Create a pushpin 
        creatingPushpin(location, '/Images/airplane1.png', 0.35, function (pin) {
            map.entities.push(pin);
    });

       
    }


 
}

function creatingPushpin(location, imgUrl, scale, callback) {
    var img = new Image();
    img.onload = function () {
        var c = document.createElement('canvas');
        c.width = img.width * scale;
        c.height = img.height * scale;

        var context = c.getContext('2d');

        //Draw scaled image
        context.drawImage(img, 0, 0, c.width, c.height);

        var pin = new Microsoft.Maps.Pushpin(location, {
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

/*send the file uploaded to the server*/
function handleFiles() {
    let file = fileElem.files[0]; //the flight to upload
    let url = "/api/FlightPlan";
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function () {
        if (this.readyState == 4) { //http sent and answer recieved
            if (this.status != 200) { //error accured
                //error accured. need to respond!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
        //take care of error!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
    };
}
/*
* end of code that uploads a new flight to the server
*/