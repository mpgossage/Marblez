using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_FLASH
#else
using System.Reflection;
#endif

/// <summary>
/// Provides base functionality for all Orthello display classes.
/// </summary>
[ExecuteInEditMode]
public class OTObject : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Pivot point location.
    /// </summary>
    public enum Pivot
    {
        
        TopLeft,
        
        Top,
        
        TopRight,
        
        Left,
        
        Center,
        
        Right,
        
        BottomLeft,
        
        Bottom,
        
        BottomRight,
        
        Custom
    };

    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Physical type
    /// </summary>
    public enum Physics
    {
        /// <summary>
        /// For input capture or custom collision detection
        /// </summary>
        Trigger,
        /// <summary>
        /// Physical floating object
        /// </summary>
        NoGravity,
        /// <summary>
        /// Physical rigid body
        /// </summary>
        RigidBody,
        /// <summary>
        /// Physical static body that collides with all 
        /// </summary>
        StaticBody,
        /// <summary>
        /// Physical static body that has a collison size of 10
        /// </summary>
        StaticRigidBody,
        /// <summary>
        /// custom adding/removing/configration of colliders and physical components.
        /// </summary>
        Custom
    };

    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Type of collider to use
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// Use a Box collider object
        /// </summary>
        Box,
        /// <summary>
        ///  use a Spherical collider object
        /// </summary>
        Sphere
    };		
	
    //-----------------------------------------------------------------------------
    // Editor settings
    //-----------------------------------------------------------------------------
    
    public string _name = "";
    
    public Vector2 _position = Vector2.zero;
    
    public Vector2 _size = new Vector2(100, 100);
    
    public float _rotation = 0;
    
    public Pivot _pivot = Pivot.Center;
    
    public Vector2 _pivotPoint = Vector2.zero;
		    
    public bool _registerInput = false;
    
    public int _depth = 0;
    
    public bool _collidable = false;
    
    public Physics _physics = Physics.Trigger;
    
    public ColliderType _colliderType = ColliderType.Box;
    
    public int _collisionDepth = 0;
    
    public float _collisionSize = -1;

    /// <summary>
    /// Makes this OTObject draggable
    /// </summary>
    /// <remarks>
    /// When an OTObject is draggable it will fire the <see cref="onDragStart" /> and <see cref="onDragEnd" /> event. The receiving OTObject (sprite) must have <see cref="registerInput" /> set to true and
    /// will fire the <see cref="onReceiveDrop" /> event. 
    /// </remarks>
    public bool draggable = false;

    /// <summary>
    /// Mousebutton that will activate dragging the sprite.
    /// </summary>
    [HideInInspector]
    public int dragButton = 0;
    
    [HideInInspector]
    public bool init = true;	
	
    
    public Rect _worldBounds = new Rect(0, 0, 0, 0);

    /// <summary>
    /// World boundary for this object
    /// </summary>
    /// <remarks>
    /// If world boundary is set this will restrict this object
    /// from leaving this boundary box. Setting this to Rect(0,0,0,0) 
    /// will impose no  boundary restriction.
    /// This is specified in pixel (world space) coordinates.
    /// </remarks>
    public Rect worldBounds
    {
        get
        {
            return _worldBounds;
        }
        set
        {
            _worldBounds = value;
			position += new Vector2(0,0.00001f);
        }
    }
	
	
	public Transform otTransform
	{
		get;
		set;
	}
	public Renderer  otRenderer
	{
		get;
		set;
	}
	public Collider  otCollider
	{
		get;
		set;		
	}
	
    /// <summary>
    /// Check and handle setting changes while playing.
    /// </summary>   
    /// <remarks>
    /// When the system checks/validates settings, it will for example
    /// add a collider and a rigidbody when you set collidable to true.
    /// Normally you wont need this functionality while the application
    /// is playing but you can set this to true when you are building
    /// up sprites from scratch. This will cost about 10fps (very rough estimate).
    /// </remarks>
    public bool dirtyChecks
    {
        get
        {
            return _dirtyChecks;
        }
        set
        {
            _dirtyChecks = value;
        }
    }
		
	Touch _dragTouch;
	/// <summary>
	/// Gets a value indicating whether this <see cref="OTObject"/> is dragging.
	/// </summary>
	/// <value>
	/// <c>true</c> if is dragging; otherwise, <c>false</c>.
	/// </value>
	public Touch dragTouch
	{
		get
		{
			return _dragTouch;
		}
		set
		{
			_dragTouch = value;
		}
	}
	
	/// <summary>
	/// data object for this sprite
	/// </summary>
	public object data;

	/// <summary>
	/// Gets or sets the depth at which this object will be dragged
	/// </summary>
	/// <value>
	/// The drag depth.
	/// </value>	
	public int dragDepth
	{
		get
		{
			return _dragDepth;
		}
		set
		{
			_dragDepth = value;
		}
	}
	
	/// <summary>
	/// Gets the y (up) vector for this sprite
	/// </summary>
	public Vector2 yVector
	{
		get
		{
			Vector2 _upVector = otTransform.up;
			if (OT.world == OT.World.WorldTopDown2D)
				_upVector = new Vector2(otTransform.up.x,otTransform.up.z);
			return _upVector;
		}
	}
	
	/// <summary>
	/// Gets the x (right) vector for this sprite
	/// </summary>
	public Vector2 xVector
	{
		get
		{
			Vector2 _upVector = otTransform.right;
			if (OT.world == OT.World.WorldTopDown2D)
				_upVector = new Vector2(otTransform.right.x,otTransform.right.z);
			return _upVector;
		}
	}
	
	/// <summary>
	/// Gets or sets the alpha value with which this object will be dragged
	/// </summary>
	/// <value>
	/// The drag alpha value
	/// </value>	
	public float dragAlpha
	{
		get
		{
			return _dragAlpha;
		}
		set
		{
			_dragAlpha = value;
		}
	}
		
    //-----------------------------------------------------------------------------
    // Fields
    //-----------------------------------------------------------------------------
    
    [HideInInspector]
    public string protoType = "";
    
    [HideInInspector]
    public string baseName = "";
    
    //-----------------------------------------------------------------------------
    // Delegates
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Is used to delegate a message from a specific owner object.
    /// </summary>
    public delegate void ObjectDelegate(OTObject owner);

	/// <summary>
	/// Called when this object is changed
	/// </summary>
	public ObjectDelegate onObjectChanged = null;
	
	/// <summary>
	/// Called when this object is created
	/// </summary>
	public ObjectDelegate onCreateObject = null;
	/// <summary>
	/// Called when this object is destroyed
	/// </summary>
	public ObjectDelegate onDestroyObject = null;			
    /// <summary>
    /// Input control delegate
    /// </summary>
    /// <remarks>
    ///  The onInput delegate will be called if this object 'registerInput' setting is set to true and if the 
    ///  user interacts with this object. Use this to capture user-input (clicks/touches).
    /// </remarks>
    /// <example> 
    /// The <strong>C# sample</strong> shows how the onInput delegate can be used.
    /// <code>
    ///  void Start()
    ///  {
    ///     // get our sprite
    ///     OTObject mySprite = OT.ObjectByName("mySprite");
    ///     // assign onInput delegate
    ///     if (mySprite!=null) mySprite.onInput = OnInput;
    ///  }
    ///  
    ///  void OnInput(OTObject owner)
    ///  {
    ///     // check for the left mouse button
    ///     if (Input.GetMouseDown(0))
    ///         Debug.Log("Left Button clicked On "+owner.name);
    ///  }
    /// </code>
    /// <i style="font-size:90%">Double-click on example code to select all code.</i>
    /// <br></br><br></br><br></br>
    ///  The <strong>Javascript sample</strong> shows how a sprite can be instructed to give an 
    ///  OnInput(OTObject owner) callback.
    ///  <code>
    ///  function Start():void
    ///  {
    ///     // get our sprite
    ///     var mySprite:OTObject = OT.ObjectByName("mySprite");
    ///     // instruct our sprite to give callbacks
    ///     if (mySprite!=null) mySprite.InitCallBacks(this);
    ///  }
    ///  
    ///  // All sprites that are instructed to give callbacks using {sprite}.InitCallBacks(this);
    ///  // will call OnInput automaticly when user input is captured.
    ///  function OnInput(owner:OTObject):void
    ///  {
    ///     // check for the left mouse button
    ///     if (Input.GetMouseDown(0))
    ///         Debug.Log("Left Button clicked On "+owner.name);
    ///  }
    ///  </code>
    /// <i style="font-size:90%">Double-click on example code to select all code.</i>
    /// </example>   
    public ObjectDelegate onInput = null;
    /// <summary>
    /// Is called when an object moves out of view.
    /// </summary>
    public ObjectDelegate onOutOfView = null;
    /// <summary>
    /// Is called when an object moves into view.
    /// </summary>
    public ObjectDelegate onIntoView = null;
    /// <summary>
    /// Is called when user starts dragging this object.
    /// </summary>
    /// <remarks>
    /// To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// </remarks>
    public ObjectDelegate onDragStart = null;
    /// <summary>
    /// Is called when user drops this object after dragging it.
    /// </summary>
    /// <remarks>
    /// To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// <see cref="dropTarget" /> will contain the sprite where this one was dropped upon.
    /// </remarks>
    public ObjectDelegate onDragging = null;
    /// <summary>
    /// Is called when user is dragging an object
    /// </summary>
    /// <remarks>
    /// To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// <see cref="dropTarget" /> will contain the sprite where this one was dropped upon.
    /// </remarks>
    public ObjectDelegate onDragEnd = null;
    /// <summary>
    /// Is called when another object is drag'n dropped on this object
    /// </summary>
    /// <remarks>
    /// This receiving object must have <see cref="registerInput" /> set to true. To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// <see cref="dropTarget" /> will contain the sprite that was dropped upon this one.
    /// </remarks>
    public ObjectDelegate onReceiveDrop = null;
    /// <summary>
    /// Orthello mouse movement handler
    /// </summary>
    public ObjectDelegate onMouseMoveOT = null;
    /// <summary>
    /// Orthello mouse enter handler
    /// </summary>
    public ObjectDelegate onMouseEnterOT = null;
    /// <summary>
    /// Orthello mouse exit handler
    /// </summary>
    public ObjectDelegate onMouseExitOT = null;
    /// <summary>
    /// Delegate to check collisions
    /// </summary>
    /// <remarks>
    ///  The onCollision delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject collides with another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onCollision = null;
    /// <summary>
    /// Collision 'enter' delegate
    /// </summary>
    /// <remarks>
    ///  The onEnter delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject enters (starts to overlap) another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onEnter = null;
    /// <summary>
    /// Collision 'exit' delegate
    /// </summary>
    /// <remarks>
    ///  The onExit delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject exits (stops to overlap) another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onExit = null;
    /// <summary>
    /// Collision 'stay' delegate
    /// </summary>
    /// <remarks>
    ///  The onStay delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject is overlapping another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onStay = null;
	
    //-----------------------------------------------------------------------------
    // public attributes (get/set)
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Name of this object
    /// </summary>
    /// <remarks>
    /// Changing this name will reflect immediately on the name of the associated Unity GameObject and
    /// vice versa.
    /// </remarks>
    new public string name
    {
        get
        {
            return _name;
        }
        set
        {
            string old = _name;
            _name = value;
            gameObject.name = _name;
            if (OT.isValid)
            {
                _name_ = _name;
                OT.RegisterLookup(this, old);
            }
        }
    }

	/// <summary>
	/// Gets or sets the layer id of this object ( and its children )
	/// </summary>
	public int layer
	{
		get
		{
			return gameObject.layer;
		}
		set
		{
			gameObject.layer = value;
			OTHelper.ChildrenSetLayer(gameObject,value);
		}
	}
			
	protected void Changed()
	{
		if (onObjectChanged!=null)
			onObjectChanged(this);
	}
		
	protected bool passiveControl = false;	
	bool _setPassive;
	void SetPassive()
	{
		_passive = _setPassive;
		enabled = !passive;	
		if (controllers.Count>0 || passiveControl)
		{
			if (enabled)
				OT.NotControlling(this);
			else
				OT.Controlling(this);
		}
		Update();
	}
	
	bool _passive = false;
	public bool passive
	{
		get
		{
			return _passive;
		}
		set
		{
			_setPassive = value;
			Invoke("SetPassive",0.05f);			
		}
	}
		
    /// <summary>
    /// Display depth of this object. -1000 (=front) to 1000 (=back)
    /// </summary>
    /// <remarks>
    /// The depth is a integer value from -1000 to 1000. Lower values will be layered on top of higher values, so by 
    /// increasing the depth you will send your object further back.
    /// </remarks>
    public int depth
    {
        get
        {
            return _depth;
        }
        set
        {
            _depth = value;
			if (OT.world != OT.World.WorldTopDown2D)
            	otTransform.localPosition = new Vector3(otTransform.localPosition.x, otTransform.localPosition.y, paintingDepth);
			else			
            	otTransform.localPosition = new Vector3(otTransform.localPosition.x, paintingDepth, otTransform.localPosition.z);
            SetCollider();
			Changed();
        }
    }

    /// <summary>
    /// Depth of collision layer
    /// </summary>
    /// <remarks>
    /// 'Collidable' objects will collide only with other 'collidable' object with the same
    /// collision depth.
    /// </remarks>
    public int collisionDepth
    {
        get
        {
            return _collisionDepth;
        }
        set
        {
            _collisionDepth = value;
            SetCollider();
        }
    }

    /// <summary>
    /// Depth (z) size of physical collider
    /// </summary>
    /// <remarks>
    /// If you are using Physics you can tweek the depth/z value of the collider
    /// using this setting. By setting this to high values, objects on other
    /// collision layers, where the collidersizes overlap can cause collisions.
    /// </remarks>
    public float collisionSize
    {
        get
        {
            return _collisionSize;
        }
        set
        {
            _collisionSize = value;
            SetCollider();
        }
    }

    /// <summary>
    /// Pivot location
    /// </summary>
    public Pivot pivot
    {
        get
        {
            return _pivot;
        }
        set
        {
            _pivot = value;
            switch (_pivot)
            {
                case Pivot.TopLeft: pivotPoint = new Vector2(-.5f, .5f); break;
                case Pivot.Top: pivotPoint = new Vector2(0, .5f); break;
                case Pivot.TopRight: pivotPoint = new Vector2(.5f, .5f); break;
                case Pivot.Left: pivotPoint = new Vector2(-.5f, 0); break;
                case Pivot.Center: pivotPoint = new Vector2(0, 0); break;
                case Pivot.Right: pivotPoint = new Vector2(.5f, 0); break;
                case Pivot.BottomLeft: pivotPoint = new Vector2(-.5f, -.5f); break;
                case Pivot.Bottom: pivotPoint = new Vector2(0, -.5f); break;
                case Pivot.BottomRight: pivotPoint = new Vector2(.5f, -.5f); break;
            }
        }
    }
    /// <summary>
    /// Custom pivot location
    /// </summary>
    /// <remarks>
    /// This Vector2 (x/y) 0/0 marks the pivot relative to the center of the object.<br></br><br></br>
    /// <strong>IMPORTANT</strong> : a sprite is always created with a size (x/y) of 1/1.
    /// </remarks>
    public Vector2 pivotPoint
    {
        get
        {
            return _pivotPoint;
        }
        set
        {
            if (!Vector2.Equals(value, _pivotPoint))
            {
                Vector2 nOff = Vector2.Scale(_pivotPoint, size);
                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation)), Vector3.one);
                    nOff = m.MultiplyPoint3x4(nOff);
                }
                position -= nOff;
                _pivotPoint = value;
                nOff = Vector2.Scale(_pivotPoint, size);
                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation)), Vector3.one);
                    nOff = m.MultiplyPoint3x4(nOff);
                }

                offset = _baseOffset;

                position += nOff;
                meshDirty = true;				
            }
        }
    }

    /// <summary>
    /// Indicates if this object is out of view
    /// </summary>
    public bool outOfView
    {
        get
        {
            //return (!inView);
            return !OT.view.Contains(this);
        }
    }
	
	Rect GetRect(bool local)
	{		
		if (otRenderer==null)
			InitComponents();
		
		Vector3 cb = otRenderer.bounds.center;
		Vector3 ce = otRenderer.bounds.extents;
		
		if (local && otTransform.parent!=null)
		{
			Vector3 ex = otTransform.parent.worldToLocalMatrix.MultiplyPoint3x4(cb + ce);
			cb = otTransform.parent.worldToLocalMatrix.MultiplyPoint3x4(cb);
			ce = ex - cb;
		}
					
        Rect r = new Rect(
            cb.x - ce.x,
            cb.y - ce.y,
            (ce.x*2),
            ce.y*2);
		
        return r;
	}
	
    /// <summary>
    /// This object's rectangle in world space
    /// </summary>
    public Rect worldRect
    {
        get
        {
			return GetRect(false);
        }
    }
	

    /// <summary>
    /// This object's rectangle in parents local space
    /// </summary>
    public Rect rect
    {
        get
        {
			return GetRect(true);
        }
    }

    /// <summary>
    /// Monitor user input yes/no
    /// </summary>
    /// <remarks>
    /// When registerInput is set to true, this OTObject is monitored for user input. When this object
    /// receives some user input the 'onInput' delegate/callback is called.
    /// </remarks>
    public bool registerInput
    {
        get
        {
            return _registerInput;
        }
        set
        {
            _registerInput = value;
            Update();
        }
    }

    /// <summary>
    /// Monitor collisions yes/no
    /// </summary>
    /// <remarks>
    /// When collidable is set to true, this OTObject is monitored for collisions. When this object collides with another
    /// 'collidable' object, the onCollision delegate/callback is called. In addition to the onCollision delegate, the
    /// onEnter, onExit and onStay delegates/callbacks are called as well as a collidable object enters, exits or resides within 
    /// another collidable object.
    /// </remarks>
    public bool collidable
    {
        get
        {
            return _collidable;
        }
        set
        {
            _collidable = value;
            Update();
        }
    }

    /// <summary>
    /// Physics type of this object
    /// </summary>
    public Physics physics
    {
        get
        {
            return _physics;
        }
        set
        {
            _physics = value;
            Update();
        }
    }

    /// <summary>
    /// Physics Collider type of this object
    /// </summary>
    public ColliderType colliderType
    {
        get
        {
            return _colliderType;
        }
        set
        {
            _colliderType = value;
            Update();
        }
    }

    /// <summary>
    /// Current collision object
    /// </summary>
    /// <remarks>
    /// The collisionObject is the destination OTObject of a collision
    /// </remarks>
    public OTObject collisionObject
    {
        get
        {
            return _collisionObject;
        }
    }
		
	int restoreDepth;
	float restoreAlpha;
	public void HandleDrag(string dragCommand, OTObject dropped)
	{
		switch(dragCommand)
		{
			case "start":
				_dropTarget = null;
		        if (onDragStart != null)
		            onDragStart(this);
		        if (!CallBack("onDragStart", callBackParams))
		            CallBack("OnDragStart", callBackParams);						
			
				// change depth and alpha
				if (dragDepth>-1000 && dragDepth<1000)
				{
					restoreDepth = depth;
					depth = dragDepth;
				}		
				if (this is OTSprite)
				{
					restoreAlpha = (this as OTSprite).alpha;
					(this as OTSprite).alpha = dragAlpha;
				}
			
			break;
			
			case "end":
			
				_dropTarget = OT.ObjectUnderPoint(Input.mousePosition, new OTObject[]{}, new OTObject[]{this} );								
				if (_dropTarget==null)
					_dropTarget = OT.ObjectUnderPoint(OT.view.camera.WorldToScreenPoint(otTransform.position), new OTObject[]{}, new OTObject[]{this} );								
			
	            if (onDragEnd != null)
	                onDragEnd(this);
	            if (!CallBack("onDragEnd", callBackParams))
	                CallBack("OnDragEnd", callBackParams);				
			
				
				if (_dropTarget!=null && _dropTarget.onReceiveDrop!=null)
					_dropTarget.HandleDrag("receive", this);
			
				// restore depth and alpha
				if (dragDepth>-1000 && dragDepth<1000)
					depth = restoreDepth;
				if (this is OTSprite)
					(this as OTSprite).alpha = restoreAlpha;
			
			break;

			case "receive":
				_dropTarget = dropped;			
	            if (onReceiveDrop != null)
	                onReceiveDrop(this);
	            if (!CallBack("onReceiveDrop", callBackParams))
	                CallBack("OnReceiveDrop", callBackParams);				
			break;
			
			
			case "drag":
	            if (onDragging != null)
	                onDragging(this);					
	            if (!CallBack("onDragging", callBackParams))
	                CallBack("OnDragging", callBackParams);							
			break;
			
		}
	}
	
    /// <summary>
    /// Current collision
    /// </summary>
    /// <remarks>
    /// The collision is only used when rigidBodys collide when using physics
    /// </remarks>
    public Collision collision
    {
        get
        {
            return _collision;
        }
    }

    /// <summary>
    /// User input hit point
    /// </summary>
    /// <remarks>
    /// The hit point is the point of user input 'inpact' when a user clicks or touches
    /// this OTObject. the x and y coordinates are relative to this objects position.
    /// </remarks>
    public Vector2 hitPoint
    {
        get
        {
            return _hitPoint;
        }
    }

    /// <summary>
    /// Target of the drag and drop action with thius sprite.
    /// </summary>
    /// <remarks>
    /// If this is the dragging object, the dropTarget will be set to the object that received the dragged sprite.
    /// If this is the receiving object (<see cref="registerInput" /> must be true), dropTarget will hold the sprite
    /// that was dragged and dropped.
    /// </remarks>
    public OTObject dropTarget
    {
        get
        {
            return _dropTarget;
        }
    }

	
	float paintingDepth
	{
		get
		{
			float _paintingDepth = 0;
			float dv=1;
			if (OT.world3D && transform.parent!=null)
				dv = 100;				
			_paintingDepth = (OT.world == OT.World.WorldTopDown2D)?-depth:(float)depth/dv;
			if (OT.painterAlgorithm)
				_paintingDepth += pd();
			return _paintingDepth;
		}		
	}
	
    /// <summary>
    /// Position (x/y) in pixels
    /// </summary>
    /// <remarks>
    /// This Vector2 object reflects the x/y values of this OTObject's associated GameObject 
    /// transform posotion Vector3 object. Z value will be controlled by this OTObject's depth.
    /// </remarks>
    public Vector2 position
    {
        get
        {
            if (otTransform != null)
			{
				if (otTransform.parent!=null)
                	return new Vector2(otTransform.localPosition.x - offset.x, (OT.world == OT.World.WorldTopDown2D)?otTransform.localPosition.z - offset.y:otTransform.localPosition.y + offset.y);
				else
                	return new Vector2(otTransform.position.x - offset.x, (OT.world == OT.World.WorldTopDown2D)?otTransform.position.z - offset.y:otTransform.position.y + offset.y);
			}
            else
                return Vector2.zero;
        }
        set
        {
			
			if (otTransform == null)
				otTransform = transform;

            Vector2 pos = value;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }					
						
			if (OT.world == OT.World.WorldTopDown2D)
            	otTransform.localPosition = new Vector3(pos.x + offset.x, paintingDepth, pos.y - offset.y);
			else
            	otTransform.localPosition = new Vector3(pos.x + offset.x, pos.y - offset.y, paintingDepth);
									
            _position = pos;
            _position_ = pos;
			Changed();
			
        }
    }
	
	/// <summary>
	/// Activates this object
	/// </summary>
	public void Activate()
	{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5										
		gameObject.SetActiveRecursively(true);
#else
		gameObject.SetActive(true);
#endif		
	}
	
	/// <summary>
	/// Deactivates this object
	/// </summary>
	public void Deactivate()
	{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5										
		gameObject.SetActiveRecursively(false);
#else
		gameObject.SetActive(false);
#endif		
	}

	/// <summary>
	/// True if this object is active
	/// </summary>
	public bool isActive
	{
		get
		{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5										
		return gameObject.active;
#else
		return gameObject.activeSelf;
#endif								
		}
	}
	
	
	protected virtual void Resized()
	{
	}
	
	/// <summary>
	/// Gets the size of this object in world coordinates
	/// </summary>
	public Vector2 worldSize
	{
		get
		{
			if (otTransform.parent==null)
				return size;
			else
				return toWorld(size) - (Vector2)otTransform.parent.position;				
		}
		set
		{
			if (otTransform.parent==null)
				size = value;
			else
				size = fromWorld((Vector2)otTransform.parent.position + value);
		}		
	}
	
	
	/// <summary>
	/// Gets the position of this object in world coordinates
	/// </summary>
	public Vector2 worldPosition
	{
		get
		{
			if (otTransform.parent==null)
				return position;
			else
				return toWorld(this.position);
		}
		set
		{
			if (otTransform.parent==null)
				position = value;
			else
				position = fromWorld(value);
		}
	}
    /// <summary>
    /// Size (x/y) in pixels
    /// </summary>
    public Vector2 size
    {
        get
        {
            if (otTransform != null)
               	return new Vector2(otTransform.localScale.x, otTransform.localScale.y);
            else
              return Vector2.zero;
        }
        set
        {
			if (otTransform==null)
				InitComponents();
			
            float x = value.x;
            float y = value.y;
            if (OT.view.customSize == 0)
            {
                if (x < 1 || y < 1)
                {
                    value = new Vector2((x < 0) ? 1 : x, (y < 0) ? 1 : y);
                }
            }
						
           	otTransform.localScale = new Vector3(value.x, value.y, 1 );
            _size = value;
            _size_ = value;
			if (oSize.Equals(Vector2.zero))
            	oSize = _size;
			
			Resized();
			Changed();
			
        }
    }
	
    /// <summary>
    /// Rotation in degrees
    /// </summary>
    /// <remarks>
    /// this reflects the z value values of this OTObject's associated GameObject 
    /// transform rotation Vector3 object (euler). x/y rotation will always be 0.
    /// </remarks>
    public float rotation
    {
        get
        {
			if (otTransform == null)
				otTransform = transform;
			if (OT.world == OT.World.WorldTopDown2D)
				return otTransform.rotation.eulerAngles.y - offsetRotation;
			else
				return otTransform.rotation.eulerAngles.z - offsetRotation;
        }
        set
        {
			if (otTransform == null)
				otTransform = transform;
            float val = value;
            // keep this rotation within 0-360
            if (val < 0) val += 360.0f;
            else
                if (val >= 360) val -= 360.0f;
			if (OT.world == OT.World.WorldTopDown2D)
            	otTransform.rotation = Quaternion.Euler(90, val + offsetRotation, 0);
			else
            	otTransform.rotation = Quaternion.Euler(0, 0, val + offsetRotation);
            _rotation = val;
			Changed();
        }
    }
	
	
	/// <summary>
	/// Inidcates if this sprite has a mesh
	/// </summary>
	public bool hasMesh
	{
		get
		{
			return (mesh!=null);
		}
	}

	/// <summary>
	/// The uv coordinates of this sprite
	/// </summary>
    public Vector2[] uv
    {
        get
        {
            if (mesh != null)
                return mesh.uv;
			else
				OT.print("mesh of "+name+" == null");
            return null;
        }
    }
		
	/// <summary>
	/// the mesh colors of this sprite
	/// </summary>
	public Color[] colors
	{
		get
		{		
			
			if (mesh.colors.Length==0)
			{
				Color[] colors = new Color[]{};
				System.Array.Resize<Color>(ref colors, mesh.vertices.Length);
				for (int i=0; i<mesh.vertices.Length;i++)
					colors[i] = Color.white;
				mesh.colors = colors;
			}				
			return mesh.colors;
		}
		set
		{
			mesh.colors = value;
		}
	}
	
	public virtual void _IMsg(string cmd)
	{
	}
	
	bool batched = false;
	public string _iMsg
	{
		set
		{
			switch(value)
			{
			case "batched": 
				batched = true; 
				_visible = otRenderer.enabled;
				break;
			case "!batched": 
				batched = false; 
				break;
			case "visible":
				_visible = true;
				break;
			case "!visible":
				_visible = false;
				break;
			case "hide":
				if (otRenderer.enabled)
					otRenderer.enabled = false;
				break;
			case "show":
				if (visible)
					otRenderer.enabled = true;
				break;
			default:
				_IMsg(value);
				break;
			}
		}
	}
		
    protected List<Renderer> hiddenRenderers = new List<Renderer>();
    protected List<OTSprite> hiddenSprites = new List<OTSprite>();
	bool _visible = true;
	
	void SetChildrenInvisible()
	{
		hiddenRenderers.Clear();
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        for (int r = 0; r < renderers.Length; r++)
        {
			if (batched)
			{
				OTSprite sprite = renderers[r].gameObject.GetComponent<OTSprite>();
				if (sprite!=null)
				{
					hiddenSprites.Add(sprite);
					sprite._iMsg = "!visible";
					sprite.Changed();
				}
				else
				{
					if (renderers[r].enabled)
					{
                    	hiddenRenderers.Add(renderers[r]);
			   			renderers[r].enabled = false;
					}
				}
			}
			else
			{
				if (renderers[r].enabled)
				{
                	hiddenRenderers.Add(renderers[r]);								
			   		renderers[r].enabled = false;							
				}
			}
        }
	}

	void SetChildrenVisible()
	{
        if (hiddenRenderers.Count > 0 || hiddenSprites.Count>0)
        {
			if (batched)
			{
                for (int s = 0; s < hiddenSprites.Count; s++)
				{
                    hiddenSprites[s]._iMsg = "visible";
					hiddenSprites[s].Changed();
				}
				hiddenSprites.Clear();
			}							
            for (int r = 0; r < hiddenRenderers.Count; r++)
                hiddenRenderers[r].enabled = true;
            hiddenRenderers.Clear();
        }
		else
		{
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int r = 0; r < renderers.Length; r++)
			{
				if (batched)
				{
					OTSprite sprite = renderers[r].gameObject.GetComponent<OTSprite>();
					if (sprite!=null)
					{
						sprite._iMsg = "visible";
						sprite.Changed();
					}
					else
						renderers[r].enabled = true;
				}
				else
                	renderers[r].enabled = true;
			}
		}
	}
	
    /// <summary>
    /// Hides and shows an Orthello object
    /// </summary>
    /// <remarks>
    /// All children of this object, that have renderers, will be automaticly hidden as well.
    /// </remarks>
    public bool visible
    {
        get
        {
			if (batched)
				return _visible;			
            return otRenderer.enabled;
        }
        set
        {			
			if (!value && (this is OTSprite) && (this as OTSprite).isInvalid)
				(this as OTSprite).IsValid();
			
            if (value != visible)
            {
				if (batched)
					_visible = value;	
				else
					otRenderer.enabled = value;
				
				if (otTransform.childCount > 0)
				{					
	                if (!value)
						SetChildrenInvisible();
	                else
						SetChildrenVisible();
				}
            }
			Changed();			
        }
    }
	
    //-----------------------------------------------------------------------------
    // private and protected fields
    //-----------------------------------------------------------------------------
    
    protected int _collisionDepth_ = 0;
    
    protected float _collisionSize_ = 0.4f;
    
    protected string _name_ = "";
    
    bool _registerInput_ = false;
    bool _collidable_ = false;
    Physics _physics_ = Physics.Trigger;
    ColliderType _colliderType_ = ColliderType.Box;
    Pivot _pivot_ = Pivot.Center;
    Vector2 _pivotPoint_ = Vector2.zero;
    
    protected bool isCopy = false;
    
	// oSize is original (current) sprite size
    [HideInInspector]
    public Vector2 oSize;
    
    Vector2 _position_ = Vector2.zero;
	int _dragDepth = 9999;
	float _dragAlpha = 1;
    
    protected Vector2 _size_ = new Vector2(100, 100);
    
    float _rotation_ = 0;
    
    protected object[] callBackParams;
	    
    [HideInInspector]
    public Vector2 imageSize = Vector2.zero;
    
    protected Vector2 offset
    {
        get
        {
            return _offset;
        }
        set
        {
            _baseOffset = value;
            Vector2 nOffset = value;						
            if (!Vector2.Equals(Vector2.zero, imageSize) && !Vector2.Equals(size, imageSize))
            {
                float off = pivotPoint.x + 0.5f;
                if (off > 0)
                    nOffset.x = (value.x + (size.x * off)) - (imageSize.x * off);

                off = (pivotPoint.y * -1) + 0.5f;
                if (off > 0)
                    nOffset.y = (value.y + (size.y * off)) - (imageSize.y * off);

                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation * -1)), Vector3.one);
                    nOffset = m.MultiplyPoint3x4(nOffset);
                }
            }
            _offset = nOffset;
        }
    }
    
    [HideInInspector]
    public Vector2 _offset = Vector2.zero;
    
    [HideInInspector]
    public Vector2 _baseOffset = Vector2.zero;
    
    protected float offsetRotation = 0;

    
    bool _dirtyChecks = false;
    
    protected Mesh mesh;
    
    protected bool meshDirty = false;
    
    protected bool isDirty = false;
    
    protected OTObject _dropTarget = null;

    Vector2 _hitPoint;
    OTObject _collisionObject = null;
	Collision _collision = null;
    List<Component> callBackTargets = new List<Component>();
    int _depth_ = 0;

    
    protected bool inView = true;

    private List<OTController> controllers = new List<OTController>();
    private Dictionary<System.Type, OTController> controllerLookupType = new Dictionary<System.Type, OTController>();
    private Dictionary<string, OTController> controllerLookupName = new Dictionary<string, OTController>();
	
	protected Vector3[] TranslatePivotVerts(Mesh mesh, Vector3[] verts)
	{
        mesh.vertices = verts;		
        mesh.RecalculateBounds();

		Bounds bo = mesh.bounds;
		Matrix4x4 mx = new Matrix4x4();
		
		Vector2 ext = bo.extents;
		Vector2 cen = -bo.center;
		
		Vector2 vdx = cen +
			new Vector2( (ext.x * 2) * -pivotPoint.x ,
					 (ext.y * 2) * -pivotPoint.y);
								
		mx.SetTRS( vdx ,Quaternion.identity,Vector3.one);		
		for (int i=0; i<verts.Length; i++)
			verts[i] = mx.MultiplyPoint3x4(verts[i]);
		
		return verts;				
	}
	
	
    // -----------------------------------------------------------------
    // virtual methods
    // -----------------------------------------------------------------
    /// <summary>
    /// Overridable virtual method that will provide object's mesh
    /// </summary>
    /// <returns>object's mesh</returns>
    protected virtual Mesh GetMesh()
    {
        return null;
    }

    /// <summary>
    /// Overridable virtual method that will provide object's type name
    /// </summary>
    /// <returns>object's type name</returns>
    protected virtual string GetTypeName()
    {
        return "Object";
    }

    protected void SetCollider()
    {
        if (physics == Physics.Custom || mesh == null)
            return;
		
        if (colliderType == ColliderType.Box)
        {
            BoxCollider b = GetComponent<BoxCollider>();
            if (b != null)
            {
                b.size = new Vector3(
					(mesh.bounds.extents.x*2),
					(mesh.bounds.extents.y*2), collisionSize);
				Vector2 p = (pivotPoint * -1); 
				p.x *= (mesh.bounds.extents.x * 2);
				p.y *= (mesh.bounds.extents.y * 2);
                b.center = new Vector3(p.x, p.y, collisionDepth - depth);
                b.isTrigger = true;
                if (collidable)
                {
                    if (physics == Physics.Trigger)
                        b.isTrigger = true;
                    else
                        b.isTrigger = false;
                }
                else
                {
                    b.isTrigger = false;
                }
            }

        }
        else
        {
            SphereCollider s = GetComponent<SphereCollider>();
            if (s != null)
            {
                if (s.radius == 0)
                    s.radius = 0.5f;
                if (collidable)
                {
                    s.center = (pivotPoint * -1);
                    s.center += new Vector3(0, 0, collisionDepth - depth);
                    s.isTrigger = true;	
                    if (physics == Physics.Trigger)
                        s.isTrigger = true;
                    else
                        s.isTrigger = false;
                }
                else
                {
                    s.center = pivotPoint * -1;
                    s.isTrigger = false;
                }
            }
        }
    }

    void CheckInputCollidable()
    {
        if (physics == Physics.Custom)
        {
            if (registerInput)
                OT.InputTo(this);
            else
                OT.NoInputTo(this);
            return;
        }
		
        BoxCollider b = GetComponent<BoxCollider>();
        SphereCollider sp = GetComponent<SphereCollider>();
        Rigidbody bd = GetComponent<Rigidbody>();

        if (!collidable && physics != Physics.Trigger)
        {
            _collidable = true; _collidable_ = true;
        }

        if (collidable)
        {
            if (colliderType == ColliderType.Box)
            {
                if (b == null)
                    b = gameObject.AddComponent<BoxCollider>();
                if (sp != null)
                    DestroyImmediate(sp);
				otCollider = collider;
            }
            else
            {
                if (sp == null)
                    sp = gameObject.AddComponent<SphereCollider>();
                if (b != null)
                    DestroyImmediate(b);
				otCollider = collider;
            }

            if (bd == null)
                bd = gameObject.AddComponent<Rigidbody>();

            switch (physics)
            {
                case Physics.Trigger:
                    bd.useGravity = false;
                    bd.isKinematic = true;
                    break;
                case Physics.RigidBody:
                    bd.useGravity = true;
                    bd.isKinematic = false;
                    break;
                case Physics.NoGravity:
                    bd.useGravity = false;
                    bd.isKinematic = false;
                    break;
                case Physics.StaticBody:
                    bd.useGravity = true;
                    bd.isKinematic = true;
                    collisionDepth = 0;
                    break;
                case Physics.StaticRigidBody:
                    bd.useGravity = true;
                    bd.isKinematic = true;
                    break;
            }
            bd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }
        else
        {
            if (bd != null)
                DestroyImmediate(bd);
            if (colliderType == ColliderType.Box && sp != null)
            {
                DestroyImmediate(sp);
                sp = null;
            }
            if (colliderType == ColliderType.Sphere && b != null)
            {
                DestroyImmediate(b);
                b = null;
            }
        }

        if (registerInput)
        {
            if (colliderType == ColliderType.Box && b == null)
                b = gameObject.AddComponent<BoxCollider>();
            if (colliderType == ColliderType.Sphere && sp == null)
                sp = gameObject.AddComponent<SphereCollider>();
            OT.InputTo(this);
			otCollider = collider;
        }
        else
        {
            OT.NoInputTo(this);
            if (!registerInput && b != null && !collidable)
                DestroyImmediate(b);
            if (!registerInput && sp != null && !collidable)
                DestroyImmediate(sp);
        }
        SetCollider();
    }

    /// <summary>
    /// Overridable virtual method that will check object's editor settings
    /// </summary>
    protected virtual void CheckSettings()
    {		
        if (collidable != _collidable_ && !collidable)
        {
            if (_physics != Physics.Custom)
                _physics = Physics.Trigger;
        }

        if (physics != _physics_ || _collisionSize == -1)
        {
            switch (physics)
            {
                case Physics.Custom:
                    _collisionSize = 0;
                    break;
                case Physics.Trigger:
                    _collisionSize = 0.4f;
                    break;
                case Physics.RigidBody:
                    _collisionSize = 10f;
                    break;
                case Physics.NoGravity:
                    _collisionSize = 10f;
                    break;
                case Physics.StaticBody:
                    _collisionSize = 2000f;
                    break;
                case Physics.StaticRigidBody:
                    _collisionSize = 10f;
                    break;
            }
        }

        if (registerInput != _registerInput_ || collidable != _collidable_ || collisionDepth != _collisionDepth_ || physics != _physics_ ||
            colliderType != _colliderType_ || collisionSize != _collisionSize_)
        {
            _registerInput_ = registerInput;
            _collidable_ = collidable;
            _collisionDepth_ = collisionDepth;
            _physics_ = physics;
            _collisionSize_ = collisionSize;
            _colliderType_ = colliderType;
            CheckInputCollidable();
        }
		
		if (OT.world2D)
		{
	        if (depth < -1000) depth = -1000;
	        if (depth > 1000) depth = 1000;
		}

        if (_depth_ != depth)
            _depth_ = depth;
								
        if (size.x < 0 || size.y < 0)
            size = new Vector2((size.x < 0) ? 0 : size.x, (size.y < 0) ? 0 : size.y);

        if (name != _name_)
        {
            OT.RegisterLookup(this, _name_);
            _name_ = name;
            gameObject.name = name;
            baseName = name;
        }
        else
            if (name != gameObject.name)
            {
                _name = gameObject.name;
                OT.RegisterLookup(this, _name_);
                _name_ = name;
                baseName = name;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
            }
		
        if (!Vector2.Equals(_size_, _size) || !Vector2.Equals(_position_, _position) || _rotation_ != _rotation)
        {								
            Vector2 pos = _position;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                if (!Vector2.Equals(_position, pos))
                    _position = pos;
            }

            if (!Vector2.Equals(_size_, _size))
            {
                float x = _size.x;
                float y = _size.y;
                if (OT.view.customSize == 0)
                    if (x < 1 || y < 1)
                    {
                        _size = new Vector2((x <= 0) ? 1 : x, (y <= 0) ? 1 : y);
                    }
            }

            if (!Vector2.Equals(_size_, _size))
            {
                size = _size;
                _size_ = _size;
            }
            if (!Vector2.Equals(_position_, _position))
            {
                position = _position;
                _position_ = _position;
            }
            if (!Vector2.Equals(_rotation_, _rotation))
            {
                rotation = _rotation;
                _rotation_ = _rotation;
            }
        }
		
        if (!Vector2.Equals(size, _size) || !Vector2.Equals(position, _position) || rotation != _rotation)
        {
			
            Vector2 pos = position;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                if (!Vector2.Equals(position, pos))
                    position = pos;
            }
            if (!Vector2.Equals(size, _size))
            {
                _size = size;
                _size_ = size;
            }
            if (!Vector2.Equals(position, _position))
            {
                _position = position;
                _position_ = position;
            }
            if (!Vector2.Equals(rotation, _rotation))
            {
                _rotation = rotation;
                _rotation_ = rotation;
            }
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
        }				

		float _paintingDepth = paintingDepth;
		if (OT.world != OT.World.WorldTopDown2D)
		{
	        if (otTransform.localPosition.z != _paintingDepth)
            	otTransform.localPosition = new Vector3(otTransform.localPosition.x, otTransform.localPosition.y, _paintingDepth);	
		}
		else
		{
	        if (otTransform.localPosition.y != _paintingDepth)
	            	otTransform.localPosition = new Vector3(otTransform.localPosition.x, _paintingDepth, otTransform.localPosition.z);	
		}
	}

    /// <summary>
    /// Overridable virtual method that will check Objects mesh settings
    /// </summary>
    protected virtual void CheckDirty()
    {
        if (_pivot_ != _pivot)
        {
            pivot = _pivot;
            _pivot_ = _pivot;
        }
        if (_pivotPoint_ != _pivotPoint)
        {
            _pivotPoint_ = pivotPoint;
            meshDirty = true;
        }
    }
	
    /// <summary>
    /// Overridable virtual method that will clean the dirty object
    /// </summary>
    protected virtual void Clean()
    {
    }

   	/// <summary>
	/// Overridable virtual method that will assign default properties 
	/// from the prototype to the instanced new object after it is created
	/// from the pool
	/// </summary>
    public virtual void Assign(OTObject protoType)
    {
        rotation = protoType.rotation;
        position = protoType.position;
        size = protoType.size;
        oSize = protoType.oSize;
		depth = protoType.depth;
		registerInput = protoType.registerInput;
		draggable = protoType.draggable;
			
        if (pivot != protoType.pivot || pivotPoint != protoType.pivotPoint)
        {
            pivot = protoType.pivot;
            pivotPoint = protoType.pivotPoint;
            Update();
        }
		
		if (protoType.collider !=null && protoType.collider is BoxCollider)
		{
			(otCollider as BoxCollider).center = (protoType.collider as BoxCollider).center;
			(otCollider as BoxCollider).size = (protoType.collider as BoxCollider).size;
			(otCollider as BoxCollider).enabled = (protoType.collider as BoxCollider).enabled;
		}
		else
		if (protoType.collider !=null && protoType.collider is SphereCollider)
		{
			(otCollider as SphereCollider).center = (protoType.collider as SphereCollider).center;
			(otCollider as SphereCollider).radius = (protoType.collider as SphereCollider).radius;
			(otCollider as SphereCollider).enabled = (protoType.collider as SphereCollider).enabled;
		}
	}

    /// <summary>
    /// Overridable virtual method that is called after create the object's mesh
    /// </summary>
    protected virtual void AfterMesh()
    {
        size = _size;
		//position = _position;
		
		if (OT.world == OT.World.WorldTopDown2D)
			otTransform.localRotation = Quaternion.Euler( new Vector3(90,rotation,0));
		else
		if (OT.world == OT.World.WorldSide2D)
			otTransform.localRotation = Quaternion.Euler( new Vector3(0,0,rotation));
				
        SetCollider();
		
		if (mesh!=null && mesh.normals.Length == 0) 
			mesh.RecalculateNormals();
		
		Changed();		
		
    }
		
    public virtual void OnInput(Vector2 hitPoint)
    {
        _hitPoint = hitPoint - position;
								
        if (onInput != null)
            onInput(this);
        if (!CallBack("onInput", callBackParams))
            CallBack("OnInput", callBackParams);

        return;
    }
    
    public virtual void OnMouseOver()
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseMoveOT != null)
            onMouseMoveOT(this);
        if (!CallBack("onMouseMoveOT", callBackParams))
            CallBack("OnMouseMoveOT", callBackParams);

        return;
    }
    
    public virtual void OnMouseEnter()
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseEnterOT != null)
            onMouseEnterOT(this);
        if (!CallBack("onMouseEnterOT", callBackParams))
            CallBack("OnMouseEnterOT", callBackParams);

        return;
    }
        
    public virtual void OnMouseExit()
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseExitOT != null)
            onMouseExitOT(this);
        if (!CallBack("onMouseExitOT", callBackParams))
            CallBack("OnMouseExitOT", callBackParams);

        return;
    }

    // -----------------------------------------------------------------
    // class methods
    // -----------------------------------------------------------------
    /// <summary>
    /// Object has to use callback functions.
    /// </summary>
    /// <param name="target">target class that will receive the callbacks.</param>
    public void InitCallBacks(Component target)
    {
        callBackTargets.Add(target);
    }

    /// <summary>
    /// Rotate so we are heading (up vector) to a specific position.
    /// </summary>
    /// <param name="toPosition">Position that we will be heading to.</param>
    public void RotateTowards(Vector2 toPosition)
    {
        Vector2 upVector = toPosition - position;
		otTransform.up = upVector.normalized;								
		if (OT.world == OT.World.WorldTopDown2D)
		{
			if (otTransform.up.Equals(new Vector3(0,-1.0f,0)))
				rotation = 180;
			else				
				otTransform.rotation = Quaternion.Euler(90, otTransform.rotation.eulerAngles.y, otTransform.rotation.eulerAngles.z);
		}		
        _rotation = rotation;
        _rotation_ = _rotation;
    }
	
    /// <summary>
    /// Rotate so we are heading (up vector) to a specific object
    /// </summary>
    /// <param name="target">Target object that we will be heading to.</param>
    public void RotateTowards(OTObject target)
    {
		RotateTowards(target.position);
    }
	
	/// <summary>
	/// Rotates the towards a specific point with a specific turning rate.
	/// </summary>
	public void RotateTowards(Vector2 toPosition, float turnRate, float minSpeed, float maxSpeed)
    {
		float minEpsilonDegree = 5; 
		 		 
		if (OT.world == OT.World.WorldSide2D)
		{
			// vector from the object to target
			Vector3 toTarget = new Vector3(toPosition.x - position.x, toPosition.y - position.y, 0);
			 
			// current object's vector (angle -> vector)
			var currentAngle1 = -Mathf.Sin(otTransform.rotation.eulerAngles.z * Mathf.Deg2Rad);
			var currentAngle2 = Mathf.Cos(otTransform.rotation.eulerAngles.z * Mathf.Deg2Rad);
			Vector3 curr = new Vector3(currentAngle1, currentAngle2, 0);
			 
			// getting amount to turn
			float angle = Vector3.Angle(toTarget, curr);
			var amount = angle * (Vector3.Dot(Vector3.forward, Vector3.Cross(toTarget, curr)) < 0 ? -1 : 1);
			
			 
			// turning
			if (Mathf.Abs(amount) > minEpsilonDegree)
			{
				amount *= turnRate * Time.deltaTime; // get time independent amount
				var absAmount = Mathf.Abs(amount);   // abs
				amount = (absAmount > minSpeed) ? amount: minSpeed*amount/absAmount; // if absAmount is less than minSpeed, use minSpeed*direction
				amount = (absAmount < maxSpeed) ? amount: maxSpeed*amount/absAmount; // if absAmount is more than maxSpeed, use maxSpeed*direction
				if (!float.IsNaN(amount))
					rotation -= amount;
			}
			else
			{
				if (!float.IsNaN(amount))				
					rotation -= amount;
			}
		} 
        _rotation = rotation;
        _rotation_ = _rotation;
    }
 
	/// <summary>
	/// Rotates the towards an object with a specific turning rate.
	/// </summary>
	public void RotateTowards(OTObject target, float turnRate, float minSpeed, float maxSpeed)
    {
		RotateTowards(target.position, turnRate, minSpeed, maxSpeed);
    }
 
	
    
    public void SetDropped(OTObject o)
    {
        _dropTarget = o;
    }

    
    public virtual void StartUp()
    {
		InitComponents();
				
        if (!OT.IsRegistered(this))
            OT.Register(this);
        if (registerInput)
            OT.InputTo(this);
        else
            OT.NoInputTo(this);

		if (mesh == null)
		{
	        MeshFilter mf = GetComponent<MeshFilter>();
	        if (mf != null) mesh = mf.mesh;
		}
				
        inView = true;
    }

    // -----------------------------------------------------------------
    // class methods
    // -----------------------------------------------------------------

    void OnTriggerEnter(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (_collisionObject != null)
        {
            if (_collisionObject.collidable)
            {
                if (onCollision != null)
                    onCollision(this);
                if (!CallBack("onCollision", callBackParams))
                    CallBack("OnCollision", callBackParams);
            }
            if (onEnter != null)
                onEnter(this);
            if (!CallBack("onEnter", callBackParams))
                CallBack("OnEnter", callBackParams);
        }
    }

    void OnTriggerExit(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (onExit != null)
            onExit(this);
        if (!CallBack("onExit", callBackParams))
            CallBack("OnExit", callBackParams);
    }

    void OnTriggerStay(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (onStay != null)
            onStay(this);
        if (!CallBack("onStay", callBackParams))
            CallBack("OnStay", callBackParams);
    }

    void OnCollisionEnter(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (_collisionObject != null)
        {
            if (_collisionObject.collidable)
            {
                if (onCollision != null)
                    onCollision(this);
                if (!CallBack("onCollision", callBackParams))
                    CallBack("OnCollision", callBackParams);
            }
            if (onEnter != null)
                onEnter(this);
            if (!CallBack("onEnter", callBackParams))
                CallBack("OnEnter", callBackParams);
        }
    }

    void OnCollisionExit(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (onExit != null)
            onExit(this);
        if (!CallBack("onExit", callBackParams))
            CallBack("OnExit", callBackParams);
    }

    void OnCollisionStay(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (onStay != null)
            onStay(this);
        if (!CallBack("onStay", callBackParams))
            CallBack("OnStay", callBackParams);
    }
	
    
    protected bool CallBack(string handler, object[] param)
    {
		// if flash we cant use reflection to activate a callback
#if UNITY_FLASH
#else
        for (int t = 0; t < callBackTargets.Count; t++)
        {
            MethodInfo mi = callBackTargets[t].GetType().GetMethod(handler);
            if (mi != null)
            {
                mi.Invoke(callBackTargets[t], param);
                return true;
            }
        }
#endif
        return false;
    }
	
	float pd()
	{
		float px = Mathf.Abs(position.x);
		float py = Mathf.Abs(position.y);
		
		if (py<0.01f && px<0.01f)
			return  position.y - (position.x / 10);
		else
		if (py<0.1f && px<0.1f)
			return  (position.y / 10) - (position.x / 100);
		else
		if (py<1 && px<1)
			return  (position.y / 100) - (position.x / 1000);
		else
			return  (position.y / 1000) - (position.x / 10000);
	}
	
	public void InitComponents()
	{
		if (otRenderer == null)
			otRenderer = renderer;
		if (otCollider == null)
			otCollider = collider;
		if (otTransform == null)
			otTransform = transform;		
	}
		
    protected virtual void Awake()
    {
		InitComponents();
				
#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
        // initialize attributes
        _position_ = position;
        _depth_ = depth;
        _size_ = size;
        _rotation_ = rotation;
        _pivot_ = pivot;
        _pivotPoint_ = pivotPoint;
        _name_ = name;
        _collidable_ = collidable;
        _collisionDepth_ = collisionDepth;
        _collisionSize_ = collisionSize;
        _physics_ = physics;
						
		if (otTransform.parent!=null)
			_position = position;			
		
		if (OT.world == OT.World.WorldTopDown2D)
			otTransform.localRotation = Quaternion.Euler( new Vector3(90,rotation,0));
	
		if (physics == Physics.RigidBody || physics == Physics.NoGravity || physics == Physics.Custom)
			passiveControl = true;
		
    }

    // -----------------------------------------------------------------
    // Use this for initialization
    
    protected OTObject copyObject = null;
	
    
	bool isStarted = false;
    protected virtual void Start()
    {	
		isStarted = true;
		InitComponents();
		
		callBackParams = new object[] { this };		
        if (!OT.isValid)
        {
            Debug.LogError("Orthello : Orthello main OT system object not available!");
            Debug.Log("Please remove this Orthello object and add the OT object first.");
        }
		
        isCopy = (name != "" && OT.ObjectByName(name) != null && OT.ObjectByName(name) != this);
        if (isCopy)
            copyObject = OT.ObjectByName(name);
				
        if (_name == "" || isCopy )
        {
            if (copyObject != null )
            {
				if (copyObject !=null)
				{
	                baseName = copyObject.baseName;
	                if (baseName == "")
	                    baseName = name;
				}				
				if (baseName!="")
				{
	                int baseIdx = 1;
	                while (GameObject.Find(baseName + "-" + baseIdx))
	                    baseIdx++;
	                name = baseName + "-" + baseIdx;
#if UNITY_EDITOR
					if (!Application.isPlaying)
						UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif			
				}
            }
			if (name == "")
			{
                name = GetTypeName() + " (id=" + this.gameObject.GetInstanceID() + ")";
#if UNITY_EDITOR
				if (!Application.isPlaying)
					UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif											
			}
        }

        // check if we have a meshfilter
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null)
            mf = gameObject.AddComponent<MeshFilter>();
        // check if we have a mesh renderer
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null)
		{
            mr = gameObject.AddComponent<MeshRenderer>();
			otRenderer = renderer;
		}

        // check if we have to generate a mesh for this object
        if (Application.isPlaying) mesh = mf.mesh;
        else mesh = mf.sharedMesh;
				
        if (mesh == null || mesh.vertexCount == 0 || isCopy)
        {
            Mesh m = null;
            if (Application.isPlaying) m = mf.mesh;
            else m = mf.sharedMesh;
						
            mesh = GetMesh();
            if (mesh != null)
            {
                if (Application.isPlaying) mf.mesh = mesh;
                else mf.sharedMesh = mesh;
                AfterMesh();

                if (!isCopy && m != null)
                {
                    if (m.uv.Length > 0)
                        mesh.uv = m.uv;
                    DestroyImmediate(m);
                }
                meshDirty = false;
            }
        };

        if (!OT.IsRegistered(this))
            OT.Register(this);

        CheckInputCollidable();
        if (!Application.isPlaying)
        {
            if (init)
            {
                if (OT.view.customSize > 0)
                    _size *= OT.view.sizeFactor;
                init = false;
            }
        }
    }
		
	/// <summary>
	/// Worldbounds of this object to the area of the provided orthello object
	/// </summary>
	public void BoundBy(OTObject o, Vector2 expand)
	{
		Rect rect = o.rect;
		float dx = size.x/2;
		float dy = size.y/2;
		
		// set world bounds
		worldBounds = new Rect(rect.xMin + dx - expand.x, rect.yMin + dy - expand.y, rect.width - size.x + (expand.x*2), Mathf.Abs(rect.height) - size.y + (expand.y*2));
						
	}			
	public void BoundBy(OTObject o)
	{
		BoundBy(o, Vector2.zero);
	}	
	    
    public void ForceUpdate()
    {
        Update();
    }

	public void UpdateControllers()
	{
        if (controllers.Count > 0)
        {
            for (int c = 0; c < controllers.Count; c++)
            {
                OTController co = controllers[c];
                if (co.enabled)
                    co.Update(Time.deltaTime);
            }
        }
	}
	
	/// <summary>
	/// Synchronize this sprite with the sprite's transform
	/// </summary>
	/// <remarks>
	/// Sometimes you want to adjust the transform of a sprite using Translate, or by adjusting the gameObject.transform. Due to
	/// optimizations and passive mode, this is ignored by Orthello. Use this method to synchronize the orthello sprite with its 
	/// Transform object. Synchronize is autmaticly called when sprite.physics is set to RigidBody, Nogravity or Custom.
	/// </remarks>
	public void Synchronize()
	{
		_position = position;
		_position_ = _position;
		_rotation = rotation;
		_rotation_ = rotation;
	}
	

	public virtual void PassiveUpdate()
	{
		// check if we need to synchronize
		if (Application.isPlaying && (physics == Physics.RigidBody || physics == Physics.NoGravity || physics == Physics.Custom ))
			Synchronize();
	}
		
    // Update is called once per frame	
	int dirtyUpdateCycles = 0;					// lets always check all settings the first 5 cycles
    protected virtual void Update()
    {			
        if (!OT.isValid || otTransform == null || gameObject == null || OT.view == null ) return;
				
		if (!isStarted && !enabled)
			Start();

		// check if we need to synchronize
		if (Application.isPlaying && (physics == Physics.RigidBody || physics == Physics.NoGravity || physics == Physics.Custom ))
			Synchronize();
				
        if (!Application.isPlaying || dirtyChecks || OT.dirtyChecks || (dirtyUpdateCycles++<5))
        {
            if (registerInput != _registerInput_ && !registerInput && draggable)
			{
                draggable = false;
			
#if UNITY_EDITOR
				if (!Application.isPlaying)
					UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
			}
			
			
            if (draggable && !registerInput)
                _registerInput = true;

            if (!OT.recordMode)
            {								
                CheckSettings();
                CheckDirty();
            }			
        }
						
        if (meshDirty)
        {						
			isDirty = true;
            otTransform.localScale = Vector3.one;

            MeshFilter mf = GetComponent<MeshFilter>();
			Mesh newMesh = GetMesh();			
            if (mesh != null && !isCopy)
            {
                if (Application.isPlaying)
                    DestroyImmediate(mesh);
                else
                    DestroyImmediate(mesh);
            }			
            if (newMesh != null)
			{		
				mesh = newMesh;
				if (Application.isPlaying)
					mf.mesh = mesh;
				else
					mf.sharedMesh = mesh;
		        mesh.RecalculateBounds();
		        mesh.RecalculateNormals();								
                AfterMesh();
			}						
            meshDirty = false;
        }
		
        if (isDirty)
            Clean();

        if (isCopy)
            isCopy = false;

		UpdateControllers();		
    }

    void OnBecameVisible()
    {
        inView = true;
        if (onIntoView != null)
            onIntoView(this);
        if (!CallBack("onIntoView", callBackParams))
            CallBack("OnIntoView", callBackParams);
    }

    void OnBecameInvisible()
    {
        inView = false;
        if (onOutOfView != null)
            onOutOfView(this);
        if (!CallBack("onOutOfView", callBackParams))
            CallBack("OnOutOfView", callBackParams);
    }

    /// <summary>
    /// Get a Controller from this object with a specific type
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller(System.Type controllerType)
    {
        OTController c = null;
        if (controllerLookupType.ContainsKey(controllerType))
            c = controllerLookupType[controllerType];
        else
        {
            for (int ci = 0; ci < controllers.Count; ci++)
            {
                OTController co = controllers[ci];
                if (co.GetType().IsSubclassOf(controllerType))
                    return co;
            }
        }
        return c;
    }
    /// <summary>
    /// Get a Controller from this object with a specific name
    /// </summary>
    /// <param name="name">Name of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller(string name)
    {
        OTController c = null;
        if (controllerLookupName.ContainsKey(name))
            c = controllerLookupName[name];
        return c;
    }
    /// <summary>
    /// Get a Controller from this object with a specific type
    /// </summary>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller<T>()
    {
        return Controller(typeof(T));
    }

    /// <summary>
    /// Adds a controller to this object
    /// </summary>
    /// <param name="c">Controller to add</param>
    public void AddController(OTController c)
    {
        if (!controllers.Contains(c))
        {
            controllers.Add(c);
            controllerLookupName.Add(c.name, c);
            controllerLookupType.Add(c.GetType(), c);
            c.SetOwner(this);
        }
    }
    /// <summary>
    /// Removes a controller from this object
    /// </summary>
    /// <param name="c">Controller to remove</param>
    public void RemoveController(OTController c)
    {
        if (c == null) return;

        if (controllerLookupType.ContainsKey(c.GetType()))
            controllerLookupType.Remove(c.GetType());
        if (controllerLookupName.ContainsKey(c.name))
            controllerLookupName.Remove(c.name);
        if (controllers.Contains(c))
            controllers.Remove(c);
    }
    /// <summary>
    /// Removes all controllers
    /// </summary>
    public void ClearControllers()
    {
        while (controllers.Count > 0)
            RemoveController(controllers[0]);
    }
	
	protected void Alert(string message)
	{
		if (OT.debug)
			OTDebug.Message(message);
		else
			Debug.Log(message);
	}
		
	public virtual void Dispose()
	{
		ClearControllers();
	}
	    
    protected void OnDestroy()
    {
		callBackTargets.Clear();
        if (mesh != null)
            DestroyImmediate(mesh);

        if (OT.isValid)
            OT.RemoveObject(this);

    }
	
	public virtual void Reset()
	{
		meshDirty = true;
		isDirty = true;		
		
		if (!position.Equals(_position))
			position = _position;
		
		CheckInputCollidable();
		Update();
	}
	
	/// <summary>
	/// Gets a Child Orthello Object that begins with the specified name.
	/// </summary>
	public OTObject Child(string name)
	{
		return OT.FindChild(this.gameObject,name);
	}
	
	/// <summary>
	/// Gets a Child sprite that begins with the specified name.
	/// </summary>
	public OTSprite Sprite(string name)
	{
		return Child(name) as OTSprite;
	}
	
	/// <summary>
	/// Sets this sprite as a child of a parent GameObject
	/// </summary>
	public void ChildOf(GameObject parent)
	{
		dirtyUpdateCycles = 0;
		if (parent == null)
			otTransform.parent = null;
		else
			otTransform.parent = parent.transform;
		Update();
	}	
	
	/// <summary>
	/// Sets this sprite as a child of a parent OTObject
	/// </summary>
	public void ChildOf(OTObject parent)
	{
		ChildOf(parent.gameObject);
	}
		
	/// <summary>
	/// transforms a vector in this  object's parent local space to world space
	/// </summary>
	public Vector2 toWorld(Vector2 v)
	{
		if (otTransform.parent!=null)
			return otTransform.parent.localToWorldMatrix.MultiplyPoint3x4(v);
		else
		   	return v;
	}
	
	/// <summary>
	/// transforms a vector from worldspace to the object's parent local space
	/// </summary>
	public Vector2 fromWorld(Vector2 v)
	{
		if (otTransform.parent!=null)
			return otTransform.parent.worldToLocalMatrix.MultiplyPoint3x4(v);
		else
		   	return v;
	}

}

/// <summary>
/// Serializable int Vector2
/// </summary>
[System.Serializable]
public class IVector2
{
	/// <summary>
	/// The x value
	/// </summary>
	public int x;
	/// <summary>
	/// The y value
	/// </summary>
	public int y;

	/// <summary>
	/// a zero vector
	/// </summary>
	/// <value>
	/// The zero.
	/// </value>
	public static IVector2 zero
	{
		get
		{
			return new IVector2(0,0);
		}
	}
	
	public IVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public IVector2()
	{
		this.x = 0;
		this.y = 0;
	}
	
	/// <summary>
	/// Creates a copy of a vector
	/// </summary>
	public IVector2 Clone()
	{
		return new IVector2(x,y);
	}

	/// <summary>
	/// Determines if two int vectors are equal
	/// </summary>
	public bool Equals(IVector2 iv)
	{
		return (x == iv.x && y == iv.y);
	}
	
	public static string operator +(string s, IVector2 v)
	{
		return s + "("+v.x+","+v.y+")";
	}
		
	
}