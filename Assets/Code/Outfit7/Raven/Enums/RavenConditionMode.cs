#if !__cplusplus
#pragma warning disable
namespace Starlite {

    namespace Raven {
    
        // ERavenConditionMode
        public enum ERavenConditionMode { 
        	Equal = 0, 
        	Less = 1, 
        	LessOrEqual = 2, 
        	GreaterOrEqual = 3, 
        	Greater = 4, 
        	NotEqual = 5, 
        	BitSet = 6, 
        	BitNotSet = 7, 
        	BitMaskSet = 8, 
        	BitMaskNotSet = 9, 
        }
        
    }
    
}
#pragma warning restore
#else
#endif
