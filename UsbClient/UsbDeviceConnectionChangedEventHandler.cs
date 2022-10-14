// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using System.Collections;
using System.Runtime.CompilerServices;
using nanoFramework.Runtime.Events;

namespace System.Device.Usb
{
    /// <summary>
    /// Indicates a change in the connection state of the USB device.
    /// </summary>
    /// <param name="sender">Specifies the object that sent the USB device connection state changed event. </param>
    /// <param name="e">Contains the connection changed event arguments. </param>
    public delegate void UsbDeviceConnectionChangedEventHandler(object sender, DeviceConnectionEventArgs e);
}
