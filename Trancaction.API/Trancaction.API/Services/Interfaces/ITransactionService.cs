namespace Transaction.API.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionEntity?> GetAsync(string transaction_id);
        public Task<IList<TransactionEntity>> ListAsync();
        public Task<IList<TransactionEntity>> ListAsync(int year);
        public Task<IList<TransactionEntity>> ListAsync(int year, int mounth);
        public Task<IList<TransactionEntity>> ListAsync(int year, string timezone);
        public Task<IList<TransactionEntity>> ListAsync(int year, int mounth, string timezone);
        public Task AddFromCsvAsync(Stream fileParh);
        Task<IActionResult> ExportCsvAsync(string transaction_id);
    }
}
