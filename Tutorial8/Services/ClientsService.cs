using Microsoft.Data.SqlClient;
using System.Globalization;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=localhost,1433;Initial Catalog=apbd;User ID=SA;Password=yourStrong(!)Password;TrustServerCertificate=True";
    private readonly ITripsService _tripsService;

    public ClientsService(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    public async Task<List<ClientTripDTO>?> GetClientTrips(int clientId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            if (!await ClientExists(conn, clientId)) return null;

            var trips = new List<ClientTripDTO>();
            var query = @"
                SELECT t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, ct.RegisteredAt, ct.PaymentDate
                FROM Client_Trip ct
                JOIN Trip t ON ct.IdTrip = t.IdTrip
                WHERE ct.IdClient = @IdClient";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IdClient", clientId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var registeredAtInt = reader.GetInt32(5);
                var registeredAt = DateTime.ParseExact(registeredAtInt.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

                DateTime? paymentDate = null;
                if (!reader.IsDBNull(6))
                {
                    var paymentDateInt = reader.GetInt32(6);
                    paymentDate = DateTime.ParseExact(paymentDateInt.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                }

                trips.Add(new ClientTripDTO
                {
                    TripName = reader.GetString(0),
                    TripDescription = reader.GetString(1),
                    TripDateFrom = reader.GetDateTime(2),
                    TripDateTo = reader.GetDateTime(3),
                    MaxPeople = reader.GetInt32(4),
                    RegisteredAt = registeredAt,
                    PaymentDate = paymentDate
                });
            }
            return trips;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve trips for client: " + ex.Message);
        }
    }

    public async Task<int> CreateClient(CreateClientDTO clientDto)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var checkCmd = new SqlCommand(@"SELECT COUNT(*) FROM Client WHERE Email = @Email OR Pesel = @Pesel", conn);
            checkCmd.Parameters.AddWithValue("@Email", clientDto.Email);
            checkCmd.Parameters.AddWithValue("@Pesel", clientDto.Pesel ?? (object)DBNull.Value);
            var exists = (int)await checkCmd.ExecuteScalarAsync();
            if (exists > 0)
                throw new Exception("Client with this Email or PESEL already exists");

            var insertCmd = new SqlCommand(@"
                INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                OUTPUT INSERTED.IdClient
                VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);", conn);

            insertCmd.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
            insertCmd.Parameters.AddWithValue("@LastName", clientDto.LastName);
            insertCmd.Parameters.AddWithValue("@Email", clientDto.Email);
            insertCmd.Parameters.AddWithValue("@Telephone", clientDto.Telephone ?? (object)DBNull.Value);
            insertCmd.Parameters.AddWithValue("@Pesel", clientDto.Pesel ?? (object)DBNull.Value);

            var newId = (int)await insertCmd.ExecuteScalarAsync();
            return newId;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create client: " + ex.Message);
        }
    }

    public async Task RegisterClientForTrip(int clientId, int tripId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            if (!await ClientExists(conn, clientId))
                throw new Exception("Client not found");

            if (!await _tripsService.TripExists(conn, tripId))
                throw new Exception("Trip not found");

            if (await IsClientAlreadyRegistered(conn, clientId, tripId))
                throw new Exception("Client already registered for this trip");

            if (!await _tripsService.HasTripFreeSpace(conn, tripId))
                throw new Exception("Trip is full");

            var nowInt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            var insertCmd = new SqlCommand(@"
                INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                VALUES (@ClientId, @TripId, @Now)", conn);

            insertCmd.Parameters.AddWithValue("@ClientId", clientId);
            insertCmd.Parameters.AddWithValue("@TripId", tripId);
            insertCmd.Parameters.AddWithValue("@Now", nowInt);

            await insertCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to register client for trip: " + ex.Message);
        }
    }

    public async Task RemoveClientFromTrip(int clientId, int tripId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var checkCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Client_Trip 
                WHERE IdClient = @ClientId AND IdTrip = @TripId", conn);

            checkCmd.Parameters.AddWithValue("@ClientId", clientId);
            checkCmd.Parameters.AddWithValue("@TripId", tripId);

            var exists = (int)await checkCmd.ExecuteScalarAsync();
            if (exists == 0)
                throw new Exception("Client is not registered for this trip");

            var deleteCmd = new SqlCommand(@"
                DELETE FROM Client_Trip 
                WHERE IdClient = @ClientId AND IdTrip = @TripId", conn);

            deleteCmd.Parameters.AddWithValue("@ClientId", clientId);
            deleteCmd.Parameters.AddWithValue("@TripId", tripId);

            await deleteCmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to remove client from trip: " + ex.Message);
        }
    }

    public async Task<bool> ClientExists(SqlConnection conn, int clientId)
    {
        var cmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @IdClient", conn);
        cmd.Parameters.AddWithValue("@IdClient", clientId);
        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> IsClientAlreadyRegistered(SqlConnection conn, int clientId, int tripId)
    {
        var cmd = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip = @TripId", conn);
        cmd.Parameters.AddWithValue("@ClientId", clientId);
        cmd.Parameters.AddWithValue("@TripId", tripId);
        return await cmd.ExecuteScalarAsync() is not null;
    }
}