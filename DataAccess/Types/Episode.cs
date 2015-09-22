using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Types
{
    public class Episode : Observable
    {
        internal Episode(Season _season)
        {
            rating = -1;
            season = _season;
            if (season != null)
            {
                season.PropertyChanged += Season_PropertyChanged; 
                season.Episodes.Add(this);
            }
        }
        private long id;
        public long ID
        {
            get { return id; }
            set { VerifyPropertyChange(ref id, ref value); }
        }
        private string episodeNo;
        public string EpisodeNo
        {
            get { return episodeNo; }
            set
            {
                //Changing the episodeNo will also change the composite for the episode.
                if (VerifyPropertyChange(ref episodeNo, ref value)) { FirePropertyChanged("Composite"); }
            }
        }
        private string code;
        public string Code
        {
            get { return code; }
            set { VerifyPropertyChange(ref code, ref value); }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { VerifyPropertyChange(ref title, ref value); }
        }
        private int rating;
        public int Rating
        {
            get { return rating; }
            set { VerifyPropertyChange(ref rating, ref value); }
        }
        private string airdate;
        public string Airdate
        {
            get { return airdate; }
            set { VerifyPropertyChange(ref airdate, ref value); }
        }
        private bool watched;
        public bool Watched
        {
            get { return watched; }
            set { VerifyPropertyChange(ref watched, ref value); }
        }
        private Season season;
        public Season Season
        {
            get { return season; }
            set
            {
                Season oldSeason = season;
                if (VerifyPropertyChange(ref season, ref value))
                {
                    //Update the episode collections of both the old and new season.
                    if (oldSeason != null) { oldSeason.Episodes.Remove(this); }
                    if (season != null) { season.Episodes.Add(this); }
                    //Change which season we are listening to changes in show for
                    if (oldSeason != null) { oldSeason.PropertyChanged -= Season_PropertyChanged; }
                    if (season != null) { season.PropertyChanged += Season_PropertyChanged; }
                    //Changing the season will also change the composite for the episode.
                    FirePropertyChanged("Composite");
                }
            }
        }
        /// <summary>
        /// A composite identifier containing the name of the show that the season belongs to,the numbering of the season and the numbering of the episode.
        /// </summary>
        public string Composite
        {
            get
            {
                string _title = "Unknown-S0";
                if (season != null) { _title = season.Composite; }
                return String.Format("{0}E{1}", _title, episodeNo);
            }
        }
        private void Season_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Season _season = (Season)sender;
            switch (e.PropertyName)
            {
                case "Show":
                    //Changing the show of the episode's season will also change the composite for the episode.
                    FirePropertyChanged("Composite");
                    break;
            }
        }
    }
}