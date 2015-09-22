using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.OleDb;
using DataAccess.Repository;
using DataAccess.Types;

namespace DataAccess.BulkLoaders
{
    public class XLSLoader
    {
        string filename;
        AbstractRepositoryFactory factory;
        //The backgroundworker running the process
        System.ComponentModel.BackgroundWorker worker;
        System.ComponentModel.DoWorkEventArgs eventArgs;

        public XLSLoader(string _filename,
            AbstractRepositoryFactory _factory,
            System.ComponentModel.BackgroundWorker _worker,
            System.ComponentModel.DoWorkEventArgs _eventArgs)
        {
            factory = _factory;
            filename = _filename;
            worker = _worker;
            eventArgs = _eventArgs;
        }
        public void Load()
        {
            DataSet episodes = parseFile();
            importEpisodes(episodes.Tables["Episodes"]);
            factory.Persist();
        }
        private DataSet parseFile()
        {
            DataSet result = new DataSet();
            //Open the file
            string provider = "Microsoft.Jet.OLEDB.4.0";
            string properties = "'Excel 8.0;HDR=Yes;'";
            string connectionString = string.Format(@"Provider={0};Data Source={1};Extended Properties={2}",
                provider,
                filename,
                properties);
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                connection.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = connection;
                //Parse file and build resultset

                //Episodes:
                cmd.CommandText = string.Format("SELECT * FROM [{0}]", "Episodes$");
                DataTable table = new DataTable("Episodes");
                new OleDbDataAdapter(cmd).Fill(table);
                result.Tables.Add(table);

                //Close out
                cmd = null;
                connection.Close();
                return result;
            }
            catch (OleDbException e)
            {
                Console.WriteLine(e);
                return result;
            }

        }
        private void importEpisodes(DataTable source)
        {

            int colShow = -1;
            int colSeason = -1;
            int colEpisode = -1;
            int colCode = -1;
            int colTitle = -1;
            int colRating = -1;
            #region Map column headers
            //Determine what the column maps to
            int counter = 0;
            foreach (DataColumn col in source.Columns)
            {
                switch (col.ColumnName)
                {
                    case "Series":
                    case "Serie":
                    case "Show":
                        colShow = counter;
                        break;
                    case "S":
                    case "Season":
                        colSeason = counter;
                        break;
                    case "E":
                    case "Episode":
                        colEpisode = counter;
                        break;
                    case "Code":
                        colCode = counter;
                        break;
                    case "Name":
                        colTitle = counter;
                        break;
                    case "Rating":
                        colRating = counter;
                        break;
                    default:
                        //TODO: Move this to logging
                        Console.WriteLine("Unhandled column: {0}", col.ColumnName);
                        break;
                }
                counter++;
            }
            #endregion
            #region Process and import data
            int totalRows = source.Rows.Count;
            int rowCount = 0;
            if (colShow != -1)
            {
                foreach (DataRow row in source.Rows)
                {
                    if (worker.CancellationPending)
                    {
                        eventArgs.Cancel = true;
                        break;
                    }
                    Show show = null;
                    Season season = null;
                    Episode episode = null;
                    bool updated = false;
                    //Show
                    if (colShow != -1
                        && !DBNull.Value.Equals(row[colShow]))
                    {
                        show = factory.ShowRepository.CreateOrGetShow(Convert.ToString(row[colShow]), false);
                        Console.Write("{0}: ", show.Title);
                    }
                    //Season
                    if (show != null
                        && colSeason != -1
                        && !DBNull.Value.Equals(row[colSeason]))
                    {
                        season = factory.SeasonRepository.CreateOrGetSeason(show, Convert.ToInt16(row[colSeason]), false);
                        Console.Write("S{0}", season.SeasonNo);
                    }
                    //Episode
                    if (season != null
                        && colEpisode != -1
                        && !DBNull.Value.Equals(row[colEpisode]))
                    {
                        episode = factory.EpisodeRepository.CreateOrGetEpisode(season, Convert.ToString(row[colEpisode]), false);
                        Console.Write("E{0}", episode.EpisodeNo);
                    }
                    //Episode Code
                    if (episode != null
                        && colCode != -1
                        && !DBNull.Value.Equals(row[colCode]))
                    {
                        string _code = Convert.ToString(row[colCode]);
                        if (episode.Code == null || episode.Code.Equals(""))
                        {
                            //Only overwrite blank values
                            episode.Code = _code;
                            updated = true;
                        }
                        else
                        {
                            if (!episode.Code.Equals(_code)) { Console.Write(" Code {0} blocked by existing value {1};", _code, episode.Code); }
                        }
                    }
                    //Episode Title
                    if (episode != null
                        && colTitle != -1
                        && !DBNull.Value.Equals(row[colTitle]))
                    {
                        string _title = Convert.ToString(row[colTitle]);
                        if (episode.Title == null || episode.Title.Equals(""))
                        {
                            //Only overwrite blank values
                            episode.Title = _title;
                            updated = true;
                        }
                        else
                        {
                            if (!episode.Title.Equals(_title)) { Console.Write(" Title {0} blocked by existing value {1};", _title, episode.Title); }
                        }
                    }
                    //Episode Rating
                    if (episode != null
                        && colRating != -1
                        && !DBNull.Value.Equals(row[colRating]))
                    {
                        int _rating = Convert.ToInt16(row[colRating]);
                        if (episode.Rating == -1)
                        {
                            //Only overwrite blank values
                            episode.Rating = _rating;
                            updated = true;
                        }
                        else
                        {
                            if (!episode.Rating.Equals(_rating)) { Console.Write(" Rating {0} blocked by existing value {1};", _rating, episode.Rating); }
                        }
                    }
                    //Save if needed
                    if (updated)
                    {
                        factory.EpisodeRepository.UpdateEpisode(episode, false);
                    }
                    Console.WriteLine();
                    //Report progress
                    rowCount++;
                    if (worker.WorkerReportsProgress) { worker.ReportProgress(rowCount * 100 / totalRows, String.Format("{0} of {1} rows imported.", rowCount, totalRows)); }
                }
            }
            #endregion
        }
    }
}
