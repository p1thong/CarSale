using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class DealerContract
{
    public int DealerContractId { get; set; }

    public int DealerId { get; set; }

    public int ManufacturerId { get; set; }

    public decimal? TargetSales { get; set; }

    public decimal? CreditLimit { get; set; }

    public DateOnly? SignedDate { get; set; }

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;
}
