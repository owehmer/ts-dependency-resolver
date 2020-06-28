using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencieResolver
{
   class OrdnerNode
   {
      public string Pfad { get; set; }
      public List<DateiNode> Dateien { get; set; }
      public List<OrdnerNode> Unterordner { get; set; }

      public IList<string> AlleDependencies
      {
         get
         {
            var liste = new List<string>();
            liste.AddRange(Dateien.SelectMany(d => d.DependencyPfade).ToList());
            liste.AddRange(Unterordner.SelectMany(u => u.AlleDependencies.ToList()));
            return liste;
         }
      }

      public IList<DateiNode> AlleDateienRekuriv
      {
         get
         {
            var liste = new List<DateiNode>();
            liste.AddRange(Dateien.ToList());
            liste.AddRange(Unterordner.SelectMany(u => u.AlleDateienRekuriv.ToList()));
            return liste;
         }
      }

      public OrdnerNode(string pfad)
      {
         Pfad = pfad;
         Dateien = new List<DateiNode>();
         Unterordner = new List<OrdnerNode>();

         if (!Directory.Exists(pfad))
         {
            throw new IOException("Pfad des Ordners exitiert nicht.");
         }
      }

      public void DateienAuswerten(bool unterordnerLesen = true)
      {
         string[] fileEntries = Directory.GetFiles(Pfad);

         var fileNodes = fileEntries
            .Select(filePath =>
            {
               var node = new DateiNode(filePath);
               node.BaueDepGraph();
               return node;
            })
            .ToList();

         Dateien.AddRange(fileNodes);

         if (!unterordnerLesen)
         {
            return;
         }

         IList<string> subdirectoryEntries = Directory.GetDirectories(Pfad)
            .Where(e => !e.Contains('.') && !e.Contains("node_modules"))
            .ToList();

         var unterordnerNodes = subdirectoryEntries.Select(subDirPath =>
         {
            var node = new OrdnerNode(subDirPath);
            node.DateienAuswerten();
            return node;
         }).ToList();

         Unterordner.AddRange(unterordnerNodes);
      }
   }
}
