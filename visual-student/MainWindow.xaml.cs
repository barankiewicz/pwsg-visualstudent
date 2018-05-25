using Microsoft.Build.BuildEngine;
using Microsoft.Build.Execution;
using Microsoft.Build.Evaluation;
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
using Microsoft.Build.Framework;

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
        private string ProjectPath;
        ObservableCollection<OpenedFile> OpenedFiles { get { return _openedFiles; } set { _openedFiles = value; RaisePropertyChanged("OpenedFiles"); } }


        public MainWindow()
        {
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            openFiles.ItemsSource = OpenedFiles;
            ProjectPath = "";
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

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            //Open project button
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = "X:\\Programming\\C#\\testapp\\testapp";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for(int i = 0; i < openFiles.Items.Count; i++)
                {
                    openFiles.Items.Remove(openFiles.Items[i]);
                }
                string mainPath = fbd.SelectedPath;
                var itemProvider = new ItemProvider();
                var items = itemProvider.GetItems(fbd.SelectedPath, out ProjectPath);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TabItem clicked = (TabItem)sender;
            MessageBox.Show("L:OL");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Execute button
            var path = ProjectPath;
            var props = new Dictionary<string, string>
            {
                {"Configuration", "Debug"},
                {"Platform", "AnyCPU"},
                {"OutputPath", "X:\\Programming"}
            };
            var pc = new ProjectInstance(path, props, "14.0");
            var buildParams = new BuildParameters()
            {
                DetailedSummary = true,
                Loggers = new List<ILogger> { new ConsoleLogger() },
                DefaultToolsVersion = "14.0"
            };
            var targets = new List<string> { "PrepareForBuild", "Build" };
            var reqData = new BuildRequestData(pc, targets.ToArray());
            BuildManager.DefaultBuildManager.BeginBuild(buildParams);
            var buildResult = BuildManager.DefaultBuildManager.BuildRequest(reqData);
            MessageBox.Show($"MSBuild build complete: {buildResult.OverallResult}");
        }
    }
}
