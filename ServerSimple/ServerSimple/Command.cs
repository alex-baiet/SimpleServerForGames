using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSimple {
    /// <summary>Used for managing commands.</summary>
    /// <remarks>This class is identical in client and server script</remarks>
    class Command {
        public delegate void CommandAction(string[] args);

        /// <summary>Name of the command.</summary>
        public string Name { get; private set; }
        /// <summary>Description describing who to use the command.</summary>
        public string Description { get; private set; }

        private CommandAction _action;

        /// <param name="action">Function to call when trying to use the command.</param>
        public Command(string name, string description, CommandAction action) {
            Name = name;
            Description = description;
            _action = action;
        }

        /// <summary>Execute the command.</summary>
        public void Execute(string[] args) { _action(args); }
    }
}
