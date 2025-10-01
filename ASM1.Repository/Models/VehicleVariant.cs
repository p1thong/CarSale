using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class VehicleVariant
{
    public int VariantId { get; set; }

    public int VehicleModelId { get; set; }

    public string Version { get; set; } = null!;

    public string? Color { get; set; }

    public int? ProductYear { get; set; }

    public decimal? Price { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

    public virtual ICollection<TestDrive> TestDrives { get; set; } = new List<TestDrive>();

    public virtual VehicleModel VehicleModel { get; set; } = null!;
}
