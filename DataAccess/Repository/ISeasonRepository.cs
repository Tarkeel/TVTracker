using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataAccess.Types;

namespace DataAccess.Repository
{
    public interface ISeasonRepository
    {
        Season GetSeason(int id);
        Season GetSeason(Show show, int season);
        Season CreateOrGetSeason(Show show, int season, bool persist = true);
        void UpdateSeason(Season updated, bool persist = true);
        void DeleteSeason(Season deleted, bool cascade = false);
    }
}