using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeautySalonApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BeautySalonApp.Areas.Identity.Data;

// Add profile data for application users by adding properties to the UserApp class
public class UserApp : IdentityUser
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public Salon? Salon { get; set; }
    public long? SalonId { get; set; }
    public List<Reservation> Reservation { get; set; } = new List<Reservation>();
    public Warehouse? Warehouse { get; set; }
    public long? WarehouseId { get; set; }

}

