#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // ERavenParameterType
        public enum ERavenParameterType { 
        	Int = 0, 
        	Float = 1, 
        	Bool = 2, 
        	Enum = 3, 
        	BoolTrigger = 4, 
        	IntTrigger = 5, 
        	EnumTrigger = 6, 
        	EnumBitMask = 7, 
        	Vector4 = 8, 
        	Object = 9, 
        	ActorList = 10, 
        	Gradient = 11, 
        }
        
    }
    
}
#pragma warning restore
#else
#endif
