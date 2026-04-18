using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace ANut.Core.Save
{
    public class SaveService : ISaveService
    {
        private const string SaveFile = "savedata";

        // Cached on the main thread in the constructor — Application.persistentDataPath
        // must NOT be accessed from a thread pool (Unity API restriction).
        private readonly string _savePath;
        private readonly string _tempPath;
        private readonly string _bakPath;

        private readonly IEnumerable<ISaveModule> _saveModules;
        private bool _isBusy;

        public SaveService(IEnumerable<ISaveModule> saveModules)
        {
            _saveModules = saveModules;

            // Constructor is called on the main thread by VContainer — safe to use Unity API here.
            string dir = Application.persistentDataPath;
            _savePath = Path.Combine(dir, SaveFile);
            _tempPath = _savePath + ".tmp";
            _bakPath = _savePath + ".bak";
        }

        // Load 

        public void Load()
        {
            var global = ReadGlobalSave();
            ApplyToAll(global);
        }

        public async UniTask LoadAsync(CancellationToken ct)
        {
            if (_isBusy) return;
            _isBusy = true;
            try
            {
                var global = await UniTask.RunOnThreadPool(() => ReadGlobalSave(), cancellationToken: ct);
                ApplyToAll(global);
            }
            finally
            {
                _isBusy = false;
            }
        }

        // Save 

        public void Save()
        {
            if (!HasDirty()) return;
            try
            {
                WriteViaTemp(BuildJson());
                ResetAllDirty();
            }
            catch (Exception e)
            {
                Log.Error("[SaveService] Save failed: {0}", e.Message);
            }
        }

        public async UniTask SaveAsync(CancellationToken ct)
        {
            if (_isBusy || !HasDirty()) return;
            _isBusy = true;
            try
            {
                string json = BuildJson();
                await UniTask.RunOnThreadPool(() => WriteViaTemp(json), cancellationToken: ct);
                ResetAllDirty();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error("[SaveService] SaveAsync failed: {0}", e.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

        // Private 

        private void ApplyToAll(GlobalSave global)
        {
            foreach (var module in _saveModules)
            {
                global.Slots.TryGetValue(module.Key, out var slot);
                module.Deserialize(slot?.Payload, slot?.Version ?? 0);
            }
        }

        private string BuildJson()
        {
            var global = new GlobalSave();
            foreach (var module in _saveModules)
            {
                global.Slots[module.Key] = new SaveSlot
                {
                    Version = module.Version,
                    Payload = module.Serialize()
                };
            }

            return JsonConvert.SerializeObject(global, Formatting.None);
        }

        private GlobalSave ReadGlobalSave()
        {
            try
            {
                string json = TryRead(_savePath) ?? TryRead(_bakPath);
                if (!string.IsNullOrEmpty(json))
                    return JsonConvert.DeserializeObject<GlobalSave>(json) ?? new GlobalSave();
            }
            catch (Exception e)
            {
                Log.Error("[SaveService] ReadGlobalSave failed: {0}", e.Message);
            }

            return new GlobalSave();
        }

        private void WriteViaTemp(string json)
        {
            File.WriteAllText(_tempPath, json);
            // Rotate .bak before replacing the main file
            if (File.Exists(_savePath))
                File.Copy(_savePath, _bakPath, overwrite: true);
            if (File.Exists(_savePath))
                File.Delete(_savePath);
            File.Move(_tempPath, _savePath);
        }

        private static string TryRead(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch
            {
                return null;
            }
        }

        private bool HasDirty() => _saveModules.Any(o => o.IsDirty);

        private void ResetAllDirty()
        {
            foreach (var o in _saveModules) o.ResetDirty();
        }
    }
}