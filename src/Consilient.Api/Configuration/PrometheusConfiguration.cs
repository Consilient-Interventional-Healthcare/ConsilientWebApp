namespace Consilient.Api.Configuration;

public class PrometheusConfiguration
{
    public bool Enabled { get; set; } = false;
    public string MetricsPath { get; set; } = "/metrics";
}
