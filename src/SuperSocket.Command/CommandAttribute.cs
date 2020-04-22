using System;

namespace SuperSocket.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; set; }

        public object Key { get; set; }

        public CommandAttribute()
        {

        }

        public CommandAttribute(string name)
        {

        }

        public CommandAttribute(string name, object key)
            : this(name)
        {
            Key = key;
        }
        
        public CommandAttribute(object enumType)
        {
            if (Enum.TryParse(enumType.GetType(), enumType.ToString(), out object command))
            {
                this.Name = ((int)command).ToString();
            }
        }
    }
}
