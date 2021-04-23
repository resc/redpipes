using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RedPipes.OpenTelemetry.Tracing
{
    /// <summary> Contains the activity sources used by this library </summary>
    public static class ActivitySources
    {
        /// <summary> The default activity source </summary>
        public static ActivitySource Default { get; }

        static ActivitySources()
        {
            var name = GetAssemblyName()!;
#if NET50
            if(name==null) return;
#endif
            var version = name.Version ?? new Version(1, 0, 0);
            Default = new ActivitySource(name.Name??"", version.ToString(3));
        }


        private static AssemblyName GetAssemblyName()
        {
            return typeof(ActivitySources).Assembly.GetName();
        }
    }
}
