using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;

namespace DataAccess.Repository
{
    sealed class XMLConfigurationRepository : IConfigurationRepository
    {
        #region Attributes and Constructor
        private XMLRepositoryFactory factory;
        private XDocument document;
        private Dictionary<string, string> cache;
        internal XMLConfigurationRepository(XMLRepositoryFactory _factory)
        {
            factory = _factory;
            document = _factory.Document;
            cache = new Dictionary<string, string>();
        }
        #endregion
        #region Interface Overrides
        public string GetValue(string setting)
        {
            string value;
            //Check if the setting is stored
            if (cache.TryGetValue(setting, out value)) { return value; }
            //If not, find the correct element in the file and parse it.
            return Parse(FindElement(setting));
        }
        public void SetValue(string setting, string value, bool persist = true)
        {
            //Find the element
            XElement element = FindElement(setting);
            if (element == null)
            {
                //Setting isn't stored, so create it
                element = new XElement("Setting",
                    new XAttribute("Key", setting),
                    new XAttribute("Value", value));
                document.Root.Element("Config").Add(element);
            }
            else
            {
                //Update the element
                XAttribute _value = element.Attribute("Value");
                if (_value == null) { element.Add(new XAttribute("Key", value)); }
                else { _value.Value = value; }
                //Update dictionary
                cache[setting] = value;
            }
            if (persist) { factory.Persist(); }
        }
        public void ClearSetting(string Setting, bool persist = true)
        {
            //Find the element
            XElement element = FindElement(Setting);
            //Remove from document
            if (element != null) { element.Remove(); }
            //Remove from dictionary
            cache.Remove(Setting);
            if (persist) { factory.Persist(); }
        }
        #endregion
        #region XML Handling
        internal XElement FindElement(string key)
        {
            return (from XElement in document.Root.Element("Config").Elements()
                    where XElement.Attribute("Key").Value.Equals(key)
                    select XElement).FirstOrDefault();
        }
        internal string Parse(XElement element)
        {
            //Check input
            if (element == null) { return null; }
            //Setting attribute
            string setting = "";
            XAttribute _setting = element.Attribute("Key");
            if (_setting != null) { setting = _setting.Value; }
            //Value attribute
            string value = "";
            XAttribute _value = element.Attribute("Value");
            if (_value != null) { value = _value.Value; }
            //Store and return
            cache.Add(setting, value);
            return value;
        }
        #endregion
    }
}