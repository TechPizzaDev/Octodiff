using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Octodiff.CommandLine.Support
{
    class CommandLocator : ICommandLocator
    {
        public ICommandMetadata[] List()
        {
            return
            (from t in assemblyCommands
             let attribute = GetCommandAttribute(t)
             where attribute != null
             select attribute).ToArray();
        }

        public ICommandMetadata Find(string name)
        {
            name = name.Trim().ToLowerInvariant();
            return (from t in assemblyCommands
                let attribute = GetCommandAttribute(t)
                where attribute != null
                where attribute.Name == name || attribute.Aliases.Any(a => a == name)
                select attribute).FirstOrDefault();
        }

        public ICommand Create(ICommandMetadata metadata)
        {
            var name = metadata.Name;
            var found = (from t in assemblyCommands
                let attribute = GetCommandAttribute(t)
                where attribute != null
                where attribute.Name == name || attribute.Aliases.Any(a => a == name)
                select t).FirstOrDefault();

            return found == null ? null : (ICommand) Activator.CreateInstance(found);
        }

        IEnumerable<Type> assemblyCommands =>
            typeof(CommandLocator).GetTypeInfo().Assembly.GetTypes().Where(t => typeof(ICommand).GetTypeInfo().IsAssignableFrom(t));

        ICommandMetadata GetCommandAttribute(Type type)
        {
            return (ICommandMetadata)type.GetTypeInfo().GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault();
        }
    }
}