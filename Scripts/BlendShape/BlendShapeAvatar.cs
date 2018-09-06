﻿using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UniGLTF;

namespace VRM
{
    [CreateAssetMenu(menuName = "VRM/BlendShapeAvatar")]
    public class BlendShapeAvatar : ScriptableObject
    {
        [SerializeField]
        public List<BlendShapeClip> Clips = new List<BlendShapeClip>();

#if UNITY_EDITOR
        [ContextMenu("Restore")]
        void Restore()
        {
            var assetPath = UnityPath.FromAsset(this);
            if (assetPath.IsNull)
            {
                return;
            }


            foreach(var x in assetPath.Parent.ChildFiles)
            {
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<BlendShapeClip>(x.Value);
                if (clip == null) continue;

                if (!Clips.Contains(clip))
                {
                    Clips.Add(clip);
                }

                Debug.LogFormat("{0}", clip.name);
            }
            Clips = Clips.OrderBy(x => BlendShapeKey.CreateFrom(x)).ToList();
        }
#endif

        /// <summary>
        /// Unknown以外で存在しないものを全て作る
        /// </summary>
        public void CreateDefaultPreset()
        {
            foreach (var preset in ((BlendShapePreset[])Enum.GetValues(typeof(BlendShapePreset)))
                .Where(x => x != BlendShapePreset.Unknown))
            {
                CreateDefaultPreset(preset);
            }
        }

        void CreateDefaultPreset(BlendShapePreset preset)
        {
            var clip = GetClip(preset);
            if (clip != null) return;
            clip = ScriptableObject.CreateInstance<BlendShapeClip>();
            clip.name = preset.ToString();
            clip.BlendShapeName = preset.ToString();
            clip.Preset = preset;
            Clips.Add(clip);
        }

        public void SetClip(BlendShapeKey key, BlendShapeClip clip)
        {
            int index = -1;
            try
            {
                index = Clips.FindIndex(x => key.Match(x));
            }
            catch (Exception)
            {

            }
            if (index == -1)
            {
                Clips.Add(clip);
            }
            else
            {
                Clips[index] = clip;
            }
        }

        public BlendShapeClip GetClip(BlendShapeKey key)
        {
            if (Clips == null) return null;
            return Clips.FirstOrDefault(x => key.Match(x));
        }

        public BlendShapeClip GetClip(BlendShapePreset preset)
        {
            return GetClip(new BlendShapeKey(preset));
        }

        public BlendShapeClip GetClip(String name)
        {
            return GetClip(new BlendShapeKey(name));
        }

        public bool Apply(string name, Transform transform, float value)
        {
            var clip = GetClip(name);
            if (clip == null) return false;
            clip.Apply(transform, value);
            return true;
        }
    }
}
