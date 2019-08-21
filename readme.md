# YGOTzolkin

YGOTzolkin is another [YGOPro](https://github.com/Fluorohydride/ygopro) client implemented on Unity 2018.3  
Currently only the most basic functions of this client are implemented and part of the implementations are temporary. The most important unrealized part is all the indicative effects like the effects when a card is activated, when a chain is disabled or when a token is disappearing. Besides, there are not any artistic style designs. This client can connect to other YGOPro server which is based on [SRVPro](https://github.com/moecube/srvpro).  

__Differences from YGOPro2__  
Although implemented on Unity as well, this client is designed to provide a new duel experience which will be totally different from [YGOPro2](https://github.com/lllyasviel/YGOProUnity_V2). The code of this client is not refactored from YGOPro2 but is fully programmed from zero. In addition, the engine version and the UI system are different.  

## Build

1. Download and install Unity 2018.3.5
2. Download and install Visual Studio 2017 or Visual Studio 2019
3. Open ```Assets/main.unity``` with Unity
4. Click ```Assets--Open C# Project``` on menu bar

## Run

* Run in Unity editor  
Copy ```strings.conf```, ```lflist.conf```, ```cards.cdb```, ```pics```, and ```textures``` from YGOPro to the project folder.

* Run built executable  
Copy ```strings.conf```, ```lflist.conf```, ```cards.cdb```, ```pics```, ```textures``` from YGOPro and ```tzlstrings.conf``` from project folder to the folder of the executable, and then run it.

## Rough TODO List

* Necessary functions of YGOPro
  * Indicative effects and animations
  * Expansions (pre-released cards) support
  * Replay mode
  * LAN mode
  * WindBot integration
  * Old Master Rule support
  * Sounds
  * Miscellaneous
* Art works
  * UI design

## Known Issues

* Render sequence of transparent objects is not correct.
