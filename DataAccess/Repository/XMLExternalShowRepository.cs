using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    sealed class XMLExternalShowRepository : IExternalShowRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<string, ExternalShow> externalsByComposite;
        private Dictionary<long, ExternalShow> externalsByID;
        private int nextID = 1;
        internal XMLExternalShowRepository(XMLRepositoryFactory _factory)
        {
            factory = _factory;
            document = _factory.Document;
            externalsByComposite = new Dictionary<string, ExternalShow>();
            externalsByID = new Dictionary<long, ExternalShow>();
        }
        #endregion
        #region Interface Overrides
        public ExternalShow GetExternalShow(int id)
        {
            ExternalShow _ext;
            if (externalsByID.TryGetValue(id, out _ext)) { return _ext; }
            return null;
        }
        public ExternalShow GetExternalShow(Show show, ExternalSource source)
        {
            ExternalShow _ext;
            if (externalsByComposite.TryGetValue(BuildComposite(show, source.Title), out _ext)) { return _ext; }
            return null;
        }
        public ExternalShow GetExternalShow(ExternalSource source, string externalID)
        {
            foreach (ExternalShow _show in source.ExternalShows)
            {
                if (_show.ExternalID.Equals(externalID)) { return _show; }
            }
            return null;
        }
        public ExternalShow CreateOrGetExternalShow(Show show, ExternalSource source, bool persist = true)
        {
            if (show == null || source == null)
            {
                //We can't really do this, so return null
                return null;
            }
            ExternalShow _ext;
            if (externalsByComposite.TryGetValue(BuildComposite(show, source.Title), out _ext)) { return _ext; }
            //Create a new as needed
            _ext = new ExternalShow(source, show);
            //ID
            _ext.ID = nextID++;
            //Store in cache
            externalsByComposite.Add(BuildComposite(_ext.Show, source.Title), _ext);
            externalsByID.Add(_ext.ID, _ext);
            //Add to document
            XElement element = new XElement("External",
                new XAttribute("Source", source.Title));
            (factory.ShowRepository as XMLShowRepository).FindElement(show.ID).Add(element);
            if (persist) { factory.Persist(); }
            return _ext;
        }
        public void UpdateExternalShow(ExternalShow updated, bool persist = true)
        {
            XElement _showElement = (factory.ShowRepository as XMLShowRepository).FindElement(updated.Show.ID);
            XElement element = (from XElement in _showElement.Descendants("External")
                                where XElement.Attribute("Source").Value.Equals(updated.ExternalSource.Title)
                                select XElement).FirstOrDefault();

            XAttribute _extID = element.Attribute("ID");
            if (updated.ExternalID != null)
            {
                if (_extID != null) { _extID.Value = updated.ExternalID; }
                else { element.Add(new XAttribute("ID", updated.ExternalID)); }
            }
            else
            {
                //TODO: Remove instead?
                if (_extID != null) { _extID.Remove(); }
            }
            if (persist) { factory.Persist(); }
        }
        public void DeleteExternalShow(ExternalShow deleted, bool cascade = false)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region XML Handling
        internal ExternalShow ParseXML(XElement element, ExternalSource externalSource)
        {
            ExternalShow _ext;
            Show _show = (factory.ShowRepository as XMLShowRepository).Parse(element.Parent);
            //element.Parent.Attribute("Title");
            //Check cache
            if (_show != null) { externalsByComposite.TryGetValue(BuildComposite(_show, externalSource.Title), out _ext); }
            //Create a new as needed
            _ext = new ExternalShow(externalSource, _show);
            //ID
            _ext.ID = nextID++;
            //Show
            _ext.Show = _show;
            _show.ExternalShows.Add(_ext);
            //Source
            _ext.ExternalSource = externalSource;

            //External ID
            XAttribute _extID = element.Attribute("ID");
            if (_extID != null) { _ext.ExternalID = _extID.Value; }
            //Store in cache
            externalsByComposite.Add(BuildComposite(_ext.Show, _ext.ExternalSource.Title), _ext);
            externalsByID.Add(_ext.ID, _ext);
            return _ext;
        }
        #endregion
        private string BuildComposite(Show show, string source)
        {
            return string.Format("{0}#{1}", show.Title, source);
        }
    }
}