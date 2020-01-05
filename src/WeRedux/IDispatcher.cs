namespace WeRedux
{
    public interface IDispatcher<TAction>
    {
        void Dispatch(TAction action) ;

        void Dispatch<T>() where T:TAction,new();
    }
}
