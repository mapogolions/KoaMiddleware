namespace Koa.Middleware
{
    using System;
    using System.Threading.Tasks;

    public class MiddlewareStack<TContext> : IMiddlewareStack<TContext>
    {
        public Func<TContext, TContext> Compose(params Func<TContext, Func<TContext>, TContext>[] middleware)
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware), "Middleware stack must be an array");
            }
            return context =>
            {
                var index = -1;
                TContext Iter(int i)
                {
                    if (index >= i) throw new InvalidOperationException("Next must be called only once");
                    index = i; // should be here!!!
                    if (i == middleware.Length) return context;
                    var fn = middleware[i];
                    return fn(context, () => Iter(i + 1));
                }
                return Iter(0);
            };
        }

        public Func<TContext, Task<TContext>> ComposeAsync(params Func<TContext, Func<Task<TContext>>, Task<TContext>>[] middleware)
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware), "Middleware stack must be an array");
            }
            return (TContext context) =>
            {
                var index = -1;
                Task<TContext> Iter(int i)
                {

                    if (index >= i) throw new InvalidOperationException("Next must be called only once");
                    index = i; // should be here!!!
                    if (i == middleware.Length) return Task.FromResult<TContext>(context);
                    var fn = middleware[i];
                    return fn(context, () => Iter(i + 1));
                }
                return Iter(0);
            };
        }
    }
}
