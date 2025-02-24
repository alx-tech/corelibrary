using LeanCode.DomainModels.Ids;
using LeanCode.DomainModels.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Id<T>> IsTypedId<T>(this PropertyBuilder<Id<T>> builder)
        where T : class, IIdentifiable<Id<T>>
    {
        return builder.HasConversion(IdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<Id<T>?> IsTypedId<T>(this PropertyBuilder<Id<T>?> builder)
        where T : class, IIdentifiable<Id<T>>
    {
        return builder.HasConversion(IdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<IId<T>> IsTypedId<T>(this PropertyBuilder<IId<T>> builder)
        where T : class, IIdentifiable<IId<T>>
    {
        return builder.HasConversion(IIdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<IId<T>?> IsTypedId<T>(this PropertyBuilder<IId<T>?> builder)
        where T : class, IIdentifiable<IId<T>>
    {
        return builder.HasConversion(IIdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<LId<T>> IsTypedId<T>(this PropertyBuilder<LId<T>> builder)
        where T : class, IIdentifiable<LId<T>>
    {
        return builder.HasConversion(LIdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<LId<T>?> IsTypedId<T>(this PropertyBuilder<LId<T>?> builder)
        where T : class, IIdentifiable<LId<T>>
    {
        return builder.HasConversion(LIdConverter<T>.Instance).ValueGeneratedNever();
    }

    public static PropertyBuilder<SId<T>> IsTypedId<T>(this PropertyBuilder<SId<T>> builder)
        where T : class, IIdentifiable<SId<T>>
    {
        return builder
            .HasConversion(SIdConverter<T>.Instance)
            .HasMaxLength(SId<T>.RawLength)
            .IsFixedLength()
            .ValueGeneratedNever();
    }

    public static PropertyBuilder<SId<T>?> IsTypedId<T>(this PropertyBuilder<SId<T>?> builder)
        where T : class, IIdentifiable<SId<T>>
    {
        return builder
            .HasConversion(SIdConverter<T>.Instance)
            .HasMaxLength(SId<T>.RawLength)
            .IsFixedLength()
            .ValueGeneratedNever();
    }

    public static PropertyBuilder<TId> IsIntTypedId<TId>(this PropertyBuilder<TId> builder)
        where TId : struct, IRawTypedId<int, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<int, TId>.Instance);
    }

    public static PropertyBuilder<TId?> IsIntTypedId<TId>(this PropertyBuilder<TId?> builder)
        where TId : struct, IRawTypedId<int, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<int, TId>.Instance);
    }

    public static PropertyBuilder<TId> IsLongTypedId<TId>(this PropertyBuilder<TId> builder)
        where TId : struct, IRawTypedId<long, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<long, TId>.Instance);
    }

    public static PropertyBuilder<TId?> IsLongTypedId<TId>(this PropertyBuilder<TId?> builder)
        where TId : struct, IRawTypedId<long, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<long, TId>.Instance);
    }

    public static PropertyBuilder<TId> IsGuidTypedId<TId>(this PropertyBuilder<TId> builder)
        where TId : struct, IRawTypedId<Guid, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<Guid, TId>.Instance);
    }

    public static PropertyBuilder<TId?> IsGuidTypedId<TId>(this PropertyBuilder<TId?> builder)
        where TId : struct, IRawTypedId<Guid, TId>
    {
        return builder.HasConversion(RawTypedIdConverter<Guid, TId>.Instance);
    }

    public static PropertyBuilder<TId> IsPrefixedTypedId<TId>(this PropertyBuilder<TId> builder)
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HasConversion(PrefixedTypedIdConverter<TId>.Instance)
            .HasMaxLength(TId.RawLength)
            .IsFixedLength()
            .ValueGeneratedNever();
    }

    public static PropertyBuilder<TId?> IsPrefixedTypedId<TId>(this PropertyBuilder<TId?> builder)
        where TId : struct, IPrefixedTypedId<TId>
    {
        return builder
            .HasConversion(PrefixedTypedIdConverter<TId>.Instance)
            .HasMaxLength(TId.RawLength)
            .IsFixedLength()
            .ValueGeneratedNever();
    }
}
