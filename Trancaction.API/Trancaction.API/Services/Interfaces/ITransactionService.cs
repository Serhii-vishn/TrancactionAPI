namespace Transaction.API.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<IList<TransactionEntity>> AddFromCsvAsync(Stream fileParh);
        public Task<IList<TransactionEntity>> GetAllAsync();
    }
}
