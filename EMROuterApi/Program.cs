using Autofac;
using Autofac.Extensions.DependencyInjection;
using DomainRule.AutofacModules;
using FluentValidation;
using Lib.Api.Configs;
using Lib.Api.Middlewares;
using Lib.Api.ModelBinding;
using Lib.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NLog.Web;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    #region Initial settings

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // 使該 Api 可用 big5

    // FluentValidation: Global settings
    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
    ValidatorOptions.Global.DisplayNameResolver = (type, mi, expression) =>
    {
        string result = string.Empty;
        if (mi != null)
            result = mi.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? mi.Name;
        return result;
    };

    #endregion

    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Autofac: Registration
    builder.Host
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>((HostContext, builder) =>
        {
            builder.RegisterModule(new DomainRuleModule());
        });

    #region Add services to the container.

    builder.Services.Configure<AppSettings>(builder.Configuration);

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        // 停用預設的 ModelStateInvalidFilter，
        // 改於 ApiActionFilterAttribute 處理 Model Validation Errors 的統一回傳格式。
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        // Use the default property casing
        options.SerializerOptions.PropertyNamingPolicy = null;
        options.SerializerOptions.DictionaryKeyPolicy = null;

        options.SerializerOptions.WriteIndented = true;
    });

    builder.Services
        .AddControllers(options =>
        {
            options.AllowEmptyInputInBodyModelBinding = true;
            // 取消 StringModelBinderProvider 改用 SettingConvertEmptyStringToNullMetadataProvider
            //options.ModelBinderProviders.Insert(0, new StringModelBinderProvider());
            options.ModelMetadataDetailsProviders.Add(
               new SettingConvertEmptyStringToNullMetadataProvider());
        })
        .AddJsonOptions(options =>
        {
            // System.Text.Json: 官方取代 Newtonsoft Json.NET 的解決方案

            // 允許基本拉丁英文及中日韓文字維持原字元
            //options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

            // Use the default property casing
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.DictionaryKeyPolicy = null;

            options.JsonSerializerOptions.WriteIndented = true;
            //options.JsonSerializerOptions.IgnoreNullValues = true;
        });
    //.AddNewtonsoftJson(options =>
    //{
    //    // Newtonsoft Json.NET: 預設即可於 Response 維持原中文內容

    //    // Use the default property casing
    //    //options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    //    options.UseMemberCasing();

    //    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
    //});

    var appSettings = builder.Configuration.Get<AppSettings>() ?? new();
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearerConfig(appSettings);

    string cychCorsPolicy = "CychCorsPolicy";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(cychCorsPolicy, builder =>
        {
            builder
                // 設定允許跨域的來源，有多個的話可用 `,` 隔開
                //.WithOrigins("http://*.cych.org.tw", "https://*.cych.org.tw", "*")
                //.SetIsOriginAllowedToAllowWildcardSubdomains()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    // 改用 Autofac
    //builder.Services.AddValidatorsFromAssemblyContaining<EMROuterValidator>();
    //builder.Services.AddSingleton<UtilLocator>();
    //builder.Services.AddSingleton<ApiUtilLocator>();
    //builder.Services.AddSingleton<OtherHospitalS3Client>();
    //builder.Services.AddScoped<DBContext>();
    //builder.Services.AddScoped<AuthService>();
    //builder.Services.AddScoped<UploadService>();

    #endregion

    var app = builder.Build();

    #region Configure the HTTP request pipeline.

    app.UseLoggerMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors(cychCorsPolicy);

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    #endregion

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}