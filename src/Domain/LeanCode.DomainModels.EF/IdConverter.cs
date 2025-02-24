using System;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeanCode.DomainModels.EF;

public class IdConverter<T> : ValueConverter<Id<T>, Guid>
    where T : class, IIdentifiable<Id<T>>
{
    public static readonly IdConverter<T> Instance = new();

    private IdConverter()
        : base(d => d.Value, d => new Id<T>(d), mappingHints: null) { }
}

public class IIdConverter<T> : ValueConverter<IId<T>, int>
    where T : class, IIdentifiable<IId<T>>
{
    public static readonly IIdConverter<T> Instance = new();

    private IIdConverter()
        : base(d => d.Value, d => new IId<T>(d), mappingHints: null) { }
}

public class LIdConverter<T> : ValueConverter<LId<T>, long>
    where T : class, IIdentifiable<LId<T>>
{
    public static readonly LIdConverter<T> Instance = new();

    private LIdConverter()
        : base(d => d.Value, d => new LId<T>(d), mappingHints: null) { }
}

public class SIdConverter<T> : ValueConverter<SId<T>, string>
    where T : class, IIdentifiable<SId<T>>
{
    public static readonly SIdConverter<T> Instance = new();

    private SIdConverter()
        : base(d => d.Value, d => SId<T>.From(d), mappingHints: null) { }
}
