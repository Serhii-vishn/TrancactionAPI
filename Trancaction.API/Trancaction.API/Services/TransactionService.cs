using System.Text.RegularExpressions;

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

            throw new NotImplementedException();
        }

        public async Task<IList<TransactionEntity>> ListAsync(int year, string timezone)
        {
            ValidateYear(year);
            ValidateTimeZone(timezone);

            throw new NotImplementedException();
        }

        private async Task<bool> CheckTransactionExist(TransactionEntity record)
        {
            using var connection = _context.CreateConnection();
                var sql = @"SELECT TOP 1 * FROM TransactionEntity 
                    WHERE Transaction_id = @Transaction_id";

                var transaction = await connection.QueryFirstOrDefaultAsync<TransactionEntity>(sql, new { record });
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

        private async Task<string> GetIanaTimezoneForCoordinates(double latitude, double longitude)
        {
            var ianaTimezone = await _geoTimezoneApiClient.GetIanaTimezone(latitude, longitude);

            return ianaTimezone;
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
            const string timeZonePattern = @"^UTC[-+]\d{1,2}(:\d{2})?$"; //fix regax

            if (!Regex.IsMatch(timezone, timeZonePattern))
                throw new ArgumentException("Incorrect time zone format");
        }
    }
}
