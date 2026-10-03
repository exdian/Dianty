using Dianty.EventArguments;
using System;

namespace Dianty.Services;

public interface INameRegistry<T> : INameRegistryReader<T>, INameRegistryWriter<T>;

public interface INameRegistryReader<T>
{
    event EventHandler<RegisteredNameChangedEventArgs<T>>? RegisteredNameChanged;

    string GetName(T key);
}

public interface INameRegistryWriter<T>
{
    void Rename(T key, string name);
}
