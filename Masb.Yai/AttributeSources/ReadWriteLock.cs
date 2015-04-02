using System;
using System.Threading;
using JetBrains.Annotations;

namespace Masb.Yai.AttributeSources
{
    public static class ReadWriteLock_Tests
    {
        private static readonly ReadWriteLockWithWritePriority Locker = new ReadWriteLockWithWritePriority();

        class Params
        {
            public Action<string> Write { get; set; }
            public int Interval { get; set; }
        }

        public static void Main(Action<string> write)
        {
            ThreadPool.QueueUserWorkItem(Reader, new Params { Write = write, Interval = 100 });
            ThreadPool.QueueUserWorkItem(Reader, new Params { Write = write, Interval = 100 });
            ThreadPool.QueueUserWorkItem(Reader, new Params { Write = write, Interval = 100 });
            ThreadPool.QueueUserWorkItem(Writer, new Params { Write = write, Interval = 500 });
            ThreadPool.QueueUserWorkItem(Writer, new Params { Write = write, Interval = 400 });
        }

        private static void Reader(object state)
        {
            var p = state as Params;
            if (p == null)
                throw new ArgumentNullException("write");

            while (true)
            {
                Locker.WriteWith(
                    () =>
                    {
                        p.Write("reading");
                        ThreadOps.Sleep(p.Interval);
                    });
            }
        }

        private static void Writer(object state)
        {
            var p = state as Params;
            if (p == null)
                throw new ArgumentNullException("write");

            while (true)
            {
                Locker.WriteWith(
                    () =>
                    {
                        p.Write("writing");
                        ThreadOps.Sleep(p.Interval);
                    });
            }
        }
    }

    internal static class ThreadOps
    {
#if !FULL_NET
        private static readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);
#endif

