using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// rope does NOT affect physics of player or cork or cork gun; rope is purely visual
// to manupulate physics of cork, experiment with values from cork gun configurable joint and cork's rigidbody

// To reduce the "bounciness" of rope upon collision, try increasing magnitude of gravityAcceleration

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [SerializeField] private LayerMask collidable;
    public Vector3 ropeStart { get; set; }
    public Vector3 ropeEnd { get; set; }
    public float segmentLength { get; set; } // ideal distance between individual joints on rope
    
    private readonly float maxSegmentLength = 0.25f;
    private readonly int jointCount = 35; // number of joints on the rope (higher => smoother rope)
    private readonly int constraintIterations = 50; // how many iterations of the constraint function to run each frame (higher => smoother/more accurate simulation)
    private readonly Vector3 gravityAcceleration = new Vector3(0, -9.81f, 0); // applied every simulation step (should include gravity)

    private LineRenderer lineRenderer;
    private List<RopeJoint> joints = new List<RopeJoint>();

    // instantly change segment length
    public void ResetSegmentLength()
    {
        segmentLength = maxSegmentLength;
    }

    private void Start()
    {
        segmentLength = maxSegmentLength;
        lineRenderer = GetComponent<LineRenderer>();
        Vector3 jointPosition = Vector3.zero;

        for (int i = 0; i < jointCount; i++)
        {
            joints.Add(new RopeJoint(jointPosition));
            jointPosition.y -= segmentLength;
        }
    }

    private void Update()
    {
        Draw();
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void Simulate()
    {
        // SIMULATION
        for (int i = 0; i < jointCount; i++)
        {
            RopeJoint joint = joints[i];
            Vector3 positionChange = joint.currentPosition - joint.previousPosition;

            Vector3 extraPreviousPosition = joint.previousPosition;
            joint.previousPosition = joint.currentPosition;
            joint.currentPosition += positionChange + gravityAcceleration * (Time.deltaTime * Time.deltaTime);
            
            // collision detection
            if (Physics.CheckSphere(joint.currentPosition, 0.1f, collidable))
            {
                joint.currentPosition = extraPreviousPosition;
            }
        }
        
        // CONSTRAINTS
        for (int i = 0; i < constraintIterations; i++)
        {
            ApplyConstraints();
        }
    }

    private void ApplyConstraints()
    {
        // set first joint to be at position of ropeStart
        joints[0].currentPosition = ropeStart;

        // set last joint to be at position of ropeEnd
        joints[jointCount - 1].currentPosition = ropeEnd;
        
        // neighboring points on rope keep a fixed distance apart from each other (this is where the magic happens)
        for (int i = 0; i < jointCount - 1; i++)
        {
            RopeJoint firstJoint = joints[i];
            RopeJoint secondJoint = joints[i + 1];
            Vector3 firstToSecond = secondJoint.currentPosition - firstJoint.currentPosition;
            float distance = firstToSecond.magnitude; // current segment length
            float error = distance - segmentLength; // difference between current and ideal segment length
            firstToSecond /= distance; // normalize firstToSecond
            Vector3 correction = firstToSecond * (error * 0.5f);

            if (i != 0)
            {
                firstJoint.currentPosition += correction;
            }
            else
            {
                secondJoint.currentPosition -= correction;
            }

            if (i + 1 != jointCount - 1)
            {
                secondJoint.currentPosition -= correction;
            }
            else
            {
                firstJoint.currentPosition += correction;
            }
            
            
        }
    }

    private void Draw()
    {
        Vector3[] segmentPositions = new Vector3[jointCount];
        for (int i = 0; i < jointCount; i++)
        {
            segmentPositions[i] = joints[i].currentPosition;
        }
        
        lineRenderer.positionCount = jointCount;
        lineRenderer.SetPositions(segmentPositions);
    }

    private class RopeJoint
    {
        public Vector3 currentPosition;
        public Vector3 previousPosition;

        public RopeJoint(Vector3 position)
        {
            currentPosition = position;
            previousPosition = position;
        }
    }
}
