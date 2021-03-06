using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour
{
	// inspector properties
	public Transform enemy;
	public Transform player;
	
	public BezierNavigator navigator;
	public BezierPath path;
	public float interval = 0;
	public float skip = 0;
	public float fov = 30;
	public float viewDistance = 10;
	
	// internal state
	private bool delayed;
	private float delay;
	private int counter;
	private bool scared;
	
	// auxiliar variables
	private new Animation animation;
	private RaycastHit hit;
	
	public void Start()
	{
		delayed = false;
		counter = 0;
		navigator.OnCurve = OnCurve;
		navigator.OnComplete = OnComplete;
		navigator.Navigate(path.GetCurvesArray());
		animation = GetComponentInChildren<Animation>();
		animation.Play("walk");
	}

	public void Update ()
	{
		if(LineOfSight() && !scared){
			scared = true;
			delayed = true;
			delay = 2.2f;
			navigator.Pause();
			animation.Stop();
			animation.Play("worry", AnimationPlayMode.Queue);
			navigator.transform.LookAt(player);
			audio.Play ();
			GameObject.Find("Player").GetComponent<CharacterBehaviour>().Scare();
			Camera.main.GetComponent<CameraBehaviour>().Scare(enemy);
		}

		if(delayed){
			delay -= Time.deltaTime;
			if(delay < 0){
				scared = false;
				delayed = false;
				animation.Play("walk", AnimationPlayMode.Mix);
				navigator.Resume();
			}
		}
	}
	
	public void OnCurve()
	{
		if(counter++ < skip){ return; }
		counter = 0;
		delayed = true;
		delay = interval;
		navigator.Pause();
		animation.Play("idle", AnimationPlayMode.Mix);
	}
	
	public void OnComplete()
	{
		navigator.Navigate(path.GetCurvesArray());
	}
	
	private bool LineOfSight()
	{
		return Vector3.Angle(player.position - enemy.position, enemy.forward) < fov && 
			Vector3.Distance(player.position, enemy.position) < viewDistance && 
			Physics.Linecast(enemy.position, player.position, out hit) && hit.transform.CompareTag("Player");
	}
}
