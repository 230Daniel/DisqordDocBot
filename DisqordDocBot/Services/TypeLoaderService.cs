using System.Collections.Generic;
using System.Reflection;
using DisqordDocBot.Extensions;

namespace DisqordDocBot.Services
{
    public class TypeLoaderService
    {
        public IReadOnlyList<TypeInfo> LoadedTypes => _typeInfos;
        
        private readonly List<TypeInfo> _typeInfos;
        
        public TypeLoaderService()
        {
            _typeInfos = new List<TypeInfo>();
            PopulateTypeCache();
        }

        private void PopulateTypeCache()
        {
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
        }
    }
}