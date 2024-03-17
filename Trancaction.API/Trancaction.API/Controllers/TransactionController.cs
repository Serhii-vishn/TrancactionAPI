using System;

namespace Transaction.API.Controllers
{
    [ApiController]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet]
        [Route("/transactions{year}")]
        public async Task<IActionResult> GetTransactionsAsyns(int year)
        {
            try
            {
                var filteredTransactions = await _transactionService.ListAsync(year);
                _logger.LogInformation($"Transactions (count = {filteredTransactions.Count}) were received");
                return Ok(filteredTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("/transactions{year}/timezone/{timezone}")]
        public async Task<IActionResult> GetTransactionsAsyns(int year, string timezone)
        {
            try
            {
                var filteredTransactions = await _transactionService.ListAsync(year, timezone);
                _logger.LogInformation($"Transactions (count = {filteredTransactions.Count}) were received");
                return Ok(filteredTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("/transactions{year}/mounth{mounth}")]
        public async Task<IActionResult> GetTransactionsAsyns(int year, int mounth)
        {
            try
            {
                var filteredTransactions = await _transactionService.ListAsync(year, mounth);
                _logger.LogInformation($"Transactions (count = {filteredTransactions.Count}) were received");
                return Ok(filteredTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("/transactions{year}/mounth{mounth}/timezone/{timezone}")]
        public async Task<IActionResult> GetTransactionsAsyns(int year, int mounth, string timezone)
        {
            try
            {
                var filteredTransactions = await _transactionService.ListAsync(year, mounth, timezone);
                _logger.LogInformation($"Transactions (count = {filteredTransactions.Count}) were received");
                return Ok(filteredTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("/transactions/import-csv")]
        public async Task<ActionResult> AddTransactionsCsvAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation($"Try get data from file {file.FileName} was received");
                await _transactionService.AddFromCsvAsync(file.OpenReadStream());
                _logger.LogInformation($"Data data from file {file.FileName} successfully received and saved in the database.");
                return Ok("Successfully received and saved in the database.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }

        [HttpPost("/transactions{transaction_id}/export-csv")]
        public async Task<IActionResult> ExportTransactions(string transaction_id)
        {
            try
            {
                var result = await _transactionService.ExportCsvAsync(transaction_id);
                _logger.LogInformation($"Export {transaction_id} to csv file");
                return result;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex.Message, ex);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message, ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return StatusCode(500);
            }
        }
    }
}
