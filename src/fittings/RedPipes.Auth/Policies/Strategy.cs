namespace RedPipes.Auth.Policies
{
    /// <summary> Policy combination strategies </summary>
    public enum Strategy : byte
    {
        /// <summary> <see cref="None"/> is used to detect uninitialized values. </summary>
        None = 0,
        
        /// <summary> If any policy decides to permit pipe execution, the decision is <see cref="Decision.Permit"/>, otherwise it's <see cref="Decision.Deny"/> </summary>
        Permissive = 1,

        /// <summary> If any policy decides to deny pipe execution, the decision is <see cref="Decision.Deny"/>, otherwise it's <see cref="Decision.Permit"/> </summary>
        Veto = 2,
        
        /// <summary> If the number of policies that vote <see cref="Decision.Permit"/> is greater than
        /// the number of policies that vote <see cref="Decision.Deny"/> the decision is <see cref="Decision.Permit"/>, otherwise it's <see cref="Decision.Deny"/>
        /// Any policy that votes <see cref="Decision.None"/> will not participate in the vote count. </summary>
        Majority = 3,
        
        /// <summary> All policies must vote <see cref="Decision.Permit"/> and there must be at least one policy that votes <see cref="Decision.Permit"/>
        /// for the result to be <see cref="Decision.Permit"/>, otherwise it will be <see cref="Decision.Deny"/>. </summary>
        Strict = 4,
    }
}
