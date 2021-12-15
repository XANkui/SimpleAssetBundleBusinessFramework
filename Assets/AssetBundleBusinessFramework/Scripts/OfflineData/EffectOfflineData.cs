using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetBundleBusinessFramework { 

	public class EffectOfflineData : OfflineData
	{
		public ParticleSystem[] Particles;
		public TrailRenderer[] TrailRenderers;

        public override void ResetProp()
        {
            base.ResetProp();

            foreach (ParticleSystem particle in Particles)
            {
                particle.Clear(true);
                particle.Play();
            }

            foreach (TrailRenderer trail in TrailRenderers)
            {
                trail.Clear();
            }
        }

        public override void BindData()
        {
            base.BindData();
            Particles = gameObject.GetComponentsInChildren<ParticleSystem>();
            TrailRenderers = gameObject.GetComponentsInChildren<TrailRenderer>();
        }
    }
}
