﻿using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<TripDTO?> GetTrip(int id);
    Task<bool> TripExists(SqlConnection conn, int tripId);
    Task<bool> HasTripFreeSpace(SqlConnection conn, int tripId);

}