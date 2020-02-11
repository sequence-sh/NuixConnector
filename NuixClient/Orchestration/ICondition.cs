namespace NuixClient.Orchestration
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Description of this condition
        /// </summary>
        string Description { get; }

        /// <summary>
        /// .Is this condition met
        /// </summary>
        bool IsMet();
    }
}