using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace visual_student
{
    public class ItemProvider
    {
        public List<Item> GetItemsInternal(string path)
        {
            var items = new List<Item>();

            var dirInfo = new DirectoryInfo(path);

            foreach (var directory in dirInfo.GetDirectories())
            {
                var item = new DirectoryItem
                {
                    Name = directory.Name,
                    Path = directory.FullName,
                    Items = GetItemsInternal(directory.FullName)
                };

                items.Add(item);
            }

            foreach (var file in dirInfo.GetFiles())
            {
                var item = new FileItem
                {
                    Name = file.Name,
                    Path = file.FullName
                };

                if(file.Extension == ".cs")
                    items.Add(item);
            }

            return items;
        }

        public List<Item> GetItems(string path, out string csProjPath)
        {
            csProjPath = "";
            var items = new List<Item>();

            var dirInfo = new DirectoryInfo(path);
            bool isCsProj = false;

            foreach (var file in dirInfo.GetFiles())
            {
                var item = new FileItem
                {
                    Name = file.Name,
                    Path = file.FullName
                };

                if (file.Extension == ".cs")
                    items.Add(item);

                if (file.Extension == ".csproj")
                {
                    isCsProj = true;
                    csProjPath = file.FullName;
                }
                    
            }

            if (!isCsProj)
            {
                MessageBox.Show("There's no .csproj file in main folder");
                return null;
            }

            foreach (var directory in dirInfo.GetDirectories())
            {
                var item = new DirectoryItem
                {
                    Name = directory.Name,
                    Path = directory.FullName,
                    Items = GetItemsInternal(directory.FullName)
                };

                items.Add(item);
            }

            return items;
        }
    }
}
