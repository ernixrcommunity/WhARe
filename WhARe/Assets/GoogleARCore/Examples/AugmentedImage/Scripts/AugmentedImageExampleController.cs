//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : MonoBehaviour
    {
        //public GameObject ARCoreDevice;
        public ARCoreSessionConfig sessionConfig;
        public AugmentedImageDatabase imageDatabase;
        public Button resetButton;

        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        public List<AugmentedImageVisualizer> prefabs = new List<AugmentedImageVisualizer>();

        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

        public Text lblDrag;
        public Text lblPointerDown;
        public Text lblPointerUp;
        public Text imageCount;

        private List<Anchor> m_anchors = new List<Anchor>();

        public void Start() {
            //sessionConfig = ARCoreDevice.GetComponent<ARCoreSessionConfig>();
            sessionConfig.AugmentedImageDatabase = imageDatabase;

        }


        /// <summary>
        /// The Unity Update method.
        /// </summary>
        public void Update()
        {

            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);
            
            imageCount.text = "Images: " + m_TempAugmentedImages.Count;
            // Create visualizers and anchors for updated augmented images that are tracking and do not previously
            // have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_TempAugmentedImages)
            {
                AugmentedImageVisualizer visualizer = null;
                m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    m_anchors.Add(anchor);
                    Debug.Log(">>> Created new anchor");
                    visualizer = (AugmentedImageVisualizer)Instantiate(prefabs[image.DatabaseIndex], anchor.transform);
                    visualizer.gameObject.SetActive(true);
                    visualizer.Image = image;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);
                }
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    m_Visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
                }
            }

            //Show the fit - to - scan overlay if there are no images that are Tracking.
            foreach (var visualizer in m_Visualizers.Values)
            {
                if (visualizer.Image.TrackingState == TrackingState.Tracking)
                {
                    FitToScanOverlay.SetActive(false);
                    resetButton.gameObject.SetActive(true);
                    return;
                }
            }

            FitToScanOverlay.SetActive(true);
            resetButton.gameObject.SetActive(false);
        }


        public void ResetButtonClicked()
        {
            FitToScanOverlay.SetActive(true);
            resetButton.gameObject.SetActive(false);
            //foreach (var vizKVP in m_Visualizers)
            //{
            //    GameObject.Destroy(vizKVP.Value.gameObject);
            //}

            //foreach (var img in m_TempAugmentedImages)
            //{
            //    var imgAnchors = img.GetAllAnchors();
            //    foreach (var imgAnchor in imgAnchors)
            //    {

            //    }
            //}


            //foreach (var anchor in m_anchors)
            //{

            //    anchor.detach();
            //    //GameObject.Destroy(anchor.gameObject);
            //}


            //m_Visualizers.Clear();
            //m_TempAugmentedImages.Clear();
            //sessionConfig.AugmentedImageDatabase = null;
            //sessionConfig.AugmentedImageDatabase = imageDatabase;


            var session = GameObject.Find("ARCore Device").GetComponent<ARCoreSession>();
            
            DestroyImmediate(session);
            

            foreach (var anch in m_anchors)
            {
                Destroy(anch);
            }
            
            var device = GameObject.Find("ARCore Device");
            session = device.GetComponent<ARCoreSession>();

            if (session == null)
            {
                session = device.AddComponent<ARCoreSession>();
                session.SessionConfig = sessionConfig;
                session.enabled = true;
            }



            Debug.Log("Pointer down");
            lblPointerDown.gameObject.SetActive(true);

            lblPointerUp.gameObject.SetActive(false);
            lblDrag.gameObject.SetActive(false);
        }

        public void ResetButtonClicked2()
        {
            Debug.Log("Pointer up");
            lblPointerUp.gameObject.SetActive(true);

            lblPointerDown.gameObject.SetActive(false);
            lblDrag.gameObject.SetActive(false);
        }

        public void ResetButtonDragged()
        {
            Debug.Log("dragged");
            lblDrag.gameObject.SetActive(true);

            lblPointerDown.gameObject.SetActive(false);
            lblPointerUp.gameObject.SetActive(false);
        }
    }
}
