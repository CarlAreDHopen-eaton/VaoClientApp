using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Vao.Sample.Navigation
{
    /// <summary>
    /// Service for managing page navigation in the application.
    /// Provides a navigation stack to support back navigation (important for tablets/mobile).
    /// </summary>
    public class NavigationService
    {
        private readonly Stack<INavigableView> mNavigationStack = new();
        private ContentControl mContentHost;

        /// <summary>
        /// Event fired when navigation occurs
        /// </summary>
        public event EventHandler<NavigationChangedEventArgs> NavigationChanged;

        /// <summary>
        /// Initialize the navigation service with a content control to host pages
        /// </summary>
        public void Initialize(ContentControl contentHost)
        {
            mContentHost = contentHost ?? throw new ArgumentNullException(nameof(contentHost));
        }

        /// <summary>
        /// Navigate to a new page, adding current page to back stack
        /// </summary>
        public void NavigateTo(INavigableView page)
        {
            if (mContentHost == null)
                throw new InvalidOperationException("NavigationService not initialized");

            // Save current page to stack if it exists and is not null
            if (mContentHost.Content is INavigableView currentPage && currentPage != null)
            {
                mNavigationStack.Push(currentPage);
            }

            mContentHost.Content = page;
            mContentHost.IsVisible = true;  // Show the overlay
            page.OnNavigatedTo();

            NavigationChanged?.Invoke(this, new NavigationChangedEventArgs 
            { 
                NewPage = page, 
                CanGoBack = mNavigationStack.Count > 0,
                IsOverlayVisible = true
            });
        }

        /// <summary>
        /// Navigate back to the previous page in the stack
        /// </summary>
        public bool GoBack()
        {
            if (mContentHost?.Content is INavigableView currentPage && currentPage != null)
            {
                currentPage.OnNavigatingFrom();
            }

            if (mNavigationStack.Count == 0)
            {
                // No more pages - hide the navigation host
                if (mContentHost != null)
                {
                    mContentHost.Content = null;
                    mContentHost.IsVisible = false;
                }

                NavigationChanged?.Invoke(this, new NavigationChangedEventArgs
                {
                    NewPage = null,
                    CanGoBack = false,
                    IsOverlayVisible = false
                });

                return false;
            }

            var previousPage = mNavigationStack.Pop();

            mContentHost.Content = previousPage;
            previousPage.OnNavigatedTo();

            NavigationChanged?.Invoke(this, new NavigationChangedEventArgs 
            { 
                NewPage = previousPage, 
                CanGoBack = mNavigationStack.Count > 0,
                IsOverlayVisible = true
            });

            return true;
        }

        /// <summary>
        /// Clear the navigation stack (useful for logout or app reset)
        /// </summary>
        public void ClearNavigationStack()
        {
            mNavigationStack.Clear();
        }

        /// <summary>
        /// Check if navigation back is possible
        /// </summary>
        public bool CanGoBack
        {
            get { return mNavigationStack.Count > 0; }
        }

         /// <summary>
         /// Get the current number of pages in the back stack
         /// </summary>
        public int NavigationStackDepth
        {
            get { return mNavigationStack.Count; }
        }
    }

    public class NavigationChangedEventArgs : EventArgs
    {
        public INavigableView NewPage { get; set; }
        public bool CanGoBack { get; set; }
        public bool IsOverlayVisible { get; set; }
    }
}
