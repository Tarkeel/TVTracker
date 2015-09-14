using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    internal class XMLEpisodeRepository : IEpisodeRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<long, Episode> cacheByID;
        private Dictionary<string, Episode> cacheByComposite;
        private int nextID;
        internal XMLEpisodeRepository(XMLRepositoryFactory _factory, int _nextID)
        {
            factory = _factory;
            nextID = _nextID;
            document = _factory.Document;
            cacheByID = new Dictionary<long, Episode>();
            cacheByComposite = new Dictionary<string, Episode>();
        }
        #endregion
        #region Interface Overrides

        public IList<Episode> GetAll()
        {
            List<Episode> episodes = new List<Episode>();
            foreach (XElement element in document.Descendants("Episode"))
            {
                episodes.Add(Parse(element));
            }
            return episodes;
        }
        public Episode GetEpisode(int id)
        {
            Episode _episode;
            if (cacheByID.TryGetValue(id, out _episode)) { return _episode; }
            return Parse(FindElementByID(id));
        }
        public Episode GetEpisode(Season season, string episode)
        {
            Episode _episode;
            if (cacheByComposite.TryGetValue(BuildComposite(season, episode), out _episode)) { return _episode; }
            //The building of the show would have loaded any seasons and episodes for it, so if it wasn't found in the cache it didn't exist.
            return null;
        }
        public Episode CreateOrGetEpisode(Season season, string episode, bool persist = true)
        {
            //Check if we already have it
            Episode _episode = GetEpisode(season, episode);
            if (_episode == null)
            {
                //Create a new season
                _episode = create(season: season, episodeNo: episode);
                //Store as XML
                Pack(_episode);
                if (persist) { factory.Persist(); }
            }
            return _episode;
        }
        public void UpdateEpisode(Episode updated, bool persist = true)
        {
            Pack(updated);
            //Flush the file
            if (persist) { factory.Persist(); }
        }
        public void DeleteEpisode(Episode deleted, bool cascade = false)
        {
            //Updated references and cache
            deleted.Season.Episodes.Remove(deleted);
            cacheByComposite.Remove(BuildComposite(deleted.Season, deleted.EpisodeNo));
            cacheByID.Remove(deleted.ID);
            //Remove from XML and persist
            FindElementByID(deleted.ID).Remove();
            factory.Persist();
        }
        #endregion
        #region XML Handling
        internal void Pack(Episode _item)
        {
            XElement element = FindElementByID(_item.ID);
            if(element == null)
            {
                element = new XElement("Episode", new XAttribute("ID", _item.ID));
                (factory.SeasonRepository as XMLSeasonRepository).FindElement(_item.Season.ID).Add(element);
            }
            //EpisodeNo
            XAttribute _episodeNo = element.Attribute("EpisodeNo");
            if (_item.EpisodeNo != null && !_item.EpisodeNo.Equals(""))
            {
                if (_episodeNo != null) { _episodeNo.Value = _item.EpisodeNo; }
                else { element.Add(new XAttribute("EpisodeNo", _item.EpisodeNo)); }
            }
            else { if (_episodeNo != null) { _episodeNo.Remove(); } }
            //Title
            XAttribute _title = element.Attribute("Title");
            if (_item.Title != null && !_item.Title.Equals(""))
            {
                if (_title != null) { _title.Value = _item.Title; }
                else { element.Add(new XAttribute("Title", _item.Title)); }
            }
            else { if (_title != null) { _title.Remove(); } }
            //Code
            XAttribute _code = element.Attribute("Code");
            if (_item.Code != null && !_item.Code.Equals(""))
            {
                if (_code != null) { _code.Value = _item.Code; }
                else { element.Add(new XAttribute("Code", _item.Code)); }
            }
            else { if (_code != null) { _code.Remove(); } }
            //Airdate
            XAttribute _airdate = element.Attribute("Airdate");
            if (_item.Airdate != null && !_item.Airdate.Equals(""))
            {
                if (_airdate != null) { _airdate.Value = _item.Airdate; }
                else { element.Add(new XAttribute("Airdate", _item.Airdate)); }
            }
            else { if (_airdate != null) { _airdate.Remove(); } }
            //Rating
            XAttribute _rating = element.Attribute("Rating");
            if (_item.Rating >= 0)
            {
                if (_rating != null) { _rating.Value = Convert.ToString(_item.Rating); }
                else { element.Add(new XAttribute("Rating", Convert.ToString(_item.Rating))); }
            }
            else { if (_rating != null) { _rating.Remove(); } }
            //Watched
            XAttribute _watched = element.Attribute("Watched");
            if (_item.Watched)
            {
                if (_watched != null) { _watched.Value = "Yes"; }
                else { element.Add(new XAttribute("Watched", "Yes")); }
            }
            else { if (_watched != null) { _watched.Remove(); } }
        }
        internal Episode Parse(XElement element)
        {
            if (element == null) { return null; }
            Episode _episode;
            //Season
            Season _season = (factory.SeasonRepository as XMLSeasonRepository).Parse(element.Parent);
            //ID attribute
            XAttribute _id = element.Attribute("ID");
            if (_id == null)
            {
                //This really shouldn't be possible, but we can always fix it by setting a new ID.
                _episode = create(_season);
            }
            else
            {
                //Check if the ID is stored first
                if (cacheByID.TryGetValue(Convert.ToInt32(_id.Value), out _episode)) { return _episode; }
                _episode = create(_season, id: Convert.ToInt32(_id.Value));
            }
            //EpisodeNo attribute
            XAttribute _episodeNo = element.Attribute("EpisodeNo");
            if (_episodeNo != null) { _episode.EpisodeNo = _episodeNo.Value; }
            //Title attribute
            XAttribute _title = element.Attribute("Title");
            if (_title != null) { _episode.Title = _title.Value; }
            //Code attribute
            XAttribute _code = element.Attribute("Code");
            if (_code != null) { _episode.Code = _code.Value; }
            //Airdate attribute
            XAttribute _airdate = element.Attribute("Airdate");
            if (_airdate != null) { _episode.Airdate = _airdate.Value; }
            //Rating attribute
            XAttribute _rating = element.Attribute("Rating");
            if (_rating != null) { _episode.Rating = Convert.ToInt32(_rating.Value); }
            else { _episode.Rating = -1; }
            //Watched
            XAttribute _watched = element.Attribute("Watched");
            if (_watched != null && _watched.Value.Equals("Yes")) { _episode.Watched = true; }
            else { _episode.Watched = false; }

            return _episode;
        }
        internal XElement FindElementByID(long id)
        {
            return (from XElement in document.Descendants("Episode")
                    where XElement.Attribute("ID").Value.Equals(Convert.ToString(id))
                    select XElement).FirstOrDefault();
        }
        #endregion
        private string BuildComposite(Season season, string episode)
        {
            return string.Format("{0}-S{1}E{2}", season.Show.Title, season.SeasonNo, episode);
        }
        private Episode create(Season season, string episodeNo = null, long id = 0)
        {
            Episode _episode = new Episode(season);
            if (id > 0) { _episode.ID = id; }
            else
            {
                lock (this)
                {
                    //Create new based on next ID
                    _episode.ID = nextID;
                    nextID++;
                    factory.ConfigurationRepository.SetValue("NextEpisodeID", Convert.ToString(nextID), false);
                }
            }
            if (episodeNo != null)
            {
                //Store in cache
                _episode.EpisodeNo = episodeNo;
                cacheByComposite.Add(BuildComposite(season, episodeNo), _episode);
            }
            //Add to dictionaries and persist
            cacheByID.Add(_episode.ID, _episode);
            //Listen to changes of title and ID, to keep cache synced
            _episode.PropertyChanged += EpisodePropertyChanged;
            return _episode;
        }
        private void EpisodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Episode _episode = (Episode)sender;
            switch (e.PropertyName)
            {
                case "ID":
                    //ID has changed, we need to change the pointer in the ID cache
                    foreach (var item in cacheByID.Where(kvp => kvp.Value == _episode).ToList())
                    {
                        cacheByID.Remove(item.Key);
                    }
                    //Re-add with new ID
                    cacheByID.Add(_episode.ID, _episode);
                    break;
                case "EpisodeNo":
                case "Season":
                    //Season or Show has changed, we need to change the pointer in the composite cache
                    foreach (var item in cacheByComposite.Where(kvp => kvp.Value == _episode).ToList())
                    {
                        cacheByComposite.Remove(item.Key);
                    }
                    //Re-add with new title
                    cacheByComposite.Add(BuildComposite(_episode.Season, _episode.EpisodeNo), _episode);
                    break;
                default:
                    //Ignore all other changes
                    break;
            }
        }
    }
}
