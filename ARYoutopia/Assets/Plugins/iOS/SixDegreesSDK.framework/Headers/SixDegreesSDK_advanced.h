/*!
 @header SixDegreesSDK_advanced.h
 @copyright Copyright (C) 2018 6degrees.xyz Inc.
 @abstract This file is part of the 6D.ai Beta SDK and is not licensed
 for commercial use.
 
 The 6D.ai Beta SDK can not be copied and/or distributed without
 the express permission of 6degrees.xyz Inc.
 
 Contact developers\@6d.ai for licensing requests.
 @version beta 0.20.0
 @ignore API_ADVANCED_METHOD
 */

#pragma once

#define API_ADVANCED_METHOD FOUNDATION_EXPORT __attribute__((visibility ("default")))

#import <ARKit/ARKit.h>

/*!
 @brief Initializes ARKit and the SDK's internal states using the Metal pipeline. Call this first!
 @discussion Most other API calls in this SDK will not work until you call this. Call only once!
 
 This API call returns early, use @link SixDegreesSDK_IsInitialized() @/link to monitor initialization progress.
 
 Initialization will fail if the current device is not supported.
 Use @link SixDegreesSDK_IsDeviceSupported() @/link to check that the device is supported.
 
 This API call is made available for expert users, others should use @link SixDegreesSDK_Initialize() @/link.
 @param configuration optional pointer to the ARWorldTrackingConfiguration object for ARKit.
 The following configuration options are required:
  - video format must be a multiple of 16:9
  - autofocus must be disabled
  - world alignment must be ARWorldAlignmentGravity
 6D will use a default configuration if the parameter is null.
 @return true if the SDK is initializing, false if there is a problem (check the logs).
 */
API_ADVANCED_METHOD bool SixDegreesSDK_InitializeWithConfig(ARWorldTrackingConfiguration* configuration);

/*!
 @brief Returns the ARKit Session object managed by the SDK. Optional ARKit features enabled by @link SixDegreesSDK_InitializeWithConfig() @/link are accessible through this object.
 @discussion the ARSession pointer gives access, through the current ARFrame, to ARAnchor objects like planes
 and images, optionally enabled with @link SixDegreesSDK_InitializeWithConfig() @/link.
 
 Users should not change the delegate object or start and stop the session.
 
 This API call is made available for expert users.
 @return the ARSession pointer of the object managed by the SDK, null if none available.
 */
API_ADVANCED_METHOD ARSession* SixDegreesSDK_GetARKitSession(void);

/*!
 @brief Populates poseDataOut with a column-major 4x4 matrix describing the transform from ARKit to world coordinates.
 @discussion Like ARKit, world coordinates assume Y is up (aligned with gravity), X is right and Z towards the user,
 relatively to the orientation of the device the first time the map was created.
 
 Use the output of this method to transform native ARKit transforms and coordinates (for instance, ARAnchor objects)
 to 6D.ai world coordinates.
 
 This API call is made available for expert users.
 @param transformDataOut pointer to the first element a float array of size 16.
 @param bufferSize must be 16.
 */
API_ADVANCED_METHOD void SixDegreesSDK_GetARKitTransform(float* transformDataOut, int bufferSize);
