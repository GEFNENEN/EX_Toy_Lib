using System;

namespace DefaultNamespace.GAS_ECS.Tag
{
    public struct GameplayTag
    {
        public readonly int ENUM;
        public readonly int[] Parents;
        public readonly int[] Children;

        public GameplayTag(int tagEnum, int[] parents, int[] children)
        {
            ENUM = tagEnum;
            Parents = parents ?? Array.Empty<int>();
            Children = children ?? Array.Empty<int>();
        }
        
        public bool HasTag(int tag)
        {
            if (ENUM == tag) return true;
            foreach (var pTag in Parents)
                if (pTag == tag)
                    return true;

            return false;
        }

        public bool HasChildTag(int child)
        {
            foreach (var cTag in Children)
                if (cTag == child)
                    return true;
    
            return false;
        }
        
        public bool HasParentTag(int tag)
        {
            foreach (var pTag in Parents)
                if (pTag == tag)
                    return true;

            return false;
        }
        
        public bool HasTag(GameplayTag tag)
        {
            if (this == tag) return true;
            foreach (var pTag in Parents)
                if (pTag == tag.ENUM)
                    return true;

            return false;
        }

        public bool HasChildTag(GameplayTag child)
        {
            foreach (var cTag in Children)
                if (cTag == child.ENUM)
                    return true;

            return false;
        }
        
        public bool HasParentTag(GameplayTag tag)
        {
            foreach (var pTag in Parents)
                if (pTag == tag.ENUM)
                    return true;

            return false;
        }
        
        public static bool operator ==(GameplayTag x, GameplayTag y)
        {
            return x.ENUM == y.ENUM;
        }

        public static bool operator !=(GameplayTag x, GameplayTag y)
        {
            return x.ENUM != y.ENUM;
        }
        
        public bool IsRoot => Parents.Length == 0;
        public bool HasChild => Children.Length > 0;
    }
}