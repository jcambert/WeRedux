using System;
using System.Collections.Generic;
using System.Text;

namespace WeRedux.Samples
{
    [Reducer(typeof(CounterState))]
    public class TemplateReducer : Reducer<CounterState, IAction>
    {
        string[] words = new[]{"lorem", "ipsum", "dolor", "sit", "amet", "consectetuer",
        "adipiscing", "elit", "sed", "diam", "nonummy", "nibh", "euismod",
        "tincidunt", "ut", "laoreet", "dolore", "magna", "aliquam", "erat"};
        public TemplateReducer()
        {
        }

        public override void Execute(IActionState<CounterState, IAction> action)
        {
            if(action.Action is TemplateAction)
                action.NewState.Template = words[new Random().Next(0, words.Length - 1)];
        }
    }
}
