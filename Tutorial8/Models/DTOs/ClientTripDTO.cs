using System;

namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public string TripName { get; set; }
    public string TripDescription { get; set; }
    public DateTime TripDateFrom { get; set; }
    public DateTime TripDateTo { get; set; }
    public int MaxPeople { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}