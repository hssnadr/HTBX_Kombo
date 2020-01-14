// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace CRI.HitBoxTemplate.Serial
{
    public static class CameraExtensions
    {
        /// <summary>
        /// Returns the bounds of the camera view.
        /// </summary>
        /// <returns>The bounds of the camera view.</returns>
        public static Bounds GetBounds(this Camera camera)
        {
            int screenWidth = 0;
            int screenHeight = 0;
            try
            {
                if (camera.targetTexture != null)
                {
                    screenWidth = camera.targetTexture.width;
                    screenHeight = camera.targetTexture.height;
                }
                else
                {
                    screenWidth = Display.displays[camera.targetDisplay].systemWidth;
                    screenHeight = Display.displays[camera.targetDisplay].systemHeight;
                }
            }
            catch (IndexOutOfRangeException)
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }

            float screenAspect = (float)screenWidth / (float)screenHeight;
            float cameraHeight = camera.orthographicSize * 2.0f;
            var bounds = new Bounds(
                            (Vector2)camera.transform.position,
                            new Vector3(cameraHeight * screenAspect, cameraHeight, 0.0f)
                        );
            return bounds;
        }
    }
}