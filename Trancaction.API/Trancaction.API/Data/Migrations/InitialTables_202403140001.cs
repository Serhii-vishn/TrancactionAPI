using FluentMigrator;

namespace Transaction.API.Data.Migrations
{
    [Migration(202403140001)]
    public class InitialTables_202403140001 : Migration
    {
        public override void Down()
        {
            Delete.Table("TransactionEntity");
        }

        public override void Up()
        {
            Create.Table("TransactionEntity")
                .WithColumn("Transaction_id").AsString().NotNullable().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("Email").AsString().NotNullable()
                .WithColumn("Amount").AsDecimal().NotNullable()
                .WithColumn("Transaction_date").AsDateTime().NotNullable()
                .WithColumn("Client_location").AsString().NotNullable();
        }
    }
}
