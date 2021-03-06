using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate.Configuration;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Introspection;

namespace HotChocolate
{
    public partial class Schema
    {
        public static Schema Create(
           string schema,
           Action<ISchemaConfiguration> configure)
        {
            return Create(schema, configure, false);
        }

        public static Schema Create(
           string schema,
           Action<ISchemaConfiguration> configure,
           IServiceProvider services)
        {
            return Create(schema, configure, false, services);
        }

        public static Schema Create(
            string schema,
            Action<ISchemaConfiguration> configure,
            bool strict)
        {
            return Create(schema, configure, strict, new DefaultServiceProvider());
        }

        public static Schema Create(
            string schema,
            Action<ISchemaConfiguration> configure,
            bool strict,
            IServiceProvider services)
        {
            if (string.IsNullOrEmpty(schema))
            {
                throw new ArgumentNullException(nameof(schema));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return Create(Parser.Default.Parse(schema), configure, strict);
        }

        public static Schema Create(
            DocumentNode schemaDocument,
            Action<ISchemaConfiguration> configure)
        {
            if (schemaDocument == null)
            {
                throw new ArgumentNullException(nameof(schemaDocument));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return Create(schemaDocument, configure, new DefaultServiceProvider());
        }

        public static Schema Create(
            DocumentNode schemaDocument,
            Action<ISchemaConfiguration> configure,
            IServiceProvider services)
        {
            if (schemaDocument == null)
            {
                throw new ArgumentNullException(nameof(schemaDocument));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return Create(schemaDocument, configure, false, services);
        }

        public static Schema Create(
            DocumentNode schemaDocument,
            Action<ISchemaConfiguration> configure,
            bool strict)
        {
            if (schemaDocument == null)
            {
                throw new ArgumentNullException(nameof(schemaDocument));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return Create(schemaDocument, configure, strict, new DefaultServiceProvider());
        }

        public static Schema Create(
            DocumentNode schemaDocument,
            Action<ISchemaConfiguration> configure,
            bool strict,
            IServiceProvider services)
        {
            if (schemaDocument == null)
            {
                throw new ArgumentNullException(nameof(schemaDocument));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            SchemaContext context = CreateSchemaContext();

            // deserialize schema objects
            SchemaSyntaxVisitor visitor = new SchemaSyntaxVisitor(context.Types);
            visitor.Visit(schemaDocument);

            SchemaNames names = new SchemaNames(
                visitor.QueryTypeName,
                visitor.MutationTypeName,
                visitor.SubscriptionTypeName);

            return CreateSchema(services, context, names, configure, strict);
        }

        public static Schema Create(
            Action<ISchemaConfiguration> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return Create(configure, false);
        }

        public static Schema Create(
            Action<ISchemaConfiguration> configure,
            IServiceProvider services)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return Create(configure, false, services);
        }

        public static Schema Create(
            Action<ISchemaConfiguration> configure,
            bool strict)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return Create(configure, strict, new DefaultServiceProvider());
        }

        public static Schema Create(
            Action<ISchemaConfiguration> configure,
            bool strict,
            IServiceProvider services)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            SchemaContext context = CreateSchemaContext();
            return CreateSchema(services, context,
                default(SchemaNames), configure, strict);
        }

        private static Schema CreateSchema(
            IServiceProvider services,
            SchemaContext context,
            SchemaNames names,
            Action<ISchemaConfiguration> configure,
            bool strict)
        {
            List<SchemaError> errors = new List<SchemaError>();

            // setup introspection fields
            IntrospectionFields introspectionFields =
                new IntrospectionFields(context, e => errors.Add(e));

            SchemaNames internalNames = names;

            try
            {
                // configure resolvers, custom types and type mappings.
                SchemaConfiguration configuration = new SchemaConfiguration(services);
                configure(configuration);
                errors.AddRange(configuration.RegisterTypes(context));
                configuration.RegisterResolvers(context);
                errors.AddRange(context.CompleteTypes());

                string queryTypeName = configuration.QueryTypeName ?? names.QueryTypeName;
                string mutationTypeName = configuration.MutationTypeName ?? names.MutationTypeName;
                string subscriptionTypeName = configuration.SubscriptionTypeName ?? names.SubscriptionTypeName;

                internalNames = new SchemaNames(queryTypeName, mutationTypeName, subscriptionTypeName);
            }
            catch (ArgumentException ex)
            {
                // TODO : maybe we should throw a more specific
                // argument exception that at least contains the config object.
                throw new SchemaException(new[]
                {
                    new SchemaError(ex.Message, null)
                });
            }

            if (strict && errors.Any())
            {
                throw new SchemaException(errors);
            }

            internalNames = string.IsNullOrEmpty(names.QueryTypeName)
                ? new SchemaNames(null, null, null)
                : names;

            if (strict && !context.Types.TryGetType<ObjectType>(
                internalNames.QueryTypeName, out ObjectType ot))
            {
                throw new SchemaException(new SchemaError(
                    "Schema is missing the mandatory `Query` type."));
            }

            return new Schema(
                services,
                SchemaTypes.Create(
                    context.Types.GetTypes(),
                    context.Types.GetTypeBindings(),
                    internalNames),
                introspectionFields);
        }

        private static SchemaContext CreateSchemaContext()
        {
            // create context with system types
            SchemaContext context = new SchemaContext();
            context.Types.RegisterType(typeof(StringType));
            context.Types.RegisterType(typeof(BooleanType));
            context.Types.RegisterType(typeof(IntType));

            // register introspection types
            context.Types.RegisterType(typeof(__Directive));
            context.Types.RegisterType(typeof(__DirectiveLocation));
            context.Types.RegisterType(typeof(__EnumValue));
            context.Types.RegisterType(typeof(__Field));
            context.Types.RegisterType(typeof(__InputValue));
            context.Types.RegisterType(typeof(__Schema));
            context.Types.RegisterType(typeof(__Type));
            context.Types.RegisterType(typeof(__TypeKind));

            return context;
        }
    }
}
