using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    sealed class XMLShowRepository : IShowRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<string, Show> cacheByTitle;
        private Dictionary<long, Show> cacheByID;
        private int nextID;
        internal XMLShowRepository(XMLRepositoryFactory _factory, int _nextID)
        {
            factory = _factory;
            nextID = _nextID;
            document = _factory.Document;
            cacheByTitle = new Dictionary<string, Show>();
            cacheByID = new Dictionary<long, Show>();
        }
        #endregion
        #region Interface Overrides
        public IList<Show> GetAll()
        {
            List<Show> _shows = new List<Show>();
            IEnumerable<XElement> elements = document.Root.Element("Shows").Elements("Show");
            foreach (XElement element in elements)
            {
                _shows.Add(Parse(element));
            }
            return _shows;
        }
        public Show GetShow(int id)
        {
            Show _item;
            //Check if we already have the item stored
            if (cacheByID.TryGetValue(id, out _item)) { return _item; }
            //If not, find the correct element in the file and parse it.
            return Parse(FindElement(id));
        }
        public Show GetShow(string title)
        {
            Show _item;
            //Check if we already have the show stored
            if (cacheByTitle.TryGetValue(title, out _item)) { return _item; }
            //If not, find the correct element in the file and parse it.
            return Parse(FindElement(title));
        }
        public Show CreateOrGetShow(string title, bool persist = true)
        {
            //See if we already have the show
            Show _show = GetShow(title);
            if (_show == null)
            {
                _show = create(title: title);
                //Put the show into the XML document
                Pack(_show);
                if (persist) { factory.Persist(); }
            }
            return _show;
        }
        public void UpdateShow(Show updated, bool persist = true)
        {
            //Pack the item into the XML document
            Pack(updated);
            //Flush the file if called for
            if (persist) { factory.Persist(); }
        }
        public void DeleteShow(Show deleted, bool cascade = false)
        {
            //Cascade to children if called for
            if (cascade)
            {
                foreach (Season season in deleted.Seasons) { factory.SeasonRepository.DeleteSeason(season, cascade); }
                foreach (ExternalShow ext in deleted.ExternalShows) { factory.ExternalShowRepository.DeleteExternalShow(ext, cascade); }
            }
            //If no children are remaining, delete
            if (deleted.ExternalShows.Count == 0 && deleted.Seasons.Count == 0)
            {
                //Updated references and cache
                cacheByTitle.Remove(deleted.Title);
                cacheByID.Remove(deleted.ID);
                //Remove from tree & persist
                FindElement(deleted.ID).Remove();
                factory.Persist();
            }
        }
        #endregion
        #region XML Handling
        internal void Pack(Show _item)
        {
            XElement element = FindElement(_item.ID);
            if (element == null)
            {
                element = new XElement("Show", new XAttribute("ID", _item.ID));
                document.Root.Element("Shows").Add(element);
            }
            //Title attribute
            XAttribute _title = element.Attribute("Title");
            if (_item.Title != null)
            {
                if (_title != null) { _title.Value = _item.Title; }
                else { element.Add(new XAttribute("Title", _item.Title)); }
            }
            else { if (_title != null) { _title.Remove(); } }
            //Year attribute
            XAttribute _year = element.Attribute("Year");
            if (_item.Year != null && _item.Year > 0)
            {
                if (_year != null) { _year.Value = Convert.ToString(_item.Year); }
                else { element.Add(new XAttribute("Year", _item.Year)); }
            }
            else { if (_year != null) { _year.Remove(); } }
        }
        internal Show Parse(XElement element)
        {
            if (element == null) { return null; }
            Show _show;
            //ID attribute
            XAttribute _id = element.Attribute("ID");
            if (_id == null)
            {
                //This shouldn't be possible, but we can always fix it by creating with a new ID
                _show = create();
            }
            else
            {
                //Check if the ID is stored first
                if (cacheByID.TryGetValue(Convert.ToInt32(_id.Value), out _show)) { return _show; }
                _show = create(Convert.ToInt32(_id.Value));
            }
            //Title attribute
            XAttribute _title = element.Attribute("Title");
            if (_title != null) { _show.Title = _title.Value; }
            //Year attribute
            XAttribute _year = element.Attribute("Year");
            if (_year != null) { _show.Year = Convert.ToInt32(_year.Value); }
            //Seasons
            IEnumerable<XElement> seasons = element.Elements("Season");
            foreach (XElement season in seasons)
            {
                //This will recursively add the seasons back to the show.
                (factory.SeasonRepository as XMLSeasonRepository).Parse(season);
            }
            return _show;
        }
        internal XElement FindElement(string title)
        {
            return (from XElement in document.Descendants("Show")
                    where XElement.Attribute("Title").Value.Equals(title)
                    select XElement).FirstOrDefault();
        }
        internal XElement FindElement(long id)
        {
            return (from XElement in document.Descendants("Show")
                    where XElement.Attribute("ID").Value.Equals(Convert.ToString(id))
                    select XElement).FirstOrDefault();
        }
        #endregion
        private Show create(long id = 0, string title = null)
        {
            Show _show = new Show();
            if (id > 0) { _show.ID = id; }
            else
            {
                //Create a new Show based on the next available ID. This needs to be thread safe.
                lock (this)
                {
                    _show.ID = nextID;
                    nextID++;
                    factory.ConfigurationRepository.SetValue("NextShowID", Convert.ToString(nextID), false);
                }
            }
            //Store in cache
            cacheByID.Add(_show.ID, _show);
            if (title != null)
            {
                _show.Title = title;
                cacheByTitle.Add(_show.Title, _show);
            }
            //Listen to changes of title and ID, to keep cache synced
            _show.PropertyChanged += ShowPropertyChanged;
            return _show;
        }
        private void ShowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Show _show = (Show)sender;
            switch (e.PropertyName)
            {
                case "ID":
                    //ID has changed, we need to change the pointer in the ID cache
                    foreach (var item in cacheByID.Where(kvp => kvp.Value == _show).ToList())
                    {
                        cacheByID.Remove(item.Key);
                    }
                    //Re-add with new ID
                    cacheByID.Add(_show.ID, _show);
                    break;
                case "Title":
                    //Title has changed, we need to change the pointer in the title cache
                    foreach (var item in cacheByTitle.Where(kvp => kvp.Value == _show).ToList())
                    {
                        cacheByTitle.Remove(item.Key);
                    }
                    //Re-add with new title
                    cacheByTitle.Add(_show.Title, _show);
                    break;
                default:
                    //Ignore all other changes
                    break;
            }
        }
    }
}