namespace Koa.Middleware.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class MiddlewareStackComposeAsyncTests
    {
        [Fact]
        public async Task ShouldExecStepByStep()
        {
            var context = new List<string>();
            var stack = new MiddlewareStack<IList<string>>();
            var fn = stack.ComposeAsync(
                async (_, next) =>
                {
                    _.Add("first start");
                    await next();
                    _.Add("first end");
                    return _;
                },
                async (_, next) =>
                {
                    _.Add("second start");
                    await next();
                    _.Add("second end");
                    return _;
                },
                (_, next) =>
                {
                    _.Add("middle");
                    return Task.FromResult(_);
                });

            var actual = await fn(context);
            var expected = new List<string>
            {
                "first start",
                "second start",
                "middle",
                "second end",
                "first end"
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldThrowExceptionWhenNextCalledMoreThanOnce()
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.ComposeAsync(async (_, next) =>
            {
                await next();
                await next();
                return _;
            });

            Assert.ThrowsAsync<InvalidOperationException>(async () => await fn(string.Empty));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("foo", "FOO")]
        [InlineData("B-a-r", "B_A_R")]
        public async Task ShoulBeAbleCallNext(string input, string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.ComposeAsync(
                async (_, next) =>
                {
                    return (await next()).ToUpper();
                },
                async (_, next) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(20));
                    return _.Replace('-', '_');;
                });

            var actual = await fn(input);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        public async Task ShouldIncrementInputValue(int input, int expected)
        {
            var stack = new MiddlewareStack<int>();
            var fn = stack.ComposeAsync(async (_, next) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20));
                return _ + 1;
            });
            var actual = await fn(input);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        public async Task ShouldBeLikeIdentityFunctionWhenMiddlewareIsEmptyCollection(string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.ComposeAsync(Array.Empty<Func<string, Func<Task<string>>, Task<string>>>());
            var actual = await fn(expected);

            Assert.Equal(expected, actual);
        }
    }
}
