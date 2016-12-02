using UnityEngine;
using System.Collections;
using TreeSharpPlus;

using NPC;
using System;

/// <summary>
/// We will use this module as the connector of NPC with
/// TreeSharpPlus. The main purpose of this module is to
/// be a shell for the already implemented affordances of
/// the agents. Nodes will tick here and the module on each
/// tick will return the current status of each node.
/// 
/// Note that for each affordance, we will need two functions:
///     1. Node return to create the actual Behavior Tree component
///     2. A wrapper controller on the affordance which will be ticked on
///        and need to return, on tick, the status of the node.
/// Although this will add an extra layer of complexity, I will concentrate
/// all the implementation on this module rather than mixing TreeSharpPlus
/// with the NPC affordances. That will allow, in the long run, for more
/// freedom, scalable, maitainable and clear code.
/// 
/// </summary>

public class NPCBehavior : MonoBehaviour, INPCModule {

    #region Members
    
    private NPCController g_NPCController;
    public bool Enabled = true;
    
    #endregion

    #region Unity_Methods

    void Awake() {
        g_NPCController = GetComponent<NPCController>();
    }

    #endregion

    #region Public_Functions
    
    public Node NPCBehavior_GoTo(Val<Vector3> location) {
        return new LeafInvoke(
            () => Behavior_GoTo(location.Value)
        );
    }


    public Node NPC_RunTo(Val<Vector3> location) {
        return new LeafInvoke(
            () => Behavior_RunTo(location)
        );
    }

    public Node NPC_Stop() {
        return new LeafInvoke(
            () => Behavior_Stop()
        );
    }

    public Node NPC_DoGesture(GESTURE_CODE gesture) {
        return new LeafInvoke(
            () => Behavior_DoGesture(gesture)
        );
    }

    #endregion

    #region Private_Functions
    
    private RunStatus Behavior_GoTo(Val<Vector3> location) {
        Vector3 val = location.Value;
        if (g_NPCController.Body.Navigating)
            return RunStatus.Running;
        else if (g_NPCController.Body.IsAtTargetLocation(val)) {
            return RunStatus.Success;
        }
        else {
            try {
                g_NPCController.GoTo(val);
                return RunStatus.Running;
            } catch(System.Exception e) {
                // this will occur if the target is unreacheable
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_RunTo(Val<Vector3> location) {
        Vector3 val = location.Value;
        if (g_NPCController.Body.Navigating)
            return RunStatus.Running;
        else if (g_NPCController.Body.IsAtTargetLocation(val)) {
            return RunStatus.Success;
        }
        else {
            try {
                g_NPCController.RunTo(val);
                // Debug.Log("Run To "+val);
                return RunStatus.Running;
            } catch(System.Exception e) {
                // this will occur if the target is unreacheable
                Debug.Log("Unreachable at "+val);
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_Stop() {
        g_NPCController.Body.StopNavigation();
        return RunStatus.Success;
    }

    private RunStatus Behavior_DoGesture(GESTURE_CODE gesture) {
        if (!g_NPCController.Body.IsGesturePlaying(gesture)) {
            g_NPCController.Body.DoGesture(gesture);
        }
        // Debug.Log("gesturing");
        return RunStatus.Success;
    }

    #endregion

    #region INPCModule

    public bool IsEnabled() {
        return Enabled;
    }

    public string NPCModuleName() {
        return "NPC TreeSP/Connector";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.AI;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.BEHAVIOR;
    }

    public void RemoveNPCModule() {
        throw new NotImplementedException();
    }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    #endregion
}
