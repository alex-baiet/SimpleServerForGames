using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimple {
    class Command {
        public delegate void CommandAction(string[] args);
        
        public string Name { get; private set; }
        public string Description { get; private set; }


        private CommandAction _action;

        public Command(string name, string description, CommandAction action) {
            Name = name;
            Description = description;
            _action = action;
        }

        public void Execute(string[] args) { _action(args); }


    }
}
