using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BetterStartPage.Control.ViewModel
{
    [DataContract]
    class ProjectGroup : ViewModelBase
    {
        private string _title;
        private IEnumerable<Project> _projects;
        private bool _hasNormalFiles;

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
                var projects = value as INotifyCollectionChanged;
                if (projects != null)
                {
                    projects.CollectionChanged += OnProjectsChanged;
                }
                OnProjectsChanged(value, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void OnProjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasNormalFiles = _projects.Any(p => p.IsNormalFile);
        }

        public bool HasNormalFiles
        {
            get { return _hasNormalFiles; }
            set
            {
                if (value.Equals(_hasNormalFiles)) return;
                _hasNormalFiles = value;
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