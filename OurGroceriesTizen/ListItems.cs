using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tizen;

namespace OurGroceriesTizen
{
    public class ListItems : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string notesHtml { get; set; }
        public string versionId { get; set; }
        public string notes { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string listType { get; set; }
        
        public string teamId { get; set; }
        public ObservableCollection<ListItem> items { get; set; }

        public ObservableCollection<ListItem> Items
        {
            get => items;
            set
            {
                items = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Items"));
            }
        }

        public override string ToString()
        {
            return $"notesHtml: {notesHtml}\n" +
                   $"versionId: {versionId}\n" +
                   $"notes: {notes}\n" +
                   $"name: {name}\n" +
                   $"id: {id}\n" +
                   $"listType: {listType}\n" +
                   $"items: [{string.Join(",\n", items)}]";
        }

        public class ListItem : INotifyPropertyChanged, IComparable<ListItem>, IEquatable<ListItem>
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string _id;
            private string _value;
            private bool _crossedOff;
            
            public string id
            {
                get => _id;
                set
                {
                    if (_id == value) return;
                    _id = value;

                    OnPropertyChanged("id");
                }
            }

            public string value
            {
                get => _value;
                set
                {
                    if (_value == value) return;
                    _value = value;

                    OnPropertyChanged("value");
                }
            }

            public bool crossedOff
            {
                get => _crossedOff;
                set
                {
                    if (_crossedOff == value) return;
                    _crossedOff = value;
                    
                    OnPropertyChanged("crossedOff");
                }
            }

            public override string ToString()
            {
                return $"id: {id}, value: {value}, crossedOff: {crossedOff}";
            }

            public int CompareTo(ListItem other)
            {
                if (crossedOff && !other.crossedOff)
                {
                    return 1;
                }
                
                if (!crossedOff && other.crossedOff)
                {
                    return -1;
                }
                
                return string.Compare(value.ToLower(), other.value.ToLower(), StringComparison.Ordinal);
            }

            public bool Equals(ListItem other)
            {
                return other != null && id == other.id && value == other.value;
            }

            private void OnPropertyChanged(string property)
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
                }
                catch (Exception e)
                {
                    Log.Error("OurGroceries", e.Message);
                }
            }
        }
    }
}
