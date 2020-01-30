using System.Collections;
using TMPro;
using UnityEngine;

namespace DetentionRoom.Scripts
{
    public class ApplyDamageIndicator : MonoBehaviour
    {
        public TextMeshProUGUI[] textArray;

        public void Display(string message)
        {
            foreach (var text in textArray)
            {
                text.text = message;
            }

            StartCoroutine(RotateDamageDisplay());
        }

        private IEnumerator RotateDamageDisplay()
        {
            for (var i = 0; i < 360; i++)
            {
                transform.localEulerAngles += new Vector3(0,1,0);
                yield return new WaitForSeconds(0.0055f);
            }
            
            Destroy(gameObject);
        }
    }
}