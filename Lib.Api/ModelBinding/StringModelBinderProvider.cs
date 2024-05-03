using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.Api.ModelBinding
{
    public class StringModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Metadata.ModelType == typeof(string)) // 自定義修改：僅處理 string
            {
                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new StringModelBinder(context.Metadata.ModelType, loggerFactory);
            }

            return null;
        }
    }
}
