using System;
using DefaultNamespace.GAS_ECS.Component;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace.GAS_ECS.AuthoringAndMono
{
    public enum AttributeType
    {
        Fight_Hp,
        Fight_Mp
    }
    
    public class ASCEntityCtrlBase : MonoBehaviour
    {
        public Entity Entity { get; private set; }
        public Entity AttributeSetEntity { get; private set; }

        private EntityManager _entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        private void OnEnable()
        {
            if (Entity == Entity.Null)
            {
                Entity = _entityManager.CreateEntity();
                AttributeSetEntity = _entityManager.CreateEntity();
                // 初始化
                _entityManager.AddComponentData(Entity,
                    new GASAttributeSet()
                    {
                        hp = new GASAttribute(){baseValue = 200,currentValue = 200},
                        mp = new GASAttribute(){baseValue = 100,currentValue = 100},
                    });
            }
        }

        private void OnDisable()
        {
            if (Entity != Entity.Null)
            {
                _entityManager.DestroyEntity(Entity);
                Entity = Entity.Null;
            }
        }

        public void GetAttributeBaseValue(AttributeType attributeType)
        {
            var attrset = _entityManager.GetComponentData<GASAttributeSet>(Entity);
            // if(attributeType == )
            // attrset.hp.baseValue
        }
    }
}