using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Helpers;
using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Services; // For LoanServiceException

namespace MoneyTrackr.Borrowers.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MoneyTrackrDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(MoneyTrackrDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Get all entities
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    return await _dbSet
                        .Include("Loans")
                        .ToListAsync();
                }

                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve all entities.", ex, 500);
            }
        }

        // Get by ID
        public async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    var borrower = await _dbSet.OfType<Borrower>()
                        .Include(b => b.Loans)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (borrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    return borrower as T;
                }

                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                    throw new LoanServiceException($"Entity with id {id} not found.", 404);

                return entity;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve entity by ID.", ex, 500);
            }
        }

        // Get by name (for Borrowers only)
        public async Task<IEnumerable<T>> GetByNameAsync(string name)
        {
            try
            {
                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .Where(b => EF.Functions.Like(b.FullName, $"%{name}%"))
                    .ToListAsync();
                return borrowers.Cast<T>();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to retrieve borrowers by name.", ex, 500);
            }
        }

        // Add entity
        public async Task AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to add entity.", ex, 500);
            }
        }

        // Update entity by ID
        public async Task UpdateAsync(int id, T entity)
        {
            try
            {
                if (typeof(T) == typeof(Loan))
                {
                    var updatedLoan = entity as Loan ?? throw new LoanServiceException("Entity must be a Loan.", 400);

                    var existingLoan = await _context.Loans.FindAsync(id);
                    if (existingLoan == null)
                        throw new LoanServiceException($"Loan with id {id} not found.", 404);

                    if (updatedLoan.Amount != default(decimal))
                        existingLoan.Amount = updatedLoan.Amount;
                    if (updatedLoan.InterestRate != default(decimal))
                        existingLoan.InterestRate = updatedLoan.InterestRate;
                    if (updatedLoan.StartDate != default(DateTime))
                        existingLoan.StartDate = updatedLoan.StartDate;
                    if (updatedLoan.PartialPaymentPaidDate != default(DateTime))
                        existingLoan.PartialPaymentPaidDate = updatedLoan.PartialPaymentPaidDate;
                    if (updatedLoan.PartialPayment != default(decimal))
                        existingLoan.PartialPayment = updatedLoan.PartialPayment;

                    existingLoan.IsPaid = updatedLoan.IsPaid;
                }
                else if (typeof(T) == typeof(Borrower))
                {
                    var updatedBorrower = entity as Borrower ?? throw new LoanServiceException("Entity must be a Borrower.", 400);

                    var existingBorrower = await _context.Borrowers.FindAsync(id);
                    if (existingBorrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    if (!string.IsNullOrWhiteSpace(updatedBorrower.FullName))
                        existingBorrower.FullName = updatedBorrower.FullName;
                    if (!string.IsNullOrWhiteSpace(updatedBorrower.PhoneNumber))
                        existingBorrower.PhoneNumber = updatedBorrower.PhoneNumber;
                    if (!string.IsNullOrWhiteSpace(updatedBorrower.Address))
                        existingBorrower.Address = updatedBorrower.Address;

                    if (updatedBorrower.Loans != null && updatedBorrower.Loans.Count > 0)
                    {
                        foreach (var loanDto in updatedBorrower.Loans)
                        {
                            var loan = await _context.Loans.FindAsync(loanDto.Id);
                            if (loan != null)
                            {
                                if (loanDto.Amount != default(decimal))
                                    loan.Amount = loanDto.Amount;
                                if (loanDto.InterestRate != default(decimal))
                                    loan.InterestRate = loanDto.InterestRate;
                                if (loanDto.StartDate != default(DateTime))
                                    loan.StartDate = loanDto.StartDate;
                                if (loanDto.PartialPayment != default(decimal))
                                    loan.PartialPayment = loanDto.PartialPayment;
                                if (loanDto.PartialPaymentPaidDate != default(DateTime))
                                    loan.PartialPaymentPaidDate = loanDto.PartialPaymentPaidDate;

                                loan.IsPaid = loanDto.IsPaid;
                            }
                        }
                    }
                }
                else
                {
                    throw new LoanServiceException("UpdateAsync is only valid for Loan or Borrower DTOs.", 400);
                }

                await _context.SaveChangesAsync();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to update entity.", ex, 500);
            }
        }

        // Delete entity by ID
        public async Task DeleteAsync(int id)
        {
            try
            {
                if (typeof(T) == typeof(Borrower))
                {
                    var borrower = await _context.Borrowers
                        .Include(b => b.Loans)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (borrower == null)
                        throw new LoanServiceException($"Borrower with id {id} not found.", 404);

                    _context.Loans.RemoveRange(borrower.Loans);
                    _context.Borrowers.Remove(borrower);
                }
                else
                {
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                        throw new LoanServiceException($"Entity with id {id} not found.", 404);

                    _dbSet.Remove(entity);
                }

                await _context.SaveChangesAsync();
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to delete entity.", ex, 500);
            }
        }

        // Get borrowers who reached 3-year anniversary in N months
        public Task<IEnumerable<T>> GetBorrowers3YearAnniversaryAsync(int month)
        {
            if (typeof(T) != typeof(Borrower))
                throw new LoanServiceException("GetBorrowers3YearAnniversaryAsync is only valid for Borrower entities.", 400);

            return Task.Run(async () =>
            {
                try
                {
                    var today = DateTime.Today;
                    var windowEnd = today.AddMonths(month);
                    windowEnd = new DateTime(
                        windowEnd.Year,
                        windowEnd.Month,
                        DateTime.DaysInMonth(windowEnd.Year, windowEnd.Month),
                        0, 0, 0,
                        DateTimeKind.Local
                    );

                    var threeYearsAgo = today.AddYears(-3);

                    var borrowers = await _context.Borrowers
                        .Include(b => b.Loans)
                        .Where(b => b.Loans.Any(loan =>
                            !loan.IsPaid &&
                            (
                                loan.StartDate <= threeYearsAgo ||
                                (loan.StartDate > threeYearsAgo && loan.StartDate <= windowEnd.AddYears(-3))
                            )
                        ))
                        .ToListAsync();

                    return borrowers.Cast<T>();
                }
                catch (Exception ex)
                {
                    throw new LoanServiceException("Failed to retrieve borrowers with 3-year anniversary.", ex, 500);
                }
            });
        }

        // Calculate interest for single loan
        public async Task<LoanInterestInfo> CalculateInterestAsync(int loanId)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                if (loan == null)
                    throw new LoanServiceException($"Loan with ID {loanId} not found.", 404);

                var borrower = await _context.Borrowers.FindAsync(loan.BorrowerId);

                decimal partialInterest = 0;
                int partialMonths = 0, partialDays = 0;
                decimal totalInterest = 0;
                decimal remainingInterest = 0;
                int remainingMonths = 0, remainingDays = 0;

                DateTime partialPaidDate = loan.PartialPaymentPaidDate ?? loan.StartDate;

                // 🧾 1️⃣ If there was a partial payment, calculate interest up to that date
                if (loan.PartialPayment > 0 && loan.PartialPaymentPaidDate.HasValue)
                {
                    partialInterest = LoanCalculator.CalculateInterest(
                        loan.Amount,
                        loan.InterestRate,
                        loan.StartDate,
                        partialPaidDate
                    );

                    (partialMonths, partialDays) = DateHelper.CalculateFullMonths(loan.StartDate, partialPaidDate);
                    totalInterest += partialInterest;
                }

                // 🏦 Remaining principal after partial payment
                decimal principal = loan.Amount - loan.PartialPayment;

                DateTime startDate = partialPaidDate;
                DateTime now = DateTime.Now;

                // 🧮 2️⃣ Calculate compounding for each full 3-year period
                int fullCycles = 0;
                DateTime cycleStart = startDate;

                while (cycleStart.AddYears(3) <= now)
                {
                    decimal cycleInterest = LoanCalculator.CalculateInterest(
                        principal,
                        loan.InterestRate,
                        cycleStart,
                        cycleStart.AddYears(3)
                    );

                    principal += cycleInterest; // compound interest after each 3 years
                    cycleStart = cycleStart.AddYears(3);
                    fullCycles++;
                }

                // ⏳ 3️⃣ Calculate remaining (less than 3 years)
                if (cycleStart < now)
                {
                    remainingInterest = LoanCalculator.CalculateInterest(
                        principal,
                        loan.InterestRate,
                        cycleStart,
                        now
                    );

                    (remainingMonths, remainingDays) = DateHelper.CalculateFullMonths(cycleStart, now);
                    totalInterest += remainingInterest;
                }

                decimal totalPayable = principal + totalInterest - loan.PartialPayment;

                // ✅ Return detailed info
                return new LoanInterestInfo
                {
                    BorrowerName = borrower?.FullName ?? "N/A",
                    PrincipalAmount = loan.Amount,
                    ParialPayment = loan.PartialPayment,
                    InterestRate = loan.InterestRate,
                    InterestPerMonthPer100 = loan.InterestRate,

                    PartialMonths = partialMonths,
                    PartialDays = partialDays,
                    PartialInterest = Math.Round(partialInterest, 2),

                    RemainingMonths = remainingMonths,
                    RemainingDays = remainingDays,
                    RemainingInterest = Math.Round(remainingInterest, 2),
                    TotalInterest = Math.Round(totalInterest, 2),
                    TotalPayableAmount = Math.Round(totalPayable, 2),
                    FullThreeYearCycles = fullCycles, // optional new property
                    LastCycleStart = cycleStart       // optional: shows last calculation point
                };
            }
            catch (LoanServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Failed to calculate interest for the loan.", ex, 500);
            }
        }


        // Calculate interest for all loans
        public async Task<List<LoanInterestInfo>> CalculateAllLoansInterestAsync()
        {
            try
            {
                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .ToListAsync();

                var result = new List<LoanInterestInfo>();

                foreach (var borrower in borrowers)
                {
                    foreach (var loan in borrower.Loans)
                    {
                        if (loan.IsPaid)
                            continue;

                        decimal principal = loan.Amount;
                        decimal totalInterest = 0;
                        decimal partialInterest = 0;
                        int partialMonths = 0, partialDays = 0;
                        decimal remainingInterest = 0;
                        int remainingMonths = 0, remainingDays = 0;

                        DateTime startDate = loan.StartDate;
                        DateTime now = DateTime.Now;

                        // 1️⃣ Handle partial payment if applicable
                        if (loan.PartialPayment > 0 && loan.PartialPaymentPaidDate.HasValue)
                        {
                            partialInterest = LoanCalculator.CalculateInterest(
                                principal,
                                loan.InterestRate,
                                startDate,
                                loan.PartialPaymentPaidDate.Value
                            );
                            (partialMonths, partialDays) = DateHelper.CalculateFullMonths(startDate, loan.PartialPaymentPaidDate.Value);

                            // reduce principal
                            principal -= loan.PartialPayment;
                            startDate = loan.PartialPaymentPaidDate.Value;
                        }

                        // 2️⃣ Calculate how many full 3-year periods have passed
                        int fullCycles = 0;
                        DateTime cycleStart = startDate;

                        while (cycleStart.AddYears(3) <= now)
                        {
                            // Calculate interest for this 3-year period
                            decimal cycleInterest = LoanCalculator.CalculateInterest(
                                principal,
                                loan.InterestRate,
                                cycleStart,
                                cycleStart.AddYears(3)
                            );
                            principal += cycleInterest;

                            // Move to next 3-year cycle
                            cycleStart = cycleStart.AddYears(3);
                            fullCycles++;
                        }

                        // 3️⃣ Calculate remaining (less than 3 years)
                        if (cycleStart < now)
                        {
                            remainingInterest = LoanCalculator.CalculateInterest(
                                principal,
                                loan.InterestRate,
                                cycleStart,
                                now
                            );

                            (remainingMonths, remainingDays) = DateHelper.CalculateFullMonths(cycleStart, now);
                            totalInterest += remainingInterest;
                        }
                        decimal totalInterestOnly = totalInterest + partialInterest;
                        decimal totalPayable = principal + totalInterestOnly;

                        result.Add(new LoanInterestInfo
                        {
                            BorrowerName = borrower.FullName,
                            PrincipalAmount = loan.Amount,
                            ParialPayment = loan.PartialPayment,
                            InterestRate = loan.InterestRate,
                            InterestPerMonthPer100 = loan.InterestRate,
                            PartialMonths = partialMonths,
                            PartialDays = partialDays,
                            PartialInterest = Math.Round(partialInterest, 2),
                            RemainingMonths = remainingMonths,
                            RemainingDays = remainingDays,
                            RemainingInterest = Math.Round(remainingInterest, 2),
                            TotalInterest = Math.Round(totalInterestOnly, 2),
                            TotalPayableAmount = Math.Round(totalPayable, 2),
                            FullThreeYearCycles = fullCycles, // optional new property
                            LastCycleStart = cycleStart
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new LoanServiceException("Unexpected error while calculating interest for all loans.", ex, 500);
            }
        }
    }

}
