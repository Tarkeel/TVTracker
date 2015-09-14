using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataAccess.Types;

namespace DataAccess.Repository
{
    public interface IShowRepository
    {
        IList<Show> GetAll();
        Show GetShow(int id);
        Show GetShow(string title);
        Show CreateOrGetShow(string title, bool persist = true);
        void UpdateShow(Show updated, bool persist = true);
        void DeleteShow(Show deleted, bool cascade = false);
    }
}
