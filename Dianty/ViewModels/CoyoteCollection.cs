using Dianty.Services;
using DungeonToolkit.Coyote;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dianty.ViewModels;

public partial class CoyoteCollection : ObservableCollection<CoyoteItem>, ICoyoteListService
{
    public CoyoteCollection(CoyoteManager coyoteManager) : base()
    {
        _coyoteManager = coyoteManager;
    }

    public CoyoteCollection(CoyoteManager coyoteManager, IEnumerable<CoyoteItem> collection) : base(collection)
    {
        _coyoteManager = coyoteManager;
    }

    public CoyoteCollection(CoyoteManager coyoteManager, List<CoyoteItem> list) : base(list)
    {
        _coyoteManager = coyoteManager;
    }

    private readonly CoyoteManager _coyoteManager;

    protected override void ClearItems()
    {
        _coyoteManager.ClearCoyotes();
        base.ClearItems();
    }

    protected override void InsertItem(int index, CoyoteItem item)
    {
        AddCoyote(item);
        base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        RemoveCoyote(this[index]);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, CoyoteItem item)
    {
        RemoveCoyote(this[index]);
        AddCoyote(item);
        base.SetItem(index, item);
    }

    private void AddCoyote(CoyoteItem coyoteItem)
    {
        if (coyoteItem is CoyoteBleItem coyoteBleItem)
            _coyoteManager.AddCoyote(coyoteBleItem.Coyote);
        else if (coyoteItem is CoyoteWsItem coyoteWsItem)
            _coyoteManager.AddCoyote(coyoteWsItem.Coyote);
    }

    private void RemoveCoyote(CoyoteItem coyoteItem)
    {
        if (coyoteItem is CoyoteBleItem coyoteBleItem)
            _coyoteManager.RemoveCoyote(coyoteBleItem.Coyote);
        else if (coyoteItem is CoyoteWsItem coyoteWsItem)
            _coyoteManager.RemoveCoyote(coyoteWsItem.Coyote);
    }
}
