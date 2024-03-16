using Dapper;

namespace Transaction.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly DapperDbContext _context;

        public TransactionService(DapperDbContext context) 
        {
            _context = context;
        }

        public async Task AddFromCsvAsync(Stream fileParh)
        {
            using var reader = new StreamReader(fileParh);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
            });
            await csv.ReadAsync();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                try
                {
                    var record = new TransactionEntity
                    {
                        Transaction_id = csv.GetField("transaction_id"),
                        Name = csv.GetField("name"),
                        Email = csv.GetField("email"),
                        Amount = decimal.Parse(csv.GetField("amount").TrimStart('$'), CultureInfo.InvariantCulture),
                        Transaction_date = await Task.Run(() => DateTime.ParseExact(csv.GetField("transaction_date"), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                        Client_location = csv.GetField("client_location")
                    };

                    if (await CheckTransactionExist(record))
                        await AddTransactionAsync(record);
                    else
                        await UpdateTransactionAsync(record);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public async Task<IList<TransactionEntity>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
                var sql = "SELECT * FROM TransactionEntity";

                var books = await connection.QueryAsync<TransactionEntity>(sql);
                return books.ToList();
        }

        private async Task<bool> CheckTransactionExist(TransactionEntity record)
        {
            using var connection = _context.CreateConnection();
                var sql = @"SELECT TOP 1 * FROM TransactionEntity 
                    WHERE Transaction_id = @Transaction_id";

                var transaction = await connection.QueryFirstOrDefaultAsync<TransactionEntity>(sql, record);
                return transaction == null;
        }

        private async Task AddTransactionAsync(TransactionEntity record)
        {
            using var connection = _context.CreateConnection();
                var sql = @"INSERT INTO TransactionEntity (Transaction_id, Name, Email, Amount, Transaction_date, Client_location)
                    VALUES (@Transaction_id, @Name, @Email, @Amount, @Transaction_date, @Client_location)";

                var rowsAffected = await connection.ExecuteAsync(sql, record);
        }

        private async Task UpdateTransactionAsync(TransactionEntity record)
        {
            using var connection = _context.CreateConnection();
                var sql = @"UPDATE TransactionEntity 
                     SET Name = @Name, 
                         Email = @Email, 
                         Amount = @Amount, 
                         Transaction_date = @Transaction_date, 
                         Client_location = @Client_location 
                     WHERE Transaction_id = @Transaction_id";

                var rowsAffected = await connection.ExecuteAsync(sql, record);
        }

    }
}
