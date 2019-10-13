using System;
using System.Threading.Tasks;

namespace FluentMediator.Pipelines.AsyncPipeline
{
    public class AsyncPipeline<TRequest> : IAsyncPipeline
    {
        private readonly IMediatorBuilder _mediatorBuilder;
        private readonly IMethodCollection<Method<Func<object, TRequest, Task>, TRequest>, TRequest > _methods;
        private IDirectAsync _direct;

        public AsyncPipeline(IMediatorBuilder mediatorBuilder)
        {
            _mediatorBuilder = mediatorBuilder;
            _methods = new MethodCollection<Method<Func<object, TRequest, Task>, TRequest>, TRequest > ();
            _direct = null!;
        }

        public AsyncPipeline<TRequest> Call<THandler>(Func<THandler, TRequest, Task> func)
        {
            Func<object, TRequest, Task> typedHandler = async(h, r) => await func((THandler) h, (TRequest) r);
            var method = new Method<Func<object, TRequest, Task>, TRequest>(typeof(THandler), typedHandler);
            _methods.Add(method);
            return this;
        }

        public IMediatorBuilder Return<TResult, THandler>(Func<THandler, TRequest, Task<TResult>> func)
        {
            var sendPipeline = new DirectAsync<TRequest, TResult, THandler>(func);
            _direct = sendPipeline;
            return _mediatorBuilder;
        }

        public async Task PublishAsync(GetService getService, object request)
        {
            foreach (var handler in _methods.GetMethods())
            {
                var concreteHandler = getService(handler.HandlerType);
                await handler.Action(concreteHandler, (TRequest) request);
            }
        }

        public async Task<TResult> SendAsync<TResult>(GetService getService, object request)
        {
            if (_direct is null)
            {
                throw new ReturnFunctionIsNullException("The return function is null. SendAsync<TResult> method not executed.");
            }

            foreach (var handler in _methods.GetMethods())
            {
                var concreteHandler = getService(handler.HandlerType);
                await handler.Action(concreteHandler, (TRequest) request);
            }

            return await _direct.SendAsync<TResult>(getService, request!) !;
        }

        public IMediatorBuilder Build()
        {
            return _mediatorBuilder;
        }
    }
}