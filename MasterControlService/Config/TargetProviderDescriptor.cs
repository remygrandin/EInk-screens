using System.Collections.Generic;
using System.Xml.Serialization;

namespace MasterControlService.Config
{
    public class TargetProviderDescriptor
    {
        public string ProviderName { get; set; } = "NotSet";

        [XmlArrayItem("Parameter")]
        public List<MasterModuleCommon.KeyValuePair<string, string>> Parameters { get; set; } = new List<MasterModuleCommon.KeyValuePair<string, string>>();
    }
}
