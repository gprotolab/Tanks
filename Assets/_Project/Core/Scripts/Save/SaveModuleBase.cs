using System;
using Newtonsoft.Json;

namespace ANut.Core.Save
{
    public abstract class SaveModuleBase<T> : ISaveModule where T : class, new()
    {
        // Data is accessible only within the subclass
        protected T Data { get; private set; } = new();

        // What the subclass must define 
        public abstract string Key { get; }
        protected virtual int CurrentVersion => 1;

        // ISaveModule — explicit implementations
        int ISaveModule.Version => CurrentVersion;
        bool ISaveModule.IsDirty => _isDirty;
        void ISaveModule.ResetDirty() => _isDirty = false;

        string ISaveModule.Serialize()
            => JsonConvert.SerializeObject(Data);

        void ISaveModule.Deserialize(string payload, int fromVersion)
        {
            try
            {
                string json = (fromVersion < CurrentVersion && !string.IsNullOrEmpty(payload))
                    ? Migrate(fromVersion, payload)
                    : payload;

                Data = string.IsNullOrEmpty(json)
                    ? new T()
                    : JsonConvert.DeserializeObject<T>(json) ?? new T();
            }
            catch (Exception e)
            {
                Log.Error("[{0}] Deserialize failed: {1}", Key, e.Message);
                Data = new T();
            }

            OnAfterDeserialize();
            _isDirty = false; // reset after load — even if MarkDirty was called in OnAfterDeserialize
        }

        // For subclasses
        protected void MarkDirty() => _isDirty = true;

        /// Hook called after deserialization. Used to synchronize
        /// runtime objects (e.g. ReactiveProperty) with loaded data.
        protected virtual void OnAfterDeserialize()
        {
        }

        /// Override when schema migration is needed.
        /// fromVersion — version from the file; returns updated JSON.
        protected virtual string Migrate(int fromVersion, string payload) => payload;

        // Private 
        private bool _isDirty;
    }
}