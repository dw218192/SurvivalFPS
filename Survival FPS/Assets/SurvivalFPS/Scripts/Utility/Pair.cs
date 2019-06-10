using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalFPS.Utility
{
    [Serializable]
    public class Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public T1 first { get; set; }
        public T2 second { get; set; }

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }

        public Pair()
        {
            first = default(T1);
            second = default(T2);
        }

        public override int GetHashCode()
        {
            return first.GetHashCode() ^ second.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pair<T1, T2>);
        }

        public bool Equals(Pair<T1, T2> other)
        {
            return other != null &&
                   EqualityComparer<T1>.Default.Equals(first, other.first) &&
                   EqualityComparer<T2>.Default.Equals(second, other.second);
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", first.ToString(), second.ToString());
        }
    }
}