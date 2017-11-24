using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AtackPointController : MonoBehaviour {

    public AtackPoint[] atackPoints;

	// Use this for initialization
	void Start () {
        atackPoints = GetComponentsInChildren<AtackPoint>()
            .OrderBy(ap => ap.name).ToArray();
        for(var i = 0; i < 6; i++)
            for(var j = 0; j < 3; j++)
            {
                var idx = i * 3 + j;
                atackPoints[idx].jointType = KinectController.KeyJoints[j];
            }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
