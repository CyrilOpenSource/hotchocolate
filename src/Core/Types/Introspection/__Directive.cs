using System.Collections.Generic;
using HotChocolate.Resolvers;

namespace HotChocolate.Types.Introspection
{
    internal sealed class __Directive
        : ObjectType
    {
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Description(
                "A Directive provides a way to describe alternate runtime execution and " +
                "type validation behavior in a GraphQL document.\n\n" +
                "In some cases, you need to provide options to alter GraphQL's " +
                "execution behavior in ways field arguments will not suffice, such as " +
                "conditionally including or skipping a field. Directives provide this by " +
                "describing additional information to the executor.");

            descriptor.Field("name")
                .Type<NonNullType<StringType>>()
                .Resolver(c => c.Parent<Directive>().Name);

            descriptor.Field("description")
                .Type<StringType>()
                .Resolver(c => c.Parent<Directive>().Description);

            descriptor.Field("locations")
                .Type<NonNullType<ListType<NonNullType<__DirectiveLocation>>>>()
                .Resolver(c => c.Parent<Directive>().Locations);

            descriptor.Field("args")
                .Type<NonNullType<ListType<NonNullType<__Type>>>>()
                .Resolver(c => c.Parent<Directive>().Arguments);

            descriptor.Field("onOperation")
                .Type<NonNullType<BooleanType>>()
                .Resolver(c => GetOnOperation(c))
                .DeprecationReason("Use `locations`.");

            descriptor.Field("onFragment")
                .Type<NonNullType<BooleanType>>()
                .Resolver(c => GetOnFragment(c))
                .DeprecationReason("Use `locations`.");

            descriptor.Field("onField")
                .Type<NonNullType<BooleanType>>()
                .Resolver(c => GetOnField(c))
                .DeprecationReason("Use `locations`.");
        }

        private static bool GetOnOperation(IResolverContext context)
        {
            IReadOnlyCollection<DirectiveLocation> locations =
                context.Parent<Directive>().Locations;

            return Contains(locations, DirectiveLocation.Query)
                || Contains(locations, DirectiveLocation.Mutation)
                || Contains(locations, DirectiveLocation.Subscription);
        }

        private static bool GetOnFragment(IResolverContext context)
        {
            IReadOnlyCollection<DirectiveLocation> locations =
                context.Parent<Directive>().Locations;

            return Contains(locations, DirectiveLocation.InlineFragment)
                || Contains(locations, DirectiveLocation.FragmentSpread)
                || Contains(locations, DirectiveLocation.FragmentDefinition);
        }

        private static bool GetOnField(IResolverContext context)
        {
            IReadOnlyCollection<DirectiveLocation> locations =
                context.Parent<Directive>().Locations;

            return Contains(locations, DirectiveLocation.Field);
        }

        private static bool Contains<T>(IReadOnlyCollection<T> collection, T item)
        {
            foreach (T element in collection)
            {
                if (element.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
