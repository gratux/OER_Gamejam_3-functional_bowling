using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveprototype : MonoBehaviour
{
    public float moveForce = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Получаем ввод от клавиш
        float moveX = Input.GetAxis("Horizontal"); // A / D
        float moveZ = Input.GetAxis("Vertical");   // W / S

        // Направление силы
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ);

        // Применяем силу, чтобы катить мяч
        rb.AddForce(moveDirection * moveForce);
    }
}