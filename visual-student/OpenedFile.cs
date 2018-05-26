using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace visual_student
{
    public class OpenedFile : INotifyPropertyChanged
    {
        //Implementation of INotifyPropertyChanged interface 
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _name;
        private string _body;
        private string _path;

        public string Name { get { return _name; } set { _name = value; RaisePropertyChanged("Name"); } }
        public string Body { get { return _body; } set { _body = value; RaisePropertyChanged("Body"); } }
        public string Path { get { return _path; } set { _path = value; RaisePropertyChanged("Path"); } }
        public OpenedFile(string name, string body, string path)
        {
            Name = name;
            Body = body;
            Path = path;
        }

        public OpenedFile()
        {
            Name = "New File";
            Body = "";
            Path = "";
        }

        public void Save()
        {
            if(Path == "")
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "C# Files (*cs) |*.cs";
                sfd.AddExtension = true;
                sfd.OverwritePrompt = true;
                if (sfd.ShowDialog() == true)
                {
                    this.Path = System.IO.Path.GetFullPath(sfd.FileName);
                    this.Name = System.IO.Path.GetFileName(sfd.FileName);
                }
                else
                    return;
            }

            try
            {
                FileStream fs = new FileStream(Path, FileMode.CreateNew, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < Body.Length; i++)
                    sw.Write(Body[i]);
                sw.Close();
            } catch (UnauthorizedAccessException e)
            {
            }

        }

        public static OpenedFile LoadFromFileStream(string path, string name)
        {
            StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open));
            StringBuilder sb = new StringBuilder();

            while (!sr.EndOfStream)
            {
                sb.Append(sr.ReadLine());
                sb.Append("\n");
            }
            sr.Close();
            return new OpenedFile(name, sb.ToString(), path);
        }
    }
}
