using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace DataAccess.Types
{
    public class ObservableShowCollection : ObservableCollection<Show> { }
    public class ObservableSeasonCollection : ObservableCollection<Season> { }
    public class ObservableEpisodeCollection : ObservableCollection<Episode> { }
    public class ObservableExternalShowCollection : ObservableCollection<ExternalShow> { }
    public class ObservableExternalSourceCollection : ObservableCollection<ExternalSource> { }
}