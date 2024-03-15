namespace Transaction.API.Data.EntityConfiguration
{
    public class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
    {
        public void Configure(EntityTypeBuilder<TransactionEntity> builder)
        {
            builder.ToTable("Transaction");

            builder.HasKey(c => c.Transaction_id);

            builder.Property(e => e.Transaction_id)
                .IsRequired();

            builder.Property(e => e.Name)
                 .IsRequired();

            builder.Property(e => e.Email)
                 .IsRequired();

            builder.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");    

            builder.Property(e => e.Transaction_date)
                .IsRequired();

            builder.Property(e => e.Client_location)
                .IsRequired();
        }
    }
}
