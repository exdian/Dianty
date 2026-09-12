using System;
using System.Collections.Generic;
using Windows.System;
using System.Linq;

namespace Dianty.Utils;

public class KeySequenceTrigger
{
    private int _maxCachedKeys; // 缓存区大小
    private int _cachedKeyIndex; // 缓存写入位置指针
    private VirtualKey[]? _cachedKeys; // 缓存区
    private int _keyCount; // 已输入的按键数量。保证如果 VirtualKey.None 也是有效的按键，也能正确匹配
    private readonly TrieNode _reverseTrieRoot = new(); // 反向 Trie树 根节点

    public void AddKeySequence(string keys, Action action)
    {
        var array = (from key in keys.ToUpperInvariant()
                     select (VirtualKey)key).ToArray();
        AddKeySequence(array, action);
    }

    public void AddKeySequence(VirtualKey[] keys, Action action)
    {
        // 反转序列并插入 Trie
        TrieNode node = _reverseTrieRoot;
        int sequenceLength = keys.Length;
        for (int i = sequenceLength - 1; i >= 0; i--)
        {
            VirtualKey key = keys[i];
            if (!node.Children.TryGetValue(key, out TrieNode? nextNode))
            {
                nextNode = new TrieNode();
                node.Children[key] = nextNode;
            }
            node = nextNode;
        }
        node.Action = action; // 在序列终点存储 Action
        if (sequenceLength > _maxCachedKeys)
        {
            // 调整缓存区大小
            _maxCachedKeys = sequenceLength;
            if (_cachedKeys is null || _cachedKeys.Length != sequenceLength)
            {
                var newCache = new VirtualKey[sequenceLength];
                if (_cachedKeys is not null && _keyCount > 0)
                {
                    int copyCount = Math.Min(_keyCount, sequenceLength);
                    int start = (_cachedKeyIndex - copyCount + _cachedKeys.Length) % _cachedKeys.Length;
                    for (int i = 0; i < copyCount; i++)
                    {
                        newCache[i] = _cachedKeys[(start + i) % _cachedKeys.Length];
                    }
                    _cachedKeyIndex = copyCount % sequenceLength;
                    _keyCount = copyCount;
                }
                else
                {
                    _cachedKeyIndex = 0;
                    _keyCount = 0;
                }
                _cachedKeys = newCache;
            }
        }
    }

    public bool RemoveKeySequence(string keys)
    {
        var array = (from key in keys.ToUpperInvariant()
                     select (VirtualKey)key).ToArray();
        return RemoveKeySequence(array);
    }

    public bool RemoveKeySequence(VirtualKey[] keys)
    {
        if (keys.Length == 0)
            return false;

        // 查找终点节点，并收集路径上的节点
        var path = new List<TrieNode>();
        TrieNode node = _reverseTrieRoot;
        for (int i = keys.Length - 1; i >= 0; i--)
        {
            if (!node.Children.TryGetValue(keys[i], out TrieNode? next))
                return false; // 序列不存在
            path.Add(next);
            node = next;
        }

        // 删除 Action
        node.Action = null;

        // 清理无用的节点
        // 从叶子（path 的最后一个元素）向上回溯
        for (int i = path.Count - 1; i >= 0; i--)
        {
            TrieNode trieNode = path[i];
            // 如果当前节点不可删除（有 Action 或有子节点），则祖先节点也不能删除，直接终止
            if (trieNode.Action is not null || trieNode.Children.Count > 0)
                break;

            // 从父节点中移除当前节点
            if (i == 0)
            {
                // 父节点是根节点，需要知道根的子键
                VirtualKey rootKey = keys[^1]; // 原始序列的最后一个键（逆序的第一个）
                _reverseTrieRoot.Children.Remove(rootKey);
            }
            else
            {
                TrieNode parent = path[i - 1];
                VirtualKey childKey = keys[keys.Length - 1 - i]; // 当前节点对应的原始按键
                parent.Children.Remove(childKey);
            }
        }

        return true;
    }

    public void ProcessKey(VirtualKey key)
    {
        if (_cachedKeys is null)
            return;

        _cachedKeys[_cachedKeyIndex] = key;
        _cachedKeyIndex = (_cachedKeyIndex + 1) % _maxCachedKeys;
        if (_keyCount < _maxCachedKeys)
            _keyCount++;

        var currentNode = _reverseTrieRoot;
        int tempKeyIndex = (_cachedKeyIndex - 1 + _maxCachedKeys) % _maxCachedKeys; // 当前指针的前一个位置

        // 反向遍历缓存(从新到旧)
        for (int i = 0; i < _keyCount; i++)
        {
            var currentKey = _cachedKeys[tempKeyIndex];

            // 尝试匹配 Trie 路径
            if (!currentNode.Children.TryGetValue(currentKey, out currentNode))
                break; // 无匹配路径

            // 发现完整序列时立即触发，并继续匹配路径
            currentNode.Action?.Invoke();

            tempKeyIndex--;
            if (tempKeyIndex < 0)
            {
                tempKeyIndex = _maxCachedKeys - 1;
            }
        }
    }

    private class TrieNode
    {
        public Dictionary<VirtualKey, TrieNode> Children { get; } = [];
        public Action? Action { get; set; }
    }
}
