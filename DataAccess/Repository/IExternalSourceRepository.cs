using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataAccess.Types;

namespace DataAccess.Repository
{
    public interface IExternalSourceRepository
    {
        ExternalSource GetSource(int id);
        ExternalSource GetSource(string title);
        ExternalSource CreateOrGetSource(string title, bool persist = true);
        void UpdateSource(ExternalSource updated, bool persist = true);
        void DeleteExternalSource(ExternalSource deleted, bool cascade = false);
    }
}