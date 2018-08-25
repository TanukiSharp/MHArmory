using System;
using System.Collections;
using System.Collections.Generic;

namespace MHArmory
{
    public class EnumDataSource<T> : IEnumerable<T>
    {
        public static EnumDataSource<T> Instance { get; }

        private readonly T[] values;
        protected EnumDataSource()
        {
            values = (T[])Enum.GetValues(typeof(T));
        }

        static EnumDataSource()
        {
            Instance = new EnumDataSource<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
