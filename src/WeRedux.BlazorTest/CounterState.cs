using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeRedux.BlazorTest
{
    public class CounterState
    {
        public CounterState()
        {

        }


        public int Count { get; internal set; } = 0;
        public string Template { get; internal set; } = string.Empty;

        public override string ToString() => $"Count:{Count} - Template:{Template}";
    }

    public class CounterState2
    {
        public CounterState2()
        {

        }


        public int Count { get; internal set; } = 0;

        public override string ToString() => $"Count:{Count} -";
    }
}
