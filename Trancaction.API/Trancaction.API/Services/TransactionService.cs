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

        public async Task<IList<TransactionEntity>> AddFromCsvAsync(Stream fileParh)
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

            var records = new List<TransactionEntity>();
            while (await csv.ReadAsync())
            {
                records.Add(new TransactionEntity
                {
                    Transaction_id = csv.GetField("transaction_id"),
                    Name = csv.GetField("name"),
                    Email = csv.GetField("email"),
                    Amount = decimal.Parse(csv.GetField("amount").TrimStart('$'), CultureInfo.InvariantCulture),
                    Transaction_date = await Task.Run(() => DateTime.ParseExact(csv.GetField("transaction_date"), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                    Client_location = csv.GetField("client_location")
                });
            }
            return records;
        }

        public async Task<IList<TransactionEntity>> GetAllAsync()
        {
            using (var connection = _context.CreateConnection())
            {  
                var sql = "SELECT * FROM Transaction";

                var books = await connection.QueryAsync<TransactionEntity>(sql);
                return books.ToList();
            }
        }
    }
}
