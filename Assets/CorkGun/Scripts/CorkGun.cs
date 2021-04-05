using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEditor;
using UnityEngine;


// TODO: make it so rope hangs on back part of gun instead of at muzzle

// if there are any problems with the cork not being able to reach muzzle, try reducing the rigidbody mass of cork

public class CorkGun : MonoBehaviour
{
    [SerializeField] private Transform muzzle; // where the cork will shoot from
    [SerializeField] private Rigidbody cork;
    [SerializeField] private Rope rope;
    private Camera mainCamera;

    private float retractionLimit = 1.5f; // defines the minimum distance between cork and muzzle when retraction switches from acceleration to linear speed
    private float retractionSpeed = 5; // use this when cork is close to muzzle (within retraction limit)
    private float retractionAcceleration = 10; // use this when cork is far from muzzle
    private float retractionSegmentLengthThreshold = 0.1f; // defines the minimum segmentLength of rope before retraction begins

    private float shootSpeed = 20; // initial speed of cork once shot
    private bool canShoot = true;
    
    // TODO: make an event that triggers whenever cork is shot
    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs { public Vector3 shootDirection; }

    private void Start()
    {
        mainCamera = Camera.main;
        cork.transform.SetParent(null);
        cork.useGravity = false;
    }

    private void Update()
    {
        if (canShoot)
        {
            cork.MovePosition(muzzle.position);
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = -mainCamera.transform.position.z;
                Vector3 target = mainCamera.ScreenToWorldPoint(mousePosition);
            
                Shoot(target);
            }
        }
        
        else if (!canShoot)
        {
            cork.useGravity = true;
            if (Input.GetMouseButton(1))
            {
                Retract();
            }
            else
            {
                rope.ResetSegmentLength();
            }
        }

        rope.ropeStart = muzzle.position;
        rope.ropeEnd = cork.position;
    }

    private void Shoot(Vector3 target)
    {
        if (canShoot)
        {
            cork.useGravity = true;
            rope.ResetSegmentLength();

            cork.MovePosition(muzzle.position);
            Vector3 shootDirection = (target - muzzle.position).normalized;
            cork.AddForce(shootDirection * shootSpeed, ForceMode.VelocityChange);

            canShoot = false;
            
            OnShoot?.Invoke(this, new OnShootEventArgs
            {
                shootDirection = shootDirection
            });
        }
    }

    private void Retract()
    {
        if (!canShoot)
        {
            rope.segmentLength *= 0.96f; // make rope tauter (make this closer to 1 to increase rate at which rope tightens)

            // cork only starts coming back towards muzzle if rope is taut enough (segment length is low enough)
            if (rope.segmentLength < retractionSegmentLengthThreshold)
            {
                Vector3 corkToMuzzle = muzzle.position - cork.position;
                float corkToMuzzleDistance = corkToMuzzle.magnitude;
                if (corkToMuzzleDistance < retractionLimit) // cork is within certain distance of muzzle
                {
                    cork.useGravity = false;
                    cork.velocity *= 0.9f;
                    // move cork towards muzzle instead of applying a force
                    cork.MovePosition(Vector3.MoveTowards(cork.position, muzzle.position, retractionSpeed * Time.deltaTime));
                
                }
                else
                {
                    corkToMuzzle /= corkToMuzzleDistance; // normalize
                    cork.AddForce(corkToMuzzle * retractionAcceleration, ForceMode.Acceleration);
                }

                // check if cork is fully retracted
                if (cork.position == muzzle.position)
                {
                    cork.useGravity = false;
                    canShoot = true;

                    rope.ResetSegmentLength();
                }
            }
            
        }
    }
}
