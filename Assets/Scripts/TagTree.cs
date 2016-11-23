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

	private BehaviorAgent behaviorAgent;

	// Use this for initialization
	void Start() {
		behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
		BehaviorManager.Instance.Register(behaviorAgent);
		behaviorAgent.StartBehavior();
	}
	
	// Update is called once per frame
	void Update() {
		// later, for triggering on user input
		// if (Input.GetKeyDown(KeyCode.R) == true) {
		// 	BehaviorEvent.Run(this.ConversationTree(), Wanderer, Friend);
		// }
	}

	protected Node Tag(GameObject it, GameObject p, bool check) {
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

		Val<Vector3> chase = Val.V(() => p.transform.position);

		if (check) {
			return new SequenceParallel (
				new LeafTrace(it+" chasing "+p),
				new LeafAssert(() => (it.transform.position-p.transform.position).magnitude > touchDistance),
				pb.NPC_RunTo(evade),
				itb.NPC_RunTo(chase)
			);
		}
		return new SequenceParallel (
			new LeafTrace(p+" tagged "+it),
			pb.NPC_RunTo(evade),
			itb.NPC_Stop(),
			new DecoratorInvert(new LeafWait(200))
		);

	}

	protected Node BuildTreeRoot() {
		return new DecoratorLoop (
			new DecoratorForceStatus (RunStatus.Success, 
			new Sequence (
				new DecoratorForceStatus (RunStatus.Success, Tag(player, player2, true)),
				// insert actual tagging gesture
				new DecoratorForceStatus (RunStatus.Success, Tag(player2, player, false)),
				new DecoratorForceStatus (RunStatus.Success, Tag(player2, player, true)),
				// insert actual tagging behavior
				new DecoratorForceStatus (RunStatus.Success, Tag(player, player2, false))
			))
		);
	}
}
