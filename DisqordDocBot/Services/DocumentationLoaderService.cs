using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
// using System.Xml;
using System.Xml.Linq;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DisqordDocBot.Services
{
    public class DocumentationLoaderService : DiscordBotService
    {
        private const string NugetPackagePath = "packages";
        private const string DisqordFilenameSeparator = "-";
        private readonly string _nugetCacheLocation;

        public IReadOnlyDictionary<string, string> Documentation => _documentation;

        private readonly Dictionary<string, string> _documentation;

        public DocumentationLoaderService(IConfiguration configuration, ILogger<DocumentationLoaderService> logger, DiscordBotBase bot) 
            : base(logger, bot)
        {
            _documentation = new Dictionary<string, string>();
            _nugetCacheLocation = configuration["nuget:cache_location"];
            LoadDocs();
        }
        
        // public override Task StartAsync(CancellationToken cancellationToken) 
        //     => LoadDocsAsync();

        private IEnumerable<string> GetDisqordXmlDocPaths()
        {
            var xmlDocPaths = new List<string>();
            var packagePath = Path.Combine(_nugetCacheLocation, NugetPackagePath);
            
            foreach (var directory in new DirectoryInfo(packagePath).GetDirectories())
            {
                if (!directory.Name.Contains(Global.DisqordNamespace, StringComparison.OrdinalIgnoreCase))
                    continue;

                var latestVersion = new DirectoryInfo(directory.FullName)
                    .GetDirectories()
                    .OrderByDescending(x => int.Parse(x.Name[x.Name.LastIndexOf(DisqordFilenameSeparator, StringComparison.Ordinal)..]))
                    .First();
                
                var xmlDocPath = new DirectoryInfo(Path.Combine(latestVersion.FullName, "lib/net5.0/")).GetFiles().First(x => x.Extension == ".xml");
                
                xmlDocPaths.Add(xmlDocPath.FullName);
            }

            return xmlDocPaths;
        }

        private void LoadDocs()
        {
            var xmlDocs = GetDisqordXmlDocPaths().Select(XDocument.Load);
            
            foreach (var xmlDoc in xmlDocs)
            {
                foreach (XElement element in xmlDoc.Descendants("member"))
                {
                    var name = element.Attributes().FirstOrDefault(x => x.Name == "name");
                    
                    if (name is null)
                        continue;
                    
                    if (element.Descendants().FirstOrDefault(x => x.Name == "summary") is { } summaryNode)
                    {
                        StringBuilder docs = new StringBuilder();

                        foreach (var childNode in summaryNode.DescendantNodes())
                        {
                            if (childNode is XElement childElement && childElement.Name == "see")
                            {
                                var attr = childElement.Attributes().FirstOrDefault(x => x.Name == "cref");

                                if (attr != null)
                                    docs.Append($"`{attr.Value.Split(".").Last()}`");
                            }
                            else
                                docs.Append(childNode);
                        }
                        _documentation.Add(name.Value, docs.ToString());
                    }
                }    
            }
        
            Console.WriteLine(_documentation.Count);
        }

        // private async Task LoadDocsAsync()
        // {
        //     var xmlDocuments = new List<XDocument>();
        //
        //     foreach (var path in GetDisqordXmlDocPaths())
        //     {
        //         var file = File.OpenRead(path);
        //         xmlDocuments.Add(await XDocument.LoadAsync(file, LoadOptions.None, default));
        //     }
        //
        //     foreach (var xmlDocument in xmlDocuments)
        //     {
        //         var members = xmlDocument.Descendants("member");
        //         
        //         foreach (var member in members)
        //         {
        //             if (member.Element("summary") is { } summaryNode)
        //             {
        //                 _documentation.Add(member.Attribute("name").Value, summaryNode.Value);
        //             }
        //         }
        //     }
        // }
        
    }
}