using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public abstract class AbstractRepositoryFactory
    {
        public abstract IShowRepository ShowRepository { get; }
        public abstract ISeasonRepository SeasonRepository { get; }
        public abstract IEpisodeRepository EpisodeRepository { get; }
        public abstract IExternalSourceRepository ExternalSourceRepository { get; }
        public abstract IExternalShowRepository ExternalShowRepository { get; }
        public abstract IConfigurationRepository ConfigurationRepository { get; }
        public abstract void Persist();
    }
}