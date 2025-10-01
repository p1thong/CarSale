using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASM1.Repository.Models;

public partial class VehicleModel
{
    public int VehicleModelId { get; set; }

    public int ManufacturerId { get; set; }

    public string Name { get; set; } = null!;

    public string ModelName => Name; // Alias for consistency

    public string? Category { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? ImageUrl { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<VehicleVariant> VehicleVariants { get; set; } = new List<VehicleVariant>();
}
