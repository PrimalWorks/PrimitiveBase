using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PBase
{
    public abstract class BaseNotifiable : BaseDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (
                ((field == null) && (value == null)) ||
                ((field != null) && field.Equals(value))
                )
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
