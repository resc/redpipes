using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary>  </summary>
    public static class Exceptions
    {
        /// <summary> Ignore exceptions for which <paramref name="filter"/> returns true </summary>
        public static IBuilder<TIn, TOut> UseExceptionFilter<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Expression<Func<Exception, bool>> filter, string? name = null)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            name ??= "Exception filter: " + filter;
            return builder.Use(next => new Pipe<TOut>(name, next, filter.Compile()), name);
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly string _name;
            private readonly IPipe<T> _next;
            private readonly Func<Exception, bool> _filter;

            public Pipe(string name, IPipe<T> next, Func<Exception, bool> filter)
            {
                _name = name;
                _next = next;
                _filter = filter;
            }

            public async Task Execute(IContext ctx, T value)
            {
                try
                {
                    await _next.Execute(ctx, value);
                }
                catch (Exception ex) when (Filter(ex))
                {
                    // ignore exceptions when the filter returns true;
                }
            }

            private bool Filter(Exception ex)
            {
                try
                {
                    return _filter(ex);
                }
                catch 
                {
                    // the filter didn't work,
                    // so we don't filter the exception
                    // TODO some logging might be nice here.
                    return false;
                }
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, _name));
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}
