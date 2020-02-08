//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ManagedDevice.h $ $Revision: 3 $
//-----------------------------------------------------------------------
#pragma once

namespace Microsoft {
    namespace Robotics {
        namespace Input {

            public ref class JoystickState 
            {
            public:
                LONG    X;                     /* x-axis position              */
                LONG    Y;                     /* y-axis position              */
                LONG    Z;                     /* z-axis position              */
                LONG    Rx;                    /* x-axis rotation              */
                LONG    Ry;                    /* y-axis rotation              */
                LONG    Rz;                    /* z-axis rotation              */
                cli::array<Int32>^ Sliders;       /* extra axes positions         */
                cli::array<Int32>^ PovHats;      /* POV directions               */
                cli::array<bool>^ Buttons;  /* 32 buttons                   */

                virtual String^ ToString() override;
            };

            delegate BOOL DiEnumDeviceObjectsCallback(LPCDIDEVICEOBJECTINSTANCE lpddoi, LPVOID pvRef);

            public ref class Device
            {
            private:
                IDirectInput8 *_pDi;
                LPDIRECTINPUTDEVICE8 _pDevice;
                int _nButtons;
                int _nSliders;
                int _nPovHats;

                BOOL EnumDeviceObjects(LPCDIDEVICEOBJECTINSTANCE lpddoi, LPVOID pvRef);

            internal:
                Device(IDirectInput8 *pDi, Guid guid);

            public:
                ~Device();

                JoystickState^ GetState();

                property String^ InstanceName
                {
                    String^ get();
                }

                property Guid Instance
                {
                    Guid get();
                }

                property String^ ProductName
                {
                    String^ get();
                }

                property Guid Product
                {
                    Guid get();
                }

                 virtual  String^ ToString() override
                 {
                     return InstanceName;
                 }
            };

        }
    }
}