using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace FlightControlWeb.Data
{
    public class DatabaseContext : DbContext, IDatabaseContext
    {

        public DbSet<FlightPlan> FlightPlans { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<InitialFlightLocation> InitialFlightLocations { get; set; }
        public DbSet<FlightSegment> FlightSegments { get; set; }
        public DbSet<FlightWithServer> FlightWithServer { get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {  
            optionsBuilder.UseSqlite("DataSource=SqlitDatabase.db;");
        }

        // Return a list of all the flight plans, if there aren't any return null.
        public async Task<List<FlightPlan>> GetAllFlightPlans()
        {
            try
            {
                var flightPlans = await this.FlightPlans
                    .Include(fp => fp.InitialLocation)
                    .ToListAsync();
                foreach (FlightPlan flightPlan in flightPlans)
                {
                    // Get the segmants for each flight plan.
                    var segments = await this.FlightSegments
                        .Where(x => x.FlightId.Equals(flightPlan.FlightId))
                        .ToListAsync();
                    flightPlan.Segments = segments;
                }
                return flightPlans;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Return specific flight plan by id, if can't find return null.
        public async Task<FlightPlan> GetFlightPlan(string id)
        {
            try
            {
                var flightPlan = await this.FlightPlans
                    .Include(fp => fp.InitialLocation)
                    .FirstAsync(s => s.FlightId.Equals(id));
                if (flightPlan != null)
                {
                    // Add the segments.
                    var segments = await this.FlightSegments
                        .Where(x => x.FlightId.Equals(id))
                        .ToListAsync();
                    flightPlan.Segments = segments;
                }
                return flightPlan;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add flight plan to db.
        public async Task<string> AddFlightPlan(FlightPlan flightPlan)
        {
            string id = GetUniqueKey();
            // Activate the algorithem until the key is unique.
            while ((await this.FlightPlans.FindAsync(id)) != null)
            {
                id = GetUniqueKey();
            }
            flightPlan.FlightId = id;
            flightPlan.InitialLocation.Id = id;
            foreach (FlightSegment segment in flightPlan.Segments)
            {
                // Add the segments.
                segment.FlightId = id;
                await this.FlightSegments.AddAsync(segment);
            }
            await this.FlightPlans.AddAsync(flightPlan);
            await this.SaveChangesAsync();
            return id;
        }

        // Delete flight plan by id, if can't find return false.
        public async Task<bool> DeleteFlightPlan(string id)
        {
            try
            {
                // Find flight to remove.
                var flightToRemove = await this.FlightPlans.FindAsync(id);
                // Find initial location to remove.
                var flightLocation = await this.InitialFlightLocations.FindAsync(id);
                this.InitialFlightLocations.Remove(flightLocation);
                var segments = await this.FlightSegments
                    .Where(x => x.FlightId.Equals(id))
                    .ToListAsync();
                // Remove all the segments.
                foreach (FlightSegment segment in segments)
                {
                    this.FlightSegments.Remove(segment);
                }
                this.FlightPlans.Remove(flightToRemove);
                await this.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Get all the servers, if there aren't any return null.
        public async Task<List<Server>> GetServers()
        {
            try
            {
                return await this.Servers.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Get specific server by id, if couldn't find return null.
        public async Task<Server> GetServer(string id)
        {
            try
            {
                return await this.Servers.FindAsync(id);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add server to db.
        public async Task<bool> AddServer(Server server)
        {
            try
            {
                await this.Servers.AddAsync(server);
                await this.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Delete server by id, if can't find return false.
        public async Task<bool> DeleteServer(string id)
        {
            try
            {
                var server = await this.Servers.FindAsync(id);
                this.Servers.Remove(server);
                await this.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Return the server url of the flight.
        public async Task<string> GetServerOfFlight(string id)
        {
            try
            {
                var flight = await this.FlightWithServer.FindAsync(id);
                return flight.ServerURL;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add all the new flight's ids with their servers.
        public async Task AddFlightsFromServers(List<FlightWithServer> flightWithServers)
        {
            foreach (FlightWithServer flight in flightWithServers)
            {
                try
                {
                    if (!await this.FlightWithServer.ContainsAsync(flight))
                    {
                        await this.FlightWithServer.AddAsync(flight);
                    }
                }
                catch (Exception)
                {
                    continue;
                }  
            }
            await this.SaveChangesAsync();
        }

        // Delete all the flight's ids with the server that has been deleted.
        public async Task DeleteFlightsFromServer(string serverURL)
        {
            try
            {
                var flightsToDelete = await this.FlightWithServer
                    .Where(x => x.ServerURL.Equals(serverURL))
                    .ToListAsync();
                foreach (FlightWithServer flight in flightsToDelete)
                {
                    this.FlightWithServer.Remove(flight);
                }
                await this.SaveChangesAsync();
            }
            catch (Exception)
            {
            }
        }

        // Get unigue key for each flight.
        private string GetUniqueKey()
        {
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digit = "0123456789";
            char[] id = new char[8];
            var random = new System.Random();
            for (int size = 0; size < 8; size++)
            {
                string option = "";
                if (size >= 4 && size <= 5)
                {
                    option += lowerCase;
                }
                else if (size >= 0 && size <= 3)
                {
                    option += upperCase;
                }
                else
                {
                    option += digit;
                }
                id[size] = option[random.Next(option.Length - 1)];
            }

            return String.Join(null, id);
        }
    }
}
