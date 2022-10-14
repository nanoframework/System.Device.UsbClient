// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using nanoFramework.Runtime.Events;

namespace System.Device.Usb
{
    internal class UsbDeviceEventListener : IEventProcessor, IEventListener
    {
        // Map of UsbStream objects
        private readonly UsbStream _usbStream;

        public UsbDeviceEventListener(UsbStream usbStream)
        {
            EventSink.AddEventProcessor(EventCategory.Usb, this);
            EventSink.AddEventListener(EventCategory.Usb, this);

            _usbStream = usbStream;
        }

        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
        {
            return new UsbDeviceEvent()
            {
                // Data1 is packed by PostManagedEvent, so we need to unpack the high word.
                EventType = (UsbEventType)(data1 & 0xFF),

                // Data2 - Low 8 bits are the interface index
                InterfaceIndex = (ushort)(data2 & 0xff)
            };
        }

        public void InitializeForEventSource()
        {
            // This method has to exist
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is UsbDeviceEvent myEvent)
            {
                switch (myEvent.EventType)
                {
                    case UsbEventType.DeviceDisconnected:

                        // call internal event handler
                        _usbStream.OnUsbDeviceConnectionChangedInternal(false);
                        break;

                    case UsbEventType.DeviceConnected:
                        // call internal event handler
                        _usbStream.OnUsbDeviceConnectionChangedInternal(true);
                        break;

                    case UsbEventType.DataAvailable:
                        // fire event, if subscribed
                        _usbStream.OnUsbStreamDataReceivedInternal();
                        break;
                }
            }

            return true;
        }
    }
}
