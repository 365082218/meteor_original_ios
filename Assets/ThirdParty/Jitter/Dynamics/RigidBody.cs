﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Collision;
using Jitter.Dynamics.Constraints;
using Jitter.DataStructures;
#endregion

using GMath = Game.Utility.Math;

namespace Jitter.Dynamics
{


    public enum RigidBodyIndex
    {
        RigidBody1, RigidBody2
    }

    /// <summary>
    /// The RigidBody class.
    /// </summary>
    public class RigidBody : IBroadphaseEntity, IDebugDrawable, IEquatable<RigidBody>, IComparable<RigidBody>
    {
        [Flags]
        public enum DampingType { None = 0x00, Angular = 0x01, Linear = 0x02 }

        internal JMatrix inertia;
        internal JMatrix invInertia;

        internal JMatrix invInertiaWorld;
        internal JMatrix orientation;
        internal JMatrix invOrientation;
        internal JVector position;
        internal JVector linearVelocity;
        internal JVector angularVelocity;

        internal Material material;

        internal JBBox boundingBox;

        internal float inactiveTime = 0.0f;

        internal bool isActive = true;
        internal bool isStatic = false;
        internal bool affectedByGravity = true;

        internal CollisionIsland island;
        internal float inverseMass;

        internal JVector force, torque;

        private int hashCode;

        internal int internalIndex = 0;

        private ShapeUpdatedHandler updatedHandler;

        internal List<RigidBody> connections = new List<RigidBody>();

        internal HashSet<Arbiter> arbiters = new HashSet<Arbiter>();
        internal HashSet<Constraint> constraints = new HashSet<Constraint>();

        private ReadOnlyHashset<Arbiter> readOnlyArbiters;
        private ReadOnlyHashset<Constraint> readOnlyConstraints;

        internal int marker = 0;


        public RigidBody(Shape shape)
            : this(shape, new Material(), false)
        {
        }

        internal bool isParticle = false;

        /// <summary>
        /// If true, the body as no angular movement.
        /// </summary>
        public bool IsParticle { 
            get { return isParticle; }
            set
            {
                if (isParticle && !value)
                {
                    updatedHandler = new ShapeUpdatedHandler(ShapeUpdated);
                    this.Shape.ShapeUpdated += updatedHandler;
                    SetMassProperties();
                    isParticle = false;
                }
                else if (!isParticle && value)
                {
                    this.inertia = JMatrix.Zero;
                    this.invInertia = this.invInertiaWorld = JMatrix.Zero;
                    this.invOrientation = this.orientation = JMatrix.Identity;
                    inverseMass = 1.0f;

                    this.Shape.ShapeUpdated -= updatedHandler;

                    isParticle = true;
                }

                Update();
            }
        }

