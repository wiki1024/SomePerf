using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BenchRunner
{
    public class FineLockCalculator : CalculatorBase
    {
        private readonly object _gate = new object();

        private enum Status
        {
            NotStarted,
            Started,
            Starting,
            Disposed
        }

        private Status _status = Status.NotStarted;

        public override void Start()
        {
            //lock (_gate)
            //{
            //    if (_status != Status.NotStarted)
            //    {
            //        return;
            //    }
            //    _status = Status.Starting;
            lock (_gate)
            {
                if (_status != Status.NotStarted)
                    return;

                _status = Status.Starting;
            }
            //}
            StartCore();
            lock (_gate)
            {
                if (_status == Status.Starting)
                {
                    _status = Status.Started;
                    return;
                }
            }

            DisposeCore();
        }

        public override void Dispose()
        {
            lock (_gate)
            {
                var origStatus = _status;
                _status = Status.Disposed;

                if (origStatus != Status.Started)
                    return;
            }

            DisposeCore();
        }
    }

    public class MyFineLockCalculator : CalculatorBase
    {
        private readonly object _gate = new object();

        private enum Status
        {
            NotStarted,
            Started,
            Starting,
            Disposed
        }

        private Status _status = Status.NotStarted;

        public override void Start()
        {
            lock (_gate)
            {
                if (_status != Status.NotStarted)
                {
                    return;
                }
                _status = Status.Starting;

            }
            StartCore();

            lock (_gate)
            {
                if (_status != Status.Disposed)
                {
                    _status = Status.Started;
                }
            }


            Dispose();
        }

        public override void Dispose()
        {
            lock (_gate)
            {
                if(_status!= Status.Started) return;
                _status = Status.Disposed;
            }

            DisposeCore();
        }
    }

    public abstract class CalculatorBase : IDisposable
    {

        protected void StartCore()
        {
            Console.WriteLine("Done real start");

            Thread.Sleep(1000);
        }

        protected void DisposeCore()
        {
            Console.WriteLine("Done real dispose");

            Thread.Sleep(1000);

        }

        public abstract void Start();

        public abstract void Dispose();
    }

    public class BigLockCalculator : CalculatorBase
    {

        private readonly object _gate = new object();

        private enum Status
        {
            NotStarted,
            Started,
            Disposed
        }

        private Status _status = Status.NotStarted;

        public override void Start()
        {
            lock (_gate)
            {
                if (_status == Status.NotStarted)
                {
                    StartCore();
                    _status = Status.Started;
                }
            }
        }

        public override void Dispose()
        {
            lock (_gate)
            {
                if (_status == Status.Started)
                {
                    DisposeCore();
                }
                _status = Status.Disposed;
            }
        }
    }

    public class MultiThreadLockPerformanceTester<T> where T : CalculatorBase, new()
    {


        public virtual void Run(int round)
        {


            var watch = new Stopwatch();
            for (int i = 0; i < round; i++)
            {
                var _i = i;
                var target = new T();
                watch.Start();
                List<Task> tasks = new List<Task>();
                for (int j = 0; j < Environment.ProcessorCount; j++)
                {
                    
                    Task t = Task.Run(() =>
                    {
                        target.Start();
                        Console.WriteLine("Done start"+ _i);
                        target.Dispose();
                        Console.WriteLine("Done dispose"+ _i);
                    });
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
                watch.Stop();
            }
            Console.WriteLine((watch.Elapsed.TotalMilliseconds / round) + " op/ms [" + typeof(T).Name + "] Multi thread");
        }


    }
}
