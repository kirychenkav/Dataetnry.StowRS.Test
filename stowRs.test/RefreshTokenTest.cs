using System.Net;
using System.Threading.Tasks;
using IdentityModel.Client;
using stowRs.test.fixtures;
using Xunit;
using Xunit.Abstractions;

namespace stowRs.test
{
    [Collection("Token client collection")]
    public class TokenTest
    {
        private readonly TokenTestFixture _fixture;
        private readonly ITestOutputHelper _output;

        public TokenTest(TokenTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Theory]
        [InlineData(Constants.EmptyBaseUriTemplate,
            Constants.EmptyClientIdTemplate,
            Constants.EmptyClientSecretTemplate,
            Constants.EmptyRefreshTokenTemplate)]
        public async Task RefreshTokenTest(string tokenUri, string clientId, string clientSecret, string refreshToken)
        {
            //Arrange
            Assert.True(ValidateParameters(tokenUri, clientId, clientSecret, refreshToken));
            var tokenClient = _fixture
                .UseBaseUri(tokenUri)
                .UseClientId(clientId)
                .UseClientSecret(clientSecret)
                .Build();

            //Act
            var tokenResponse = await tokenClient.RequestRefreshTokenAsync(refreshToken);

            //Asset
            if (tokenResponse.IsError)
            {
                _output.WriteLine("Error Type: {0}", tokenResponse.ErrorType);
                _output.WriteLine("Error: {0}", tokenResponse.Error);
                _output.WriteLine("Error Description: {0}", tokenResponse.ErrorDescription);
            }
            
            Assert.False(tokenResponse.IsError);
            Assert.Equal(HttpStatusCode.OK, tokenResponse.HttpStatusCode);
            
            _output.WriteLine("Refresh Token: {0}", tokenResponse.RefreshToken);
            _output.WriteLine("Access Token: {0}", tokenResponse.AccessToken);
            _output.WriteLine("Token Type: {0}", tokenResponse.TokenType);

        }

        private bool ValidateParameters(string baseUri, string clientId, string clientSecret, string refreshToken)
        {
            var result = true;
            if (baseUri == Constants.EmptyBaseUriTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid base uri for STAGING or PRODUCTION. See https://github.com/ibrsp/dataentry-api-postman-collection#getting-started-by-cloning-repository");
                result = false;
            }

            if (clientId == Constants.EmptyClientIdTemplate)
            {
                _output.WriteLine(
                    "You must specify the valid client ID.");
                result = false;
            }

            if (clientSecret == Constants.EmptyClientSecretTemplate)
            {
                _output.WriteLine("You must specify the valid client secret");
                result = false;
            }

            if (refreshToken == Constants.EmptyRefreshTokenTemplate)
            {
                _output.WriteLine("You must specify the valid refresh token");
                result = false;
            }

            return result;
        }

    }
}