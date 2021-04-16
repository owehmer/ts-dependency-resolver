using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencieResolver
{
   class FileNode
   {
      public string PathToFile { get; }
      public IList<string> DependencyPaths { get; set; } // => [root/Foo/Bar.ts]

      private IList<string> FolderHierarchie
      {
         get
         {
            var pathInclFile = PathToFile.Split(Path.DirectorySeparatorChar).ToList();
            pathInclFile.RemoveAt(pathInclFile.Count - 1); // Delete current file
            return pathInclFile;
         }
      }

      public FileNode(string path)
      {
         DependencyPaths = new List<string>();
         PathToFile = path;

         if (!File.Exists(PathToFile))
         {
            throw new IOException("Path doesn't exist!");
         }
      }

      public void AnalyzeImportPaths()
      {
         DependencyPaths = new List<string>();

         using StreamReader sr = new StreamReader(PathToFile);
         var line = sr.ReadLine();

         while (line != null)
         {
            var lineIsEmpty = String.IsNullOrEmpty(line.Trim());
            if (lineIsEmpty)
            {
               line = sr.ReadLine();
               continue;
            }

            if (!FileHelper.IsImportLine(line))
            {
               break;
            }

            var importLine = FileHelper.ReadFullImportLine(line, sr);
            var importDestination = FileHelper.ReadImportDestination(importLine);

            importDestination = FileHelper.ReplaceRelativePaths(importDestination, FolderHierarchie);
            
            DependencyPaths.Add(importDestination);

            line = sr.ReadLine();
         }
      }

      public void BuildDepGraph(bool loadRecursive = false)
      {
         if (!DependencyPaths.Any())
         {
            AnalyzeImportPaths();
         }

         FillDepGraph();

         if (!loadRecursive)
         {
            return;
         }

         foreach (var dependencyPfad in DependencyPaths)
         {
            var depAsFile = new FileNode(dependencyPfad);
            depAsFile.BuildDepGraph(true);
         }
      }

      private void FillDepGraph()
      {
         foreach (var dependency in DependencyPaths)
         {
            if (FileHelper.DepsToFile.TryGetValue(dependency, out var externalDep))
            {
               if (!externalDep.Contains(PathToFile))
               {
                  externalDep.Add(PathToFile);
               }
            }
            else
            {
               FileHelper.DepsToFile.Add(dependency, new List<string> { PathToFile });
            }
         }
         
      }
   }
}
