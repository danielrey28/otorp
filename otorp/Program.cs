using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CommandLine;

namespace otorp
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(GenerateProtoFiles);
            Console.ReadLine();
        }

        private static void GenerateProtoFiles(Options options)
        {
            
            var propertyPattern = new Regex(@"(?<type>[^\s]+)\s(?<name>[^\s]+)(?=\s\{\sget;)");

            var protoTypes = new Dictionary<string, string>
            {
                {"int", "int32"},
                {"string", "string"},
                {"bool", "bool"},
                {"DateTime", "date"},
            };

            

            Console.WriteLine("Creating Proto files...");

            var directory = new DirectoryInfo(options.InputDirectory);

            foreach (var file in directory.GetFiles("*.cs"))
            {
                using (var r = new StreamReader(file.FullName))
                {
                    string line;
                    var messageName = file.Name;
                    var fieldOrder = 1;
                    var protoLines = new List<string> {"syntax = \"proto3\";\n"};

                    if (!string.IsNullOrEmpty(options.Namespace))
                    {
                        protoLines.Add($"option csharp_namespace = \"{options.Namespace}\";\n");
                    }

                    if (!string.IsNullOrEmpty(options.Package))
                    {
                        protoLines.Add($"package = {options.Package};\n");
                    }

                    protoLines.Add("message " + messageName.RemoveFileExtension() + " {");
                
                    while ((line = r.ReadLine()) != null)
                    {
                        var m = propertyPattern.Match(line);

                        if (!m.Success) continue;

                        var matchedType = m.Groups["type"].Value;
                        protoTypes.TryGetValue(matchedType, out var type);
                        if (string.IsNullOrEmpty(type))
                        {
                            if (IsCollectionType(matchedType))
                            {
                                type = GetCollectionType(matchedType);
                            }
                            else
                            {
                                type = matchedType;
                                type.ToCamelCase();
                            }
                        }

                        var name = m.Groups["name"].Value.ToUnderscoreCase();
                        protoLines.Add($" {type} {name} = {fieldOrder};");
                        fieldOrder++;
                    }

                    protoLines.Add("}\n");

                    var protoFile = string.IsNullOrEmpty(options.OutputDirectory)
                        ? $"{directory}\\{messageName.RemoveFileExtension()}.proto"
                        : options.OutputDirectory;

                    if (!options.Overwrite && File.Exists(protoFile))
                    {
                        Console.WriteLine($"Proto file {messageName.RemoveFileExtension()} exists! Use -w to overwrite.");
                    }
                    else
                    {
                        using (var sw = new StreamWriter(protoFile))
                        {
                            
                            foreach (var protoLine in protoLines)
                            {
                                sw.WriteLine(protoLine);
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Proto files created! Happy Servicing!");

        }

        private static string GetCollectionType(string type)
        {
            var collectionPattern = new Regex(@"(?<collection>[^\s]+\<(?<type>[^\s]+)\>)");
            var matchedString = $"repeated {collectionPattern.Match(type).Groups["type"].Value}";
            return matchedString;
        }

        private static bool IsCollectionType(string type)
        {
            var collectionPattern = new Regex(@"(?<collection>[^\s]+\<(?<type>[^\s]+)\>)");
            var match = collectionPattern.Match(type).Success;
            return match;
        }
    }

    
}
