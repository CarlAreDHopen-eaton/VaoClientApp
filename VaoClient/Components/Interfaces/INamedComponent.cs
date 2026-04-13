namespace Vao.Client.Components.Interfaces
{
   internal interface INamedComponent
   {
      /// <summary>
      /// Gets the name of the component
      /// </summary>
      string Name { get; }

      /// <summary>
      /// Sets a new name for the component.
      /// </summary>
      /// <param name="newName">The new name</param>
      /// <returns>True is success</returns>
      bool SetName(string newName);
   }
}
