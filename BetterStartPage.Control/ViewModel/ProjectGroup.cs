using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BetterStartPage.Control.ViewModel
{
    [DataContract]
    class ProjectGroup : ViewModelBase
    {
        private string _title;
        private IEnumerable<Project> _projects;

        [DataMember]
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public IEnumerable<Project> Projects
        {
            get { return _projects; }
            set
            {
                if (Equals(value, _projects)) return;
                _projects = value;
                OnPropertyChanged();
            }
        }

        public ProjectGroup()
        {
            Projects = new ObservableCollection<Project>();
        }

        public static ProjectGroup[] Deserialize(byte[] raw)
        {
            var formatter = new DataContractSerializer(typeof (ProjectGroup[]));
            using (var ms = new MemoryStream(raw))
            {
                return (ProjectGroup[]) formatter.ReadObject(ms);
            }
        }

        public static byte[] Serialize(ProjectGroup[] groups)
        {
            var formatter = new DataContractSerializer(typeof (ProjectGroup[]));
            using (var ms = new MemoryStream())
            {
                formatter.WriteObject(ms, groups);
                return ms.ToArray();
            }
        }
    }
}