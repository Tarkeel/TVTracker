using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataAccess.Types;

namespace DataAccess.Repository
{
    public interface IEpisodeRepository
    {
        IList<Episode> GetAll();
        Episode GetEpisode(int id);
        Episode GetEpisode(Season season, string episode);
        Episode CreateOrGetEpisode(Season season, string episode, bool persist = true);
        void UpdateEpisode(Episode updated, bool persist = true);
        void DeleteEpisode(Episode deleted, bool cascade = false);
    }
}