
#pragma strict

var plane : Transform;

function CalculateObliqueMatrix( projection : Matrix4x4 , clipPlane : Vector4 ) : Matrix4x4
{
	var q : Vector4 = projection.inverse * Vector4(
		Mathf.Sign(clipPlane.x),
		Mathf.Sign(clipPlane.y),
		1.0,
		1.0);
	var c : Vector4 = clipPlane * (2.0 / (Vector4.Dot(clipPlane, q)));
	
	projection[2] = c.x - projection[3];
	projection[6] = c.y - projection[7];
	projection[10] = c.z - projection[11];
	projection[14] = c.w - projection[15];
	
	return projection;
}

function OnPreCull()
{
	var projection : Matrix4x4 = GetComponent.<Camera>().projectionMatrix;
	
	var m : Matrix4x4 = GetComponent.<Camera>().worldToCameraMatrix;
	var planePos : Vector3 = m.MultiplyPoint(plane.position);
	var planeNormal : Vector3 = m.MultiplyVector(-Vector3.up);
	planeNormal.Normalize();
	var nearPlane : Vector4 = planeNormal;
	nearPlane.w = -Vector3.Dot(planeNormal, planePos);
	
	
	GetComponent.<Camera>().projectionMatrix = CalculateObliqueMatrix(projection, nearPlane);
}