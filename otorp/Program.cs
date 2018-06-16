using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace otorp
{
    class Program
    {
        private const string PropertPattern = @"(?<type>[^\s]+)\s(?<name>[^\s]+)(?=\s\{\sget;)";
        private static List<string> MessagesToImport = new List<string>();
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(GenerateProtoFiles);
        }

        private static void GenerateProtoFiles(Options options)
        {
            
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
                    var messageName = file.Name;
                    
                    var generatedMessageProperties = GenerateProtoMessages(protoTypes, r);

                    var protoFile = string.IsNullOrEmpty(options.OutputDirectory)
                        ? $"{directory}\\{messageName.RemoveFileExtension()}.proto"
                        : options.OutputDirectory;

                    WriteMessage(options, messageName, generatedMessageProperties, protoFile);
                }
            }
            Console.WriteLine($"Proto files created! Happy Servicing!");

        }

        private static List<MessageProperty> GenerateProtoMessages(Dictionary<string, string> protoTypes, StreamReader r)
        {
            var messageProperties = new List<MessageProperty>();
            var propertyPattern = new Regex(PropertPattern);
            string line;

            var fieldOrder = 1;
            while ((line = r.ReadLine()) != null)
            {
                var m = propertyPattern.Match(line);

                if (!m.Success) continue;

                var matchedType = m.Groups["type"].Value.Replace("?", "");

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
                        MessagesToImport.Add(type.ToLower());
                        type.ToCamelCase();
                    }
                }

                var name = m.Groups["name"].Value.ToUnderscoreCase();

                messageProperties.Add(
                    new MessageProperty
                    {
                        Order = fieldOrder,
                        Name = name,
                        Type = type
                    });

                
                fieldOrder++;
            }

            return messageProperties;
        }

        private static void WriteMessage(Options options, string messageName, List<MessageProperty> messageProperties, string protoFile)
        {
            if (!options.Overwrite && File.Exists(protoFile))
            {
                Console.WriteLine($"Proto file {messageName.RemoveFileExtension()} exists! Use -w to overwrite.");
            }
            else
            {
                using (var sw = new StreamWriter(protoFile))
                {
                    WriteMessageHeaders(sw, messageProperties, options);
                    sw.WriteLine();
                    WriteMessageBody(sw, messageProperties, messageName, options);
                }
            }
        }

        private static void WriteMessageHeaders(StreamWriter sw, List<MessageProperty> messageProperties, Options options)
        {
            sw.WriteLine("syntax = \"proto3\";\n");
           
            if (!string.IsNullOrEmpty(options.Namespace))
            {
                sw.WriteLine($"option csharp_namespace = \"{options.Namespace}\";\n");
            }

            if (!string.IsNullOrEmpty(options.Package))
            {
                sw.WriteLine($"package = {options.Package};\n");
            }

            foreach (var message in MessagesToImport.Distinct())
            {
                sw.WriteLine($"import \"{message}.proto\";");
            }
        }


        private static void WriteMessageBody(StreamWriter sw, List<MessageProperty> protoLines, string messageName, Options options)
        {
            sw.WriteLine("message " + messageName.RemoveFileExtension() + " {");
            foreach (var protoLine in protoLines)
            {
                sw.WriteLine($" {protoLine.Type} {protoLine.Name} = {protoLine.Order};");
            }
            sw.WriteLine("}\n");
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
