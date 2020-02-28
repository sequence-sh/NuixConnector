using System;
using System.Reflection;
using CSharpFunctionalExtensions;

namespace NuixClientConsole
{
    public static class ArgumentHelpers
    {
        public static Result<object> TryParseArgument(string s, Type parameterType)
        {
            var type = parameterType;

            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                type = underlyingType ?? throw new ArgumentException("Problem converting isParameterNullable type.");
            }

            if (type.IsEnum)
            {
                return ConvertToResult((Enum.TryParse(parameterType, s, true, out var ev), ev));
            }

            var  pair = ParseArgument(type, s);
            return ConvertToResult(pair);


            static (bool, object?)  ParseArgument(MemberInfo underlyingType, string s)
            {
                switch (underlyingType.Name)
                {
                    case nameof(String): return (true, s);
                    case nameof(Boolean): return (bool.TryParse(s, out var r1), r1);
                    case nameof(Int16): return (short.TryParse(s, out var r2), r2);
                    case nameof(Int32): return (int.TryParse(s, out var r3), r3);
                    case nameof(Int64): return (long.TryParse(s, out var r4), r4);
                    case nameof(Double): return (double.TryParse(s, out var r5), r5);
                    case nameof(Decimal): return (decimal.TryParse(s, out var r6), r6);
                    case nameof(Guid): return (Guid.TryParse(s, out var r7), r7);

                    default:
                    {
                        return (false, null);
                    }
                }
            }

            static Result<object> ConvertToResult((bool success, object? result) pair)
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return pair.success ? Result.Success(pair.result) : Result.Failure<object>("Parse error");
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }
    }
}