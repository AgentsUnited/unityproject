using ASAPToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;

public class AddConvenientSkeletonTargets : MonoBehaviour
{
    private UMADynamicAvatar uma;

    // Use this for initialization
    void Start()
    {
        uma = GetComponent<UMADynamicAvatar>();
        uma.CharacterCreated.AddListener(OnCreated);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCreated(UMAData umaData)
    {
        ASAPToolkit.Unity.ASAPAgent agent = GetComponent<ASAPAgent>();
        if (agent == null) return;
        Transform lhand = umaData.GetBoneGameObject("hand_L").transform;
        if (lhand != null)
        {
            Transform lhandref = lhand.Find("l_hand_" + agent.agentId);
            if (lhandref == null)
            {
                lhandref = new GameObject("l_hand_" + agent.agentId).transform;
                lhandref.parent = lhand;
                lhandref.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                lhandref.localRotation = Quaternion.identity;
                lhandref.gameObject.AddComponent<ASAPToolkit.Unity.Environment.WorldObject>();
            }
        }
        Transform rhand = umaData.GetBoneGameObject("hand_R").transform;
        if (rhand != null)
        {
            Transform rhandref = rhand.Find("r_hand_" + agent.agentId);
            if (rhandref == null)
            {
                rhandref = new GameObject("r_hand_" + agent.agentId).transform;
                rhandref.parent = rhand;
                rhandref.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                rhandref.localRotation = Quaternion.identity;
                rhandref.gameObject.AddComponent<ASAPToolkit.Unity.Environment.WorldObject>();
            }
        }
    }
}