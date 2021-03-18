namespace Koa.Middleware.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class MiddlewareStackTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("foo", "FOO")]
        [InlineData("B-a-r", "B_A_R")]
        public async Task ComposeAsync(string input, string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.ComposeAsync(
                async (_, next) =>
                {
                    return (await next()).ToUpper();
                },
                async (_, next) =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    return await Task.FromResult(_.Replace('-', '_'));
                });

            var actual = await fn(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShouldExecStepByStep()
        {
            var context = new List<string>();
            var stack = new MiddlewareStack<IList<string>>();
            var fn = stack.Compose(
                (_, next) =>
                {
                    _.Add("first start");
                    next();
                    _.Add("first end");
                    return _;
                },
                (_, next) =>
                {
                    _.Add("second start");
                    next();
                    _.Add("second end");
                    return _;
                },
                (_, next) =>
                {
                    _.Add("middle");
                    return _;
                });

            var actual = fn(context);
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
            var fn = stack.Compose((_, next) =>
            {
                next();
                next();
                return _;
            });

            Assert.Throws<InvalidOperationException>(() => fn(string.Empty));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("foo", "FOO")]
        [InlineData("B-a-r", "B_A_R")]
        public void ShoulBeAbleCallNext(string input, string expected)
        {
            var stack = new MiddlewareStack<string>();
            var fn = stack.Compose(
                (_, next) =>
                {
                    return next().ToUpper();
                },
                (_, next) =>
                {
                    return _.Replace('-', '_');;
                });

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
