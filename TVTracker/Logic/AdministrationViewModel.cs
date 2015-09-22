using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using DataAccess.Types;
using DataAccess.Repository;


namespace TVTracker.Logic
{
    class AdministrationViewModel
    {

        //TODO: Implement the observers to listen to newly added shows, seasons and episodes.

        private AbstractRepositoryFactory repository;

        #region Shows Collection
        /// <summary>
        /// The viewable collection of all shows.
        /// </summary>
        public ICollectionView Shows { get; private set; }
        /// <summary>
        /// The currently selected show
        /// </summary>
        public Show CurrentShow { get; set; }
        /// <summary>
        /// The collection containing the contents for Shows.
        /// </summary>
        private ObservableCollection<Show> showsContent;
        /// <summary>
        /// Fired when the current show is changed; updated the Seasons collection to reflect the currently selected show.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Shows_CurrentChanged(object sender, EventArgs e)
        {
            //Change the contents of the Seasons collection, episodes will be cleared out by the change of the seasons collection
            seasonsContent.Clear();
            Show _show = CurrentShow;

            if (_show != null)
            {
                //Console.WriteLine("Selected {0} with {1} Seasons", _show.Title, _show.Seasons.Count);
                foreach (Season _season in _show.Seasons)
                {
                    seasonsContent.Add(_season);
                }
            }
            //else { Console.WriteLine("Selected Nothing"); }
        }
        #endregion
        #region Seasons Collection
        /// <summary>
        /// The viewable collection of seasons for the selected show.
        /// </summary>
        public ICollectionView Seasons { get; private set; }
        /// <summary>
        /// The currently selected season.
        /// </summary>
        public Season CurrentSeason { get; set; }
        /// <summary>
        /// The collection containing the contents for Seasons.
        /// </summary>
        private ObservableCollection<Season> seasonsContent;
        /// <summary>
        /// Fired when the current season is changed; updated the Episodes collection to reflect the currently selected season.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Seasons_CurrentChanged(object sender, EventArgs e)
        {
            //Change the contents of the Episodes collection
            Season _season = CurrentSeason;
            episodesContent.Clear();
            if (_season != null)
            {
                //Console.WriteLine("Selected {0} with {1} Episodes", _season.SeasonNo, _season.Episodes.Count);
                foreach (Episode _episode in _season.Episodes)
                {
                    episodesContent.Add(_episode);
                }
            }
            //else { Console.WriteLine("Selected Nothing"); }
        }
        #endregion
        #region Episodes Collection

        /// <summary>
        /// The viewable collection of episodes for the selected season.
        /// </summary>
        public ICollectionView Episodes { get; private set; }
        /// <summary>
        /// The currently selected episode.
        /// </summary>
        public Episode CurrentEpisode { get; set; }
        /// <summary>
        /// The collection containing the contents for Episodes
        /// </summary>
        private ObservableCollection<Episode> episodesContent;
        /// <summary>
        /// Fired when the current episode is changed; does nothing for now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Episodes_CurrentChanged(object sender, EventArgs e)
        {
            //Nothing to do here for now
        }
        #endregion

        public AdministrationViewModel()
        {
            //TODO: Load this from a setting
            repository = XMLRepositoryFactory.Instance;

            //Initialize the contents of the three CollectionViews
            showsContent = new ObservableShowCollection();
            foreach (Show _show in repository.ShowRepository.GetAll())
            {
                showsContent.Add(_show);
            }
            seasonsContent = new ObservableSeasonCollection();
            episodesContent = new ObservableEpisodeCollection();

            //Create the CollectionViews for the three collections
            Shows = CollectionViewSource.GetDefaultView(showsContent);
            Seasons = CollectionViewSource.GetDefaultView(seasonsContent);
            Episodes = CollectionViewSource.GetDefaultView(episodesContent);

            //Subscribe to changes in selection
            Shows.CurrentChanged += Shows_CurrentChanged;
            Seasons.CurrentChanged += Seasons_CurrentChanged;
            Episodes.CurrentChanged += Episodes_CurrentChanged;
        }

    }
}