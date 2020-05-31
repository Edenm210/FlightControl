# FLIGHTS WEB APPLICATION
A web application that allows the user to upload new flights to the server and view the active flights.
The server connects to other servers, and presnts flights from it's database, and the flights of the servers that it is connected to.
-----------------------------------------------------
server side:
uses Microsoft SQlite EF Core for the database.
The database stores the flights and additional servers that the server connects to.
The server recieves and sends json files.
The server can return a flight plan of a specific flight, or a list of all the flights that are active at a certain time, and the position of the flights at the time given.

HTTP command:
- GET /api/Flights?relative_to=<DATE_TIME>
The server return a list of all the flights in the data base that are active at the time given, and the position of the flights at that time.
- GET /api/Flights?relative_to=<DATE_TIME>&sync_all
Like the previous, with the addition of all the active flights in the servers that it is synced with.
- POST /api/FlightPlan
Gives the server a new flight plan to save in the database. The flight plan is a jzon file in the body
- GET /api/FlightPlan/{id}
The server returns a flight plan with the given flight ID.
- DELETE /api/FlightPlan/{id}
THe server erases the flight with the given ID from the database.
- GET /api/servers
The server return a list of the other servers it is synced with.
- POST /api/servers
Gives the server a new server to sync with.
- DELETE /api/servers/{id}
delete a server

--------------------------------------------------------------------------
CLIENT SIDE
- The webpage was designed useing css and bootstrap
- Presents all the active flights in a table, separationg internal and external flights.
- Shows the current position of the flights on the map.
- If a flight is pushed, in the table or the map, the complete route of the flight is shown.
- Option of adding new flights.
- Option of deleting an internal flight from the server

-----------------------------------------------------------------------
JSON FILES EXAMPLES:
* flightPlan:
{ "passengers": 216,
 "company_name": "SwissAir",
  "initial_location": {
    "longitude": 32.244,
    "latitude": 31.12,
    "date_time": "2020-01-01T07:00:00Z"
  },
 "segments": [
 {
 "longitude": 33.234,
 "latitude": 31.18,
 "timespan_seconds": 800
 }, {
 "longitude": 34.234,
 "latitude": 33.18,
 "timespan_seconds": 700
 }
   ]
}

* flight:
{
 "flight_id": "[FLIGHT_ID]",
 "longitude": 33.244,
 "latitude": 31.12,
 "passengers": 216,
 "company_name": "SwissAir",
 "date_time": "2020-12-26T23:56:21Z",
 "is_external": false
}

* server: 
 "ServerId": "[SERVER_ID]",
 "ServerURL": "www.server.com"
}

-------------------------------------------------------------------------------------------

COMMENTS:
* All the times in the program are relative to UTC
* The client cannot access outside servers (add or remove)
