using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Marler.Net
{
    public delegate void CommandFunction(String line);
    public class Command
    {
        public readonly CommandFunction function;
        public readonly String command;
        public readonly String commandLowerCaseInvariant;
        public readonly String[] aliases;
        public readonly String[] aliasesLowerCaseInvariant;

        public readonly String description;
        public Command(CommandFunction function, String command, String description, params String[] aliases)
        {
            this.function = function;

            this.command = command;
            this.commandLowerCaseInvariant = command.ToLower(CultureInfo.InvariantCulture);

            this.aliases = aliases;
            if (aliases == null || aliases.Length <= 0)
            {
                this.aliasesLowerCaseInvariant = null;
            }
            else
            {
                aliasesLowerCaseInvariant = new String[aliases.Length];
                for (int i = 0; i < aliases.Length; i++)
                {
                    aliasesLowerCaseInvariant[i] = aliases[i].ToLower(CultureInfo.InvariantCulture);
                }
            }

            this.description = description;
        }
    }
    public class CommandProcessor
    {
        private readonly Dictionary<String, Command> commandDictionary;
        public CommandProcessor()
        {
            this.commandDictionary = new Dictionary<String, Command>();
        }
        public void AddCommand(Command command)
        {
            //
            // Check that this string command doesn't conflict with anything
            //
            if (commandDictionary.ContainsKey(command.commandLowerCaseInvariant))
            {
                throw new InvalidOperationException(String.Format(
                    "Command '{0}' has already been added", command.command));
            }
            commandDictionary.Add(command.commandLowerCaseInvariant, command);

            if (command.aliases != null)
            {
                for (int i = 0; i < command.aliases.Length; i++)
                {
                    if (commandDictionary.ContainsKey(command.aliasesLowerCaseInvariant[i]))
                    {
                        throw new InvalidOperationException(String.Format(
                            "Command alias '{0}' has already been added", command.aliases[i]));
                    }
                    commandDictionary.Add(command.aliasesLowerCaseInvariant[i], command);
                }
            }
        }

        // returns false if command was not found
        public Boolean ProcessCommandLine(String commandString, String restOfLine)
        {
            Command command;
            if (commandDictionary.TryGetValue(commandString, out command))
            {
                command.function(restOfLine);
                return true;
            }
            return false;
        }
        public void PrintCommands()
        {
            String columnFormat = "{0,-9} {1,-12} {2}";
            Console.WriteLine(columnFormat, "Command", "Alias", "Description");
            Console.WriteLine(columnFormat, "-------", "-----", "-----------");
            foreach (KeyValuePair<String,Command> pair in commandDictionary)
            {
                Command command = pair.Value;
                String aliases = String.Empty;
                if (command.aliases != null && command.aliases.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    int j = 0;
                    for (; j < command.aliases.Length - 1; j++)
                    {
                        builder.Append(command.aliases[j]);
                        builder.Append(",");
                    }
                    builder.Append(command.aliases[j]);
                    aliases = builder.ToString();
                }
                Console.WriteLine(columnFormat, command.command, aliases, command.description);
            }
        }
    }
}