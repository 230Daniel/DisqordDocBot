using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using DisqordDocBot.Extensions;
using Microsoft.Extensions.Logging;

namespace DisqordDocBot.Services
{
    public class TypeLoaderService : DiscordBotService
    {
        public IReadOnlyList<TypeInfo> LoadedTypes => _typeInfos;

        private readonly List<TypeInfo> _typeInfos;
        
        public TypeLoaderService(ILogger<TypeLoaderService> logger, DiscordBotBase client) : base(logger, client)
        {
            _typeInfos = new List<TypeInfo>();
            PopulateTypeCache();
        }

        public IEnumerable<MethodInfo> GetAllExtensionMethods()
        {
            var sw = Stopwatch.StartNew();
            var extensionMethods = new List<MethodInfo>();

            foreach (var loadedType in LoadedTypes)
                extensionMethods.AddRange(loadedType.GetExtensionMethodsFromType());
            
            sw.Stop();
            Logger.LogInformation($"Found all extension methods in {sw.ElapsedMilliseconds}ms");

            return extensionMethods;
        }

        private void PopulateTypeCache()
        {
            var sw = Stopwatch.StartNew();
            foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                var asm = Assembly.Load(assemblyName);
                var exportedTypes = asm.GetExportedTypes();

                foreach (var exportedType in exportedTypes)
                {
                    var exportedTypeInfo = exportedType.GetTypeInfo();
                    if(exportedTypeInfo.IsDisqordType())
                        _typeInfos.Add(exportedTypeInfo);
                }
            }
            sw.Stop();
            Logger.LogInformation($"Loaded all Disqord types in {sw.ElapsedMilliseconds}ms");
        }
    }
}