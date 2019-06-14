using System;
using System.Collections.Generic;
using BetterStartPage.ViewModel;

namespace BetterStartPage.Settings
{
    public class DummySettingsProvider : ISettingsProvider
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
                    Title = "MyProject Trunk",
                    Projects = new[]
                    {
                        new Project(@"C:\GitProjects\MyProject\Test.sln"),
                        new Project(@"C:\GitProjects\MyProject\All.sln"),
                        new Project(@"C:\GitProjects\MyProject\Logging.sln"),
                        new Project(@"C:\GitProjects\MyProject\Tools.sln"),
                    }
                },
                new ProjectGroup
                {
                    Title = "MyProject 5.1",
                    Projects = new[]
                    {
                        new Project(@"C:\GitProjects\MyProject5.1\Logging.sln"),
                    }
                },
                new ProjectGroup
                {
                    Title = "MyProject 5.0",
                    Projects = new[]
                    {
                        new Project(@"C:\GitProjects\MyProject5.0\Test.sln"),
                        new Project(@"C:\GitProjects\MyProject5.0\All.sln"),
                        new Project(@"C:\GitProjects\MyProject5.0\Logging.sln"),
                        new Project(@"C:\GitProjects\MyProject5.0\Tools.sln"),
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

        public void WriteDouble(string name, double value)
        {
            _settings[name] = value;
        }

        public double ReadDouble(string name, double value = Double.NaN)
        {
            if (!_settings.ContainsKey(name))
            {
                return value;
            }

            return (double)_settings[name];
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
