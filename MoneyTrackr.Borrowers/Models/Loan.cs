using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackr.Borrowers.Models
{
    [Table("Loans")]
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Borrower")]
        public int BorrowerId { get; set; }

        [Column(TypeName = "decimal(10,2)")]  // smaller, good for MySQL
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(5,2)")]   // stores interest like 7.25%
        public decimal InterestRate { get; set; }

        public DateTime StartDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PartialPayment { get; set; }

        public DateTime? PartialPaymentPaidDate { get; set; }

        [NotMapped]
        public DateTime EndDate => StartDate.AddYears(3);

        public bool IsPaid { get; set; }
    }
}
