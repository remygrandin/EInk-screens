using ScreenConnection;

namespace MasterControlService.Config
{
    public class ScreenDescriptor
    {
        public string Id { get; set; } = "NotSet";
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public Rotation Rotation { get; set; } = Rotation.DEG_0;
    }
}
