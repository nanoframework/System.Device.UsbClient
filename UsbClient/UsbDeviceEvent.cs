// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.

using nanoFramework.Runtime.Events;

namespace System.Device.Usb
{
    internal class UsbDeviceEvent : BaseEvent
    {
        private UsbEventType _eventType;
        private ushort _eventData;
        private ushort _interfaceIndex;

        public UsbEventType EventType { get => _eventType; set => _eventType = value; }

        public ushort EventData { get => _eventData; set => _eventData = value; }

        public ushort InterfaceIndex { get => _interfaceIndex; set => _interfaceIndex = value; }
    }
}
