using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Lib.Api.ModelBinding
{
    public class SettingConvertEmptyStringToNullMetadataProvider : IMetadataDetailsProvider, IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            // 可解決複雜類型、簡單類型 (form-data、query string) 空字串轉為 null 問題
            if (context.Key.MetadataKind == ModelMetadataKind.Property ||
                context.Key.MetadataKind == ModelMetadataKind.Parameter)
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
