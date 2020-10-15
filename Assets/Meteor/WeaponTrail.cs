#define USE_INTERPOLATION


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WeaponTrail : MonoBehaviour
{
	[SerializeField]
	bool _emit = false;
	public bool Emit { set{_emit = value;} }

	[SerializeField]
	float _emitTime = 0.0f;

	[SerializeField]
	Material _material;

	[SerializeField]
	public float _lifeTime = 1.0f;

	[SerializeField]
	public Color[] _colors;

	[SerializeField]
	float[] _sizes;

	[SerializeField]
	float _minVertexDistance = 0.1f;
	[SerializeField]
	float _maxVertexDistance = 10.0f;

	float _minVertexDistanceSqr = 0.0f;
	float _maxVertexDistanceSqr = 0.0f;

	[SerializeField]
	float _maxAngle = 3.00f;

	[SerializeField]
	bool _autoDestruct = false;

#if USE_INTERPOLATION
	[SerializeField]
	int subdivisions = 4;
#endif


	[SerializeField]
	Transform[] _transforms;

	List<Point> _points = new List<Point>();
#if USE_INTERPOLATION
	List<Point> _smoothedPoints = new List<Point>();
#endif
	GameObject _trailObject;
	Mesh _trailMesh;
	Vector3 _lastPosition;

	[System.Serializable]
	public class Point
	{
		public float timeCreated = 0.0f;
		public List<Vector3>  allVectors= new List<Vector3>(); 

		public void RemoveVectors()
		{
			allVectors.Clear ();
		}
	}

    public void Init(MeteorUnit owner)
    {
        _lastPosition = transform.position;
        _trailObject = new GameObject(owner.Attr.Name + "_Trail");
        _trailObject.transform.parent = null;
        _trailObject.transform.position = Vector3.zero;
        _trailObject.transform.rotation = Quaternion.identity;
        _trailObject.transform.localScale = Vector3.one;
        _trailObject.AddComponent(typeof(MeshFilter));
        _trailObject.AddComponent(typeof(MeshRenderer));
        _trailObject.gameObject.layer = gameObject.layer;
        _material = Instantiate(Resources.Load<Material>("WeaponTail"));
        _trailObject.GetComponent<Renderer>().material = _material;
        _colors = new Color[1];
        _colors[0] = new Color(1, 1, 198.0f/255.0f, 0.5f);
        _trailMesh = new Mesh();
        _trailMesh.name = _trailObject.name;
        _trailObject.GetComponent<MeshFilter>().mesh = _trailMesh;

        _minVertexDistanceSqr = _minVertexDistance * _minVertexDistance;
        _maxVertexDistanceSqr = _maxVertexDistance * _maxVertexDistance;
        _transforms = subTrans.ToArray();
    }

    List<Transform> subTrans = new List<Transform>();
    public void AddTransform(Transform control)
    {
        subTrans.Add(control);
    }

	void OnDisable()
	{
		Destroy(_trailObject);
	}

	public void Update()
	{
        if (_transforms == null)
            return;

		if (_transforms.Length < 2) {
			return ;
		}
		
		if (_emit && _emitTime != 0)
		{
			_emitTime -= Time.deltaTime;
			if (_emitTime == 0) _emitTime = -1;
			if (_emitTime < 0) _emit = false;
		}

		if (!_emit && _points.Count == 0 && _autoDestruct)
		{
			Destroy(_trailObject);
			Destroy(gameObject);
		}

		// early out if there is no camera
		if (!Camera.main) return;

		// if we have moved enough, create a new vertex and make sure we rebuild the mesh
		float theDistanceSqr = (_lastPosition - transform.position).sqrMagnitude;
		if (_emit)
		{
			if (theDistanceSqr > _minVertexDistanceSqr)
			{
				bool make = false;
				if (_points.Count < 3)
				{
					make = true;
				}
				else
				{
					//Vector3 l1 = _points[_points.Count - 2].basePosition - _points[_points.Count - 3].basePosition;
					//Vector3 l2 = _points[_points.Count - 1].basePosition - _points[_points.Count - 2].basePosition;
					Vector3 l1 = _points[_points.Count - 2].allVectors[0] - _points[_points.Count - 3].allVectors[0];
					Vector3 l2 = _points[_points.Count - 1].allVectors[0] - _points[_points.Count - 2].allVectors[0];
					if (Vector3.Angle(l1, l2) > _maxAngle || theDistanceSqr > _maxVertexDistanceSqr) make = true;
				}

				if (make)
				{
					Point p = new Point();
					//p.allVectors.Clear();
					for(int i = 0; i < _transforms.Length; ++i)
					{
						p.allVectors.Add(_transforms[i].position);
					}
					p.timeCreated = Time.time;
					_points.Add(p);
					_lastPosition = transform.position;

#if USE_INTERPOLATION
					if (_points.Count == 1)
					{
						_smoothedPoints.Add(p);
					}
					else if (_points.Count > 1)
					{
						// add 1+subdivisions for every possible pair in the _points
						for (int n = 0; n < 1+subdivisions; ++n)
							_smoothedPoints.Add(p);
					}

					// we use 4 control points for the smoothing
					if (_points.Count >= 4)
					{
						List<List<Vector3> > vecsmooths = new List< List<Vector3> >();
						int  transformsSize =  _points[_points.Count - 4].allVectors.Count;
						int smoothTipListCount = 0 ;
						for(int i = 0 ; i < transformsSize; ++i)
						{
							//Debug.LogError( "transformsSize" + transformsSize );
							Vector3[] tipPoints = new Vector3[4];
							tipPoints[0] = _points[_points.Count - 4].allVectors[i];
							tipPoints[1] = _points[_points.Count - 3].allVectors[i];
							tipPoints[2] = _points[_points.Count - 2].allVectors[i];
							tipPoints[3] = _points[_points.Count - 1].allVectors[i];

							/// donot use Bezier  HelloHuan;
							//IEnumerable<Vector3> smoothTip = Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), tipPoints, subdivisions);
							IEnumerable<Vector3> smoothTip = Interpolate.NewCatmullRom(tipPoints, subdivisions, false);

							List<Vector3> Tmp = new List<Vector3>(smoothTip);
							smoothTipListCount = Tmp.Count;
							//Debug.LogError( "smoothTipListCount\t\t" + smoothTipListCount ); 
							vecsmooths.Add( Tmp );
						}

						float firstTime = _points[_points.Count - 4].timeCreated;
						float secondTime = _points[_points.Count - 1].timeCreated;

						//Debug.Log(" smoothTipList.Count: " + smoothTipList.Count);
						for (int n = 0; n <smoothTipListCount; ++n)
						{
							int idx = _smoothedPoints.Count - (smoothTipListCount-n);
							// there are moments when the _smoothedPoints are lesser
							// than what is required, when elements from it are removed
							if (idx > -1 && idx < _smoothedPoints.Count)
							{
								Point sp = new Point();

								for(int i = 0; i < transformsSize; ++i )
								{
									sp.allVectors.Add( vecsmooths[i][n]  );
								}
								sp.timeCreated = Mathf.Lerp(firstTime, secondTime, (float)n/ smoothTipListCount);
								_smoothedPoints[idx] = sp;
							}
							//else
							//{
							//	Debug.LogError(idx + "/" + _smoothedPoints.Count);
							//}
						}
					}
#endif
				}
				else
				{
					if(_points[_points.Count - 1].allVectors.Count == _transforms.Length)
					{
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_points[_points.Count - 1].allVectors[i] = (_transforms[i].position);
						}
					}
					else{
						_points[_points.Count - 1].RemoveVectors();
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_points[_points.Count - 1].allVectors.Add(_transforms[i].position);
						}
					}

					//_points[_points.Count - 1].timeCreated = Time.time;

#if USE_INTERPOLATION
//					_smoothedPoints[_smoothedPoints.Count - 1].RemoveVectors();
//					for(int i = 0; i < _transforms.Length; ++i)
//					{
//						_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Add(_transforms[i].position);
//					}

					if(_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Count == _transforms.Length)
					{
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_smoothedPoints[_smoothedPoints.Count - 1].allVectors[i] = (_transforms[i].position);
						}
					}
					else{
						_smoothedPoints[_smoothedPoints.Count - 1].RemoveVectors();
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Add(_transforms[i].position);
						}
					}
					#endif
				}
			}
			else
			{
				if (_points.Count > 0)
				{
//					for(int i = 0; i < _transforms.Length; ++i)
//					{
//						_points[_points.Count - 1].allVectors.Add(_transforms[i].position);
//					}
					if(_points[_points.Count - 1].allVectors.Count == _transforms.Length)
					{
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_points[_points.Count - 1].allVectors[i] = (_transforms[i].position);
						}
					}
					else{
						_points[_points.Count - 1].RemoveVectors();
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_points[_points.Count - 1].allVectors.Add(_transforms[i].position);
						}
					}
					//_points[_points.Count - 1].timeCreated = Time.time;
				}

