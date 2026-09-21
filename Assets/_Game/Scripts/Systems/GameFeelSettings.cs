using System;
using UnityEngine;

namespace Shift.Game
{
    [Serializable]
    public sealed class GameFeelSettings
    {
        [Header("Timing (seconds)")]
        [Range(.12f, .18f)] public float moveDuration = .14f;
        [Range(.02f, .08f)] public float tapAnticipation = .045f;
        [Range(.06f, .25f)] public float piecePulseDuration = .14f;
        [Range(.06f, .2f)] public float blockedDuration = .12f;
        [Range(.06f, .2f)] public float exitDuration = .1f;
        [Range(.06f, .2f)] public float shakeDuration = .1f;
        [Range(.1f, .5f)] public float successDuration = .28f;
        [Range(.1f, .4f)] public float hudPulseDuration = .22f;
        [Range(.2f, 1f)] public float chainFadeDuration = .55f;
        [Range(.08f, .3f)] public float panelDuration = .16f;
        [Header("Strength")]
        [Range(0, .25f)] public float tapPunch = .12f;
        [Range(0, .2f)] public float impactPunch = .07f;
        [Range(0, 10)] public float blockedPixels = 5;
        [Range(0, 6)] public float shakePixels = 2.5f;
        [Range(0, .08f)] public float successScale = .035f;
        [Range(0, .3f)] public float hudPunch = .12f;
        [Range(0, 1)] public float trailOpacity = .25f;
        [Range(0, 1)] public float highlightStrength = .3f;
        [Min(0)] public int lowMovesThreshold = 2;
        [Header("Accessibility / device fallback")]
        public bool reducedMotion;
        public bool hapticsEnabled;
        [Min(.1f)] public float hapticCooldown = .25f;
    }
}
