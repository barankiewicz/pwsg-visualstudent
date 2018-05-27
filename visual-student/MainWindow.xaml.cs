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
using PluginContracts;
using System.Reflection;

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
        private ObservableCollection<IPlugin> _plugins;
        private ObservableCollection<string> _pluginNames;
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
        public ObservableCollection<IPlugin> Plugins { get { return _plugins; } set { _plugins = value; OnPropertyChanged(); } }
        public ObservableCollection<string> PluginNames { get { return _pluginNames; } set { _pluginNames = value; OnPropertyChanged(); } }

        public object ViewModel { get; private set; }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            _errorMessages = new List<ErrorMessage>();
            _plugins = new ObservableCollection<IPlugin>();
            _pluginNames = new ObservableCollection<string>();
            Load_Plugins();

            ProjectPath = "";
            _consoleMessages = "";
            _selectedTab = null;
            _selectedTabIndex = 0;

            //openFiles.ItemsSource = OpenedFiles;
            errorListBox.ItemsSource = ErrorMesssages;
            pluginsMenuItem.ItemsSource = PluginNames;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //About button
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
                for(int i = 0; i < OpenedFiles.Count; i++)
                {
                    if(file.Name == OpenedFiles[i].Name)
                    {
                        SelectedTabIndex = i;
                        return;
                    }
                }
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
                AddMenuItem(file, OpenedFiles.Count - 1);
                SelectedTabIndex = OpenedFiles.Count - 1;
            }
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Implementation of save
            if(SelectedTab != null)
                SelectedTab.Save();
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            //Implementation of saveAs
            if (SelectedTab != null)
                SelectedTab.SaveAs();
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
            OpenedFile file = ((sender as Button).DataContext) as OpenedFile;
            if(file.Modified)
            {
                if (MessageBox.Show("This file has been modified. Do you want to save before closing?", "Save file?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    file.Save();
            }
            OpenedFiles.Remove(file);
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

        private void RichTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            RichTextBox Rtb = (RichTextBox)sender;
            /* text to insert */
            string text = e.Text;

            /* get start pointer */
            TextPointer startPtr = Rtb.Document.ContentStart;

            /* get current caret position */
            int start = startPtr.GetOffsetToPosition(Rtb.CaretPosition);

            /* insert text */
            Rtb.CaretPosition.InsertTextInRun(text);

            /* update caret position */
            Rtb.CaretPosition = startPtr.GetPositionAtOffset((start) + text.Length);

            /* update focus */
            Rtb.Focus();
            e.Handled = true;
            OpenedFiles[SelectedTabIndex].Modified = true;
            //if (OpenedFiles[SelectedTabIndex] != null && !OpenedFiles[SelectedTabIndex].Modified)
            //    OpenedFiles[SelectedTabIndex].Modified = true;
        }

        private void Load_Plugins()
        {
            var files = Directory.EnumerateFiles(System.AppDomain.CurrentDomain.BaseDirectory);
            foreach(string file in files)
                if(System.IO.Path.GetExtension(file) == ".dll")
                {
                    Assembly asm = Assembly.LoadFile(file);
                    Type[] tlist = asm.GetTypes();
                    foreach (Type t in tlist)
                    {
                        var i = t.GetInterface("IPlugin");
                        if (i != null)
                        {
                            IPlugin myPlugin = Activator.CreateInstance(t) as IPlugin;
                            Plugins.Add(myPlugin);
                            PluginNames.Add(t.Name);
                            break;
                        }
                    }
                }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void Run_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Paragraph_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RichTextBox Rtb = (RichTextBox)sender;
            if (e.Key == Key.Enter)
            {
                /* text to insert */
                string text = "\n";

                /* get start pointer */
                TextPointer startPtr = Rtb.Document.ContentStart;

                /* get current caret position */
                int start = startPtr.GetOffsetToPosition(Rtb.CaretPosition);

                /* insert text */
                Rtb.CaretPosition.InsertTextInRun(text);

                /* update caret position */
                Rtb.CaretPosition = startPtr.GetPositionAtOffset((start) + text.Length);

                /* update focus */
                Rtb.Focus();
                e.Handled = true;
                OpenedFiles[SelectedTabIndex].Modified = true;
            }
        }

        private void AddMenuItem(OpenedFile file, int index)
        {
            TabItem tabItem = new TabItem();
            DataTemplate dataTemplate = (DataTemplate)FindResource("HeaderTemplate");
            var res = dataTemplate.Resources;
            var does = res.Contains("MainGrid");
            tabItem.HeaderTemplate = (DataTemplate)FindResource("HeaderTemplate");
            tabItem.DataContext = OpenedFiles[index];

            MessageBox.Show(VisualTreeHelper.GetChildrenCount(tabItem).ToString());
            //DataTemplateSelector templateSelector = new DataTemplateSelector();
            //templateSelector.

            Paragraph paragraph = new Paragraph();
            Run run = new Run();

            //tabItem.He
            //TextBlock header = (TextBlock)tabItem.FindName("MainGrid");

            //Binding headerBinding = new Binding("Name");
            ////PresentationTraceSources.SetTraceLevel(headerBinding, PresentationTraceLevel.High);
            //headerBinding.Source = OpenedFiles[index];
            //headerBinding.Mode = BindingMode.TwoWay;
            //headerBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //tabItem.SetBinding(TabItem.HeaderProperty, headerBinding);

            Binding bodyBinding = new Binding("Body");
            //PresentationTraceSources.SetTraceLevel(bodyBinding, PresentationTraceLevel.High);
            bodyBinding.Source = OpenedFiles[index];
            bodyBinding.Mode = BindingMode.TwoWay;
            bodyBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            run.SetBinding(Run.TextProperty, bodyBinding);

            paragraph.Inlines.Add(run);
            FlowDocument flowDocument = new FlowDocument(paragraph);
            RichTextBox rtb = new RichTextBox(flowDocument);

            tabItem.Content = rtb;
            openFiles.Items.Add(tabItem);
            foreach (var textblock in FindVisualChildren<Grid>(this))
            {
                if (textblock.Name == "MainGrid")
                {
                    /*   Your code here  */
                }
            }
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }

}
