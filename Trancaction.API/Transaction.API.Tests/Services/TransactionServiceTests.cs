namespace Transaction.API.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly ITransactionService _transactionServiceMock;

        private readonly Mock<DapperDbContext> _contextMock;
        private readonly Mock<GeoTimezoneApiClient> _geoTimezoneApiClientMock;


        public TransactionServiceTests()
        {
            _contextMock = new Mock<DapperDbContext>();
            _geoTimezoneApiClientMock = new Mock<GeoTimezoneApiClient>();

            _transactionServiceMock = new TransactionService(_contextMock.Object, _geoTimezoneApiClientMock.Object);
        }

        [Fact]
        public async Task GetAsync_Returns_Null_For_Nonexistent_Transaction()
        {
            string nonExistentTransactionId = "non_existent_id";
            _contextMock.Setup(c => c.CreateConnection()).Returns(Mock.Of<DbConnection>()); // Mocking CreateConnection method

            // Act
            var result = await _transactionServiceMock.GetAsync(nonExistentTransactionId);

            result.Should().BeNull();

        }


    }
}