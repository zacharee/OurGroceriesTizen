using System;
using Tizen;
using Xamarin.Forms;

namespace OurGroceriesTizen
{
    public class CrossedOffLabel : Label
    {
        public static readonly BindableProperty CrossedOffProperty =
            BindableProperty.Create(
                "CrossedOff", 
                typeof(bool), 
                typeof(CrossedOffLabel), 
                false);
        
        public bool CrossedOff
        {
            get => (bool)GetValue(CrossedOffProperty);
            set => SetValue(CrossedOffProperty, value);
        }

        public CrossedOffLabel()
        {
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            VerticalOptions = LayoutOptions.Center;
            HorizontalTextAlignment = TextAlignment.Center;
            VerticalTextAlignment = TextAlignment.Center;
            HeightRequest = 80;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
        
            try
            {
                TextColor = CrossedOff ? Color.Gray : Color.White;
                TextDecorations = CrossedOff ? TextDecorations.Strikethrough : TextDecorations.None;
            }
            catch (Exception e)
            {
                Log.Error("OurGroceries", e.Message);
            }
        }
    }
}