using OpenTelemetry.Metrics;

namespace RedPipes.Telemetry.Metrics
{
    internal static class Meters
    {
        public static Meter Default { get; }

        static Meters()
        {
            var name = typeof(Duration).Assembly.GetName();
            Default = MeterProvider.Default.GetMeter(name.Name, name.Version.ToString(3));
        }
    }
}
