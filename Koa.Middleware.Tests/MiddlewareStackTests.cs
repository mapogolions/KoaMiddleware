using System;
namespace Koa.Middleware.Tests
{
    using Xunit;

    public class MiddlewareStackTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("foo", "FOO")]
        [InlineData("B-a-r", "B_A_R")]
        public void ShoulBeAbleCallNext(string input, string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.Compose((_, next) =>
            {
                return next().ToUpper();
            },
            (_, next) =>
            {
                return _.Replace('-', '_');;
            }
            );

            var actual = fn(input);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ShouldIncrementInputValue(int input)
        {
            var stack = new MiddlewareStack<int>();
            var fn = stack.Compose((_, next) =>
            {
                return _ + 1;
            });
            var actual = fn(input);
            var expected = input + 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldThrowExceptionWhenMiddlewareIsNull()
        {
            var stack = new MiddlewareStack<string>();

            Assert.Throws<ArgumentNullException>(() => stack.Compose(null));
        }


        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        public void ShouldBeLikeIdentityFunctionWhenMiddlewareIsEmptyCollection(string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.Compose(Array.Empty<Func<string, Func<string>, string>>());
            var actual = fn(expected);

            Assert.Equal(expected, actual);
        }
    }
}
