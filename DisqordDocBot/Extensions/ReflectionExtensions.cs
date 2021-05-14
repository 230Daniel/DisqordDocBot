using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DisqordDocBot.Extensions
{
    public static class ReflectionExtensions
    {
        private const string DisqordNamespace = "Disqord";
        private const char GenericNameCharacter = '`';

        public static bool IsDisqordType(this TypeInfo typeInfo)
        {
            if (typeInfo.Namespace is null)
                return false;
            
            return typeInfo.Namespace.StartsWith(DisqordNamespace);
        }

        // gets all members
        public static IReadOnlyList<MemberInfo> GetAllMembers(this TypeInfo typeInfo)
        {
            var members = new List<MemberInfo>();
            members.AddRange(typeInfo
                .GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Except(typeInfo.GetAllBackingMethods()));
            
            // interface members dont include members implemented from other interfaces
            if (typeInfo.IsInterface)
            {
                foreach (var inheritedMembers in typeInfo.GetInterfaces().Select(x => x.GetTypeInfo().GetAllMembers()))
                    members.AddRange(inheritedMembers);
            }

            return members;
        }

        public static IReadOnlyList<MethodInfo> GetAllBackingMethods(this TypeInfo typeInfo)
        {
            var properties = typeInfo.GetProperties();
            var events = typeInfo.GetEvents();
            var backingMethods = new List<MethodInfo>();

            foreach (var property in properties)
            {
                if (property.CanRead)
                    backingMethods.Add(property.GetMethod);
                
                if (property.CanWrite)
                    backingMethods.Add(property.SetMethod);
            }

            foreach (var @event in events)
            {
                if (@event.AddMethod is not null)
                    backingMethods.Add(@event.AddMethod);
                
                if (@event.RemoveMethod is not null)
                    backingMethods.Add(@event.RemoveMethod);
                
                if (@event.RaiseMethod is not null)
                    backingMethods.Add(@event.RaiseMethod);
            }

            return backingMethods;
        }

        // public static IReadOnlyList<MethodInfo> GetAllExtensionMethods(this Type typeToSearch, IReadOnlyList<TypeInfo> typesToSearch)
        // {
        //     var extensionMethods = new List<MethodInfo>();
        //     foreach (var type in typesToSearch)
        //     {
        //         // check for static
        //         if (!(type.IsAbstract && type.IsSealed && !type.IsNested))
        //             continue;
        //
        //         var methods = type.GetMethods();
        //         extensionMethods.AddRange(methods.Where
        //             (x => x.IsDefined(typeof(ExtensionAttribute), false) &&
        //                   x.GetParameters().FirstOrDefault() is { } param && 
        //                   typeToSearch.IsAssignableTo(param.ParameterType)));
        //     }
        //
        //     return extensionMethods;
        // }

        public static IEnumerable<MethodInfo> GetExtensionMethodsFromType(this Type type)
        {
            // check for static
            if (!(type.IsAbstract && type.IsSealed && !type.IsNested))
                return Array.Empty<MethodInfo>();
            
            var methods = type.GetMethods();
            return methods.Where(x => x.IsExtensionMethod());
        }

        public static bool IsExtensionMethod(this MethodInfo methodInfo)
            => methodInfo.IsDefined(typeof(ExtensionAttribute), false) &&
               methodInfo.GetParameters().FirstOrDefault() is { };
        
        
        public static bool IsConstantField(this FieldInfo info)
            => info.IsLiteral && !info.IsInitOnly;
        
        public static bool IsBootlegConstantField(this FieldInfo info)
            => info.IsStatic && !info.IsLiteral && info.IsInitOnly;

        public static string CreateArgString(this MethodBase methodBase)
            => $"{string.Join(", ", methodBase.GetParameters().Select(x => $"{x.ParameterType.Humanize()} {x.Name}"))}";

        public static string Humanize(this Type type)
        {
            if (type.IsGenericType)
            {
                var humanName = type.Name[..type.Name.IndexOf(GenericNameCharacter)];
                return $"{humanName}<{string.Join(", ", type.GenericTypeArguments.Select(x => x.Humanize()))}>";
            }

            return type.Name;
        }
    }
}