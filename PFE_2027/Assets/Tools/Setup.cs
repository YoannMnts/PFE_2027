using UnityEditor;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEngine.Application;


public static class Setup
{
   [MenuItem("Tools/Setup/Create Default Folder")]
   public static void CreateDefaultForlders()
   {
      Folders.CreateDefault("_Project", "Animation", "Art", "Materials", "Prefabs", "Resources", "Scripts", "Settings");
      UnityEditor.AssetDatabase.Refresh();
   }

   static class Folders
   {
      public static void CreateDefault(string root, params string[] folders)
      {
         var fullpath = Combine(dataPath, root);
         foreach (var folder in folders)
         {
            var path = Combine(fullpath, folder);
            if(!Exists(path))
               CreateDirectory(path);
         }
      }
   }
}
