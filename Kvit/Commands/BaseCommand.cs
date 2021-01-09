using System.CommandLine;

namespace Kvit.Commands
{
    public abstract class BaseCommand : Command
    {
        protected abstract string CommandAlias { get; }
        protected abstract string CommandDescription { get; }

        protected BaseCommand(string name) : base(name)
        {
            Description = CommandDescription;
            AddAlias(CommandAlias);
        }
    }
}