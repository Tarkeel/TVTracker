using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Types
{
    /// <summary>
    /// A small class for holding choices for the user to select between.
    /// Title is meant to be displayed to the user.
    /// Description is a more in-depth description to help the user differentiate.
    /// Type determines what kind of of selection this is.
    /// ExternalID is the identifier for the choice in the external source.
    /// InternalID is the identifier for the choice locally, if available.
    /// </summary>
    public class SelectionChoice
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public SourceType Type { get; set; }
        public string ExternalID { get; set; }
        public long InternalID { get; set; }

        public enum SourceType { Show, Season, Episode }
    }
}