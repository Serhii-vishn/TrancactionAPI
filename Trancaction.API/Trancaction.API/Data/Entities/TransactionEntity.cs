namespace Transaction.API.Data.Entities
{
    public class TransactionEntity
    {
        public string Transaction_id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Transaction_date { get; set; }
        public string Client_location { get; set; } = null!;
    }
}
