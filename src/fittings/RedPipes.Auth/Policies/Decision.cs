namespace RedPipes.Auth.Policies
{
    /// <summary> Policy decision outcome </summary>
    public enum Decision : byte
    {
        /// <summary> <see cref="None"/> is used by a <see cref="Policy{T}"/> to indicate it has no opinion on whether or not to permit pipe execution </summary>
        None = 0,
        /// <summary> <see cref="Deny"/> is used by a <see cref="Policy{T}"/> to indicate it denies pipe execution</summary>
        Deny = 1,
        /// <summary> <see cref="Permit"/> is used by a <see cref="Policy{T}"/> to indicate it permits pipe execution </summary>
        Permit = 2,
    }
}
