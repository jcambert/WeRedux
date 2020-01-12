namespace WeRedux.BlazorTest
{
    public class IncrementCounter : IAction
    {
        private readonly int _step;

        public IncrementCounter() : this(1)
        {

        }

        public IncrementCounter(int step)
        {
            this._step = step;
        }

        public int Step => _step;
    }

    public class IncrementCounter2: IAction
    {
        private readonly int _step;

        public IncrementCounter2() : this(1)
        {

        }

        public IncrementCounter2(int step)
        {
            this._step = step;
        }

        public int Step => _step;
    }
}
