using Unity.Entities;
using UnityEngine;

namespace ECS.AuthoringAndMono
{
    public class ExecuteAuthoring : MonoBehaviour
    {
        public bool MainThread;
        public bool IsJobEntity;
        public bool usePrefab;
        public bool aspect;
        public bool ExecuteGameObjectSync;
        private class ExecuteAuthoringBaker : Baker<ExecuteAuthoring>
        {
            public override void Bake(ExecuteAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                if (authoring.MainThread) AddComponent<MainThread>(entity);
                if (authoring.IsJobEntity) AddComponent<IsJobEntity>(entity);
                if (authoring.usePrefab) AddComponent<UsePrefab>(entity);
                if (authoring.aspect) AddComponent<UseAspect>(entity);
                if (authoring.ExecuteGameObjectSync) AddComponent<ExecuteGameObjectSync>(entity);
            }
        }
    }
    
    public struct MainThread : IComponentData
    {
    }
    
    public struct IsJobEntity : IComponentData
    {
    }

    public struct UsePrefab : IComponentData
    {
    }

    public struct UseAspect : IComponentData
    {
    }

    public struct ExecuteGameObjectSync : IComponentData
    {
        
    }
}