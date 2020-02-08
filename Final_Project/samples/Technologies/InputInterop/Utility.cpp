//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Utility.cpp $ $Revision: 3 $
//-----------------------------------------------------------------------
#include "stdafx.h"

Guid 
FromGUID( 
    const _GUID guid 
    ) 
{
   return Guid( guid.Data1, guid.Data2, guid.Data3, 
                guid.Data4[ 0 ], guid.Data4[ 1 ], 
                guid.Data4[ 2 ], guid.Data4[ 3 ], 
                guid.Data4[ 4 ], guid.Data4[ 5 ], 
                guid.Data4[ 6 ], guid.Data4[ 7 ] );
}

_GUID 
ToGUID( 
    Guid& guid 
    ) 
{
   array<Byte>^ guidData = guid.ToByteArray();
   pin_ptr<Byte> data = &(guidData[ 0 ]);

   return *(_GUID *)data;
}

