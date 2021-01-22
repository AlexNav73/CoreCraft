namespace PricingCalc.Model.Engine.Commands
{
    internal class CommandRunner : ICommandRunner
    {
        private readonly IJobService _jobService;

        public CommandRunner(IJobService jobService)
        {
            _jobService = jobService;
        }

        public void Run(ModelCommand command, IBaseModel model)
        {
            _jobService.StartNew(() => model.Run(command), result => model.RaiseEvent(result));
        }
    }
}
