using System;
using System.Collections.Generic;
using System.Text;

namespace ComponentsTest.Components.Controls
{
    public interface IKeyboard
    {
        event EventHandler<CharInputedEventArgs> CharInputed;
        event EventHandler RequestClear;
    }

    public class CharInputedEventArgs : EventArgs
    {
        public CharInputedEventArgs(string inputText)
        {
            Text = inputText;
        }

        public string Text { get; }
    }
}
