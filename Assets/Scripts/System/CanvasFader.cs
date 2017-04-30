using UnityEngine;
using System.Collections;
namespace Gossage.System
{
    /// <summary>
    /// Performs alpha fade in/out of a canvas group 
    /// & optionally enables/disables the interaction at the correct time
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasFader : MonoBehaviour
    {
        [SerializeField] bool m_effectInteraction = true;
        private CanvasGroup m_canvas;
        private float m_fadeTarget, m_fadeRate;
        private bool m_enableWhenArrived;
        private bool m_fadingNow;
        void Awake()
        {
            m_canvas = GetComponent<CanvasGroup>();
            m_fadingNow = false;    // not fading now
        }
        void Update()
        {
            if (!m_fadingNow) return;
            float alpha = m_canvas.alpha;
            alpha = Mathf.MoveTowards(alpha, m_fadeTarget, m_fadeRate * Time.deltaTime);
            if (Mathf.Approximately(alpha,m_fadeTarget))
            {
                alpha = m_fadeTarget;
                m_fadingNow = false;
                if (m_effectInteraction)
                {
                    m_canvas.interactable = m_canvas.blocksRaycasts = m_enableWhenArrived;
                }
            }
            m_canvas.alpha = alpha;
        }

        public void FadeIn(float time=0.1f,float targetAlpha=1.0f)
        {
            Fade(time,targetAlpha,true);
        }
        public void FadeOut(float time = 0.1f, float targetAlpha = 0.0f)
        {
            Fade(time,targetAlpha,false);
        }
        public void Fade(float time, float targetAlpha,bool enableAtEnd)
        {
            m_fadingNow = true;
            m_enableWhenArrived = enableAtEnd;
            m_fadeTarget = targetAlpha;
            m_fadeRate = 1.0f / time;
        }
        public void SetOut(float targetAlpha = 0.0f)
        {
            m_fadingNow = false; // finished a fade
            if (m_effectInteraction)
            {
                m_canvas.interactable = m_canvas.blocksRaycasts = m_enableWhenArrived;
            }
            m_canvas.alpha = targetAlpha;
        }
    }

}