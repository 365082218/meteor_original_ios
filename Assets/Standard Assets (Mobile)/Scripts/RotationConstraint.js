//////////////////////////////////////////////////////////////
// RotationConstraint.js
// Penelope iPhone Tutorial
//
// RotationConstraint constrains the relative rotation of a 
// Transform. You select the constraint axis in the editor and 
// specify a min and max amount of rotation that is allowed 
// from the default rotation
//////////////////////////////////////////////////////////////

enum ConstraintAxis
{
	X = 0,
	Y,
	Z
}

public var axis : ConstraintAxis; 			// Rotation around this axis is constrained
public var min : float;						// Relative value in degrees
public var max : float;						// Relative value in degrees
private var thisTransform : Transform;
private var rotateAround : Vector3;
private var minQuaternion : Quaternion;
private var maxQuaternion : Quaternion;
private var range : float;

function Start()
{
	thisTransform = transform;
	
	// Set the axis that we will rotate around
	switch ( axis )
	{
		case ConstraintAxis.X:
			rotateAround = Vector3.right;
			break;
			
		case ConstraintAxis.Y:
			rotateAround = Vector3.up;
			break;
			
		case ConstraintAxis.Z:
			rotateAround = Vector3.forward;
			break;
	}
	
	// Set the min and max rotations in quaternion space
	var axisRotation = Quaternion.AngleAxis( thisTransform.localRotation.eulerAngles[ axis ], rotateAround );
	minQuaternion = axisRotation * Quaternion.AngleAxis( min, rotateAround );
	maxQuaternion = axisRotation * Quaternion.AngleAxis( max, rotateAround );
	range = max - min;
}

// We use LateUpdate to grab the rotation from the Transform after all Updates from
// other scripts have occured
function LateUpdate() 
{
	// We use quaternions here, so we don't have to adjust for euler angle range [ 0, 360 ]
	var localRotation = thisTransform.localRotation;
	var axisRotation = Quaternion.AngleAxis( localRotation.eulerAngles[ axis ], rotateAround );
	var angleFromMin = Quaternion.Angle( axisRotation, minQuaternion );
	var angleFromMax = Quaternion.Angle( axisRotation, maxQuaternion );
		
	if ( angleFromMin <= range && angleFromMax <= range )
		return; // within range
	else
	{		
		// Let's keep the current rotations around other axes and only
		// correct the axis that has fallen out of range.
		var euler = localRotation.eulerAngles;			
		if ( angleFromMin > angleFromMax )
			euler[ axis ] = maxQuaternion.eulerAngles[ axis ];
		else
			euler[ axis ] = minQuaternion.eulerAngles[ axis ];

		thisTransform.localEulerAngles = euler;		
	}
}
