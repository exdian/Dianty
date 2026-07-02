using Windows.System;

namespace Dianty.Utils;

public class KeyDownMessage
{
    public KeyDownMessage() { }

    public KeyDownMessage(VirtualKey key)
    {
        Key = key;
    }

    public VirtualKey Key { get; set; }
}
