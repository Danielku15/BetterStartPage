using System.IO;
using System.Runtime.Serialization;
using BetterStartPage.ViewModel;

namespace BetterStartPage.Control.Settings
{
    [DataContract]
    class SettingsExportViewModel
    {
        [DataMember]
        public ProjectGroup[] ProjectGroups { get; set; }
        [DataMember]
        public int GroupColumns { get; set; }
        [DataMember]
        public int ProjectColumns { get; set; }

        public static SettingsExportViewModel Deserialize(byte[] raw)
        {
            var formatter = new DataContractSerializer(typeof(SettingsExportViewModel));
            using (var ms = new MemoryStream(raw))
            {
                return (SettingsExportViewModel)formatter.ReadObject(ms);
            }
        }

        public static byte[] Serialize(SettingsExportViewModel model)
        {
            var formatter = new DataContractSerializer(typeof(SettingsExportViewModel));
            using (var ms = new MemoryStream())
            {
                formatter.WriteObject(ms, model);
                return ms.ToArray();
            }
        }
    }
}
