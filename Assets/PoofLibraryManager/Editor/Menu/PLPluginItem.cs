using UnityEngine.Serialization;

namespace PoofLibraryManager.Editor
{
    [System.Serializable]
    public class PLPluginItem
    {
        public string Name;
        public string Intro;
        
        public string MenuPath;
        public string[] Tags;
        
        public string LocalPath;
        public string GitURL_Path;
        
        public string GitURL_Username;
        public string GitURL_RepoName;
        public string GitURL_Branch;
    }
}