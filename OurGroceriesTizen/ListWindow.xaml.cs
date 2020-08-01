using System.Collections.Generic;
using System.Collections.ObjectModel;
using OurGroceriesTizen.Api;
using Xamarin.Forms;

namespace OurGroceriesTizen
{
    public partial class ListWindow
    {
        private static Client client => (Application.Current as App)?.Client;
        private readonly ObservableCollection<ListItems.ListItem> _items;
        private readonly string _listId;
        private readonly string _teamId;

        public ListWindow(string title, string listId, string teamId, ListItems items)
        {
            InitializeComponent();

            ItemList.ItemsSource = items.Items;
            _items = items.Items;
            
            _listId = listId;
            _teamId = teamId;
            
            Label.Text = title;
        }

        private async void OnListItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (ItemList.SelectedItem == null) return;
            
            Device.BeginInvokeOnMainThread(() => Progress.IsVisible = true);
            
            var selectedItem = args.SelectedItem as ListItems.ListItem;
            var prevCo = selectedItem.crossedOff;

            ItemList.SelectedItem = null;

            await client.SetItemCrossedOff(_listId, _teamId, selectedItem.id, !prevCo);

            Device.BeginInvokeOnMainThread(() =>
            {
                selectedItem.crossedOff = !prevCo;
                _items.Sort();
                Progress.IsVisible = false;
            });
        }
    }
}