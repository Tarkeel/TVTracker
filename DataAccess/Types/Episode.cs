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
            season = _season;
            if (season != null) { season.Episodes.Add(this); }
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
            set { VerifyPropertyChange(ref episodeNo, ref value); }
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
                    if (oldSeason != null) { oldSeason.Episodes.Remove(this); }
                    if (season != null) { season.Episodes.Add(this); }
                }
            }
        }
    }
}