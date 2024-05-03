using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.ExceptionServices;

namespace Lib.Api.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinder"/> for string types.
    /// </summary>
    /// <remarks>https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Mvc/Mvc.Core/src/ModelBinding/Binders/SimpleTypeModelBinder.cs</remarks>
    public class StringModelBinder : IModelBinder
    {
        private readonly TypeConverter _typeConverter;

        /// <summary>
        /// Initializes a new instance of <see cref="StringModelBinder"/>.
        /// </summary>
        /// <param name="type">The type to create binder for.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public StringModelBinder(Type type, ILoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(loggerFactory);

            _typeConverter = TypeDescriptor.GetConverter(type);
        }

        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                // no entry
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            try
            {
                var value = bindingContext.ModelMetadata.IsFlagsEnum
                    ? valueProviderResult.Values.ToString()
                    : valueProviderResult.FirstValue;

                object? model;
                if (bindingContext.ModelType == typeof(string))
                {
                    // Already have a string. No further conversion required but handle ConvertEmptyStringToNull.
                    // 自定義修改：IsNullOrWhiteSpace 改為 IsNullOrEmpty，使 Model Binding 可接收空格
                    if (bindingContext.ModelMetadata.ConvertEmptyStringToNull && string.IsNullOrEmpty(value))
                    {
                        model = null;
                    }
                    else
                    {
                        model = value;
                    }
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    // Other than the StringConverter, converters Trim() the value then throw if the result is empty.
                    model = null;
                }
                else
                {
                    model = _typeConverter.ConvertFrom(
                        context: null,
                        culture: valueProviderResult.Culture,
                        value: value);
                }

                CheckModel(bindingContext, valueProviderResult, model);
            }
            catch (Exception exception)
            {
                var isFormatException = exception is FormatException;
                if (!isFormatException && exception.InnerException != null)
                {
                    // TypeConverter throws System.Exception wrapping the FormatException,
                    // so we capture the inner exception.
                    exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                }

                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    exception,
                    bindingContext.ModelMetadata);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// If the <paramref name="model" /> is <see langword="null" />, verifies that it is allowed to be <see langword="null" />,
        /// otherwise notifies the <see cref="P:ModelBindingContext.ModelState" /> about the invalid <paramref name="valueProviderResult" />.
        /// Sets the <see href="P:ModelBindingContext.Result" /> to the <paramref name="model" /> if successful.
        /// </summary>
        protected virtual void CheckModel(
            ModelBindingContext bindingContext,
            ValueProviderResult valueProviderResult,
            object? model)
        {
            // When converting newModel a null value may indicate a failed conversion for an otherwise required
            // model (can't set a ValueType to null). This detects if a null model value is acceptable given the
            // current bindingContext. If not, an error is logged.
            if (model == null && !bindingContext.ModelMetadata.IsReferenceOrNullableType)
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                        valueProviderResult.ToString()));
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }
        }
    }
}
