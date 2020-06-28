using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DependencieResolver
{
   class Program
   {
      static void Main(string[] args)
      {
         if (!args.Any())
         {
            throw new IOException("Du musst einen Pfad definieren!");
         }

         var folderPath = args[0];
         var rootFolder = new OrdnerNode(folderPath);
         rootFolder.DateienAuswerten();

         var alleDependencies = DateiHelper.DepsAufDatei;
      }

      // TODO
      private static void AngularJsonAuslesen(string root)
      {
         var angularJsonPfad = root + Path.DirectorySeparatorChar + "angular.json";

         if (!File.Exists(angularJsonPfad))
         {
            return;
         }

         using StreamReader sr = new StreamReader(angularJsonPfad);
         var angularJson = sr.ReadToEnd();

         IDictionary<string, JToken> Jsondata = JObject.Parse(angularJson);
         Jsondata.TryGetValue("projects", out var projects);
         var projektRoots = projects.Select(projekt =>
         {
            var projektProperties = projekt.Children().First().Children();
            var rootPfadNode = projektProperties
               .FirstOrDefault(p =>
               {
                  if (p is JProperty pr)
                  {
                     return pr.Name == "root";
                  }

                  return false;
               });

            var rootPfadValueNode = rootPfadNode.Children().First();
            var rootPfadValue = rootPfadValueNode.Value<string>();
            /*
             * ?.Select(prop => ((JProperty) prop).Name)
               .FirstOrDefault();
             */

            return rootPfadValue;
         }).ToList();
      }
   }
}
