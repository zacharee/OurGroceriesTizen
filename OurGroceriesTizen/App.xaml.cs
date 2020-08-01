using System.Threading.Tasks;
using OurGroceries.Api;
using OurGroceriesTizen.Api;
using Tizen;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace OurGroceriesTizen
{
    public partial class App
    {
        public readonly Client Client = new Client();

        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly ContentPage _progress = new ActivityIndicatorPage();
        private readonly NavigationPage _navigationPage;
        private readonly LoginWindow _loginWindow;

        public App()
        {
            _loginWindow = new LoginWindow(OnSignIn);
            _navigationPage = new NavigationPage(_mainWindow)
            {
                HeightRequest = 80
            };

            InitializeComponent();

            MainPage = _progress;
            
            SetUp();
        }

        private async void SetUp()
        {
            var userPerm = new UserPermission();
            var result = await userPerm.CheckAndRequestPermission("http://tizen.org/privilege/internet");
            if (!result)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.DisplayText("Please allow internet access.", 2000);
                });
                Quit();
                return;
            }
            
            Client.OnCreate();
            
            var isSignedIn = Client.IsSignedIn();

            if (!isSignedIn)
            {
                LaunchSignIn();
                return;
            }

            var teamJson = await Client.LoadLists();

            _mainWindow.ParseContent(teamJson);

            MainPage = _navigationPage;
        }

        private async Task<bool> OnSignIn(string email, string password)
        {
            try
            {
                email = "zachary.wander@gmail.com";
                password = "Ouander1236!";
                var result = await Client.SignIn(email, password);

                _mainWindow.ParseContent(result);
                MainPage = _navigationPage;

                return true;
            }
            catch (Client.NotSignedInException)
            {
                return false;
            }
        }
        
        private void LaunchSignIn()
        {
            MainPage = _loginWindow;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            Client.OnSleep();
        }

        protected override void OnResume()
        {
        }
    }
}
