using System;
using AutoMapper;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// This is a shim until the company can upgrade
    /// </summary>
    public class MapperConfiguration : IMapperConfiguration
    {
        public MapperConfiguration(Action<IMapperConfiguration> configure)
        {
            
        }

        public IMapper CreateMapper()
        {
            return DefaultMapper.Instance;
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return DefaultMapper.Instance.CreateMap<TSource, TDestination>();
        }
    }
}
