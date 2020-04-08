using System;
using System.Threading.Tasks;

namespace WeRedux
{
    public interface IDispatcher<TAction>
    {
        void Dispatch<T>(T action) where T : TAction;

        void Dispatch<T>(Action<T> action = null) where T : TAction, new();

        Task DispatchAsync<T>(Action<T> action = null) where T : TAction, new();

        void Dispatch(string action);

    }
}
