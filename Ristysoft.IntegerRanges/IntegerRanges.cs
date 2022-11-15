using System;
using System.Linq;
using System.Collections.Generic;

namespace Ristysoft
{
    public class IntegerRanges
    {
        private Node rootNode;
        private List<IntegerRange> intRanges;

        public IntegerRanges() { }

        public IntegerRanges(string ranges)
        {
            AddFromString(ranges);
        }

        public IntegerRanges(IEnumerable<int> numbers)
        {
            AddArray(numbers);
        }

        public IntegerRanges(int from, int to)
        {
            AddRange(from, to);
        }

        public static IntegerRanges Parse(string ranges)
        {
            IntegerRanges result = new IntegerRanges();
            result.AddFromString(ranges);
            return result;
        }

        public static bool IncludedInRange(string ranges, int num)
        {
            return Parse(ranges).Included(num);
        }

        public void AddRange(int from)
        {
            AddRange(from, from);
        }

        public void AddRange(int from, int to)
        {
            if (to < from)
            {
                int temp = to;
                to = from;
                from = temp;
            }

            if (rootNode == null) { rootNode = new Node { From = from, To = to }; return; }
            addToNode(rootNode, from, to);
            intRanges = null; // remove cache
        }

        public void RemoveRange(int from)
        {
            RemoveRange(from, from);
        }

        public void RemoveRange(int from, int to)
        {
            if (rootNode == null) return;
            intRanges = null;
            rootNode = removeFromNode(rootNode, from, to);
        }

        public void AddFromString(string ranges)
        {
            if (string.IsNullOrEmpty(ranges)) return;

            int lastIndex = 0;
            int? from = null, to = null;
            for (int i = 0; i < ranges.Length; i++)
            {
                switch (ranges[i])
                {
                    case '-':
                        if (to.HasValue) throw new FormatException("The ranges is invalid.");
                        if (i == lastIndex || from.HasValue) break; // negative
                        from = int.Parse(ranges.Substring(lastIndex, i - lastIndex));
                        lastIndex = i + 1;
                        break;
                    case ',':
                        if (!from.HasValue)
                        {
                            from = int.Parse(ranges.Substring(lastIndex, i - lastIndex));
                            lastIndex = i + 1;
                        }
                        else if (i - lastIndex > 0)
                        {
                            to = int.Parse(ranges.Substring(lastIndex, i - lastIndex));
                            lastIndex = i + 1;
                        }
                        AddRange(from.Value, to ?? from.Value);
                        from = to = null;
                        break;
                    case ' ':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        break;
                    default:
                        throw new FormatException("The ranges is invalid.");

                }
            }

            if (!from.HasValue)
            {
                from = int.Parse(ranges.Substring(lastIndex, ranges.Length - lastIndex));
            }
            else if (ranges.Length - lastIndex > 0)
            {
                to = int.Parse(ranges.Substring(lastIndex, ranges.Length - lastIndex));
            }
            AddRange(from.Value, to ?? from.Value);
        }

        public void AddArray(IEnumerable<int> numbers)
        {
            foreach (int num in numbers) AddRange(num);
        }

        public IntegerRange[] GetRanges()
        {
            if (rootNode == null) return new IntegerRange[0];
            getRanges();
            return intRanges.ToArray();
        }

        public int[] ToArray()
        {
            if (rootNode == null) return new int[0];
            getRanges();
            List<int> ints = new List<int>();
            for (int i = 0; i < intRanges.Count; i++)
                for (int n = intRanges[i].From; n <= intRanges[i].To; n++)
                    ints.Add(n);
            return ints.ToArray();
        }

        public bool Included(int num)
        {
            if (rootNode == null) return false;
            return isNodeIncluded(rootNode, num);
        }

        public int GetCount()
        {
            if (rootNode == null) return 0;
            getRanges();
            return intRanges.Sum(r => r.To - r.From + 1);
        }

        void getRanges()
        {
            if (rootNode == null)
                throw new ArgumentException("There is no defined range.");
            if (intRanges != null) return;
            intRanges = new List<IntegerRange>();
            transverse(rootNode, intRanges);
        }

        public override string ToString()
        {
            if (rootNode == null) return string.Empty;
            getRanges();
            return string.Join(",", intRanges.Select(r => r.ToString()));
        }

        static bool isNodeIncluded(Node node, int num)
        {
            if (node.From <= num && node.To >= num) return true;
            if (num < node.From && node.Prev != null) return isNodeIncluded(node.Prev, num);
            if (num > node.To && node.Next != null) return isNodeIncluded(node.Next, num);
            return false;
        }

