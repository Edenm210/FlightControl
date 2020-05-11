
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




   

