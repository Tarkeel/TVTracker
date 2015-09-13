using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Types
{
    public class ExternalShow : Observable
    {
        internal ExternalShow(ExternalSource _source, Show _show)
        {
            externalSource = _source;
            show = _show;

            if (externalSource != null) { externalSource.ExternalShows.Add(this); }
            if (show != null) { show.ExternalShows.Add(this); }
        }
        private long id;
        public long ID
        {
            get { return id; }
            set { VerifyPropertyChange(ref id, ref value); }
        }
        private string externalID;
        public string ExternalID
        {
            get { return externalID; }
            set { VerifyPropertyChange(ref externalID, ref value); }
        }
        private ExternalSource externalSource;
        public virtual ExternalSource ExternalSource
        {
            get { return externalSource; }
            set
            {
                ExternalSource oldSource = externalSource;
                if (VerifyPropertyChange(ref externalSource, ref value))
                {
                    if (oldSource != null) { oldSource.ExternalShows.Remove(this); }
                    if (externalSource != null) { externalSource.ExternalShows.Add(this); }
                }
            }
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
                    if (oldShow != null) { oldShow.ExternalShows.Remove(this); }
                    if (show != null) { show.ExternalShows.Add(this); }
                }
            }
        }
    }
}