using AutoMapper;

namespace ShopAtHome.DapperORMCore
{
    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// This is a shim until the company can upgrade
    /// </summary>
    public class DefaultMapper : IMapper
    {
        private static DefaultMapper _instance;

        public TDestination Map<TDestination>(object source)
        {
            return Mapper.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Mapper.Map(source, destination);
        }

        public IConfigurationProvider ConfigurationProvider => Mapper.Engine.ConfigurationProvider;

        public static DefaultMapper Instance => _instance ?? (_instance = new DefaultMapper());

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Mapper.CreateMap<TSource, TDestination>();
        }
    }

    /// <summary>
    /// We downgraded from Automapper 4, which contained this instance API, to 3, which only exposes statics.
    /// This is a shim until the company can upgrade
    /// </summary>
    public static class MapperExtensions
    {
        public static TypeMap FindTypeMapFor<TSource, TDestination>(this IConfigurationProvider provider)
        {
            return DefaultMapper.Instance.ConfigurationProvider.FindTypeMapFor(typeof (TSource), typeof (TDestination));
        }
    }
}
