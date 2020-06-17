using System;

namespace OrmGift.DataModeling
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DataKeyAttribute : Attribute
    {
    }
}