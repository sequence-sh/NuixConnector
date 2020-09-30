using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public static class TestExtensions
    {
        public static async Task ShouldSucceed(this Task<Result> resultTask)
        {
            var r = await resultTask;
            r.ShouldBeSuccessful();
        }

        public static async Task ShouldSucceed<T>(this Task<Result<T>> resultTask)
        {
            var r = await resultTask;
            r.ShouldBeSuccessful();
        }


        public static void ShouldBe<T>(this Result<T> result, Result<T> expectedResult)
        {
            if (expectedResult.IsSuccess)
            {
                result.ShouldBeSuccessful();
                result.Value.Should().Be(expectedResult.Value);
            }
            else
                result.ShouldBeFailure(expectedResult.Error);
        }

        public static void ShouldBeFailure<T, TE>(this Result<T, TE> result, Func<TE, string> convert, string messageShouldContain)
        {
            result.IsFailure.Should().BeTrue("expected failure");

            var s = convert(result.Error);

            s.Should().Contain(messageShouldContain);
        }

        public static void ShouldBeSuccessful(this Result result)
        {
            var (_, isFailure, error) = result;
            if (isFailure)
                throw new XunitException(error);
        }

        public static void ShouldBeSuccessful<T, TE>(this Result<T, TE> result, Func<TE, string> convert)
        {
            var (_, isFailure, _, error) = result;
            if (isFailure)
                throw new XunitException(convert(error));
        }

        public static void ShouldBeSuccessful<T>(this Result<T> result)
        {
            var (_, isFailure, _, error) = result;
            if (isFailure)
                throw new XunitException(error);
        }


        public static void ShouldBeFailure(this Result result, string? expectedError = null)
        {
            var (_, isFailure, realError) = result;
            Assert.True(isFailure);

            if (expectedError != null)
                realError.Should().Be(expectedError);
        }


        public static void ShouldBeFailure<T>(this Result<T> result, string? expectedError = null)
        {
            var (_, isFailure, _, realError) = result;
            Assert.True(isFailure);

            if (expectedError != null)
                realError.Should().Be(expectedError);
        }


    }
}
