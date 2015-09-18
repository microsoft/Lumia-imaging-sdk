//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

#include "pch.h"
#include "ImageProcessingUtils.h"

int ImageProcessingUtils::HueToRgb(int hue)
{
    int r = 0;
    int g = 0;
    int b = 0;

    if (hue < 0 || hue >= 360)
    {
        hue = 0;
    }

    if (hue >= 0 && hue < 60)
    {
        r = 255;
        g = (int)(hue * 4.25);
        b = 0;
    }
    else if (hue >= 60 && hue < 120)
    {
        g = 255;
        r = (int)((120 - hue) * 4.25);
        b = 0;
    }
    else if (hue >= 120 && hue < 180)
    {
        g = 255;
        b = (int)((hue - 120) * 4.25);
        r = 0;
    }
    else if (hue >= 180 && hue < 240)
    {
        b = 255;
        g = (int)((240 - hue) * 4.25);
        r = 0;
    }
    else if (hue >= 240 && hue < 300)
    {
        b = 255;
        r = (int)((hue - 240) * 4.25);
        g = 0;
    }
    else
    {
        r = 255;
        b = (int)((360 - hue) * 4.25);
        g = 0;
    }

    return 0xFF000000 | (r << 16) | (g << 8) | b;
}

bool ImageProcessingUtils::IsPureColor(int color)
{
    int r = (color >> 16) & 0x000000FF;
    int g = (color >> 8) & 0x000000FF;
    int b = color & 0x000000FF;
    return ((r == 0 || g == 0 || b == 0) && (r == 255 || g == 255 || b == 255));
}
