// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using Function.Domain.Models.Settings;
using Function.Domain.Models.Purview;
using Function.Domain.Helpers;
using Function.Domain.Models.OL;
using Function.Domain.Constants;
using Newtonsoft.Json;

namespace QualifiedNameConfigTester
{
    public class Program
    {
        private static string _path = "./config.json";

        static public void Main(string[] args)
        {
            if (args.Length > 1 && args[0] == "--path")
            {
                _path = args[1];
            }

            while (true)
            {
                try
                {

                    Console.WriteLine("Enter an OpenLineage NameSpace to test - Enter 'exit' to exit");
                    var nameSpace = Console.ReadLine();
                    if (nameSpace == null || nameSpace.Length == 0)
                    {
                        continue;
                    }
                    if (nameSpace.ToLower() == "exit")
                    {
                        break;
                    }
                    Console.WriteLine("Enter an OpenLineage Name to test");
                    var name = Console.ReadLine();
                    if (name == null || name.Length == 0)
                    {
                        continue;
                    }                
                    var config = LoadSettings();
                    IQnParser qnParser = new QnParser(config, new LoggerFactory());
                    var rtrns = qnParser.GetIdentifiers(nameSpace, name);

                    Console.WriteLine(JsonConvert.SerializeObject(rtrns));
                }
                catch (MissingCriticalDataException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: you may be missing a config file.  Please specify a path e.g. \"--path ../x/y/config.json\"." +
                                        $"  The default path is {_path}, the error message is: {ex.Message}");
                    break;
                }
            }
        }

        static public ParserSettings LoadSettings()
        {
            ParserSettings rtrn = new ParserSettings();

            var configstr = File.ReadAllText(_path);
            rtrn = JsonConvert.DeserializeObject<ParserSettings>(configstr) ?? rtrn;

            return rtrn;
        }
    }
}
