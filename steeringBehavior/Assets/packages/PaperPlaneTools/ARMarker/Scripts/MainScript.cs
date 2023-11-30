namespace PaperPlaneTools.AR {
	using OpenCvSharp;

	using UnityEngine;
	using System.Collections;
	using System.Runtime.InteropServices;
	using System;
	using System.Collections.Generic;
	using UnityEngine.UI;
	
    /// <summary>
    /// MainScript is a subclass of WebCamera!
    /// </summary>
	public class MainScript: WebCamera {

    
		[Serializable]
		public class MarkerObject
		{
			public int markerId;
			public GameObject markerPrefab;
		}

		public class MarkerOnScene
		{
			public int bestMatchIndex = -1;
			public float destroyAt = -1f;
			public GameObject gameObject = null;
		}

        public class BallOnScene
        {
            public int size;
            public int xCoord;
            public int yCoord;
            public Point[] theContour;
            // destroyAt should represent a moment in the future where the ball will become invisible
            public float destroyAt = -1f;   
        }

		/// <summary>
		/// List of possible markers
		/// The list is set in Unity Inspector
		/// </summary>
		public List<MarkerObject> markers;

        public int thresholdBall = 50;
        public int ballPoints = 15;
        // display size of current ball found
        public int curBalSize;  // to see how many pixels we need

        [Header("Ball size")]
        public int minBallSize = 10;   // in pixels
        public int maxBallSize = 10;   // in pixels

        [Header("Color bounds")]
        public int lowerHue = 0;
        public int lowerSaturation = 50;
        public int lowerValue = 50;
        public int upperHue = 10;
        public int upperSaturation = 255;
        public int upperValue = 255;

        [Header("parameters for coordinate conversion")]
        public float a;
        public float b;
        public float c;
        public float d;
        [Header("coordinates in (OpenCV) pixel coordinates")]
        public Point2f origLeftUpperCorner;
        public Point2f origRightLowerCorner;
        [Header("coordinates in marker coordinates")]
        public Point2f leftUpperCorner;
        public Point2f rightLowerCorner;

        public bool recordOrigLeftUp = false;
        public bool recordOrigRightDown = false;

        /// <summary>
        /// The marker detector
        /// </summary>
        private MarkerDetector markerDetector;

        private BallOnScene currentBall;

		/// <summary>
		/// Objects on scene (game objects with their associated markers)
		/// </summary>
		private Dictionary<int, List<MarkerOnScene>> gameObjects = new Dictionary<int, List<MarkerOnScene>>();

		void Start () {
			markerDetector = new MarkerDetector ();

			foreach (MarkerObject markerObject in markers) {
				gameObjects.Add(markerObject.markerId, new List<MarkerOnScene>());
			}
            Debug.Log("Marker list = " + markers.Count.ToString());

		}


		protected override void Awake() {
			int cameraIndex = -1;
            Debug.Log("Nr of devices = " + WebCamTexture.devices.Length.ToString());

            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                WebCamDevice webCamDevice = WebCamTexture.devices[i];
                if (webCamDevice.isFrontFacing == false)
                {
                    cameraIndex = i;
                    break;
                }
                if (cameraIndex < 0)
                {
                    cameraIndex = i;
                }
            }

            Debug.Log("Camera index = " + cameraIndex.ToString());

			if (cameraIndex >= 0) {
				DeviceName = WebCamTexture.devices [cameraIndex].name;
                Debug.Log("Camera name = " + DeviceName);
                // Gidi: next line was commented out?
                webCamDevice = WebCamTexture.devices [cameraIndex];
            }
		}

        protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output) {
			Mat img = Unity.TextureToMat (input, TextureParameters);

            img = img.Flip(FlipMode.Y);     // Gidi: otherwise the image is mirrored? Why is this standard?

            Debug.Log("Img width = " + img.Cols.ToString() + " Rows = " + img.Rows.ToString());

            // try to find the markers
			ProcessFrame(img, img.Cols, img.Rows);
            // Gidi: N.B. img may be transformed by this ProcessFrame!

            // Gidi: now we want to see the ball by contour detection!
            //ProcessContour(img);

            // Gidi: find the ball by color and contour
            findRightColor(img);

            output = Unity.MatToTexture(img, output);
   
			return true;
		}

        /// <summary>
        /// Find contours
        /// 
        /// Gidi: I based this on the ContoursByShapeScript.cs file
        /// </summary>
        /// <param name="mat"></param>
        private void ProcessContour(Mat mat)
        {
            Mat grayMat = new Mat();
            Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);

            Mat thresh = new Mat();
            Cv2.Threshold(grayMat, thresh, thresholdBall, 255, ThresholdTypes.BinaryInv); // thresh = 127

            // Extract Contours
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(thresh, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone, null);

            foreach (Point[] contour in contours)
            {
                double length = Cv2.ArcLength(contour, true);
                Point[] approx = Cv2.ApproxPolyDP(contour, length * 0.01, true);
                string shapeName = null;
                Scalar color = new Scalar();

                // Gidi: we only want to see circles, which are elements with many contour points
                if (approx.Length >= ballPoints)    // 15
                {
                    // try to see if the ball is the right size
                    OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                    int size = rect.Width * rect.Height;
                    if ((size <= maxBallSize) && (size >= minBallSize))
                    {
                        shapeName = "Ball";
                        color = new Scalar(0, 0, 255);
                    }
                }

                if (shapeName != null)
                {
                    Moments m = Cv2.Moments(contour);
                    int cx = (int)(m.M10 / m.M00);
                    int cy = (int)(m.M01 / m.M00);

                    Cv2.DrawContours(mat, new Point[][] { contour }, 0, color, -1);
                    Cv2.PutText(mat, shapeName, new Point(cx - 50, cy), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
                }
            }
        }

        private void findRightColor(Mat mat)
        {
            Mat hsvMat = new Mat();
            Cv2.CvtColor(mat, hsvMat, ColorConversionCodes.BGR2HSV);

            Scalar lowerBound = new Scalar (lowerHue, lowerSaturation, lowerValue);
            Scalar upperBound = new Scalar (upperHue, upperSaturation, upperValue);

            // Threshold the image to only select pixels within the defined color range
            Mat imgThresholded = new Mat();
            Cv2.InRange(hsvMat, lowerBound, upperBound, imgThresholded);

            // Perform morphological operations to remove noise
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(imgThresholded, imgThresholded, MorphTypes.Open, kernel);
            Cv2.MorphologyEx(imgThresholded, imgThresholded, MorphTypes.Close, kernel);

            // find contours in the thresholded image
            Point[][] contours;
            HierarchyIndex[] hierarchy;

            Cv2.FindContours(imgThresholded, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);

            Scalar color = new Scalar();
//          color = new Scalar(0, 0, 255);  // if we comment this out, the ball is black when no ball is found (for 1 sec)
            if (currentBall != null)
            {
                currentBall.size = 0;   // initialize to zero at the start of this detection round
                curBalSize = currentBall.size;
            }
            foreach (Point[] contour in contours)
            {
                double length = Cv2.ArcLength(contour, true);
                Point[] approx = Cv2.ApproxPolyDP(contour, length * 0.01, true);
                string shapeName = null;

                // Gidi: we only want to see circles, which are elements with many contour points
                if (approx.Length >= ballPoints)    // 15
                {
                    // try to see if the ball is the right size
                    OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                    int size = rect.Width * rect.Height;

                    //if ((size <= maxBallSize) && (size >= minBallSize))
                    if (true)   // disregard size
                    {
                        shapeName = "Ball";
                        color = new Scalar(0, 0, 255);
                    }
                }

                if (shapeName != null)
                {
                    // we have found a possible ball
                    if (currentBall != null)
                    {
                        // compare the size of this shape with the current (largest) one found
                        OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                        int size = rect.Width * rect.Height;
                        if (size > currentBall.size)
                        {
                            // new shape is bigger, so replaces the current ball
                            currentBall.theContour = contour;
                            Moments m = Cv2.Moments(contour);
                            currentBall.xCoord = (int)(m.M10 / m.M00);  // Gidi: 1e moment in X-richting is zwaartepunt?
                            currentBall.yCoord = (int)(m.M01 / m.M00);  // Gidi: 1e moment in Y-richting
                            currentBall.size = size;
                            curBalSize = currentBall.size;
                            currentBall.destroyAt = Time.fixedTime + 1.0f; // ball will remain visible for 1 second after disappearing
                        } 
                    }
                    else
                    {
                        // create a currentBall
                        currentBall = new BallOnScene();
                        currentBall.theContour = contour;
                        Moments m = Cv2.Moments(contour);
                        currentBall.xCoord = (int)(m.M10 / m.M00);  // Gidi: 1e moment in X-richting is zwaartepunt?
                        currentBall.yCoord = (int)(m.M01 / m.M00);  // Gidi: 1e moment in Y-richting
                        OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                        currentBall.size = rect.Width * rect.Height;
                        curBalSize = currentBall.size;
                        currentBall.destroyAt = Time.fixedTime + 1.0f; // ball will remain visible for 1 second after disappearing
                    }
                }

                //if (shapeName != null) 
                //{
                //    // we have found a possible ball
                //    Moments m = Cv2.Moments(contour);
                //    int cx = (int)(m.M10 / m.M00);  // Gidi: 1e moment in X-richting is zwaartepunt?
                //    int cy = (int)(m.M01 / m.M00);  // Gidi: 1e moment in Y-richting

                //    // Gidi: apparently, this is the position in (OpenCV) pixel coordinates
                //    string pos = "(" + cx.ToString() + "," + cy.ToString() + ")";

                //    Cv2.DrawContours(mat, new Point[][] { contour }, 0, color, -1);
                //    //Cv2.PutText(mat, shapeName, new Point(cx - 50, cy), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
                //    Cv2.PutText(mat, pos, new Point(cx - 50, cy), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
                //}
            }
            if (currentBall != null) 
            { 
                if (currentBall.destroyAt < Time.fixedTime)
                {
                    // no longer visible, do nothing
                }
                else
                {
                    // ball is visible!
                    // Gidi: apparently, this is the position in (OpenCV) pixel coordinates
                    int myX = currentBall.xCoord;
                    int myY = currentBall.yCoord;
                    Point2f aPoint = new Point2f((float)myX, (float)myY);
                    if (recordOrigLeftUp)
                    {
                        // store
                        origLeftUpperCorner = aPoint;
                        recordOrigLeftUp = false;   // reset
                    }
                    if (recordOrigRightDown)
                    {
                        // store
                        origRightLowerCorner = aPoint;
                        recordOrigRightDown = false;   // reset
                    }


                    string pos = "(" + myX.ToString() + "," + currentBall.yCoord.ToString() + ")";

                    Cv2.DrawContours(mat, new Point[][] { currentBall.theContour }, 0, color, -1);
                    //Cv2.PutText(mat, shapeName, new Point(cx - 50, cy), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
                    Cv2.PutText(mat, pos, new Point(myX - 50, myY), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
                }
            }
        }

        private void ProcessFrame (Mat mat, int width, int height) {
			List<int> markerIds = markerDetector.Detect (mat, width, height);

			int count = 0;
			foreach (MarkerObject markerObject in markers) {
				List<int> markersFound = new List<int>();
				for (int i=0; i<markerIds.Count; i++) {
					if (markerIds[i] == markerObject.markerId) {
						markersFound.Add(i);
						count++;
					}
				}

				ProcessMarkesWithSameId(markerObject, gameObjects[markerObject.markerId], markersFound);
			}
		}

		private void ProcessMarkesWithSameId(MarkerObject markerObject, List<MarkerOnScene> gameObjects, List<int> markersFound) {
			int index = 0;
		
			index = gameObjects.Count - 1;
			while (index >= 0) {
				MarkerOnScene markerOnScene = gameObjects[index];
				markerOnScene.bestMatchIndex = -1;
				if (markerOnScene.destroyAt > 0 && markerOnScene.destroyAt < Time.fixedTime) {
					Destroy(markerOnScene.gameObject);
					gameObjects.RemoveAt(index);
				}
				--index;
			}

			index = markersFound.Count - 1;

			// Match markers with existing gameObjects
			while (index >= 0) {
				int markerIndex = markersFound[index];
				Matrix4x4 transforMatrix = markerDetector.TransfromMatrixForIndex(markerIndex);
				Vector3 position = MatrixHelper.GetPosition(transforMatrix);
                // Gidi: 
                // Debug.Log("Position ");
                //Debug.Log(position.ToString());

				float minDistance = float.MaxValue;
				int bestMatch = -1;
				for (int i=0; i<gameObjects.Count; i++) {
					MarkerOnScene markerOnScene = gameObjects [i];
					if (markerOnScene.bestMatchIndex >= 0) {
						continue;
					}
					float distance = Vector3.Distance(markerOnScene.gameObject.transform.position, position);
					if (distance<minDistance) {
						bestMatch = i;
					}
				}

				if (bestMatch >=0) {
					gameObjects[bestMatch].bestMatchIndex = markerIndex;
					markersFound.RemoveAt(index);
				} 
				--index;
			}

			//Destroy excessive objects
			index = gameObjects.Count - 1;
			while (index >= 0) {
				MarkerOnScene markerOnScene = gameObjects[index];
				if (markerOnScene.bestMatchIndex < 0) {
					if (markerOnScene.destroyAt < 0) {
						markerOnScene.destroyAt = Time.fixedTime + 0.2f;
					}
				} else {
					markerOnScene.destroyAt = -1f;
					int markerIndex = markerOnScene.bestMatchIndex;
					Matrix4x4 transforMatrix = markerDetector.TransfromMatrixForIndex(markerIndex);
					PositionObject(markerOnScene.gameObject, transforMatrix);
				}
				index--;
			}

			//Create objects for markers not matched with any game object
			foreach (int markerIndex in markersFound) {
				GameObject newGameObject = Instantiate(markerObject.markerPrefab);
				MarkerOnScene markerOnScene = new MarkerOnScene() {
					gameObject = newGameObject
				};
				gameObjects.Add(markerOnScene);

				Matrix4x4 transforMatrix = markerDetector.TransfromMatrixForIndex(markerIndex);
				PositionObject(markerOnScene.gameObject, transforMatrix);
			}
		}

        // Gidi: volgens mij neemt deze functie een 4x4 transformMatrix als input,
        // en zet een gegeven gameObject dan op deze positie 
		private void PositionObject(GameObject gameObject, Matrix4x4 transformMatrix) {
			Matrix4x4 matrixY = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
			Matrix4x4 matrixZ = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
            Matrix4x4 matrix = matrixY * transformMatrix * matrixZ;

			gameObject.transform.localPosition = MatrixHelper.GetPosition (matrix); // localposition is position relative to the parent transform
			gameObject.transform.localRotation = MatrixHelper.GetQuaternion (matrix);
			gameObject.transform.localScale = MatrixHelper.GetScale (matrix);
		}

        /// <summary>
        /// Compute new coordinates for the ball in a different coordinate system
        /// </summary>
        /// <param name="origCoord"></param>
        /// <returns></returns>
        public Point2f computeBallCoord( Point2f origCoord)
        {
            Point2f result = new Point2f();

            result.X = (a * origCoord.X) + b;
            result.Y = (c + origCoord.Y) + d;

            return result;
        }

        /// <summary>
        /// Based on two given point sets, compute parameters for coordinate conversion to
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        private void computeParameters(Point2f origLeftUp, Point2f origRightDown, Point2f newLeftUp, Point2f newRightDown)
        {
            float diffX = origRightDown.X - origLeftUp.X;
            float diffY = origRightDown.Y - origLeftUp.Y;

            if (Mathf.Approximately(diffX, 0.0f))
            {
                Debug.LogError("The original points are too close in the X direction");
            }
            else if (Mathf.Approximately(diffY, 0.0f))
            {
                Debug.LogError("The original points are too close in the Y direction");
            }
            else
            {
                // normal situation
                a = (newRightDown.X - newLeftUp.X) / diffX;
                b = newLeftUp.X - (a * origLeftUp.X);

                c = (newRightDown.Y - newLeftUp.Y) / diffY;
                d = newLeftUp.Y - (c * origLeftUp.Y);
            }
        }

        public Vector2 GetBallCoordinates()
        {
            Vector2 result = new Vector2();

            if (currentBall != null)
            {
                result.x = currentBall.xCoord;
                result.y = currentBall.yCoord;
            }
            return result;
        }
	}
}
