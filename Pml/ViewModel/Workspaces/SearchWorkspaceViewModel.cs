﻿namespace Sweng500.Pml.ViewModel.Workspaces
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using GalaSoft.MvvmLight.Threading;
    using log4net;
    using Sweng500.Pml.DataAccessLayer;

    /// <summary>
    /// The workspace used to perform the search and display the results
    /// </summary>
    public class SearchWorkspaceViewModel : WorkspaceViewModel
    {
        #region Statics

        /// <summary>
        /// Property name for the AwaitingResponse property
        /// </summary>
        public const string AwaitingResponsePropertyName = "AwaitingResponse";

        /// <summary>
        /// Class logger
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion Statics

        #region Fields

        /// <summary>
        /// The collection of search results
        /// </summary>
        private ObservableCollection<Media> mSearchResults = new ObservableCollection<Media>();

        /// <summary>
        /// A value indicating whether a response has been received
        /// </summary>
        private bool mAwaitingResponse = true;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SearchWorkspaceViewModel class
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <param name="inventory">A value indicating whether to search the inventory</param>
        public SearchWorkspaceViewModel(string title, bool? inventory = null)
            : base(string.Empty)
        {
            this.Results = new ListCollectionView(this.mSearchResults);

            // Kick off the search
            if (false == string.IsNullOrWhiteSpace(title) &&
                null == inventory)
            {
                this.Name = "Searching Amazon By Author For " + "\"" + title + "\"";
                this.PerformSearch(() =>
                    {
                        var mediaSearch = Repository.Instance.ServiceLocator.GetInstance<ISearchMediaService>();

                        // Return the results to another task to fill
                        return mediaSearch.AuthorSearchRemote(title, string.Empty).ToList();
                    });
            }
            else if (false == string.IsNullOrWhiteSpace(title) &&
                true == inventory.GetValueOrDefault())
            {
                this.Name = "Searching Inventory By Title For " + "\"" + title + "\"";
                this.PerformSearch(() =>
                    {
                        var mediaSearch = Repository.Instance.ServiceLocator.GetInstance<ISearchMediaService>();

                        // Return the results to another task to fill
                        return mediaSearch.GetMediaItemsContaining(title).ToList();
                    });
            }
            else if (false == string.IsNullOrWhiteSpace(title) &&
              false == inventory.GetValueOrDefault())
            {
                this.Name = "Searching Amazon By Title For " + "\"" + title + "\"";
                this.PerformSearch(() =>
                    {
                        var mediaSearch = Repository.Instance.ServiceLocator.GetInstance<ISearchMediaService>();

                        // Return the results
                        return mediaSearch.SearchRemote(title, string.Empty).ToList();
                    });
            } 
        }
        
        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the search results
        /// </summary>
        public ICollectionView Results
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a response was received
        /// </summary>
        public bool AwaitingResponse
        {
            get
            {
                return this.mAwaitingResponse;
            }

            protected set
            {
                if (value != this.AwaitingResponse)
                {
                    this.mAwaitingResponse = value;
                    this.RaisePropertyChanged(AwaitingResponsePropertyName);
                }
            }
        }

        #endregion Properties

        /// <summary>
        /// Performs the search and populates the results datagrid
        /// </summary>
        /// <param name="search">The func that performs the actual search</param>
        private void PerformSearch(Func<IList<Media>> search)
        {
            // Kick off a search that is performed asynchronously
            Task<IList<Media>>.Factory.StartNew(() =>
                {
                    return search();
                }).ContinueWith((task) =>
                    {
                        if (null != task.Exception)
                        {
                            this.ErrorMessage = task.Exception.Message;
                            return;
                        }
                        else if (null == task.Result ||
                            false == task.Result.Any())
                        {
                            this.ErrorMessage = "No Search Results";
                            return;
                        }

                        // Add all the search results
                        foreach (var media in task.Result)
                        {
                            DispatcherHelper.UIDispatcher.Invoke(new Action(() =>
                                {
                                    this.mSearchResults.Add(media);
                                }));
                        }

                        this.AwaitingResponse = false;
                    });
        }
    }
}
