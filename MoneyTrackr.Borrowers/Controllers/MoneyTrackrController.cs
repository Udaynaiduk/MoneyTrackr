using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.Borrowers.Models;
using MoneyTrackr.Borrowers.Services;

namespace MoneyTrackr.Borrowers.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MoneyTrackrController : ControllerBase
    {
        private readonly ILoanService _loanService;
        public MoneyTrackrController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet("loans")]
        public async Task<IActionResult> GetAllLoans()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        [HttpGet("loans/{fullName}")]
        public async Task<IActionResult> GetLoansByBorrowerName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return BadRequest("Borrower name must be provided.");

            var loans = await _loanService.GetLoansByBorrowerNameAsync(fullName);

            if (loans == null || !loans.Any())
                return NotFound($"No loans found for borrower name containing '{fullName}'.");

            return Ok(loans);
        }

        [HttpGet("borrowers/{id}")]
        public async Task<IActionResult> GetBorrowerWithLoans(int id)
        {
            var borrower = await _loanService.GetBorrowerWithLoansAsync(id);
            if (borrower == null)
                return NotFound();
            return Ok(borrower);
        }

        [HttpPost("loan")]
        public async Task<IActionResult> AddLoan([FromBody] Borrower borrower)
        {
            await _loanService.AddLoanAsync(borrower);
            return CreatedAtAction(nameof(GetAllLoans), new { id = borrower.Id}, borrower);
        }

        [HttpPut("loan/{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan loan)
        {
            await _loanService.UpdateLoanAsync(id, loan);
            return Ok(loan);
        }

        [HttpPut("borrower/{id}")]
        public async Task<IActionResult> UpdateBorrower(int id, [FromBody] Borrower borrower)
        {
            await _loanService.UpdateBorrowerAsync(id, borrower);
            return Ok(borrower);
        }

        [HttpDelete("loan/{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            await _loanService.DeleteLoanAsync(id);
            return NoContent();
        }
        [HttpDelete("borrower/{id}")]
        public async Task<IActionResult> Deleteborrower(int id)
        {
            await _loanService.DeleteBorrowerAsync(id);
            return NoContent();
        }

        [HttpGet("borrowers/3year-status/{months}")]
        public async Task<IActionResult> GetBorrowers3YearStatus(int months)
        {
            if (months < 0) return BadRequest("Months must be 0 or positive.");

            var borrowers = await _loanService.GetAllBorrowersWhoReached3YearsInMonthAsync(months);
            return Ok(borrowers);
        }

        [HttpGet("loans/{loanId}/interest")]
        public async Task<IActionResult> CalculateInterest(int loanId)
        {
            if (loanId <= 0)
                return BadRequest("Invalid loan ID.");

            var interest = await _loanService.CalculateInterestAsync(loanId);
            return Ok(new { LoanId = loanId, Interest = interest });
        }

        [HttpGet("loans/all/interest")]
        public async Task<IActionResult> CalculateAllLoansInterest()
        {
            var result = await _loanService.CalculateAllLoansInterestAsync();

            // Transform tuples into objects for nicer JSON output
            var response = result.Select(r => new
            {
                r.LoanId,
                r.BorrowerName,
                r.TotalBorrowedAmount,
                r.Interest
            });

            return Ok(response);
        }


    }
}
