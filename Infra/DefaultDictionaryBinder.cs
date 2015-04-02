﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using Microsoft.CSharp.RuntimeBinder;

namespace Infra
{
    /// <summary>
    /// ASP.NET MVC Default Dictionary Binder.
    /// </summary>
    public class DefaultDictionaryBinder : DefaultModelBinder
    {
        //// based on: https://github.com/loune/MVCStuff

        private static readonly object GetValueProviderKeys_CacheKey = new CustomKey("GetValueProviderKeys_CacheKey");

        private static readonly ConcurrentDictionary<Type, MetaData> ModelTypeToMetadata
            = new ConcurrentDictionary<Type, MetaData>();

        private static readonly ConcurrentDictionary<Type, MetaData> KeyTypeToMetadata
            = new ConcurrentDictionary<Type, MetaData>();

        private readonly IModelBinder nextBinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDictionaryBinder"/> class. 
        /// </summary>
        public DefaultDictionaryBinder()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDictionaryBinder"/> class. 
        /// </summary>
        /// <param name="nextBinder">
        /// The next model binder to chain call. If null, by default, the DefaultModelBinder is called.
        /// </param>
        public DefaultDictionaryBinder(IModelBinder nextBinder)
        {
            this.nextBinder = nextBinder;
        }

        /// <summary>
        /// Binds the model by using the specified controller context and binding context.
        /// </summary>
        /// <returns> The bound object. </returns>
        /// <param name="controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <param name="bindingContext">The context within which the model is bound. The context includes information such as the model object, model name, model type, property filter, and value provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="bindingContext "/>parameter is null.</exception>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Type modelType = bindingContext.ModelType;
            MetaData metaData = ModelTypeToMetadata.GetOrAdd(modelType, CreateMetaData);

            // optional delegates won't bind...
            // this is useful to use a method that has a delegate in the declaration as an action
            // and also as a method that can be called, passing an optional delegate
            // todo: use IoC to pass a delegate when one is required
            if (modelType.IsSubclassOf(typeof(Delegate)) && !bindingContext.ModelMetadata.IsRequired)
                return null;

            // skipping collections containing an Index property
            bool isOldCollection = metaData.ListType != null
                                   && bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Index") != null;

            // todo: should check modelType against IDictionary<> implementation
            if (!modelType.Name.StartsWith("Dictionary`") || metaData.DictType == null || isOldCollection)
            {
                return
                    this.nextBinder != null
                        ? this.nextBinder.BindModel(controllerContext, bindingContext)
                        : base.BindModel(controllerContext, bindingContext);
            }

            dynamic result = null;
            var dictionaryKeys = new HashSet<string>();

