using System.Collections.Generic;
using System.Linq;

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
        public override IEnumerable<string> Errors => Enumerable.Empty<string>();
    }

    /// <summary>
    /// The result of a failed operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Failure<T> : Result<T>
    {
        private readonly string[] _errors;

        internal Failure(string[] errors)
        {
            _errors = errors;
        }

        public override bool WasSuccessful => false;
        public override IEnumerable<string> Errors => _errors;
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

        public static Result<T> Failure(params string[] errors)
        {
            return new Failure<T>(errors);
        }
        
        public static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Failure<T>(errors.ToArray());
        }

        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public abstract bool WasSuccessful { get; }

        /// <summary>
        /// The error message if the operation was not successful.
        /// </summary>
        public abstract IEnumerable<string> Errors { get; }
    }
}
