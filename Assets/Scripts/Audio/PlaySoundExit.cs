using UnityEngine;

public class PlaySoundExit : StateMachineBehaviour
{
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0, 1)] private float volume = 1f;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int LayerIndex)
    {
        SoundManager.PlaySound(sound, volume);
    }

}
