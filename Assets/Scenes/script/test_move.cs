using UnityEngine;

public class test_move : MonoBehaviour
{
    public GameObject car;

    void Start()
    {
        
    }

    void Update()
    {

        if(Input.GetKey(KeyCode.W))
        {
            car.transform.Translate(Vector3.forward * Time.deltaTime * 2);
        }
         if(Input.GetKey(KeyCode.S))
        {
            car.transform.Translate(Vector3.back * Time.deltaTime * 2);
        }
         if(Input.GetKey(KeyCode.A))
        {
            car.transform.Translate(Vector3.left * Time.deltaTime * 2);
        }
         if(Input.GetKey(KeyCode.D))
        {
            car.transform.Translate(Vector3.right * Time.deltaTime * 2);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            car.transform.rotation *= Quaternion.Euler(0, -20 * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            car.transform.rotation *= Quaternion.Euler(0, 20 * Time.deltaTime, 0);
        }
    }
}
