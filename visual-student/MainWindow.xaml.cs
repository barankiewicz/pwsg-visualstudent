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
using System.Runtime.CompilerServices;

namespace visual_student
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //Implementation of INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<OpenedFile> _openedFiles;
        private string _consoleMessages;
        private List<ErrorMessage> _errorMessages;
        private OpenedFile _selectedTab;
        private string ProjectPath;

        public ObservableCollection<OpenedFile> OpenedFiles { get { return _openedFiles; } set { _openedFiles = value; OnPropertyChanged(); } }
        public string ConsoleMessages{ get { return _consoleMessages; } set { _consoleMessages = value; OnPropertyChanged(); } }
        public List<ErrorMessage> ErrorMesssages { get { return _errorMessages; } set { _errorMessages = value; OnPropertyChanged(); } }
        public OpenedFile SelectedTab { get { return _selectedTab; } set { _selectedTab = value; OnPropertyChanged(); } }


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            _errorMessages = new List<ErrorMessage>();

            ProjectPath = "";
            _consoleMessages = "";
            _selectedTab = null;

            openFiles.ItemsSource = OpenedFiles;
            errorListBox.ItemsSource = ErrorMesssages;
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
            if(SelectedTab != null)
                SelectedTab.Save();
            openFiles.Items.Refresh();
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
            if (ProjectPath == "")
            {
                MessageBox.Show("No project is opened!");
                return;
            }

            Build_Project();
            //if (chooseComboBox.Uid == "0")
            //    Build_Project();
            //else
            //    BuildAndRun_Project();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TabItem clicked = (TabItem)sender;
            MessageBox.Show("L:OL");
        }

        private void Message_Handler(object sender, BuildMessageEventArgs e)
        {
            TabItem clicked = (TabItem)sender;
            MessageBox.Show("L:OL");
        }

        private void Build_Project()
        {
            ConsoleMessages = "";
            ErrorMesssages.Clear();
            var props = new Dictionary<string, string>
            {
                {"OutputPath", ProjectPath}
            };
            var pc = new ProjectInstance(ProjectPath, props, "14.0");

            StringBuilder sb = new StringBuilder();
            WriteHandler handler = (x) =>
            {
                sb.AppendLine(x);

                var divided = x.Split(new char[] { ' ', ':' });
                var couldBeError = divided.Length > 1 ? true : false;
                if (couldBeError && divided[2] == "error")
                {
                    ErrorMesssages.Add(new ErrorMessage());
                }
                    
            };
            var logger = new ConsoleLogger(LoggerVerbosity.Minimal, handler, null, null);
           
            var buildParams = new BuildParameters()
            {
                DetailedSummary = false,
                Loggers = new List<ILogger> { logger },
                DefaultToolsVersion = "14.0"
            };
            var targets = new List<string> { "Build" };
            var reqData = new BuildRequestData(pc, targets.ToArray());

            BuildManager.DefaultBuildManager.Build(buildParams, reqData);
            ConsoleMessages = sb.ToString();
            //ConsoleMessagesTextBlock.Text = ConsoleMessages;
        }

        private void BuildAndRun_Project()
        {
            var props = new Dictionary<string, string>
            {
                {"Configuration", "Debug"},
                {"Platform", "AnyCPU"},
                {"OutputPath", ProjectPath}
            };
            var pc = new ProjectInstance(ProjectPath, props, "14.0");
            var logger = new ConsoleLogger();
            logger.Verbosity = LoggerVerbosity.Diagnostic;
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
        }
    }
}
