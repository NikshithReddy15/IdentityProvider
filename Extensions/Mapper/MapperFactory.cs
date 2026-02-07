using AutoMapper;

namespace Extensions.Mapper;

public static class MapperFactory
{
    private static IServiceProvider _serviceProvider;

    public static void InitializeServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static IMapper CreateMapper()
    {
        return _serviceProvider.GetService(typeof(IMapper)) as IMapper;
    }
}