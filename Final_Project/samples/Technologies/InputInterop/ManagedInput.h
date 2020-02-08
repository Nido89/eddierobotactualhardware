//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ManagedInput.h $ $Revision: 3 $
//-----------------------------------------------------------------------
#pragma once

namespace Microsoft { 
    namespace Robotics { 
        namespace Input {

            delegate BOOL DiEnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef);

            ref class DeviceEnumerator : public System::Collections::IEnumerator
            {
            private:
                IDirectInput8 *_pDi;
                List<Guid>^ _guids;
                int _index;

                BOOL EnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef);

            public:
                DeviceEnumerator(IDirectInput8* pDi);
                ~DeviceEnumerator();

                property Object^ Current
                {
                    virtual Object^ get();
                }

                virtual bool MoveNext();

                virtual void Reset();
            };

            public ref class DeviceCollection : public System::Collections::IEnumerable
            {
            private:
                IDirectInput8 *_pDi;

            internal:
                DeviceCollection(IDirectInput8 *pDi)
                {
                    _pDi = pDi;
                    _pDi->AddRef();
                }

            public:
                ~DeviceCollection()
                {
                    _pDi->Release();
                }

                virtual System::Collections::IEnumerator^ GetEnumerator()
                {
                    return gcnew DeviceEnumerator(_pDi);
                }
            };

            public ref class DirectInput
            {
            private:
                IDirectInput8 *_pDi;

            public:
                DirectInput();
                ~DirectInput();

                property DeviceCollection^ Devices
                {
                    DeviceCollection^ get()
                    {
                        return gcnew DeviceCollection(_pDi);
                    }
                }
            };

        }
    }
}