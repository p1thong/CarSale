using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int DealerId { get; set; }

    public int CustomerId { get; set; }

    public int VariantId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly? OrderDate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<SalesContract> SalesContracts { get; set; } = new List<SalesContract>();

    public virtual VehicleVariant Variant { get; set; } = null!;
}
