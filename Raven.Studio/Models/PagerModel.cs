using System;
using System.Windows.Input;
using Raven.Studio.Commands;
using Raven.Studio.Infrastructure;

namespace Raven.Studio.Models
{
	public class PagerModel : NotifyPropertyChangedBase
	{
		public event EventHandler PagerChanged;

		public PagerModel()
		{
			IsSkipBasedOnTheUrl = true;
			PageSize = 25;
			SetTotalResults();
		}

		private bool totalResultsPreviouslySet;
		public void SetTotalResults(Observable<long?> observable = null)
		{
			TotalResults = observable ?? new Observable<long?>();
			TotalResults.PropertyChanged += (sender, args) =>
			                                {
												if (totalResultsPreviouslySet && ((Observable<long?>)sender).Value == TotalResults.Value) 
													return;
			                                	totalResultsPreviouslySet = true;
			                                	OnPropertyChanged("TotalPages");
			                                	OnPropertyChanged("HasNextPage");
			                                };
		}

		private int pageSize;
		public int PageSize
		{
			get { return pageSize; }
			set
			{
				pageSize = value;
				OnPropertyChanged();
				OnPropertyChanged("CurrentPage");
				OnPropertyChanged("TotalPages");
			}
		}

		public int CurrentPage
		{
			get { return Skip/PageSize + 1 + (Skip != 0 && Skip < PageSize ? 1 : 0); }
		}

		public Observable<long?> TotalResults { get; private set; }
		public long TotalPages
		{
			get
			{
				int add = (TotalResults.Value ?? 0) % PageSize == 0 ? 0 : 1;
				var totalPages = (TotalResults.Value ?? 0)/ PageSize + add;

				// if all documents from this page where deleted we want to go the the previous page so we won't show an empty page
				if(totalPages < CurrentPage && totalPages != 0) 
					NavigateToPrevPage();

				return totalPages == 0 ? 1 : totalPages;
			}
		}

		private short? skip;
		public short Skip
		{
			get
			{
				if (skip == null)
				{
					if (IsSkipBasedOnTheUrl)
						SetSkip(new UrlParser(UrlUtil.Url));
					else
						skip = 0;
				}
				return skip.Value;
			}
			set
			{
				skip = Math.Max((short) 0, value);
				OnPropertyChanged();
				OnPropertyChanged("CurrentPage");
				OnPropertyChanged("HasPrevPage");
			}
		}

		public bool IsSkipBasedOnTheUrl { get; set; }

		public void SetSkip(UrlParser urlParser)
		{
			short s;
			short.TryParse(urlParser.GetQueryParam("skip"), out s);
			Skip = s;
		}

		public bool HasNextPage
		{
			get { return CurrentPage < TotalPages; }
		}

		public bool HasPrevPage
		{
			get { return Skip > 0; }
		}

		public void NavigateToNextPage()
		{
			NavigateToPage(1);
		}

		public void NavigateToPrevPage()
		{
			NavigateToPage(-1);
		}

		private void NavigateToPage(int pageOffset)
		{
			var skip1 = Skip + pageOffset*PageSize;
			Skip = (short) skip1;

			if (IsSkipBasedOnTheUrl)
			{
				var urlParser = new UrlParser(UrlUtil.Url);
				urlParser.SetQueryParam("skip", Skip);
				UrlUtil.Navigate(urlParser.BuildUrl());
			}

			OnPagerChanged();
		}

		public void OnPagerChanged()
		{
			if (PagerChanged != null)
				PagerChanged(this, EventArgs.Empty);
		}

		public ICommand NextPage
		{
			get { return new NavigateToNextPageCommand(this); }
		}

		public ICommand PrevPage
		{
			get { return new NavigateToPrevPageCommand(this); }
		}
	}
}