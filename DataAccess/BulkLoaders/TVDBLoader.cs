using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Xml.Linq;
using DataAccess.Repository;
using DataAccess.Types;

namespace DataAccess.BulkLoaders
{
    public class TVDBLoader
    {
        private string mirrorPath = "http://thetvdb.com";
        private string apiKey = "A019E698F4D58953";

        private AbstractRepositoryFactory repositories;
        //The backgroundworker running the process
        System.ComponentModel.BackgroundWorker worker;
        System.ComponentModel.DoWorkEventArgs eventArgs;

        public TVDBLoader(AbstractRepositoryFactory _repositories,
            System.ComponentModel.BackgroundWorker _worker,
            System.ComponentModel.DoWorkEventArgs _eventArgs)
        {
            repositories = _repositories;
            worker = _worker;
            eventArgs = _eventArgs;
        }
        public List<SelectionChoice> searchByTitle(string search)
        {
            List<SelectionChoice> _rv = new List<SelectionChoice>();

            //Download the searchresults and load as XML document
            String url = String.Format("{0}/api/GetSeries.php?seriesname={1}",
                mirrorPath,
                search);
            XDocument results = XDocument.Load(url);

            //Parse each result into a SelectionChoice
            IEnumerable<XElement> elements = results.Root.Elements("Series");
            foreach (XElement source in elements)
            {
                _rv.Add(parseSeriesToChoice(source));
            }
            return _rv;
        }
        private SelectionChoice parseSeriesToChoice(XElement node)
        {
            //TODO: Let's not assume that these all exist and hold data, even though they should.
            SelectionChoice _choice = new SelectionChoice();
            _choice.Title = node.Element("SeriesName").Value;
            _choice.Description = node.Element("Overview").Value;
            _choice.ExternalID = node.Element("seriesid").Value;
            _choice.Type = SelectionChoice.SourceType.Show;
            return _choice;
        }
        public void LoadShow(string externalID, bool forceOverwrite = false)
        {
            //Download the XML file for the show
            String url = String.Format("{0}/api/{1}/series/{2}/all/",
                mirrorPath,
                apiKey,
                externalID);

            XDocument results = XDocument.Load(url);
            //Determine the local show matching the ID
            Show currentShow = null;
            //First we try the ExternalShow
            ExternalSource _source = repositories.ExternalSourceRepository.CreateOrGetSource("thetvdb");
            ExternalShow _ext = repositories.ExternalShowRepository.GetExternalShow(_source, externalID);
            if (_ext != null)
            {
                currentShow = _ext.Show;
            }

            //Parse base show data
            IEnumerable<XElement> series = results.Root.Elements("Series");
            foreach (XElement show in series)
            {
                if (currentShow == null)
                {
                    //If we didn't find the show earlier, we need to create one.
                    currentShow = repositories.ShowRepository.CreateOrGetShow(show.Element("SeriesName").Value, false);
                    //And store it for later use
                    _ext = repositories.ExternalShowRepository.CreateOrGetExternalShow(currentShow, _source, false);
                    _ext.ExternalID = externalID;
                    repositories.ExternalShowRepository.UpdateExternalShow(_ext, false);
                }
                //Year
                string _year = show.Element("FirstAired").Value;
                if (_year != null && currentShow.Year >= 0)
                {
                    currentShow.Year = Convert.ToInt16(_year.Substring(0, 4));
                    repositories.ShowRepository.UpdateShow(currentShow, false);
                }
                //IMDB_ID: TODO

            }

            //Parse episodes data
            Season currentSeason = null;
            Episode currentEpisode = null;

            IEnumerable<XElement> episodes = results.Root.Elements("Episode");
            foreach (XElement episode in episodes)
            {
                Boolean updated = false;
                //Season
                int _season = Convert.ToInt16(episode.Element("SeasonNumber").Value);
                //Only import "Real" seasons, above 0
                if (_season > 0)
                {
                    currentSeason = repositories.SeasonRepository.CreateOrGetSeason(currentShow, _season, false);
                }
                //Episode
                string _episode = episode.Element("EpisodeNumber").Value;
                if (currentSeason != null && _episode != null && _episode != "")
                {
                    currentEpisode = repositories.EpisodeRepository.CreateOrGetEpisode(currentSeason, _episode, false);
                }
                Console.WriteLine("S{0}E{1}", _season, _episode);
                //Code
                string _code = episode.Element("ProductionCode").Value;
                if (currentEpisode != null && _code != null && _code != "")
                {
                    if (forceOverwrite || currentEpisode.Code == null || currentEpisode.Code.Equals(""))
                    {
                        //Only overwrite blank values
                        int _length = _code.Length;
                        if (_length > 5) { _length = 5; }
                        _code = _code.Substring(0, _length);
                        currentEpisode.Code = _code;
                        updated = true;
                    }
                    else
                    {
                        //TODO: Update this for user confirmation
                        if (!currentEpisode.Code.Equals(_code)) { Console.Write(" Code {0} blocked by existing value {1};", _code, currentEpisode.Code); }
                    }
                }
                //Title
                string _title = episode.Element("EpisodeName").Value;
                if (currentEpisode != null && _title != null && _title != "")
                {
                    if (forceOverwrite || currentEpisode.Title == null || currentEpisode.Title.Equals(""))
                    {
                        //Only overwrite blank values
                        int _length = _title.Length;
                        if (_length > 75) { _length = 75; }
                        _title = _title.Substring(0, _length);
                        currentEpisode.Title = _title;
                        updated = true;
                    }
                    else
                    {
                        //TODO: Update this for user confirmation
                        if (!currentEpisode.Title.Equals(_title)) { Console.Write(" Title {0} blocked by existing value {1};", _title, currentEpisode.Title); }
                    }
                }
                //Airdate
                string _airdate = episode.Element("FirstAired").Value;
                if (currentEpisode != null && _airdate != null && _airdate != "")
                {
                    if (forceOverwrite || currentEpisode.Airdate == null || currentEpisode.Airdate.Equals(""))
                    {
                        //Only overwrite blank values
                        currentEpisode.Airdate = _airdate;
                        updated = true;
                    }
                    else
                    {
                        //TODO: Update this for user confirmation
                        if (!currentEpisode.Airdate.Equals(_airdate)) { Console.Write(" Airdate {0} blocked by existing value {1};", _airdate, currentEpisode.Airdate); }
                    }
                }
                //Update as needed
                if (updated)
                {
                    repositories.EpisodeRepository.UpdateEpisode(currentEpisode, false);
                }
                //Update Progress: TODO
                //worker.ReportProgress();
                //                repositories.Persist();
            }
            repositories.Persist();
        }

    }
}
