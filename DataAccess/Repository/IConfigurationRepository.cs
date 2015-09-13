using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Repository
{
    public interface IConfigurationRepository
    {
        string GetValue(string Setting);
        void SetValue(string Setting, string Value, bool persist = true);
        void ClearSetting(string Setting, bool persist = true);
    }
}