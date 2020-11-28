using RedPipes.Configuration.Nulls;

namespace RedPipes.Configuration
{
    class BuilderProvider : IBuilderProvider
    {
        public static IBuilderProvider Instance { get; } = new BuilderProvider();

        private BuilderProvider() { }

        public IBuilder<T, T> For<T>()
        {
            return NullBuilder<T>.Instance;
        }

        public IBuilder<TIn, TOut> For<TIn, TOut>(IBuilder<TIn, TOut> transform)
        {
            return NullBuilder<TIn>.Instance.Transform().Use(transform);
        }
    }
}
