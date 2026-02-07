namespace Extensions.Mapper;

public static class MapperExtensions
{
    public static TDestination MapTo<TDestination>(this object source)
    {
        if (source == null)
        {
            return default;
        }

        var mapper = MapperFactory.CreateMapper();
        return mapper.Map<TDestination>(source);
    }
}