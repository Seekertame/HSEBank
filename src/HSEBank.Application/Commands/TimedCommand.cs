using Microsoft.Extensions.Logging;

namespace HSEBank.Application.Commands
{
    public sealed class TimedCommand : ICommand
    {
        private readonly ICommand _inner;
        private readonly ILogger _log;

        public TimedCommand(ICommand inner, ILogger<TimedCommand> log) { _inner = inner; _log = log; }

        public void Execute()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _inner.Execute();
            sw.Stop();
            _log.LogInformation("Command {Command} executed in {Ms} ms", _inner.GetType().Name, sw.ElapsedMilliseconds);
        }
    }
}