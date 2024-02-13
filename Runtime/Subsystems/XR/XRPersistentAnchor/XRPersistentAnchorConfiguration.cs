// Copyright 2022-2024 Niantic.

using System;
using System.Runtime.InteropServices;

using System.Text;
using Niantic.Lightship.AR.Utilities.Logging;
using UnityEngine;

namespace Niantic.Lightship.AR.XRSubsystems
{
    /// <summary>
    /// Configuration for PersistentAnchorSubsystem.
    /// </summary>
    /// <seealso cref="XRPersistentAnchorConfiguration"/>
    [StructLayout(LayoutKind.Sequential)]
    public class XRPersistentAnchorConfiguration
    {
        // Note: Current default values match AnchorModuleConfiguration defaults
        internal const bool DefaultContinuousLocalizationEnabled = false;
        internal const bool DefaultTemporalFusionEnabled = false;
        internal const bool DefaultSlickLocalizationEnabled = false;
        internal const float DefaultCloudLocalizerMaxRequestsPerSecond = 1;
        internal const float DefaultSlickLocalizationFps = 0; // Full Frame-rate
        internal const uint DefaultCloudLocalizationTemporalFusionWindowSize = 5;
        internal const uint DefaultSlickLocalizationTemporalFusionWindowSize = 100;
        internal const bool DefaultDiagnosticsEnabled = false;

        private bool _continuousLocalizationEnabled;
        private bool _temporalFusionEnabled;
        private bool _slickLocalizationEnabled;
        private float _cloudLocalizerMaxRequestsPerSecond;
        private float _slickLocalizationFps;
        private uint _cloudLocalizationTemporalFusionWindowSize;
        private uint _slickLocalizationTemporalFusionWindowSize;
        private bool _diagnosticsEnabled;

        public bool ContinuousLocalizationEnabled
        {
            get => _continuousLocalizationEnabled;
            set => _continuousLocalizationEnabled = value;
        }

        public bool TemporalFusionEnabled
        {
            get => _temporalFusionEnabled;
            set => _temporalFusionEnabled = value;
        }

        public bool SlickLocalizationEnabled
        {
            get => _slickLocalizationEnabled;
            set => _slickLocalizationEnabled = value;
        }

        public float CloudLocalizerMaxRequestsPerSecond
        {
            get => _cloudLocalizerMaxRequestsPerSecond;
            set => _cloudLocalizerMaxRequestsPerSecond = value;
        }

        public float SlickLocalizationFps
        {
            get => _slickLocalizationFps;
            set => _slickLocalizationFps = value;
        }

        public uint CloudLocalizationTemporalFusionWindowSize
        {
            get => _cloudLocalizationTemporalFusionWindowSize;
            set => _cloudLocalizationTemporalFusionWindowSize = value;
        }

        public uint SlickLocalizationTemporalFusionWindowSize
        {
            get => _slickLocalizationTemporalFusionWindowSize;
            set => _slickLocalizationTemporalFusionWindowSize = value;
        }

        public bool DiagnosticsEnabled
        {
            get => _diagnosticsEnabled;
            set => _diagnosticsEnabled = value;
        }

        /// <summary>
        /// Default constructor for the XRPersistentAnchorConfiguration.
        /// </summary>
        public XRPersistentAnchorConfiguration()
        {
            _continuousLocalizationEnabled = DefaultContinuousLocalizationEnabled;
            _temporalFusionEnabled = DefaultTemporalFusionEnabled;
            _slickLocalizationEnabled = DefaultSlickLocalizationEnabled;
            _cloudLocalizerMaxRequestsPerSecond = DefaultCloudLocalizerMaxRequestsPerSecond;
            _slickLocalizationFps = DefaultSlickLocalizationFps;
            _cloudLocalizationTemporalFusionWindowSize = DefaultCloudLocalizationTemporalFusionWindowSize;
            _slickLocalizationTemporalFusionWindowSize = DefaultSlickLocalizationTemporalFusionWindowSize;
            _diagnosticsEnabled = DefaultDiagnosticsEnabled;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRPersistentAnchorConfiguration"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRPersistentAnchor"/>, otherwise false.</returns>
        private bool Equals(XRPersistentAnchorConfiguration other)
        {
            return  _continuousLocalizationEnabled == other._continuousLocalizationEnabled &&
                    _temporalFusionEnabled == other._temporalFusionEnabled &&
                    _slickLocalizationEnabled == other._slickLocalizationEnabled &&
                    _cloudLocalizerMaxRequestsPerSecond == other._cloudLocalizerMaxRequestsPerSecond &&
                    _slickLocalizationFps == other._slickLocalizationFps &&
                    _cloudLocalizationTemporalFusionWindowSize == other._cloudLocalizationTemporalFusionWindowSize &&
                    _slickLocalizationTemporalFusionWindowSize == other._slickLocalizationTemporalFusionWindowSize &&
                    _diagnosticsEnabled == other._diagnosticsEnabled;
        }
    }
}
