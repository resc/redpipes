namespace RedPipes.Configuration
{
    public interface IBuilderProvider
    {
        /// <summary> Makes the pipe builder typed for <typeparam name="T">type T</typeparam></summary>
        IBuilder<T,T> For<T>();

        /// <summary> Makes the pipe builder typed for <typeparam name="TIn">input type TIn</typeparam>
        /// and adds a pipe that converts the value in the pipe to <typeparam name="TOut">type TOut</typeparam></summary>
        IBuilder<TIn, TOut> For<TIn, TOut>(IBuilder<TIn, TOut> transform);
    }
}
