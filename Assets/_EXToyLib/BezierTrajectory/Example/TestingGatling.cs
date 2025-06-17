using EXToyLib;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _EXToyLib
{
    public class TestingGatling : MonoBehaviour
    {
        public GameObject prefabBullet;

        public GameObject target;
        
        [Range(0,360)]
        public int angle = 30; // 发射角度
        
        [Range(0,10)]
        public float bezierDistance = 5f; // 贝塞尔曲线控制点距离

        
        public float fireDuration = 1f;
    
        public bool AutoFire;
        
        private float timeCounter = 0f;
        private void Update()
        {
            if (AutoFire && fireDuration>0)
            {
                timeCounter += Time.deltaTime;
                if (!(timeCounter >= fireDuration)) return;
                Fire();
                timeCounter = 0f;
            }
        }
        
        public void Fire()
        {
            if (prefabBullet == null || target == null)
            {
                Debug.LogError("请设置子弹预制体和目标物体");
                return;
            }

            // 实例化子弹
            var bullet = Instantiate(prefabBullet, transform.position, Quaternion.identity);
            
            // 获取贝塞尔轨迹组件
            var bezierTrajectory = bullet.GetComponent<BezierTrajectoryController>();
            if (bezierTrajectory == null)
            {
                bezierTrajectory = bullet.AddComponent<BezierTrajectoryController>();
            }

            bezierTrajectory.bullet = bullet.transform; // 设置子弹的Transform
            
            // 设置起点、终点和控制点
            bezierTrajectory.trajectoryConfig.startPoint = transform.position;
            bezierTrajectory.trajectoryConfig.endPoint = target.transform.position;

            // 启用角度自动计算控制点
            bezierTrajectory.trajectoryConfig.useAngleCalculation = true;
            // 角度随机值设置，范围0到设置的angle
            var random = new Random((uint)System.DateTime.Now.Ticks);
            bezierTrajectory.trajectoryConfig.startAngle = new Vector3(0,random.NextFloat(0, angle) , random.NextFloat(0, angle)); // 设置发射角度
            // 设置控制点随机距离， 范围0到设置的bezierDistance
            bezierTrajectory.trajectoryConfig.controlPointDistance = bezierDistance; // 设置控制点距离
            
            // 注册自动销毁事件
            bezierTrajectory.RegisterOnPlayEnd(() =>
            {
                Destroy(bullet);
            });
            
            // 启动子弹运动
            bullet.GetComponent<BezierTrajectoryController>().Play();
        }
    }
}