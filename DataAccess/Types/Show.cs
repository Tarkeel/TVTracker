using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace DataAccess.Types
{
    public class Show : Observable
    {
        internal Show()
        {
            externalShows = new ObservableExternalShowCollection();
            seasons = new ObservableSeasonCollection();
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
        private int? year;
        public int? Year
        {
            get { return year; }
            set { VerifyPropertyChange(ref year, ref value); }
        }

        private ObservableExternalShowCollection externalShows;
        public ObservableExternalShowCollection ExternalShows { get { return externalShows; } }
        private ObservableSeasonCollection seasons;
        public ObservableSeasonCollection Seasons { get { return seasons; } }
    }
}