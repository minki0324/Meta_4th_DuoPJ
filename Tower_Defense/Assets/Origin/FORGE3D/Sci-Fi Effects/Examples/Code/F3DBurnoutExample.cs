﻿using UnityEngine;
using System.Collections;

namespace FORGE3D
{
    public class F3DBurnoutExample : MonoBehaviour
    {
        public ParticleSystem[] Heat;

        private bool heatShow;
        private MeshRenderer[] _turretParts;
        private int _burnoutId;

        private float burnout;
        private float heatBias = 0f;
        float timer;
        // Use this for initialization
        private void Start()
        {
            _burnoutId = Shader.PropertyToID("_Burnout");
            _turretParts = GetComponentsInChildren<MeshRenderer>();
          timer = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            
            timer += Time.deltaTime ;
            burnout = Mathf.Lerp(0, 1f, timer / 2f);

            if (burnout > heatBias && heatShow)
            {
                heatShow = false;

                for (var i = 0; i < Heat.Length; i++)
                    Heat[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else if (burnout <= heatBias && !heatShow)
            {
                heatShow = true;

                for (var i = 0; i < Heat.Length; i++)
                    Heat[i].Play(true);
            }


            for (var i = 0; i < _turretParts.Length; i++)
                _turretParts[i].material.SetFloat(_burnoutId, burnout);
        }
    }
}