#if USE_INTERPOLATION
				if (_smoothedPoints.Count > 0)
				{
//					for(int i = 0; i < _transforms.Length; ++i)
//					{
//						_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Add(_transforms[i].position);
//					}
					if(_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Count == _transforms.Length)
					{
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_smoothedPoints[_smoothedPoints.Count - 1].allVectors[i] = (_transforms[i].position);
						}
					}
					else{
						_smoothedPoints[_smoothedPoints.Count - 1].RemoveVectors();
						for(int i = 0; i < _transforms.Length; ++i)
						{
							_smoothedPoints[_smoothedPoints.Count - 1].allVectors.Add(_transforms[i].position);
						}
					}
				}
#endif
			}
		}

		RemoveOldPoints(_points);
		if (_points.Count == 0)
		{
			_trailMesh.Clear();
		}

#if USE_INTERPOLATION
		RemoveOldPoints(_smoothedPoints);
		if (_smoothedPoints.Count == 0)
		{
			_trailMesh.Clear();
		}
#endif


#if USE_INTERPOLATION
		List<Point> pointsToUse = _smoothedPoints;
#else
		List<Point> pointsToUse = _points;
#endif

		if (pointsToUse.Count > 1)
		{
			int sectionPointSize = _transforms.Length;
			Vector3[] newVertices = new Vector3[pointsToUse.Count * sectionPointSize];
			Vector2[] newUV = new Vector2[pointsToUse.Count * sectionPointSize];
			int[] newTriangles = new int[(pointsToUse.Count - 1) * 6*(sectionPointSize - 1)];
			Color[] newColors = new Color[pointsToUse.Count * sectionPointSize];

			for (int n = 0; n < pointsToUse.Count; ++n)
			{
				Point p = pointsToUse[n];
				float time = (Time.time - p.timeCreated) / _lifeTime;

				Color color = Color.Lerp(Color.white, Color.clear, time);
				if (_colors != null && _colors.Length > 0)
				{
					float colorTime = time * (_colors.Length - 1);
					float min = Mathf.Floor(colorTime);
					float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, _colors.Length - 1);
					float lerp = Mathf.InverseLerp(min, max, colorTime);
					if (min >= _colors.Length) min = _colors.Length - 1; if (min < 0) min = 0;
					if (max >= _colors.Length) max = _colors.Length - 1; if (max < 0) max = 0;
					color = Color.Lerp(_colors[(int)min], _colors[(int)max], lerp);
				}

				float size = 0f;
				if (_sizes != null && _sizes.Length > 0)
				{
					float sizeTime = time * (_sizes.Length - 1);
					float min = Mathf.Floor(sizeTime);
					float max = Mathf.Clamp(Mathf.Ceil(sizeTime), 1, _sizes.Length - 1);
					float lerp = Mathf.InverseLerp(min, max, sizeTime);
					if (min >= _sizes.Length) min = _sizes.Length - 1; if (min < 0) min = 0;
					if (max >= _sizes.Length) max = _sizes.Length - 1; if (max < 0) max = 0;
					size = Mathf.Lerp(_sizes[(int)min], _sizes[(int)max], lerp);
				}

				Vector3 lineDirection = p.allVectors[sectionPointSize-1] - p.allVectors[0] ;
				//newVertices[n * 2] = p.basePosition - (lineDirection * (size * 0.5f));
				//newVertices[(n * 2) + 1] = p.tipPosition + (lineDirection * (size * 0.5f));

				float deltaRatioxx = 1.0F/(sectionPointSize-1);
				float uvRatio = (float)n/pointsToUse.Count;
				for(int i = 0; i < sectionPointSize; ++i)
				{
					newVertices[n * sectionPointSize + i] = p.allVectors[i] + (lineDirection * (size * ((deltaRatioxx*i) - 0.5f) ));
					newColors[(n * sectionPointSize) + i]  =  color;
					newUV[(n * sectionPointSize) + i] = new Vector2(uvRatio,  deltaRatioxx * i);
				}
				if (n > 0)
				{
					int triCount = (sectionPointSize-1)*2;
					int indexCount = triCount * 3;

					for(int k = 0; k < sectionPointSize - 1; ++k)
					{
						newTriangles[(n - 1) * indexCount + 0+6*k] = ( (n-1) * sectionPointSize) + k;
						newTriangles[((n - 1) * indexCount) + 1+6*k] = ((n-1) * sectionPointSize) +1 +k;
						newTriangles[((n - 1) * indexCount) + 2+6*k] = (n * sectionPointSize) + k;
						
						newTriangles[((n - 1) * indexCount) + 3+6*k] =  (n * sectionPointSize) + 1 + k;
						newTriangles[((n - 1) * indexCount) + 4+6*k] =  (n * sectionPointSize) + 0 + k;
						newTriangles[((n - 1) * indexCount) + 5+6*k] =((n-1) * sectionPointSize) + 1+k;
					}
				}
			}

			_trailMesh.Clear();
			_trailMesh.vertices = newVertices;
			_trailMesh.colors = newColors;
			_trailMesh.uv = newUV;
			_trailMesh.triangles = newTriangles;
            if (_trailObject != null)
            {
                _trailObject.GetComponent<Renderer>().material.SetColor("_TintColor", _colors[0]);
                _trailObject.GetComponent<Renderer>().material.SetFloat("_Alpha", 0.5f);
            }
        }
	}

	void RemoveOldPoints(List<Point> pointList)
	{
		List<Point> remove = new List<Point>();
		foreach (Point p in pointList)
		{
			// cull old points first
			if (Time.time - p.timeCreated > _lifeTime)
			{
				remove.Add(p);
			}
		}
		foreach (Point p in remove)
		{
			p .RemoveVectors();
			pointList.Remove(p);
		}
	}

    public void Open(float life = 0.2f)
    {
        Emit = true;
        _colors[0] = Color.white;
        _lifeTime = life;
    }
}
