using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    internal class NodeState<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly bool isDeleted;
        private readonly ListNode<TKey, TValue> next;

        public NodeState(bool isDeleted, ListNode<TKey, TValue> next)
        {
            this.isDeleted = isDeleted;
            this.next = next;
        }
        public bool IsDeleted
        {
            get { return isDeleted; }
        }
        public ListNode<TKey, TValue> Next
        {
            get { return next; }
        }

    }

    internal class ListNode<TKey, TValue> where TKey : IComparable<TKey>
    {
        private KeyValuePair<TKey, TValue> pair;
        private NodeState<TKey, TValue> state;

        public ListNode() : this(default(TKey), default(TValue))
        {
            state = new NodeState<TKey, TValue>(false, null);
        }

        public ListNode(TKey key, TValue value)
        {
            pair.Key = key;
            pair.Value = value;
        }
        private bool CasState(NodeState<TKey, TValue> oldState, NodeState<TKey, TValue> newState)
        {
            return SyncMethods.CAS<nodestate<TKey, TValue>>(ref state, oldState, newState);
        }

        public void FlagAsDeleted()
        {
            NodeState<TKey, TValue> newState;
            NodeState<TKey, TValue> oldState;
            do
            {
                oldState = state;
                newState = new NodeState<TKey, TValue>(true, oldState.Next);
            } while (!CasState(oldState, newState));
        }
        public void TryDeleteChild(ListNode<TKey, TValue> child)
        {
            NodeState<TKey, TValue> oldState = state;
            if (oldState.Next == child)
            {
                NodeState<TKey, TValue> newState = new NodeState<TKey, TValue>(oldState.IsDeleted, child.state.Next);
                CasState(oldState, newState);
            }
        }
        public ListNode<TKey, TValue> GetNext()
        {
            ListNode<TKey, TValue> node = state.Next;
            while ((node != null) && (node.state.IsDeleted))
            {
                TryDeleteChild(node);
                node = state.Next;
            }
            return node;
        }
        public bool InsertChild(ListNode<TKey, TValue> newNode, ListNode<TKey, TValue> successor)
        {
            NodeState<TKey, TValue> oldState = state;

            if ((!oldState.IsDeleted) && (oldState.Next == successor))
            {
                NodeState<TKey, TValue> newState = new NodeState<TKey, TValue>(false, newNode);
                newNode.state = new NodeState<TKey, TValue>(false, oldState.Next);
                return CasState(oldState, newState);
            }
            return false;
        }

    }

    public class LockFreeLinkedList<TKey, TValue> where TKey : IComparable<TKey>
    {
        private ListNode<TKey, TValue> head;

        public LockFreeLinkedList()
        {
            head = new ListNode<TKey, TValue>();
        }
        private bool NodeHasSameKey(ListNode<TKey, TValue> node, TKey key)
        {
            return (node != null) && node.IsEqualToKey(key);
        }

        private ListNode<TKey, TValue> FindNode(TKey key, out ListNode<TKey, TValue> parent)
        {
            ListNode<TKey, TValue> dad = head;
            ListNode<TKey, TValue> node = dad.GetNext();

            while ((node != null) && node.IsLessThanKey(key))
            {
                dad = node;
                node = dad.GetNext();
            }
            parent = dad;
            return node;
        }

        public bool Find(TKey key, out TValue value)
        {
            ListNode<TKey, TValue> parent;
            ListNode<TKey, TValue> node = FindNode(key, out parent);
            if (NodeHasSameKey(node, key))
            {
                value = node.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue Find(TKey key)
        {
            TValue value;
            Find(key, out value);
            return value;
        }
        public void Delete(TKey key)
        {
            ListNode<TKey, TValue> parent;
            ListNode<TKey, TValue> node = FindNode(key, out parent);
            if (NodeHasSameKey(node, key))
            {
                node.FlagAsDeleted();
                parent.TryDeleteChild(node);
            }
        }
        public void Add(TKey key, TValue value)
        {
            ListNode<TKey, TValue> parent;
            ListNode<TKey, TValue> node;
            ListNode<TKey, TValue> newNode = new ListNode<TKey, TValue>(key, value);

            do
            {
                node = FindNode(key, out parent);
                if (NodeHasSameKey(node, key))
                    throw new InvalidOperationException("Key already exists in linked list but keys must be unique");
            } while (!parent.InsertChild(newNode, node));
        }

    }

    }
