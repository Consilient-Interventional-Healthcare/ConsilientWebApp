namespace Consilient.Api.Client
{
    internal sealed class AddUserToHeaderHandler : DelegatingHandler
    {
        private const string _headerName = "X-User";
        private readonly Func<string?> _getUserName;

        // Constructor for use with IHttpClientFactory / DI (factory will assign InnerHandler)
        public AddUserToHeaderHandler(Func<string?> getUserName)
            : base()
        {
            _getUserName = getUserName ?? throw new ArgumentNullException(nameof(getUserName));
        }

        // Constructor for manual use where you provide an inner handler (avoids InvalidOperationException)
        public AddUserToHeaderHandler(Func<string?> getUserName, HttpMessageHandler innerHandler)
            : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
        {
            _getUserName = getUserName ?? throw new ArgumentNullException(nameof(getUserName));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Only add the header when it's not already present and we have a non-empty username
            if (!request.Headers.Contains(_headerName))
            {
                var userName = _getUserName();
                if (!string.IsNullOrEmpty(userName))
                {
                    request.Headers.Add(_headerName, userName);
                }
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
