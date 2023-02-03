using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.LowLevel;
using System;
using MyProject.Common;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCraft : SpacecraftBase
{
    protected override void HandleManeuver()
    {
        Vector3 rot = Vector3.zero;
        bool has_effective = false;

        if (keybord.aKey.isPressed) {
            rot += Vector3.up;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }
        else if (keybord.dKey.isPressed) {
            rot += Vector3.down;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }

        if (keybord.wKey.isPressed) {
            rot += Vector3.right;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }
        else if (keybord.sKey.isPressed) {
            rot += Vector3.left;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }

        if (keybord.qKey.isPressed) {
            rot += Vector3.forward;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }
        else if (keybord.eKey.isPressed) {
            rot += Vector3.back;
            has_effective = true;
            poseStablizer.Inhibit = true;
        }

        if (keybord.iKey.isPressed) {
            transController.MoveForward();
        } else if (keybord.kKey.isPressed) {
            transController.MoveBackward();
        }

        if (keybord.jKey.isPressed) {
            transController.MoveLeft();
        } else if (keybord.lKey.isPressed) {
            transController.MoveRight();
        }

        if (keybord.uKey.isPressed) {
            transController.MoveUpward();
        } else if (keybord.hKey.isPressed) {
            transController.MoveDownward();
        }

        if (!has_effective && poseStablizer.Enable && poseStablizer.Inhibit) {
            poseStablizer.Inhibit = false;
        }

        Vector3 nrot = rot.normalized;
        nrot.Scale(Kinematics.MaxFlywheelTorque * 1000f / PhysicsConstant.AbsoluteMass(craft.Mass));
        
        float acc = Kinematics.GetAcceleration(craft);
        if (acc > float.Epsilon) {
            Vector3 v = Vector3.forward * acc;
            AddRelativeAcceleration(v);
        }
        body.AddRelativeTorque(nrot, ForceMode.VelocityChange);
    }

    protected override void onInputEvent(InputEventPtr ptr, InputDevice dev)
    {
        if (dev.deviceId != keybord.deviceId) {
            return;
        }

        Keyboard kbd = (Keyboard)dev;

        if (kbd.tKey.isPressed) {
            poseStablizer.SwitchEnable();
        }

        if (kbd.digit1Key.isPressed) {
            // prograde
            poseAdjuster.SetDynamicTargetPose(AttitudePrograde);
            poseStablizer.Enable = false;
        }
        else if (kbd.digit2Key.isPressed) {
            // retrograde
            poseAdjuster.SetDynamicTargetPose(AttitudeRetrograde);
            poseStablizer.Enable = false;
        }

        if (kbd.digit3Key.isPressed) {
            // normal
            poseAdjuster.SetDynamicTargetPose(AttitudeNormal);
            poseStablizer.Enable = false;
        }
        else if (kbd.digit4Key.isPressed) {
            // antinormal
            poseAdjuster.SetDynamicTargetPose(AttitudeAntinormal);
            poseStablizer.Enable = false;
        }

        if (kbd.digit5Key.isPressed) {
            // radial
            poseAdjuster.SetDynamicTargetPose(AttitudeRadial);
            poseStablizer.Enable = false;
        }
        else if (kbd.digit6Key.isPressed) {
            // antiradial
            poseAdjuster.SetDynamicTargetPose(AttitudeAntiradial);
            poseStablizer.Enable = false;
        }

        if (kbd.digit0Key.isPressed) {
            // clear goal pose
            poseAdjuster.SetTargetPose(Vector3.zero);
            poseStablizer.Enable = true;
        }

        if (kbd.vKey.isPressed) {
            DrawTrajectory(false);
        }
        else if (kbd.bKey.isPressed) {
            DrawTrajectory(true);
        }

        if (kbd.leftShiftKey.isPressed) {
            Kinematics.IncreaseThrottle();
        }
        else if (kbd.leftCtrlKey.isPressed) {
            Kinematics.DecreaseThrottle();
        }

        if (kbd.xKey.isPressed) {
            Kinematics.ThrottleRatio = 0f;
        }
        else if (kbd.zKey.isPressed) {
            Kinematics.ThrottleRatio = 1f;
        }
    }
}
