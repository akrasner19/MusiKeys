using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackFingertips : MonoBehaviour
{
    // Start is called before the first frame update
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;

    public List<GameObject> fingerTips;
    private List<OVRBone> fingertipBones;
    private List<OVRBone> testrightbones;
    private OVRBone testingbone;
    void Start()
    {
        testrightbones = new List<OVRBone>(rightSkeleton.Bones);
        foreach (OVRBone bone in leftSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_PinkyTip)
            {
                fingertipBones[0] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_RingTip)
            {
                fingertipBones[1] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
            {
                fingertipBones[2] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                fingertipBones[3] = bone;
                testingbone = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
            {
                fingertipBones[4] = bone;
            }
        }
        foreach (OVRBone bone in testrightbones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_PinkyTip)
            {
                fingertipBones[5] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_RingTip)
            {
                fingertipBones[6] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
            {
                fingertipBones[7] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                fingertipBones[8] = bone;
            }
            else if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
            {
                fingertipBones[9] = bone;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 10; ++i)
        {
            fingerTips[i].transform.position = fingertipBones[i].Transform.position;
        }
        //fingerTips[3].transform.position = testingbone.Transform.position;
    }
}
