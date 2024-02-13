// Copyright 2022-2024 Niantic.

using System;
using System.Runtime.InteropServices;
using Niantic.Lightship.AR.Utilities.Logging;
using Niantic.Lightship.AR.Utilities;
using Niantic.Lightship.AR.Utilities.Profiling;
using Niantic.Lightship.AR.Utilities.Textures;
using Niantic.Lightship.Spaces;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;

namespace Niantic.Lightship.AR.PAM
{
    internal static class ImageConverter
    {
        private static Material s_material;
        private static readonly int s_unityDisplayTransform = Shader.PropertyToID("_UnityDisplayTransform");

        /// Converts an existing XRCpuImage into the specified width and height ratio as well as the targeted format.
        /// Conversion includes only scaling. Will then write the result into the destination pointer, that should be
        /// acquired from a correctly sized NativeArray<byte>. If the input image is invalid, will do nothing.
        public static void ConvertOnCpuAndWriteToMemory
        (
            this XRCpuImage image,
            Vector2Int outputResolution,
            IntPtr destinationPointer,
            TextureFormat outputFormat = TextureFormat.RGBA32
        )
        {
            if (!image.valid)
                return;

            // Downscaling should not change the orientation
            Debug.Assert(outputResolution.x > outputResolution.y);

            // Calculate cropping
            var xMin = 0;
            var xMax = image.width;
            var yMin = 0;
            var yMax = image.height;

            float inputRatio = (float)image.width / image.height;
            float outputRatio = (float)outputResolution.x / outputResolution.y;

            if (inputRatio > outputRatio)
            {
                // We need to crop along the X axis
                var scale = (outputResolution.x * (float)image.height / outputResolution.y) / image.width;
                var translate = (1.0f - scale) * 0.5f;
                xMin = Mathf.FloorToInt(translate * image.width);
                xMax = Mathf.FloorToInt((translate + scale) * image.width);
            }
            else
            {
                // We need to crop along the Y axis
                var scale = (outputResolution.y * (float)image.width / outputResolution.x) / image.height;
                var translate = (1.0f - scale) * 0.5f;
                yMin = Mathf.FloorToInt(translate * image.height);
                yMax = Mathf.FloorToInt((translate + scale) * image.height);
            }

            // Configure conversion
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin),
                outputDimensions = outputResolution,
                outputFormat = outputFormat,
                transformation = XRCpuImage.Transformation.None,
            };


#if NIANTIC_LIGHTSHIP_SPACES_ENABLED
            // Spaces by default has it upside down, which in this context requires a MirrorX to correct.
            conversionParams.transformation = XRCpuImage.Transformation.MirrorX;
            image.Convert(conversionParams, destinationPointer, image.GetConvertedDataSize(conversionParams));
            //debug display - TODO: remove when Spaces verified to work with all features
            SpacesCameraImageHack.ImageUtilsConvertCameraImage_Added(destinationPointer);
#else
            // Apply conversion
            image.Convert(conversionParams, destinationPointer, image.GetConvertedDataSize(conversionParams));
#endif
        }

        /// Write the conversion of the input texture into the output texture.
        /// The conversion includes scaling using the specified filter mode, cropping and rotating.
        ///
        /// Note: For our computer vision algorithms in ardk we always expect landscape right as orientation.
        public static void ConvertOnGpuAndCopy
        (
            this Texture2D image,
            Texture2D output,
            ScreenOrientation outputOrientation = ScreenOrientation.LandscapeLeft,
            FilterMode filterMode = FilterMode.Point,
            bool mirrorX = false
        )
        {
            if (s_material == null)
            {
                var lightshipImageConversionShader = Shader.Find("Unlit/LightshipImageConversion");
                if (lightshipImageConversionShader == null)
                {
                    Log.Error("Could not locate Unlit/LightshipImageConversion shader");
                    return;
                }
                s_material = new Material(lightshipImageConversionShader);
            }

            var inputResolution = new Vector2Int(image.width, image.height);

            var ogFilterMode = image.filterMode;
            image.filterMode = filterMode;

            var displayTransform = CameraMath.CalculateDisplayMatrix
            (
                inputResolution.x,
                inputResolution.y,
                output.width,
                output.height,
                outputOrientation,
                mirrorX,
                layout: CameraMath.MatrixLayout.RowMajor
            );

            s_material.SetMatrix(s_unityDisplayTransform, displayTransform);

            image.ReadFromExternalTexture(output, s_material);
            image.filterMode = ogFilterMode;
        }

        // Resizes intrinsics doing first the crop, then the scale.
        // Writes out the result into the destination buffer,
        // flattening the 3x3 matrix into a column-major array
        //
        // (F is focal length, C is principal point)
        //
        // | Fx  0  Cx |
        // | 0  Fy  Cy |
        // | 0  0   1  |
        public static void ConvertCameraIntrinsics
        (
            XRCameraIntrinsics inputIntrinsics,
            Vector2Int outputResolution,
            NativeArray<float> destinationBuffer
        )
        {
            float widthRatio;
            float heightRatio;

            int inputWidth = inputIntrinsics.resolution.x;
            int inputHeight = inputIntrinsics.resolution.y;

            var outFocalLength = inputIntrinsics.focalLength;
            var outPrincipalPoint = inputIntrinsics.principalPoint;

            // Decide if we are going to crop from top/bottom or left/right
            if (inputIntrinsics.resolution.y >= CalculateHeight(inputWidth, outputResolution))
            {
                int newHeight = CalculateHeight(inputWidth, outputResolution);
                int offset = (inputHeight - newHeight) / 2;
                outPrincipalPoint.y -= offset;

                widthRatio = outputResolution.x / (float)inputWidth;
                heightRatio = outputResolution.y / (float)newHeight;
            }
            else
            {
                int newWidth = CalculateWidth(inputHeight, outputResolution);
                int offset = (inputWidth - newWidth) / 2;
                outPrincipalPoint.x -= offset;

                widthRatio = outputResolution.x / (float)newWidth;
                heightRatio = outputResolution.y / (float)inputHeight;
            }

            outFocalLength.x *= widthRatio;
            outPrincipalPoint.x *= widthRatio;
            outFocalLength.y *= heightRatio;
            outPrincipalPoint.y *= heightRatio;

            destinationBuffer[0] = outFocalLength.x;
            destinationBuffer[4] = outFocalLength.y;
            destinationBuffer[6] = outPrincipalPoint.x;
            destinationBuffer[7] = outPrincipalPoint.y;
            destinationBuffer[8] = 1;
        }

        private static int CalculateHeight(int width, Vector2Int aspectRatio)
        {
            int height = (aspectRatio.y * width) / aspectRatio.x;
            height -= height & 1; // Nearest smaller multiple of two
            return height;
        }

        private static int CalculateWidth(int height, Vector2Int aspectRatio)
        {
            int width = (aspectRatio.x * height) / aspectRatio.y;
            width -= width & 1; // Nearest smaller multiple of two
            return width;
        }
    }
}
