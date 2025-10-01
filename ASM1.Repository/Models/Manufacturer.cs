using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Manufacturer
{
    public int ManufacturerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Country { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<DealerContract> DealerContracts { get; set; } = new List<DealerContract>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<VehicleModel> VehicleModels { get; set; } = new List<VehicleModel>();
}
