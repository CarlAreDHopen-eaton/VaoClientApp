using Vao.Sample.Layouts;

namespace Vao.Sample.Controls;

/// <summary>Wraps a video layout definition for display in the split selector list.</summary>
public class SplitSelectionItem
{
   /// <summary>The underlying layout definition.</summary>
   public VideoLayoutDefinition Layout { get; }

   /// <summary>The display name shown in the list.</summary>
   public string DisplayName
   {
      get { return Layout.DisplayName; }
   }

   /// <summary>The layout key used to switch layouts.</summary>
   public string Key
   {
      get { return Layout.Key; }
   }

   public SplitSelectionItem(VideoLayoutDefinition layout)
   {
      Layout = layout;
   }
}
