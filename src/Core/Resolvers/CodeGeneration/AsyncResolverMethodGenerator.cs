using System.Linq;
using System.Text;

namespace HotChocolate.Resolvers.CodeGeneration
{
    internal sealed class AsyncResolverMethodGenerator
        : SourceCodeGenerator
    {
        protected override void GenerateResolverInvocation(
            FieldResolverDescriptor resolverDescriptor, StringBuilder source)
        {
            source.AppendLine($"var resolver = ctx.{nameof(IResolverContext.Service)}<{resolverDescriptor.ResolverType.FullName}>();");
            source.Append($"return resolver.{resolverDescriptor.Member.Name} (");

            if (resolverDescriptor.ArgumentDescriptors.Any())
            {
                string arguments = string.Join(", ",
                    resolverDescriptor.ArgumentDescriptors.Select(t => t.Name));
                source.Append(arguments);
            }

            source.Append(");");
        }

        public override bool CanGenerate(
            FieldResolverDescriptor resolverDescriptor)
                => resolverDescriptor.IsAsync
                    && resolverDescriptor.IsMethod
                    && resolverDescriptor.Kind == FieldResolverKind.Collection;
    }
}
