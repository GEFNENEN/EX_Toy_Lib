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
  在Unity Package Manager中添加git地址:https://github.com/No78Vino/EX_Toy_Lib.git?path=Assets/ExOpenSourcePluginManager
- 使用git clone本仓库，或者直接下载压缩包，然后将[Assets/ExOpenSourcePluginManager]文件夹拷贝到你的项目中即可