            foreach (var genericArgs in metaData.ListDictionaryGenericArgs)
            {
                var keyType = genericArgs[0];
                var valueType = genericArgs[1];
                var typePair = new TypePair(keyType, valueType);
                var valueBinder = this.Binders.GetBinder(valueType);
                var keyTypeMetaData = KeyTypeToMetadata.GetOrAdd(keyType, t => new MetaData());

                foreach (string key in GetValueProviderKeys(controllerContext))
                {
                    if (!key.StartsWith(bindingContext.ModelName + "[", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    int endbracket = key.IndexOf("]", bindingContext.ModelName.Length + 1, StringComparison.Ordinal);
                    if (endbracket == -1)
                        continue;

                    // if type conversion ever throws a NotSupportedException, it will never be done again
                    if (keyTypeMetaData.TypeConverterThrew)
                        continue;

                    if (metaData.DictionaryMethodThrew.ContainsKey(typePair))
                        continue;

                    dynamic dictKey;
                    string dictKeyStr = key.Substring(
                        bindingContext.ModelName.Length + 1, endbracket - bindingContext.ModelName.Length - 1);

                    try
                    {
                        dictKey = this.ConvertType(dictKeyStr, keyType);
                    }
                    catch (NotSupportedException)
                    {
                        keyTypeMetaData.TypeConverterThrew = true;
                        continue;
                    }

                    if (dictionaryKeys.Contains(dictKeyStr))
                    {
                        continue;
                    }

                    dictionaryKeys.Add(dictKeyStr);

                    var innerBindingContext = new ModelBindingContext
                    {
                        ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, valueType),
                        ModelName = key.Substring(0, endbracket + 1),
                        ModelState = bindingContext.ModelState,
                        PropertyFilter = bindingContext.PropertyFilter,
                        ValueProvider = bindingContext.ValueProvider
                    };
                    dynamic newPropertyValue = valueBinder.BindModel(controllerContext, innerBindingContext);

                    result = result ?? this.CreateModel(controllerContext, bindingContext, metaData.DictType);

                    try
                    {
                        if (result.ContainsKey(dictKey))
                            result[dictKey] = newPropertyValue;
                        else
                            result.Add(dictKey, newPropertyValue);
                    }
                    catch (RuntimeBinderException)
                    {
                        metaData.DictionaryMethodThrew.TryAdd(typePair, new EmptyStruct());
                    }
                }
            }

            if (result == null)
                return null;

            if (metaData.ListType != null)
            {
                // Here is where we convert back to a list.
                var collectionResult = (IEnumerable)result.Values;
                dynamic listObject = Activator.CreateInstance(metaData.ListType);
                foreach (dynamic item in collectionResult)
                    listObject.Add(item);

                if (metaData.ToArray)
                    return listObject.ToArray();

                return listObject;
            }

            return result;
        }

        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            return base.CreateModel(controllerContext, bindingContext, modelType);
        }

        /// <summary>
        /// Converts the given string to the destination type using TypeDescriptor class.
        /// </summary>
        /// <param name="stringValue">String representing the value of the given type.</param>
        /// <param name="type">The type of data represented as a string.</param>
        /// <returns>Returns the object of the given type, containing the value represented by the given string value.</returns>
        protected virtual object ConvertType(string stringValue, Type type)
        {
            return TypeDescriptor.GetConverter(type).ConvertFrom(stringValue);
        }

        /// <summary>
        /// Get the keys of the value provider in the given controller context.
        /// </summary>
        /// <param name="controllerContext">The context within which the controller operates. The context information includes the controller, HTTP content, request context, and route data.</param>
        /// <returns>Returns a list of keys representing available values.</returns>
        private static IEnumerable<string> GetValueProviderKeys(ControllerContext controllerContext)
        {
            var contextItems = controllerContext.HttpContext.Items;

            if (!contextItems.Contains(GetValueProviderKeys_CacheKey))
            {
                var keys = new List<string>();
                keys.AddRange(controllerContext.HttpContext.Request.Form.Keys.Cast<string>());
                keys.AddRange(((IDictionary<string, object>)controllerContext.RouteData.Values).Keys);
                keys.AddRange(controllerContext.HttpContext.Request.QueryString.Keys.Cast<string>());
                keys.AddRange(controllerContext.HttpContext.Request.Files.Keys.Cast<string>());
                contextItems[GetValueProviderKeys_CacheKey] = keys;
            }

            return (IEnumerable<string>)contextItems[GetValueProviderKeys_CacheKey];
        }