        /// <summary>
        /// Initializes a new instance of the RigidBody class.
        /// </summary>
        /// <param name="shape">The shape of the body.</param>
        public RigidBody(Shape shape, Material material)
            :this(shape,material,false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RigidBody class.
        /// </summary>
        /// <param name="shape">The shape of the body.</param>
        /// <param name="isParticle">If set to true the body doesn't rotate. 
        /// Also contacts are only solved for the linear motion part.</param>
        public RigidBody(Shape shape, Material material, bool isParticle)
        {
            readOnlyArbiters = new ReadOnlyHashset<Arbiter>(arbiters);
            readOnlyConstraints = new ReadOnlyHashset<Constraint>(constraints);

            instance = Interlocked.Increment(ref instanceCount);
            hashCode = CalculateHash(instance);

            this.Shape = shape;
            orientation = JMatrix.Identity;

            if (!isParticle)
            {
                updatedHandler = new ShapeUpdatedHandler(ShapeUpdated);
                this.Shape.ShapeUpdated += updatedHandler;
                SetMassProperties();
            }
            else
            {
                this.inertia = JMatrix.Zero;
                this.invInertia = this.invInertiaWorld = JMatrix.Zero;
                this.invOrientation = this.orientation = JMatrix.Identity;
                inverseMass = 1.0f;
            }

            this.material = material;

            AllowDeactivation = true;
            EnableSpeculativeContacts = false;

            this.isParticle = isParticle;

            Update();
        }

        /// <summary>
        /// Calculates a hashcode for this RigidBody.
        /// The hashcode should be unique as possible
        /// for every body.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        public ReadOnlyHashset<Arbiter> Arbiters { get { return readOnlyArbiters; } }
        public ReadOnlyHashset<Constraint> Constraints { get { return readOnlyConstraints; } }

        /// <summary>
        /// If set to false the body will never be deactived by the
        /// world.
        /// </summary>
        public bool AllowDeactivation { get; set; }

        public bool EnableSpeculativeContacts { get; set; }

        /// <summary>
        /// The axis aligned bounding box of the body.
        /// </summary>
        public JBBox BoundingBox { get { return boundingBox; } }


        private static int instanceCount = 0;
        private int instance;

        private int CalculateHash(int a)
        {
            a = (a ^ 61) ^ (a >> 16);
            a = a + (a << 3);
            a = a ^ (a >> 4);
            a = a * 0x27d4eb2d;
            a = a ^ (a >> 15);
            return a;
        }

        /// <summary>
        /// Gets the current collision island the body is in.
        /// </summary>
        public CollisionIsland CollisionIsland { get { return this.island; } }

        /// <summary>
        /// If set to false the velocity is set to zero,
        /// the body gets immediately freezed.
        /// </summary>
        public bool IsActive
        {
            get 
            {
                return isActive;
            }
            set
            {
                if (!isActive && value)
                {
                    // if inactive and should be active
                    inactiveTime = 0.0f;
                }
                else if (isActive && !value)
                {
                    // if active and should be inactive
                    inactiveTime = float.PositiveInfinity;
                    this.angularVelocity.MakeZero();
                    this.linearVelocity.MakeZero();
                }

                isActive = value;
            }
        }

        /// <summary>
        /// Applies an impulse on the center of the body. Changing
        /// linear velocity.
        /// </summary>
        /// <param name="impulse">Impulse direction and magnitude.</param>
        public void ApplyImpulse(JVector impulse)
        {
            if (this.isStatic)
                throw new InvalidOperationException("Can't apply an impulse to a static body.");

            JVector temp;
            JVector.Multiply(ref impulse, inverseMass, out temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);
        }

        /// <summary>
        /// Applies an impulse on the specific position. Changing linear
        /// and angular velocity.
        /// </summary>
        /// <param name="impulse">Impulse direction and magnitude.</param>
        /// <param name="relativePosition">The position where the impulse gets applied
        /// in Body coordinate frame.</param>
        public void ApplyImpulse(JVector impulse, JVector relativePosition)
        {
            if (this.isStatic)
                throw new InvalidOperationException("Can't apply an impulse to a static body.");

            JVector temp;
            JVector.Multiply(ref impulse, inverseMass, out temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);

            JVector.Cross(ref relativePosition, ref impulse, out temp);
            JVector.Transform(ref temp, ref invInertiaWorld, out temp);
            JVector.Add(ref angularVelocity, ref temp, out angularVelocity);
        }

        /// <summary>
        /// Adds a force to the center of the body. The force gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the force depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="force">The force to add next <see cref="World.Step"/>.</param>
        public void AddForce(JVector force)
        {
            JVector.Add(ref force, ref this.force, out this.force);
        }

        /// <summary>
        /// Adds a force to the center of the body. The force gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the force depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="force">The force to add next <see cref="World.Step"/>.</param>
        /// <param name="pos">The position where the force is applied.</param>
        public void AddForce(JVector force, JVector pos)
        {
            JVector.Add(ref this.force, ref force, out this.force);
            JVector.Subtract(ref pos, ref this.position, out pos);
            JVector.Cross(ref pos, ref force, out pos);
            JVector.Add(ref pos, ref this.torque, out this.torque);
        }

        /// <summary>
        /// Returns the torque which acts this timestep on the body.
        /// </summary>
        public JVector Torque { get { return torque; } }

        /// <summary>
        /// Returns the force which acts this timestep on the body.
        /// </summary>
        public JVector Force { get { return force; } set { force = value; } }

        /// <summary>
        /// Adds torque to the body. The torque gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the torque depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="torque">The torque to add next <see cref="World.Step"/>.</param>
        public void AddTorque(JVector torque)
        {
            JVector.Add(ref torque, ref this.torque, out this.torque);
        }

        protected bool useShapeMassProperties = true;

        /// <summary>
        /// By calling this method the shape inertia and mass is used.
        /// </summary>
        public void SetMassProperties()
        {
            this.inertia = Shape.inertia;
            JMatrix.Inverse(ref inertia, out invInertia);
            this.inverseMass = GMath.ToFixed(1.0f / Shape.mass);
            useShapeMassProperties = true;
        }

        /// <summary>
        /// The engine used the given values for inertia and mass and ignores
        /// the shape mass properties.
        /// </summary>
        /// <param name="inertia">The inertia/inverse inertia of the untransformed object.</param>
        /// <param name="mass">The mass/inverse mass of the object.</param>
        /// <param name="setAsInverseValues">Sets the InverseInertia and the InverseMass
        /// to this values.</param>
        public void SetMassProperties(JMatrix inertia, float mass, bool setAsInverseValues)
        {
            if (setAsInverseValues)
            {
                if (!isParticle)
                {
                    this.invInertia = inertia;
                    JMatrix.Inverse(ref inertia, out this.inertia);
                }
                this.inverseMass = mass;
            }
            else
            {
                if (!isParticle)
                {
                    this.inertia = inertia;
                    JMatrix.Inverse(ref inertia, out this.invInertia);
                }
                this.inverseMass = 1.0f / mass;
            }

            useShapeMassProperties = false;
            Update();
        }

        private void ShapeUpdated()
        {
            if (useShapeMassProperties) SetMassProperties();
            Update();
            UpdateHullData();
        }

        /// <summary>
        /// Allows to set a user defined value to the body.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// The shape the body is using.
        /// </summary>
        public Shape Shape 
        {
            get { return shape; } 
            set 
            {
                // deregister update event
                if(shape != null) shape.ShapeUpdated -= updatedHandler;

                // register new event
                shape = value; 
                shape.ShapeUpdated += new ShapeUpdatedHandler(ShapeUpdated); 
            } 
        }

        private Shape shape;

        #region Properties

        private DampingType damping = DampingType.Angular | DampingType.Linear;

        public DampingType Damping { get { return damping; } set { damping = value; } }

        public Material Material { get { return material; } set { material = value; } }

        /// <summary>
        /// The inertia currently used for this body.
        /// </summary>
        public JMatrix Inertia { get { return inertia; } }

        /// <summary>
        /// The inverse inertia currently used for this body.
        /// </summary>
        public JMatrix InverseInertia { get { return invInertia; } }

        /// <summary>
        /// The velocity of the body.
        /// </summary>
        public JVector LinearVelocity
        {
            get { return linearVelocity; }
            set 
            { 
                if (this.isStatic) 
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                linearVelocity = value;
            }
        }

        // TODO: check here is static!
        /// <summary>
        /// The angular velocity of the body.
        /// </summary>
        public JVector AngularVelocity
        {
            get { return angularVelocity; }
            set
            {
                if (this.isStatic)
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                angularVelocity = value;
            }
        }

        /// <summary>
        /// The current position of the body.
        /// </summary>
        public JVector Position
        {
            get { return position; }
            set { position = value ; Update(); }
        }

        /// <summary>
        /// The current oriention of the body.
        /// </summary>
        public JMatrix Orientation
        {
            get { return orientation; }
            set { orientation = value; Update(); }
        }

        /// <summary>
        /// If set to true the body can't be moved.
        /// </summary>
        public bool IsStatic
        {
            get
            {
                return isStatic;
            }
            set
            {
                if (value && !isStatic)
                {
                    if(island != null)
                    island.islandManager.MakeBodyStatic(this);

                    this.angularVelocity.MakeZero();
                    this.linearVelocity.MakeZero();
                }
                isStatic = value;
            }
        }

        public bool AffectedByGravity { get { return affectedByGravity; } set { affectedByGravity = value; } }

        /// <summary>
        /// The inverse inertia tensor in world space.
        /// </summary>
        public JMatrix InverseInertiaWorld
        {
            get
            {
                return invInertiaWorld;
            }
        }

        /// <summary>
        /// Setting the mass automatically scales the inertia.
        /// To set the mass indepedently from the mass use SetMassProperties.
        /// </summary>
        public float Mass
        {
            get { return 1.0f / inverseMass; }
            set 
            {
                if (value <= 0.0f) throw new ArgumentException("Mass can't be less or equal zero.");

                // scale inertia
                if (!isParticle)
                {
                    JMatrix.Multiply(ref Shape.inertia, value / Shape.mass, out inertia);
                    JMatrix.Inverse(ref inertia, out invInertia);
                }

                inverseMass = 1.0f / value;
            }
        }

        #endregion


        internal JVector sweptDirection = JVector.Zero;

        public void SweptExpandBoundingBox(float timestep)
        {
            sweptDirection = linearVelocity * timestep;

            if (sweptDirection.X < 0.0f)
            {
                boundingBox.Min.X += sweptDirection.X;
            }
            else
            {
                boundingBox.Max.X += sweptDirection.X;
            }

            if (sweptDirection.Y < 0.0f)
            {
                boundingBox.Min.Y += sweptDirection.Y;
            }
            else
            {
                boundingBox.Max.Y += sweptDirection.Y;
            }

            if (sweptDirection.Z < 0.0f)
            {
                boundingBox.Min.Z += sweptDirection.Z;
            }
            else
            {
                boundingBox.Max.Z += sweptDirection.Z;
            }
        }

        /// <summary>
        /// Recalculates the axis aligned bounding box and the inertia
        /// values in world space.
        /// </summary>
        public virtual void Update()
        {
            if (isParticle)
            {
                this.inertia = JMatrix.Zero;
                this.invInertia = this.invInertiaWorld = JMatrix.Zero;
                this.invOrientation = this.orientation = JMatrix.Identity;
                this.boundingBox = shape.boundingBox;
                JVector.Add(ref boundingBox.Min, ref this.position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref this.position, out boundingBox.Max);

                angularVelocity.MakeZero();
            }
            else
            {
                // Given: Orientation, Inertia
                JMatrix.Transpose(ref orientation, out invOrientation);
                this.Shape.GetBoundingBox(ref orientation, out boundingBox);
                JVector.Add(ref boundingBox.Min, ref this.position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref this.position, out boundingBox.Max);


                if (!isStatic)
                {
                    JMatrix.Multiply(ref invOrientation, ref invInertia, out invInertiaWorld);
                    JMatrix.Multiply(ref invInertiaWorld, ref orientation, out invInertiaWorld);
                }
            }
        }

        public bool Equals(RigidBody other)
        {
            return (other.instance == this.instance);
        }

        public int CompareTo(RigidBody other)
        {
            if (other.instance < this.instance) return -1;
            else if (other.instance > this.instance) return 1;
            else return 0;
        }

        public int BroadphaseTag { get; set; }


        public virtual void PreStep(float timestep)
        {
            //
        }

        public virtual void PostStep(float timestep)
        {
            //
        }


        public bool IsStaticOrInactive
        {
            get { return (!this.isActive || this.isStatic); }
        }

        private bool enableDebugDraw = false;
        public bool EnableDebugDraw
        {
            get { return enableDebugDraw; }
            set
            {
                enableDebugDraw = value;
                UpdateHullData();
            }
        }

        private List<JVector> hullPoints = new List<JVector>();

        private void UpdateHullData()
        {
            hullPoints.Clear();

            if(enableDebugDraw) shape.MakeHull(ref hullPoints, 3);
        }


        public void DebugDraw(IDebugDrawer drawer)
        {
            JVector pos1,pos2,pos3;

            for(int i = 0;i<hullPoints.Count;i+=3)
            {
                pos1 = hullPoints[i + 0];
                pos2 = hullPoints[i + 1];
                pos3 = hullPoints[i + 2];

                JVector.Transform(ref pos1, ref orientation, out pos1);
                JVector.Add(ref pos1, ref position, out pos1);

                JVector.Transform(ref pos2, ref orientation, out pos2);
                JVector.Add(ref pos2, ref position, out pos2);

                JVector.Transform(ref pos3, ref orientation, out pos3);
                JVector.Add(ref pos3, ref position, out pos3);

                drawer.DrawTriangle(pos1, pos2, pos3);
            }
        }
    }
}
