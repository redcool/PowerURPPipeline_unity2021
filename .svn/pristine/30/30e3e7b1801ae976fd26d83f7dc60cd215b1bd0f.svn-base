using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Rendering.Universal
{
    partial class ScriptableRenderer
    {
        public void DequeuePass(ScriptableRenderPass pass)
        {
            if (m_ActiveRenderPassQueue.Contains(pass))
                m_ActiveRenderPassQueue.Remove(pass);
            
        }
    }
}
