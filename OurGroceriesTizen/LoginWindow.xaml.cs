using System;
using System.Linq;
using System.Threading.Tasks;
using Tizen;
using Tizen.NUI;
using Tizen.Uix.InputMethod;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Window = ElmSharp.Window;

namespace OurGroceriesTizen
{
    public partial class LoginWindow : BezelInteractionPage
    {
        public delegate Task<bool> OnLoginSubmitted(string email, string password);

        private readonly OnLoginSubmitted _listener;

        public LoginWindow(OnLoginSubmitted listener)
        {
            _listener = listener;
            InitializeComponent();
        }
        
        public void OnSubmit(object sender, EventArgs args)
        {
            Progress.IsVisible = true;
            HandleLogin();
        }

        public async void HandleLogin()
        {
            var success = await _listener(Email.Text, Password.Text);

            Progress.IsVisible = false;

            if (success)
            {
                
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Toast.DisplayText("Check login details.", 3000);
                    }
                    catch (Exception e)
                    {
                        Log.Error("OurGroceries", e.Message);
                    }
                });
            }
        }
    }
}