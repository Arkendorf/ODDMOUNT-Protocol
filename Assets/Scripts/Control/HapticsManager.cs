using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticsManager : MonoBehaviour
{
    // Singleton instance
    private static HapticsManager _hapticsManager;
    public static HapticsManager Instance
    {
        get
        {
            if (!_hapticsManager)
            {
                _hapticsManager = FindObjectOfType(typeof(HapticsManager)) as HapticsManager;
            }
            return _hapticsManager;
        }
    }

    public XRBaseController leftController;
    public XRBaseController rightController;

    public enum Controller { Left, Right, Both}

    public void SendHapticImpulse(float amplitude, float duration, Controller controller)
    {
        if (controller == Controller.Left)
        {
            leftController?.SendHapticImpulse(amplitude, duration);
        }
        else if (controller == Controller.Right)
        {
            rightController?.SendHapticImpulse(amplitude, duration);
        }
        else
        {
            leftController?.SendHapticImpulse(amplitude, duration);
            rightController?.SendHapticImpulse(amplitude, duration);
        }
    }

    public void SendHapticImpulse(float amplitude, float duration, XRBaseController controller)
    {
        controller?.SendHapticImpulse(amplitude, duration);
    }
}
