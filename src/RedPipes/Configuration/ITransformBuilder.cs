﻿namespace RedPipes.Configuration
{
    /// <summary> This is an intermediate step to make the fluent api nicer to use, please do not stop here, but call <see cref="Use{TTo}"/> </summary>
    public interface ITransformBuilder<TIn, TFrom>
    {
        /// <summary> Use the <paramref name="transform"/> in the pipe </summary>
        IBuilder<TIn, TTo> Use<TTo>(IBuilder<TFrom, TTo> transform);
    }
}