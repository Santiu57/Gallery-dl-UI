UI for [Gallery-dl](https://github.com/mikf/gallery-dl) made with Win Forms.

Uses [Sharp Clipboard](https://github.com/Willy-Kimura/SharpClipboard) to make easier download multiple links by just copying them.

Must have Gallery Dl installed, in config there is a button to install gallery dl with [pip](https://github.com/pypa/pip)

All configurations of the App:

<img width="452" height="323" alt="image" src="https://github.com/user-attachments/assets/7d76494a-ba6f-47b2-a2af-80e86cbb613c" />

Main Window:

<img width="390" height="297" alt="image" src="https://github.com/user-attachments/assets/59c801aa-55fb-42d8-8a98-02913f6223e8" />

You can paste the Urls to the TextBox or just copy them from any site.

The URLs will be placed in the table, getting an estimate of the URL site

You can delete all URLs by clicking the trash button below.

Right clicking any URL will delete the clicked URL

Left clicking any URL will open the clicked URL

Arguments:

<img width="750" height="483" alt="image" src="https://github.com/user-attachments/assets/a7d8e493-d355-46a8-975c-4d12c68d3a76" />


Check [Gallery dl options](https://github.com/mikf/gallery-dl/blob/master/docs/options.md) for more information and commands, 
hovering the mouse over an argument will show a brief description an even an example

Enable or disable the arguments you want to Add, only enabled ones will be used in the downloads and will be manipulable, 
if you can't modify the value, first verify that you have it enabled

Buttons with a folder opens a select directory window, to make easier choose the path

Buttons with "+" will open another window with more options

Button with trash will only clear the textbox of the argument

The Cookies from browser argument is unreliable, look [yt-dlp issue](https://github.com/yt-dlp/yt-dlp/issues/7271) for more info.

Is recommended to extract cookies with an [extension](https://chromewebstore.google.com/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc) and put the cookies path directly into Gallery-Dl settings. EX:

```json
"extractor": {
        "twitter": {
            "cookies": "C:/Users/[USER]/Downloads/Cookies/x.txt"
        }
    },
```

Log:

<img width="1064" height="488" alt="image" src="https://github.com/user-attachments/assets/c567212a-dee7-4d80-956f-e90c5e51322a" />

The App saves what URLs you have downloaded, it doesn't do anything with it so you can always delete it with the trash button, reopen the Logform to see that everithing is deleted

Recomendations:

Change the main Font, the windows scales by the font size so if you want them bigger change the font to bigger number and so on.

Put a directory path in the arguments or on the config file of Gallery-Dl, if there is no directory path all the downloads will go to the same folder of the App

If any download gets an error first verify that is a supported URL (When copying and pasting the App does a URL Verification), if is a supported site try putting your cookies, a lot of sites needs login, an api or a token.

[Install pip](https://pypi.org/project/pip/)

Aditional Info:

You can add multiple Arguments into the extra Argument zone <-Arg1> <value1> <-Arg2> <value2>. just ensure to leave a blank space between values

There is currently 1 filter that is with "cunnyx" for discord, it just removes the "cuuny" to leave with a "X" URL. CunnyX URLs will throw an error

The App checks for Gallery-Dl updates when launched using pip

You can change the images to whatever you want, just keep the names intact or the App wont open or will crash
