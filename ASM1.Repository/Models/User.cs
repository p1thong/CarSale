using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? DealerId { get; set; }

    public int? ManufacturerId { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual Manufacturer? Manufacturer { get; set; }
}
