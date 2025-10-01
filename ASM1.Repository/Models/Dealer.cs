using System;
using System.Collections.Generic;

namespace ASM1.Repository.Models;

public partial class Dealer
{
    public int DealerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public int? TransactionId { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<DealerContract> DealerContracts { get; set; } = new List<DealerContract>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
