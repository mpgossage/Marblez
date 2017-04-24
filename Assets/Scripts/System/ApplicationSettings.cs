using UnityEngine;
using System.Collections;

namespace Gossage.System
{
    // simple helper to provide singleton like access to settings 
    public class ApplicationSettings : MonoBehaviour
    {
        [SerializeField]
        private ApplicationSettingsObject m_settings;
        private static ApplicationSettingsObject s_settings;

        void Awake()
        {
            s_settings = m_settings;
        }
        public static ApplicationSettingsObject Instance { get { return s_settings; } }
    }
}