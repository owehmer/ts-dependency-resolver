using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DependencieResolver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                throw new IOException("You need to define a path!");
            }

            var folderPath = args[0];
            var rootFolder = new FolderNode(folderPath);
            rootFolder.AnalyzeFiles();

            var allDependencies = FileHelper.DepsToFile;

            using StreamWriter sw = new StreamWriter(folderPath + Path.DirectorySeparatorChar + "dependencies.json");
           
            sw.Write(JsonConvert.SerializeObject(allDependencies, Formatting.Indented));
            sw.Close();
        }

        private static void AnalyzeAngularJson(string root)
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
