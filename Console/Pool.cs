using System.Collections.Generic;

namespace Console
{
    public class Pool<T>
    {
        private static Pool<T> _Pool = new Pool<T>();
        
        private bool Initialized = false;
        private Queue<T> _pool = new Queue<T>();

        /// <summary>
        /// Create new pool and initialize it
        /// </summary>
        static Pool()
        {
            var pool = _Pool;
            if (pool.Initialized)
                return;

            pool.Initialize();
        }

        /// <summary>
        /// Initialize pool with 10 default values of T
        /// </summary>
        private void Initialize()
        {
            for (var i = 0; i < 10; i++)
            {
                Add();
            }

            Initialized = true;
        }

        /// <summary>
        /// Get T from pool
        /// </summary>
        /// <returns>T from pool</returns>
        public static T Get()
        {
            var pool = _Pool;

            while (true)
            {
                if (pool.Initialized)
                    return pool._pool.Dequeue();

                pool.Initialize();
            }
        }

        /// <summary>
        /// Add an item to pool
        /// </summary>
        public static void Add()
        {
            var pool = _Pool;

            while (true)
            {
                if (pool.Initialized)
                    pool._pool.Enqueue(default(T));
            }
        }
    }
}