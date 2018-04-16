//////////////////////////////////////////////////////////////
// PlayerRelativeControl.js
// Penelope iPhone Tutorial
//
// PlayerRelativeControl creates a control scheme similar to what
// might be found in a 3rd person, over-the-shoulder game found on
// consoles. The left stick is used to move the character, and the
// right stick is used to rotate the character. A quick double-tap
// on the right joystick will make the character jump.
//////////////////////////////////////////////////////////////

#pragma strict

@script RequireComponent( CharacterController )

// This script must be attached to a GameObject that has a CharacterController
var moveJoystick : Joystick;
var rotateJoystick : Joystick;

var cameraPivot : Transform;						// The transform used for camera rotation

var forwardSpeed : float = 4;
var backwardSpeed : float = 1;
var sidestepSpeed : float = 1;
var jumpSpeed : float = 8;
var inAirMultiplier : float = 0.25;					// Limiter for ground speed while jumping
var rotationSpeed : Vector2 = Vector2( 50, 25 );	// Camera rotation speed for each axis

private var thisTransform : Transform;
private var character : CharacterController;
private var cameraVelocity : Vector3;
private var velocity : Vector3;						// Used for continuing momentum while in air

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
	moveJoystick.Disable();
	rotateJoystick.Disable();	

	// Don't allow any more control changes when the game ends
	this.enabled = false;
}

function Update()
{
	var movement = thisTransform.TransformDirection( Vector3( moveJoystick.position.x, 0, moveJoystick.position.y ) );

	// We only want horizontal movement
	movement.y = 0;
	movement.Normalize();

	var cameraTarget = Vector3.zero;

	// Apply movement from move joystick
	var absJoyPos = Vector2( Mathf.Abs( moveJoystick.position.x ), Mathf.Abs( moveJoystick.position.y ) );	
	if ( absJoyPos.y > absJoyPos.x )
	{
		if ( moveJoystick.position.y > 0 )
			movement *= forwardSpeed * absJoyPos.y;
		else
		{
			movement *= backwardSpeed * absJoyPos.y;
			cameraTarget.z = moveJoystick.position.y * 0.75;
		}
	}
	else
	{
		movement *= sidestepSpeed * absJoyPos.x;
		
		// Let's move the camera a bit, so the character isn't stuck under our thumb
		cameraTarget.x = -moveJoystick.position.x * 0.5;
	}
	
	// Check for jump
	if ( character.isGrounded )
	{
		if ( rotateJoystick.tapCount == 2 )
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
		
		// Move the camera back from the character when we jump
		cameraTarget.z = -jumpSpeed * 0.25;
		
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
	
	// Seek camera towards target position
	var pos = cameraPivot.localPosition;
	pos.x = Mathf.SmoothDamp( pos.x, cameraTarget.x, cameraVelocity.x, 0.3 );
	pos.z = Mathf.SmoothDamp( pos.z, cameraTarget.z, cameraVelocity.z, 0.5 );
	cameraPivot.localPosition = pos;

	// Apply rotation from rotation joystick
	if ( character.isGrounded )
	{
		var camRotation = rotateJoystick.position;
		camRotation.x *= rotationSpeed.x;
		camRotation.y *= rotationSpeed.y;
		camRotation *= Time.deltaTime;
		
		// Rotate the character around world-y using x-axis of joystick
		thisTransform.Rotate( 0, camRotation.x, 0, Space.World );
		
		// Rotate only the camera with y-axis input
		cameraPivot.Rotate( camRotation.y, 0, 0 );
	}
}