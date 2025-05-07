using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Controllers;

[Route("api/clients")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }
    /// <summary>
    /// Retrieves all trips that a specific client is registered for.
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <returns>List of trips with registration and payment info</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT with JOIN on Client_Trip and Trip tables to get trip data for client
    /// - SELECT 1 FROM Client to verify client existence
    /// </remarks>
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        var trips = await _clientsService.GetClientTrips(id);
        if (trips == null)
            return NotFound($"Client with id {id} does not exist.");
        if (trips.Count == 0)
            return Ok(new List<ClientTripDTO>());
        return Ok(trips);
    }

    /// <summary>
    /// Creates a new client entry in the database.
    /// </summary>
    /// <param name="dto">Client data (FirstName, LastName, Email, Telephone, Pesel)</param>
    /// <returns>The ID of the newly created client</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT COUNT(*) FROM Client WHERE Email OR PESEL match — to check duplicates
    /// - INSERT INTO Client (FirstName, LastName, ...) OUTPUT INSERTED.IdClient — to create and return ID
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var id = await _clientsService.CreateClient(dto);
            return CreatedAtAction(nameof(CreateClient), new { id }, $"Client created with ID = {id}");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("already exists"))
                return Conflict(new { message = ex.Message });
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Registers a client for a specific trip.
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="tripId">Trip ID</param>
    /// <returns>Confirmation or error</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT 1 FROM Client and Trip — to validate existence
    /// - SELECT 1 FROM Client_Trip — to check duplicate registration
    /// - SELECT MaxPeople and COUNT(*) — to check capacity
    /// - INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) — to register
    /// </remarks>
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterTrip(int id, int tripId)
    {
        try
        {
            await _clientsService.RegisterClientForTrip(id, tripId);
            return Ok("Client registered for trip.");
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            if (ex.Message.Contains("already registered") || ex.Message.Contains("Trip is full"))
                return Conflict(new { message = ex.Message });
            return StatusCode(500, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Removes a client's registration from a trip.
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="tripId">Trip ID</param>
    /// <returns>No content on success</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT COUNT(*) FROM Client_Trip — to check if registration exists
    /// - DELETE FROM Client_Trip — to remove registration
    /// </remarks>
    [HttpDelete("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClientFromTrip(int clientId, int tripId)
    {
        try
        {
            await _clientsService.RemoveClientFromTrip(clientId, tripId);
            return NoContent();
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not registered"))
                return NotFound(new { message = ex.Message });
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
