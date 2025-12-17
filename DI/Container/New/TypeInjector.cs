using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
namespace BB.Di
{
    public sealed class TypeInjector
    {
        public readonly Type _type;
        public readonly List<IElementInjector> _elementInjectors = new();
        public readonly List<string> _errors = new();
        public TypeInjector(Type type)
        {
            _type = type;
            if (type.IsInterface)
                return;
            var currentType = type;
            while (currentType != typeof(object))
            {
                var members = currentType.GetMembers(
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);

                foreach (var member in members)
                {
                    if (member.DeclaringType != currentType)
                        continue;
                    switch (member)
                    {
                        case FieldInfo fieldInfo:
                            {

                                if (fieldInfo.HasAttribute(out InjectAttribute fieldAttribute))
                                    _elementInjectors.Add(new FieldElementInjector
                                    {
                                        Field = fieldInfo,
                                        Attribute = fieldAttribute
                                    });
                            }
                            break;
                        case PropertyInfo propertyInfo:
                            {
                                if (propertyInfo.HasAttribute(out InjectAttribute propertyAttribute))
                                    _elementInjectors.Add(new PropertyElementInjector
                                    {
                                        Property = propertyInfo,
                                        Attribute = propertyAttribute
                                    });
                            }
                            break;

                        case MethodInfo methodInfo:
                            {
                                if (methodInfo.HasAttribute(out OnEventAttribute eventAttribute))
                                {
                                    var eventTypes = GetValidEventTypesForSubscription(
                                        methodInfo,
                                        eventAttribute);

                                    if (eventTypes != null)
                                        foreach (var eventType in eventTypes)
                                            _elementInjectors.Add(
                                                new EventSubscriptionInjector(
                                                    methodInfo,
                                                    eventType,
                                                    eventAttribute));
                                }
                                //if (methodInfo.HasAttribute(out OnUpdateAttribute updateAttribute))
                                //{
                                //    if (methodInfo.ReturnType != typeof(void))
                                //    {
                                //        _errors.Add(
                                //            $"Can't subscribe method {methodInfo.Name} to OnUpdate. " +
                                //            $"Return type must be void.");
                                //        break;
                                //    }

                                //    var args = methodInfo.GetParameters();
                                //    if (args.Length != 1)
                                //    {
                                //        _errors.Add(
                                //            $"Can't subscribe method {methodInfo.Name} to OnUpdate. " +
                                //            $"Must have only 1 parameter of type {typeof(UpdateTime).Name}.");
                                //        break;
                                //    }

                                //    if (args[0].ParameterType != typeof(UpdateTime))
                                //    {
                                //        _errors.Add(
                                //           $"Can't subscribe method {methodInfo.Name} to OnUpdate. " +
                                //           $"Parameter must be of type {typeof(UpdateTime).Name}.");
                                //        break;
                                //    }

                                //    _elementInjectors.Add(new UpdateSubscriptionInjector
                                //    {
                                //        Attribute = updateAttribute,
                                //        Method = methodInfo
                                //    });
                                //}
                            }
                            break;
                    }
                }
                currentType = currentType.BaseType;
            }
        }
        Type[] GetValidEventTypesForSubscription(MethodInfo method, OnEventAttribute attribute)
        {
            var args = method.GetParameters();
            if (method.ReturnType == typeof(void))
            {
                switch (args.Length)
                {
                    case 0:
                        return GetEventTypesFromAttribute();
                    case 1:
                        if (args[0].ParameterType == typeof(CancellationToken))
                        {
                            _errors.Add(
                                $"Can't subscribe method {method.Name} to OnEvent. " +
                                $"Event type can not be CancellationToken.");
                            return null;
                        }
                        return new[] { args[0].ParameterType };
                    default:
                        _errors.Add(
                                $"Can't subscribe method {method.Name} to OnEvent. " +
                                $"Methods with return type void can have at most 1 parameter");
                        return null;
                }
            }

            if (method.ReturnType == typeof(UniTaskVoid))
            {
                switch (args.Length)
                {
                    case 0:
                        return GetEventTypesFromAttribute();
                    case 1:
                        if (args[0].ParameterType == typeof(CancellationToken))
                            return GetEventTypesFromAttribute();
                        return new[] { args[0].ParameterType };
                    case 2:
                        if (args[0].ParameterType == typeof(CancellationToken)
                            || args[1].ParameterType != typeof(CancellationToken))
                        {
                            _errors.Add(
                                    $"Can't subscribe method {method.Name} to OnEvent. " +
                                    $"CancelationToken must be set as last param.");
                            return null;
                        }
                        return new[] { args[0].ParameterType };
                    default:
                        _errors.Add(
                                $"Can't subscribe method {method.Name} to OnEvent. " +
                                $"Methods with return type UniTaskVoid can have at most 2 parameters");
                        return null;
                }
            }
            _errors.Add($"Can't subscribe method {method.Name} to OnEvent. " +
                $"Methods can only have return type void or UniTaskVoid");
            return null;

            Type[] GetEventTypesFromAttribute()
            {
                if (attribute._eventTypes.IsNullOrEmpty())
                {
                    _errors.Add(
                        $"Can't subscribe method {method.Name} to OnEvent. " +
                        $"No event type has been specified.");
                    return null;
                }
                return attribute._eventTypes;
            }
        }
    }
}