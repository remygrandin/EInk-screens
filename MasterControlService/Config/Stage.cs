namespace MasterControlService.Config
{
    public class Stage
    {
        public int RoutineId { get; set; } = -1;
        public int RepeatCount { get; set; } = 1;
        public int PostExecDelay { get; set; } = 5000;
    }
}
