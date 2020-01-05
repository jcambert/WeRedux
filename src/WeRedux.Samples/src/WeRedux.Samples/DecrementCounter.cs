namespace WeRedux.Samples
{
    public class DecrementCounter : IAction
    {
        private readonly int _step;

        public DecrementCounter():this(1)
        {

        }

        public DecrementCounter(int step)
        {
            this._step = step;
        }

        public int Step => _step;
    }
}
