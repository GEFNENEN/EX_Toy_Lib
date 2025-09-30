using System;
using System.Collections.Generic;

namespace ExMech
{
    /// <summary>
    /// 机械生物节点基类（纯数据层面）
    /// </summary>
    public abstract class MechNodeBase
    {
        private List<MechNodeBase> _children;
        private MechNodeBase _parent;
        
        /// <summary> 节点唯一标识符 </summary>
        public Guid Guid { get; } = Guid.NewGuid();
        
        /// <summary> 父节点 </summary>
        public MechNodeBase Parent
        {
            get => _parent;
            private set
            {
                if (_parent == value) return;
                _parent?.RemoveChild(this);
                _parent = value;
            }
        }

        /// <summary> 子节点列表（只读） </summary>
        public IReadOnlyList<MechNodeBase> Children => 
            _children?.AsReadOnly() ?? new List<MechNodeBase>().AsReadOnly();

        /// <summary> 是否是根节点 </summary>
        public bool IsRoot => Parent == null;

        /// <summary> 是否是叶节点 </summary>
        public bool IsLeaf => _children == null || _children.Count == 0;

        /// <summary> 添加子节点 </summary>
        /// <exception cref="InvalidOperationException">添加自身或祖先节点时抛出</exception>
        public void AddChild(MechNodeBase child)
        {
            if (child == this)
                throw new InvalidOperationException("Cannot add self as child");
                
            if (IsAncestorOf(child))
                throw new InvalidOperationException("Cannot add ancestor as child");
            
            _children ??= new List<MechNodeBase>();
            
            if (!_children.Contains(child))
            {
                _children.Add(child);
                child.Parent = this;
            }
        }

        /// <summary> 批量添加子节点 </summary>
        public void AddChildren(IEnumerable<MechNodeBase> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        /// <summary> 移除子节点 </summary>
        /// <returns>是否成功移除</returns>
        public bool RemoveChild(MechNodeBase child)
        {
            if (child == null || _children == null) return false;
            
            bool removed = _children.Remove(child);
            if (removed)
            {
                child.Parent = null;
            }
            return removed;
        }

        /// <summary> 从父节点中移除自身 </summary>
        public void RemoveFromParent()
        {
            Parent?.RemoveChild(this);
        }

        /// <summary> 检查是否是某节点的祖先 </summary>
        public bool IsAncestorOf(MechNodeBase node)
        {
            if (node == null) return false;
            
            MechNodeBase current = node.Parent;
            while (current != null)
            {
                if (current == this) return true;
                current = current.Parent;
            }
            return false;
        }

        /// <summary> 深度优先遍历所有后代节点（包括自身） </summary>
        public IEnumerable<MechNodeBase> Traverse()
        {
            var stack = new Stack<MechNodeBase>();
            stack.Push(this);
            
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;
                
                if (current._children != null)
                {
                    // 逆序添加以保证遍历顺序正确
                    for (int i = current._children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current._children[i]);
                    }
                }
            }
        }

        /// <summary> 在整个子树中查找特定类型的节点 </summary>
        public T FindNodeInTree<T>() where T : MechNodeBase
        {
            foreach (var node in Traverse())
            {
                if (node is T result)
                {
                    return result;
                }
            }
            return null;
        }
    }
}