using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage.Control.Settings
{
    class VsSettingsProvider : ISettingsProvider
    {
        private const string SettingsRootPrefix = @"StartPage\Settings\";

        private readonly ServiceProvider _serviceProvider;
        private IVsWritableSettingsStore _settingsStore;
        private string _settingsRoot;

        private IVsWritableSettingsStore SettingsStore
        {
            get
            {
                if (_settingsStore == null && _serviceProvider != null)
                {
                    var settingsManager = (IVsSettingsManager)_serviceProvider.GetService(typeof(SVsSettingsManager));
                    settingsManager.GetWritableSettingsStore((uint)__VsSettingsScope.SettingsScope_UserSettings, out _settingsStore);
                }
                return _settingsStore;
            }
        }

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

        public VsSettingsProvider(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void WriteString(string name, string value)
        {
            if (_serviceProvider == null) return;
            int exists;
            SettingsStore.CollectionExists(SettingsRoot, out exists);
            if (exists != 1)
            {
                SettingsStore.CreateCollection(SettingsRoot);
            }
            SettingsStore.SetString(SettingsRoot, name, value);
        }

        public string ReadString(string name, string defaultValue = "")
        {
            if (_serviceProvider == null) return defaultValue;
            string value;
            SettingsStore.GetStringOrDefault(SettingsRoot, name, defaultValue, out value);
            return value;
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
            SettingsStore.DeleteCollection(SettingsRoot);
        }

        public void WriteInt32(string name, int value)
        {
            if (_serviceProvider == null) return;
            int exists;
            SettingsStore.CollectionExists(SettingsRoot, out exists);
            if (exists != 1)
            {
                SettingsStore.CreateCollection(SettingsRoot);
            }
            SettingsStore.SetInt(SettingsRoot, name, value);
        }

        public int ReadInt32(string name, int defaultValue = 0)
        {
            if (_serviceProvider == null) return defaultValue;
            int value;
            SettingsStore.GetIntOrDefault(SettingsRoot, name, defaultValue, out value);
            return value;
        }

        public void WriteBytes(string name, byte[] value)
        {
            if (_serviceProvider == null) return;
            SettingsStore.SetBinary(SettingsRoot, name, (uint)value.Length, value);
        }

        public byte[] ReadBytes(string name, byte[] defaultValue = null)
        {
            if (_serviceProvider == null) return defaultValue;
            var actualNumberOfBytes = new uint[1];
            SettingsStore.GetBinary(SettingsRoot, name, 0, null, actualNumberOfBytes);
            var value = new byte[actualNumberOfBytes[0]];
            SettingsStore.GetBinary(SettingsRoot, name, (uint)value.Length, value, actualNumberOfBytes);
            return value;
        }
    }
}
