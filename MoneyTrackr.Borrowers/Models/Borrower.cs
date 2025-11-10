using System.ComponentModel.DataAnnotations;

namespace MoneyTrackr.Borrowers.Models
{
    public class Borrower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(250)]
        public string Address { get; set; }

        // Navigation property for multiple loans
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        // Helper property: has any loan reached 3 years
        public bool HasReachedThreeYears =>
            Loans != null && Loans.Any(l => DateTime.Now >= l.StartDate.AddYears(3));

        // Optional: total borrowed amount
        public decimal TotalBorrowedAmount =>
            Loans != null ? Loans.Sum(l => l.Amount) : 0;
    }
}

