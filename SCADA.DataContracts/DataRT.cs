using SCADA.Common;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SCADA.DataContracts
{
    public delegate object DataGetter(string dataName);

    public class DataRT : IDisposable
    {
        private readonly SCADA.Common.PeriodicTimer _dataAcquisitionTimer;
        private readonly ConcurrentDictionary<string, DataGetter> _dataGetters;
        private readonly ConcurrentDictionary<string, DataGetter> _dataGettersShouldSaveDB;

        private bool _disposedValue;

        public DataRT()
        {
            _dataGetters = new ConcurrentDictionary<string, DataGetter>();
            _dataGettersShouldSaveDB = new ConcurrentDictionary<string, DataGetter>();
            _dataAcquisitionTimer = new SCADA.Common.PeriodicTimer((int)(DataAcquisitionIntervalSeconds * 1000));
            _dataAcquisitionTimer.Callback += DataAcquisitionTimer_Callback;
        }

        public double DataAcquisitionIntervalSeconds { get; private set; } = 1.0;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public int Get()
        {
            return 0;
        }

        public void Register(string dataName, DataGetter getter, bool saveDB = false)
        {
            if (_dataGetters.TryAdd(dataName, getter))
            {
                throw new ArgumentException($"Data getter for data name {dataName} is already registered.");
            }
            if (saveDB)
            {
                _dataGettersShouldSaveDB.TryAdd(dataName, getter);
            }
        }

        public void Set(int intervalInSeconds)
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Unregister(string dataName)
        {
            _dataGetters.TryRemove(dataName, out _);
            _dataGettersShouldSaveDB.TryRemove(dataName, out _);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        private void DataAcquisitionTimer_Callback(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DataRT()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
    }
}