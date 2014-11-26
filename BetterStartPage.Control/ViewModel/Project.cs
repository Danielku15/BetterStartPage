using System;
using System.IO;
using System.Runtime.Serialization;

namespace BetterStartPage.Control.ViewModel
{
    [DataContract]
    class Project : ViewModelBase
    {
        private Uri _fileInfo;

        public string Name
        {
            get
            {
                if (_fileInfo.IsFile)
                {
                    return Path.GetFileName(FullName);
                }
                return _fileInfo.Host;
            }
        }

        public string DirectoryName
        {
            get
            {
                if (_fileInfo.IsFile)
                {
                    return Path.GetDirectoryName(FullName);
                }
                return _fileInfo.ToString();
            }
        }

        [DataMember]
        public string FullName
        {
            get
            {
                if (_fileInfo.IsFile)
                {
                    return _fileInfo.LocalPath;
                }
                return _fileInfo.ToString();
            }
            set
            {
                _fileInfo = new Uri(value);
                OnPropertyChanged();
                OnPropertyChanged("Name");
                OnPropertyChanged("DirectoryName");
            }
        }

        public bool IsNormalFile
        {
            get
            {
                var extension = Path.GetExtension(Name);
                if (extension == null) return true;
                if (extension.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase)) return false;
                if (extension.EndsWith("proj", StringComparison.InvariantCultureIgnoreCase)) return false;
                return true;
            }
        }

        public Project()
        {
        }

        public Project(string fullName)
        {
            FullName = fullName;
        }
    }
}
