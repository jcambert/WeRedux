using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        public static IServiceCollection AddRedux<TState, TAction>(this IServiceCollection services, string name, Action<IStore<TState, TAction>> storeOpt = null, Action<IMapperConfigurationExpression> mapper = null,Action <Redux<TState,TAction>> reduxOpt=null)
             where TState : new()
            where TAction : IAction
        {
            services
            .AddStore<TState, TAction>(name, storeOpt, mapper)
            .AddStorage()
            .AddScoped<IRedux<TState, TAction>, Redux<TState, TAction>>(services =>
            {
                var store = services.GetRequiredService<IStore<TState, TAction>>();
                var storage = services.GetRequiredService<LocalStorage>();
                //var js = services.GetRequiredService<IJSRuntime>();
                var logger = services.GetRequiredService<ILogger<Redux<TState, TAction>>>();
                var redux = new Redux<TState, TAction>(store, storage, logger, name);
                reduxOpt?.Invoke(redux);
                return redux;
            });
            return services;
        }
        public static IServiceCollection AddStore<TState, TAction>(this IServiceCollection services, string name, Action<IStore<TState, TAction>> opt = null, Action<IMapperConfigurationExpression> mapper = null)
            where TState : new()
            where TAction : IAction
        {

            services.AddScoped<IStore<TState, TAction>, Store<TState, TAction>>(services =>
            {
                // var logger = services.GetRequiredService<ILogger<Store<TState, TAction>>>();
                var store = new Store<TState, TAction>(name,
                    cfg =>
                    {
                        cfg.CreateMap<TState, TState>();
                        mapper?.Invoke(cfg);
                    });
                opt?.Invoke(store);

                return store;
            });
            return services;
        }
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.AddScoped<LocalStorage>();
            services.AddScoped<SessionStorage>();
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
