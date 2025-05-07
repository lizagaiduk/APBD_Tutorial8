using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;
namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetClientTrips(int clientId);
    Task<int> CreateClient(CreateClientDTO clientDto);
    Task RegisterClientForTrip(int clientId, int tripId);
    Task RemoveClientFromTrip(int clientId, int tripId);
}