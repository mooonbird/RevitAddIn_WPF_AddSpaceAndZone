using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ComboBox = System.Windows.Controls.ComboBox;
using Binding = System.Windows.Data.Binding;
using System.Transactions;

namespace RevitAddIn_MyProject
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private DataManager _DataManager;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(DataManager dataManager)
        {
            InitializeComponent();
            _DataManager = dataManager;

            levelComboBox.ItemsSource = _DataManager.Levels;
            levelComboBox.DisplayMemberPath = "Name";
            levelComboBox.SelectedItem = _DataManager.CurrentLevel;

            spacesListView.DataContext = _DataManager.SpaceCollection;
            _ = spacesListView.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

            zonesTreeView.DataContext = _DataManager.ZoneCollection;
            _ = zonesTreeView.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());
        }

        public void Dispose() { }

        private void LevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DataManager.CurrentLevel = levelComboBox.SelectedItem as Level;
        }

        private void CreateSpacesButton_Click(object sender, RoutedEventArgs e)
        {
            _DataManager.CreateSpaces();
        }

        private void CreateZoneButton_Click(object sender, RoutedEventArgs e)
        {
            _DataManager.CreateZone();
        }

        private void ZonesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ZoneSpacesNode)
            {
                editZoneButton.IsEnabled = true;
            }
            else
            {
                editZoneButton.IsEnabled = false;
            }
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EditZoneButton_Click(object sender, RoutedEventArgs e)
        {
            ZoneSpacesNode zoneSpacesNode= zonesTreeView.SelectedItem as ZoneSpacesNode;


            try
            {
                SubTransaction subTransaction = new SubTransaction(_DataManager.CommandData.Application.ActiveUIDocument.Document);
                _ = subTransaction.Start();

                bool? dialogResult = default;
                using (EditZoneWindow editZoneWindow = new EditZoneWindow(_DataManager, zoneSpacesNode))
                {
                    dialogResult = editZoneWindow.ShowDialog();
                }
                if (dialogResult == true)
                {
                    _ = subTransaction.Commit();
                    _DataManager.UpdateSpaceDictionary();
                    _DataManager.UpdateZoneDictionary();
                    _DataManager.UpdateObservableCollections();
                }
                else
                {
                    _ = subTransaction.RollBack();
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }

           

            
        }
    }
}
