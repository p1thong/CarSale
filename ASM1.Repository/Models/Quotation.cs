using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Quotation
{
    public int QuotationId { get; set; }

    public int CustomerId { get; set; }

    public int VariantId { get; set; }

    public int DealerId { get; set; }

    public decimal Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual VehicleVariant Variant { get; set; } = null!;
}
