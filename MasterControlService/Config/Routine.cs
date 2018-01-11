namespace MasterControlService.Config
{
    public class Routine
    {
        public int Id { get; set; } = -1;
        public GraphicProviderDescriptor GraphicProvider { get; set; } = new GraphicProviderDescriptor();
        public TargetProviderDescriptor TargetProvider { get; set; } = new TargetProviderDescriptor();
        public TransitionProviderDescriptor TransitionProvider { get; set; } = new TransitionProviderDescriptor();
    }
}
