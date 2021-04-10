using MicroS_Common.Actions;

namespace WeRedux.BlazorTest
{
    public class IncrementCounter : IAction
    {
       

        public IncrementCounter() : this(1)
        {

        }

        public IncrementCounter(int step)
        {
            this.Step = step;
        }

        public int Step { get; set; }
    }

    public class IncrementCounter2: IAction
    {

        public IncrementCounter2() : this(1)
        {

        }

        public IncrementCounter2(int step)
        {
            this.Step = step;
        }

        public int Step { get; set; }
    }
}
