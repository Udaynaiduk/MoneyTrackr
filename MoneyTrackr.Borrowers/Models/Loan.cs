using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrackr.Borrowers.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Borrower")]
        public int BorrowerId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal InterestRate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public decimal DeductedAmount { get; set; } 

        // EndDate is always 3 years after StartDate
        public DateTime EndDate => StartDate.AddYears(3);

        public bool IsPaid { get; set; }

    }
}
