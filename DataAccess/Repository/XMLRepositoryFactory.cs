using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using DataAccess.Types;

namespace DataAccess.Repository
{
    public sealed class XMLRepositoryFactory : AbstractRepositoryFactory
    {
        #region Internal attributes
        internal XDocument Document { get; private set; }
        private string filename;
        #endregion
        #region Singleton
        //We apply the Singleton pattern to ensure that only one repository is in use.
        //TODO: Load the filename in a more dynamic way.
        private static readonly Lazy<XMLRepositoryFactory> instance = new Lazy<XMLRepositoryFactory>(() =>
            new XMLRepositoryFactory(@"D:\Projects\TVRepository.xml"));
        public static AbstractRepositoryFactory Instance
        {
            get { return instance.Value; }
        }
        private XMLRepositoryFactory(string _filename)
        {
            filename = _filename;
            try
            {
                Document = XDocument.Load(filename);
            }
            //System.IO.DirectoryNotFoundException
            catch (System.IO.FileNotFoundException ex)
            {
                //Repository doesn't exist, so create a new baseline.
                Document = new XDocument(
                    new XElement("Repository",
                        new XElement("Config"),
                        new XElement("Shows")
                    )
                );
                Persist();
            }
            configurationRepository = new XMLConfigurationRepository(this);
            //Initialize Show repository, with next ID from config
            string nextShowID = ConfigurationRepository.GetValue("NextShowID");
            if (nextShowID == null || nextShowID.Equals(""))
            {
                ConfigurationRepository.SetValue("NextShowID", "1");
                showRepository = new XMLShowRepository(this, 1);
            }
            else { showRepository = new XMLShowRepository(this, Convert.ToInt32(nextShowID)); }
            //Initialize Season repository, with next ID from config
            string nextSeasonID = ConfigurationRepository.GetValue("NextSeasonID");
            if (nextSeasonID == null || nextSeasonID.Equals(""))
            {
                ConfigurationRepository.SetValue("NextSeasonID", "1");
                seasonRepository = new XMLSeasonRepository(this, 1);
            }
            else { seasonRepository = new XMLSeasonRepository(this, Convert.ToInt32(nextSeasonID)); }
            //Initialize Season repository, with next ID from config
            string nextEpisodeID = ConfigurationRepository.GetValue("NextEpisodeID");
            if (nextEpisodeID == null || nextEpisodeID.Equals(""))
            {
                ConfigurationRepository.SetValue("NextEpisodeID", "1");
                episodeRepository = new XMLEpisodeRepository(this, 1);
            }
            else { episodeRepository = new XMLEpisodeRepository(this, Convert.ToInt32(nextEpisodeID)); }
        }
        #endregion
        #region Repository Delegates
        private IShowRepository showRepository;
        public override IShowRepository ShowRepository
        {
            get { return showRepository; }
        }
        private ISeasonRepository seasonRepository;
        public override ISeasonRepository SeasonRepository
        {
            get { return seasonRepository; }
        }
        private IEpisodeRepository episodeRepository;
        public override IEpisodeRepository EpisodeRepository
        {
            get { return episodeRepository; }
        }
        private IExternalSourceRepository externalSourceRepository;
        public override IExternalSourceRepository ExternalSourceRepository
        {
            get
            {
                if (externalSourceRepository == null) { externalSourceRepository = new XMLExternalSourceRepository(this); }
                return externalSourceRepository;
            }
        }
        private IExternalShowRepository externalShowRepository;
        public override IExternalShowRepository ExternalShowRepository
        {
            get
            {
                if (externalShowRepository == null) { externalShowRepository = new XMLExternalShowRepository(this); }
                return externalShowRepository;
            }
        }
        private XMLConfigurationRepository configurationRepository;
        public override IConfigurationRepository ConfigurationRepository
        {
            get { return configurationRepository; }
        }
        public override void Persist()
        {
            Document.Save(filename);
        }
        #endregion
    }
}