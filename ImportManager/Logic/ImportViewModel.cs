using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportManager.Types;

using DataAccess.Repository;
using System.Windows.Input;

namespace ImportManager.Logic
{
    /// <summary>
    /// This is the ViewModel controller for the Import window, and is normally loaded by MainWindow.xaml
    /// 
    /// It keeps track of the rows that are being processed.
    /// It will instantiate the import adapters conforming to AbstractImportAdapters which will call back to add sources to the rows.
    /// Finally it can loop through the rows, and import those that are flagged and ready for import.
    /// </summary>
    public class ImportViewModel
    {
        private AbstractRepositoryFactory repository;
        /// <summary>
        /// An observable collection of EpisodeImportRow for import processing.
        /// An import adapter is free to access and add to this directly, or use the AddRow method.
        /// </summary>
        public EpisodeImportRowCollection Rows { get; private set; }
        /// <summary>
        /// An observable collection of AbstractImportAdapters that can be invoked to load EpisodeImportRows from given sources.
        /// </summary>
        public ImportAdapterCollection ImportAdapters { get; private set; }

        #region Commands
        private ICommand importCommand;
        public ICommand ImportCommand
        #endregion
        {
            get { return importCommand; }
        }
        public void AddRow(string importSource, string showTitle, string seasonNo, string episodeNo, string title, string code, string rating, string airdate)
        {
            //TODO: Find a better way to enter the identifying attributes, and then the other attributes.
            //Get the Row for the identifier, if we already have it.
            EpisodeImportRow row = (from _row in Rows
                                    where _row.ShowTitle.Equals(showTitle) &&
                                        _row.EpisodeNo.Equals(episodeNo) &&
                                        _row.SeasonNo.Equals(seasonNo)
                                    select _row).FirstOrDefault();
            if (row == null)
            {
                //Create a new row, and add it to collection
                row = new EpisodeImportRow(repository, showTitle, seasonNo, episodeNo);
                Rows.Add(row);
            }
            //Delegate the attributes to the row itself.
            row.AddSource(importSource, title, code, rating, airdate);
        }
        public void Import()
        {
            Console.WriteLine("Import button clicked");
        }

        private void addTestData()
        {
            AddRow("TestSource1", "This is a test", "1", "1",
                "Just Testing", "101", "5", "2015-05-28");
            AddRow("TestSource2", "This is a test", "1", "1",
                "Just Testing More", "", "5", "2015-05-28");
            AddRow("TestSource1", "Veronica Mars", "1", "1",
                "Just Testing", "101", "5", "2015-05-28");
        }

        public ImportViewModel()
        {
            //Initialize collections and variables
            repository = XMLRepositoryFactory.Instance;
            Rows = new EpisodeImportRowCollection();
            ImportAdapters = new ImportAdapterCollection();
            //Initialize buttons
            importCommand = new ImportCommand(this);

            //Add adapters

            //Test: Add test data
            addTestData();
        }

    }
}
