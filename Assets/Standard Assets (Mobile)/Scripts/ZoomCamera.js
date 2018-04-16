//////////////////////////////////////////////////////////////
// ZoomCamera.js
// Penelope iPhone Tutorial
//
// ZoomCamera is a simple camera that uses a zoom value to zoom 
// the camera in or out relatively from the default position set
// in the editor. It can snap to zoom values when moving closer
// to the specified origin and smoothly seeks when moving farther
// away. The camera checks for any objects that obstruct the view
// of the camera to the origin and snaps to be in front of those
// locations.
//////////////////////////////////////////////////////////////

#pragma strict

var origin : Transform; // What is considered the origin to the camera
var zoom : float;
var zoomMin : float = -5;
var zoomMax : float = 5;
var seekTime = 1.0;
var smoothZoomIn = false;
private var defaultLocalPosition : Vector3;
private var thisTransform : Transform;
private var currentZoom : float;
private var targetZoom : float;
private var zoomVelocity : float;

function Start()
{
	// Cache component instead of looking it up every frame
	thisTransform = transform;
	
	// The default position is the position that is set in the editor
	defaultLocalPosition = thisTransform.localPosition;
	
	// Default the current zoom to what was set in the editor 
	currentZoom = zoom;
}

function Update() 
{
	// The zoom set externally must still be within the min-max range
	zoom = Mathf.Clamp( zoom, zoomMin, zoomMax );
	
	// Only collide with non-Player (8) layers
	var layerMask = ~((1 << 8) | (1 << 2));

	var hit : RaycastHit;
	var start = origin.position;
	var zoomedPosition = defaultLocalPosition + thisTransform.parent.InverseTransformDirection( thisTransform.forward * zoom );
	var end = thisTransform.parent.TransformPoint( zoomedPosition );
	
	// Cast a line from the origin transform to the camera and find out if we hit anything in-between
	if ( Physics.Linecast( start, end, hit, layerMask ) ) 
	{
		// We hit something, so translate this to a zoom value
		var position = hit.point + thisTransform.TransformDirection( Vector3.forward );
		var difference = position - thisTransform.parent.TransformPoint( defaultLocalPosition );
		targetZoom = difference.magnitude;
	}
	else
		// We didn't hit anything, so the camera should use the zoom set externally
		targetZoom = zoom;
	
	// Clamp target zoom to our min-max range
	targetZoom = Mathf.Clamp( targetZoom, zoomMin, zoomMax );
	
	if ( !smoothZoomIn && ( targetZoom - currentZoom ) > 0 )
	{
		// Snap the current zoom to our target if it is closer. This is useful if
		// some object is between the camera and the origin
		currentZoom = targetZoom;
	}
	else
	{
		// Smoothly seek towards our target zoom value
		currentZoom = Mathf.SmoothDamp( currentZoom, targetZoom, zoomVelocity, seekTime );
	}
	
	// Set the position of the camera
	zoomedPosition = defaultLocalPosition + thisTransform.parent.InverseTransformDirection( thisTransform.forward * currentZoom );
	thisTransform.localPosition = zoomedPosition;
}