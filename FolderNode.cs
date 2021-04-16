using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencieResolver
{
   class FolderNode
   {
      public string FolderPath { get; set; }
      public List<FileNode> Files { get; set; }
      public List<FolderNode> SubDictionaries { get; set; }

      public IList<string> AllDependencies
      {
         get
         {
            var list = new List<string>();
            list.AddRange(Files.SelectMany(d => d.DependencyPaths).ToList());
            list.AddRange(SubDictionaries.SelectMany(u => u.AllDependencies.ToList()));
            return list;
         }
      }

      public IList<FileNode> AllFilesRecursive
      {
         get
         {
            var list = new List<FileNode>();
            list.AddRange(Files.ToList());
            list.AddRange(SubDictionaries.SelectMany(u => u.AllFilesRecursive.ToList()));
            return list;
         }
      }

      public FolderNode(string path)
      {
         FolderPath = path;
         Files = new List<FileNode>();
         SubDictionaries = new List<FolderNode>();

         if (!Directory.Exists(path))
         {
            throw new IOException("Path to folder doesn't exist.");
         }
      }

      public void AnalyzeFiles(bool readSubDictionaries = true)
      {
         string[] fileEntries = Directory.GetFiles(FolderPath);

         var fileNodes = fileEntries
            .Select(filePath =>
            {
               var node = new FileNode(filePath);
               node.BuildDepGraph();
               return node;
            })
            .ToList();

         Files.AddRange(fileNodes);

         if (!readSubDictionaries)
         {
            return;
         }

         IList<string> subdirectoryEntries = Directory.GetDirectories(FolderPath)
            .Where(e => !e.Contains('.') && !e.Contains("node_modules"))
            .ToList();

         var subFolderNodes = subdirectoryEntries.Select(subDirPath =>
         {
            var node = new FolderNode(subDirPath);
            node.AnalyzeFiles();
            return node;
         }).ToList();

         SubDictionaries.AddRange(subFolderNodes);
      }
   }
}
