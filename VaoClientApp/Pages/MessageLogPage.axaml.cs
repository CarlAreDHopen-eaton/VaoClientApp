using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Vao.Sample.Controls;
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages
{
   public partial class MessageLogPage : NavigableViewBase
   {
      private readonly MessageLogPanel mMessageLogPanel;

      public MessageLogPage(MessageLogPanel messageLogPanel)
      {
         mMessageLogPanel = messageLogPanel;
         InitializeComponent();
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }

      public override void OnNavigatedTo()
      {
         // Reparent the shared MessageLogPanel into this page
         if (mMessageLogPanel.Parent is Panel oldParent)
            oldParent.Children.Remove(mMessageLogPanel);

         var host = this.FindControl<Border>("messageLogHost");
         if (host != null)
            host.Child = mMessageLogPanel;
      }

      public override void OnNavigatingFrom()
      {
         // Remove from this page so it can be reparented later
         var host = this.FindControl<Border>("messageLogHost");
         if (host != null)
            host.Child = null;
      }

      public override string GetPageTitle() => "Message Log";

      private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();
   }
}
