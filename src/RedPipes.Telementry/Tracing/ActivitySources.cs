using System.Diagnostics;

namespace RedPipes.Telemetry.Tracing
{
    public static class ActivitySources
    {
        public static ActivitySource Default { get; }

        static ActivitySources()
        {
            var name = typeof(ActivitySources).Assembly.GetName();
            Default =new ActivitySource(name.Name, name.Version.ToString(3));
        }
    }
}
