using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Consilient.Api.Infra
{
    public class YyyyMmDdDateModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var t = context.Metadata.ModelType;
            if (t == typeof(DateTime) || t == typeof(DateTime?))
            {
                // Use BinderTypeModelBinder so DI can resolve the actual binder if needed
                return new BinderTypeModelBinder(typeof(YyyyMmDdDateModelBinder));
            }

            return null;
        }
    }
}