        /// <summary>
        /// Creates metadata for a given model type.
        /// </summary>
        /// <param name="modelType">Model type to create metadata for.</param>
        /// <returns>Returns a metadata object containing information about the given model type.</returns>
        private static MetaData CreateMetaData(Type modelType)
        {
            Type dictType = null;
            Type listType = null;
            bool toArray = false;

            var listDictionaryGenericArgs = new List<Type[]>();

            // For collection classes, proceed as dictionary, then convert back to list.
            if (modelType.IsArray && modelType.GetArrayRank() == 1)
            {
                Type itemType = modelType.GetElementType();
                listType = typeof(List<>).MakeGenericType(itemType);
                var genericArgs = new[] { typeof(int), itemType };
                listDictionaryGenericArgs.Add(genericArgs);
                dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                toArray = true;
            }
            else if (modelType.IsGenericType)
            {
                TypeFilter typeFilter = (t, fc) => t.IsGenericType && t.GetGenericTypeDefinition() == (Type)fc;
                Type genericTypeDefinition = modelType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IDictionary<,>) || genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var genericArgs = modelType.GetGenericArguments();
                    listDictionaryGenericArgs.Add(genericArgs);
                    dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                }
                else if (genericTypeDefinition == typeof(IEnumerable<>)
                         || genericTypeDefinition == typeof(ICollection<>)
                         || genericTypeDefinition == typeof(IList<>)
                         || genericTypeDefinition == typeof(List<>))
                {
                    Type itemType = modelType.GetGenericArguments()[0];
                    listType = typeof(List<>).MakeGenericType(itemType);
                    var genericArgs = new[] { typeof(int), itemType };
                    listDictionaryGenericArgs.Add(genericArgs);
                    dictType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
                }
                else if (modelType.IsClass && !modelType.IsAbstract && !modelType.IsGenericTypeDefinition)
                {
                    Type[] implementedDictionaries = modelType.FindInterfaces(typeFilter, typeof(IDictionary<,>));
                    if (implementedDictionaries.Length > 0)
                    {
                        dictType = modelType;
                        listDictionaryGenericArgs.AddRange(
                            implementedDictionaries.Select(id => id.GetGenericArguments()));
                    }
                    else
                    {
                        Type[] implementedCollections = modelType.FindInterfaces(typeFilter, typeof(ICollection<>));
                        if (implementedCollections.Length > 0)
                        {
                            listType = modelType;
                            dictType = typeof(Dictionary<int, object>);
                            listDictionaryGenericArgs.AddRange(
                                implementedCollections.Select(ic => new[] { typeof(int), ic.GetGenericArguments()[0] }));
                        }
                    }
                }
            }

            if (listDictionaryGenericArgs.Count == 0)
                listDictionaryGenericArgs = null;

            var result = new MetaData(dictType, listType, listDictionaryGenericArgs, toArray);

            return result;
        }

        /// <summary>
        /// VOID struct without fields.
        /// Unfortunately .Net does not allow 0 size structs, and make them always 1 byte.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 0)]
        private struct EmptyStruct
        {
        }

        private struct TypePair
        {
            public readonly Type A;
            public readonly Type B;

            public TypePair(Type a, Type b)
            {
                this.A = a;
                this.B = b;
            }
        }

        private class MetaData
        {
            // This does not need thread synchronization... if value is wrong, the code just runs slower
            public readonly Type DictType;
            public readonly Type ListType;
            public readonly List<Type[]> ListDictionaryGenericArgs;
            public readonly bool ToArray;

            public readonly ConcurrentDictionary<TypePair, EmptyStruct> DictionaryMethodThrew
                = new ConcurrentDictionary<TypePair, EmptyStruct>(TypePairEqualityComparer.Instance);

            public MetaData()
            {
            }

            public MetaData(Type dictType, Type listType, List<Type[]> listDictionaryGenericArgs, bool toArray)
            {
                this.DictType = dictType;
                this.ListType = listType;
                this.ListDictionaryGenericArgs = listDictionaryGenericArgs;
                this.ToArray = toArray;
            }

            public bool TypeConverterThrew { get; set; }
        }

        private class TypePairEqualityComparer : IEqualityComparer<TypePair>
        {
            public static readonly TypePairEqualityComparer Instance = new TypePairEqualityComparer();

            public bool Equals(TypePair x, TypePair y)
            {
                return x.A == y.A && x.B == y.B;
            }

            public int GetHashCode(TypePair obj)
            {
                return obj.A.GetHashCode() ^ obj.B.GetHashCode();
            }
        }
    }
}