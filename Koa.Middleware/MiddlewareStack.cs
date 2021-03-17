namespace Koa.Middleware
{
    using System;

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
                TContext iter(int i)
                {
                    if (index >= i) throw new InvalidOperationException("Next must be called only once");
                    index = i; // should be here!!!
                    if (i == middleware.Length) return context;
                    var fn = middleware[i];
                    return fn(context, () => iter(i + 1));
                }
                return iter(0);
            };
        }
    }
}
