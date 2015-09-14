using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccess.Repository
{
    public interface IConfigurationRepository
    {
        string GetValue(string setting);
        void SetValue(string setting, string value, bool persist = true);
        void ClearSetting(string setting, bool persist = true);
    }
}