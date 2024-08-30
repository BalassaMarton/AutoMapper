using System.Collections.Immutable;

namespace AutoMapper.UnitTests.Mappers.ImmutableCollections;

public class When_mapping_to_ImmutableArray : ImmutableCollectionMapperSpecBase<IImmutableList<int>>
{
}

public class When_mapping_to_IImmutableList : ImmutableCollectionMapperSpecBase<IImmutableList<int>>
{
}

public class When_mapping_to_ImmutableList : ImmutableCollectionMapperSpecBase<IImmutableList<int>>
{
}

public class When_mapping_to_IImmutableQueue : ImmutableCollectionMapperSpecBase<IImmutableQueue<int>>
{
}

public class When_mapping_to_ImmutableQueue : ImmutableCollectionMapperSpecBase<ImmutableQueue<int>>
{
}

public class When_mapping_to_IImmutableStack : ImmutableStackMapperSpecBase<IImmutableStack<int>>
{
}

public class When_mapping_to_ImmutableStack : ImmutableStackMapperSpecBase<ImmutableStack<int>>
{
}

public class When_mapping_to_IImmutableSet : ImmutableSetMapperSpecBase<IImmutableSet<int>>
{
}

public class When_mapping_to_ImmutableHashSet : ImmutableSetMapperSpecBase<ImmutableHashSet<int>>
{
}

public abstract class ImmutableCollectionMapperSpecBase<TImmutableCollection> : AutoMapperSpecBase where TImmutableCollection : IEnumerable<int>
{
    public class Source
    {
        public int[] Values { get; set; }
    }

    public class Destination
    {
        public TImmutableCollection Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Destination>();
    });

    protected virtual int[] GetSourceValues(IEnumerable<int> source) => source.ToArray();

    protected virtual int[] GetDestinationValues(IEnumerable<int> destination) => destination.ToArray();

    [Fact]
    public void Should_map_collection_values()
    {
        var source = new Source
        {
            Values = [1, 2, 3, 4]
        };

        var dest = Mapper.Map<Destination>(source);

        GetDestinationValues(dest.Values).ShouldBe(GetSourceValues(source.Values));
    }
}

public abstract class ImmutableSetMapperSpecBase<TImmutableCollection> : ImmutableCollectionMapperSpecBase<TImmutableCollection> where TImmutableCollection : IEnumerable<int>, IImmutableSet<int>
{
    protected override int[] GetSourceValues(IEnumerable<int> source) => source.OrderBy(x => x).ToArray();

    protected override int[] GetDestinationValues(IEnumerable<int> destination) => destination.OrderBy(x => x).ToArray();
}

public abstract class ImmutableStackMapperSpecBase<TImmutableCollection> : ImmutableCollectionMapperSpecBase<TImmutableCollection> where TImmutableCollection : IEnumerable<int>, IImmutableStack<int>
{
    protected override int[] GetSourceValues(IEnumerable<int> source) => source.OrderBy(x => x).ToArray();

    protected override int[] GetDestinationValues(IEnumerable<int> destination) => destination.OrderBy(x => x).ToArray();
}