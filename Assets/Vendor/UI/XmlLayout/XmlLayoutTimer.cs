using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace UI.Xml
{
    internal class DelayedEditorAction
    {
        internal double TimeToExecute;
        internal Action Action;
        internal MonoBehaviour ActionTarget;
        internal bool ForceEvenIfTargetIsGone;

        public DelayedEditorAction(double timeToExecute, Action action, MonoBehaviour actionTarget, bool forceEvenIfTargetIsGone = false)
        {
            TimeToExecute = timeToExecute;
            Action = action;
            ActionTarget = actionTarget;
            ForceEvenIfTargetIsGone = forceEvenIfTargetIsGone;
        }
    }

    public static class XmlLayoutTimer
    {
        private static XmlLayoutTimerComponent _timerComponent;
        private static XmlLayoutTimerComponent timerComponent
        {
            get
            {
                if (_timerComponent == null)
                {
                    _timerComponent = GameObject.FindObjectOfType<XmlLayoutTimerComponent>();

                    if (_timerComponent == null && !IsQuitting)
                    {
                        var timerGO = new GameObject("XmlLayoutTimer");
                        _timerComponent = timerGO.AddComponent<XmlLayoutTimerComponent>();

                        GameObject.DontDestroyOnLoad(timerGO);
                    }
                }

                return _timerComponent;
            }
        }

        public static bool IsFirstFrame
        {
            /*get { return timerComponent.IsFirstFrame; } */
            get { return Time.frameCount <= 1; }
        }

        private static bool IsQuitting { get; set; }

        public static void StartCoroutine(IEnumerator coroutine)
        {
            if (Application.isPlaying) timerComponent.StartCoroutine(coroutine);
        }

#if UNITY_2018_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod()]
        public static void OnLoad()
        {
            Application.quitting += () => IsQuitting = true;
        }
#endif

#if UNITY_EDITOR
        static List<DelayedEditorAction> delayedEditorActions = new List<DelayedEditorAction>();

        static XmlLayoutTimer()
        {
            UnityEditor.EditorApplication.update += EditorUpdate;
        }
#endif

        public static WaitForSecondsRealtime GetWaitForSecondsRealtimeInstruction(float seconds)
        {
            return new WaitForSecondsRealtime(seconds);
        }

        public static WaitForSeconds GetWaitForSecondsInstruction(float seconds)
        {
            return new WaitForSeconds(seconds);
        }

        static void EditorUpdate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            var actionsToExecute = delayedEditorActions.Where(dea => UnityEditor.EditorApplication.timeSinceStartup >= dea.TimeToExecute).ToList();

            if (actionsToExecute.Count == 0) return;

            foreach (var actionToExecute in actionsToExecute)
            {
                try
                {
                    if (actionToExecute.ActionTarget != null || actionToExecute.ForceEvenIfTargetIsGone) // don't execute if the target is gone
                    {
                        actionToExecute.Action.Invoke();
                    }
                }
                finally
                {
                    delayedEditorActions.Remove(actionToExecute);
                }
            }
#endif
        }

        /// <summary>
        /// Call Action 'action' after the specified delay, provided the 'actionTarget' is still present and active in the scene at that time.
        /// Can be used in both edit and play modes.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        /// <param name="actionTarget"></param>
        public static void DelayedCall(float delay, Action action, MonoBehaviour actionTarget, bool forceEvenIfObjectIsInactive = false)
        {
            if (Application.isPlaying)
            {
                if (timerComponent != null) timerComponent.DelayedCall(delay, action, actionTarget, forceEvenIfObjectIsInactive);
            }
#if UNITY_EDITOR
            else
            {
                delayedEditorActions.Add(new DelayedEditorAction(UnityEditor.EditorApplication.timeSinceStartup + delay, action, actionTarget, forceEvenIfObjectIsInactive));
            }
#endif
        }

        /// <summary>
        /// Shorthand for DelayedCall(0, action, actionTarget)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="actionTarget"></param>
        public static void AtEndOfFrame(Action action, MonoBehaviour actionTarget, bool forceEvenIfObjectIsInactive = false)
        {
            DelayedCall(0, action, actionTarget, forceEvenIfObjectIsInactive);
        }
    }
}
