using ComponentsTest.Components.Controls;

using System;
using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

using XButton=Xamarin.Forms.Button;

namespace ComponentsTest.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            // 编辑时候 赋值
            lableC.Text = "dfdfdsg";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            XButton[] buttons = { btn1, btn2, btn3};
            Keyboard board = new Keyboard(buttons);
            board.SetClearBtn(clearBtn);
            lableC.Keyboard= board;

        }


        private sealed class Keyboard : IKeyboard
        {
            public Keyboard(params XButton[] buttons)
            {
                foreach (XButton button in buttons)
                {
                    button.Clicked += OnClick;
                }
            }

            public void SetClearBtn(XButton clearBtn) { 
                clearBtn.Clicked += (s,e)=> RequestClear?.Invoke(this,EventArgs.Empty);
            } 

           public event EventHandler RequestClear;          

            private void OnClick(object sender, EventArgs e)
            {
                XButton btn = (XButton)sender;

                CharInputed?.Invoke(this, new CharInputedEventArgs(btn.Text));
            }

            public event EventHandler<CharInputedEventArgs> CharInputed;
        }
    }
}