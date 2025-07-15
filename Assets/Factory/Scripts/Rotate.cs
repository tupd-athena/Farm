using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Factory
{
    public class Rotate : MonoBehaviour
    {
        public Vector3 rotation;


        void Update()
        {
            if (rotation != Vector3.zero)
            {
                transform.Rotate(rotation * Time.deltaTime);
            }
        }
    }
}
