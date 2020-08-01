using Xamarin.Forms;

namespace OurGroceriesTizen
{
    public class ActivityIndicatorPage : ContentPage
    {
        public ActivityIndicatorPage()
        {
            Content = new ActivityIndicator
            {
                IsRunning = true
            };
        }
    }
}