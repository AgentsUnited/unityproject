using System.Collections;
using System.Collections.Generic;
using UMA;
using UnityEngine;

public class AddNewUMAToGretaTracker : MonoBehaviour
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
        GameObject greta_environment_synchronizer = GameObject.FindGameObjectWithTag("greta_environment_synchronizer");
        Transform head = umaData.GetBoneGameObject("Head").transform;
        Transform headcenter = head.Find("head_" + umaData.name);
        greta_environment_synchronizer.GetComponent<GretaEnvironmentSynchronizer>().synchronizedObjects.Add(headcenter.gameObject);
    }
}