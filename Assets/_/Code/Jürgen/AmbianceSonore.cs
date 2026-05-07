using UnityEngine;

public class AmbianceSonore : MonoBehaviour
{
    public AudioSource m_audioSource;
    public bool m_isPlay;

    public void SoundControl()
    {
        m_isPlay = !m_isPlay;

        if (m_isPlay)
        {
            m_audioSource.UnPause();
        }
        else
        {
            m_audioSource.Pause();
        }
    }



}
