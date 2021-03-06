using UnityEngine;
using System.Collections;

public class BillboardLaub: MonoBehaviour {

	private Transform mainCamTransform;
	private Transform cachedTransform;
	private const int skip = 2;
	private int frame = 0;
	
	// Use this for initialization
	void Start () {
		mainCamTransform = Camera.mainCamera.transform;
		cachedTransform = transform;
	}
	
	void Update()
	{
		if(frame++ < skip ){ return; }
		frame = 0;
		
		if (mainCamTransform.InverseTransformPoint( cachedTransform.position).z>=0){
			Vector3 v = mainCamTransform.position - cachedTransform.position;
			v.x=v.z=0;
			cachedTransform.LookAt( mainCamTransform.position-v);
			renderer.enabled = true;
		}
		else{
			renderer.enabled = false;	
		}

	}

}
