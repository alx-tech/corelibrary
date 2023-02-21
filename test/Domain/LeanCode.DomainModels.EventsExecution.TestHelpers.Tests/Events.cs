using System;
using LeanCode.DomainModels.Model;

namespace LeanCode.DomainModels.EventsExecution.TestHelpers.Tests;

internal sealed class SampleEvent1 : IDomainEvent
{
    public Guid Id { get; }
    public DateTime DateOccurred { get; }

    public SampleEvent1(Guid id)
    {
        Id = id;
    }
}

internal sealed class SampleEvent2 : IDomainEvent
{
    public Guid Id { get; }
    public DateTime DateOccurred { get; }

    public SampleEvent2(Guid id)
    {
        Id = id;
    }
}
