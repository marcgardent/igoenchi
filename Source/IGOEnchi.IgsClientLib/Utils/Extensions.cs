using System;
using System.Collections.Generic;

namespace IGoEnchi
{
    public delegate A Func<A>();

    public delegate B Func<A, B>(A value);

    public sealed class Maybe
    {
        private Maybe()
        {
        }

        public static Maybe<A> Some<A>(A item)
        {
            return Maybe<A>.Some(item);
        }
    }

    public abstract class Maybe<A>
    {
        public static readonly Maybe<A> None = new _None();

        public bool IsNone
        {
            get { return this is _None; }
        }

        public bool IsSome
        {
            get { return this is _Some; }
        }

        public static Maybe<A> Some(A item)
        {
            return new _Some(item);
        }

        public A Get()
        {
            return (this as _Some).Item;
        }

        public void Iter(Action<A> action)
        {
            if (this is _Some)
            {
                action((this as _Some).Item);
            }
        }

        public Maybe<A> Filter(Predicate<A> predicate)
        {
            if (this is _Some &&
                predicate((this as _Some).Item))
            {
                return this;
            }
            return None;
        }

        public Maybe<B> Map<B>(Func<A, B> function)
        {
            if (this is _Some)
            {
                return Maybe<B>.Some(function((this as _Some).Item));
            }
            return Maybe<B>.None;
        }

        private sealed class _None : Maybe<A>
        {
        }

        private sealed class _Some : Maybe<A>
        {
            public readonly A Item;

            public _Some(A item)
            {
                Item = item;
            }
        }
    }

    public abstract class Either<A, B>
    {
        public bool IsLeft
        {
            get { return this is _Left; }
        }

        public bool IsRight
        {
            get { return this is _Right; }
        }

        public static Either<A, B> Left(A item)
        {
            return new _Left(item);
        }

        public static Either<A, B> Right(B item)
        {
            return new _Right(item);
        }

        private sealed class _Left : Either<A, B>
        {
            public readonly A Item;

            public _Left(A item)
            {
                Item = item;
            }
        }

        private sealed class _Right : Either<A, B>
        {
            public readonly B Item;

            public _Right(B item)
            {
                Item = item;
            }
        }
    }

    public static class Collection
    {
        public static IEnumerable<B> Map<A, B>(IEnumerable<A> collection, Func<A, B> f)
        {
            foreach (var item in collection)
            {
                yield return f(item);
            }
        }

        public static int SumMeasure<A>(IEnumerable<A> collection, Func<A, int> measure)
        {
            var sum = 0;
            foreach (var item in collection)
            {
                sum += measure(item);
            }
            return sum;
        }

        public static int MaxMeasure<A>(IEnumerable<A> collection, Func<A, int> measure)
        {
            var max = 0;
            foreach (var item in collection)
            {
                max = Math.Max(max, measure(item));
            }
            return max;
        }

        public static IEnumerable<A> Range<A>(int count, A value)
        {
            for (var i = 0; i < count; i++)
            {
                yield return value;
            }
        }

        public static IEnumerable<A> Range<A>(int count, Func<A> generator)
        {
            for (var i = 0; i < count; i++)
            {
                yield return generator();
            }
        }

        public static IEnumerable<A> Range<A>(int count, Func<int, A> generator)
        {
            for (var i = 0; i < count; i++)
            {
                yield return generator(i);
            }
        }

        public static bool ForAll<A>(IEnumerable<A> collection, Predicate<A> predicate)
        {
            foreach (var item in collection)
            {
                if (!predicate(item))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<A> ToList<A>(IEnumerable<A> collection)
        {
            return new List<A>(collection);
        }

        public static A[] ToArray<A>(IEnumerable<A> collection)
        {
            return ToList(collection).ToArray();
        }
    }
}