        static void transverse(Node node, List<IntegerRange> ranges)
        {
            if (node.Prev != null)
                transverse(node.Prev, ranges);

            ranges.Add(new IntegerRange(node.From, node.To));

            if (node.Next != null)
                transverse(node.Next, ranges);
        }

        static void tryToAggregate(Node node)
        {
            if (node.Prev != null)
            {
                tryToAggregate(node.Prev);

                if (node.Prev.To >= node.From - 1)
                {
                    if (node.Prev.From < node.From)
                        node.From = node.Prev.From;
                    node.Prev = null;
                }
            }

            if (node.Next != null)
            {
                tryToAggregate(node.Next);
                if (node.To >= node.Next.From - 1)
                {
                    if (node.Next.To > node.To)
                        node.To = node.Next.To;
                    node.Next = null;
                }
            }
        }

        Node removeFromNode(Node node, int from, int to)
        {
            if (from > node.To)
            {
                if (node.Next != null)
                    node.Next = removeFromNode(node.Next, from, to);
                return node;
            }
            else if (to < node.From)
            {
                if (node.Prev != null)
                    node.Prev = removeFromNode(node.Prev, from, to);
                return node;
            }
            if (from <= node.From && to >= node.To)
            {
                // Remove
                if (node.Prev != null)
                    node.Prev = removeFromNode(node.Prev, from, to);
                if (node.Next != null)
                    node.Next = removeFromNode(node.Next, from, to);

                if (node.Prev != null)
                {
                    Node replaceNodeParent = findLargestParent(node.Prev);
                    if (replaceNodeParent != null)
                    {
                        node.From = replaceNodeParent.Next.From;
                        node.To = replaceNodeParent.Next.To;
                        replaceNodeParent.Next = null;
                    }
                    else
                    {
                        node.From = node.Prev.From;
                        node.To = node.Prev.To;
                        node.Prev = node.Prev.Prev;
                    }
                }
                else if (node.Next != null)
                {
                    Node replaceNodeParent = findSmallestParent(node.Next);
                    if (replaceNodeParent != null)
                    {
                        node.From = replaceNodeParent.Prev.From;
                        node.To = replaceNodeParent.Prev.To;
                        replaceNodeParent.Prev = null;
                    }
                    else
                    {
                        node.From = node.Next.From;
                        node.To = node.Next.To;
                        node.Next = node.Next.Next;
                    }
                }
                else
                {
                    return null;
                }
            }
            else if (from > node.From && to < node.To)
            {
                // Split
                var next = node.Next;
                node.Next = new Node { From = to + 1, To = node.To, Next = next };
                node.To = from - 1;
            }
            else if (from <= node.From && to < node.To)
            {
                if (node.Prev != null)
                    node.Prev = removeFromNode(node.Prev, from, to);
                node.From = to + 1;
            }
            else if (to >= node.To && from > node.From)
            {
                if (node.Next != null)
                    node.Next = removeFromNode(node.Next, from, to);
                node.To = from - 1;
            }
            return node;
        }

        static Node findLargestParent(Node node)
        {
            if (node.Next != null)
                return findLargestParent(node.Next) ?? node;
            return null;
        }

        static Node findSmallestParent(Node node)
        {
            if (node.Prev != null)
                return findSmallestParent(node.Prev) ?? node;
            return null;
        }

        static void addToNode(Node node, int from, int to)
        {
            if (from <= node.From - 1)
            {
                if (to >= node.To)
                {
                    node.From = from;
                    node.To = to;
                    tryToAggregate(node);

                }
                else if (to >= node.From - 1)
                {
                    if (from < node.From)
                    {
                        node.From = from;
                        tryToAggregate(node);
                    }
                }
                else
                {
                    if (node.Prev == null)
                    {
                        node.Prev = new Node
                        {
                            From = from,
                            To = to
                        };
                    }
                    else
                    {
                        addToNode(node.Prev, from, to);
                    }
                }
            }
            else if (from <= node.To + 1)
            {
                if (to > node.To)
                {
                    node.To = to;
                    tryToAggregate(node);
                }
            }
            else
            {
                if (node.Next == null)
                {
                    node.Next = new Node
                    {
                        From = from,
                        To = to
                    };
                }
                else
                {
                    addToNode(node.Next, from, to);
                }
            }
        }

        private class Node
        {
            public int From { get; set; }
            public int To { get; set; }
            public Node Prev { get; set; }
            public Node Next { get; set; }
        }
    }
}
