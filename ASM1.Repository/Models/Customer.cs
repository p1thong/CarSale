using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int DealerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public DateOnly? Birthday { get; set; }

    public virtual Dealer Dealer { get; set; } = null!;

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

    public virtual ICollection<TestDrive> TestDrives { get; set; } = new List<TestDrive>();
}
