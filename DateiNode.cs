using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencieResolver
{
   class DateiNode
   {
      public string Pfad { get; }
      public IList<string> DependencyPfade { get; set; } // => [root/Foo/Bar.ts]

      private IList<string> OrderHierarchie
      {
         get
         {
            var pfadeInklDatei = Pfad.Split(Path.DirectorySeparatorChar).ToList();
            pfadeInklDatei.RemoveAt(pfadeInklDatei.Count - 1); // Aktuelle Datei löschen
            return pfadeInklDatei;
         }
      }

      public DateiNode(string pfad)
      {
         DependencyPfade = new List<string>();
         Pfad = pfad;

         if (!File.Exists(Pfad))
         {
            throw new IOException("Pfad existiert nicht!");
         }
      }

      public void WerteImportPfadeAus()
      {
         DependencyPfade = new List<string>();

         using StreamReader sr = new StreamReader(Pfad);
         var line = sr.ReadLine();

         while (line != null)
         {
            var zeileIstLeer = String.IsNullOrEmpty(line.Trim());
            if (zeileIstLeer)
            {
               line = sr.ReadLine();
               continue;
            }

            if (!DateiHelper.IstImportZeile(line))
            {
               break;
            }

            var importZeile = DateiHelper.LiesGanzeImportZeile(line, sr);
            var importZiel = DateiHelper.LiesImportZielAus(importZeile);

            importZiel = DateiHelper.ErsetzeRelativePfade(importZiel, OrderHierarchie);
            
            DependencyPfade.Add(importZiel);

            line = sr.ReadLine();
         }
      }

      public void BaueDepGraph(bool rekursivLaden = false)
      {
         if (!DependencyPfade.Any())
         {
            WerteImportPfadeAus();
         }

         FuelleDepGraph();

         if (!rekursivLaden)
         {
            return;
         }

         foreach (var dependencyPfad in DependencyPfade)
         {
            var depAlsDatei = new DateiNode(dependencyPfad);
            depAlsDatei.BaueDepGraph(true);
         }
      }

      private void FuelleDepGraph()
      {
         foreach (var dependency in DependencyPfade)
         {
            if (DateiHelper.DepsAufDatei.TryGetValue(dependency, out var externeDep))
            {
               if (!externeDep.Contains(Pfad))
               {
                  externeDep.Add(Pfad);
               }
            }
            else
            {
               DateiHelper.DepsAufDatei.Add(dependency, new List<string> { Pfad });
            }
         }
         
      }
   }
}
