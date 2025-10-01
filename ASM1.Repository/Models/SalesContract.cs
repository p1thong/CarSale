using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class SalesContract
{
    public int SaleContractId { get; set; }

    public int OrderId { get; set; }

    public DateOnly? SignedDate { get; set; }

    public DateOnly? ContractDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Terms { get; set; }

    public string? Status { get; set; }

    public virtual Order Order { get; set; } = null!;
}
