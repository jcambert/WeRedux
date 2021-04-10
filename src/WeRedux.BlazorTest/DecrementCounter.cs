using MicroS_Common.Actions;

namespace WeRedux.BlazorTest
{
    public class DecrementCounter:IAction
    {
        private readonly int _step;

        public DecrementCounter() : this(1)
        {

        }

        public DecrementCounter(int step)
        {
            this._step = step;
        }

        public int Step => _step;
    }

    public class DecrementCounter2 : IAction
    {
        private readonly int _step;

        public DecrementCounter2() : this(1)
        {

        }

        public DecrementCounter2(int step)
        {
            this._step = step;
        }

        public int Step => _step;
    }
}
