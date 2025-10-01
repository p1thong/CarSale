using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public int OrderId { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? PromotionCode { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public virtual Order Order { get; set; } = null!;
}
