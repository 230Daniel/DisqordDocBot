using System;
using System.Linq;
using System.Reflection;

namespace DisqordDocBot.Extensions
{
    public static class FormattingExtensions
    {
        private const char GenericNameCharacter = '`';
        
        public static string CreateArgString(this MethodBase methodBase)
        {
            return methodBase.IsExtensionMethod() ? 
                $"this {string.Join(", ", methodBase.GetParameters().Select(x => $"{x.ParameterType.Humanize()} {x.Name}"))}" : 
                $"{string.Join(", ", methodBase.GetParameters().Select(x => $"{x.ParameterType.Humanize()} {x.Name}"))}";
        }

        public static string Humanize(this MethodInfo methodInfo) 
            => !methodInfo.IsGenericMethodDefinition ?
                $"{methodInfo.ReturnType.Humanize()} {methodInfo.Name}({methodInfo.CreateArgString()})" :
                $"{methodInfo.ReturnType.Humanize()} {methodInfo.Name}<{string.Join(", ", methodInfo.GetGenericArguments().Select(x => x.Humanize()))}>({methodInfo.CreateArgString()})";

        public static string Humanize(this Type type)
        {
            if (type.IsGenericType)
            {
                var humanName = type.Name[..type.Name.IndexOf(GenericNameCharacter)];
                return $"{humanName}<{string.Join(", ", type.GetGenericArguments().Select(x => x.Humanize()))}>";
            }

            return type.Name;
        }
    }
}