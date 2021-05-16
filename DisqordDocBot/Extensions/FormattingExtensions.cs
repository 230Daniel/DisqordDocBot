using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DisqordDocBot.Extensions
{
    public static class FormattingExtensions
    {
        public static string CreateArgString(this MethodBase methodBase)
        {
            return methodBase.IsExtensionMethod() ? 
                $"this {string.Join(", ", methodBase.GetParameters().Select(x => $"{x.ParameterType.Humanize()} {x.Name}"))}" : 
                $"{string.Join(", ", methodBase.GetParameters().Select(x => $"{x.ParameterType.Humanize()} {x.Name}"))}";
        }
        
        public static string CreateGenericArgString(this MethodBase methodBase) 
            => methodBase.IsGenericMethod ? 
                $"<{string.Join(", ", methodBase.GetGenericArguments().Select(x => x.Humanize()))}>" : 
                string.Empty;

        public static string Humanize(this Type type)
        {
            if (type.IsGenericType)
            {
                var humanName = type.Name[..type.Name.IndexOf(Global.GenericNameCharacter)];
                return $"{humanName}<{string.Join(", ", type.GetGenericArguments().Select(x => x.Humanize()))}>";
            }

            return type.Name;
        }

        public static string GetDocumentationKey(this MemberInfo info)
        {
            return info switch
            {
                TypeInfo typeInfo => $"T:{typeInfo.FullName}",
                PropertyInfo propertyInfo => $"P:{propertyInfo.DeclaringType?.FullName}.{propertyInfo.Name}",
                MethodBase methodInfo => methodInfo.GetDocumentationKeyForMethod(),
                EventInfo eventInfo => $"E:{eventInfo.DeclaringType?.FullName}.{eventInfo.Name}",
                FieldInfo fieldInfo => $"F:{fieldInfo.DeclaringType?.FullName}.{fieldInfo.Name}",
                _ => ""
            };
        }

        private static string GetDocumentationKeyForMethod(this MethodBase methodInfo)
        {
            var sb = new StringBuilder();
            sb.Append($"M:{methodInfo.DeclaringType?.FullName}.");
            sb.Append(methodInfo is not ConstructorInfo ? $"{methodInfo.Name}" : "#ctor");

            var genericArguments = Array.Empty<Type>();
            if (methodInfo.IsGenericMethod)
            {
                genericArguments = methodInfo.GetGenericArguments();
                sb.Append($"``{genericArguments.Length}");
            }

            if (methodInfo.GetParameters().Length > 0)
                sb.Append($"({string.Join(',', methodInfo.GetParameters().Select(x => x.GetDocumentationFormatForParameter(genericArguments)))})");

            return sb.ToString();
        }

        private static string GetDocumentationFormatForParameter(this ParameterInfo param, Type[] genericMethodArgs)
        {
            var sb = new StringBuilder();
            sb.Append($"{param.ParameterType.Namespace}.{param.ParameterType.Name.Split(Global.GenericNameCharacter).First()}");

            if (param.ParameterType.IsGenericType)
            {
                sb.Append('{');
                
                var genericArgs = new List<string>();

                foreach (var genericArgument in param.ParameterType.GetGenericArguments())
                {
                    string argument = null;
                    for (var i = 0; i < genericMethodArgs.Length; i++)
                    {
                        if (genericArgument == genericMethodArgs[i])
                        {
                            argument = $"``{i}";
                            break;
                        }
                    }

                    argument ??= genericArgument.FullName;
                    genericArgs.Add(argument);
                }

                sb.Append(string.Join(',', genericArgs));
                
                sb.Append('}');
            }

            return sb.ToString();
        }
    }
}