namespace Consilient.Api.Configuration;

public class PrometheusOptions
{
    public const string SectionName = "Prometheus";

    public bool Enabled { get; init; } = false;
    public string MetricsPath { get; init; } = "/metrics";
}
