using SlimDX;
using System.Collections.Generic;

namespace ModelEx
{
	public class CameraSet
	{
		public enum CameraMode : int
		{
			Ego = 0,
			Orbit = 1,
			OrbitPan = 2
		}

		Scene _ownerScene;
		int _cameraIndex;
		List<DynamicCamera> _cameras = new List<DynamicCamera>();

		public int CameraIndex
		{
			get
			{
				return _cameraIndex;
			}
			set
			{
				_cameraIndex = value;
				CurrentCamera = _cameras[_cameraIndex];
			}
		}

		public DynamicCamera CurrentCamera { get; private set; }

		public CameraSet(Scene ownerScene)
		{
			_ownerScene = ownerScene;

			EgoCamera ec = new EgoCamera();
			OrbitCamera oc = new OrbitCamera();
			OrbitPanCamera ocp = new OrbitPanCamera();
			_cameras.Add(ec);
			_cameras.Add(oc);
			_cameras.Add(ocp);

			_cameraIndex = 0;
			CurrentCamera = _cameras[_cameraIndex];
		}

		public void ResetPositions()
		{
			Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 target = new Vector3(0.0f, 0.0f, 1.0f);

			if (_ownerScene != null)
			{
				BoundingSphere boundingSphere = _ownerScene.GetBoundingSphere();
				if (boundingSphere != null)
				{
					target = boundingSphere.Center;
					eye = target - new Vector3(0.0f, 0.0f, boundingSphere.Radius * 2.5f);
				}
			}

			// Only reset the predefined ones!
			_cameras[(int)CameraMode.Ego].SetView(eye, target, new Vector3(0, 1, 0));
			_cameras[(int)CameraMode.Orbit].SetView(eye, target, new Vector3(0, 1, 0));
			_cameras[(int)CameraMode.OrbitPan].SetView(eye, target, new Vector3(0, 1, 0));
		}
	}
}
