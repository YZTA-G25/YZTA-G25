using UnityEngine;

public class PlayFootStep : MonoBehaviour
{
    public void PlaySound()
    {
        SoundManager.PlaySound(SoundType.FOOTSTEP);
    }
}
