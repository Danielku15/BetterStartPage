using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterStartPage.Settings
{
    public class VsSettingsProvider : ISettingsProvider
    {
        private const string SettingsRootPrefix = @"StartPage\Settings\";

        private readonly WritableSettingsStore _settingsStore;
        private string _settingsRoot;

        private string SettingsRoot
        {
            get
            {
                if (_settingsRoot == null)
                {
                    var settingsRoot = new StringBuilder(SettingsRootPrefix);
                    settingsRoot.Append("BetterStartPage");
                    settingsRoot.Append("1.0");

                    _settingsRoot = settingsRoot.ToString();
                }

                return _settingsRoot;
            }
        }

        public VsSettingsProvider(IServiceProvider serviceProvider)
        {
            var manager = new ShellSettingsManager(serviceProvider);
            _settingsStore = manager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        public void WriteString(string name, string value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetString(SettingsRoot, name, value);
        }

        private bool EnsureCollection()
        {
            if (_settingsStore == null) return false;
            if (!_settingsStore.CollectionExists(SettingsRoot))
            {
                _settingsStore.CreateCollection(SettingsRoot);
            }
            return true;
        }

        public string ReadString(string name, string defaultValue = "")
        {
            if (_settingsStore == null) return defaultValue;
            return _settingsStore.GetString(SettingsRoot, name, defaultValue);
        }

        public void WriteBool(string name, bool value)
        {
            WriteInt32(name, value ? 1 : 0);
        }

        public bool ReadBool(string name, bool defaultValue = false)
        {
            int value = ReadInt32(name, -1);
            if (value == -1)
            {
                return defaultValue;
            }
            return value != 0;
        }

        public void Reset()
        {
            _settingsStore.DeleteCollection(SettingsRoot);
        }

        public void WriteInt32(string name, int value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetInt32(SettingsRoot, name, value);
        }

        public int ReadInt32(string name, int defaultValue = 0)
        {
            if (_settingsStore == null) return defaultValue;
            return _settingsStore.GetInt32(SettingsRoot, name, defaultValue);
        }

        public void WriteDouble(string name, double value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetInt64(SettingsRoot, name, BitConverter.DoubleToInt64Bits(value));
        }

        public double ReadDouble(string name, double defaultValue = Double.NaN)
        {
            if (_settingsStore == null) return defaultValue;

            return BitConverter.Int64BitsToDouble(_settingsStore.GetInt64(SettingsRoot, name, BitConverter.DoubleToInt64Bits(defaultValue)));
        }

        public void WriteBytes(string name, byte[] value)
        {
            if (!EnsureCollection()) return;
            _settingsStore.SetMemoryStream(SettingsRoot, name, new MemoryStream(value));
        }

        public byte[] ReadBytes(string name, byte[] defaultValue = null)
        {
            if (_settingsStore == null) return defaultValue;

            if (!_settingsStore.PropertyExists(SettingsRoot, name))
            {
                return defaultValue;
            }

            return _settingsStore.GetMemoryStream(SettingsRoot, name).ToArray();
        }
    }
}
