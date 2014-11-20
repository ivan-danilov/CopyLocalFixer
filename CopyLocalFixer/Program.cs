using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CopyLocalFixer
{
    internal class Program
    {
        private static void WriteUsage()
        {
            Console.WriteLine("Usage: CopyLocalFixer {/rewrite | /restore} <Path-to-SLN-file>");
            Console.WriteLine();
            Console.WriteLine("CopyLocalFixer analyzes solution and re-writes csproj files so that each external");
            Console.WriteLine("assembly referenced with CopyLocal (or Private which is the same) not more than once.");
            Console.WriteLine("It is needed because otherwise during parallel build copying of same file happens");
            Console.WriteLine("twice at times and results in an error about not accessible file for one of them.");
        }

        private static void Main(string[] args)
        {
            if (args.Length != 2 || args[0] == "-h" || args[0] == "/?" || args[0] == "/h" ||
                args[0] != "/rewrite" && args[0] != "/restore")
            {
                WriteUsage();
                return;
            }
            bool rewrite = args[0] == "/rewrite";

            var solutionPath = args[1];
            var sln = new SlnParser().Parse(new StreamReader(solutionPath));

            var assemblies = new HashSet<string>();
            foreach (var csprojPath in sln.Projects.Select(p => p.Path))
            {
                var fullCsprojPath = Path.Combine(Path.GetDirectoryName(solutionPath), csprojPath);
                if (rewrite)
                    Rewrite(fullCsprojPath, assemblies);
                else
                    Restore(fullCsprojPath);
            }
        }

        private static void Rewrite(string fullCsprojPath, HashSet<string> assemblies)
        {
            bool wasChanged = false;
            var writeTime = new FileInfo(fullCsprojPath).LastWriteTimeUtc;
            var doc = XDocument.Load(fullCsprojPath);
            var items = from itemGroupElement in doc.Root.Elements()
                where itemGroupElement.Name.LocalName == "ItemGroup"
                from item in itemGroupElement.Elements()
                select item;

            var refElements = from item in items
                where item.Name.LocalName == "Reference"
                select item;

            foreach (var refElement in refElements)
            {
                string assemblyName = refElement.Attributes().Single(attr => attr.Name.LocalName == "Include").Value;
                string filename = assemblyName.Split(',')[0];
                var isPrivateElement = refElement.Elements().FirstOrDefault(e => e.Name.LocalName == "Private");
                bool isPrivate = isPrivateElement == null || !String.Equals(isPrivateElement.Value, "false", StringComparison.InvariantCultureIgnoreCase);
                if (!isPrivate) continue;
                bool firstTime = assemblies.Add(filename);
                if (firstTime) continue;

                wasChanged = true;
                if (isPrivateElement == null)
                {
                    isPrivateElement = new XElement(doc.Root.GetDefaultNamespace() + "Private", "False");
                    refElement.Add(isPrivateElement);
                }
                else
                {
                    isPrivateElement.Value = "False";
                }
            }
            if (wasChanged)
            {
                StripReadonlyIfSet(fullCsprojPath);
                File.Move(fullCsprojPath, fullCsprojPath + ".backup");
                doc.Save(fullCsprojPath, SaveOptions.None);
                new FileInfo(fullCsprojPath).LastWriteTimeUtc = writeTime;
            }
        }

        private static void Restore(string fullCsprojPath)
        {
            if (File.Exists(fullCsprojPath + ".backup"))
            {
                File.Delete(fullCsprojPath);
                File.Move(fullCsprojPath + ".backup", fullCsprojPath);
            }
        }

        private static void StripReadonlyIfSet(string filename)
        {
            var outputFileAttributes = File.GetAttributes(filename);
            if ((outputFileAttributes & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(filename, outputFileAttributes ^ FileAttributes.ReadOnly);
        }
    }
}