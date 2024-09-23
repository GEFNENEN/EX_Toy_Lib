using System.Collections.Generic;

namespace DefaultNamespace.GAS_ECS.Tag
{
    public struct GTagLib
    {
        public static readonly GameplayTag Fight = new GameplayTag(
            GTag.Fight.GetHashCode(),
            null,
            new[]{GTag.Fight_Hp.GetHashCode(),GTag.Fight_Mp.GetHashCode()});

        public static readonly Dictionary<int, GameplayTag> Map = new()
        {
            { GTag.Fight.GetHashCode(), Fight },
        };
    }
}