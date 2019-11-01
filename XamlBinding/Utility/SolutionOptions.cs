using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace XamlBinding.Utility
{
    /// <summary>
    /// Package options that are saved per-solution
    /// </summary>
    internal class SolutionOptions : ObservableObject
    {
        private ConcurrentDictionary<string, object> solutionOptions;

        public SolutionOptions()
        {
            this.solutionOptions = new ConcurrentDictionary<string, object>();
        }

        public void Set(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (value == null)
                {
                    this.solutionOptions.TryRemove(key, out _);
                }
                else
                {
                    this.solutionOptions[key] = value;
                }

                this.NotifyPropertyChanged(key);
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (!this.solutionOptions.TryGetValue(key, out object objectValue) || !(objectValue is T value))
            {
                value = defaultValue;
            }

            return value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (this.solutionOptions.TryGetValue(key, out object objectValue) && objectValue is T)
            {
                value = (T)objectValue;
                return true;
            }

            value = default;
            return false;
        }

        public void Load(Stream stream)
        {
            this.solutionOptions.Clear();

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                if (formatter.Deserialize(stream) is Dictionary<string, object> options)
                {
                    this.solutionOptions = new ConcurrentDictionary<string, object>(options);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("Failed to load options", ex.Message);
            }

            this.NotifyPropertyChanged();
        }

        public void Save(Stream stream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Dictionary<string, object> options = new Dictionary<string, object>(this.solutionOptions);
                formatter.Serialize(stream, options);
            }
            catch (Exception ex)
            {
                Debug.Fail("Failed to save options", ex.Message);
            }
        }
    }
}
