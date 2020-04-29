// <copyright file="Utility.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.IO;

namespace Bobcat.Common
{
    public static class Utility
    {
        public static byte[] ApplyFrameToMessage(MemoryStream ms)
        {
            var byteArr = new byte[(int)ms.Length + 4];

            byte[] intBytes = BitConverter.GetBytes((int)ms.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            Array.Copy(intBytes, byteArr, intBytes.Length);
            Array.Copy(ms.ToArray(), 0, byteArr, 4, ms.Length);

            return byteArr;
        }

        public static MemoryStream ExtractMessage(byte[] buffer)
        {
            // This method expects the buffer to be framed with a 4 byte integer value indicating how large the message is.
            var frame = new byte[4];
            Array.Copy(buffer, 0, frame, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(frame);

            var dataLength = BitConverter.ToInt32(frame, 0);

            return new MemoryStream(buffer, 4, dataLength);
        }
    }
}
