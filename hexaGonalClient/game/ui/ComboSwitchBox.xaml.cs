using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hexaGonalClient.game.util
{
    /// <summary>
    /// Interaction logic for ComboSwitchBox.xaml
    /// </summary>
    public partial class ComboSwitchBox : UserControl
    {

        private int selectedItem = 0;
        private List<KeyValuePair<int, object>> items = new();

        public event EventHandler<object> SelectedChanged;

        public int SeletedItem
        {
            get => items.ElementAt(selectedItem).Key;
            set 
            {
                for (int i = 0; i < items.Count; i++)
                    if (items[i].Key == value)
                        selectedItem = i;

                updateSlectedDisplay();
            }
        }

        public ComboSwitchBox()
        {
            InitializeComponent();
        }

        public void ReadEnumContent<T>(T enval) where T : Enum
        {
            items.Clear();
            selectedItem = 0;
            var v = (int[])Enum.GetValues(typeof(T));
            for (int i = 0; i < v.Length; i++)
                AddItem(v[i], (T)(Object)v[i]);

            if (items.Count > 0)
                updateSlectedDisplay();
        }

        public void AddItem(int key, object value)
        {
            items.Add(new KeyValuePair<int, object>(key, value));
            if (items.Count == 1)
                updateSlectedDisplay();
        }

        public void RemoveItem(int key)
        {
            items.Remove(items.Where(k => k.Key == key).First());
        }

        private void updateSlectedDisplay()
        {
            TxtBlock.Text = items.ElementAt(selectedItem).Value.ToString();
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e) => SelChanged(-1);

        private void ButtonNext_Click(object sender, RoutedEventArgs e) => SelChanged(1);

        private void SelChanged(int offset)
        {
            selectedItem += offset;
            if (selectedItem < 0)
                selectedItem = items.Count - 1;
            else
                selectedItem %= items.Count;

            updateSlectedDisplay();
            SelectedChanged?.Invoke(this, items.ElementAt(selectedItem).Value);
        }
    }
}
