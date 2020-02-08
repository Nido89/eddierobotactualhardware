//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: stdafx.h $ $Revision: 3 $
//-----------------------------------------------------------------------
#pragma once

#define DIRECTINPUT_VERSION 0x0800
#define INITGUID
#include <DInput.h>

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

Guid FromGUID(const _GUID guid);
_GUID ToGUID(Guid& guid);


#include "ManagedDevice.h"
#include "ManagedInput.h"
