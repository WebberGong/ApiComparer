using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ApiComparer.Model;
using ApiComparer.Model.Help;

namespace ApiComparer
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApiSelectionChanged(ListBox listBox1, ListBox listBox2)
        {
            if (listBox2.Items.Count == 0 || listBox1.SelectedItem == null)
                return;
            var item1 = (KeyValuePair<ApiKey, ApiList>)listBox1.SelectedItem;
            var dic2 = (IDictionary<ApiKey, ApiList>)listBox2.Items.SourceCollection;
            var item2 = dic2.FirstOrDefault(x => x.Key.Equals(item1.Key));
            if (item2.Value != null)
            {
                listBox2.SelectedItem = item2;
                listBox2.ScrollIntoView(item2);
            }
            else
            {
                listBox2.SelectedItem = null;
            }
            listBox1.Focus();
        }

        private void Api1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApiSelectionChanged(Api1, Api2);
        }

        private void Api2_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApiSelectionChanged(Api2, Api1);
        }

        private void JsonTree_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var elem = sender as UIElement;
            elem?.RaiseEvent(eventArg);
        }

        private void ApiSearch(ListBox listBox, TextBox textBox)
        {
            if (listBox.Items.Count == 0)
                return;
            var dic = (IDictionary<ApiKey, ApiList>)listBox.Items.SourceCollection;
            var item = dic.Skip(listBox.SelectedIndex + 1).FirstOrDefault(x => x.Key.Group.Contains(textBox.Text));
            if (item.Value == null)
            {
                item = dic.FirstOrDefault(x => x.Key.Group.Contains(textBox.Text));
            }
            if (item.Value != null)
            {
                listBox.SelectedItem = item;
                listBox.ScrollIntoView(item);
            }
            else
            {
                listBox.SelectedItem = null;
            }
            listBox.Focus();
        }

        private void SearchButton1_OnClick(object sender, RoutedEventArgs e)
        {
            ApiSearch(Api1, SearchText1);
        }

        private void SearchText1_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApiSearch(Api1, SearchText1);
            }
        }

        private void SearchButton2_OnClick(object sender, RoutedEventArgs e)
        {
            ApiSearch(Api2, SearchText2);
        }

        private void SearchText2_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApiSearch(Api2, SearchText2);
            }
        }
    }
}