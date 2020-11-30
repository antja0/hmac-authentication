using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace HMACAuthentication.Tests.Integration
{
    [TestFixture]
    internal class HMACSignatureHandlerTests : IntegrationTestBase
    {
        [Test]
        public async Task PostScheme1Endpoint_NoHMACHeader_AuthenticationFails()
        {
            // Arrange
            var request = new[] { "random", "stuff" };

            // Act
            var response = await TestClient.PostAsJsonAsync("api/scheme1", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task PostScheme1Endpoint_CorrectHMACHeader_ReturnsOk()
        {
            // Arrange
            var request = new[] { "random", "stuff" };
            var header = GetSignatureHeader(request, "Scheme1");

            // Act
            var response = await TestClient.PostAsJsonAsync("api/scheme1", request, header);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}