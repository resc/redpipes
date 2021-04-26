namespace RedPipes.Configuration
{
    /// <summary> This is an intermediate step to make the fluent api nicer to use, please do not stop here, but call <see cref="Builder{TTo}"/> </summary>
    public interface ITransformBuilder<TIn, TFrom>
    {
        /// <summary> Use the <paramref name="transform"/> in the pipe </summary>
        IBuilder<TIn, TTo> Builder<TTo>(IBuilder<TFrom, TTo> transform);
    }
}
