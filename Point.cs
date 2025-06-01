using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometryclass;

namespace lab12._4
{
    public class Point<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; set; }
        public bool IsDeleted { get; set; }

        public Point(TKey key, TValue value)
        {
            Key = key;
            Value = value;
            IsDeleted = false;
        }

        public override string ToString() => $"{Key}:{Value}";
    }
}
