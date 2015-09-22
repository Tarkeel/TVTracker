using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Types
{
    public class Season : Observable
    {
        internal Season(Show _show)
        {
            seasonNo = -1;
            episodes = new ObservableEpisodeCollection();
            show = _show;
            if (show != null) { show.Seasons.Add(this); }
        }
        private long id;
        public long ID
        {
            get { return id; }
            set { VerifyPropertyChange(ref id, ref value); }
        }
        private int seasonNo;
        public int SeasonNo
        {
            get { return seasonNo; }
            set { VerifyPropertyChange(ref seasonNo, ref value); }
        }
        private string quality;
        public string Quality
        {
            get { return quality; }
            set { VerifyPropertyChange(ref quality, ref value); }
        }
        private Show show;
        public Show Show
        {
            get { return show; }
            set
            {
                Show oldShow = show;
                if (VerifyPropertyChange(ref show, ref value))
                {
                    if (oldShow != null) { oldShow.Seasons.Remove(this); }
                    if (show != null) { show.Seasons.Add(this); }
                }
            }
        }
        private ObservableEpisodeCollection episodes;
        public ObservableEpisodeCollection Episodes { get { return episodes; } }

        /// <summary>
        /// A composite identifier containing the name of the show that the season belongs to and the numbering of the season.
        /// </summary>
        public string Composite
        {
            get
            {
                string _title = "Unknown";
                if (show != null) { _title = show.Title; }
                return String.Format("{0}-S{1}", _title, seasonNo);
            }
        }
    }
}