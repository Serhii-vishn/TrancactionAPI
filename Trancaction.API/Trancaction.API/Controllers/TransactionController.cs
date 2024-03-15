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

        [HttpPost]
        [Route("/read-transactions-csv")]
        public async Task<ActionResult> GetTransactionsCSV([FromForm] IFormFileCollection file)
        {
            try
            {
                var employees = await _transactionService.AddFromCsvAsync(file[0].OpenReadStream());
                _logger.LogInformation($"Get data from file {file[0].FileName} was received");
                return Ok(employees);
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
        [Route("/transactions")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var employees = await _transactionService.GetAllAsync();
                _logger.LogInformation($"Get data from db was received");
                return Ok(employees);
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
