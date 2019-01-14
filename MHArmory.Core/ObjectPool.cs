using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core
{
    public class ObjectPool<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> storage;
        private readonly Func<T> objectFactory;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null)
                throw new ArgumentNullException(nameof(objectGenerator));

            storage = new ConcurrentQueue<T>();

            objectFactory = objectGenerator;
        }

        public int Size
        {
            get
            {
                return storage.Count;
            }
        }

        public T GetObject()
        {
            if (storage.TryDequeue(out T item))
                return item;

            return objectFactory();
        }

        public void PutObject(T item)
        {
            storage.Enqueue(item);
        }

        public void Dispose()
        {
        }
    }
}
