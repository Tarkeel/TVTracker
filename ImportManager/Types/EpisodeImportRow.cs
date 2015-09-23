using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows.Data;

using DataAccess.Types;
using DataAccess.Repository;

namespace ImportManager.Types
{
    /// <summary>
    /// Represents a "row of data" that the Import Manager should display and let the user select which values to keep.
    /// It exposes the following values:
    ///  * CompositeIdentifier - A Composite of all the attributes that uniquely identify the import row.
    ///  * ImportSources - A collection containing the names of the sources, organized in the order of the values.
    ///  * Titles/Codes/Ratings/Airdates - Collections representing the attributes of episodes.
    /// The following are important internal variables:
    /// 
    /// </summary>
    public class EpisodeImportRow : Observable
    {
        public enum ImportStatus
        {
            Initial,
            NonConflicting,
            Conflicting,
            ManuallySelected
        };
        private readonly string _empty = "[blank]";
        AbstractRepositoryFactory storage;
        private bool expanded;
        public bool Expanded
        {
            get { return expanded; }
            set { VerifyPropertyChange(ref expanded, ref value); }
        }

        #region Identifying Attributes
        private string showTitle;
        public string ShowTitle
        {
            get { return showTitle; }
            set { if (VerifyPropertyChange(ref showTitle, ref value)) { FirePropertyChanged("CompositeIdentifier"); } }
        }
        private string seasonNo;
        public string SeasonNo
        {
            get { return seasonNo; }
            set { if (VerifyPropertyChange(ref seasonNo, ref value)) { FirePropertyChanged("CompositeIdentifier"); } }
        }
        private string episodeNo;
        public string EpisodeNo
        {
            get { return episodeNo; }
            set { if (VerifyPropertyChange(ref episodeNo, ref value)) { FirePropertyChanged("CompositeIdentifier"); } }
        }
        public string CompositeIdentifier
        {
            get { return String.Format("{0} - S{1}E{2}", ShowTitle, SeasonNo, EpisodeNo); }
        }
        #endregion
        #region Source Collection
        //TODO: Future functionality, make it so that selecting a source also selects all elements from that source.
        public ObservableStringCollection ImportSources { get; private set; }
        private ImportStatus rowStatus;
        public ImportStatus RowStatus
        {
            get { return rowStatus; }
            private set { VerifyPropertyChange(ref rowStatus, ref value); }
        }
        #endregion
        #region Title Collection
        public ICollectionView ImportTitles { get; private set; }
        private ObservableStringCollection importTitlesContent;
        private string selectedTitle;
        public string SelectedTitle
        {
            get { return selectedTitle; }
            //Selecting the title will change TitleStatus to ManuallySelected, which will force the RowStatus to be recalculated too
            set { if (VerifyPropertyChange(ref selectedTitle, ref value)) { TitleStatus = ImportStatus.ManuallySelected; } }
        }
        private ImportStatus titleStatus;
        public ImportStatus TitleStatus
        {
            get { return titleStatus; }
            set { if (VerifyPropertyChange(ref titleStatus, ref value)) { recalculateRowStatus(); } }
        }
        public void ImportTitles_CurrentChanged(object sender, EventArgs e)
        {
            //Changing the SelectedTitle will force the TitleStatus to be updated, which forces a recalculation of the row status
            SelectedTitle = ImportTitles.CurrentItem as string;
        }
        #endregion
        #region Code Collection
        public ICollectionView ImportCodes { get; private set; }
        private ObservableStringCollection importCodesContent;
        private string selectedCode;
        public string SelectedCode
        {
            get { return selectedCode; }
            //Selecting the code will change CodeStatus to ManuallySelected, which will force the RowStatus to be recalculated too
            set { if (VerifyPropertyChange(ref selectedCode, ref value)) { CodeStatus = ImportStatus.ManuallySelected; } }
        }
        private ImportStatus codeStatus;
        public ImportStatus CodeStatus
        {
            get { return codeStatus; }
            set { if (VerifyPropertyChange(ref codeStatus, ref value)) { recalculateRowStatus(); } }
        }
        void ImportCodes_CurrentChanged(object sender, EventArgs e)
        {
            //Changing the SelectedCode will force the CodeStatus to be updated, which forces a recalculation of the row status
            SelectedCode = ImportCodes.CurrentItem as string;
        }
        #endregion
        #region Rating Collection
        public ICollectionView ImportRatings { get; private set; }
        private ObservableStringCollection importRatingsContent;
        private string selectedRating;
        public string SelectedRating
        {
            get { return selectedRating; }
            //Selecting the rating will change RatingStatus to ManuallySelected, which will force the RowStatus to be recalculated too
            set { if (VerifyPropertyChange(ref selectedRating, ref value)) { RatingStatus = ImportStatus.ManuallySelected; } }
        }
        private ImportStatus ratingStatus;
        public ImportStatus RatingStatus
        {
            get { return ratingStatus; }
            set { if (VerifyPropertyChange(ref ratingStatus, ref value)) { recalculateRowStatus(); } }
        }
        void ImportRatings_CurrentChanged(object sender, EventArgs e)
        {
            //Changing the SelectedRating will force the RatingStatus to be updated, which forces a recalculation of the row status
            SelectedRating = ImportRatings.CurrentItem as string;
        }
        #endregion
        #region Airdate Collection
        public ICollectionView ImportAirdates { get; private set; }
        private ObservableStringCollection importAirdatesContent;
        private string selectedAirdate;
        public string SelectedAirdate
        {
            get { return selectedAirdate; }
            //Selecting the airdate will change AirdateStatus to ManuallySelected, which will force the RowStatus to be recalculated too
            set { if (VerifyPropertyChange(ref selectedAirdate, ref value)) { AirdateStatus = ImportStatus.ManuallySelected; } }
        }
        private ImportStatus airdateStatus;
        public ImportStatus AirdateStatus
        {
            get { return airdateStatus; }
            set { if (VerifyPropertyChange(ref airdateStatus, ref value)) { recalculateRowStatus(); } }
        }
        void ImportAirdates_CurrentChanged(object sender, EventArgs e)
        {
            //Changing the SelectedAirdate will force the AirdateStatus to be updated, which forces a recalculation of the row status
            SelectedAirdate = ImportAirdates.CurrentItem as string;
        }
        #endregion

