using System;
using System.Collections.Generic;
using System.Text;

namespace Marler.NetworkTools
{
    public class StringCommand
    {
        public readonly Int32 id;
        public readonly String command;
        public readonly String description;
        public readonly String[] aliases;

        public StringCommand(Int32 id, String command, String description)
        {
            this.id = id;
            this.command = command;
            this.description = description;
            this.aliases = null;
        }
        public StringCommand(Int32 id, String command, String description, params String[] aliases)
        {
            this.id = id;
            this.command = command;
            this.description = description;
            this.aliases = aliases;
        }
    }

    public class CommandDictionary
    {
        private readonly List<StringCommand> stringCommandList;
        private readonly Dictionary<String, StringCommand> lookupDictionary;

        public CommandDictionary()
        {
            this.stringCommandList = new List<StringCommand>();
            this.lookupDictionary = new Dictionary<String, StringCommand>();
        }

        public void AddCommand(StringCommand stringCommand)
        {
            if (stringCommand == null) throw new ArgumentNullException("stringCommand");

            //
            // Check that this string command doesn't conflict with anything
            //
            if (lookupDictionary.ContainsKey(stringCommand.command))
            {
                throw new InvalidOperationException(String.Format(
                    "CommandDictionary already contains command string '{0}'", stringCommand.command));
            }
            if (stringCommand.aliases != null)
            {
                for (int i = 0; i < stringCommand.aliases.Length; i++)
                {
                    if (lookupDictionary.ContainsKey(stringCommand.aliases[i]))
                    {
                        throw new InvalidOperationException(String.Format(
                            "CommandDictionary already contains command string '{0}'", stringCommand.aliases[i]));
                    }
                }
            }
            
            //
            // Since there are no conflics, we can add it
            //
            stringCommandList.Add(stringCommand);
            lookupDictionary.Add(stringCommand.command, stringCommand);
            if (stringCommand.aliases != null)
            {
                for (int i = 0; i < stringCommand.aliases.Length; i++)
                {
                    lookupDictionary.Add(stringCommand.aliases[i], stringCommand);
                }
            }
        }

        public StringCommand TryGetCommand(String commandString)
        {
            StringCommand stringCommand;
            if (lookupDictionary.TryGetValue(commandString, out stringCommand))
            {
                return stringCommand;
            }
            return null;
        }

        public Int32 TryGetCommandID(String commandString)
        {
            StringCommand stringCommand;
            if (lookupDictionary.TryGetValue(commandString, out stringCommand))
            {
                return stringCommand.id;
            }
            return -1;
        }

        public Boolean TryGetCommandID(String commandString, out Int32 id)
        {
            StringCommand stringCommand;
            if (lookupDictionary.TryGetValue(commandString, out stringCommand))
            {
                id = stringCommand.id;
                return true;
            }
            id = -1;
            return false;
        }

        public Int32 GetCommandID(String commandString)
        {
            StringCommand stringCommand;
            if (lookupDictionary.TryGetValue(commandString, out stringCommand))
            {
                return stringCommand.id;
            }
            throw new KeyNotFoundException(String.Format("Could not find command '{0}'", commandString));
        }

        public void PrintCommands()
        {
            Console.WriteLine("{0,-9} {1,-12} {2}", "Command", "Alias", "Description");
            Console.WriteLine("{0,-9} {1,-12} {2}", "-------", "-----", "-----------");
            foreach (StringCommand stringCommand in stringCommandList)
            {
                String aliases = String.Empty;
                if (stringCommand.aliases != null && stringCommand.aliases.Length > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    int j = 0;
                    for (; j < stringCommand.aliases.Length - 1; j++)
                    {
                        builder.Append(stringCommand.aliases[j]);
                        builder.Append(",");
                    }
                    builder.Append(stringCommand.aliases[j]);
                    aliases = builder.ToString();
                }
                Console.WriteLine("{0,-9} {1,-12} {2}", stringCommand.command, aliases, stringCommand.description);
            }
        }

    }
}