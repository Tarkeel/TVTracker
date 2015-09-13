using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataAccess.Types
{
    public abstract class Observable : INotifyPropertyChanged
    {
        /// <summary>
        /// The listeners
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool VerifyPropertyChange<T>(ref T oldValue, ref T newValue, [CallerMemberName] string propertyName = null)
        {
            //No change occured if values is and was null
            if (oldValue == null && newValue == null) { return false; }
            //Change occured; set value and fire events
            if ((oldValue == null && newValue != null) || !oldValue.Equals((T)newValue))
            {
                oldValue = newValue;
                FirePropertyChanged(propertyName);
                return true;
            }
            //No change occured
            return false;
        }
        protected void FirePropertyChanged(string propertyName)
        {
            //Threading safe check that someone is listening
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}