[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.OpenTok/master/Shared/Icon.png "Zebble.OpenTok"


## Zebble.OpenTok

![logo]

A Zebble plugin that allows you to work with OpenTok platform.


[![NuGet](https://img.shields.io/nuget/v/Zebble.OpenTok.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.OpenTok/)

> OpenTok makes it easy to embed high-quality interactive video, voice, messaging, and screen sharing into web and mobile apps and this plugin makes developers able to work with OpenTok in a Zebble application.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.OpenTok/](https://www.nuget.org/packages/Zebble.OpenTok/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage

To use this plugin you need a valid TokBox account with a Token and session, then you can use it like below:

```xml
<OpenTokView UserToken = "You're Token" SessionId = "You're SessionID" />
```

### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| SessionId            | string           | x       | x   | x       |
| UserToken            | string           | x       | x   | x       |


### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| MessageReceived               | Action<string, string&gt;    | x       | x   | x       |

### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| SendEmojiToAll         | void| name -> string| x       | x   | x       |