        public EpisodeImportRow(AbstractRepositoryFactory _baseRepository, string _showTitle, string _seasonNo, string _episodeNo)
        {
            //Store a reference to the repository that we will load the comparison from and store the result to.
            storage = _baseRepository;

            //Store the identifiers
            showTitle = _showTitle;
            seasonNo = _seasonNo;
            episodeNo = _episodeNo;

            //Initialize Collections with content and listen to changes in selection.
            ImportSources = new ObservableStringCollection();

            importTitlesContent = new ObservableStringCollection();
            ImportTitles = CollectionViewSource.GetDefaultView(importTitlesContent);
            ImportTitles.CurrentChanged += ImportTitles_CurrentChanged;

            importCodesContent = new ObservableStringCollection();
            ImportCodes = CollectionViewSource.GetDefaultView(importCodesContent);
            ImportCodes.CurrentChanged += ImportCodes_CurrentChanged;

            importRatingsContent = new ObservableStringCollection();
            ImportRatings = CollectionViewSource.GetDefaultView(importRatingsContent);
            ImportRatings.CurrentChanged += ImportRatings_CurrentChanged;

            importAirdatesContent = new ObservableStringCollection();
            ImportAirdates = CollectionViewSource.GetDefaultView(importAirdatesContent);
            ImportAirdates.CurrentChanged += ImportAirdates_CurrentChanged;

            //Initialize status for fields
            rowStatus = ImportStatus.Initial;
            titleStatus = ImportStatus.Initial;
            codeStatus = ImportStatus.Initial;
            ratingStatus = ImportStatus.Initial;
            airdateStatus = ImportStatus.Initial;

            //Load the comparison from the database
            string _source = "Existing";
            string _title = _empty;
            string _code = _empty;
            string _rating = _empty;
            string _airdate = _empty;

            Show _show;
            Season _season = null;
            Episode _episode = null;

            _show = storage.ShowRepository.GetShow(showTitle);
            if (_show != null) { _season = storage.SeasonRepository.GetSeason(_show, Convert.ToInt16(seasonNo)); }
            if (_season != null) { _episode = storage.EpisodeRepository.GetEpisode(_season, episodeNo); }
            if (_episode != null)
            {
                if (_episode.Title != null && _episode.Title != "") { _title = _episode.Title; }
                if (_episode.Code != null && _episode.Code != "") { _code = _episode.Code; }
                if (_episode.Rating >= 0) { _rating = Convert.ToString(_episode.Rating); }
                if (_episode.Airdate != null && _episode.Airdate != "") { _airdate = _episode.Airdate; }
            }
            //Add the existing stored values
            AddSource(_source, _title, _code, _rating, _airdate);
        }
        public bool AddSource(string _source, string _title, string _code, string _rating, string _airdate)
        {
            if (ImportSources.Contains(_source))
            {
                //We already have this source, so block it.
                //TODO: In the future, we might want to refresh the existing values instead.
                return false;
            }
            //Add the values to all the collections.
            //TODO: Should we maybe reset status when adding a non-empty new item?
            ImportSources.Add(_source);
            if (_title == null || _title == "") { importTitlesContent.Add(_empty); }
            else { importTitlesContent.Add(_title); }
            if (_code == null || _code == "") { importCodesContent.Add(_empty); }
            else { importCodesContent.Add(_code); }
            if (_rating == null || _rating == "") { importRatingsContent.Add(_empty); }
            else { importRatingsContent.Add(_rating); }
            if (_airdate == null || _airdate == "") { importAirdatesContent.Add(_empty); }
            else { importAirdatesContent.Add(_airdate); }

            //Recalculate status
            recalculateStatus();
            return true;
        }
        public bool PersistRow(bool batchMode, bool partialImport = false)
        {
            //For a partial import, we can import any attribute that is ready. If not, we only import when the entire row is ready to be persisted.
            bool rowReady = (rowStatus == ImportStatus.NonConflicting || rowStatus == ImportStatus.ManuallySelected);
            //Find/Create episode etc
            Show _show = storage.ShowRepository.CreateOrGetShow(showTitle, false);
            Season _season = storage.SeasonRepository.CreateOrGetSeason(_show, Convert.ToInt16(seasonNo), false);
            Episode _episode = storage.EpisodeRepository.CreateOrGetEpisode(_season, episodeNo, false);

            //Title
            bool titleReady = (titleStatus == ImportStatus.NonConflicting || titleStatus == ImportStatus.ManuallySelected);
            if ((rowReady || partialImport) && titleReady)
            {
                if (SelectedTitle == null || SelectedTitle.Equals("") || SelectedTitle.Equals(_empty)) { _episode.Title = null; }
                else { _episode.Title = SelectedTitle; }
            }
            //Code
            bool codeReady = (codeStatus == ImportStatus.NonConflicting || codeStatus == ImportStatus.ManuallySelected);
            if ((rowReady || partialImport) && codeReady)
            {
                if (SelectedCode == null || SelectedCode.Equals("") || SelectedCode.Equals(_empty)) { _episode.Code = null; }
                else { _episode.Code = SelectedCode; }
            }
            //Airdate
            bool airdateReady = (airdateStatus == ImportStatus.NonConflicting || airdateStatus == ImportStatus.ManuallySelected);
            if ((rowReady || partialImport) && airdateReady)
            {
                if (SelectedAirdate == null || SelectedAirdate.Equals("") || SelectedAirdate.Equals(_empty)) { _episode.Airdate = null; }
                else { _episode.Airdate = SelectedAirdate; }
            }
            //Rating
            bool ratingReady = (ratingStatus == ImportStatus.NonConflicting || ratingStatus == ImportStatus.ManuallySelected);
            if ((rowReady || partialImport) && ratingReady)
            {
                if (SelectedRating == null || SelectedRating.Equals("") || SelectedRating.Equals(_empty)) { _episode.Rating = -1; }
                else { _episode.Rating = Convert.ToInt32(SelectedRating); }
            }

            //Persist unless we are in batch-mode
            storage.EpisodeRepository.UpdateEpisode(_episode, !batchMode);
            //Return true if all parts were updated
            return rowReady;
        }
        private void recalculateStatus()
        {
            #region Recalculate Title
            switch (titleStatus)
            {
                case ImportStatus.ManuallySelected: //The field value has been manually selected by the user, and should not be recalculated.
                case ImportStatus.Conflicting: //Since values are already conflicting, adding a new value is not going to make a difference.
                    break; //Do nothing for these two cases
                case ImportStatus.NonConflicting: //All the values are either blank or the same. Loop through and check
                    string _selected = ""; //This one keeps track of the "selected" value, and will be used to set the actual selected value at the end.
                    bool _conflicting = false;
                    foreach (string _current in ImportTitles)
                    {
                        if (_current.Equals(_empty))
                        {
                            //We only care about an empty value if there is no current "selected" value; we then set this blank as current, and keeps the status as non-conflicting
                            if (_selected.Equals("")) { _selected = _current; }
                            //We implicitly any other values in current, as it's not pertinent.
                        }
                        else
                        {
                            //If the "selected" value is undetermined, we can just use the current instead. Status remains the same
                            if (_selected.Equals(_empty) || _selected.Equals("")) { _selected = _current; }
                            //If the "selected" value is different to the current value, we have conflicting values.
                            if (!_selected.Equals(_current)) { _conflicting = true; break; }
                            //We implicitly skip the case where the "selected" value is the same as the current value, so as to stay with the first occurence of the value.
                        }
                    }
                    //Finally update the selected item depending on if we have a conflict or not.
                    if (_conflicting)
                    {
                        ImportTitles.MoveCurrentToPosition(0);//Reset selection to database value
                        TitleStatus = ImportStatus.Conflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    }
                    else
                    {
                        ImportTitles.MoveCurrentTo(_selected);
                        TitleStatus = ImportStatus.NonConflicting;//Changing the current selection reset this.
                    }
                    break;
                case ImportStatus.Initial: //The status for the field has not yet been calculated; this would imply that there is only one value.
                    //We cheat and set it the back way, and manually fire the change, to avoid triggering this same recalculation.
                    string newSelectedTitle = importTitlesContent.First();
                    VerifyPropertyChange(ref selectedTitle, ref newSelectedTitle);
                    //ImportTitles.MoveCurrentToPosition(0);
                    TitleStatus = ImportStatus.NonConflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    break;
            }
            #endregion
            #region Recalculate Code
            switch (codeStatus)
            {
                case ImportStatus.ManuallySelected: //The field value has been manually selected by the user, and should not be recalculated.
                case ImportStatus.Conflicting: //Since values are already conflicting, adding a new value is not going to make a difference.
                    break; //Do nothing for these two cases
                case ImportStatus.NonConflicting: //All the values are either blank or the same. Loop through and check
                    string _selected = ""; //This one keeps track of the "selected" value, and will be used to set the actual selected value at the end.
                    bool _conflicting = false;
                    foreach (string _current in ImportCodes)
                    {
                        if (_current.Equals(_empty))
                        {
                            //We only care about an empty value if there is no current "selected" value; we then set this blank as current, and keeps the status as non-conflicting
                            if (_selected.Equals("")) { _selected = _current; }
                            //We implicitly any other values in current, as it's not pertinent.
                        }
                        else
                        {
                            //If the "selected" value is undetermined, we can just use the current instead. Status remains the same
                            if (_selected.Equals(_empty) || _selected.Equals("")) { _selected = _current; }
                            //If the "selected" value is different to the current value, we have conflicting values.
                            if (!_selected.Equals(_current)) { _conflicting = true; break; }
                            //We implicitly skip the case where the "selected" value is the same as the current value, so as to stay with the first occurence of the value.
                        }
                    }
                    //Finally update the selected item depending on if we have a conflict or not.
                    if (_conflicting)
                    {
                        ImportCodes.MoveCurrentToPosition(0);//Reset selection to database value
                        CodeStatus = ImportStatus.Conflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    }
                    else
                    {
                        ImportCodes.MoveCurrentTo(_selected);
                        CodeStatus = ImportStatus.NonConflicting;//Changing the current selection reset this.
                    }
                    break;
                case ImportStatus.Initial: //The status for the field has not yet been calculated; this would imply that there is only one value.
                    //We cheat and set it the back way, and manually fire the change, to avoid triggering this same recalculation.
                    string newSelectedCode = importTitlesContent.First();
                    VerifyPropertyChange(ref selectedCode, ref newSelectedCode);
                    CodeStatus = ImportStatus.NonConflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    break;
            }
            #endregion
            #region Rating
            switch (ratingStatus)
            {
                case ImportStatus.ManuallySelected: //The field value has been manually selected by the user, and should not be recalculated.
                case ImportStatus.Conflicting: //Since values are already conflicting, adding a new value is not going to make a difference.
                    break; //Do nothing for these two cases
                case ImportStatus.NonConflicting: //All the values are either blank or the same. Loop through and check
                    string _selected = ""; //This one keeps track of the "selected" value, and will be used to set the actual selected value at the end.
                    bool _conflicting = false;
                    foreach (string _current in ImportRatings)
                    {
                        if (_current.Equals(_empty))
                        {
                            //We only care about an empty value if there is no current "selected" value; we then set this blank as current, and keeps the status as non-conflicting
                            if (_selected.Equals("")) { _selected = _current; }
                            //We implicitly any other values in current, as it's not pertinent.
                        }
                        else
                        {
                            //If the "selected" value is undetermined, we can just use the current instead. Status remains the same
                            if (_selected.Equals(_empty) || _selected.Equals("")) { _selected = _current; }
                            //If the "selected" value is different to the current value, we have conflicting values.
                            if (!_selected.Equals(_current)) { _conflicting = true; break; }
                            //We implicitly skip the case where the "selected" value is the same as the current value, so as to stay with the first occurence of the value.
                        }
                    }
                    //Finally update the selected item depending on if we have a conflict or not.
                    if (_conflicting)
                    {
                        ImportRatings.MoveCurrentToPosition(0);//Reset selection to database value
                        RatingStatus = ImportStatus.Conflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    }
                    else
                    {
                        ImportRatings.MoveCurrentTo(_selected);
                        RatingStatus = ImportStatus.NonConflicting;//Changing the current selection reset this.
                    }
                    break;
                case ImportStatus.Initial: //The status for the field has not yet been calculated; this would imply that there is only one value.
                    //We cheat and set it the back way, and manually fire the change, to avoid triggering this same recalculation.
                    string newSelectedRating = importRatingsContent.First();
                    VerifyPropertyChange(ref selectedRating, ref newSelectedRating);
                    RatingStatus = ImportStatus.NonConflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    break;
            }
            #endregion
            #region Airdate
            switch (airdateStatus)
            {
                case ImportStatus.ManuallySelected: //The field value has been manually selected by the user, and should not be recalculated.
                case ImportStatus.Conflicting: //Since values are already conflicting, adding a new value is not going to make a difference.
                    break; //Do nothing for these two cases
                case ImportStatus.NonConflicting: //All the values are either blank or the same. Loop through and check
                    string _selected = ""; //This one keeps track of the "selected" value, and will be used to set the actual selected value at the end.
                    bool _conflicting = false;
                    foreach (string _current in ImportAirdates)
                    {
                        if (_current.Equals(_empty))
                        {
                            //We only care about an empty value if there is no current "selected" value; we then set this blank as current, and keeps the status as non-conflicting
                            if (_selected.Equals("")) { _selected = _current; }
                            //We implicitly any other values in current, as it's not pertinent.
                        }
                        else
                        {
                            //If the "selected" value is undetermined, we can just use the current instead. Status remains the same
                            if (_selected.Equals(_empty) || _selected.Equals("")) { _selected = _current; }
                            //If the "selected" value is different to the current value, we have conflicting values.
                            if (!_selected.Equals(_current)) { _conflicting = true; break; }
                            //We implicitly skip the case where the "selected" value is the same as the current value, so as to stay with the first occurence of the value.
                        }
                    }
                    //Finally update the selected item depending on if we have a conflict or not.
                    if (_conflicting)
                    {
                        ImportAirdates.MoveCurrentToPosition(0);//Reset selection to database value
                        AirdateStatus = ImportStatus.Conflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    }
                    else
                    {
                        ImportAirdates.MoveCurrentTo(_selected);
                        AirdateStatus = ImportStatus.NonConflicting;//Changing the current selection reset this.
                    }
                    break;
                case ImportStatus.Initial: //The status for the field has not yet been calculated; this would imply that there is only one value.
                    //We cheat and set it the back way, and manually fire the change, to avoid triggering this same recalculation.
                    string newSelectedAirdate = importAirdatesContent.First();
                    VerifyPropertyChange(ref selectedAirdate, ref newSelectedAirdate);
                    AirdateStatus = ImportStatus.NonConflicting;//Do this after, as changing selection will change the status to ManuallySelected for a brief period.
                    break;
            }
            #endregion
            //Total Row
            recalculateRowStatus();
        }
        private void recalculateRowStatus()
        {
            if (airdateStatus == ImportStatus.Conflicting ||
                codeStatus == ImportStatus.Conflicting ||
                ratingStatus == ImportStatus.Conflicting ||
                titleStatus == ImportStatus.Conflicting)
            {
                //If any of the fields is in conflict, the row is also conflicting
                RowStatus = ImportStatus.Conflicting;
            }
            else if (airdateStatus == ImportStatus.Initial ||
                codeStatus == ImportStatus.Initial ||
                ratingStatus == ImportStatus.Initial ||
                titleStatus == ImportStatus.Initial)
            {
                //If any of the fields are still initial, the row is also initial initial
                RowStatus = ImportStatus.Initial;
            }
            else if (airdateStatus == ImportStatus.ManuallySelected &&
                codeStatus == ImportStatus.ManuallySelected &&
                ratingStatus == ImportStatus.ManuallySelected &&
                titleStatus == ImportStatus.ManuallySelected)
            {
                //If ALL of the fields are manually selected, the row is also selected
                RowStatus = ImportStatus.ManuallySelected;
            }
            else
            {
                //If none of the others have kicked in, it must be non-conflicting
                RowStatus = ImportStatus.NonConflicting;
            }
        }

    }
}