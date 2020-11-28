using System;

namespace RedPipes.Telemetry.Metrics.Impl
{
    interface IObserverMetric : IDisposable
    {
        string Name { get; }
        void Observe();
    }
}