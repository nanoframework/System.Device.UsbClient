// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

namespace System.Device.Usb
{
    /// <summary>
    /// Represents the method that will handle the <see cref="UsbStream.DataReceived"/> event of a <see cref="UsbStream"/> object.
    /// </summary>
    /// <param name="sender">The sender of the event, which is the <see cref="UsbStream"/> object.</param>
    /// <param name="e">A <see cref="UsbStreamDataReceivedEventArgs"/> object that contains the event data.</param>
    public delegate void UsbStreamDataReceivedEventHandler(
        object sender,
        UsbStreamDataReceivedEventArgs e);
}
