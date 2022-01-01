using System;

public interface IPriorityQueue<TElement, TPriority>
{
    void Insert(TElement element, TPriority priority);

    TElement Peek();

    TElement Pop();
}

public class PriorityQueue<TElement, TPriority> : IPriorityQueue<TElement, TPriority>
    where TPriority : IComparable<TPriority>
{
    private readonly FibonacciHeap<TElement, TPriority> Heap;

    public bool IsEmpty
    {
        get
        {
            return Heap.Min == null;
        }
    }

    public PriorityQueue()
    {
        Heap = new FibonacciHeap<TElement, TPriority>();
    }

    public void Insert(TElement value, TPriority priority)
    {
        Heap.Insert(new FibonacciHeapNode<TElement, TPriority>(value, priority));
    }

    public TElement Peek()
    {
        return Heap.Min.Value;
    }

    public TElement Pop()
    {
        return Heap.DeleteMin().Value;
    }
}

public class FibonacciHeap<TElement, TPriority>
    where TPriority : IComparable<TPriority>
{
    public static readonly double InverseLogPhi = 1.0 / Math.Log((1.0 + Math.Sqrt(5.0)) / 2.0);
    public FibonacciHeapNode<TElement, TPriority> Min { get; private set; } = null;

    private readonly TPriority MinPriority;
    private int Size = 0;

    public void Insert(FibonacciHeapNode<TElement, TPriority> toInsert)
    {
        if (Min != null)
        {
            toInsert.SetRight(Min.Right);
            Min.SetRight(toInsert);
        }

        if (Min == null || toInsert.Priority.CompareTo(Min.Priority) < 0)
            Min = toInsert;
        
        Size++;
    }

    public FibonacciHeapNode<TElement, TPriority> DeleteMin()
    {
        FibonacciHeapNode<TElement, TPriority> min = Min;

        if(min != null)
        {
            int childCount = min.Degree;
            FibonacciHeapNode<TElement, TPriority> prevChild = min.Child;

            while(childCount > 0)
            {
                FibonacciHeapNode<TElement, TPriority> nextChild = prevChild.Right;

                prevChild.Left.SetRight(prevChild.Right);

                prevChild.SetRight(min.Right);
                min.SetRight(prevChild);

                prevChild.Orphan();
                prevChild = nextChild;
                childCount--;
            }

            if (min == min.Right)
                Min = null;
            else
            {
                Min = min.Right;
                min.Left.SetRight(min.Right);
                Consolidate();
            }
            Size--;
        }
        return min;
    }

    private void Consolidate()
    {
        int arraySize = (int)Math.Floor(Math.Log(Size) * InverseLogPhi) + 1;

        FibonacciHeapNode<TElement, TPriority>[] degreeArray = new FibonacciHeapNode<TElement, TPriority>[arraySize];
        for (int i = 0; i < arraySize; i++)
            degreeArray[i] = null;

        int rootCount = 0;
        FibonacciHeapNode<TElement, TPriority> x = Min;

        if(x != null)
            do
            {
                rootCount++;
                x = x.Right;
            } while (x != Min);

        while(rootCount > 0)
        {
            int d = x.Degree;
            FibonacciHeapNode<TElement, TPriority> next = x.Right;

            while(d < arraySize && degreeArray[d] != null)
            {
                FibonacciHeapNode<TElement, TPriority> y = degreeArray[d];
                if (x.Priority.CompareTo(y.Priority) > 0)
                {
                    FibonacciHeapNode<TElement, TPriority> temp = y;
                    y = x;
                    x = temp;
                }

                y.GiveParent(x);

                degreeArray[d++] = null;
            }

            degreeArray[d] = x;
            x = next;
            rootCount--;
        }

        Min = null;
        for(int i = 0; i < arraySize; i++)
        {
            FibonacciHeapNode<TElement, TPriority> y = degreeArray[i];
            if (y != null)
            {
                if (Min != null)
                {
                    y.Left.SetRight(y.Right);

                    y.SetRight(Min.Right);
                    Min.SetRight(y);

                    if (y.Priority.CompareTo(Min.Priority) < 0)
                        Min = y;
                }
                else
                    Min = y;
            }
        }
    }
}

public class FibonacciHeapNode<TElement, TPriority>
    where TPriority : IComparable<TPriority>
{
    public readonly TElement Value;
    public readonly TPriority Priority;

    public FibonacciHeapNode<TElement, TPriority> Parent { get; protected set; } = null;
    public FibonacciHeapNode<TElement, TPriority> Child { get; protected set; } = null;
    public FibonacciHeapNode<TElement, TPriority> Left { get; protected set; }
    public FibonacciHeapNode<TElement, TPriority> Right { get; protected set; }
    public int Degree { get; private set; } = 0;

    public FibonacciHeapNode(TElement value, TPriority priority)
    {
        Value = value;
        Priority = priority;
        SetRight(this);
    }

    public void SetRight(FibonacciHeapNode<TElement, TPriority> right)
    {
        Right = right;
        right.Left = this;
    }

    public void GiveParent(FibonacciHeapNode<TElement, TPriority> parent)
    {
        Left.SetRight(Right);

        Parent = parent;

        if(parent.Child == null)
        {
            parent.Child = this;
            SetRight(this);
        }
        else
        {
            SetRight(parent.Child.Right);
            parent.Child.SetRight(this);
        }

        parent.Degree++;
    }

    public void Orphan()
    {
        Parent = null;
    }
}