namespace PricingCalc.Model.Engine.Commands
{
    public sealed class ExecutionResult
    {
        public bool IsSuccess { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

        private ExecutionResult()
        {
        }

        public static readonly ExecutionResult Success = new ExecutionResult()
        {
            IsSuccess = true
        };

        public static ExecutionResult Info(string message)
        {
            return new ExecutionResult()
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static ExecutionResult Error(string message)
        {
            return new ExecutionResult()
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}
