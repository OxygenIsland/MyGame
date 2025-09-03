using System;

namespace StarWorld.FrameWork
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ModuleDescripeAttribute : Attribute
    {
        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Description { get; private set; }

        public string Category { get; private set; }

        public ModuleDescripeAttribute(string name, string version, string description, string category)
        {
            Name = name;
            Version = version;
            Description = description;
            Category = category;
        }
    }
}