using Consilient.Data.GraphQL.Contracts;
using EntityGraphQL.AspNet;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.Data.GraphQL
{
    public static class GraphQlServiceCollectionExtensions
    {
        public static void RegisterGraphQlServices(this IServiceCollection services)
        {
            services.AddScoped<IGraphQlService, GraphQlService>();
            services.AddGraphQLSchema<ConsilientDbContext>(options =>
            {
                options.AutoBuildSchemaFromContext = false;
                options.AutoCreateEnumTypes = true;
                options.AutoCreateFieldWithIdArguments = false;
                options.AutoCreateInputTypes = false;
                options.AutoCreateInterfaceTypes = false;
                options.AutoCreateNewComplexTypes = false;
                options.AutoCreateNewComplexTypes = false;
                options.PreBuildSchemaFromContext = schema =>
                {
                    if (!schema.HasType(typeof(DateOnly)))
                    {
                        schema.AddScalarType<DateOnly>("DateOnly", "Date value only scalar");
                    }
                };
                options.ConfigureSchema = schema =>
                {
                    GraphQlSchemaConfigurator.ConfigureSchema(schema);
                };
            });
        }

    }
}
