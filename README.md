# FLIGHTS WEB APPLICATION
A web application that allows the user to view the active flights and to upload or delete flights from server.
-----------------------------------------------------
SERVER SIDE:
- uses Microsoft SQlite EF Core for the database.
- The database stores the flights and additional servers that the server connects to.
- The server synchronizes with other servers and receives additional flights from them.
- The server receives and sends json files.

HTTP command:
- GET /api/Flights?relative_to=<DATE_TIME>
The server return a list of all the flights in the database that are active at the time given, and the position of the flights at that time.
- GET /api/Flights?relative_to=<DATE_TIME>&sync_all
Like the previous, with the addition of all the active flights in the servers that it is synchronized with.
- POST /api/FlightPlan
Gives the server a new flightPlan to save in the database. The flight plan is a jzon file in the body
- GET /api/FlightPlan/{id}
The server returns a flighPlan with the given flight ID.
- DELETE /api/FlightPlan/{id}
The server erases the flight with the given ID from the database.
- GET /api/servers
The server return a list of the other servers it is synchronized with.
- POST /api/servers
Gives the server a new server to synchronize with.
- DELETE /api/servers/{id}
delete a server

--------------------------------------------------------------------------
CLIENT SIDE
- The webpage was designed using css and bootstrap
- Presents all the active flights in a table, separating internal and external flights.
- Shows the current position of the flights on the map.
- If a flight is clicked, in the table or the map, the complete route of the flight is shown.
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
 "serverId": "[SERVER_ID]",
 "serverURL": "www.server.com"
}

-------------------------------------------------------------------------------------------

COMMENTS:
* All the times in the program are relative to UTC.
* The client cannot access outside servers (add or remove).
* The calculations of the current position of the flights assumes a flat plain.
