using ComponentsTest.ViewModels;

using System.ComponentModel;

using Xamarin.Forms;

namespace ComponentsTest.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}