# YGOTzolkin

YGOTzolkin is another <a herf=https://github.com/Fluorohydride/ygopro>YGOPro</a> client implemented on Unity 2018.3  
Currently only the most basic functions of this client are implemented and part of the implementations are temporary. The most important unrealized part is all the indicative effects like the effects when a card is activating, when a chain is disabled or when a token is disappearing. Besides, there are not any artistic style designs. This client can connect to other YGOPro server which is based on <a herf=https://github.com/moecube/srvpro>SRVPro</a>.  

__Differences from YGOPro2__  
Although implemented on Unity as well, this client is designed to provide a new duel experience which will be totally different from <a herf=https://github.com/lllyasviel/YGOProUnity_V2>YGOPro2</a>. The code of this client is not refactored from YGOPro2 but is fully progarmmed from zero. In addition, the engine version and the UI system is different.  

## Build

1. Download and install Unity 2018.3
2. Download and install Visual Studio 2017 or Visual Studio 2019
3. Open ```Assets/main.unity``` with Unity
4. Click ```Assets--Open C# Project``` on menu bar

## Run

* Run in Unity editor
Copy ```strings.conf```, ```lflist.conf```, ```cards.cdb```, ```pics```, and ```texture``` from YGOPro to the project folder.

* Run built executable
Copy ```strings.conf```, ```lflist.conf```, ```cards.cdb```, ```pics```, ```texture``` from YGOPro and ```tzlstrings.conf``` from project folder to the folder of the executable, and then run it.

## Rough TODO List

* Necessary function of YGOPro
  * Indicative effects and animations
  * Expansions(pre-released cards) support
  * Replay mode
  * WindBot integration
  * Old Master Rule support
  * Sound
  * Others
* Art works
  * UI design

## Known Issues

* Render sequence of transparent objects is not correct.