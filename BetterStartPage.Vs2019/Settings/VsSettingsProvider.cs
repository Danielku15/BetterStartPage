using System;
using System.IO;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterStartPage.Settings
{
    public class VsSettingsProvider : ISettingsProvider
    {
        private const string SettingsRootPrefix = @"StartPage\Settings\BetterStartPage2019";

        private readonly WritableSettingsStore _settingsStore;

        public VsSettingsProvider(IServiceProvider serviceProvider)
        {
            var manager = new ShellSettingsManager(serviceProvider);
            _settingsStore = manager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        private bool EnsureCollection()
        {
            if (_settingsStore == null) return false;
            if (!_settingsStore.CollectionExists(SettingsRootPrefix))
            {
                _settingsStore.CreateCollection(SettingsRootPrefix);
            }
            return true;
        }

        public void Reset()
        {
            _settingsStore.DeleteCollection(SettingsRootPrefix);
        }

        public void WriteInt32(string name, int value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetInt32(SettingsRootPrefix, name, value);
        }

        public int ReadInt32(string name, int defaultValue = 0)
        {
            if (_settingsStore == null) return defaultValue;
            return _settingsStore.GetInt32(SettingsRootPrefix, name, defaultValue);
        }

        public void WriteDouble(string name, double value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetInt64(SettingsRootPrefix, name, BitConverter.DoubleToInt64Bits(value));
        }

        public double ReadDouble(string name, double defaultValue = Double.NaN)
        {
            if (_settingsStore == null) return defaultValue;

            return BitConverter.Int64BitsToDouble(_settingsStore.GetInt64(SettingsRootPrefix, name, BitConverter.DoubleToInt64Bits(defaultValue)));
        }

        public void WriteBytes(string name, byte[] value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetMemoryStream(SettingsRootPrefix, name, new MemoryStream(value));
        }

        public byte[] ReadBytes(string name, byte[] defaultValue = null)
        {
            if (_settingsStore == null) return defaultValue;

            if (!_settingsStore.PropertyExists(SettingsRootPrefix, name))
            {
                return defaultValue;
            }

            return _settingsStore.GetMemoryStream(SettingsRootPrefix, name).ToArray();
        }
    }
}
