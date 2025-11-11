using MoneyTrackr.Borrowers.Helpers;
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
            try
            {
                return await _borrowerRepo.GetAllAsync();
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message,ex.ErrorCode);
            }
        }

        public async Task<Borrower?> GetBorrowerWithLoansAsync(int borrowerId)
        {
            try
            {
                return await _borrowerRepo.GetByIdAsync(borrowerId) as Borrower;
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<IEnumerable<Borrower>> GetLoansByBorrowerNameAsync(string fullName)
        {
            try
            {
                return await _borrowerRepo.GetByNameAsync(fullName);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task AddLoanAsync(Borrower borrowerInput)
        {
            try
            {
                var newLoan = borrowerInput.Loans.FirstOrDefault();
                if (newLoan == null)
                    throw new ArgumentException("At least one loan must be provided with the borrower.");

                var existingBorrowers = await _borrowerRepo.GetByNameAsync(borrowerInput.FullName);
                var existingBorrower = existingBorrowers
                    .FirstOrDefault(b => b.FullName.Equals(borrowerInput.FullName, StringComparison.OrdinalIgnoreCase));

                if (existingBorrower == null)
                {
                    var newBorrower = new Borrower
                    {
                        FullName = borrowerInput.FullName,
                        PhoneNumber = borrowerInput.PhoneNumber,
                        Address = borrowerInput.Address,
                        Loans = new List<Loan> { newLoan }
                    };
                    await _borrowerRepo.AddAsync(newBorrower);
                }
                else
                {
                    bool loanExists = existingBorrower.Loans
                        .Any(l => l.Amount == newLoan.Amount && l.StartDate == newLoan.StartDate);

                    if (!loanExists)
                    {
                        newLoan.BorrowerId = existingBorrower.Id;
                        await _loanRepo.AddAsync(newLoan);
                    }
                    else
                    {
                        throw new ArgumentException("A similar loan already exists for this borrower.");
                    }
                }
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task UpdateLoanAsync(int loanId, Loan loan)
        {
            try
            {
                await _loanRepo.UpdateAsync(loanId, loan);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task DeleteLoanAsync(int loanId)
        {
            try
            {
                await _loanRepo.DeleteAsync(loanId);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task UpdateBorrowerAsync(int borrowerId, Borrower borrower)
        {
            try
            {
                await _borrowerRepo.UpdateAsync(borrowerId, borrower);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<IEnumerable<Borrower>> GetAllBorrowersWhoReached3YearsInMonthAsync(int month)
        {
            try
            {
                return await _borrowerRepo.GetBorrowers3YearAnniversaryAsync(month);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<LoanInterestInfo> CalculateInterestAsync(int loanId)
        {
            try
            {
                return await _loanRepo.CalculateInterestAsync(loanId);
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync()
        {
            try
            {
                return await _borrowerRepo.CalculateAllLoansInterestAsync();
            }
            catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message, ex.ErrorCode);
            }
        }

        public async Task DeleteBorrowerAsync(int Id)
        {
            try
            {
                await _borrowerRepo.DeleteAsync(Id);
            }
             catch (LoanServiceException ex)
            {
                throw new LoanServiceException(ex.Message,ex.ErrorCode);
            }
        }
    }
}
