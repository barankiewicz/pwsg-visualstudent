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
using System.Diagnostics;
using System.Windows.Markup;

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
        private string BuildPath;
        private int _selectedTabIndex;

        public ObservableCollection<OpenedFile> OpenedFiles { get { return _openedFiles; } set { _openedFiles = value; OnPropertyChanged(); } }
        public string ConsoleMessages{ get { return _consoleMessages; } set { _consoleMessages = value; OnPropertyChanged(); } }
        public List<ErrorMessage> ErrorMesssages { get { return _errorMessages; } set { _errorMessages = value; OnPropertyChanged(); } }
        public OpenedFile SelectedTab { get { return _selectedTab; } set { _selectedTab = value; OnPropertyChanged(); } }
        public int SelectedTabIndex { get { return _selectedTabIndex; } set { _selectedTabIndex = value; OnPropertyChanged(); } }


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            _errorMessages = new List<ErrorMessage>();

            ProjectPath = "";
            _consoleMessages = "";
            _selectedTab = null;
            _selectedTabIndex = 0;

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
                    OpenedFiles.RemoveAt(i);
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
                SelectedTabIndex = OpenedFiles.Count - 1;
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
                SelectedTabIndex = OpenedFiles.Count - 1;
            }
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Implementation of save
            if(SelectedTab != null)
                SelectedTab.Save();
        }
        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //New file button
            OpenedFile file = new OpenedFile();
            OpenedFiles.Add(file);
            SelectedTabIndex = OpenedFiles.Count - 1;
        }
        private void ExecuteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Implementation of Execute
            if (ProjectPath == "")
            {
                MessageBox.Show("No project is opened!");
                return;
            }
            bool flag = Build_Project();
            if (chooseComboBox.SelectedIndex == 1 && flag)
                Run_Project();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Close tab button
            OpenedFiles.RemoveAt(0);
        }

        private bool Build_Project()
        {
            ConsoleMessages = "";
            BuildPath = System.IO.Path.GetDirectoryName(ProjectPath) + "\\build";

            if(Directory.Exists(BuildPath))
                Directory.Delete(BuildPath, true);

            ErrorMesssages.Clear();
            var props = new Dictionary<string, string>
            {
                {"OutputPath", BuildPath}
            };
            var pc = new ProjectInstance(ProjectPath, props, "14.0");

            StringBuilder sb = new StringBuilder();
            WriteHandler handler = (x) =>
            {
                sb.AppendLine(x);

                var divided = x.Split(new char[] { ' ', ':' }, 4, StringSplitOptions.RemoveEmptyEntries);
                var couldBeError = divided.Length > 1 ? true : false;
                if (couldBeError && divided[1] == "error")
                    ErrorMesssages.Add(new ErrorMessage(divided[0], "error " + divided[2], divided[3]));
                    
            };
            var logger = new ConsoleLogger(LoggerVerbosity.Normal, handler, null, null);
           
            var buildParams = new BuildParameters()
            {
                DetailedSummary = false,
                Loggers = new List<ILogger> { logger },
                DefaultToolsVersion = "14.0"
            };
            var targets = new List<string> { "Build" };
            var reqData = new BuildRequestData(pc, targets.ToArray());

            var res = BuildManager.DefaultBuildManager.Build(buildParams, reqData);
            ConsoleMessages = sb.ToString();

            if (res.OverallResult == BuildResultCode.Failure)
                return false;
            return true;
        }

        private void Run_Project()
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(ProjectPath) + ".exe";
            string fullpath = BuildPath + "\\" + file;
            Process proc = new Process();
            proc.StartInfo.FileName = fullpath;
            proc.StartInfo.WorkingDirectory = BuildPath;
            proc.Start();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageBox.Show(OpenedFiles[0].Body);
        }

        private void Paragraph_TextInput(object sender, TextCompositionEventArgs e)
        {
            MessageBox.Show(OpenedFiles[0].Body);
        }

        private void Run_TextInput(object sender, TextCompositionEventArgs e)
        {
            MessageBox.Show(OpenedFiles[0].Body);
        }
    }
}
