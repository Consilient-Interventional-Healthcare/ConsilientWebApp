using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Consilient.Api.Infra
{
    public class YyyyMmDdDateModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var value = valueResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                // no value; leave as not bound so regular validation can handle required checks
                return Task.CompletedTask;
            }

            if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                bindingContext.Result = ModelBindingResult.Success(parsed);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Date must be in yyyyMMdd format.");
            }

            return Task.CompletedTask;
        }
    }
}