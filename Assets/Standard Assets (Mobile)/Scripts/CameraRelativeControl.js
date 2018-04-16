//////////////////////////////////////////////////////////////
// CameraRelativeControl.js
// Penelope iPhone Tutorial
//
// CameraRelativeControl creates a control scheme similar to what
// might be found in 3rd person platformer games found on consoles.
// The left stick is used to move the character, and the right
// stick is used to rotate the camera around the character.
// A quick double-tap on the right joystick will make the 
// character jump. 
//////////////////////////////////////////////////////////////

#pragma strict

// This script must be attached to a GameObject that has a CharacterController
@script RequireComponent( CharacterController )

var moveJoystick : Joystick;
var rotateJoystick : Joystick;

var cameraPivot : Transform;						// The transform used for camera rotation
var cameraTransform : Transform;					// The actual transform of the camera

var speed : float = 5;								// Ground speed
var jumpSpeed : float = 8;
var inAirMultiplier : float = 0.25; 				// Limiter for ground speed while jumping
var rotationSpeed : Vector2 = Vector2( 50, 25 );	// Camera rotation speed for each axis

private var thisTransform : Transform;
private var character : CharacterController;
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

function FaceMovementDirection()
{	
	var horizontalVelocity : Vector3 = character.velocity;
	horizontalVelocity.y = 0; // Ignore vertical movement
	
	// If moving significantly in a new direction, point that character in that direction
	if ( horizontalVelocity.magnitude > 0.1 )
		thisTransform.forward = horizontalVelocity.normalized;
}

function OnEndGame()
{
	// Disable joystick when the game ends	
	moveJoystick.Disable();
	rotateJoystick.Disable();
	
	// Don't allow any more control changes when the game ends
	this.enabled = false;
}

function Update()
{
	var movement = cameraTransform.TransformDirection( Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );
	// We only want the camera-space horizontal direction
	movement.y = 0;
	movement.Normalize(); // Adjust magnitude after ignoring vertical movement
	
	// Let's use the largest component of the joystick position for the speed.
	var absJoyPos = Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );
	movement *= speed * ( ( absJoyPos.x > absJoyPos.y ) ? absJoyPos.x : absJoyPos.y );
	
	// Check for jump
	if ( character.isGrounded )
	{
		if ( !rotateJoystick.IsFingerDown() )
			canJump = true;
		
		if ( canJump && rotateJoystick.tapCount == 2 )
		{
			// Apply the current movement to launch velocity
			velocity = character.velocity;
			velocity.y = jumpSpeed;
			canJump = false;
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
	
	// Face the character to match with where she is moving
	FaceMovementDirection();	
	
	// Scale joystick input with rotation speed
	var camRotation = rotateJoystick.position;
	camRotation.x *= rotationSpeed.x;
	camRotation.y *= rotationSpeed.y;
	camRotation *= Time.deltaTime;
	
	// Rotate around the character horizontally in world, but use local space
	// for vertical rotation
	cameraPivot.Rotate( 0, camRotation.x, 0, Space.World );
	cameraPivot.Rotate( camRotation.y, 0, 0 );
}