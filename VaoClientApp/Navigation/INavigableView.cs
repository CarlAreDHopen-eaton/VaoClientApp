namespace Vao.Sample.Navigation
{
    /// <summary>
    /// Interface for views/pages that can be navigated to in the application.
    /// Replaces modal dialogs with page-based navigation for better tablet support.
    /// </summary>
    public interface INavigableView
    {
        /// <summary>
        /// Called when the page is navigated to
        /// </summary>
        void OnNavigatedTo();

        /// <summary>
        /// Called when the page is being navigated away from
        /// </summary>
        void OnNavigatingFrom();

        /// <summary>
        /// Get the display title for this page (shown in back navigation)
        /// </summary>
        string GetPageTitle();
    }
}
