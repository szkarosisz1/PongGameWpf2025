using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PongGameWpf2025
{
    public class Pad : INotifyPropertyChanged
        {
            private int _yPosition;

            public int YPosition
            {
                get { return _yPosition; }
                set
                {
                    _yPosition = value;
                    OnPropertyChanged("YPosition");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            void OnPropertyChanged([CallerMemberName] String T = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(T));
            }
     }
}
