using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MoneyTrackr.Borrowers.Models
{
    [Table("Borrowers")]
    public class Borrower
    {
        [Key]
        public int Id { get; set; }

        
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string FullName { get; set; } = string.Empty;

        
        [MaxLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(250)]
        [Column(TypeName = "varchar(250)")]
        public string Address { get; set; } = string.Empty;

        // Navigation property — multiple loans per borrower
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

        // Derived property: checks if any loan reached 3 years
        [NotMapped]
        public bool HasReachedThreeYears =>
            Loans != null && Loans.Any(l => DateTime.UtcNow >= l.StartDate.AddYears(3));

        // Derived property: total borrowed amount (only active loans)
        [NotMapped]
        public decimal TotalBorrowedAmount =>
            Loans?.Sum(l => l.Amount) ?? 0m;

        // Derived property: total still unpaid (optional, useful in UI)
        [NotMapped]
        public decimal TotalRemaining =>
            Loans?.Sum(l => l.Amount - l.PartialPayment) ?? 0m;
    }
}
