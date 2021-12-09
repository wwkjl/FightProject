// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraWork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the Camera work to follow the player
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;


	/// <summary>
	/// Camera work. Follow a target
	/// </summary>
	public class CameraWork : MonoBehaviour
	{
        #region Private Fields

	    [Tooltip("The distance in the local x-z plane to the target")]
	    [SerializeField]
	    private float distance = 8.0f;
	    
	    [Tooltip("The height we want the camera to be above the target")]
	    [SerializeField]
	    private float height = 14.0f;

		[SerializeField]
		private float damp = 1.0f;


        // cached transform of the target
		[SerializeField]
        private Transform cameraTransform;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		
		// Cache for camera offset
		Vector3 cameraOffset = Vector3.zero;

		public GameObject target;


	#endregion

	#region MonoBehaviour Callbacks

	/// <summary>
	/// MonoBehaviour method called on GameObject by Unity during initialization phase
	/// </summary>
	void OnEnable()
		{
			target = gameObject;
		}


		void LateUpdate()
		{
			if(target)
				Cut();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Raises the start following event. 
		/// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
		/// </summary>
		
		#endregion

		#region Private Methods

		void Cut()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;
			cameraOffset.x = -damp;

			//cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);
			cameraTransform.position = target.transform.position + cameraOffset;

			cameraTransform.LookAt(target.transform.position);
		}
		#endregion
	}