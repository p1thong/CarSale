using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class TestDrive
{
    public int TestDriveId { get; set; }

    public int CustomerId { get; set; }

    public int VariantId { get; set; }

    public DateOnly? ScheduledDate { get; set; }

    public string? Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual VehicleVariant Variant { get; set; } = null!;
}
