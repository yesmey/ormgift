using System;

namespace OrmGift.DataModeling
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class DataModelAttribute : Attribute
    {
        public string Name { get; }
        public DataModelAttribute()
        {
        }

        public DataModelAttribute(string name)
        {
            Name = name;
        }
    }
}