using AutoMapper;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// This is a shim until the company can upgrade
    /// </summary>
    public interface IMapper
    {
        TDestination Map<TDestination>(object source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        IConfigurationProvider ConfigurationProvider { get; }
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
    }
}
