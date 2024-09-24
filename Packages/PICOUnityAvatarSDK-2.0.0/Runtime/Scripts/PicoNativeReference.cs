//#define PICO_ENABLE_CHECK_MEMORY_LEAK
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Pico
{
	namespace Avatar
	{
		// Reference target whose lifetime is managed by reference count.
		public interface RefTarget
		{
			// Gets current reference count.
			int refCount { get; }


			// Increment reference count.
			void Retain();


			// Decrement reference count.
			void Release();
		}

		// Base class for object that can be referenced with reference count.
		public class ReferencedObject : RefTarget
		{
			// Reference count.
			private volatile int _refCount = 0;

			// whether has been destroyed. When OnDestroy invoked, set the flag.
			private bool _destroyed = false;

#if DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK
			private static int _instanceCount = 0;
			private static Dictionary<System.Type, int> _objects = new Dictionary<System.Type, int>();
			private bool _debugDestroyed = false;
#endif

			// Default constructor to keep track the instances for debug purpose.
			public ReferencedObject()
			{
#if DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK
				++_instanceCount;
				if (_objects.TryGetValue(this.GetType(), out int count))
				{
					_objects[this.GetType()] = count + 1;
				}
				else
				{
					_objects[this.GetType()] = 1;
				}
#endif
			}

			// Deconstructor. check memory leak.
			~ReferencedObject()
			{
#if DEBUG
				if (this.refCount > 0)
				{
					UnityEngine.Debug.LogWarning(string.Format(
						"~ReferencedObject refCount is not cleared. Type:{0} refCount:{1}", this.GetType().Name,
						this.refCount.ToString()));
				}
#endif

#if DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK
				--_instanceCount;
				if (!_debugDestroyed)
				{
					//
					_objects[this.GetType()] = _objects[this.GetType()] - 1;
					UnityEngine.Debug.LogError("OnDestroy not invoked. Type:" + this.GetType().Name);
				}
#endif
			}

			// Whether is destroyed
			public bool isDestroyed
			{
				get => _destroyed;
			}

			// Gets current reference count.
			public int refCount
			{
				get { return _refCount; }
			}

			//Increment reference count and specify that using the object.
#if DEBUG
			public virtual void Retain()
#else
            public void Retain()
#endif
			{
				++_refCount;
			}


			// Decrease the reference count. If dropped to zero, invoke OnDestroy method.
			public void Release()
			{
				if (--_refCount == 0)
				{
					OnDestroy();
				}
#if DEBUG
				if (_refCount < 0)
				{
					UnityEngine.Debug.LogError("ReferenceObject Release wrong:" + this.GetType().Name);
				}
#endif
			}

			// Check whether reference count of the object has decreased to 0, if so, invoke OnDestroy(...). 
			public void CheckDelete()
			{
				if (_refCount <= 0)
				{
					OnDestroy();
				}
			}

			// Derived class can override the method to release resources when the object will be destroyed.
			// The method maybe invoke one more time, so derived class should destroy objects carefully.
			protected virtual void OnDestroy()
			{
				//
				_destroyed = true;

				// Do Nothing.
#if DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK
				//
				if (_debugDestroyed)
				{
					UnityEngine.Debug.LogError("OnDestroy has been invoked.");
				}
				else
				{
					_objects[this.GetType()] = _objects[this.GetType()] - 1;
				}

				_debugDestroyed = true;
#endif
			}

			// Helper method to replace variable which is derived from RefTarget.
			public static void Replace<T>(ref T dest, T src) where T : RefTarget
			{
				if (src != null)
				{
					src.Retain();
				}

				//
				if (dest != null)
				{
					dest.Release();
				}

				dest = src;
			}

			// Helper method to release field variable which is derived from RefTarget.
			public static void ReleaseField<T>(ref T field) where T : RefTarget
			{
				if (field != null)
				{
					var tmp = field;
					field = default(T);
					tmp.Release();
				}
			}

			// Log object counts.
			static internal void LogStats()
			{
#if DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK

				var sb = new System.Text.StringBuilder();
				sb.Append("AvatarStats C# References: ");
				foreach (var x in _objects)
				{
					sb.Append(x.Key.Name);
					sb.Append(":");
					sb.Append(x.Value);
					sb.Append(" | ");
				}

				// use unity log to avoid avatar main log performance burden.
				UnityEngine.Debug.Log(sb.ToString());
				return;
#else
                UnityEngine.Debug.Log("DEBUG && PICO_ENABLE_CHECK_MEMORY_LEAK needed for ReferenceObject.LogStats Works.");
#endif
			}
		}

		// Native object that should be managed by reference count.
		public abstract class NativeObject : ReferencedObject
		{
			#region Public Properties/Methods

			// Gets native handle.
			public System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			//Increase reference count for native object. 
			//Invoked for native object that not derived from NativeObject.
			public static void RetainNative(System.IntPtr nativeObjectPtr)
			{
#if DEBUG
				if (nativeObjectPtr == System.IntPtr.Zero)
				{
					throw new System.NullReferenceException("NativeObject.Ref null ptr.");
				}
#endif
				pav_Object_Retain(nativeObjectPtr);
			}

			// Decrease reference count for native object.
			public static void ReleaseNative(ref System.IntPtr nativeObjectPtr)
			{
				if (nativeObjectPtr != System.IntPtr.Zero)
				{
					var tmp = nativeObjectPtr;
					//
					nativeObjectPtr = System.IntPtr.Zero;
					//
					pav_Object_Release(tmp);
				}
			}

			// If object is deserialized, the method would be invoked to do some post work.
			public virtual void OnPostLoad()
			{
			}

			#endregion


			#region Private Field

			// native handle.
			private System.IntPtr _nativeHandle;

			#endregion


			#region Private/Protected Methods

			// should check release native object.
			~NativeObject()
			{
				// should check release native object.
				if (_nativeHandle != System.IntPtr.Zero)
				{
					pav_Object_Release(_nativeHandle);
					// clear handle.
					_nativeHandle = System.IntPtr.Zero;
				}
			}

			// Destroy the object and release native reference count.
			// @note Derived class MUST invoke the method if override it.
			[UnityEngine.Scripting.PreserveAttribute]
			protected override void OnDestroy()
			{
				// Do Nothing.
				base.OnDestroy();
				//
				if (_nativeHandle != System.IntPtr.Zero)
				{
					pav_Object_Release(_nativeHandle);
					// clear handle.
					_nativeHandle = System.IntPtr.Zero;
				}
			}

			// Sets native handle.
			protected void SetNativeHandle(System.IntPtr nativeHandle_, bool needRetain)
			{
				if (nativeHandle_ == _nativeHandle)
				{
					return;
				}

				if (nativeHandle_ != System.IntPtr.Zero)
				{
					if (needRetain)
					{
						pav_Object_Retain(nativeHandle_);
					}
				}

				// check release previous handle.
				if (_nativeHandle != System.IntPtr.Zero)
				{
					pav_Object_Release(_nativeHandle);
				}

				_nativeHandle = nativeHandle_;
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_Object_Retain(System.IntPtr nativeObjectPtr);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_Object_Release(System.IntPtr nativeObjectPtr);

			#endregion
		}

		// Guard for RefTarget.
		public class RefTargetGuard<T> : System.IDisposable where T : RefTarget
		{
			public RefTargetGuard(T obj)
			{
				_Object = obj;
				//
				if (obj != null)
				{
					obj.Retain();
					//
					_NeedRelease = true;
				}
			}

			~RefTargetGuard()
			{
				Dispose();
			}

			//
			public void Dispose()
			{
				if (_NeedRelease)
				{
					_NeedRelease = false;
					//
					_Object.Release();
				}
			}

			public T target
			{
				get { return _Object; }
			}

			private T _Object;
			private bool _NeedRelease = false;
		}


		// Fixed size of T[], to helper manage retain/release a group of referenced object..
		public class ReferencedObjectArray<T> where T : ReferencedObject
		{
			// ObjectReferenceArray.
			public ReferencedObjectArray(int count)
			{
				_Array = new T[count];
			}

			// length.
			public int Length
			{
				get { return _Array.Length; }
			}

			public void Clear()
			{
				for (int i = 0; i < _Array.Length; ++i)
				{
					if (_Array[i] != null)
					{
						_Array[i].Release();
						_Array[i] = null;
					}
				}
			}

			// Sets/Gets indiced item.
			public T this[int i]
			{
				get { return _Array[i]; }
				set
				{
					if (value != null)
					{
						value.Retain();
					}

					if (_Array[i] != null)
					{
						_Array[i].Release();
					}

					_Array[i] = value;
				}
			}

			private T[] _Array;
		}
	}
}