using System;
using IdentityModel.Client;

namespace stowRs.test.fixtures
{
    public class TokenTestFixture : IDisposable
    {
        public TokenTestFixture()
        {
            
        }

        private static TokenClient _tokenClient;

        private string _tokenUri;
        private string _clientId;
        private string _clientSecret;


        public void Dispose()
        {
            _tokenClient?.Dispose();
        }

        public TokenTestFixture UseBaseUri(string tokenUri)
        {
            _tokenUri = tokenUri;
            return this;
        }

        public TokenTestFixture UseClientId(string id)
        {
            _clientId = id;
            return this;
        }

        public TokenTestFixture UseClientSecret(string secret)
        {
            _clientSecret = secret;
            return this;
        }

        public TokenClient Build()
        {
            return _tokenClient ?? (_tokenClient = new TokenClient(_tokenUri, _clientId, _clientSecret));
        }
    }
}