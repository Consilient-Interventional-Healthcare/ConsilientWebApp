namespace Consilient.Api.Client
{
    internal class AddUserToHeaderHandler(Func<string?> getUserName) : DelegatingHandler
    {
        private const string _headerName = "X-User";
        private readonly Func<string?> _getUserName = getUserName ?? throw new ArgumentNullException(nameof(getUserName));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(_headerName))
            {
                return await base.SendAsync(request, cancellationToken);
            }
            var userName = _getUserName();
            if (string.IsNullOrEmpty(userName))
            {
                request.Headers.Add(_headerName, userName);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
