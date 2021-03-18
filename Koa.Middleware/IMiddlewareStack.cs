namespace Koa.Middleware
{
    using System;
    using System.Threading.Tasks;

    public interface IMiddlewareStack<TContext>
    {
        Func<TContext, TContext> Compose(params Func<TContext, Func<TContext>, TContext>[] middleware);
        Func<TContext, Task<TContext>> ComposeAsync(params Func<TContext, Func<Task<TContext>>, Task<TContext>>[] middleware);
    }
}
