using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace visual_student
{
    public class OpenedFile
    {
        public string name { get; set; }
        public string body { get; set; }
        public string path { get; set; }
        public OpenedFile(string _name, string _body, string _path)
        {
            name = _name;
            body = _body;
            path = _path;
        }

        public OpenedFile()
        {
            name = "New File";
            body = " ";
            path = "C:\\";
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
