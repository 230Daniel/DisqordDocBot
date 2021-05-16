using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using DisqordDocBot.Extensions;
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

        public string GetSummaryFromInfo(MemberInfo memberInfo)
        {
            if (_documentation.TryGetValue(memberInfo.GetDocumentationKey(), out var summary))
                return summary;
            else
            {
                if (memberInfo.DeclaringType is { } declaringType)
                {
                    var types = declaringType.GetAllComposingTypes();

                    foreach (var type in types)
                    {
                        var members = type.GetMember(memberInfo.Name);
                        foreach (var member in members)
                        {
                            if (_documentation.TryGetValue(member.GetDocumentationKey(), out var inheritedSummary))
                                return inheritedSummary;
                        }
                        
                    }
                }
            }

            return "Could not find a summary for this item.";
        }

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
                    .OrderBy(x => int.Parse(x.Name[x.Name.LastIndexOf(DisqordFilenameSeparator, StringComparison.Ordinal)..]))
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
                foreach (var element in xmlDoc.Descendants("member"))
                {
                    var name = element.Attributes().FirstOrDefault(x => x.Name == "name");
                    
                    if (name is null)
                        continue;

                    if (element.Descendants().FirstOrDefault(x => x.Name == "summary") is { } summaryNode)
                    {
                        var docs = new StringBuilder();
                        var skipNext = false;
                        

                        foreach (var childNode in summaryNode.DescendantNodes())
                        {
                            if (skipNext)
                            {
                                skipNext = false;
                                continue;
                            }
                            
                            if (childNode is XElement childElement)
                            {
                                if (childElement.Name == "see")
                                {
                                    if (childElement.FirstAttribute?.Name == "cref")
                                    {
                                        var valueWithoutGenericChar = childElement.FirstAttribute.Value.Split(Global.GenericNameCharacter).First();
                                        var scopes = valueWithoutGenericChar.Split(".");
                                        
                                        docs.Append($"`{(scopes[0].StartsWith("T:") ? scopes.Last() : string.Join(".", scopes.TakeLast(2)))}`");
                                    }
                                    else if (childElement.FirstAttribute?.Name == "langword")
                                        docs.Append($"*`{childElement.FirstAttribute.Value}`*");
                                }
                                else if (childElement.Name == "c")
                                {
                                    docs.Append($"**{childElement.Value}**");
                                    skipNext = true;
                                }
                            }
                            else
                                docs.Append(childNode);}
                        
                        _documentation.Add(name.Value, docs.ToString());
                    }
                }    
            }
        }

    }
}