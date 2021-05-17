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

        public IReadOnlyList<MethodInfo> ExtensionMethods => _extensionMethods;

        private readonly List<TypeInfo> _typeInfos;
        private readonly List<MethodInfo> _extensionMethods;

        public TypeLoaderService(ILogger<TypeLoaderService> logger, DiscordBotBase client) : base(logger, client)
        {
            _typeInfos = new List<TypeInfo>();
            _extensionMethods = new List<MethodInfo>();
            PopulateTypeCache();
            PopulateExtensionCache();
        }

        public void PopulateExtensionCache()
        {
            var sw = Stopwatch.StartNew();

            foreach (var loadedType in LoadedTypes)
                _extensionMethods.AddRange(loadedType.GetExtensionMethodsFromType());
            
            sw.Stop();
            Logger.LogInformation($"Found all extension methods in {sw.ElapsedMilliseconds}ms");
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
                    if (exportedTypeInfo.IsDisqordType() && !exportedType.IsHidden())
                        _typeInfos.Add(exportedTypeInfo);
                }
            }
            sw.Stop();
            Logger.LogInformation($"Loaded all Disqord types in {sw.ElapsedMilliseconds}ms");
        }
    }
}