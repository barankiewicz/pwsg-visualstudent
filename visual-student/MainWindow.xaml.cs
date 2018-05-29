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
        private string ProjectPath;
        private string BuildPath;
        private int _selectedTabIndex;

        public ObservableCollection<OpenedFile> OpenedFiles { get { return _openedFiles; } set { _openedFiles = value; OnPropertyChanged(); } }
        public string ConsoleMessages{ get { return _consoleMessages; } set { _consoleMessages = value; OnPropertyChanged(); } }
        public List<ErrorMessage> ErrorMessages { get { return _errorMessages; } set { _errorMessages = value; OnPropertyChanged(); } }
        public int SelectedTabIndex { get { return _selectedTabIndex; } set { _selectedTabIndex = value; OnPropertyChanged(); } }
        public ObservableCollection<IPlugin> Plugins { get { return _plugins; } set { _plugins = value; OnPropertyChanged(); } }
        public ObservableCollection<string> PluginNames { get { return _pluginNames; } set { _pluginNames = value; OnPropertyChanged(); } }
        public List<Item> OpenedProjects;
        public List<RichTextBox> CreatedBoxes;
        public List<int> AppliedPlugins;
        public List<string> prevTexts;


        public object ViewModel { get; private set; }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            _openedFiles = new ObservableCollection<OpenedFile>();
            _errorMessages = new List<ErrorMessage>();
            _plugins = new ObservableCollection<IPlugin>();
            _pluginNames = new ObservableCollection<string>();
            OpenedProjects = new List<Item>();
            CreatedBoxes = new List<RichTextBox>();
            AppliedPlugins = new List<int>();
            prevTexts = new List<string>();

            errorListBox.ItemsSource = ErrorMessages;
            pluginsMenuItem.ItemsSource = PluginNames;
            openFiles.ItemsSource = OpenedFiles;
            Load_Plugins();

            ProjectPath = "";
            _consoleMessages = "";
            _selectedTabIndex = 0;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //About button
            MessageBox.Show("This is a simple C# editor and compiler.", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Exit button
            for(int i = OpenedFiles.Count - 1; i >=0 ; i--)
            {
                OpenedFile file = OpenedFiles[i];
                if (file.Modified)
                {
                    if (MessageBox.Show("This file has been modified. Do you want to save before closing?", "Save file " + file.Name + "?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        file.Save();
                }
                OpenedFiles.Remove(file);
            }
            Close();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            //Open project button
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ItemProvider prov = new ItemProvider();
                List<Item> tmp = prov.GetItems(fbd.SelectedPath, out ProjectPath);
                if (tmp == null)
                    return;
                OpenedProjects.Add(tmp[0]);
                fileTree.DataContext = OpenedProjects;
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
                SelectedTabIndex = OpenedFiles.Count - 1;
            }
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Implementation of save
            if(OpenedFiles[SelectedTabIndex] != null)
                OpenedFiles[SelectedTabIndex].Save();
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            //Implementation of saveAs
            if (OpenedFiles[SelectedTabIndex] != null)
                OpenedFiles[SelectedTabIndex].SaveAs();
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
            if (file.Modified)
            {
                if (MessageBox.Show("This file has been modified. Do you want to save before closing?", "Save file " + file.Name + "?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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

            ErrorMessages.Clear();
            var props = new Dictionary<string, string>
            {
                {"OutputPath", BuildPath}
            };
            var pc = new ProjectInstance(ProjectPath, props, "4.0");

            StringBuilder sb = new StringBuilder();
            WriteHandler handler = (x) =>
            {
                sb.AppendLine(x);

                var divided = x.Split(new char[] { ' ', ':' }, 4, StringSplitOptions.RemoveEmptyEntries);
                var couldBeError = divided.Length > 1 ? true : false;
                if (couldBeError && divided[1] == "error")
                    ErrorMessages.Add(new ErrorMessage(divided[0], "error " + divided[2], divided[3]));
                    
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
            errorListBox.Items.Refresh();
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

        private void ApplyPlugin(int index)
        {
            foreach(RichTextBox r in CreatedBoxes)
                Plugins[index].Do(r);
        }

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            RichTextBox richTextBox = (RichTextBox)sender;
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            string prev = range.Text;
            string body = OpenedFiles[SelectedTabIndex].Body;
            string test = prev.TrimEnd(new char[] { '\n', '\r' });
            string test2 = body.TrimEnd(new char[] { '\n', '\r' });

            if (test == test2)
                return;


            OpenedFiles[SelectedTabIndex].Body = range.Text;

            //foreach (int i in AppliedPlugins)
            //    Plugins[i].Do(richTextBox);


            if (test != test2)
                OpenedFiles[SelectedTabIndex].Modified = true;
        }

        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            var tabItem = (sender as TabItem);

            if (null != tabItem.DataContext)
            {
                //  Hey, what about TabControl.ItemTemplate, eh? 
                var dataTemplate = (DataTemplate)tabItem.FindResource("RichTextBoxTemplate");

                tabItem.Content = dataTemplate.LoadContent();
                (tabItem.Content as FrameworkElement).DataContext = tabItem.DataContext;
            }
        }


        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            RichTextBox rtb = (RichTextBox)sender;

            
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            run.Text = OpenedFiles[SelectedTabIndex].Body;
            prevTexts.Add(OpenedFiles[SelectedTabIndex].Body);

            paragraph.Margin = new Thickness(0);
            paragraph.FontFamily = new FontFamily("Monaco");
            paragraph.FontSize = 12;
            paragraph.Inlines.Add(run);
            FlowDocument flowDocument = new FlowDocument(paragraph);
            rtb.Document = flowDocument;
            
            foreach (int i in AppliedPlugins)
                Plugins[i].Do(rtb);

            if (!CreatedBoxes.Contains(rtb))
                CreatedBoxes.Add(rtb);

            rtb.Focus();

        }

        private void fileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(fileTree.SelectedItem is DirectoryItem)
            {
                DirectoryItem it = (DirectoryItem)fileTree.SelectedItem;
                var split = it.Name.Split(':');

                if (split.Length > 1 && split[0] == "Project")
                    ProjectPath = it.ProjPath;
            }

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            ContentPresenter cont = (ContentPresenter)textBlock.TemplatedParent;
            MenuItem menu = (MenuItem)cont.TemplatedParent;

            int index = 0;
            //Find index
            for (int i = 0; i < PluginNames.Count; i++)
            {
                if (textBlock.Text == PluginNames[i])
                {
                    index = i;
                    break;
                }
            }

            ApplyPlugin(index);

            if (menu.IsChecked)
            {
                AppliedPlugins.Remove(index);
                menu.IsChecked = false;
            }
            else
            {
                AppliedPlugins.Add(index);
                menu.IsChecked = true;
            } 
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            for (int i = OpenedFiles.Count - 1; i >= 0; i--)
            {
                OpenedFile file = OpenedFiles[i];
                if (file.Modified)
                {
                    if (MessageBox.Show("This file has been modified. Do you want to save before closing?", "Save file " + file.Name + "?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        file.Save();
                }
                OpenedFiles.Remove(file);
            }
        }

        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }
    }

}
