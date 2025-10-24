using Consilient.Api.Client.Contracts;
using System.Reflection;

namespace Consilient.Api.Client
{
    public class ConsilientApiClient : IConsilientApiClient, IDisposable, IAsyncDisposable
    {
        // Keep references to instantiated children so we can dispose them if they require disposal.
        private readonly List<object> _children = [];

        private readonly ConsilientApiClientConfiguration _configuration;
        private bool _disposed;

        public ConsilientApiClient(ConsilientApiClientConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            InitializeChildren();
        }

        public IPatientsApi Patients { get; private set; } = null!;


        // Synchronous dispose: dispose any child that implements IDisposable or IAsyncDisposable.
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var child in _children)
            {
                try
                {
                    if (child is IDisposable d)
                    {
                        d.Dispose();
                    }
                    else if (child is IAsyncDisposable ad)
                    {
                        // run async dispose synchronously
                        ad.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    }
                }
                catch
                {
                    // swallow: best-effort dispose of children
                }
            }

            GC.SuppressFinalize(this);
        }

        // Async dispose: prefer async disposal when available.
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var child in _children)
            {
                try
                {
                    if (child is IAsyncDisposable ad)
                    {
                        await ad.DisposeAsync().ConfigureAwait(false);
                    }
                    else if (child is IDisposable d)
                    {
                        d.Dispose();
                    }
                }
                catch
                {
                    // swallow: best-effort dispose of children
                }
            }

            GC.SuppressFinalize(this);
        }

        private static object? CreateInstanceWithKnownServices(Type implType, HttpClient httpClient)
        {
            return Activator.CreateInstance(implType, [httpClient]);
        }

        private void InitializeChildren()
        {
            // find public instance properties that are interfaces and writable
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.PropertyType.IsInterface);

            // search for implementations in this assembly (adjust if implementations live elsewhere)
            var assembly = typeof(ConsilientApiClient).Assembly;
            foreach (var prop in properties)
            {
                // find a concrete implementation assignable to the interface
                var implType = assembly.GetTypes()
                    .FirstOrDefault(t => prop.PropertyType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

                if (implType == null)
                {
                    continue;
                }

                // create a named HttpClient for the implementation (allows per-client configuration)
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(_configuration.BaseUrl)
                };

                var instance = CreateInstanceWithKnownServices(implType, httpClient);
                if (instance != null)
                {
                    prop.SetValue(this, instance);
                }
            }
        }
    }
}
