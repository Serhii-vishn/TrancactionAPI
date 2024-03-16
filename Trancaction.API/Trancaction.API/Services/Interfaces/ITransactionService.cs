namespace Transaction.API.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task AddFromCsvAsync(Stream fileParh);
        public Task<IList<TransactionEntity>> GetAllAsync();
    }
}
