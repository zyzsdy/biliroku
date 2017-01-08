# biliroku
bilibili 生放送（直播）录制

Licensed by GPLv3 详情请见LICENSE文件

BiliRoku是是B站（bilibili）直播内容录制工具。
原理：直接接收直播的视频流并保存到本地。

使用方法：输入房间号。
房间号就是看直播的网址：http://live.bilibili.com/xxxxx
里面xxxxx的数字。

作者：Zyzsdy
（主页 http://zyzsdy.com/biliroku)

运行平台: .NET framework 4.0以上

## 开发工具

VS2015

Clone源代码后使用VS2015或者更高版本打开，直接点击编译，会通过NuGet获取依赖库。

如果你使用比VS2015更高的版本，请在通过Pull Request提交代码时不要包含sln文件。

## MediaInfo

本软件使用了MediaInfo（BSD）的库和代码