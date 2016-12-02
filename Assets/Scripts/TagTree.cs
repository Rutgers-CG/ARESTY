using UnityEngine;
using System;
using System.Collections;
using TreeSharpPlus;
using NPC;

public class TagTree : MonoBehaviour {

	public GameObject player;
	public GameObject player2;
	public float touchDistance;
	public float runDist = 5;
	public int[] angles = new int[6] {-45, -30, -15, 15, 30, 45};
	private Vector3 projections;

	private BehaviorAgent ba;
	private BehaviorAgent ba2;
	private bool on;

	// Use this for initialization
	void Start() {
		ba = new BehaviorAgent(this.BuildTreeRoot());
		BehaviorManager.Instance.Register(ba);
		// BehaviorManager.Instance.Register(ba2);
		ba.StartBehavior();
		on = true;
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			Debug.Log("Swap");
			if (on) {
				ba2 = new BehaviorAgent(this.BuildTreeRoot2());
				BehaviorManager.Instance.ClearReceivers();
				BehaviorManager.Instance.Register(ba2);
				ba2.StartBehavior();
			}
			else {
				ba = new BehaviorAgent(this.BuildTreeRoot());
				BehaviorManager.Instance.ClearReceivers();
				BehaviorManager.Instance.Register(ba);
				ba.StartBehavior();
			}
			on = !on;
		}
	}

	protected Node Tag(GameObject it, GameObject p) {
		NPCBehavior pb = p.GetComponent<NPCBehavior>();
		NPCBehavior itb = it.GetComponent<NPCBehavior>();
		// change evade to use raycasts to find better direction to run in
		Func<Vector3> evFunc = delegate() {
			Vector3 delta = (p.transform.position - it.transform.position);
			delta.Normalize();
			Vector3 pos = p.transform.position + runDist*delta;
			NavMeshHit hit;
			NavMesh.SamplePosition(pos, out hit, runDist, NavMesh.AllAreas);
			Vector3 rot;
			Vector3 maxPos = hit.position;
			foreach (int a in angles) {
				rot = Quaternion.Euler(0,a,0) * delta;
				pos = p.transform.position + runDist*rot;
				NavMesh.SamplePosition(pos, out hit, runDist, NavMesh.AllAreas);
				if ((hit.position-p.transform.position).magnitude>(maxPos-p.transform.position).magnitude) {
					maxPos = hit.position;
				}
			}
			return maxPos;
		};
		Val<Vector3> evade = Val.V(evFunc);

		Func<Vector3> evInvFunc = delegate() {
			Vector3 delta = (it.transform.position - p.transform.position);
			delta.Normalize();
			Vector3 pos = it.transform.position + runDist*delta;
			NavMeshHit hit;
			NavMesh.SamplePosition(pos, out hit, runDist, NavMesh.AllAreas);
			Vector3 rot;
			Vector3 maxPos = hit.position;
			foreach (int a in angles) {
				rot = Quaternion.Euler(0,a,0) * delta;
				pos = it.transform.position + runDist*rot;
				NavMesh.SamplePosition(pos, out hit, runDist, NavMesh.AllAreas);
				if ((hit.position-it.transform.position).magnitude>(maxPos-it.transform.position).magnitude) {
					maxPos = hit.position;
				}
			}
			return maxPos;
		};
		Val<Vector3> evadeInv = Val.V(evInvFunc);

		Val<Vector3> chase = Val.V(() => p.transform.position);

		return new Sequence (
			new DecoratorForceStatus (RunStatus.Success, new SequenceParallel (
				new LeafTrace(it+" chasing "+p),
				new DecoratorLoop (new LeafAssert(() => (it.transform.position-p.transform.position).magnitude > touchDistance)),
				pb.NPC_RunTo(evade),
				itb.NPC_RunTo(chase)
			)),
			itb.NPC_DoGesture(GESTURE_CODE.GRAB_FRONT),
			pb.NPC_Stop(),
			new DecoratorForceStatus (RunStatus.Success, new SequenceParallel (
				new DecoratorLoop (new LeafAssert(() => (it.transform.position-p.transform.position).magnitude < 1.5*touchDistance)),
				itb.NPC_RunTo(evadeInv)
			))		
		);

	}

	protected Node BuildTreeRoot() {
		return new DecoratorLoop (
			new DecoratorForceStatus (RunStatus.Success, 
			new Sequence (
				new DecoratorForceStatus (RunStatus.Success, Tag(player, player2)),
				new DecoratorForceStatus (RunStatus.Success, Tag(player2, player))
			))
		);
	}

	protected Node BuildTreeRoot2() {
		return new DecoratorLoop (
			new DecoratorForceStatus (RunStatus.Success, 
			new Sequence (
				new DecoratorForceStatus (RunStatus.Success, Tag(player2, player)),
				new DecoratorForceStatus (RunStatus.Success, Tag(player, player2))
			))
		);
	}
}
