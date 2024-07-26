
The original project is [MaxLauncher 1.31.0.0](https://sourceforge.net/projects/maxlauncher/files/MaxLauncher/1.31.0.0/) by madproton.

This project adds the following features (so may be named as version 1.32.0.0 :)):
- Import `.mld` data file. An Import menu is added to the Data menu
- Check duplicated Buttons in Tabs. The report is listed in the log file `FileImportLog.md` 

<img src="https://raw.githubusercontent.com/Megre/Media/main/image-20240726201228114.png" alt="image-20240726201228114" style="zoom: 33%;" />

I have several computers with the same portable applications at the same path (`mklink` tool can be used if not). Sometimes different new apps are added to the two computers, but they will be synchronized later. I modifed the `.mld` in both computers. I wish the `.mld` can be synchronized too. Thus, this project aimed to:
- Allow you modify `ComputerA.mld` and `ComputerB.mld` respectively
- Support synchronization of `.mld` files by importing `ComputerB.mld` to `ComputerA.mld` and vice versa

If you imports `Other.mld` to `Current.mld`, buttons in `Other.mld` will be copied to `Current.mld` with the same Tab name/ID. A new Tab will be created in `Current.mld` while no Tab with the same name exists. Duplicated buttons (identified by file path and name) will not be copied to `Current.mld`, but reported in the log file `FileImportLog.md`. If the target Tab is full, not copied buttons will be logged too.

The log file `FileImportLog.md` will be showed while the import completes. Or you can find it in the same folder as `MaxLauncher.exe`.