        public static void Sleep(int millisecondsTimeout)
        {
#if FULL_NET
            Thread.Sleep(0);
#else
            // Set is never called, so we wait always until the timeout occurs.
            // Socumentation says that when: millisecondsTimeout == 0
            // then the thread does not block... we need it to block, so we replace 0ms with 1ms.
            ResetEvent.WaitOne(millisecondsTimeout == 0 ? 1 : millisecondsTimeout);
#endif
        }
    }

    /// <summary>
    /// Locking primitive that spins multiple sleep cycles.
    /// </summary>
    internal struct SpinSleepLock
    {
        private volatile int state;
        private Thread ownerThread;
        private int recursiveLocksCount;
        private int retries;

        public void Enter()
        {
            // ReSharper disable once CSharpWarnings::CS0420 - Interlocked has stronger guarantees than volatile... so its not a problem
            while (Interlocked.Exchange(ref this.state, -1) != 0 && this.ownerThread != Thread.CurrentThread)
                ThreadOps.Sleep(Math.Min(Math.Max(this.retries++, 0) >> 4, 1000));

            this.ownerThread = Thread.CurrentThread;
            this.recursiveLocksCount++;
            this.retries = 0;
        }

        public void Exit()
        {
            if (this.ownerThread != Thread.CurrentThread)
                throw new Exception("`SpinSleepLock.Exit()` can only be called by the thread that called `SpinSleepLock.Enter()`.");

            if (this.recursiveLocksCount == 0)
                throw new Exception("Cannot call `SpinSleepLock.Exit()` more times than `SpinSleepLock.Enter()`.");

            this.recursiveLocksCount--;
            if (this.recursiveLocksCount == 0)
            {
                this.ownerThread = null;
                this.state = 0;
            }
        }
    }

    internal class EventSleepLock :
        IDisposable
    {
        private readonly AutoResetEvent lockEvent = new AutoResetEvent(false);
        private volatile int locker;

        public void Lock(Action action)
        {
            this.Enter();

            try
            {
                action();
            }
            finally
            {
                this.Exit();
            }
        }

        public void Enter()
        {
#if NET45
            Thread.BeginCriticalRegion();
#endif
            // ReSharper disable once CSharpWarnings::CS0420 - Interlocked.CompareExchange already inhibits instruction reordering
            while (Interlocked.Exchange(ref this.locker, -1) != 0)
                this.lockEvent.WaitOne();
        }

        public void Exit()
        {
            this.locker = 0;
            this.lockEvent.Set();
#if NET45
            Thread.EndCriticalRegion();
#endif
        }

        public void Dispose()
        {
            this.lockEvent.Dispose();
        }
    }

    internal class ReadWriteLockWithWritePriority :
        IDisposable
    {
        private readonly EventSleepLock locker = new EventSleepLock();
        private readonly AutoResetEvent writeEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent readEvent = new ManualResetEvent(false);
        private int writesAwaiting;
        private int readsAwaiting;
        private int readsRunning;
        private bool disposed;

        /// <summary>
        /// Calls a write action, ensuring the following rules:
        /// <para>- only one write action can run at the same moment</para>
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- writes have preference over reads</para>
        /// <para>- all writes are executed in order</para>
        /// </summary>
        /// <param name="writter">The action that represents the write operation.</param>
        public void WriteWith([NotNull] Action writter)
        {
            if (writter == null)
                throw new ArgumentNullException("writter");

            this.StartWriting();

            try
            {
                writter();
            }
            finally
            {
                this.EndWriting();
            }
        }

        public void StartWriting()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            bool wait = false;
            this.locker.Enter();
            try
            {
                // ManualResetEvent is meant to release threads while it is set.
                // If the interval between Set and Reset is too short, it could release even no threads at all.
                // So, we can stop releasing more threads, by calling `Reset`...
                // this will allow the write to be processed sooner,
                // because `this.readsRunning` will stop increasing and reach 0 faster.
                this.readEvent.Reset();

                wait = this.writesAwaiting >= 1 || this.readsRunning != 0;
                this.writesAwaiting++;
            }
            finally
            {
                this.locker.Exit();
            }

            if (wait)
                this.writeEvent.WaitOne();
        }

        public void EndWriting()
        {
            this.locker.Enter();
            try
            {
                this.writesAwaiting--;

                if (this.writesAwaiting > 0)
                    this.writeEvent.Set();

                if (this.readsAwaiting > 0)
                    this.readEvent.Set();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        /// <summary>
        /// Calls a read action, ensuring the following rules:
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- writes have preference over reads</para>
        /// <para>- all reads are started in order, but may run in parallel</para>
        /// </summary>
        /// <param name="reader">The action that represents the read operation.</param>
        /// <returns>The value returned by the read action.</returns>
        /// <typeparam name="T">The type returned by the read action.</typeparam>
        public T ReadWith<T>([NotNull] Func<T> reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.StartReading();

            try
            {
                return reader();
            }
            finally
            {
                this.EndReading();
            }
        }

        public void StartReading()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            bool wait = false;
            this.locker.Enter();
            try
            {
                wait = this.writesAwaiting != 0;

                if (wait)
                {
                    this.readEvent.Reset();
                    this.readsAwaiting++;
                }
                else
                    this.readsRunning++;
            }
            finally
            {
                this.locker.Exit();
            }

            if (wait)
            {
                this.readEvent.WaitOne();
                this.locker.Enter();
                try
                {
                    this.readsAwaiting--;
                    this.readsRunning++;
                }
                finally
                {
                    this.locker.Exit();
                }
            }
        }

        public void EndReading()
        {
            this.locker.Enter();
            try
            {
                this.readsRunning--;

                if (this.readsRunning == 0 && this.writesAwaiting > 0)
                    this.writeEvent.Set();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        public void Dispose()
        {
            this.disposed = true;

            if (this.locker != null)
                this.locker.Dispose();

            if (this.readEvent != null)
                this.readEvent.Dispose();

            if (this.writeEvent != null)
                this.writeEvent.Dispose();
        }
    }

    /// <summary>
    /// Read/Write lock that does not allow readers starvation,
    /// but allows writers starvation.
    /// </summary>
    internal class ReadWriteLockWithReadPriority :
        IDisposable
    {
        private readonly EventSleepLock locker = new EventSleepLock();
        private readonly AutoResetEvent autoEvent = new AutoResetEvent(true);
        private volatile int readsRunning;
        private bool disposed;

        /// <summary>
        /// Calls a write action, ensuring the following rules:
        /// <para>- only one write action can run at the same moment</para>
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- reads have preference over writes</para>
        /// <para>- all writes are executed in order</para>
        /// </summary>
        /// <param name="writter">The action that represents the write operation.</param>
        public void WriteWith([NotNull] Action writter)
        {
            if (writter == null)
                throw new ArgumentNullException("writter");

            this.StartWriting();

            try
            {
                writter();
            }
            finally
            {
                this.EndWriting();
            }
        }

        public void StartWriting()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            this.autoEvent.WaitOne();
        }

        public void EndWriting()
        {
            this.autoEvent.Set();
        }

        /// <summary>
        /// Calls a read action, ensuring the following rules:
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- reads have preference over writes</para>
        /// <para>- all reads are started in order, but may run in parallel</para>
        /// </summary>
        /// <param name="reader">The action that represents the read operation.</param>
        /// <returns>The value returned by the read action.</returns>
        /// <typeparam name="T">The type returned by the read action.</typeparam>
        public T ReadWith<T>([NotNull] Func<T> reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.StartReading();

            try
            {
                return reader();
            }
            finally
            {
                this.EndReading();
            }
        }

        public void StartReading()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            this.locker.Enter();
            try
            {
                this.readsRunning++;
                if (this.readsRunning == 1)
                    this.autoEvent.WaitOne();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        public void EndReading()
        {
            this.locker.Enter();
            try
            {
                this.readsRunning--;
                if (this.readsRunning == 0)
                    this.autoEvent.Set();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        public void Dispose()
        {
            this.disposed = true;

            if (this.locker != null)
                this.locker.Dispose();

            if (this.autoEvent != null)
                this.autoEvent.Dispose();
        }
    }

    internal class ReadWriteLockUpgradable :
        IDisposable
    {
        private readonly EventSleepLock locker = new EventSleepLock();
        private readonly AutoResetEvent writeEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent readEvent = new ManualResetEvent(false);
        private int writesAwaiting;
        private int readsAwaiting;
        private int readsRunning;
        private bool disposed;

        /// <summary>
        /// Calls a write action, ensuring the following rules:
        /// <para>- only one write action can run at the same moment</para>
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- writes have preference over reads</para>
        /// <para>- all writes are executed in order</para>
        /// </summary>
        /// <param name="writter">The action that represents the write operation.</param>
        public void WriteWith([NotNull] Action writter)
        {
            if (writter == null)
                throw new ArgumentNullException("writter");

            this.StartWriting();

            try
            {
                writter();
            }
            finally
            {
                this.EndWriting();
            }
        }

        public void StartWriting()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            bool wait = false;
            this.locker.Enter();
            try
            {
                // ManualResetEvent is meant to release threads while it is set.
                // If the interval between Set and Reset is too short, it could release even no threads at all.
                // So, we can stop releasing more threads, by calling `Reset`...
                // this will allow the write to be processed sooner,
                // because `this.readsRunning` will stop increasing and reach 0 faster.
                this.readEvent.Reset();

                wait = this.writesAwaiting >= 1 || this.readsRunning != 0;
                this.writesAwaiting++;
            }
            finally
            {
                this.locker.Exit();
            }

            if (wait)
                this.writeEvent.WaitOne();
        }

        public void EndWriting()
        {
            this.locker.Enter();
            try
            {
                this.writesAwaiting--;

                if (this.writesAwaiting > 0)
                    this.writeEvent.Set();

                if (this.readsAwaiting > 0)
                    this.readEvent.Set();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        /// <summary>
        /// Calls a read action, ensuring the following rules:
        /// <para>- no read actions can run while a write is running</para>
        /// <para>- writes have preference over reads</para>
        /// <para>- all reads are started in order, but may run in parallel</para>
        /// </summary>
        /// <param name="reader">The action that represents the read operation.</param>
        /// <returns>The value returned by the read action.</returns>
        /// <typeparam name="T">The type returned by the read action.</typeparam>
        public T ReadWith<T>([NotNull] Func<T> reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            this.StartReading();

            try
            {
                return reader();
            }
            finally
            {
                this.EndReading();
            }
        }

        public void StartReading()
        {
            if (this.disposed)
                throw new ObjectDisposedException("The `ReadWriteLock` was already disposed.");

            bool wait = false;
            this.locker.Enter();
            try
            {
                wait = this.writesAwaiting != 0;

                if (wait)
                {
                    this.readEvent.Reset();
                    this.readsAwaiting++;
                }
                else
                    this.readsRunning++;
            }
            finally
            {
                this.locker.Exit();
            }

            if (wait)
            {
                this.readEvent.WaitOne();
                this.locker.Enter();
                try
                {
                    this.readsAwaiting--;
                    this.readsRunning++;
                }
                finally
                {
                    this.locker.Exit();
                }
            }
        }

        public void EndReading()
        {
            this.locker.Enter();
            try
            {
                this.readsRunning--;

                if (this.readsRunning == 0 && this.writesAwaiting > 0)
                    this.writeEvent.Set();
            }
            finally
            {
                this.locker.Exit();
            }
        }

        public bool TryStartWriting()
        {
            
        }

        public void Dispose()
        {
            this.disposed = true;

            if (this.locker != null)
                this.locker.Dispose();

            if (this.readEvent != null)
                this.readEvent.Dispose();

            if (this.writeEvent != null)
                this.writeEvent.Dispose();
        }
    }
}