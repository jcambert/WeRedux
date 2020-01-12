using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WeRedux;

namespace WeReduxBlazor
{

    public static class Extensions
    {
        public static IServiceCollection AddStore<TState, TAction>(this IServiceCollection services,string name=null, Action<IMapperConfigurationExpression> mapper = null)
            where TState : new()
            where TAction : IAction
        {
            services.AddScoped<IStore<TState, TAction>, Store<TState, TAction>>(services =>
            {
                return new Store<TState, TAction>(
                    cfg =>
                    {
                        cfg.CreateMap<TState, TState>();
                        mapper?.Invoke(cfg);
                    },name);
            });
            return services;
        }
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.TryAddScoped<LocalStorage>();
            services.TryAddScoped<SessionStorage>();
            return services;
        }

        public static T JsRuntimeInvoke<T>(this IJsRuntimeAccess jsRuntime, string identifier, params object[] args)
        {
            return jsRuntime.Invoke<T>(identifier, args);
        }

        public static ValueTask<T> JsRuntimeInvokeAsync<T>(this IJsRuntimeAccess jsRuntime, string identifier, params object[] args)
        {
            return jsRuntime.InvokeAsync<T>(identifier, args);
        }

        public static ValueTask<T> JsRuntimeInvokeAsync<T>(this IJsRuntimeAccess jsRuntime, string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default)
        {
            return jsRuntime.InvokeAsync<T>(identifier, args, cancellationToken);
        }
    }

    public interface IJsRuntimeAccess
    {
        T Invoke<T>(string identifier, params object[] args);

        ValueTask<T> InvokeAsync<T>(string identifier, params object[] args);

        ValueTask<T> InvokeAsync<T>(string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default);
    }

    public abstract class JsRuntimeAccessBase<TJsRuntime> : IJsRuntimeAccess where TJsRuntime : IJSRuntime
    {
        protected TJsRuntime JsRuntime { get; }

        protected JsRuntimeAccessBase(TJsRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        public abstract T Invoke<T>(string identifier, params object[] args);

        public ValueTask<T> InvokeAsync<T>(string identifier, params object[] args)
        {
            return JsRuntime.InvokeAsync<T>(identifier, args);
        }

        public ValueTask<T> InvokeAsync<T>(string identifier, IEnumerable<object> args, CancellationToken cancellationToken = default)
        {
            return JsRuntime.InvokeAsync<T>(identifier, cancellationToken, args.ToArray());
        }
    }

    public class ServerSideJsRuntimeAccess : JsRuntimeAccessBase<IJSRuntime>
    {
        public ServerSideJsRuntimeAccess(IJSRuntime jsRuntime) : base(jsRuntime) { }

        public override T Invoke<T>(string identifier, params object[] args)
        {
            throw new NotSupportedException("Synchronous storage access is not supported in a server-side app. Please use the asynchronous implementation.");
        }
    }

    public class ClientSideJsRuntimeAccess : JsRuntimeAccessBase<IJSInProcessRuntime>
    {
        public ClientSideJsRuntimeAccess(IJSInProcessRuntime jsRuntime) : base(jsRuntime) { }

        public override T Invoke<T>(string identifier, params object[] args)
        {
            return JsRuntime.Invoke<T>(identifier, args);
        }
    }
}
