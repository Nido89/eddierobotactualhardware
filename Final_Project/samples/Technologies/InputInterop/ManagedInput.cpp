//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ManagedInput.cpp $ $Revision: 3 $
//-----------------------------------------------------------------------
#include "stdafx.h"

namespace Microsoft {
    namespace Robotics {
        namespace Input {


DirectInput::DirectInput()
{
    HRESULT hr = S_OK;
    IDirectInput8 *pDi = NULL;

    try
    {
        _pDi = NULL;

        hr = CoCreateInstance(CLSID_DirectInput8, 
                              NULL, 
                              CLSCTX_INPROC_SERVER,
                              IID_IDirectInput8, 
                              (LPVOID *)&pDi);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        hr = pDi->Initialize(GetModuleHandle(NULL), DIRECTINPUT_VERSION);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }

        _pDi = pDi;
        _pDi->AddRef();
    }
    finally
    {
        if (pDi != NULL)
        {
            pDi->Release();
        }
    }
}

DirectInput::~DirectInput()
{
    if (_pDi != NULL)
    {
        _pDi->Release();
    }
}

DeviceEnumerator::DeviceEnumerator(
    IDirectInput8* pDi
    )
{
    HRESULT hr = S_OK;

    try
    {
        _pDi = pDi;
        _pDi->AddRef();

        _guids = gcnew List<Guid>();
        _index = -1;

        Delegate^ d = gcnew DiEnumDevicesCallback(this, &DeviceEnumerator::EnumDevicesCallback);
        GCHandle hd = GCHandle::Alloc(d);

        IntPtr callback = Marshal::GetFunctionPointerForDelegate(d);

        hr = _pDi->EnumDevices(DI8DEVCLASS_GAMECTRL,
                               (LPDIENUMDEVICESCALLBACKW)callback.ToPointer(),
                               NULL,
                               DIEDFL_ATTACHEDONLY);
        if (FAILED(hr))
        {
            Marshal::ThrowExceptionForHR(hr);
        }
    }
    finally
    {
    }
}

BOOL
DeviceEnumerator::EnumDevicesCallback(
    LPCDIDEVICEINSTANCE lpddi, 
    LPVOID pvRef
    )
{
    pvRef;

    _guids->Add(FromGUID(lpddi->guidInstance));
    return DIENUM_CONTINUE;
}


DeviceEnumerator::~DeviceEnumerator()
{
    if (_pDi != NULL)
    {
        _pDi->Release();
    }
}

bool
DeviceEnumerator::MoveNext()
{
    _index++;

    return (_index < _guids->Count);
}

void
DeviceEnumerator::Reset()
{
    _index = -1;
}

Object^
DeviceEnumerator::Current::get()
{
    return gcnew Device(_pDi, _guids[_index]);
}

        }
    }
}