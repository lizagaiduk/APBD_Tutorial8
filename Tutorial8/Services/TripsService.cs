using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=localhost,1433;Initial Catalog=apbd;User ID=SA;Password=yourStrong(!)Password;TrustServerCertificate=True";

    public async Task<List<TripDTO>> GetTrips()
    {
        try
        {
            var trips = new List<TripDTO>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var tripCmd = new SqlCommand(@"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
            FROM Trip t", conn);
            using var reader = await tripCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                trips.Add(new TripDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                    Countries = new List<CountryDTO>()
                });
            }
            await reader.CloseAsync();
            foreach (var trip in trips)
            {
                var countriesCmd = new SqlCommand(@"
                SELECT c.Name
                FROM Country c
                JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                WHERE ct.IdTrip = @IdTrip", conn);
                countriesCmd.Parameters.AddWithValue("@IdTrip", trip.Id);
                using var countriesReader = await countriesCmd.ExecuteReaderAsync();
                while (await countriesReader.ReadAsync())
                {
                    trip.Countries.Add(new CountryDTO
                    {
                        Name = countriesReader.GetString(0)
                    });
                }
                await countriesReader.CloseAsync();
            }
            return trips;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve trips: " + ex.Message);
        }
    }
    
    public async Task<TripDTO?> GetTrip(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var cmd = new SqlCommand(@"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople
        FROM Trip t
        WHERE t.IdTrip = @IdTrip", conn);
        cmd.Parameters.AddWithValue("@IdTrip", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows) return null;
        TripDTO trip = null;
        if (await reader.ReadAsync())
        {
            trip = new TripDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                Countries = new List<CountryDTO>()
            };
        }
        await reader.CloseAsync();
        if (trip is null) return null;
        var countriesCmd = new SqlCommand(@"
        SELECT c.Name
        FROM Country c
        JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
        WHERE ct.IdTrip = @IdTrip", conn);
        countriesCmd.Parameters.AddWithValue("@IdTrip", id);
        using var countriesReader = await countriesCmd.ExecuteReaderAsync();
        while (await countriesReader.ReadAsync())
        {
            trip.Countries.Add(new CountryDTO
            {
                Name = countriesReader.GetString(0)
            });
        }
        return trip;
    }
}
