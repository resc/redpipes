
using System;
using System.Diagnostics.CodeAnalysis;

namespace RedPipes.Configuration
{
    class BuilderProvider : IBuilderProvider
    {
        public static IBuilderProvider Instance { get; } = new BuilderProvider();

        private BuilderProvider() { }

        public IBuilder<T, T> For<T>()
        {
            return Builder.Unit<T>();
        }

        public IBuilder<TIn, TOut> For<TIn, TOut>(IBuilder<TIn, TOut> transform)
        {
            return transform ?? throw new ArgumentNullException();
        }
    }
}
