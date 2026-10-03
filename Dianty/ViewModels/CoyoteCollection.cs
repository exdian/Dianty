using DungeonToolkit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Dianty.ViewModels;

public sealed partial class CoyoteCollection : ObservableCollection<CoyoteItemBase>, IDataSource<CoyoteItemBase>, IDisposable
{
    private IReadOnlyList<CoyoteItemBase> _coyoteItemSnapshot = [];
    private EventHandler? _coyoteItemDataSourceChanged;
    private bool _isDisposed;

    event EventHandler? IDataSource<CoyoteItemBase>.Changed
    {
        add => _coyoteItemDataSourceChanged += value;
        remove => _coyoteItemDataSourceChanged -= value;
    }

    IReadOnlyList<CoyoteItemBase> IDataSource<CoyoteItemBase>.GetSnapshot()
    {
        return Volatile.Read(ref _coyoteItemSnapshot);
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        OnCoyoteItemDataSourceChanged();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Dispose();
            }
        }

        _isDisposed = true;
    }

    private void OnCoyoteItemDataSourceChanged()
    {
        Volatile.Write(ref _coyoteItemSnapshot, [.. this]);
        _coyoteItemDataSourceChanged?.Invoke(this, EventArgs.Empty);
    }
}
