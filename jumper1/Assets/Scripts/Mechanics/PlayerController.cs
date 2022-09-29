using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public Transform hammerHead;
        public Transform body2;
        public float forcePower = 10f;
        public float maxRange = 0.9f;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();

            

            base.Update();
          
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            //hammerTime();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            //if (move.x > 0.01f)
            //    spriteRenderer.flipX = false;
            //else if (move.x < -0.01f)
            //    spriteRenderer.flipX = true;

            //animator.SetBool("grounded", IsGrounded);
            //animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public void hammerTime()
        {
            // Screen center and mouse position in screen space
            float depth = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 center =
                new Vector3(Screen.width / 2, Screen.height / 2, depth);
            Vector3 mouse =
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);

            // Transform to world space
            center = Camera.main.ScreenToWorldPoint(center);
            mouse = Camera.main.ScreenToWorldPoint(mouse);

            // Compute mouseVec for hammer control
            Vector3 mouseVec = Vector3.ClampMagnitude(mouse - center, maxRange);

            // hammerHead.GetComponent<Rigidbody2D>().MovePosition(body.position +
            // mouseVec); return;
            //bool mousePos = mouse.y > Screen.height / 2.0f;

            // Check if hammer head is collided with scene objects
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.useLayerMask = true;
            contactFilter.layerMask = LayerMask.GetMask("Default");
            Collider2D[] results = new Collider2D[5];
            if (hammerHead.GetComponent<Rigidbody2D>().OverlapCollider(
                    contactFilter, results) > 0 )  // If collided with scene objects

            {
                //Debug.Log("collieded");
                // Update body pos
                Vector3 targetBodyPos = hammerHead.position - mouseVec;
               
                Vector3 force = Vector3.Normalize(targetBodyPos - body2.position) * forcePower;
   
                //if (!(angle > -40 && angle < 40) && !(angle > 100 && angle < 140) ) { body.GetComponent<Rigidbody2D>().AddForce(force); }
                body.GetComponent<Rigidbody2D>().AddForce(force);
                //trying this


                //body.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude(
                //    body.GetComponent<Rigidbody2D>().velocity, 6);


            }

            // Compute new hammer pos
            Vector3 newHammerPos = body2.position + mouseVec;
            Vector3 hammerMoveVec = newHammerPos - hammerHead.position;
            newHammerPos = hammerHead.position + hammerMoveVec * 0.2f;

            // Update hammer pos
            hammerHead.GetComponent<Rigidbody2D>().MovePosition(newHammerPos);

            // Update hammer rotation
            hammerHead.rotation = Quaternion.FromToRotation(
                Vector3.right, newHammerPos - body2.position);
        }
    

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}