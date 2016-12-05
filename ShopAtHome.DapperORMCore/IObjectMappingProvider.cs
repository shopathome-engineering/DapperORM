using AutoMapper;

namespace ShopAtHome.DapperORMCore
{
    public interface IObjectMappingProvider
    {
        void ApplyMappings(IMapperConfiguration configuration);
    }
}
