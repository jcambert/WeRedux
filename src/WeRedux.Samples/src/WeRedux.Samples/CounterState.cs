namespace WeRedux.Samples
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
}
