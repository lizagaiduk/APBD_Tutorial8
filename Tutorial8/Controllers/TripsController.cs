using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/trips")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    /// <summary>
    /// Retrieves all available trips, including basic trip details and associated countries.
    /// </summary>
    /// <returns>List of trips with country information</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip
    /// - For each trip, SELECT c.Name FROM Country JOIN Country_Trip ON IdCountry WHERE IdTrip = @IdTrip
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        try
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to fetch trips", detail = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a specific trip by ID, including associated countries.
    /// </summary>
    /// <param name="id">The ID of the trip</param>
    /// <returns>Trip details or 404 if not found</returns>
    /// <remarks>
    /// SQL queries used:
    /// - SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip WHERE IdTrip = @IdTrip
    /// - SELECT c.Name FROM Country JOIN Country_Trip ON IdCountry WHERE IdTrip = @IdTrip
    /// </remarks>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(int id)
    {
        try
        {
            var trip = await _tripsService.GetTrip(id);
            if (trip == null)
            {
                return NotFound(new { message = $"Trip with ID {id} not found" });
            }
            return Ok(trip);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to fetch trip", detail = ex.Message });
        }
    }
}
