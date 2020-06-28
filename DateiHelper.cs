using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DependencieResolver
{
   static class DateiHelper
   {
      public static Dictionary<string, IList<string>> DepsAufDatei = new Dictionary<string, IList<string>>();

      public static string LiesGanzeImportZeile(string aktuelleZeile, StreamReader sr)
      {
         var istEinzeiligerImport = DateiHelper.IstImportZeile(aktuelleZeile) && DateiHelper.IstImportZeileEnde(aktuelleZeile) || IstZeileUnspezifischerImport(aktuelleZeile);
         if (istEinzeiligerImport)
         {
            return aktuelleZeile;
         }

         var zusammengesetzteZeile = new StringBuilder();
         while (aktuelleZeile != null && !DateiHelper.IstImportZeileEnde(aktuelleZeile))
         {
            zusammengesetzteZeile.Append(aktuelleZeile);
            try
            {
               aktuelleZeile = sr.ReadLine();
            }
            catch (IOException)
            {
               break;
            }
         }
         zusammengesetzteZeile.Append(aktuelleZeile);

         return zusammengesetzteZeile.ToString();
      }

      public static bool IstImportZeile(string zeile)
      {
         return zeile?.Trim().StartsWith("import ") ?? false;
      }
      public static bool IstImportZeileEnde(string zeile)
      {
         var fromIstEnthalten = zeile?.Contains(" from ") ?? false;
         return fromIstEnthalten;
      }

      private static readonly string RegexImport = "^import ['|\"]([^'\"]+)['|\"][;]*$";

      public static bool IstZeileUnspezifischerImport(string zeile)
      {
         var importRegel = new Regex(RegexImport);
         return importRegel.IsMatch(zeile);
      }

      public static string LiesImportZielAus(string zeile)
      {
         if (IstZeileUnspezifischerImport(zeile))
         {
            var match = new Regex(RegexImport).Match(zeile);
            return match.Groups.Values.Last().Value;
         }
         var split = zeile.Split(" from ");

         var fromPfad = split[^1]
            .Replace("'", string.Empty)
            .Replace("\"", string.Empty)
            .Replace(";", string.Empty);

         return fromPfad;
      }

      public static string ErsetzeRelativePfade(string zeile, IList<string> orderBisZurDatei)
      {
         var zeileEnthaeltRelativePfade = zeile.Contains('.') || zeile.Contains("..");
         if (!zeileEnthaeltRelativePfade)
         {
            return zeile;
         }

         var zeileMitRichtigenSeperatoren = zeile.UmwandelnZuRichtigenSeperatoren();
         var zeileOhneRelativesHier = ErsetzeRelativesHier(zeileMitRichtigenSeperatoren, orderBisZurDatei);
         var zeileOhneRelativeOrdnerHoch = ErsetzeRelativesOrderHoch(zeileOhneRelativesHier, orderBisZurDatei);

         return zeileOhneRelativeOrdnerHoch;
      }

      private static string ErsetzeRelativesHier(string zeile, IList<string> orderBisZurDatei)
      {
         if (zeile.StartsWith($".{Path.DirectorySeparatorChar}"))
         {
            var aktuellerPfad = String.Join(Path.DirectorySeparatorChar, orderBisZurDatei) + Path.DirectorySeparatorChar;
            zeile = zeile.Replace($".{Path.DirectorySeparatorChar}", aktuellerPfad); // Was wenn mehrere Punkte da sind?
         }

         return zeile;
      }

      private static string ErsetzeRelativesOrderHoch(string zeile, IList<string> orderBisZurDatei)
      {
         var anzahlEbenenHoch = new Regex(Regex.Escape("..")).Matches(zeile).Count;

         if (anzahlEbenenHoch == 0)
         {
            return zeile;
         }

         while (anzahlEbenenHoch > 0)
         {
            orderBisZurDatei.RemoveAt(orderBisZurDatei.Count - 1);
            anzahlEbenenHoch--;
         }

         var zeileOhneRelativePfade = zeile.Replace($"..{Path.DirectorySeparatorChar}", ""); // Ebenen hoch löschen
         var zeileMitErsetztemPfad = String.Join(Path.DirectorySeparatorChar, orderBisZurDatei) + Path.DirectorySeparatorChar + zeileOhneRelativePfade;

         return zeileMitErsetztemPfad;
      }

      public static string UmwandelnZuRichtigenSeperatoren(this string zeileMitPfad)
      {
         return zeileMitPfad
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
      }
   }
}
