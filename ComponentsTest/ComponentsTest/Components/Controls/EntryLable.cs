using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace ComponentsTest.Components.Controls
{
    public class EntryLable : Label
    {
        public static readonly BindableProperty keyboardProperty =
            BindableProperty.Create(nameof(Keyboard), typeof(IKeyboard), typeof(EntryLable), null);

        public IKeyboard Keyboard
        {
            get => (IKeyboard)GetValue(keyboardProperty);
            set
            {
                SetValue(keyboardProperty, value);

                if (Keyboard != null)
                {
                    value.CharInputed += OnCharInputed;
                    value.RequestClear += (s, e) => Text = "";
                }
            }
        }

        private void OnCharInputed(object sender, CharInputedEventArgs e)
        {
            Text += e.Text;
        }

        public EntryLable()
        {
            // 属性设置 
            TextColor = Color.White;
            BackgroundColor = Color.Green;
            Text = "testLable";
           /* GestureRecognizers.Add(new TapGestureRecognizer
            {
                // Launcher.OpenAsync is provided by Xamarin.Essentials
                Command = new Command(async () => await Launcher.OpenAsync(Url))
            });*/
        }
    }
}
