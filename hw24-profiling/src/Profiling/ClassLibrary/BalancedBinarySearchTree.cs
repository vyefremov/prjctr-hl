using System.Collections;

namespace ClassLibrary;

public sealed class AvlTree<T> : ICollection<T> where T : IComparable<T>
{
    #region Fields

    [Serializable]
    private class AvlNode<TKey>(TKey value)
    {
        public TKey Value = value;
        public AvlNode<TKey> Left = default!;
        public AvlNode<TKey> Right = default!;
    }

    private AvlNode<T> _root = default!;
    private int _count = 0;
    private readonly bool _isReadOnly = false;

    #endregion

    #region Properties

    /// <summary>
    /// Count of items currently stored in the tree.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// True if the collection is readonly. False otherwise.
    /// </summary>
    public bool IsReadOnly => _isReadOnly;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of an AVLTree collection.
    /// </summary>
    public AvlTree()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds the value to the tree.
    /// </summary>
    /// <param name="value">Value to be added to the tree.</param>
    public void Add(T value)
    {
        _count++;
        var newItem = new AvlNode<T>(value);
        if (_root == null)
            _root = newItem;
        else
            _root = RecursiveInsertion(_root, newItem);
    }

    /// <summary>
    /// Checks if the key is contained into the tree.
    /// </summary>
    /// <param name="value">Value to be checked if present in the tree.</param>
    /// <returns>True if the value is in the tree.</returns>
    public bool Contains(T value)
    {
        var node = Find(value, _root);
        if (node == null) return false;

        if (node.Value.CompareTo(value) == 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Removes the specified value from the tree.
    /// </summary>
    /// <param name="value">Value to be deleted.</param>
    public bool Remove(T value)
    {
        _root = Delete(_root, value)!;
        return true;
    }

    /// <summary>
    /// Clears the tree.
    /// </summary>
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
                _count--;
            }

            if (currentNode.Right != null)
            {
                queue.Enqueue(currentNode.Right);
                currentNode.Right = default!;
                _count--;
            }
        }

        _root = default!;
        _count--;
    }

    /// <summary>
    /// Copies the tree onto the provided array.
    /// </summary>
    /// <param name="array">Array to store the values in the tree.</param>
    /// <param name="arrayIndex">Starting index of the provided array.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        var queue = new Queue<AvlNode<T>>();
        queue.Enqueue(_root);
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            array[arrayIndex++] = currentNode.Value;
            if (currentNode.Left != null) queue.Enqueue(currentNode.Left);

            if (currentNode.Right != null) queue.Enqueue(currentNode.Right);
        }
    }

    /// <summary>
    /// Enumerator that iterates over the tree.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        return GetEnumerator(_root);
    }

    /// <summary>
    /// Copies the tree structure into an array.
    /// </summary>
    /// <returns>Array containing the values contained in the tree.</returns>
    public T[] ToArray()
    {
        var array = new T[_count];
        CopyTo(array, 0);
        return array;
    }

    #endregion

    #region Private Methods

    private AvlNode<T> RecursiveInsertion(AvlNode<T> current, AvlNode<T> n)
    {
        if (current == null)
        {
            current = n;
            return current;
        }
        else if (n.Value.CompareTo(current.Value) < 0)
        {
            current.Left = RecursiveInsertion(current.Left, n);
            current = BalanceTree(current);
        }
        else if (n.Value.CompareTo(current.Value) > 0)
        {
            current.Right = RecursiveInsertion(current.Right, n);
            current = BalanceTree(current);
        }

        return current;
    }

    private AvlNode<T> BalanceTree(AvlNode<T> current)
    {
        var b_factor = BalanceFactor(current);
        if (b_factor > 1)
        {
            if (BalanceFactor(current.Left) > 0)
                current = RotateLL(current);
            else
                current = RotateLR(current);
        }
        else if (b_factor < -1)
        {
            if (BalanceFactor(current.Right) > 0)
                current = RotateRL(current);
            else
                current = RotateRR(current);
        }

        return current;
    }

    private AvlNode<T>? Delete(AvlNode<T> current, T target)
    {
        if (current == null)
        {
            return null;
        }
        else
        {
            //left subtree
            if (target.CompareTo(current.Value) < 0)
            {
                current.Left = Delete(current.Left, target)!;
                if (BalanceFactor(current) == -2) //here
                {
                    if (BalanceFactor(current.Right) <= 0)
                        current = RotateRR(current);
                    else
                        current = RotateRL(current);
                }
            }
            //right subtree
            else if (target.CompareTo(current.Value) > 0)
            {
                current.Right = Delete(current.Right, target)!;
                if (BalanceFactor(current) == 2)
                {
                    if (BalanceFactor(current.Left) >= 0)
                        current = RotateLL(current);
                    else
                        current = RotateLR(current);
                }
            }
            //if target is found
            else
            {
                _count--;
                if (current.Right != null)
                {
                    //delete its inorder successor
                    var parent = current.Right;
                    while (parent.Left != null) parent = parent.Left;

                    current.Value = parent.Value;
                    current.Right = Delete(current.Right, parent.Value)!;
                    if (BalanceFactor(current) == 2) //rebalancing
                    {
                        if (BalanceFactor(current.Left) >= 0)
                            current = RotateLL(current);
                        else
                            current = RotateLR(current);
                    }
                }
                else
                {
                    //if current.left != null
                    return current.Left;
                }
            }
        }

        return current;
    }

    private AvlNode<T>? Find(T target, AvlNode<T> current)
    {
        if (current == null) return null;

        if (target.CompareTo(current.Value) < 0)
        {
            if (target.CompareTo(current.Value) == 0)
                return current;
            else
                return Find(target, current.Left);
        }
        else
        {
            if (target.CompareTo(current.Value) == 0)
                return current;
            else
                return Find(target, current.Right);
        }
    }

    private int Max(int l, int r)
    {
        return l > r ? l : r;
    }

    private int GetHeight(AvlNode<T> current)
    {
        var height = 0;
        if (current != null)
        {
            var l = GetHeight(current.Left);
            var r = GetHeight(current.Right);
            var m = Max(l, r);
            height = m + 1;
        }

        return height;
    }

    private int BalanceFactor(AvlNode<T> current)
    {
        var l = GetHeight(current.Left);
        var r = GetHeight(current.Right);
        var b_factor = l - r;
        return b_factor;
    }

    private AvlNode<T> RotateRR(AvlNode<T> parent)
    {
        var pivot = parent.Right;
        parent.Right = pivot.Left;
        pivot.Left = parent;
        return pivot;
    }

    private AvlNode<T> RotateLL(AvlNode<T> parent)
    {
        var pivot = parent.Left;
        parent.Left = pivot.Right;
        pivot.Right = parent;
        return pivot;
    }

    private AvlNode<T> RotateLR(AvlNode<T> parent)
    {
        var pivot = parent.Left;
        parent.Left = RotateRR(pivot);
        return RotateLL(parent);
    }

    private AvlNode<T> RotateRL(AvlNode<T> parent)
    {
        var pivot = parent.Right;
        parent.Right = RotateLL(pivot);
        return RotateRR(parent);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    private IEnumerator<T> GetEnumerator(AvlNode<T> rootNode)
    {
        var queue = new Queue<AvlNode<T>>();
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            yield return currentNode.Value;
            if (currentNode.Left != null) queue.Enqueue(currentNode.Left);

            if (currentNode.Right != null) queue.Enqueue(currentNode.Right);
        }
    }

    #endregion
}