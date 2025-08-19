# ActivityQueueController 活动队列控制器
## 简介
自工作以来，遇到了很多次关于时序执行一些活动的需求。比如UI里的悬浮Tip，多条提示时要求装入队列一条一条的展示；相机的机位运动，分段执行；等等。
我索性把活动队列抽象出来，整合为了一个更加通用的小框架，方便拓展和使用。

## 相关类与接口说明
### BaseActivity
- 活动抽象基类。活动就是指执行的具体业务，比如播悬浮Tip，相机运镜，人物行为，等等。
  活动的具体逻辑通过重载以下4个函数即可：
    - OnStart() 开始回调
    - OnUpdate() 更新回调
    - OnComplete() 完成回调
    - OnInterrupt() 被打断回调
- ID  
### ActivityQueueController
### ActivityQueueController
- Instance 插件实例:懒加载，初始化会自动创建更新的Host GameObject
- Enable() 启用重力控制
- Disable() 禁用重力控制
- SetGravity(float gravity) 设置重力值
- GetGravity() 获取当前重力值
- SetGroundDetectionMethod(GroundDetectionMethod method) 设置地面检测方法
  -  GroundDetectionMethod.Default:  使用CharacterController默认的地面检测方法isGrounded
  - GroundDetectionMethod.SphereCheck: 使用SphereCheck方法检测地面.在CharacterController底部画一个小球检测范围
- SetGroundDistance(float distance) 设置地面检测用的小球半径
- SetGroundMask(LayerMask layer) 设置地面检测的LayerMask
- Register(CharacterController controller, float rate = 1f) 注册启用重力的CharacterController组件
  - rate: 单个CharacterController的重力强度，默认1.0 
  - 可以通过注册时重力强度的控制，实现个体之间的重力差异
- Unregister(CharacterController controller) 注销CharacterController组件
