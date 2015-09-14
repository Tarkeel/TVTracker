using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataAccess.Types;

namespace DataAccess.Repository
{
    public interface IExternalShowRepository
    {
        ExternalShow GetExternalShow(int id);
        ExternalShow GetExternalShow(Show show, ExternalSource source);
        ExternalShow GetExternalShow(ExternalSource source, string externalID);
        ExternalShow CreateOrGetExternalShow(Show show, ExternalSource source, bool persist = true);
        void UpdateExternalShow(ExternalShow updated, bool persist = true);
        void DeleteExternalShow(ExternalShow deleted, bool cascade = false);
    }
}