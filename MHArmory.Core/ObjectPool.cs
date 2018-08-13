using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core
{
    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> storage;
        private readonly Func<T> objectFactory;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
                throw new ArgumentNullException(nameof(objectGenerator));

            storage = new ConcurrentBag<T>();

            objectFactory = objectGenerator;
        }

        public int Size => storage.Count;

        public T GetObject()
        {
            if (storage.TryTake(out T item))
                return item;

            return objectFactory();
        }

        public void PutObject(T item)
        {
            storage.Add(item);
        }
    }
}
