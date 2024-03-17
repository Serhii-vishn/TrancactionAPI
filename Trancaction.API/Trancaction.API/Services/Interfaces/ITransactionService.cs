namespace Transaction.API.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task AddFromCsvAsync(Stream fileParh);
        public Task<IList<TransactionEntity>> ListAsync();
        public Task<IList<TransactionEntity>> ListAsync(int year);
        public Task<IList<TransactionEntity>> ListAsync(int year, int mounth);
        public Task<IList<TransactionEntity>> ListAsync(int year, string timezone);
        public Task<IList<TransactionEntity>> ListAsync(int year, int mounth, string timezone);
    }
}
