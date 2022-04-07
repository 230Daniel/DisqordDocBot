using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DisqordDocBot.Extensions
{
    public static class ReflectionExtensions
    {
        private const BindingFlags SearchFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static bool IsDisqordType(this TypeInfo typeInfo)
        {
            if (typeInfo.Namespace is null)
                return false;

            return typeInfo.Namespace.StartsWith(Global.DisqordNamespace);
        }

        // gets all members
        public static IReadOnlyList<MemberInfo> GetAllMembers(this TypeInfo typeInfo)
        {
            var members = new List<MemberInfo>();
            members.AddRange(typeInfo
                .GetMembers(SearchFlags)
                .Where(x => !x.IsHidden())
                .Except(typeInfo.GetAllBackingMethods()));

            // interface members dont include members implemented from other interfaces
            if (typeInfo.IsInterface)
            {
                foreach (var inheritedMembers in typeInfo.GetInterfaces().Select(x => x.GetTypeInfo().GetAllMembers()))
                    members.AddRange(inheritedMembers);
            }

            return members;
        }

        public static IEnumerable<MemberInfo> GetAllBackingMethods(this TypeInfo typeInfo)
        {
            // would love to not string compare everything but explicit interface impls got the best of me
            return typeInfo.GetMembers(SearchFlags).Where(x => x is MethodInfo &&
                                                               (x.Name.Contains("get_") ||
                                                                x.Name.Contains("set_") ||
                                                                x.Name.Contains("add_") ||
                                                                x.Name.Contains("remove_")));
        }

        public static IEnumerable<MethodInfo> GetExtensionMethodsFromType(this Type type)
        {
            // check for static
            if (!(type.IsAbstract && type.IsSealed && !type.IsNested))
                return Array.Empty<MethodInfo>();

            var methods = type.GetMethods();
            return methods.Where(x => x.IsExtensionMethod());
        }

        public static bool IsExtensionMethod(this MethodBase methodBase)
            => methodBase.IsDefined(typeof(ExtensionAttribute), false) &&
               methodBase.GetParameters().FirstOrDefault() is { };

        public static bool IsConstantField(this FieldInfo info)
            => info.IsLiteral && !info.IsInitOnly;

        public static bool IsBootlegConstantField(this FieldInfo info)
            => info.IsStatic && !info.IsLiteral && info.IsInitOnly;

        public static IReadOnlyList<Type> GetAllComposingTypes(this Type type)
        {
            var types = new List<Type>();
            types.AddRange(type.GetInterfaces());

            var currentType = type;
            while (currentType.BaseType is not null)
            {
                types.Add(currentType.BaseType);
                currentType = currentType.BaseType;
            }

            return types;
        }

        public static bool IsHidden(this MemberInfo info)
        {
            if (info is MethodInfo methodInfo)
                return methodInfo.IsPrivate;

            return false;
        }
    }
}
