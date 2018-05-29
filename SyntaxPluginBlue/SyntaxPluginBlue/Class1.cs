using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginContracts;

namespace SyntaxPluginBlue
{
    public class SyntaxPluginBlue : IPlugin
    {
        public string Name
        {
            get;
        }

        public SyntaxPluginBlue()
        {
            Name = "SyntaxPluginBlue";
        }

        public void Do(System.Windows.Controls.RichTextBox richTextBox)
        {
            //DO
        }
    }
}
