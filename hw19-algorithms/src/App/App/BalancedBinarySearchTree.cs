namespace App;

public sealed class AvlTree<T> where T : IComparable<T>
{
    private AvlNode<T> _root;
    
    public int Count { get; private set; }
    public int BalanceFactor => _root is null ? 0 : GetBalanceFactor(_root);
    
    public void Add(T value)
    {
        Count++;

        var node = new AvlNode<T>(value);
        
        _root = _root == null ? node : InsertRecursive(_root, node);
    }
    
    public bool Contains(T value)
    {
        var node = FindRecursive(_root, value);

        if (node == null) return false;

        return node.Value.CompareTo(value) == 0;
    }
    
    public bool Remove(T value)
    {
        _root = RemoveRecursive(_root, value);
        
        return true;
    }

    public void Clear()
    {
        var queue = new Queue<AvlNode<T>>();
        queue.Enqueue(_root);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (currentNode.Left != null)
            {
                queue.Enqueue(currentNode.Left);
                currentNode.Left = default!;
                Count--;
            }

            if (currentNode.Right != null)
            {
                queue.Enqueue(currentNode.Right);
                currentNode.Right = default!;
                Count--;
            }
        }

        _root = null;
        Count--;
    }

    private static AvlNode<T> InsertRecursive(AvlNode<T> current, AvlNode<T> n)
    {
        if (current == null)
        {
            return n;
        }
        
        switch (n.Value.CompareTo(current.Value))
        {
            case < 0: // new value is less than current
                current.Left = InsertRecursive(current.Left, n);
                break;
            case >= 0: // new value is greater than or equal to current
                current.Right = InsertRecursive(current.Right, n);
                break;
        }

        return BalanceTree(current);
    }

    private static AvlNode<T> BalanceTree(AvlNode<T> current)
    {
        var balanceFactor = GetBalanceFactor(current);
        
        if (balanceFactor > 1)
        {
            current = GetBalanceFactor(current.Left) > 0 ? RightRotate(current) : LeftThenRightRotate(current);
        }
        else if (balanceFactor < -1)
        {
            current = GetBalanceFactor(current.Right) > 0 ? RightThenLeftRotate(current) : LeftRotate(current);
        }

        return current;
    }

    private AvlNode<T> RemoveRecursive(AvlNode<T> current, T target)
    {
        if (current is null)
        {
            return null;
        }

        // Left subtree
        if (target.CompareTo(current.Value) < 0)
        {
            current.Left = RemoveRecursive(current.Left, target);
            
            if (GetBalanceFactor(current) == -2) //here
            {
                if (GetBalanceFactor(current.Right) <= 0)
                    current = LeftRotate(current);
                else
                    current = RightThenLeftRotate(current);
            }
        }
        // Right subtree
        else if (target.CompareTo(current.Value) > 0)
        {
            current.Right = RemoveRecursive(current.Right, target);
            if (GetBalanceFactor(current) == 2)
            {
                if (GetBalanceFactor(current.Left) >= 0)
                    current = RightRotate(current);
                else
                    current = LeftThenRightRotate(current);
            }
        }
        // Match
        else
        {
            Count--;
            if (current.Right != null)
            {
                //delete its inorder successor
                var parent = current.Right;
                while (parent.Left != null) parent = parent.Left;

                current.Value = parent.Value;
                current.Right = RemoveRecursive(current.Right, parent.Value)!;
                if (GetBalanceFactor(current) == 2) // rebalancing
                {
                    if (GetBalanceFactor(current.Left) >= 0)
                        current = RightRotate(current);
                    else
                        current = LeftThenRightRotate(current);
                }
            }
            else
            {
                //if current.left != null
                return current.Left;
            }
        }

        return current;
    }

    private static AvlNode<T> FindRecursive(AvlNode<T> current, T target)
    {
        if (current is null) return null;

        if (target.CompareTo(current.Value) < 0)
        {
            return target.CompareTo(current.Value) == 0 ? current : FindRecursive(current.Left, target);
        }

        return target.CompareTo(current.Value) == 0 ? current : FindRecursive(current.Right, target);
    }

    private static AvlNode<T> LeftRotate(AvlNode<T> parent) // Right-Right Case
    {
        var pivot = parent.Right;
        parent.Right = pivot.Left;
        pivot.Left = parent;
        return pivot;
    }

    private static AvlNode<T> RightRotate(AvlNode<T> parent) // Left-Left Case
    {
        var pivot = parent.Left;
        parent.Left = pivot.Right;
        pivot.Right = parent;
        return pivot;
    }

    private static AvlNode<T> LeftThenRightRotate(AvlNode<T> parent)
    {
        var pivot = parent.Left;
        parent.Left = LeftRotate(pivot);
        return RightRotate(parent);
    }

    private static AvlNode<T> RightThenLeftRotate(AvlNode<T> parent)
    {
        var pivot = parent.Right;
        parent.Right = RightRotate(pivot);
        return LeftRotate(parent);
    }

    private static int GetHeight(AvlNode<T> current) => current is null ? 0 : 1 + Math.Max(GetHeight(current.Left), GetHeight(current.Right));

    private static int GetBalanceFactor(AvlNode<T> current) => GetHeight(current.Left) - GetHeight(current.Right);

    private class AvlNode<TKey>(TKey value)
    {
        public TKey Value { get; set; } = value;
        public AvlNode<TKey> Left { get; set; }
        public AvlNode<TKey> Right { get; set; }
    }
}
