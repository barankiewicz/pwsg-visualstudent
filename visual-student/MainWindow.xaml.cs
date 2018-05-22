using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                //Open new tab
                TabItem newTabItem = new TabItem();
                newTabItem.Header = opf.SafeFileName;
                StreamReader sr = new StreamReader(new FileStream(opf.FileName, FileMode.Open));
                StringBuilder sb = new StringBuilder();

                while(!sr.EndOfStream)
                {
                    sb.Append(sr.ReadLine());
                    sb.Append("\n");
                }

                RichTextBox textbox = new RichTextBox();
                textbox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                Paragraph p = textbox.Document.Blocks.FirstBlock as Paragraph;
                textbox.Document.Blocks.Add(new Paragraph(new Run(sb.ToString())));
                newTabItem.Content = textbox;
                openFiles.Items.Add(newTabItem);
                openFiles.SelectedIndex = openFiles.Items.Count - 1;
                openFiles.Items.Refresh();
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //New file button
            TabItem newTabItem = new TabItem();
            newTabItem.Header = "New file";
            TextBox textbox = new TextBox();
            textbox.Text = "";
            newTabItem.Content = textbox;
            textbox.TextWrapping = TextWrapping.Wrap;
            textbox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            openFiles.Items.Add(newTabItem);
            openFiles.SelectedIndex = openFiles.Items.Count - 1;
            openFiles.Items.Refresh();
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
                        return;
                }
                //Open new tab
                TabItem newTabItem = new TabItem();
                StreamReader sr = new StreamReader(new FileStream(file.Path, FileMode.Open));
                StringBuilder sb = new StringBuilder();

                while (!sr.EndOfStream)
                {
                    sb.Append(sr.ReadLine());
                    sb.Append("\n");
                }

                RichTextBox textbox = new RichTextBox();
                textbox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                Paragraph p = textbox.Document.Blocks.FirstBlock as Paragraph;
                textbox.Document.Blocks.Add(new Paragraph(new Run(sb.ToString())));
                newTabItem.Header = file.Name;
                newTabItem.Content = textbox;
                openFiles.Items.Add(newTabItem);
                openFiles.SelectedIndex = openFiles.Items.Count - 1;
                openFiles.Items.Refresh();
            }
        }
    }
}
