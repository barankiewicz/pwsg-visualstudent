using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace visual_student
{
    public class ErrorMessage : INotifyPropertyChanged
    {
        //Implementation of INotifyPropertyChanged interface 
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _line;
        private string _code;
        private string _message;

        public string Line { get { return _line; } set { _line = value; OnPropertyChanged(); } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(); } }
        public string Message { get { return _message; } set { _message = value; OnPropertyChanged(); } }

        public ErrorMessage(string line, string code, string message)
        {
            Line = line;
            Code = code;
            Message = message;
        }
    }
}
