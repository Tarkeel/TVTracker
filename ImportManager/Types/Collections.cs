using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

using ImportManager.Adapters;

namespace ImportManager.Types
{
    public class EpisodeImportRowCollection : ObservableCollection<EpisodeImportRow> { }
    public class ObservableStringCollection : ObservableCollection<string> { }
    public class ImportAdapterCollection : ObservableCollection<AbstractImportAdapter> { }
}
