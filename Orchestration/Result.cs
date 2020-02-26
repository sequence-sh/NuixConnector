namespace Orchestration
{
    /// <summary>
    /// The result of a successful operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Success<T> : Result<T>
    {
        internal Success(T result)
        {
            Result = result;
        }

        public T Result { get; }

        public override bool WasSuccessful => true;
        public override string? Error => null;
    }

    /// <summary>
    /// The result of a failed operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Failure<T> : Result<T>
    {
        internal Failure(string error)
        {
            Error = error;
        }

        public override bool WasSuccessful => false;
        public override string Error { get; }
    }

    /// <summary>
    /// The result of an operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Result<T>
    {
        public static Result<T> Success(T r)
        {
            return new Success<T>(r);
        }

        public static Result<T> Failure(string error)
        {
            return new Failure<T>(error);
        }

        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public abstract bool WasSuccessful { get; }

        /// <summary>
        /// The error message if the operation was not successful.
        /// </summary>
        public abstract string? Error { get; }
    }
}
