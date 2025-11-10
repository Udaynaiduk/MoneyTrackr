using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Repository;

namespace MoneyTrackr.Borrowers.Services
{
    public class LoanService : ILoanService
    {
        private readonly IRepository<Borrower> _borrowerRepo;
        private readonly IRepository<Loan> _loanRepo;

        public LoanService(IRepository<Borrower> borrowerRepo, IRepository<Loan> loanRepo)
        {
            _borrowerRepo = borrowerRepo;
            _loanRepo = loanRepo;
        }

        public async Task<IEnumerable<Borrower>> GetAllLoansAsync()
        {
            return await _borrowerRepo.GetAllAsync();
        }

        public async Task<Borrower?> GetBorrowerWithLoansAsync(int id)
        {
            return await _borrowerRepo.GetByIdAsync(id) as Borrower;
        }

        public async Task<IEnumerable<Borrower>> GetLoansByBorrowerNameAsync(string fullName)
        {
            return await _borrowerRepo.GetByNameAsync(fullName);
        }

        public async Task AddLoanAsync(Borrower borrowerInput)
        {
            // Ensure the borrower has at least one loan
            var newLoan = borrowerInput.Loans.FirstOrDefault();
            if (newLoan == null)
                throw new ArgumentException("At least one loan must be provided with the borrower.");

            // Get all borrowers from repository
            var existingBorrowers = await _borrowerRepo.GetByNameAsync(borrowerInput.FullName);

            var existingBorrower = existingBorrowers
                .FirstOrDefault(b => b.FullName.Equals(borrowerInput.FullName, StringComparison.OrdinalIgnoreCase));

            if (existingBorrower == null)
            {
                // Create a new borrower with all details
                var newBorrower = new Borrower
                {
                    FullName = borrowerInput.FullName,
                    PhoneNumber = borrowerInput.PhoneNumber,
                    Address = borrowerInput.Address,
                    Loans = new List<Loan> { newLoan }
                };

                // Save borrower (and their first loan)
                await _borrowerRepo.AddAsync(newBorrower);
            }
            else
            {
                bool loanExists = existingBorrower.Loans
                                  .Any(l => l.Amount == newLoan.Amount && l.StartDate == newLoan.StartDate);

                if (!loanExists) 
                {
                    // Link the loan to the existing borrower
                    newLoan.BorrowerId = existingBorrower.Id;
                    await _loanRepo.AddAsync(newLoan);
                }
                else
                { 
                    throw new ArgumentException("A similar loan already exists for this borrower.");
                }
            }
        }

        public async Task UpdateLoanAsync(int id, Loan loan)
        {
           await _loanRepo.UpdateAsync(id, loan);
        }

        public async Task DeleteLoanAsync(int id)
        {
           await _loanRepo.DeleteAsync(id);
        }

        public async Task UpdateBorrowerAsync(int borrowerId, Borrower borrower)
        {
            await _borrowerRepo.UpdateAsync(borrowerId, borrower);
        }

        public async Task<IEnumerable<Borrower>> GetAllBorrowersWhoReached3YearsInMonthAsync(int month)
        {
            return await _borrowerRepo.GetBorrowers3YearAnniversaryAsync(month);
        }

        public async Task<decimal> CalculateInterestAsync(int loanId)
        {
           return await _loanRepo.CalculateInterestAsync(loanId);
        }

        public Task<List<(int LoanId, string BorrowerName, decimal TotalBorrowedAmount, decimal Interest)>> CalculateAllLoansInterestAsync()
        {
            return _borrowerRepo.CalculateAllLoansInterestAsync();
        }

        public async Task DeleteBorrowerAsync(int Id)
        {
            await _borrowerRepo.DeleteAsync(Id);
        }


    }
}
