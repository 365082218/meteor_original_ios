//////////////////////////////////////////////////////////////
// FirstPersonControl.js
//
// FirstPersonControl creates a control scheme where the camera 
// location and controls directly map to being in the first person.
// The left pad is used to move the character, and the
// right pad is used to rotate the character. A quick double-tap
// on the right joystick will make the character jump.
//
// If no right pad is assigned, then tilt is used for rotation
// you double tap the left pad to jump
//////////////////////////////////////////////////////////////

#pragma strict

@script RequireComponent( CharacterController )

// This script must be attached to a GameObject that has a CharacterController
var moveTouchPad : Joystick;
var rotateTouchPad : Joystick;						// If unassigned, tilt is used

var cameraPivot : Transform;						// The transform used for camera rotation

var forwardSpeed : float = 4;
var backwardSpeed : float = 1;
var sidestepSpeed : float = 1;
var jumpSpeed : float = 8;
var inAirMultiplier : float = 0.25;					// Limiter for ground speed while jumping
var rotationSpeed : Vector2 = Vector2( 50, 25 );	// Camera rotation speed for each axis
var tiltPositiveYAxis = 0.6;
var tiltNegativeYAxis = 0.4;
var tiltXAxisMinimum = 0.1;

private var thisTransform : Transform;
private var character : CharacterController;
private var cameraVelocity : Vector3;
private var velocity : Vector3;						// Used for continuing momentum while in air
private var canJump = true;

function Start()
{
	// Cache component lookup at startup instead of doing this every frame		
	thisTransform = GetComponent( Transform );
	character = GetComponent( CharacterController );	

	// Move the character to the correct start position in the level, if one exists
	var spawn = GameObject.Find( "PlayerSpawn" );
	if ( spawn )
		thisTransform.position = spawn.transform.position;
}

function OnEndGame()
{
	// Disable joystick when the game ends	
	moveTouchPad.Disable();
	
	if ( rotateTouchPad )
		rotateTouchPad.Disable();	

	// Don't allow any more control changes when the game ends
	this.enabled = false;
}

function Update()
{
	var movement = thisTransform.TransformDirection( Vector3( moveTouchPad.position.x, 0, moveTouchPad.position.y ) );

	// We only want horizontal movement
	movement.y = 0;
	movement.Normalize();

	// Apply movement from move joystick
	var absJoyPos = Vector2( Mathf.Abs( moveTouchPad.position.x ), Mathf.Abs( moveTouchPad.position.y ) );	
	if ( absJoyPos.y > absJoyPos.x )
	{
		if ( moveTouchPad.position.y > 0 )
			movement *= forwardSpeed * absJoyPos.y;
		else
			movement *= backwardSpeed * absJoyPos.y;
	}
	else
		movement *= sidestepSpeed * absJoyPos.x;		
	
	// Check for jump
	if ( character.isGrounded )
	{		
		var jump = false;
		var touchPad : Joystick;
		if ( rotateTouchPad )
			touchPad = rotateTouchPad;
		else
			touchPad = moveTouchPad;
	
		if ( !touchPad.IsFingerDown() )
			canJump = true;
		
	 	if ( canJump && touchPad.tapCount >= 2 )
	 	{
			jump = true;
			canJump = false;
	 	}	
		
		if ( jump )
		{
			// Apply the current movement to launch velocity		
			velocity = character.velocity;
			velocity.y = jumpSpeed;	
		}
	}
	else
	{			
		// Apply gravity to our velocity to diminish it over time
		velocity.y += Physics.gravity.y * Time.deltaTime;
				
		// Adjust additional movement while in-air
		movement.x *= inAirMultiplier;
		movement.z *= inAirMultiplier;
	}
		
	movement += velocity;	
	movement += Physics.gravity;
	movement *= Time.deltaTime;
	
	// Actually move the character	
	character.Move( movement );
	
	if ( character.isGrounded )
		// Remove any persistent velocity after landing	
		velocity = Vector3.zero;
	
	// Apply rotation from rotation joystick
	if ( character.isGrounded )
	{
		var camRotation = Vector2.zero;
		
		if ( rotateTouchPad )
			camRotation = rotateTouchPad.position;
		else
		{
			// Use tilt instead
//			print( iPhoneInput.acceleration );
			var acceleration = Input.acceleration;
			var absTiltX = Mathf.Abs( acceleration.x );
			if ( acceleration.z < 0 && acceleration.x < 0 )
			{
				if ( absTiltX >= tiltPositiveYAxis )
					camRotation.y = (absTiltX - tiltPositiveYAxis) / (1 - tiltPositiveYAxis);
				else if ( absTiltX <= tiltNegativeYAxis )
					camRotation.y = -( tiltNegativeYAxis - absTiltX) / tiltNegativeYAxis;
			}
			
			if ( Mathf.Abs( acceleration.y ) >= tiltXAxisMinimum )
				camRotation.x = -(acceleration.y - tiltXAxisMinimum) / (1 - tiltXAxisMinimum);
		}
		
		camRotation.x *= rotationSpeed.x;
		camRotation.y *= rotationSpeed.y;
		camRotation *= Time.deltaTime;
		
		// Rotate the character around world-y using x-axis of joystick
		thisTransform.Rotate( 0, camRotation.x, 0, Space.World );
		
		// Rotate only the camera with y-axis input
		cameraPivot.Rotate( -camRotation.y, 0, 0 );
	}
}