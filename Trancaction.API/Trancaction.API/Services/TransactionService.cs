namespace Transaction.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly DapperDbContext _context;
        private readonly GeoTimezoneApiClient _geoTimezoneApiClient;

        public TransactionService(DapperDbContext context, GeoTimezoneApiClient geoTimezoneApiClient)
        {
            _context = context;
            _geoTimezoneApiClient = geoTimezoneApiClient;
        }

        public async Task<TransactionEntity?> GetAsync(string transaction_id)
        {
            using var connection = _context.CreateConnection();
                var sql = @"SELECT TOP 1 * FROM TransactionEntity WHERE Transaction_id = @Transaction_id";

                return await connection.QueryFirstOrDefaultAsync<TransactionEntity>(sql, new { transaction_id });
        }

        public async Task<IList<TransactionEntity>> ListAsync()
        {
            using var connection = _context.CreateConnection();
                var sql = "SELECT * FROM TransactionEntity";

                var transactions = await connection.QueryAsync<TransactionEntity>(sql);
                return transactions.ToList();
        }

        public async Task<IList<TransactionEntity>> ListAsync(int year)
        {
            ValidateYear(year);

            using var connection = _context.CreateConnection();
                var sql = @"SELECT * FROM TransactionEntity WHERE YEAR(Transaction_date) = @Year";

                var filteredTransactions = await connection.QueryAsync<TransactionEntity>(sql, new { year});
                return filteredTransactions.ToList();
        }

        public async Task<IList<TransactionEntity>> ListAsync(int year, int month)
        {
            ValidateYear(year);
            ValidateMonth(month);

            using var connection = _context.CreateConnection();
                var sql = @"SELECT * FROM TransactionEntity WHERE YEAR(Transaction_date) = @Year AND MONTH(Transaction_date) = @Month";

                var filteredTransactions = await connection.QueryAsync<TransactionEntity>(sql, new { Year = year, Month = month });
                return filteredTransactions.ToList();
        }

        public async Task<IList<TransactionEntity>> ListAsync(int year, int month, string timezone)
        {
            ValidateYear(year);
            ValidateMonth(month);
            ValidateTimeZone(timezone);

            var transactionsList = await ListAsync(year, month);
            var filteredTransactions = new List<TransactionEntity>();

            foreach (var transaction in transactionsList)
            {
                string transactionTimezone = await GetTimezoneForCoordinates(transaction.Client_location);

                if (string.Equals(timezone.ToUpper(), transactionTimezone.ToUpper()))
                    filteredTransactions.Add(transaction);
            }

            return filteredTransactions;
        }

        public async Task<IList<TransactionEntity>> ListAsync(int year, string timezone)
        {
            ValidateYear(year);
            ValidateTimeZone(timezone);

            var transactionsList = await ListAsync(year);
            var filteredTransactions = new List<TransactionEntity>();

            foreach (var transaction in transactionsList)
            {
                string transactionTimezone = await GetTimezoneForCoordinates(transaction.Client_location);

                if (string.Equals(timezone.ToUpper(), transactionTimezone.ToUpper()))
                    filteredTransactions.Add(transaction);
            }

            return filteredTransactions;
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
                var record = new TransactionEntity
                {
                    Transaction_id = csv.GetField("transaction_id"),
                    Name = csv.GetField("name"),
                    Email = csv.GetField("email"),
                    Amount = decimal.Parse(csv.GetField("amount").TrimStart('$'), CultureInfo.InvariantCulture),
                    Transaction_date = await Task.Run(() => DateTime.ParseExact(csv.GetField("transaction_date"), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                    Client_location = csv.GetField("client_location")
                };

                if (await GetAsync(record.Transaction_id) is null)
                    await AddTransactionAsync(record);
                else
                    await UpdateTransactionAsync(record);
            }
        }

        public async Task<IActionResult> ExportCsvAsync(string transaction_id)
        {
            var transaction = await GetAsync(transaction_id);
            if (transaction is null)
                throw new ArgumentNullException("Transaction not found");

            var builder = new StringBuilder();
            builder.AppendLine("Transaction_id, Name, Transaction_date, Amount");
            builder.AppendLine($"{transaction.Transaction_id}, {transaction.Name}, {transaction.Transaction_date}, {transaction.Amount}");

            var csvData = Encoding.UTF8.GetBytes(builder.ToString());
            return new FileContentResult(csvData, "text/csv") { FileDownloadName = $"{transaction_id}.csv" };
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

        private async Task<string> GetTimezoneForCoordinates(string location)
        {
            var coordinates = location.Trim().Split(',');

            double latitude = double.Parse(coordinates[0]);
            double longitude = double.Parse(coordinates[1]);

            return await _geoTimezoneApiClient.GetTimezone(latitude, longitude);
        }

        private static void ValidateYear(int year)
        {
            if (year < 1990 || year > DateTime.Now.Year)
                throw new ArgumentException($"The year must be in the range from 1990 to {DateTime.Now.Year}.");
        }

        private static void ValidateMonth(int month)
        {
            if (month <= 0 || month > 12)
                throw new ArgumentException($"The month must be in the range from 1 to 12.");
        }

        private static void ValidateTimeZone(string timezone)
        {
            const string timeZonePattern = @"^(UTC[-+](?:1[0-4]|[0]?[0-9])(?::(?:0[0-9]|[1-5][0-9]))?)$";

            if (!Regex.IsMatch(timezone.ToUpper(), timeZonePattern))
                throw new ArgumentException("Incorrect time zone format");
        }
    }
}
