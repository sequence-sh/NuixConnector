namespace NuixClient.Orchestration.Conditions
{

    /// <summary>
    /// A condition that is required for the process to execute
    /// </summary>
    internal abstract class Condition
    {

        /// <summary>
        /// Description of this condition
        /// </summary>
        public abstract string GetDescription();


        /// <summary>
        /// String representation of this Description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetDescription();
        }

        /// <summary>
        /// Is this condition met
        /// </summary>
        public abstract bool IsMet();

        
    }
}