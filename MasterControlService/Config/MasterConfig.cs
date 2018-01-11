using System.Collections.Generic;

namespace MasterControlService.Config
{
    public class MasterConfig
    {
        public List<ScreenDescriptor> Screens { get; set; } = new List<ScreenDescriptor>();
        public List<Routine> Routines { get; set; } = new List<Routine>();
        public List<Stage> Sequence { get; set; } = new List<Stage>();
    }
}
