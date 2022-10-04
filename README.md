[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Device.UsbClient&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_System.Device.UsbClient) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_System.Device.UsbClient&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_System.Device.UsbClient) [![NuGet](https://img.shields.io/nuget/dt/System.Device.UsbClient.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/System.Device.UsbClient/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the .NET **nanoFramework** USB Client repository

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| System.Device.UsbClient | [![Build Status](https://dev.azure.com/nanoframework/System.Device.UsbClient/_apis/build/status/System.Device.UsbClient?branchName=main)](https://dev.azure.com/nanoframework/System.Device.UsbClient/_build/latest?definitionId=99&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/System.Device.UsbClient.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/System.Device.UsbClient/) |

## Usage

### USB Stream

`UsbStream` class provides a seamless interface to a stream that can read from and write to an USB Device that's enumerated as a WinUSB device.
This allows shipping your .NET nanoFramework device as an USB device without the need for any INF file or specific driver instalation.

#### Creating an UsbStream

Creating an `UsbStream` requries 2 parameters: a `Guid` that will be used as the Device Interface ID and a `string` which will be used as the device description for the USB device.

```csharp
private static Guid deviceInterfaceId = new Guid("9e48651c-fa68-4b39-8731-1ee84659aac5");
private static string deviceDescription = "nanoDevice";

// create USB Stream
var usbStream = UsbClient.CreateUsbStream(deviceInterfaceId, deviceDescription);
```

#### Writing to the `UsbStream`

To write to the `UsbStream` just call the `Write()` method just like any other .NET stream. Like this:

```csharp
// buffer with dummy data 
var bufer = new byte[] { 1, 2, 3 };

usbStream.Write(bufer, 0, bufer.Length);
```

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
