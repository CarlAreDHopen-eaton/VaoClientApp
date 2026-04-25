using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Vao.Sample.Navigation
{
    /// <summary>
    /// Base class for navigable pages/views.
    /// Provides common functionality for pages in the navigation system.
    /// </summary>
    public class NavigableViewBase : UserControl, INavigableView
    {
        public NavigationService NavigationService { get; set; }

        public virtual void OnNavigatedTo()
        {
            // Override in derived classes to perform initialization
        }

        public virtual void OnNavigatingFrom()
        {
            // Override in derived classes to perform cleanup
        }

        public virtual string GetPageTitle()
        {
            return this.GetType().Name;
        }

        /// <summary>
        /// Helper to perform navigation with null-safety
        /// </summary>
        protected void Navigate(INavigableView page)
        {
            if (NavigationService != null)
                NavigationService.NavigateTo(page);
        }

        /// <summary>
        /// Helper to go back in navigation stack
        /// </summary>
        protected bool GoBack()
        {
            return NavigationService?.GoBack() ?? false;
        }

        /// <summary>
        /// Check if we can go back
        /// </summary>
        protected bool CanGoBack => NavigationService?.CanGoBack ?? false;
    }
}
