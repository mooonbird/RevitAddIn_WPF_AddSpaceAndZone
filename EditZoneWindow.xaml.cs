using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace RevitAddIn_MyProject
{
    /// <summary>
    /// EditZoneWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditZoneWindow : Window, IDisposable
    {
        private DataManager _DataManager;
        private ZoneSpacesNode _CurrentZoneSpacesNode;

        public EditZoneWindow()
        {
            InitializeComponent();
        }

        public EditZoneWindow(DataManager dataManager, ZoneSpacesNode currentZoneSpacesNode)
        {
            _DataManager = dataManager;
            _CurrentZoneSpacesNode = currentZoneSpacesNode;

            InitializeComponent();

            Title = "EditZone: " + currentZoneSpacesNode.ZoneName;
            dataManager.UpdateAvailableSpaceCollection(currentZoneSpacesNode);
            availableSpacesListView.DataContext = dataManager.AvailableSpaceCollection;
            _ = availableSpacesListView.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

            dataManager.UpdateCurrentZoneSpaceCollection(currentZoneSpacesNode);
            currentZoneSpacesListBox.DataContext = dataManager.CurrentZoneSpaceCollection;
            _ = currentZoneSpacesListBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

        }

        public void Dispose() { }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _DataManager.SelectedZoneAddSpaces(availableSpacesListView.SelectedItems, _CurrentZoneSpacesNode);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            _DataManager.SelectedZoneRemoveSpaces(currentZoneSpacesListBox.SelectedItems, _CurrentZoneSpacesNode);
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void availableSpacesListView_GotFocus(object sender, RoutedEventArgs e)
        {
            addButton.IsEnabled = true;
            removeButton.IsEnabled = false;
        }

        private void currentZoneSpacesListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            addButton.IsEnabled = false;
            removeButton.IsEnabled = true;
        }
    }
}
