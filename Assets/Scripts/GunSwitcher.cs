using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GunSwitcher : MonoBehaviour
{

    [SerializeField] private HumanoidLandInput input;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private GameObject[] gunArray;
    [SerializeField] private int activeGun;

    private void Update()
    {
        SwitchWeapon();
    }

    private void RigSwitch()
    {
        var animator = GetComponent<Animator>();
        var rigBuilder = GetComponent<RigBuilder>();
        gunArray[activeGun].SetActive(true);
        leftHandIK.data.hint = gunArray[activeGun].GetComponentsInChildren<hintPlaceholder>()[0].transform;
        leftHandIK.data.target = gunArray[activeGun].GetComponentsInChildren<targetPlaceholder>()[0].transform;
        rigBuilder.Build();
        animator.Rebind();
    }

    private void SwitchWeapon()
    {
        
        switch (input.SwitchWeaponUpDown)
        {
            case > 0:
            {
                for (int i = 0; i < gunArray.Length; i++)
                {
                    gunArray[i].SetActive(false);
                }
                activeGun = activeGun == gunArray.Length - 1 ? 0 : activeGun + 1;
                RigSwitch();
                break;
            }
            case < 0:
            {
                for (int i = 0; i < gunArray.Length; i++)
                {
                    gunArray[i].SetActive(false);
                }
                activeGun = activeGun == 0 ? gunArray.Length - 1 : activeGun - 1;
                RigSwitch();
                break;
            }
        }
        
    }
}
