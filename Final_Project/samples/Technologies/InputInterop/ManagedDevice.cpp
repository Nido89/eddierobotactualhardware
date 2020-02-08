//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ManagedDevice.cpp $ $Revision: 3 $
//-----------------------------------------------------------------------
#include "stdafx.h"

namespace Microsoft {
    namespace Robotics {
        namespace Input {

Device::Device(
    IDirectInput8 *pDi,
    Guid guid)
{
    HRESULT hr = S_OK;
    LPDIRECTINPUTDEVICE8 pDevice = NULL;
    Delegate^ d = gcnew DiEnumDeviceObjectsCallback(this, &Device::EnumDeviceObjects);
    GCHandle gch = GCHandle::Alloc(d);

    try
    {
        _pDevice = NULL;
        _nButtons = 0;
        _nPovHats = 0;
        _nSliders = 0;

        _pDi = pDi;
        _pDi->AddRef();

        hr = _pDi->CreateDevice(::ToGUID(guid), &pDevice, NULL);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        hr = pDevice->SetDataFormat(&c_dfDIJoystick);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        DIPROPDWORD axismode;
        axismode.diph.dwSize = sizeof axismode;
        axismode.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        axismode.diph.dwHow = DIPH_DEVICE;
        axismode.diph.dwObj = 0;
        axismode.dwData = DIPROPAXISMODE_ABS;

        hr = pDevice->SetProperty(DIPROP_AXISMODE, (LPDIPROPHEADER)&axismode);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        _pDevice = pDevice;
        _pDevice->AddRef();


        IntPtr callback = Marshal::GetFunctionPointerForDelegate(d);

        hr = pDevice->EnumObjects((LPDIENUMDEVICEOBJECTSCALLBACK)callback.ToPointer(),
                                  NULL,
                                  DIDFT_ALL);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }
        _pDevice->Acquire();
    }
    finally
    {
        gch.Free();

        if (pDevice != NULL)
        {
            pDevice->Release();
        }
    }
}

BOOL
Device::EnumDeviceObjects(
    LPCDIDEVICEOBJECTINSTANCE lpddoi,
    LPVOID lpvRef
    )
{
    HRESULT hr = S_OK;

    lpvRef;

    if (lpddoi->dwType & DIDFT_AXIS)
    {
        DIPROPRANGE range;
        DIPROPDWORD deadzone;

        // set axis range and deadzone

        range.lMin = -1000;
        range.lMax = 1000;
        range.diph.dwSize = sizeof range;
        range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        range.diph.dwHow = DIPH_BYID;
        range.diph.dwObj = lpddoi->dwType;

        hr = _pDevice->SetProperty(DIPROP_RANGE, (LPDIPROPHEADER)&range);

        deadzone.dwData = 1000;
        deadzone.diph.dwSize = sizeof deadzone;
        deadzone.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        deadzone.diph.dwHow = DIPH_BYID;
        deadzone.diph.dwObj = lpddoi->dwType;

        hr = _pDevice->SetProperty(DIPROP_DEADZONE, (LPDIPROPHEADER)&deadzone);
    }
    if (lpddoi->dwType & DIDFT_BUTTON)
    {
        _nButtons++;
    }
    if (lpddoi->dwType & DIDFT_POV || ::IsEqualGUID(lpddoi->guidType, GUID_POV))
    {
        _nPovHats++;
    }
    if (::IsEqualGUID(lpddoi->guidType,GUID_Slider))
    {
        _nSliders++;
    }
    return DIENUM_CONTINUE;
}


Device::~Device()
{
    if (_pDevice)
    {
        _pDevice->Release();
    }
    if (_pDi)
    {
        _pDi->Release();
    }
}

JoystickState^
Device::GetState()
{
    HRESULT hr = S_OK;
    DIJOYSTATE state = {0};
    JoystickState^ result = nullptr;

    try
    {
        hr = _pDevice->Poll();
        if (FAILED(hr))
        {
            hr = _pDevice->Acquire();
            if (FAILED(hr))
            {
                Marshal::ThrowExceptionForHR(hr);
            }

            hr = _pDevice->Poll();
            if (FAILED(hr))
            {
                Marshal::ThrowExceptionForHR(hr);
            }
        }

        hr = _pDevice->GetDeviceState(sizeof state, &state);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        result = gcnew JoystickState();
        
        result->X = state.lX;
        result->Y = state.lY;
        result->Z = state.lZ;
        result->Rx = state.lRx;
        result->Ry = state.lRy;
        result->Rz = state.lRz;
        
        result->Sliders = gcnew cli::array<Int32>(_nSliders);
        for (int i = 0; i < result->Sliders->Length && i < ARRAYSIZE(state.rglSlider); i++)
        {
            result->Sliders[i] = state.rglSlider[i];
        }

        result->PovHats = gcnew cli::array<Int32>(_nPovHats);
        for (int i = 0; i < result->PovHats->Length && i < ARRAYSIZE(state.rgdwPOV); i++)
        {
            result->PovHats[i] = state.rgdwPOV[i];
        }

        result->Buttons = gcnew cli::array<bool>(_nButtons);
        for (int i = 0; i < result->Buttons->Length && i < ARRAYSIZE(state.rgbButtons);i++)
        {
            result->Buttons[i] = (state.rgbButtons[i] != 0);
        }

    }
    finally
    {
    }

    return result;
}

String^
Device::InstanceName::get()
{
    DIDEVICEINSTANCE info = {0};
    HRESULT hr = S_OK;

    info.dwSize = sizeof info;
    hr = _pDevice->GetDeviceInfo(&info);
    if (FAILED(hr))
    {
        Marshal::ThrowExceptionForHR(hr);
    }

    return Marshal::PtrToStringUni(IntPtr::IntPtr(info.tszInstanceName));
}

String^
Device::ProductName::get()
{
    DIDEVICEINSTANCE info = {0};
    HRESULT hr = S_OK;

    info.dwSize = sizeof info;
    hr = _pDevice->GetDeviceInfo(&info);
    if (FAILED(hr))
    {
        Marshal::ThrowExceptionForHR(hr);
    }

    return Marshal::PtrToStringUni(IntPtr::IntPtr(info.tszProductName));
}

Guid
Device::Instance::get()
{
    DIDEVICEINSTANCE info = {0};
    HRESULT hr = S_OK;

    info.dwSize = sizeof info;
    hr = _pDevice->GetDeviceInfo(&info);
    if (FAILED(hr))
    {
        Marshal::ThrowExceptionForHR(hr);
    }

    return ::FromGUID(info.guidInstance);
}

Guid
Device::Product::get()
{
    DIDEVICEINSTANCE info = {0};
    HRESULT hr = S_OK;

    info.dwSize = sizeof info;
    hr = _pDevice->GetDeviceInfo(&info);
    if (FAILED(hr))
    {
        Marshal::ThrowExceptionForHR(hr);
    }

    return ::FromGUID(info.guidProduct);
}


String^
JoystickState::ToString()
{
    System::Text::StringBuilder^ builder = gcnew System::Text::StringBuilder();

    builder->AppendFormat("X = {0}, Y = {1}, Z = {2}", X, Y, Z);
    builder->AppendLine();

    builder->AppendFormat("Rx = {0}, Ry = {1}, Rz = {2}", Rx, Ry, Rz);

    if (Sliders->Length > 0)
    {
        builder->AppendLine();
        for (int i = 0; i < Sliders->Length; i++)
        {
            builder->AppendFormat("S{0} = {1}, ", i, Sliders[i]);
        }
    }

    if (PovHats->Length > 0)
    {
        builder->AppendLine();
        for (int i = 0; i < PovHats->Length; i++)
        {
            builder->AppendFormat("P{0} = {1}, ", i, PovHats[i]);
        }
    }

    if (Buttons->Length > 0)
    {
        builder->AppendLine();
        for (int i = 0; i < Buttons->Length; i++)
        {
            builder->AppendFormat("B{0} = {1}, ", i, Buttons[i]);
        }
    }

    return builder->ToString();
}

        }
    }
}