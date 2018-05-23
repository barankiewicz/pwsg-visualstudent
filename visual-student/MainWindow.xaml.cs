using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

namespace visual_student
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //Implementation of INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<OpenedFile> _openedFiles;
        private OpenedFile _selectedTab;
        ObservableCollection<OpenedFile> OpenedFiles { get { return _openedFiles; } set { _openedFiles = value; RaisePropertyChanged("OpenedFiles"); } }
        OpenedFile SelectedTab { get { return _selectedTab; } set { _selectedTab = value; RaisePropertyChanged("SelectedTab"); } }


        public MainWindow()
        {
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            SelectedTab = new OpenedFile();
            openFiles.ItemsSource = OpenedFiles;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a simple C# editor and compiler.", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Exit button
            Close();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //Open file button
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "C# Files (*cs) |*.cs";
            if(opf.ShowDialog() == true)
            {
                OpenedFile file = OpenedFile.LoadFromFileStream(opf.FileName, opf.SafeFileName);
                OpenedFiles.Add(file);
                openFiles.SelectedIndex = OpenedFiles.Count - 1;
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //New file button
            OpenedFile file = new OpenedFile();
            OpenedFiles.Add(file);
            openFiles.SelectedIndex = OpenedFiles.Count - 1;
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            //Open project button
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for(int i = 0; i < openFiles.Items.Count; i++)
                {
                    openFiles.Items.Remove(openFiles.Items[i]);
                }
                string mainPath = fbd.SelectedPath;
                var itemProvider = new ItemProvider();
                var items = itemProvider.GetItems(fbd.SelectedPath);
                fileTree.DataContext = items;
                fileTree.Items.Refresh();
            }
        }

        private void fileTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object selectedItem = fileTree.SelectedItem;
            Point pos = e.GetPosition(this);

            if(selectedItem is FileItem)
            {
                FileItem file = selectedItem as FileItem;
                for (int i = 0; i < openFiles.Items.Count; i++)
                {
                    TabItem it = openFiles.Items[i] as TabItem;
                    if ((string)it.Header == file.Name)
                    {
                        openFiles.SelectedIndex = i;
                        return;
                    }
                }
                //Open new tab
                OpenedFile openedfile = OpenedFile.LoadFromFileStream(file.Path, file.Name);
                OpenedFiles.Add(openedfile);
                openFiles.SelectedIndex = OpenedFiles.Count - 1;
            }
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "C# Files (*cs) |*.cs";
            if (opf.ShowDialog() == true)
            {
                OpenedFile file = OpenedFile.LoadFromFileStream(opf.FileName, opf.SafeFileName);
                OpenedFiles.Add(file);
                openFiles.SelectedIndex = OpenedFiles.Count - 1;
            }
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
           //Implementation of save
        }
        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //New file button
            OpenedFile file = new OpenedFile();
            OpenedFiles.Add(file);
            openFiles.SelectedIndex = OpenedFiles.Count - 1;
        }
        private void ExecuteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Implementation of Execute
        }
    }
}
