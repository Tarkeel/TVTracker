using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    sealed class XMLExternalSourceRepository : IExternalSourceRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<string, ExternalSource> cacheByTitle;
        private Dictionary<long, ExternalSource> cacheByID;
        private int nextID = 1;
        internal XMLExternalSourceRepository(XMLRepositoryFactory _factory)
        {
            factory = _factory;
            document = _factory.Document;
            cacheByTitle = new Dictionary<string, ExternalSource>();
            cacheByID = new Dictionary<long, ExternalSource>();
        }
        #endregion
        #region Interface Overrides
        public ExternalSource GetSource(int id)
        {
            //As there are no stored values for external sources, calls for ID will only check those sources that have been instanced 
            return cacheByID[id];
        }
        public ExternalSource GetSource(string title)
        {
            ExternalSource _source;
            //Check if we already have the item stored
            if (cacheByTitle.TryGetValue(title, out _source)) { return _source; }
            //If not, find the correct element(s) in the file and parse it.
            foreach(XElement element in FindElements(title)) { Parse(element); }
            //Retry the cache
            return cacheByTitle[title];
        }
        public ExternalSource CreateOrGetSource(string title, bool persist = true)
        {
            //Check if it's cached
            ExternalSource _ext;
            if (cacheByTitle.TryGetValue(title, out _ext)) { return _ext; }


            if (persist) { factory.Persist(); }
            return _ext;
        }
        public void UpdateSource(ExternalSource updated, bool persist = true)
        {
            //All the updating is done at the ExternalShow level.
        }
        public void DeleteExternalSource(ExternalSource deleted, bool cascade = false)
        {
            if (cascade)
            {
                foreach (ExternalShow _extShow in deleted.ExternalShows) { factory.ExternalShowRepository.DeleteExternalShow(_extShow); }
            }
            if (deleted.ExternalShows.Count == 0)
            {
                cacheByID.Remove(deleted.ID);
                cacheByTitle.Remove(deleted.Title);
            }
        }
        #endregion
        #region XML Handling
        internal ExternalSource Parse(XElement element)
        {
            throw new NotImplementedException();
        }


        internal IEnumerable<XElement> FindElements(string title)
        {
            return (from XElement in document.Descendants("External")
                    where XElement.Attribute("Source").Value.Equals(title)
                    select XElement);
        }
        #endregion
        private ExternalSource Create(string title)
        {
            //Create new
            ExternalSource _ext = new ExternalSource();
            _ext.Title = title;
            _ext.ID = nextID++;
            //Find all the ExternalShows for it
            IEnumerable<XElement> externals = (from XElement in document.Descendants("External")
                                               where XElement.Attribute("Source").Value.Equals(title)
                                               select XElement);
            foreach (XElement _element in externals)
            {
                ExternalShow _extShow = (factory.ExternalShowRepository as XMLExternalShowRepository).ParseXML(_element, _ext);
                _extShow.ExternalSource = _ext;
                _ext.ExternalShows.Add(_extShow);
            }
            //Add to cache
            cacheByTitle.Add(_ext.Title, _ext);
            cacheByID.Add(_ext.ID, _ext);

            return _ext;
        }
    }
}