using System.Collections.Generic;

namespace StarWorld.FrameWork
{
    public class ModuleConfig
    {
        public string FullName;

        public string Version;

        public string Descript;

        public List<string> SubModules { get; set; }

        public List<string> Controllers { get; set; }
    }
}
