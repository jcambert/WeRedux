using System;

namespace WeReduxBlazor
{
    public class StorageEventArgs : EventArgs
    {
        public string Key { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}
