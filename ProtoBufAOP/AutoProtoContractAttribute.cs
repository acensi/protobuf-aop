using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using ProtoBuf;

namespace ProtoBufAOP
{
    [Serializable]
    [MulticastAttributeUsage(Inheritance = MulticastInheritance.None, AllowMultiple = false)]
    public sealed class AutoProtoContractAttribute : Attribute, IAspectProvider
    {
        private static List<AspectInstance> ProvideAspects(Type targetType, int startTag, out int endTag)
        {
            var aspects = new List<AspectInstance>
            {
                GetProtoContract(targetType)
            };

            aspects.AddRange(targetType.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance)
                           .Where(property => property.CanWrite)
                           .Select(property => GetProtoMember(property, startTag++)));

            endTag = startTag;

            return aspects;
        }

        private static AspectInstance GetProtoMember(PropertyInfo targetType, int tag)
        {
            var ctor = new ObjectConstruction(typeof(ProtoMemberAttribute).GetConstructor(new Type[] { typeof(int) }), new object[] { tag });
            return new AspectInstance(targetType, new CustomAttributeIntroductionAspect(ctor));
        }

        private static AspectInstance GetProtoContract(Type targetType)
        {
            var ctor = new ObjectConstruction(typeof(ProtoContractAttribute).GetConstructor(Type.EmptyTypes));
            return new AspectInstance(targetType, new CustomAttributeIntroductionAspect(ctor));
        }

        private static AspectInstance GetProtoInclude(Type baseType, Type targetType, int tag)
        {
            var ctor = new ObjectConstruction(typeof(ProtoIncludeAttribute).GetConstructor(new Type[] { typeof(int), typeof(Type) }), new object[] { tag, targetType });
            return new AspectInstance(baseType, new CustomAttributeIntroductionAspect(ctor));
        }

        private IEnumerable<Type> GerFirstLevelDerivedType(Type targetType)
        {
            var derivedTypes = targetType.Assembly.GetTypes()
                                                    .Where(t =>
                                                            t != targetType &&
                                                            t.BaseType == targetType);
            return derivedTypes;
        }

        public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
        {
            var targetType = (Type)targetElement;
            int tag = 1;

            var aspects = ProvideAspects(targetType, tag, out tag);
            foreach (var dt in GerFirstLevelDerivedType(targetType))
            {
                var protoInclude = GetProtoInclude(targetType, dt, tag++);
                if (protoInclude != null)
                {
                    aspects.Add(protoInclude);
                }
                aspects.AddRange(ProvideAspects(dt));
            }

            return aspects;
        }
    }
}
