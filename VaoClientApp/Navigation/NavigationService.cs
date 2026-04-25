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
        private readonly Stack<INavigableView> _navigationStack = new();
        private ContentControl _contentHost;

        /// <summary>
        /// Event fired when navigation occurs
        /// </summary>
        public event EventHandler<NavigationChangedEventArgs> NavigationChanged;

        /// <summary>
        /// Initialize the navigation service with a content control to host pages
        /// </summary>
        public void Initialize(ContentControl contentHost)
        {
            _contentHost = contentHost ?? throw new ArgumentNullException(nameof(contentHost));
        }

        /// <summary>
        /// Navigate to a new page, adding current page to back stack
        /// </summary>
        public void NavigateTo(INavigableView page)
        {
            if (_contentHost == null)
                throw new InvalidOperationException("NavigationService not initialized");

            // Save current page to stack if it exists and is not null
            if (_contentHost.Content is INavigableView currentPage && currentPage != null)
            {
                _navigationStack.Push(currentPage);
            }

            _contentHost.Content = page;
            _contentHost.IsVisible = true;  // Show the overlay
            page.OnNavigatedTo();

            NavigationChanged?.Invoke(this, new NavigationChangedEventArgs 
            { 
                NewPage = page, 
                CanGoBack = _navigationStack.Count > 0,
                IsOverlayVisible = true
            });
        }

        /// <summary>
        /// Navigate back to the previous page in the stack
        /// </summary>
        public bool GoBack()
        {
            if (_contentHost?.Content is INavigableView currentPage && currentPage != null)
            {
                currentPage.OnNavigatingFrom();
            }

            if (_navigationStack.Count == 0)
            {
                // No more pages - hide the navigation host
                if (_contentHost != null)
                {
                    _contentHost.Content = null;
                    _contentHost.IsVisible = false;
                }

                NavigationChanged?.Invoke(this, new NavigationChangedEventArgs
                {
                    NewPage = null,
                    CanGoBack = false,
                    IsOverlayVisible = false
                });

                return false;
            }

            var previousPage = _navigationStack.Pop();

            _contentHost.Content = previousPage;
            previousPage.OnNavigatedTo();

            NavigationChanged?.Invoke(this, new NavigationChangedEventArgs 
            { 
                NewPage = previousPage, 
                CanGoBack = _navigationStack.Count > 0,
                IsOverlayVisible = true
            });

            return true;
        }

        /// <summary>
        /// Clear the navigation stack (useful for logout or app reset)
        /// </summary>
        public void ClearNavigationStack()
        {
            _navigationStack.Clear();
        }

        /// <summary>
        /// Check if navigation back is possible
        /// </summary>
        public bool CanGoBack => _navigationStack.Count > 0;

        /// <summary>
        /// Get the current number of pages in the back stack
        /// </summary>
        public int NavigationStackDepth => _navigationStack.Count;
    }

    public class NavigationChangedEventArgs : EventArgs
    {
        public INavigableView NewPage { get; set; }
        public bool CanGoBack { get; set; }
        public bool IsOverlayVisible { get; set; }
    }
}
