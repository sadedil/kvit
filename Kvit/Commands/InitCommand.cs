using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Kvit.Commands
{
    public class InitCommand : BaseCommand
    {
        protected override string CommandAlias => "i";
        protected override string CommandDescription => "Creates a .kvit folder and makes the first contact";

        public InitCommand() : base("init")
        {
            var arg = new Argument<string>("filename")
            {
                Arity = new ArgumentArity(0, 1),
                Description = "An option whose argument is parsed as a bool",
            };
            AddArgument(arg);
            Handler = CommandHandler.Create<string>(Execute);
        }

        private void Execute(string filename)
        {
            Console.WriteLine($"Init completed. Filename: {filename}");
        }
    }
}