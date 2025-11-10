using Microsoft.EntityFrameworkCore;
using MoneyTrackr.Borrowers.Models;


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

        // Get all entities with navigation properties
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Borrower))
            {
                return await _dbSet
                    .Include("Loans") // Load Loans for Borrower
                    .ToListAsync();
            }

            return await _dbSet.ToListAsync();
        }

        // Get by ID with navigation properties
        public async Task<T?> GetByIdAsync(int id)
        {
          if (typeof(T) == typeof(Borrower))
            {
                return await _dbSet.OfType<Borrower>()
                    .Include(b => b.Loans)
                    .FirstOrDefaultAsync(b => b.Id == id) as T;
            }

            return await _dbSet.FindAsync(id);
        }

           // Get loans by borrower name (only valid for Loan)
         public async Task<IEnumerable<T>> GetByNameAsync(string name)
         {
               // Use EF Core's 'Contains' for SQL LIKE behavior
             var borrowers = await _context.Borrowers
                            .Include(b => b.Loans)
                             .Where(b => EF.Functions.Like(b.FullName, $"%{name}%")) // SQL LIKE '%name%'
                             .ToListAsync();

                return borrowers.Cast<T>();
            }

        // Add entity
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // Update entity by ID
        public async Task UpdateAsync(int id, T entity)
        {
            if (typeof(T) == typeof(Loan))
            {
                var updatedLoan = entity as Loan ?? throw new ArgumentException("Entity must be a Loan");

                var existingLoan = await _context.Loans.FindAsync(id);
                if (existingLoan == null)
                    throw new ArgumentException($"Loan with id {id} not found");

                // Only update if value is not default
                if (updatedLoan.Amount != default(decimal))
                    existingLoan.Amount = updatedLoan.Amount;

                if (updatedLoan.InterestRate != default(decimal))
                    existingLoan.InterestRate = updatedLoan.InterestRate;

                if (updatedLoan.StartDate != default(DateTime))
                    existingLoan.StartDate = updatedLoan.StartDate;

                if (updatedLoan.DeductedAmount != default(decimal))
                    existingLoan.DeductedAmount = updatedLoan.DeductedAmount;

                existingLoan.IsPaid = updatedLoan.IsPaid; // Always update bool
            }
            else if (typeof(T) == typeof(Borrower))
            {
                var updatedBorrower = entity as Borrower ?? throw new ArgumentException("Entity must be a Borrower");

                var existingBorrower = await _context.Borrowers.FindAsync(id);
                if (existingBorrower == null)
                    throw new ArgumentException($"Borrower with id {id} not found");

                if (!string.IsNullOrWhiteSpace(updatedBorrower.FullName))
                    existingBorrower.FullName = updatedBorrower.FullName;

                if (!string.IsNullOrWhiteSpace(updatedBorrower.PhoneNumber))
                    existingBorrower.PhoneNumber = updatedBorrower.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(updatedBorrower.Address))
                    existingBorrower.Address = updatedBorrower.Address;

                // Optional: update loans
                if (updatedBorrower.Loans != null && updatedBorrower.Loans.Count > 0)
                {
                    foreach (var loanDto in updatedBorrower.Loans)
                    {
                        var loan = await _context.Loans.FindAsync(loanDto.Id);
                        if (loan != null)
                        {
                            // Update individual loan
                            if (loanDto.Amount != default(decimal))
                                loan.Amount = loanDto.Amount;

                            if (loanDto.InterestRate != default(decimal))
                                loan.InterestRate = loanDto.InterestRate;

                            if (loanDto.StartDate != default(DateTime))
                                loan.StartDate = loanDto.StartDate;

                            if (loanDto.DeductedAmount != default(decimal))
                                loan.DeductedAmount = loanDto.DeductedAmount;

                            loan.IsPaid = loanDto.IsPaid;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("UpdateAsync is only valid for Loan or Borrower DTOs.");
            }

            await _context.SaveChangesAsync();
        }


        // Delete entity by ID
        public async Task DeleteAsync(int id)
        {
            if (typeof(T) == typeof(Borrower)) // <-- fix here
            {
                var borrower = await _context.Borrowers
                    .Include(b => b.Loans)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (borrower == null)
                    throw new ArgumentException($"Borrower with id {id} not found");

                // Remove related loans first (if cascade delete not configured)
                _context.Loans.RemoveRange(borrower.Loans);
                _context.Borrowers.Remove(borrower);
            }
            else
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                    throw new ArgumentException($"Entity with id {id} not found");

                _dbSet.Remove(entity);
            }

            await _context.SaveChangesAsync();
        }

        public Task<IEnumerable<T>> GetBorrowers3YearAnniversaryAsync(int month)
        {
            return Task.Run(async () =>
            {
                if (typeof(T) != typeof(Borrower))
                    throw new InvalidOperationException("GetBorrowers3YearAnniversaryAsync is only valid for Borrower entities.");

                var today = DateTime.Today;
                var windowEnd = today.AddMonths(month);
                windowEnd = new DateTime(windowEnd.Year, windowEnd.Month,
                                         DateTime.DaysInMonth(windowEnd.Year, windowEnd.Month));

                // compute start of 3-year period range
                var threeYearsAgo = today.AddYears(-3);

                var borrowers = await _context.Borrowers
                    .Include(b => b.Loans)
                    .Where(b => b.Loans.Any(loan =>
                        !loan.IsPaid &&
                        (
                            loan.StartDate <= threeYearsAgo ||        // Already reached 3 years
                            (loan.StartDate > threeYearsAgo &&             // Will reach in next N months
                             loan.StartDate <= windowEnd.AddYears(-3))
                        )
                    ))
                    .ToListAsync();


                return borrowers.Cast<T>();
            });
        }
        public async Task<decimal> CalculateInterestAsync(int loanId)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null)
                throw new ArgumentException($"Loan with id {loanId} not found");

            // Principal amount (consider deductions if any)
            decimal principal = loan.Amount - loan.DeductedAmount;

            // Duration from start date to now
            var duration = DateTime.Now - loan.StartDate;

            // Calculate months and remaining days (each month = 30 days)
            int totalMonths = (int)(duration.TotalDays / 30);
            int remainingDays = (int)(duration.TotalDays % 30);

            // Interest per 100 rupees per month
            decimal interestPerMonthPer100 = loan.InterestRate;  // e.g., 2 means ₹2 per ₹100 per month

            // Interest for full months
            decimal interestForMonths = (principal / 100) * interestPerMonthPer100 * totalMonths;

            // Interest for remaining days (pro-rated)
            decimal interestForDays = (principal / 100) * interestPerMonthPer100 * (remainingDays / 30m);

            // Total interest
            decimal totalInterest = interestForMonths + interestForDays;

            return totalInterest;
        }

        public async Task<List<(int LoanId, string BorrowerName,decimal TotalBorrowedAmount,decimal Interest)>> CalculateAllLoansInterestAsync()
        {
            var borrowers = await _context.Borrowers
                .Include(b => b.Loans)
                .ToListAsync();

            var result = new List<(int LoanId, string BorrowerName,decimal TotalBorrowedAmount, decimal Interest)>();

            foreach (var borrower in borrowers)
            {
                foreach (var loan in borrower.Loans)
                {
                    // Skip fully paid loans
                    if (loan.IsPaid)
                        continue;

                    // Principal = total amount minus any deductions
                    decimal principal = loan.Amount - loan.DeductedAmount;

                    // Cap duration at 3 years (loan term)
                    var now = DateTime.Now;
                    var effectiveEnd = loan.EndDate < now ? loan.EndDate : now;

                    var duration = effectiveEnd - loan.StartDate;

                    // Calculate months and days (each month = 30 days)
                    int totalMonths = (int)(duration.TotalDays / 30);
                    int remainingDays = (int)(duration.TotalDays % 30);

                    // Interest per ₹100 per month (e.g., InterestRate = 2 means ₹2 per ₹100/month)
                    decimal interestPerMonthPer100 = loan.InterestRate;

                    // Calculate interest for full months
                    decimal interestForMonths = (principal / 100) * interestPerMonthPer100 * totalMonths;

                    // Calculate interest for remaining days (pro-rata for 30-day month)
                    decimal interestForDays = (principal / 100) * interestPerMonthPer100 * (remainingDays / 30m);

                    // Total interest
                    decimal totalInterest = interestForMonths + interestForDays;

                    result.Add((loan.Id, borrower.FullName, borrower.TotalBorrowedAmount, totalInterest));
                }
            }

            return result;
        }
    }
}
