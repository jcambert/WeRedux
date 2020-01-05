namespace WeRedux.Samples
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
}
