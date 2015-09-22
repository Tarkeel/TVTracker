using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    internal class XMLSeasonRepository : ISeasonRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<long, Season> cacheByID;
        private Dictionary<string, Season> cacheByComposite;
        private int nextID;
        internal XMLSeasonRepository(XMLRepositoryFactory _factory, int _nextID)
        {
            factory = _factory;
            nextID = _nextID;
            document = _factory.Document;
            cacheByID = new Dictionary<long, Season>();
            cacheByComposite = new Dictionary<string, Season>();
        }
        #endregion
        #region Interface Overrides
        public Season GetSeason(int id)
        {
            Season _item;
            //Check if we already have the item stored
            if (cacheByID.TryGetValue(id, out _item)) { return _item; }
            //If not, find the correct element in the file and parse it.
            return Parse(FindElement(id));
        }
        public Season GetSeason(Show show, int seasonNo)
        {
            //See if the show contains the given season
            return (from Season season in show.Seasons
                    where season.SeasonNo == seasonNo
                    select season).FirstOrDefault();
            //The building of the show would have loaded any seasons for it, so if it wasn't found it didn't exist.
        }
        public Season CreateOrGetSeason(Show show, int season, bool persist = true)
        {
            //Check if we already have it
            Season _season = GetSeason(show, season);
            if (_season == null)
            {
                //Create a new season
                _season = create(show: show, seasonNo: season);
                //Store as XML
                Pack(_season);
                if (persist) { factory.Persist(); }
            }
            return _season;
        }
        public void UpdateSeason(Season updated, bool persist = true)
        {
            Pack(updated);
            //Flush the file
            if (persist) { factory.Persist(); }
        }
        public void DeleteSeason(Season deleted, bool cascade = false)
        {
            //Cascade to children if called for
            if (cascade)
            {
                foreach (Episode episode in deleted.Episodes) { factory.EpisodeRepository.DeleteEpisode(episode); }
            }
            //If no children are remaining, delete

            if (deleted.Episodes.Count == 0)
            {
                //Updated references and cache
                deleted.Show.Seasons.Remove(deleted);
                cacheByComposite.Remove(deleted.Composite);
                cacheByID.Remove(deleted.ID);
                //Remove from XML and persist
                FindElement(deleted.ID).Remove();
                factory.Persist();
            }
        }
        #endregion
        #region XML Handling
        internal void Pack(Season _item)
        {
            XElement element = FindElement(_item.ID);
            if (element == null)
            {
                element = new XElement("Season", new XAttribute("ID", _item.ID));
                (factory.ShowRepository as XMLShowRepository).FindElement(_item.Show.ID).Add(element);
            }
            //SeasonNo
            XAttribute _seasonNo = element.Attribute("SeasonNo");
            if (_item.SeasonNo >= 0)
            {
                if (_seasonNo != null) { _seasonNo.Value = Convert.ToString(_item.SeasonNo); }
                else { element.Add(new XAttribute("SeasonNo", _item.SeasonNo)); }
            }
            else { if (_seasonNo != null) { _seasonNo.Remove(); } }
            //Quality
            XAttribute _quality = element.Attribute("Quality");
            if (_item.Quality != null && !_item.Quality.Equals(""))
            {
                if (_quality != null) { _quality.Value = _item.Quality; }
                else { element.Add(new XAttribute("Quality", _item.Quality)); }
            }
            else { if (_quality != null) { _quality.Remove(); } }
        }
        internal Season Parse(XElement element)
        {
            if (element == null) { return null; }
            Season _season;
            //Show
            Show _show = (factory.ShowRepository as XMLShowRepository).Parse(element.Parent);
            //ID attribute
            XAttribute _id = element.Attribute("ID");
            if (_id == null)
            {
                //This really shouldn't be possible, but we can always fix it by setting a new ID.
                _season = create(_show);
            }
            else
            {
                //Check if the ID is stored first
                if (cacheByID.TryGetValue(Convert.ToInt32(_id.Value), out _season)) { return _season; }
                _season = create(_show, id: Convert.ToInt32(_id.Value));
            }
            //SeasonNo attribute
            XAttribute _seasonNo = element.Attribute("SeasonNo");
            if (_seasonNo != null) { _season.SeasonNo = Convert.ToInt32(_seasonNo.Value); }
            //Quality attribute
            XAttribute _quality = element.Attribute("Quality");
            if (_quality != null) { _season.Quality = _quality.Value; }
            //Episodes
            IEnumerable<XElement> episodes = element.Elements("Episode");
            foreach (XElement episode in episodes)
            {
                //This will recursively add the episodes back to the season
                (factory.EpisodeRepository as XMLEpisodeRepository).Parse(episode);
            }
            return _season;
        }
        internal XElement FindElement(long id)
        {
            return (from XElement in document.Descendants("Season")
                    where XElement.Attribute("ID").Value.Equals(Convert.ToString(id))
                    select XElement).FirstOrDefault();
        }
        #endregion
        private Season create(Show show, int seasonNo = 0, long id = 0)
        {
            Season _season = new Season(show);
            if (id > 0) { _season.ID = id; }
            else
            {
                lock (this)
                {
                    //Create new based on next ID
                    _season.ID = nextID;
                    nextID++;
                    factory.ConfigurationRepository.SetValue("NextSeasonID", Convert.ToString(nextID), false);
                }
            }
            if (seasonNo > 0)
            {
                //Store in cache
                _season.SeasonNo = seasonNo;
                cacheByComposite.Add(_season.Composite, _season);
            }
            //Add to dictionaries and persist
            cacheByID.Add(_season.ID, _season);
            //Listen to changes of title and ID, to keep cache synced
            _season.PropertyChanged += SeasonPropertyChanged;
            return _season;
        }
        private void SeasonPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Season _season = (Season)sender;
            switch (e.PropertyName)
            {
                case "ID":
                    //ID has changed, we need to change the pointer in the ID cache
                    foreach (var item in cacheByID.Where(kvp => kvp.Value == _season).ToList())
                    {
                        cacheByID.Remove(item.Key);
                    }
                    //Re-add with new ID
                    cacheByID.Add(_season.ID, _season);
                    break;
                case "Composite":
                    //Season or Show has changed, we need to change the pointer in the composite cache
                    foreach (var item in cacheByComposite.Where(kvp => kvp.Value == _season).ToList())
                    {
                        cacheByComposite.Remove(item.Key);
                    }
                    //Re-add with new title
                    cacheByComposite.Add(_season.Composite, _season);
                    break;
                default:
                    //Ignore all other changes
                    break;
            }
        }
    }
}
