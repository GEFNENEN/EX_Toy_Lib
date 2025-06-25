# EX开源插件管理器
## 前言
也许你自己平时在github开发或者发现了一些大大小小的好用插件，但每次要用的时候，又需要来来回回去github的主页收藏夹里一个一个的捞出来。
捞的时候呢，有时又还不能全捞，只捞指定的一个文件夹里的东西。
我这个闲人，平时自己写插件，被这个问题来来回回困扰了不少，UPM，Nuget或多或少都有自己用起来麻烦的地方，或者直接说他们缺一个自定义的收藏夹。
在网上也搜了不少解决方案。果不其然，有不少团队为了解决这个，都自己开发了一个小型UPM。不过都是不公开的，毕竟是各司内部自己开发的框架插件。

那既然找不到公开，我就自己快速写一个。现在国内的AI辅助也算不错了，插件工具写起来已经很方便了，费不了多少时间。我就花了3个晚上的时间，把这工具给写完了。
不过时间比较短，有些地方还是不够完善，后边自己用的时候会更新优化。

## 使用说明
### 安装
1. 导入Odin Inspector插件(付费),Odin Inspector来源请自行解决。建议使用3.2+版本。
2. 导入本插件，以下两种方式：
- 使用Unity Package Manager安装
  - 在Unity Package Manager中添加git地址:https://github.com/No78Vino/EX_Toy_Lib.git?path=Assets/ExOpenSourcePluginManager
- 使用git clone本仓库，或者直接下载压缩包.
  - 下载完项目后，将[Assets/ExOpenSourcePluginManager]文件夹拷贝到你的项目中
  - 然后再去Unity Package Manager中安装本插件依赖的Editor Coroutines工具库。

### 打开
Unity顶部工具栏：EX_Tool -> EX开源插件管理器（Github）
### 配置
- 打开管理器后，你可以粗略的看一眼首页。
- 来到左侧页签栏，选择【设置】。从上到下依次可以看到：
  - GitHub Token：访问令牌（可选）。
    - 我们都知道，github的匿名访问是限流的，60次/每小时。有了令牌就可以解除限制。
    - 如果你自己创建的目录，或者其中包含的小插件，链接的是你的私有仓库，那么令牌就是必要的。
    - 令牌是个人安全信息，所以不能存储于工程里。请把令牌保存在一个txt文件内，放置于安全的非工程路径。
  - 网络测试：用于检测你当前的网络情况，DNS，git令牌是否都正常
  - 连接的仓库配置：相当于自定义收藏夹，他连接的是你自建的插件菜单JSON文件
    - git用户名，仓库名，分支名，远端菜单路径：这些是用于定位菜单文件的基础信息。实际是拼接出URL，格式：
      https://raw.githubusercontent.com/用户名/仓库名/分支名/远端菜单路径
    - 本地菜单路径：下载菜单的本地路径
### Menu.json文件规范
菜单json文件就是本管理器的核心了，相当于一个自定义收藏夹。
结合例子来说明：
```
{
  "Version": "1.0",
  "Name": "测试插件库A",
  "Owner": "作者A",
  "Intro": "插件库简介A",
  "DefaultGit_UserName": "authorA",
  "DefaultGit_RepoName": "testRepoA",
  "DefaultGit_Branch": "main",
  "Plugins": [
    {
      "Name": "音乐播放器",
      "Version": "1.0",
      "Tags": [
        "music",
        "播放器"
      ],
      "MenuPath": "测试插件库A/音乐播放器",
      "LocalPath": "Assets/TestRepoA/MusicPlayer",
      "GitURL_Username": "authorB",
      "GitURL_RepoName": "testRepoB",
      "GitURL_Branch": "main",
      "GitURL_Path": "Assets/MusicPlayer",
      "Intro": "音乐播放器简介"
    },
    {
      "Name": "龙卷风摧毁停车场模拟器",
      "Version": "1.0",
      "Tags": [
        "模拟器",
        "天气"
      ],
      "MenuPath": "测试插件库A/龙卷风摧毁停车场模拟器",
      "LocalPath": "Assets/TestRepoA/Tornado",
      "GitURL_Path": "Assets/Tornado",
      "Intro": "龙卷风摧毁停车场模拟器简介"
    }
  ]
}
```
- Version: 菜单版本号。这个版本号其实没什么实际作用，真更新了，还是以Plugins变化为准。
- Name：这个收藏夹菜单的名字
- Owner：菜单拥有者
- Intro：菜单收藏简介
- DefaultGit_UserName：默认git用户名。
  - 插件Plugin的默认连接仓库地址。
  - 如果插件没有标明具体的来源git仓库的信息，就会使用默认的信息
  - 设计这个的意思就是，你的菜单收藏夹里，可以搜罗来自不同仓库，不同分支的仓库。
  - 当然前提是这些仓库是public，或者token用户的私有仓库
- DefaultGit_RepoName：默认git仓库名。同上
- DefaultGit_Branch：默认git分支名。同上
- Plugins：插件列表，每次有新的插件加入，只需要在这个列表里添加即可
  - 来介绍一下Plugin的信息规范
  - Name：插件名
  - Version：插件版本号
  - Intro：插件介绍
  - Tags：插件描述用标签。这个是预留给过滤功能用的参数。现在还没做。
  - MenuPath：菜单目录树路径。
    - s 
  - GitURL_Path：远端git仓库里插件文件夹的路径
  - LocalPath：本地工程安装插件的路径
  - GitURL_Username:
  - GitURL_RepoName:
  - GitURL_Branch: