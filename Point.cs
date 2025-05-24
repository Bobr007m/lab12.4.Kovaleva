using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometryclass;

namespace laba12._2
{
    public class Point<T> where T : Geometryfigure1
    {
        public T Data { get; set; }
        public Point<T> Next { get; set; }
        public bool IsDeleted { get; set; }
        public string Key { get; set; }
        public T Value { get; set; }

        public Point()
        {
            Data = default(T);
            Next = null;
        }
        public Point(string key, T value)
        {
            Key = key;
            Value = value;
            IsDeleted = false;
        }


        public override string ToString()
        {
            if (Data != null)
                return Data.ToString();
            else
                return "null";
        }
    }
}
