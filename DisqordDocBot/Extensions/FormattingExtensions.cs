using System;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public static string GetDocumentationKey(this MemberInfo info)
        {
            if (info is MethodInfo methodInfoa && methodInfoa.Name == "WithContent")
                Console.WriteLine("spotted");
            
            return info switch
            {
                TypeInfo typeInfo => $"T:{typeInfo.FullName}",
                PropertyInfo propertyInfo => $"P:{propertyInfo.DeclaringType}.{propertyInfo.Name}",
                MethodInfo methodInfo => $"M:{methodInfo.DeclaringType}.{methodInfo.Name}({string.Join(',', methodInfo.GetParameters().Select(x => x.ParameterType.FullName))})",
                EventInfo eventInfo => $"E:{eventInfo.DeclaringType}.{eventInfo.Name}",
                FieldInfo fieldInfo => $"F:{fieldInfo.DeclaringType}.{fieldInfo.Name}",
                _ => ""
            };
        }
    }
}