// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

namespace System.Device.UsbClient
{
    internal enum UsbEventType : byte
    {
        Invalid = 0,
        DeviceConnected = 1,
        DeviceDisconnected = 2
    }
}
