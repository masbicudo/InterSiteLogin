using System.Collections.Generic;

namespace Infra
{
    public class DependencyInfo
    {
        private readonly List<string> files = new List<string>();

        public void Add(string path)
        {
            this.files.Add(path);
        }

        public string[] GetAllFiles()
        {
            return this.files.ToArray();
        }
    }
}