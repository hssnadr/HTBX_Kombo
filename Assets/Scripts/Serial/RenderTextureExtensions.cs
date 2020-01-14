// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace CRI.HitBoxTemplate.Serial
{
    public static class RenderTextureExtensions
    {
        /// <summary>
        /// Read all the pixels in a RenderTexture and convert it into a Texture2D
        /// </summary>
        /// <param name="rt">The RenderTexture</param>
        /// <returns>A Texture2D</returns>
        public static void GetRTPixels(this RenderTexture rt, ref Texture2D tex)
        {
            // Remember currently active render texture
            var currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            // Read the RenderTexture image into the Texture2D
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            // Restorie previously active render texture
            RenderTexture.active = currentActiveRT;
        }
    }
}