using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private CorkGun corkGun;
    private float moveSpeed = 10;
    private float jumpSpeed = 5;
    private bool canJump = true;
    private WaitForSeconds jumpCooldown = new WaitForSeconds(1);

    private float corkKnockbackSpeed = 8;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        corkGun = GetComponentInChildren<CorkGun>();
        corkGun.OnShoot += CorkKnockback;
    }
    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float jump = Input.GetAxis("Jump");
        
        // horizontal movement
        rb.MovePosition(rb.position + Vector3.right * (horizontal * moveSpeed * Time.deltaTime));
        
        // z-axis movement
        rb.MovePosition(rb.position + Vector3.forward * (vertical * moveSpeed * Time.deltaTime));

        // jumping
        if (canJump && jump > 0)
        {
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
            canJump = false;
            StartCoroutine(JumpCooldown());
        }
    }

    private void CorkKnockback(object sender, CorkGun.OnShootEventArgs e)
    {
        rb.AddForce(-e.shootDirection * corkKnockbackSpeed, ForceMode.VelocityChange);
    }

    private IEnumerator JumpCooldown()
    {
        yield return jumpCooldown;
        canJump = true;
    }
}
