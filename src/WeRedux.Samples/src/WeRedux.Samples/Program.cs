using System;

namespace WeRedux.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var store = new Store<CounterState, IAction>();
            store.OnReduced.Subscribe(state =>
            {
                Console.WriteLine($"State Count has Changed :{state}");
            });
            //store.AddReducer<CounterReducer>();
           //store.AddReducer<TemplateReducer>();
            IAction[] actions = new IAction[] { new TemplateAction(), new IncrementCounter(new Random().Next(50,100)),new DecrementCounter(new Random().Next(1,49)) };
            //store.Dispatch<IncrementCounter>();
            //store.Dispatch<IncrementCounter>();
            store.On<IncrementCounter>().Subscribe(action =>
            {
                Console.WriteLine($"Call IncrementCounter Subscriber");
                var a = action.Action as IncrementCounter;
                action.NewState.Count = action.State.Count + a.Step;
            });
            store.On<DecrementCounter>().Subscribe(action =>
            {
                Console.WriteLine($"Call DecrementCounter Subscriber");
                var a = action.Action as DecrementCounter;
                action.NewState.Count = action.State.Count - a.Step;
            });
            store.On<TemplateAction>().Subscribe(action =>
            {
                Console.WriteLine($"Call TemplateAction Subscriber");
            }); 
            int max = new Random().Next(0,100);
            int j = 0;
            for (int i = 0; i < max; i++)
            {
                store.Dispatch(actions[j++]);
                if (j > 2) j = 0;
            }
            Console.ReadLine();
        }
    }
}
