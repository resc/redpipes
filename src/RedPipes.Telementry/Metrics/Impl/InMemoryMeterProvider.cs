using OpenTelemetry.Metrics;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class InMemoryMeterProvider : MeterProvider
    {
        public override Meter GetMeter(string name, string version = null)
        {
            return new InMemoryMeter(name, version);
        }
    }
}