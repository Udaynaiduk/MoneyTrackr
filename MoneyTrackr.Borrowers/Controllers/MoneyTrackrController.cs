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
            try
            {
                var loans = await _loanService.GetAllLoansAsync();
                return Ok(loans);
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }

        [HttpGet("loans/{fullName}")]
        public async Task<IActionResult> GetLoansByBorrowerName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return BadRequest(new { Error = "Borrower name must be provided." });

            try
            {
                var loans = await _loanService.GetLoansByBorrowerNameAsync(fullName);
                if (loans == null || !loans.Any())
                    return NotFound(new { Error = $"No loans found for borrower name '{fullName}'." });

                return Ok(loans);
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }

        [HttpGet("borrowers/{id}")]
        public async Task<IActionResult> GetBorrowerWithLoans(int id)
        {
            try
            {
                var borrower = await _loanService.GetBorrowerWithLoansAsync(id);
                if (borrower == null)
                    return NotFound(new { Error = $"Borrower with ID {id} not found." });

                return Ok(borrower);
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }

        [HttpPost("loan")]
        public async Task<IActionResult> AddLoan([FromBody] Borrower borrower)
        {
            try
            {
                await _loanService.AddLoanAsync(borrower);
                return CreatedAtAction(nameof(GetAllLoans), new { id = borrower.Id }, borrower);
            }
            catch (LoanServiceException ex)
            {
                int statusCode = ex.ErrorCode >= 400 && ex.ErrorCode < 600 ? ex.ErrorCode : 500;
                return StatusCode(statusCode, new { Error = ex.Message });
            }
        }

        [HttpPut("loan/{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] Loan loan)
        {
            try
            {
                await _loanService.UpdateLoanAsync(id, loan);
                return Ok(loan);
            }
            catch (LoanServiceException ex)
            {
                int statusCode = ex.ErrorCode >= 400 && ex.ErrorCode < 600 ? ex.ErrorCode : 500;
                return StatusCode(statusCode, new { Error = ex.Message });
            }
        }

        [HttpPut("borrower/{id}")]
        public async Task<IActionResult> UpdateBorrower(int id, [FromBody] Borrower borrower)
        {
            try
            {
                await _loanService.UpdateBorrowerAsync(id, borrower);
                return Ok(borrower);
            }
            catch (LoanServiceException ex)
            {
                int statusCode = ex.ErrorCode >= 400 && ex.ErrorCode < 600 ? ex.ErrorCode : 500;
                return StatusCode(statusCode, new { Error = ex.Message });
            }
        }

        [HttpDelete("loan/{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            try
            {
                await _loanService.DeleteLoanAsync(id);
                return NoContent();
            }
            catch (LoanServiceException ex)
            {
                int statusCode = ex.ErrorCode >= 400 && ex.ErrorCode < 600 ? ex.ErrorCode : 500;
                return StatusCode(statusCode, new { Error = ex.Message });
            }
        }

        [HttpDelete("borrower/{id}")]
        public async Task<IActionResult> DeleteBorrower(int id)
        {
            try
            {
                await _loanService.DeleteBorrowerAsync(id);
                return NoContent();
            }
            catch (LoanServiceException ex)
            {
                int statusCode = ex.ErrorCode >= 400 && ex.ErrorCode < 600 ? ex.ErrorCode : 500;
                return StatusCode(statusCode, new { Error = ex.Message });
            }
        }

        [HttpGet("borrowers/3year-status/{months}")]
        public async Task<IActionResult> GetBorrowers3YearStatus(int months)
        {
            if (months < 0) return BadRequest(new { Error = "Months must be 0 or positive." });

            try
            {
                var borrowers = await _loanService.GetAllBorrowersWhoReached3YearsInMonthAsync(months);
                return Ok(borrowers);
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }

        [HttpGet("loans/{loanId}/interest")]
        public async Task<IActionResult> CalculateInterest(int loanId)
        {
            if (loanId <= 0)
                return BadRequest(new { Error = "Invalid loan ID." });

            try
            {
                var interest = await _loanService.CalculateInterestAsync(loanId);
                return Ok(new { LoanId = loanId, Interest = interest });
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }

        [HttpGet("loans/all/interest")]
        public async Task<IActionResult> CalculateAllLoansInterest()
        {
            try
            {
                var result = await _loanService.CalculateAllLoansInterestAsync();

                return Ok(result);
            }
            catch (LoanServiceException ex)
            {
                return StatusCode(ex.ErrorCode, new { Error = ex.Message });
            }
        }
    }
}
