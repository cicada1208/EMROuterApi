using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using DomainRule.Clients;
using FluentValidation;
using Lib.Api.Utilities;
using Lib.Utilities;
using System.Reflection;

namespace DomainRule.AutofacModules
{
    public class DomainRuleModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UtilLocator>().SingleInstance();
            builder.RegisterType<ApiUtilLocator>().SingleInstance();
            builder.RegisterType<DBUtilLocator>().SingleInstance();

            var ExecutingAssembly = Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(ExecutingAssembly)
                   .Where(t => t.Name.EndsWith("S3Client"))
                   .SingleInstance();

            builder.RegisterAutoMapper(ExecutingAssembly);

            builder.RegisterAssemblyTypes(ExecutingAssembly)
                   .AsClosedTypesOf(typeof(IValidator<>))
                   .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(ExecutingAssembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(ExecutingAssembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .InstancePerLifetimeScope();
        }
    }
}
