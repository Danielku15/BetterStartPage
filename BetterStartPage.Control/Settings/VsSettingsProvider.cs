using System.Text;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BetterStartPage.Control.Settings
{
    internal class VsSettingsProvider : ISettingsProvider
    {
        private const string SettingsRootPrefix = @"StartPage\Settings\";

        private readonly IVsWritableSettingsStore _settingsStore;
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

        public VsSettingsProvider(DTE2 dte)
        {
            var serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte);
            var settingsManager = (IVsSettingsManager)serviceProvider.GetService(typeof(SVsSettingsManager));
            settingsManager.GetWritableSettingsStore((uint)__VsSettingsScope.SettingsScope_UserSettings, out _settingsStore);
        }

        public void WriteString(string name, string value)
        {
            if (_settingsStore == null) return;
            int exists;
            _settingsStore.CollectionExists(SettingsRoot, out exists);
            if (exists != 1)
            {
                _settingsStore.CreateCollection(SettingsRoot);
            }
            _settingsStore.SetString(SettingsRoot, name, value);
        }

        public string ReadString(string name, string defaultValue = "")
        {
            if (_settingsStore == null) return defaultValue;
            string value;
            _settingsStore.GetStringOrDefault(SettingsRoot, name, defaultValue, out value);
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
            _settingsStore.DeleteCollection(SettingsRoot);
        }

        public void WriteInt32(string name, int value)
        {
            if (_settingsStore == null) return;
            int exists;
            _settingsStore.CollectionExists(SettingsRoot, out exists);
            if (exists != 1)
            {
                _settingsStore.CreateCollection(SettingsRoot);
            }
            _settingsStore.SetInt(SettingsRoot, name, value);
        }

        public int ReadInt32(string name, int defaultValue = 0)
        {
            if (_settingsStore == null) return defaultValue;
            int value;
            _settingsStore.GetIntOrDefault(SettingsRoot, name, defaultValue, out value);
            return value;
        }

        public void WriteBytes(string name, byte[] value)
        {
            if (_settingsStore == null) return;
            _settingsStore.SetBinary(SettingsRoot, name, (uint)value.Length, value);
        }

        public byte[] ReadBytes(string name, byte[] defaultValue = null)
        {
            if (_settingsStore == null) return defaultValue;
            var actualNumberOfBytes = new uint[1];
            _settingsStore.GetBinary(SettingsRoot, name, 0, null, actualNumberOfBytes);
            var value = new byte[actualNumberOfBytes[0]];
            _settingsStore.GetBinary(SettingsRoot, name, (uint)value.Length, value, actualNumberOfBytes);
            return value;
        }
    }
}
