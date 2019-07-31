using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace YGOTzolkin.Utility
{
    public static class TransformExtension
    {
        public static IEnumerator MoveAndRotateTo(this Transform transform, Vector3 tarPos, Quaternion tarRot, float duration)
        {
            var startRot = transform.rotation;
            var startPos = transform.position;
            var elapsed = 0.0f;
            while (elapsed <= duration)
            {
                elapsed += Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(startPos, tarPos, elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRot, tarRot, elapsed / duration);
                yield return null;
            }
        }

        public static IEnumerator MoveTo(this Transform transform, Vector3 tarPos, float duration)
        {
            var startPos = transform.position;
            var elapsed = 0.0f;
            while (elapsed <= duration)
            {
                elapsed += Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(startPos, tarPos, elapsed / duration);
                yield return null;
            }
        }

        public static IEnumerator RotateTo(this Transform transform, Quaternion tarRot, float duration)
        {
            var startRot = transform.rotation;
            var elapsed = 0f;
            var segment = duration * 0.3f;
            var p1Rot = Quaternion.Slerp(startRot, tarRot, 0.85f);
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.rotation = Quaternion.Slerp(startRot, p1Rot, Mathf.Pow(elapsed / segment, 3));
                yield return null;
            }
            elapsed = 0;
            segment = duration - segment;
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.rotation = Quaternion.Slerp(p1Rot, tarRot, Mathf.Pow(elapsed / segment, 0.3f));
                yield return null;
            }
        }

        public static IEnumerator SmoothDampTo(this Transform transform, Vector3 tarPos, Quaternion tarRot, float duration)
        {
            var startRot = transform.rotation;
            var startPos = transform.localPosition;
            var elapsed = 0f;
            var segment = duration * 0.3f;
            var p1pos = Vector3.Lerp(startPos, tarPos, 0.85f);
            var p1Rot = Quaternion.Slerp(startRot, tarRot, 0.85f);
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.localPosition = Vector3.Lerp(startPos, p1pos, Mathf.Pow(elapsed / segment, 2));
                transform.rotation = Quaternion.Slerp(startRot, p1Rot, Mathf.Pow(elapsed / segment, 2));
                yield return null;
            }
            elapsed = 0;
            segment = duration - segment;
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.localPosition = Vector3.Lerp(p1pos, tarPos, Mathf.Pow(elapsed / segment, 0.25f));
                transform.rotation = Quaternion.Slerp(p1Rot, tarRot, Mathf.Pow(elapsed / segment, 0.25f));
                yield return null;
            }
        }

        public static IEnumerator SmoothDampTo(this RectTransform transform, Vector3 tarPos, float duration)
        {
            var startPos = transform.anchoredPosition3D;
            var elapsed = 0f;
            var segment = duration * 0.3f;
            var p1pos = Vector3.Lerp(startPos, tarPos, 0.85f);
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.anchoredPosition3D = Vector3.Lerp(startPos, p1pos, Mathf.Pow(elapsed / segment, 2));
                yield return null;
            }
            elapsed = 0;
            segment = duration - segment;
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.anchoredPosition3D = Vector3.Lerp(p1pos, tarPos, Mathf.Pow(elapsed / segment, 0.25f));
                yield return null;
            }
        }

        public static IEnumerator FreeSmoothDampTo(this Transform transform, Vector3 tarPos, Quaternion tarRot)
        {
            float duration = Vector3.Distance(transform.localPosition, tarPos) / 200;
            var startRot = transform.rotation;
            var startPos = transform.localPosition;
            var elapsed = 0f;
            var segment = duration * 0.6f;
            var p1pos = Vector3.Lerp(startPos, tarPos, 0.85f);
            var p1Rot = Quaternion.Slerp(startRot, tarRot, 0.85f);
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.localPosition = Vector3.Lerp(startPos, p1pos, Mathf.Pow(elapsed / segment, 2));
                transform.rotation = Quaternion.Slerp(startRot, p1Rot, Mathf.Pow(elapsed / segment, 2));
                yield return null;
            }
            elapsed = 0;
            segment = duration * 0.4f;
            while (elapsed <= segment)
            {
                elapsed += Time.fixedDeltaTime;
                transform.localPosition = Vector3.Lerp(p1pos, tarPos, Mathf.Pow(elapsed / segment, 0.25f));
                transform.rotation = Quaternion.Slerp(p1Rot, tarRot, Mathf.Pow(elapsed / segment, 0.25f));
                yield return null;
            }
        }

        public static void SetButtonName(this Button button, string name)
        {
            button.transform.GetChild(0).GetComponent<Text>().text = name;
        }

        public static void SetToggleName(this Toggle toggle, string name)
        {
            toggle.transform.GetChild(1).GetComponent<Text>().text = name;
        }
    }
}
