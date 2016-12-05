using AutoMapper;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// This is a shim until the company can upgrade
    /// </summary>
    public interface IMapperConfiguration
    {
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
    }
}
