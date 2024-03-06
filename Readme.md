[![OhShape](https://ohshapevr.com/wp-content/uploads/2019/07/Asset-33@3x-500x68.png "OhShape")](https://ohshapevr.com)  

# 🎵 OhShape Level Editor
The official level editor for **OhShape** VR Game. 

[![OhShapeEditor](https://ohshapevr.com/ohshape-a-new-vr-rhythm-game/editor_mockup-3/  "OhShapeEditor")](#)

The game is available on:
- [Steam]
- [Oculus Store]
- [HTC Viveport (Viveport & Infinity)]

You can download the last binary of the Level Editor for Windows from [OhShape] web site.
## ℹ Usage

- Put your custom maps on **/My Documents/OhShape/Songs**
- You can **create folders to make albums** and organize your own content

Maybe you want to start watching some nice tutorials using the editor from Youtube:
- [Basic concepts tutorial]
- [Advanced Tutorial I]

### 🚀 HotKeys
- Space - Play / Pause
- n - New wall
- ctrl + c - Copy selection of walls
- ctrl + v - Paste copied walls
- ctrl + s - Save
- ctrl + shift + s - Save as
- Supr - Delete selected walls
- Left/Right arrows or A/D control time bar
- ctrl + m - Mirror (left/right) selected panels
- ctrl + b - Paste mirror copied walls

## ℹ Level format

OhShape Maps are in **.YML** format. Each **.YML** file includes a reference to a song file in **.OGG** format. OhShape includes a demo song ('Ice Flow' by [Kevin MacLeod]). You can open the demo .YML file to get an idea of how easy the format is. This example song is also inclueded in this repository, on the **examples** folder.

## 🔧 Repository contents
- /docs - Documentation and tutorials
- /src - Source code
- /examples - Some level examples

## 🔧 How to compile
### Requisites
Download the editor and build it for Windows with Unity 2017.4.30f1.
If you want to add the video library, buy UniversalMediaPlayer and add this line to `UniversalMediaPlayer/Scripts/UniversalMediaPlayer.cs` class:

```
    public static bool IsValidLibrary()
    {
        return true;
    }
```

Also, you have to delete `Assets/Scripts/UniversalMediaPlayer.cs`

And add this line to `Assets/Scripts/Manager/VideoManager.cs`

```
      using UMP;
```

### Dependences
- [UniversalMediaPlayer]: if you don't have this dependence, you will not be abble to add videos, but the editor stills working

## ℹ Another useful links
- [The OhShape main website]
- [The OhShape Discord]
- [OhShape Twitter]
- [OhShape Instagram]

## 🔧 Release notes

### 1.0.10
- Fix double null
- Optimization wave

### 1.0.9
- Fix open dialog folder (last used)

### 1.0.8
- Fix Width of WA walls

### 1.0.7
- Fix text in pause

### 1.0.6
- Added paste between editors feature
- Width WA keys update
- Fix zoom

### 1.0.5
- Added some ‘hidden’ walls
- Updated the difficulty system
- Updated the new ‘Electro party’ scenario

### 1.0.4
- Posibility to add the scenary you want to use in your songs 😁
- Fix Paste Command when the walls overflow the length of the song
- New Commands to move across the song. Right/Left arrows or A/D keys
- Mirror the selected walls with ctrl + m
- Paste the walls copied as mirrored with ctrl + b

### 1.0.3
- Added beginner mode
- Changing difficulty will change the speed of the song

### 1.0.2
- Fix bug in the GridLines

### 1.0.1
- Fix bug where sometimes, more than one .yml file was created for the same song.
- Add buttons to increase and decrease the Grid Offset and the Grid BPM.
- Grid BPM now accepts two decimal digits.

### 1.0.0
- Add the option to snap walls to the Grid.  
- Change icons for fullscreen/normalscreen and make window on normalscreen resizable.  

### 0.7.8
- Drag walls in the timeline with the left button.
- Superposition of the walls if they are very very closed when the zoom is more than or equal to 8. Also, there are superposition if the walls are at the same time (no matters the zoom). Limit of 5.
- Add new popup when the user tries to exit the editor without saving.
- Add the ability to insert the 66 shapes.
- Fix Grid.
- Save (Ctrl + s), Save As (Ctrl + Shift + s) Hotkeys.

### 0.7.7
- Select multiple Walls with the right button of the mouse.
- Copy (Ctrl + c), Paste (Ctrl + v) and Delete (Supr) Hotkeys for walls.
- A new wall transition while the song is playing. You can now see the shapes in real-time.
- Fix a problem in which sometimes some lines didn't appear.
- Fix a problem in which the frame of the video didn't update when time on the wave was selected.
- The button Save now show an interaction when the file it's saved.  
- Add a new screen at the beginning.  

### 0.7.4
- Buttons of the editor now are selected with the configuration of the shape/hit/dodge/coin. 
- Spacebar now works only for play/pause the song of the editor. It will not activate or deactivate pressed buttons. 
- Fix + and - buttons on TIME input in the shape editor. 
- Change the order of the legs in the shape editor (right and left now are in their correct place). 
- Improve the algorithm of the calculus of the coin position when you drag it. A problem with the values 11 in the x-axis and 5 in the y-axis are fixed. 
- Delete a semi-transparent green background behind the text of the walls. 
- Changes in the wall colors. Now also there is a difference when a wall is selected, not only when it's pressed. 
- Delete popup and backups when 'Save' is pressed. 
- Hotkey 'n' and 'New Wall' now works the same way. It creates a new wall with the same characteristics. 
- Walls with the legs to the left and the right now are recognized as a Shape Wall in the timeline. 
- The algorithm of the zoom has been changed. Now it works by powers of 2. 
- Popup has been deleted when you want to create a wall. Instead, it will delete the wall instantly. Caution! 
- The resolution when the user minimize the Oh Shape Editor is now 1440x880. 
- MarkLine and Text of the walls in timeline are only showed when zoom is 3 or more.  

[OhShape]: <https://ohshapevr.com>
[Steam]: <https://store.steampowered.com/app/1098100/OnShape/>
[Oculus Store]: <https://www.oculus.com/experiences/rift/2125948974167426/>
[HTC Viveport (Viveport & Infinity)]: <https://www.viveport.com/apps/f4e7ef44-f6f3-420e-93a0-fd6ea5bd38df/OhShape/>
[Basic concepts tutorial]:<https://youtu.be/GIHrbcXZna4>
[Advanced Tutorial I]:<https://youtu.be/JMCSHU7YV5U>
[UniversalMediaPlayer]: <https://assetstore.unity.com/packages/tools/video/ump-pro-win-mac-linux-webgl-83281>
[The OhShape main website]: <https://ohshapevr.com/>
[The OhShape Discord]: <https://discord.gg/jAGYAvU>
[OhShape Twitter]: <https://twitter.com/OhShape>  
[OhShape Instagram]: <https://www.instagram.com/ohshape_vr/>
