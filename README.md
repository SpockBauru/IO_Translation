# IO Translation

English fan translation project for Insult Order. The translations are applied while the game is running and do not require replacing or modifying any game files.

## Prerequisites

- [BepInEx 5.4](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.8)
- [XUnity.AutoTranslator](https://github.com/bbepis/XUnity.AutoTranslator)
- [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases)
- [IO_ResizeKeypad](https://github.com/SpockBauru/SpockPlugins_Miconisomi/releases/tag/r5)
- [IO_EnglishLauncher](https://github.com/SpockBauru/SpockPlugins_Miconisomi/releases/tag/r5) (Optional, but recommended)
- [IO_DisableTextScrolling](https://github.com/SpockBauru/SpockPlugins_Miconisomi/releases/tag/r4) (Optional)

## How translations work

Each translation line follows the pattern: 
```
original text=translated text
```
Example: 
```
悟飯=Food
``` 
Lines beginning with `//` are considered comments, i.e., they are not considered in the translation.

The translations are all inside the `Bepinex\Translation\en` folder. They are then split into the following directories:
- `Text` - Text translations that are loaded when the game opens. If there is no translation, then the original untranslated text is displayed in the game, Google Translator is called, the translated text is written to `_AutoGeneratedTranslations.txt` and only then the translation is displayed in the game. The text in this folder can be handled with regular expressions (Regex).
- `Texture` - Contains the translated version of game images. The game images are translated using image editing software. If available the source files (`.psd`, `.xcf`, etc.) files can be found in the project's `Image Source Files` folder.
- `RedirectedResources` - Replaces the texts embedded in the game files. This directory mimics the structure of the game, placing a folder for each resource file (such as the ".unity3d" files). Its must follow the game programming structure, so is not easy to humans edit. For Insult Order, we use the tool [TranslateRedirectedResources.exe](https://github.com/SpockBauru/SpockPlugins_Miconisomi/releases/tag/r9) to convert the regular translations from `text` folder to RedirectedResources. See bellow how to use it.

## How to add or improve translations in GitHub

- If you want to make a simple edit simply open the file in question and click edit. After you are done editing, commit the changes and start a pull request.
- If you have more translations to submit [fork the repository](https://help.github.com/articles/fork-a-repo/). This will make a copy of the original project in your account. Upload your changes to the fork into your account, and then [send a pull request](https://help.github.com/articles/about-pull-requests/) to the original project. Your pull request will be reviewed and accepted after a quality check. Again, avoid raw machine translations. Proper capitalization, punctuation, and spelling is a must.
- Righ now we need help to translate the story and sex dialogues. They are in the files `Text\ADV_ManualTranslations.txt` and `Text\POSITIONS_ManualTranslations.txt` respectively.
  - Inside these files you will see commented lines and uncommented lines. Uncommented lines (without `//` in the beginning) were already translated by some good soul. Commented lines were translated by machine, and must be properly translated by a good human being.
  - To add a translation, first make a proper manual translation of the text on a commented line. After finished, uncomment the line and it will be considered as translated. The goal is to translate everything!
 - If you want to see the changes in your game, use the tool [TranslateRedirectedResources.exe](https://github.com/SpockBauru/SpockPlugins_Miconisomi/releases/tag/r9) to send your translations to the RedirectedResources file. To use it, just extract the file to the game folder (where the Launcher is) and run the program. A message of conclusion will eventually appear in a console window, and its done! Simple like that :)


<!---
## Installation

1. Ensure you have the prerequisites installed.
2. Go to the "releases" page above and download the latest version. 
3. Extract the zip inside your game folder.
--->
