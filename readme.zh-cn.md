# YGOTzolkin

YGOTzolkin是另一个用Unity 2018.3实现的[YGOPro](https://github.com/Fluorohydride/ygopro)客户端。
目前这个客户端仅实现了最基本的功能并且部分实现是临时的。未实现的最重要的部分是一些具有指示性的特效如当发动一张卡时的特效、当连锁被无效时的特效或当一个衍生物消失时的特效等。此外还没有任何美术风格上的设计。此客户端能够连接到基于[SRVPro](https://github.com/moecube/srvpro)的YGOPro服务器。

__与YGOPro2的不同__  
虽然同样用Unity实现，此客户端旨在提供与[YGOPro2](https://github.com/lllyasviel/YGOProUnity_V2)完全不同的决斗体验。代码并非重构自YGOPro2而是从零开始编写，另外引擎版本和UI系统也不同。

## Build

1. 下载并安装Unity 2018.3
2. 下载并安装Visual Studio 2017 或 Visual Studio 2019
3. 用Unity打开 ```Assets/main.unity```
4. 点击菜单栏 ```Assets--Open C# Project```

## Run

* 在Unity editor中运行  
从YGOPro中复制```strings.conf```、```lflist.conf```、```cards.cdb```、```pics```和```textures```到项目文件夹。

* 运行可执行程序  
从YGOPro中复制```strings.conf```、```lflist.conf```、```cards.cdb```、```pics```、```textures```，从项目文件夹中复制```tzlstrings.conf```到可执行程序的文件夹然后运行。

## 粗略的TODO List

* YGOPro的必要功能
  * 指示性特效和动画
  * 先行卡支持
  * 录像功能
  * 集成WindBot
  * 旧规则支持
  * 音效
  * 其他
* 美工
  * UI设计

## 已知问题

* 透明物体的渲染顺序不正确
