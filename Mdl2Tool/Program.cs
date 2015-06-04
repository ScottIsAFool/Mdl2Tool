using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using Newtonsoft.Json;
using System.Collections;
using System;

namespace Mdl2Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            TemplarianItems items;
            using (TextReader tr = new StreamReader(@"Templarian.json"))
            {
                var json = tr.ReadToEnd();

                items = JsonConvert.DeserializeObject<TemplarianItems>(json);
            }

            var actualItems = items.Items.Where(x => !x.Keywords.Contains("duplicate") && x.Name != "name" && x.Name != "unknown").Distinct(new NameComparer()).ToList();

            var list = actualItems.Select(x => new TemplarianClass {Code = "&#x" + x.Code + ";", Name = GetName(x.Name)}).OrderBy(x => x.Name).ToList();

            var sb = CreateCsFile(list);
            
            WriteToFile(@"..\..\..\Nuget\Content\Mdl2.cs", sb.ToString());

            sb = CreateXamlFile(list);

            WriteToFile(@"..\..\..\Nuget\Content\Mdl2.xaml", sb.ToString());
        }

        private static StringBuilder CreateXamlFile(List<TemplarianClass> list)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            sb.AppendLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");

            foreach (var item in list)
            {
                sb.AppendLine(string.Format("    <x:String x:Key=\"{0}\">{1}</x:String>", item.Name, item.Code));
            }

            sb.AppendLine("</ResourceDictionary>");

            return sb;
        }

        private static StringBuilder CreateCsFile(IEnumerable<TemplarianClass> list)
        {
            var sb = new StringBuilder();
            sb.AppendLine("namespace Mdl2Tool");
            sb.AppendLine("{");
            sb.AppendLine("    public class Mdl2");
            sb.AppendLine("    {");

            foreach (var item in list)
            {
                sb.AppendLine(string.Format("        public static string {0} => \"{1}\";", item.Name, item.Code));
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb;
        }

        private static string GetName(string name)
        {
            name = name.Replace("-", "_");
            return name.Pascalize();
        }

        private static void WriteToFile(string filename, string content)
        {
            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var sr = new StreamWriter(filename))
            {
                sr.Write(content);
            }
        }        
    }

    internal class NameComparer : IEqualityComparer<TemplarianClass>
    {
        public bool Equals(TemplarianClass x, TemplarianClass y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(TemplarianClass obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
