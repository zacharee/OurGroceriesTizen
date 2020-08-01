using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OurGroceries.Api.Entities;
using OurGroceriesTizen.Api;
using Tizen;
using Xamarin.Forms;

namespace OurGroceriesTizen
{
    public partial class MainWindow
    {
        private readonly ObservableCollection<ListItems> _lists = new ObservableCollection<ListItems>();

        private static Client client => (Application.Current as App)?.Client;

        public MainWindow()
        {
            InitializeComponent();

            ListList.ItemsSource = _lists;
            Title = "Lists";
        }

        public void ParseContent(string teamJson)
        {
            if (string.IsNullOrEmpty(teamJson))
            {
                return;
            }
            
            try
            {
                var team = JsonConvert.DeserializeObject<ShoppingTeam>(teamJson);

                team.shoppingLists.ForEach(async delegate (ShoppingList shoppingList)
                {
                    var list = await client.GetList(shoppingList.id, team.teamId);
                    list.teamId = team.teamId;

                    _lists.Add(list);
                });
            }
            catch (Exception e)
            {
                Log.Error("OurGroceries", "Exception: " + e.Message + e.StackTrace);
            }
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var selectedItem = (ListItems) args.SelectedItem;
            if (selectedItem == null) return;

            (sender as ListView).SelectedItem = null;
            
            var listPage = new ListWindow(selectedItem.name, selectedItem.id, selectedItem.teamId, selectedItem);
            await Navigation.PushAsync(listPage);
        }
    }
}