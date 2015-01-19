using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using GoblinXNA;
using GoblinXNA.SceneGraph;
using GoblinXNA.Device.Capture;
using GoblinXNA.Device.Vision;
using GoblinXNA.Device.Vision.Marker;

namespace ARUtils
{
    public class ARManager
    {
        public static MarkerNode markerNode;
        public static MarkerNode myNode;
        public static void SetupMarkerTracking(Scene scene, string file, int inputDevice) 
        {
            DirectShowCapture captureDevice = new DirectShowCapture();
            captureDevice.InitVideoCapture(inputDevice, FrameRate._30Hz, Resolution._640x480,
                ImageFormat.R8G8B8_24, false);

            scene.AddVideoCaptureDevice(captureDevice);

            IMarkerTracker tracker;
            // Create an optical marker tracker that uses ALVAR library
            tracker = new ALVARMarkerTracker();
            ((ALVARMarkerTracker)tracker).MaxMarkerError = 0.02f;
            tracker.InitTracker(captureDevice.Width, captureDevice.Height, file, 9.0);

            // Set the marker tracker to use for our scene
            scene.MarkerTracker = tracker;

            scene.ShowCameraImage = true;
        }

        public static MarkerNode SetupMarkers(Scene scene, int playerId)
        {
            //en el archivo file esta nuestra definicion de marcadores.
            //se deben escribir los archivos correspondientes en formato goblin
            //luego para cada uno se hace:

            int id = 20 + playerId;
            if (id > 27) id++;
            int[] ids = new int[20];

            for (int i = 0; i < ids.Length; i++)
            {
                if (i < 10)
                    ids[i] = i;
                else ids[i] = i + 1;
            }

            if (ARManager.myNode == null)
                ARManager.myNode = new MarkerNode(scene.MarkerTracker, "Toolbars/Astr" + playerId + ".txt", new int[1] { id });

            if (ARManager.markerNode == null)
                ARManager.markerNode = new MarkerNode(scene.MarkerTracker, "Toolbars/ground.txt", ids);
            
            //scene.RootNode.AddChild(markerNode);           
            return ARManager.markerNode;

        }
    }
}
