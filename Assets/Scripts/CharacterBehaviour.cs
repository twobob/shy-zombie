using UnityEngine;
using System.Collections;

public class CharacterBehaviour : MonoBehaviour
{
	// Inspector properties
	public float walkSpeed = 1.5f;
	public float walkAcceleration = 5.0f;
	public float walkDeceleration = 5.0f;
	public float walkAnimationSpeed = 2.5f;
	public float rotateSpeed = 1.5f;
	public float idleTime = 10.0f;
	
	private AudioSource bored;
	private AudioSource eating;
	private AudioSource falling;
	
	// Global state
	public static bool leftPressed;
	public static bool rightPressed;
	public static bool walkPressed;
	public static int brainz = 0;
	
	private int state;
	private int previousState;
	private float currentSpeed;
	private float _idleTime;
	private float scaredTime;
	
	// Auxiliar variables
	private Touch touch;
	private new Animation animation;
	private CharacterController controller;
	private Vector3 forward;
	private GameObject[] brainzArr;
	
	private Vector2 leftButton;
	private Vector2 rightButton;
	private Vector2 walkButton;
	
	
	public void Start ()
	{
		leftPressed = false;
		rightPressed = false;
		walkPressed = false;
		state = CharacterConstants.IDLE_STATE;
		previousState = CharacterConstants.IDLE_STATE;
		
		animation = GetComponentInChildren<Animation>();
		animation.Play("zombie_idle");
		animation["zombie_walk"].speed = walkAnimationSpeed;
		
		controller = GetComponent<CharacterController>();
		
		leftButton = new Vector2(80, 184);
		rightButton = new Vector2(192, 80);
		walkButton = new Vector2(888, 136);
		
		brainzArr = GameObject.FindGameObjectsWithTag("Collectable");
		
		AudioSource[] sources = GetComponents<AudioSource>();
		bored = sources[1];
		eating = sources[2];
		falling = sources[3];
	}
	
	public void Update ()
	{
		DetectInput();
		SwitchState();
		StopFreeFall();
		UpdateState();
		ChangeAnimation();
	}
	
	private void DetectInput()
	{
		leftPressed = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
		rightPressed = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
		walkPressed = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);

		for(int i = 0; i < Input.touchCount; i++){
			touch = Input.GetTouch(i);
			if(Vector2.Distance(touch.position, leftButton) < 64){ leftPressed = true; }
			if(Vector2.Distance(touch.position, rightButton) < 64){ rightPressed = true; }
			if(Vector2.Distance(touch.position, walkButton) < 64){ walkPressed = true; }
		}
	}
	
	private void SwitchState()
	{
		if(state == CharacterConstants.SCARED_STATE){
			scaredTime -=Time.deltaTime;
			if(scaredTime > 0){
				return;
			} else {
				Restart();
			}
		}
		
		if(walkPressed){
			state = CharacterConstants.WALKING_STATE;
		} else if(leftPressed) {
			state = CharacterConstants.TURNING_LEFT_STATE;
		} else if(rightPressed){
			state = CharacterConstants.TURNING_RIGHT_STATE;
		} else {
			state = CharacterConstants.IDLE_STATE;
		}
	}
	
	private void UpdateState()
	{
		if(state == CharacterConstants.SCARED_STATE){
			return;
		}
		
		if(leftPressed){
			transform.Rotate(0, -rotateSpeed, 0);
			
		} else if(rightPressed) {
			transform.Rotate(0, rotateSpeed, 0);
		}
		
		if(walkPressed){
			currentSpeed += walkAcceleration * Time.deltaTime;
		} else {
			currentSpeed -= walkDeceleration * Time.deltaTime;
		}
		
		currentSpeed = Mathf.Clamp(currentSpeed, 0, walkSpeed);
		forward = transform.TransformDirection(Vector3.forward);
		controller.SimpleMove(forward * currentSpeed);
	}
	
	private void StopFreeFall()
	{
		if(transform.position.y < -3){
			state = CharacterConstants.FALLING_STATE;
		}
		if(transform.position.y < -30){
			animation.Play("zombie_idle");
			Restart();
		}
	}
	
	private void ChangeAnimation()
	{
		if(previousState == state){
			if(state == CharacterConstants.IDLE_STATE){
				_idleTime += Time.deltaTime;
				if(_idleTime > idleTime){
					animation.Play("zombie_attack");
					animation.Play("zombie_idle", AnimationPlayMode.Queue);
					_idleTime = -2;
					bored.Play();
				}
			}
			return;
		}
		
		_idleTime = 0;
		
		switch(state){
		case CharacterConstants.IDLE_STATE:
			animation.CrossFade("zombie_idle");
			audio.Pause();
			break;
		case CharacterConstants.TURNING_LEFT_STATE:
		case CharacterConstants.TURNING_RIGHT_STATE:
		case CharacterConstants.WALKING_STATE:
			animation.CrossFade("zombie_walk");
			audio.Play();
			break;
		
		case CharacterConstants.FALLING_STATE:
			animation.CrossFade("flight_idle");
			falling.Play();
			break;
		} 
		
		previousState = state;
	}
	
	public void OnTriggerEnter (Collider trigger)
	{
		if(trigger.tag == "Collectable"){
			brainz++;
			trigger.gameObject.SetActiveRecursively(false);
			eating.Play();
		}
		
		if(trigger.tag == "Portal"){
			Application.LoadLevel(trigger.GetComponent<Portal>().level);
		}
	}
	
	public void Scare()
	{
		if (state == CharacterConstants.SCARED_STATE) { return; }
		
		state = CharacterConstants.SCARED_STATE;
		scaredTime = 2;
	}
	
	public void Restart()
	{
		GameObject spawnPoint = GameObject.Find("SpawnPoint");
		transform.position = spawnPoint.transform.position;
		transform.rotation = spawnPoint.transform.rotation;
		currentSpeed = 0;
		brainz = 0;
		for(int i = 0; i < brainzArr.Length; i++){
			brainzArr[i].SetActiveRecursively(true);
		}
	}
}
