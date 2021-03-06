using System;

namespace HotChocolate.Configuration
{
    internal class BindResolver<TResolver>
        : IBindResolver<TResolver>
        where TResolver : class
    {
        private readonly ResolverCollectionBindingInfo _bindingInfo;

        public BindResolver(ResolverCollectionBindingInfo bindingInfo)
        {
            if (bindingInfo == null)
            {
                throw new ArgumentNullException(nameof(bindingInfo));
            }

            _bindingInfo = bindingInfo;
        }

        public IBindFieldResolver<TResolver> Resolve(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            FieldResolverBindungInfo fieldBindingInfo =
                new FieldResolverBindungInfo
                {
                    FieldName = fieldName
                };
            _bindingInfo.Fields.Add(fieldBindingInfo);
            return new BindFieldResolver<TResolver>(
                _bindingInfo, fieldBindingInfo);
        }

        public IBoundResolver<TResolver> To(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            _bindingInfo.ObjectTypeName = typeName;
            return this;
        }

        public IBoundResolver<TResolver, TObjectType> To<TObjectType>()
            where TObjectType : class
        {
            _bindingInfo.ObjectType = typeof(TObjectType);
            return new BoundResolver<TResolver, TObjectType>(_bindingInfo);
        }
    }
}
