using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Types
{
    public class ExternalSource : Observable
    {
        internal ExternalSource()
        {
            externalShows = new ObservableExternalShowCollection();
        }
        private long id;
        public long ID
        {
            get { return id; }
            set { VerifyPropertyChange(ref id, ref value); }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { VerifyPropertyChange(ref title, ref value); }
        }
        public ObservableExternalShowCollection externalShows;
        public ObservableExternalShowCollection ExternalShows { get { return externalShows; } }
    }
}