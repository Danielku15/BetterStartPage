using System.Collections.Generic;
using BetterStartPage.Control.ViewModel;

namespace BetterStartPage.Control.Settings
{
    class DummySettingsProvider : ISettingsProvider
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public DummySettingsProvider()
        {
            _settings["GroupColumns"] = 2;
            _settings["ProjectWidth"] = 300;
            _settings["ShowIcons"] = true;
            _settings["StoredGroups"] = ProjectGroup.Serialize(new[]
            {
                new ProjectGroup
                {
                    Title = "Spider Trunk",
                    Projects = new[]
                    {
                        new Project(@"C:\PerfProjects\Spider\SBC\src\SBC.sln"),
                        new Project(@"C:\PerfProjects\Spider\Spider.All.sln"),
                        new Project(@"C:\PerfProjects\Spider\Spider.Logging.sln"),
                        new Project(@"C:\PerfProjects\Spider\Spider.Tools.sln"),
                    }
                },
                new ProjectGroup
                {
                    Title = "Spider 5.1",
                    Projects = new[]
                    {
                        new Project(@"C:\PerfProjects\Spider5.1\SBC\src\SBC.sln"),
                    }
                },
                new ProjectGroup
                {
                    Title = "Spider 5.0",
                    Projects = new[]
                    {
                        new Project(@"C:\PerfProjects\Spider5.0\SBC\src\SBC.sln"),
                        new Project(@"C:\PerfProjects\Spider5.0\Spider.All.sln"),
                        new Project(@"C:\PerfProjects\Spider5.0\Spider.Logging.sln"),
                        new Project(@"C:\PerfProjects\Spider5.0\Spider.Tools.sln"),
                    }
                }
            });
        }

        public void WriteString(string name, string value)
        {
            _settings[name] = value;
        }

        public string ReadString(string name, string defaultValue = "")
        {
            if (!_settings.ContainsKey(name))
            {
                return defaultValue;
            }
            return (string)_settings[name];
        }

        public void WriteBool(string name, bool value)
        {
            _settings[name] = value;
        }

        public bool ReadBool(string name, bool defaultValue = false)
        {
            if (!_settings.ContainsKey(name))
            {
                return defaultValue;
            }
            return (bool)_settings[name];
        }

        public void Reset()
        {
            _settings.Clear();
        }

        public void WriteInt32(string name, int value)
        {
            _settings[name] = value;
        }

        public int ReadInt32(string name, int defaultValue = 0)
        {
            if (!_settings.ContainsKey(name))
            {
                return defaultValue;
            }
            return (int)_settings[name];
        }

        public void WriteBytes(string name, byte[] value)
        {
            _settings[name] = value;
        }

        public byte[] ReadBytes(string name, byte[] defaultValue = null)
        {
            if (!_settings.ContainsKey(name))
            {
                return defaultValue;
            }
            return (byte[])_settings[name];
        }
    }
}
