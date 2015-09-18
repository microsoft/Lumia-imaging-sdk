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

#ifdef _M_ARM
#include <armintr.h>
#endif

#ifndef IMAGEPROCESSINGUTILS_H
#define IMAGEPROCESSINGUTILS_H

namespace ImageProcessingUtils
{
    // Converts a hue into a "pure" color (one that is on the top edge of
    // the HSV color cylinder)
    // hue - The angle on the color circle [0, 360]
    int HueToRgb(int hue);

    // Checks if the given color is a "pure" color (one that is on the top
    // edge of the HSV color cylinder)
    inline bool IsPureColor(int color);

    inline int DIV255(int val)
    {
        return ((val >> 8) + val + 1) >> 8;
    }

    inline int BW(int r, int g, int b)
    {
        return (77 * r + 151 * g + 28 * b) >> 8;
    }

    inline int SAT255(int x)
    {
#ifdef _M_ARM
        return _arm_usat(8, x, _ARM_LSL, 0);
#else
        return (x < 0) ? (0) : (x > 255 ? 255 : x);
#endif
    }

    inline int SAT(int x, int min, int max)
    {
        return (x < min) ? min : (x > max ? max : x);
    }

    inline int ABS(int x)
    {
        return (x > 0) ? x : -x;
    }

    inline int MIN(int a, int b)
    {
        return (a < b) ? a : b;
    }

    inline int MIN(int a, int b, int c)
    {
        return MIN(MIN(a, b), c);
    }

    inline int MAX(int a, int b)
    {
        return (a > b) ? a : b;
    }

    inline int MAX(int a, int b, int c)
    {
        return MAX(MAX(a, b), c);
    }

    inline float ABS(float x)
    {
        return (x > 0) ? x : -x;
    }

    inline float MIN(float a, float b)
    {
        return (a < b) ? a : b;
    }

    inline float MIN(float a, float b, float c)
    {
        return MIN(MIN(a, b), c);
    }

    inline float MAX(float a, float b)
    {
        return (a > b) ? a : b;
    }

    inline float MAX(float a, float b, float c)
    {
        return MAX(MAX(a, b), c);
    }
}

#endif // IMAGEPROCESSINGUTILS_H
