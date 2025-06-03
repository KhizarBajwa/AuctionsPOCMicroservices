using System;

namespace AuctionService.DTOs;

public class AuctionDto
{
    public int Id { get; set; }

    public int ReservePrice { get; set; }

    public string Seller { get; set; }

    public string Winner { get; set; }

    public int? SoldAmount { get; set; }

    public int? CurrentHighBid { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime AuctionEnd { get; set; }

    // Flatten the status property to a string
    public string Status { get; set; }
    
    // Flatten the item properties
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
    
}
