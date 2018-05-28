using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace visual_student
{
    public class DirectoryItem : Item
    {
        public List<Item> Items { get; set; }
        public string ProjPath;
        public DirectoryItem()
        {
            Items = new List<Item>();
            ProjPath = "";
        }

        public DirectoryItem(string s, string path, List<Item> items)
        {
            Items = items;
            Name = s;
            ProjPath = path;
        }
    }
}
