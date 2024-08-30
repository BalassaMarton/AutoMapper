using System.Collections.Immutable;

namespace AutoMapper.UnitTests.Mappers.ImmutableDictionaries;

public class When_mapping_to_IImmutableDictionary : ImmutableDictionaryMapperSpecBase<IImmutableDictionary<string, int>> 
{ 
}

public class When_mapping_to_ImmutableDictionary : ImmutableDictionaryMapperSpecBase<ImmutableDictionary<string, int>>
{
}

public abstract class ImmutableDictionaryMapperSpecBase<TImmutableDictionary> : AutoMapperSpecBase where TImmutableDictionary : IImmutableDictionary<string, int>
{
    public class Source
    {
        public Dictionary<string, int> Values { get; set; }
    }

    public class Destination
    {
        public TImmutableDictionary Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Destination>();
    });

    protected virtual KeyValuePair<string, int>[] GetSourceValues(IEnumerable<KeyValuePair<string, int>> source) => source.OrderBy(x => x.Key).ToArray();

    protected virtual KeyValuePair<string, int>[] GetDestinationValues(IEnumerable<KeyValuePair<string, int>> destination) => destination.OrderBy(x => x.Key).ToArray();

    [Fact]
    public void Should_map_dictionary_values()
    {
        var source = new Source
        {
            Values = new()
            {
                { "a", 1 },
                { "b", 2 },
                { "c", 3 },
                { "d", 4 },
            }
        };

        var dest = Mapper.Map<Destination>(source);

        GetDestinationValues(dest.Values).ShouldBe(GetSourceValues(source.Values));
    }
}
